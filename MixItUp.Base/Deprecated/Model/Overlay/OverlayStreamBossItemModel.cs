using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Twitch.Bits;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Services.Twitch.New;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [Obsolete]
    [DataContract]
    public class OverlayStreamBossItemModel : OverlayHTMLTemplateItemModelBase
    {
        public const string HTMLTemplate =
        @"<table cellpadding=""10"" style=""border-style: solid; border-width: 5px; border-color: {BORDER_COLOR}; background-color: {BACKGROUND_COLOR}; width: {WIDTH}px; height: {HEIGHT}px;"">
          <tbody>
            <tr>
              <td rowspan=""2"">
                <img src=""{USER_IMAGE}"" width=""{USER_IMAGE_SIZE}"" height=""{USER_IMAGE_SIZE}"" style=""vertical-align: middle;"">
              </td>
              <td style=""padding-bottom: 0px;"">
                <span style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; font-weight: bold; color: {TEXT_COLOR};"">{USERNAME}</span>
              </td>
              <td style=""padding-bottom: 0px;"">
                <span style=""font-family: '{TEXT_FONT}'; font-size: {TEXT_SIZE}px; font-weight: bold; color: {TEXT_COLOR}; float: right; margin-right: 10px"">{HEALTH_REMAINING} / {MAXIMUM_HEALTH}</span>
              </td>
            </tr>
            <tr>
              <td colspan=""2"" style=""padding-top: 0px;"">
                <div style=""background-color: black; height: {TEXT_SIZE}px; margin-right: 10px"">
                  <div style=""background-color: {PROGRESS_COLOR}; width: {PROGRESS_WIDTH}%; height: {TEXT_SIZE}px;""></div>
                </div>
              </td>
            </tr>
          </tbody>
        </table>";

        [DataMember]
        public int StartingHealth { get; set; }

        [DataMember]
        public string BorderColor { get; set; }
        [DataMember]
        public string BackgroundColor { get; set; }
        [DataMember]
        public string TextColor { get; set; }
        [DataMember]
        public string TextFont { get; set; }
        [DataMember]
        public string ProgressColor { get; set; }

        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public double FollowBonus { get; set; }
        [DataMember]
        public double HostBonus { get; set; }
        [DataMember]
        public double RaidBonus { get; set; }
        [DataMember]
        public double SubscriberBonus { get; set; }
        [DataMember]
        public double DonationBonus { get; set; }
        [DataMember]
        public double BitsBonus { get; set; }

        [DataMember]
        public double HealingBonus { get; set; }
        [DataMember]
        public double OverkillBonus { get; set; }

        [DataMember]
        public OverlayItemEffectVisibleAnimationTypeEnum DamageAnimation { get; set; }
        [DataMember]
        public string DamageAnimationName { get { return OverlayItemEffectsModel.GetAnimationClassName(this.DamageAnimation); } set { } }
        [DataMember]
        public OverlayItemEffectVisibleAnimationTypeEnum NewBossAnimation { get; set; }
        [DataMember]
        public string NewBossAnimationName { get { return OverlayItemEffectsModel.GetAnimationClassName(this.NewBossAnimation); } set { } }

        [DataMember]
        public Guid CurrentBossID { get; set; } = Guid.Empty;
        [DataMember]
        public int CurrentStartingHealth { get; set; }
        [DataMember]
        public int CurrentHealth { get; set; }
        [DataMember]
        public bool NewBoss { get; set; }
        [DataMember]
        public bool DamageTaken { get; set; }

        [DataMember]
        public Guid StreamBossChangedCommandID { get; set; }

        [JsonIgnore]
        public CustomCommandModel StreamBossChangedCommand
        {
            get { return (CustomCommandModel)ChannelSession.Settings.GetCommand(this.StreamBossChangedCommandID); }
            set
            {
                if (value != null)
                {
                    this.StreamBossChangedCommandID = value.ID;
                    ChannelSession.Settings.SetCommand(value);
                }
                else
                {
                    ChannelSession.Settings.RemoveCommand(this.StreamBossChangedCommandID);
                    this.StreamBossChangedCommandID = Guid.Empty;
                }
            }
        }

        [JsonIgnore]
        public UserV2ViewModel CurrentBoss { get; set; }

        private SemaphoreSlim HealthSemaphore = new SemaphoreSlim(1);

        [JsonIgnore]
        private HashSet<Guid> follows = new HashSet<Guid>();
        [JsonIgnore]
        private HashSet<Guid> hosts = new HashSet<Guid>();
        [JsonIgnore]
        private HashSet<Guid> raids = new HashSet<Guid>();

        public OverlayStreamBossItemModel() : base() { }

        public OverlayStreamBossItemModel(string htmlText, int startingHealth, int width, int height, string textColor, string textFont, string borderColor, string backgroundColor,
            string progressColor, double followBonus, double hostBonus, double raidBonus, double subscriberBonus, double donationBonus, double bitsBonus, double healingBonus, double overkillBonus,
            OverlayItemEffectVisibleAnimationTypeEnum damageAnimation, OverlayItemEffectVisibleAnimationTypeEnum newBossAnimation, CustomCommandModel streamBossChangedCommand)
            : base(OverlayItemModelTypeEnum.StreamBoss, htmlText)
        {
            this.StartingHealth = startingHealth;
            this.Width = width;
            this.Height = height;
            this.TextColor = textColor;
            this.TextFont = textFont;
            this.BorderColor = borderColor;
            this.BackgroundColor = backgroundColor;
            this.ProgressColor = progressColor;
            this.FollowBonus = followBonus;
            this.HostBonus = hostBonus;
            this.RaidBonus = raidBonus;
            this.SubscriberBonus = subscriberBonus;
            this.DonationBonus = donationBonus;
            this.BitsBonus = bitsBonus;
            this.HealingBonus = healingBonus;
            this.OverkillBonus = overkillBonus;
            this.DamageAnimation = damageAnimation;
            this.NewBossAnimation = newBossAnimation;
            this.StreamBossChangedCommand = streamBossChangedCommand;
        }

        public override async Task Enable()
        {
            this.DamageTaken = false;
            this.NewBoss = false;

            if (this.CurrentBossID != Guid.Empty)
            {
                //this.CurrentBoss = await ServiceManager.Get<UserService>().GetUserByID(this.CurrentBossID);
                //if (this.CurrentBoss == null)
                //{
                //    this.CurrentBossID = Guid.Empty;
                //}
            }

            if (this.CurrentBoss == null)
            {
                this.CurrentBoss = ChannelSession.User;
                this.CurrentHealth = this.CurrentStartingHealth = this.StartingHealth;
            }
            this.CurrentBossID = this.CurrentBoss.ID;

            if (this.FollowBonus > 0.0)
            {
                EventService.OnFollowOccurred += GlobalEvents_OnFollowOccurred;
            }
            if (this.HostBonus > 0.0)
            {

            }
            if (this.RaidBonus > 0.0)
            {
                EventService.OnRaidOccurred += GlobalEvents_OnRaidOccurred;
            }
            if (this.SubscriberBonus > 0.0)
            {
                //EventService.OnSubscribeOccurred += GlobalEvents_OnSubscribeOccurred;
                //EventService.OnResubscribeOccurred += GlobalEvents_OnResubscribeOccurred;
                //EventService.OnSubscriptionGiftedOccurred += GlobalEvents_OnSubscriptionGiftedOccurred;
            }
            if (this.DonationBonus > 0.0)
            {
                EventService.OnDonationOccurred += GlobalEvents_OnDonationOccurred;
            }
            if (this.BitsBonus > 0.0)
            {
                EventService.OnTwitchBitsCheeredOccurred += GlobalEvents_OnBitsOccurred;
            }

            await base.Enable();
        }

        public override async Task Disable()
        {
            EventService.OnFollowOccurred -= GlobalEvents_OnFollowOccurred;
            EventService.OnRaidOccurred -= GlobalEvents_OnRaidOccurred;
            //EventService.OnSubscribeOccurred -= GlobalEvents_OnSubscribeOccurred;
            //EventService.OnResubscribeOccurred -= GlobalEvents_OnResubscribeOccurred;
            //EventService.OnSubscriptionGiftedOccurred -= GlobalEvents_OnSubscriptionGiftedOccurred;
            EventService.OnDonationOccurred -= GlobalEvents_OnDonationOccurred;
            EventService.OnTwitchBitsCheeredOccurred -= GlobalEvents_OnBitsOccurred;

            await base.Disable();
        }

        protected override async Task<Dictionary<string, string>> GetTemplateReplacements(CommandParametersModel parameters)
        {
            UserV2ViewModel boss = null;
            int health = 0;

            await this.HealthSemaphore.WaitAsync();

            boss = this.CurrentBoss;
            health = this.CurrentHealth;

            this.HealthSemaphore.Release();

            Dictionary<string, string> replacementSets = new Dictionary<string, string>();

            replacementSets["BACKGROUND_COLOR"] = this.BackgroundColor;
            replacementSets["BORDER_COLOR"] = this.BorderColor;
            replacementSets["TEXT_COLOR"] = this.TextColor;
            replacementSets["TEXT_FONT"] = this.TextFont;
            replacementSets["WIDTH"] = this.Width.ToString();
            replacementSets["HEIGHT"] = this.Height.ToString();
            replacementSets["TEXT_SIZE"] = ((int)(0.2 * ((double)this.Height))).ToString();

            if (boss != null)
            {
                replacementSets["USERNAME"] = boss.DisplayName;
                replacementSets["USER_IMAGE"] = boss.AvatarLink;
            }
            replacementSets["USER_IMAGE_SIZE"] = ((int)(0.8 * ((double)this.Height))).ToString();

            replacementSets["HEALTH_REMAINING"] = health.ToString();
            replacementSets["MAXIMUM_HEALTH"] = this.CurrentStartingHealth.ToString();

            replacementSets["PROGRESS_COLOR"] = this.ProgressColor;
            replacementSets["PROGRESS_WIDTH"] = ((((double)health) / ((double)this.CurrentStartingHealth)) * 100.0).ToString();

            return replacementSets;
        }

        private async Task ReduceHealth(UserV2ViewModel user, double amount)
        {
            await this.HealthSemaphore.WaitAsync();

            this.DamageTaken = false;
            this.NewBoss = false;

            if (this.CurrentBoss.Equals(user) && this.HealingBonus > 0.0)
            {
                int healingAmount = (int)(this.HealingBonus * amount);
                this.CurrentHealth = Math.Min(this.CurrentStartingHealth, this.CurrentHealth + healingAmount);
            }
            else
            {
                this.CurrentHealth -= (int)amount;
                this.DamageTaken = true;
            }

            if (this.CurrentHealth <= 0)
            {
                this.NewBoss = true;
                this.CurrentBoss = user;
                this.CurrentBossID = user.ID;

                int newHealth = this.StartingHealth;
                if (this.OverkillBonus > 0.0)
                {
                    int overkillAmount = this.CurrentHealth * -1;
                    newHealth += (int)(this.OverkillBonus * overkillAmount);
                }
                this.CurrentHealth = this.CurrentStartingHealth = newHealth;

                if (this.StreamBossChangedCommand != null)
                {
                    await ServiceManager.Get<CommandService>().Queue(this.StreamBossChangedCommand);
                }
            }

            this.SendUpdateRequired();

            this.HealthSemaphore.Release();
        }

        private async void GlobalEvents_OnFollowOccurred(object sender, UserV2ViewModel user)
        {
            if (!this.follows.Contains(user.ID))
            {
                this.follows.Add(user.ID);
                await this.ReduceHealth(user, this.FollowBonus);
            }
        }

        private async void GlobalEvents_OnHostOccurred(object sender, UserV2ViewModel host)
        {
            if (!this.hosts.Contains(host.ID))
            {
                this.hosts.Add(host.ID);
                await this.ReduceHealth(host, this.HostBonus);
            }
        }

        private async void GlobalEvents_OnRaidOccurred(object sender, Tuple<UserV2ViewModel, int> raid)
        {
            if (!this.raids.Contains(raid.Item1.ID))
            {
                this.raids.Add(raid.Item1.ID);
                await this.ReduceHealth(raid.Item1, (Math.Max(raid.Item2, 1) * this.RaidBonus));
            }
        }

        private async void GlobalEvents_OnSubscribeOccurred(object sender, UserV2ViewModel user)
        {
            await this.ReduceHealth(user, this.SubscriberBonus);
        }

        private async void GlobalEvents_OnResubscribeOccurred(object sender, Tuple<UserV2ViewModel, int> user)
        {
            await this.ReduceHealth(user.Item1, this.SubscriberBonus);
        }

        private async void GlobalEvents_OnSubscriptionGiftedOccurred(object sender, Tuple<UserV2ViewModel, UserV2ViewModel> e)
        {
            await this.ReduceHealth(e.Item1, this.SubscriberBonus);
        }

        private async void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel donation) { await this.ReduceHealth(donation.User, (donation.Amount * this.DonationBonus)); }

        private async void GlobalEvents_OnBitsOccurred(object sender, TwitchBitsCheeredEventModel e) { await this.ReduceHealth(e.User, (e.Amount * this.BitsBonus)); }
    }
}
