using Mixer.Base.Model.User;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayStreamBossItemModel : OverlayHTMLTemplateItemModelBase
    {
        public const string NewStreamBossCommandName = "New Stream Boss";

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
        public double SubscriberBonus { get; set; }
        [DataMember]
        public double DonationBonus { get; set; }
        [DataMember]
        public double SparkBonus { get; set; }
        [DataMember]
        public double EmberBonus { get; set; }

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
        public CustomCommand NewStreamBossCommand { get; set; }

        [DataMember]
        public UserViewModel CurrentBoss { get; set; }

        private SemaphoreSlim HealthSemaphore = new SemaphoreSlim(1);

        private HashSet<Guid> follows = new HashSet<Guid>();
        private HashSet<Guid> hosts = new HashSet<Guid>();
        private HashSet<Guid> subs = new HashSet<Guid>();

        public OverlayStreamBossItemModel() : base() { }

        public OverlayStreamBossItemModel(string htmlText, int startingHealth, int width, int height, string textColor, string textFont, string borderColor, string backgroundColor,
            string progressColor, double followBonus, double hostBonus, double subscriberBonus, double donationBonus, double sparkBonus, double emberBonus, double healingBonus, double overkillBonus,
            OverlayItemEffectVisibleAnimationTypeEnum damageAnimation, OverlayItemEffectVisibleAnimationTypeEnum newBossAnimation, CustomCommand newStreamBossCommand)
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
            this.SubscriberBonus = subscriberBonus;
            this.DonationBonus = donationBonus;
            this.SparkBonus = sparkBonus;
            this.EmberBonus = emberBonus;
            this.HealingBonus = healingBonus;
            this.OverkillBonus = overkillBonus;
            this.DamageAnimation = damageAnimation;
            this.NewBossAnimation = newBossAnimation;
            this.NewStreamBossCommand = newStreamBossCommand;
        }

        public override async Task Enable()
        {
            this.DamageTaken = false;
            this.NewBoss = false;

            if (this.CurrentBossID != Guid.Empty)
            {
                UserDataModel userData = ChannelSession.Settings.GetUserData(this.CurrentBossID);
                if (userData != null)
                {
                    this.CurrentBoss = new UserViewModel(userData);
                }
                else
                {
                    this.CurrentBossID = Guid.Empty;
                }
            }

            if (this.CurrentBoss == null)
            {
                this.CurrentBoss = await ChannelSession.GetCurrentUser();
                this.CurrentHealth = this.CurrentStartingHealth = this.StartingHealth;
            }
            this.CurrentBossID = this.CurrentBoss.ID;

            if (this.FollowBonus > 0.0)
            {
                GlobalEvents.OnFollowOccurred += GlobalEvents_OnFollowOccurred;
            }
            if (this.HostBonus > 0.0)
            {
                GlobalEvents.OnHostOccurred += GlobalEvents_OnHostOccurred;
            }
            if (this.SubscriberBonus > 0.0)
            {
                GlobalEvents.OnSubscribeOccurred += GlobalEvents_OnSubscribeOccurred;
                GlobalEvents.OnResubscribeOccurred += GlobalEvents_OnResubscribeOccurred;
                GlobalEvents.OnSubscriptionGiftedOccurred += GlobalEvents_OnSubscriptionGiftedOccurred;
            }
            if (this.DonationBonus > 0.0)
            {
                GlobalEvents.OnDonationOccurred += GlobalEvents_OnDonationOccurred;
            }
            if (this.SparkBonus > 0.0)
            {
                GlobalEvents.OnSparkUseOccurred += GlobalEvents_OnSparkUseOccurred;
            }
            if (this.EmberBonus > 0.0)
            {
                GlobalEvents.OnEmberUseOccurred += GlobalEvents_OnEmberUseOccurred;
            }

            await base.Enable();
        }

        public override async Task Disable()
        {
            GlobalEvents.OnFollowOccurred -= GlobalEvents_OnFollowOccurred;
            GlobalEvents.OnHostOccurred -= GlobalEvents_OnHostOccurred;
            GlobalEvents.OnSubscribeOccurred -= GlobalEvents_OnSubscribeOccurred;
            GlobalEvents.OnResubscribeOccurred -= GlobalEvents_OnResubscribeOccurred;
            GlobalEvents.OnSubscriptionGiftedOccurred -= GlobalEvents_OnSubscriptionGiftedOccurred;
            GlobalEvents.OnDonationOccurred -= GlobalEvents_OnDonationOccurred;
            GlobalEvents.OnSparkUseOccurred -= GlobalEvents_OnSparkUseOccurred;
            GlobalEvents.OnEmberUseOccurred -= GlobalEvents_OnEmberUseOccurred;

            await base.Disable();
        }

        protected override async Task<Dictionary<string, string>> GetTemplateReplacements(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            UserViewModel boss = null;
            int health = 0;

            await this.HealthSemaphore.WaitAndRelease(() =>
            {
                boss = this.CurrentBoss;
                health = this.CurrentHealth;
                return Task.FromResult(0);
            });

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
                replacementSets["USERNAME"] = boss.Username;
                replacementSets["USER_IMAGE"] = boss.AvatarLink;
            }
            replacementSets["USER_IMAGE_SIZE"] = ((int)(0.8 * ((double)this.Height))).ToString();

            replacementSets["HEALTH_REMAINING"] = health.ToString();
            replacementSets["MAXIMUM_HEALTH"] = this.CurrentStartingHealth.ToString();

            replacementSets["PROGRESS_COLOR"] = this.ProgressColor;
            replacementSets["PROGRESS_WIDTH"] = ((((double)health) / ((double)this.CurrentStartingHealth)) * 100.0).ToString();

            return replacementSets;
        }

        private async Task ReduceHealth(UserViewModel user, double amount)
        {
            await this.HealthSemaphore.WaitAndRelease(async () =>
            {
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

                    if (this.NewStreamBossCommand != null)
                    {
                        await this.NewStreamBossCommand.Perform();
                    }
                }

                this.SendUpdateRequired();

                return Task.FromResult(0);
            });
        }

        private async void GlobalEvents_OnFollowOccurred(object sender, UserViewModel user)
        {
            if (!this.follows.Contains(user.ID))
            {
                this.follows.Add(user.ID);
                await this.ReduceHealth(user, this.FollowBonus);
            }
        }

        private async void GlobalEvents_OnHostOccurred(object sender, Tuple<UserViewModel, int> host)
        {
            if (!this.hosts.Contains(host.Item1.ID))
            {
                this.hosts.Add(host.Item1.ID);
                await this.ReduceHealth(host.Item1, (Math.Max(host.Item2, 1) * this.HostBonus));
            }
        }

        private async void GlobalEvents_OnSubscribeOccurred(object sender, UserViewModel user)
        {
            if (!this.subs.Contains(user.ID))
            {
                this.subs.Add(user.ID);
                await this.ReduceHealth(user, this.SubscriberBonus);
            }
        }

        private async void GlobalEvents_OnResubscribeOccurred(object sender, Tuple<UserViewModel, int> user)
        {
            if (!this.subs.Contains(user.Item1.ID))
            {
                this.subs.Add(user.Item1.ID);
                await this.ReduceHealth(user.Item1, this.SubscriberBonus);
            }
        }

        private async void GlobalEvents_OnSubscriptionGiftedOccurred(object sender, Tuple<UserViewModel, UserViewModel> e)
        {
            if (!this.subs.Contains(e.Item2.ID))
            {
                this.subs.Add(e.Item2.ID);
                await this.ReduceHealth(e.Item2, this.SubscriberBonus);
            }
        }

        private async void GlobalEvents_OnDonationOccurred(object sender, UserDonationModel donation) { await this.ReduceHealth(donation.User, (donation.Amount * this.DonationBonus)); }

        private async void GlobalEvents_OnSparkUseOccurred(object sender, Tuple<UserViewModel, uint> sparkUsage) { await this.ReduceHealth(sparkUsage.Item1, (sparkUsage.Item2 * this.SparkBonus)); }

        private async void GlobalEvents_OnEmberUseOccurred(object sender, UserEmberUsageModel emberUsage) { await this.ReduceHealth(emberUsage.User, (emberUsage.Amount * this.EmberBonus)); }
    }
}
