using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.ViewModel.Chat.Trovo;
using MixItUp.Base.ViewModel.Chat.YouTube;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayStreamBossV3Model : OverlayVisualTextV3ModelBase
    {
        public const string BossImageProperty = "BossImage";
        public const string BossNameProperty = "BossName";
        public const string BossHealthProperty = "BossHealth";
        public const string BossMaxHealthProperty = "BossMaxHealth";
        public const string BossHealthBarRemainingProperty = "BossHealthBarRemaining";

        public static readonly string DefaultHTML = OverlayResources.OverlayStreamBossDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayTextDefaultCSS + "\n\n" + OverlayResources.OverlayStreamBossDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayStreamBossDefaultJavascript;

        [DataMember]
        public string BorderColor { get; set; }
        [DataMember]
        public string BackgroundColor { get; set; }
        [DataMember]
        public string HealthColor { get; set; }
        [DataMember]
        public string DamageColor { get; set; }

        [DataMember]
        public Guid CurrentBoss { get; set; }
        [DataMember]
        public int CurrentHealth { get; set; }
        [DataMember]
        public int CurrentMaxHealth { get; set; }

        [DataMember]
        public int BaseHealth { get; set; }
        [DataMember]
        public int KillBonusHealth { get; set; }
        [DataMember]
        public int OverkillBonusMultiplier { get; set; }

        [DataMember]
        public double BossSelfHealingMultiplier { get; set; }

        [DataMember]
        public double FollowDamage { get; set; }

        [DataMember]
        public double RaidTotalDamage { get; set; }
        [DataMember]
        public double RaidPerViewDamage { get; set; }

        [DataMember]
        public Dictionary<int, double> TwitchSubscriptionsDamage { get; set; }
        [DataMember]
        public Dictionary<string, double> YouTubeMembershipsDamage { get; set; }
        [DataMember]
        public double TrovoSubscriptionDamage { get; set; }

        [DataMember]
        public double DonationDamage { get; set; }

        [DataMember]
        public double TwitchBitsDamage { get; set; }

        [DataMember]
        public double YouTubeSuperChatDamage { get; set; }

        [DataMember]
        public double TrovoSpellDamage { get; set; }

        [DataMember]
        public OverlayAnimationV3Model DamageAnimation { get; set; }
        [DataMember]
        public OverlayAnimationV3Model HealingAnimation { get; set; }
        [DataMember]
        public OverlayAnimationV3Model NewBossAnimation { get; set; }

        [DataMember]
        public Guid DamageOccurredCommandID { get; set; }
        [DataMember]
        public Guid HealingOccurredCommandID { get; set; }
        [DataMember]
        public Guid NewBossCommandID { get; set; }

        [JsonIgnore]
        public int HealthRemainingPercentage { get { return Math.Max(Math.Min((int)Math.Round((this.CurrentHealth / this.CurrentMaxHealth) * 100.0), 100), 0); } }

        public OverlayStreamBossV3Model() : base(OverlayItemV3Type.StreamBoss) { }

        public override Dictionary<string, string> GetGenerationProperties()
        {
            Dictionary<string, string> properties = base.GetGenerationProperties();

            properties[nameof(this.BorderColor)] = this.BorderColor.ToString();
            properties[nameof(this.BackgroundColor)] = this.BackgroundColor.ToString();
            properties[nameof(this.HealthColor)] = this.HealthColor.ToString();
            properties[nameof(this.DamageColor)] = this.DamageColor.ToString();

            properties[nameof(this.DamageAnimation)] = this.DamageAnimation.GenerateAnimationJavascript(OverlayItemV3ModelBase.MainDivElementID);
            properties[nameof(this.HealingAnimation)] = this.HealingAnimation.GenerateAnimationJavascript(OverlayItemV3ModelBase.MainDivElementID);
            properties[nameof(this.NewBossAnimation)] = this.NewBossAnimation.GenerateAnimationJavascript(OverlayItemV3ModelBase.MainDivElementID);

            return properties;
        }

        public override async Task ProcessGenerationProperties(Dictionary<string, string> properties, CommandParametersModel parameters)
        {
            await base.ProcessGenerationProperties(properties, parameters);

            UserV2ViewModel boss = await ServiceManager.Get<UserService>().GetUserByID(this.CurrentBoss);
            if (boss == null)
            {
                boss = ChannelSession.User;
            }

            properties[BossImageProperty] = boss.AvatarLink;
            properties[BossNameProperty] = boss.DisplayName;
            properties[BossHealthProperty] = this.CurrentHealth.ToString();
            properties[BossMaxHealthProperty] = this.CurrentMaxHealth.ToString();
            properties[BossHealthBarRemainingProperty] = this.HealthRemainingPercentage.ToString();
        }

        protected override async Task WidgetEnableInternal()
        {
            await base.WidgetEnableInternal();

            if (this.CurrentBoss == Guid.Empty)
            {
                this.CurrentBoss = ChannelSession.User.ID;
                this.CurrentHealth = this.CurrentMaxHealth = this.BaseHealth;
            }

            if (this.FollowDamage > 0)
            {
                EventService.OnFollowOccurred += EventService_OnFollowOccurred;
            }

            if (this.RaidTotalDamage > 0 || this.RaidPerViewDamage > 0)
            {
                EventService.OnRaidOccurred += EventService_OnRaidOccurred;
            }

            if (this.TwitchSubscriptionsDamage.Count > 0 || this.YouTubeMembershipsDamage.Count > 0 || this.TrovoSubscriptionDamage > 0)
            {
                EventService.OnSubscribeOccurred += EventService_OnSubscribeOccurred;
                EventService.OnResubscribeOccurred += EventService_OnSubscribeOccurred;
                EventService.OnSubscriptionGiftedOccurred += EventService_OnSubscribeOccurred;
                EventService.OnMassSubscriptionsGiftedOccurred += EventService_OnMassSubscriptionsGiftedOccurred;
            }

            if (this.DonationDamage > 0)
            {
                EventService.OnDonationOccurred += EventService_OnDonationOccurred;
            }

            if (this.TwitchBitsDamage > 0)
            {
                EventService.OnTwitchBitsCheeredOccurred += EventService_OnTwitchBitsCheeredOccurred;
            }

            if (this.YouTubeSuperChatDamage > 0)
            {
                EventService.OnYouTubeSuperChatOccurred += EventService_OnYouTubeSuperChatOccurred;
            }

            if (this.TrovoSpellDamage > 0)
            {
                EventService.OnTrovoSpellCastOccurred += EventService_OnTrovoSpellCastOccurred;
            }
        }

        protected override async Task WidgetDisableInternal()
        {
            await base.WidgetDisableInternal();

            EventService.OnFollowOccurred -= EventService_OnFollowOccurred;
            EventService.OnRaidOccurred -= EventService_OnRaidOccurred;
            EventService.OnSubscribeOccurred -= EventService_OnSubscribeOccurred;
            EventService.OnResubscribeOccurred -= EventService_OnSubscribeOccurred;
            EventService.OnSubscriptionGiftedOccurred -= EventService_OnSubscribeOccurred;
            EventService.OnMassSubscriptionsGiftedOccurred -= EventService_OnMassSubscriptionsGiftedOccurred;
            EventService.OnDonationOccurred -= EventService_OnDonationOccurred;
            EventService.OnTwitchBitsCheeredOccurred -= EventService_OnTwitchBitsCheeredOccurred;
            EventService.OnTrovoSpellCastOccurred -= EventService_OnTrovoSpellCastOccurred;
            EventService.OnYouTubeSuperChatOccurred -= EventService_OnYouTubeSuperChatOccurred;
        }

        private async void EventService_OnFollowOccurred(object sender, UserV2ViewModel user)
        {
            await this.DealDamage(user, this.FollowDamage);
        }

        private async void EventService_OnRaidOccurred(object sender, Tuple<UserV2ViewModel, int> raid)
        {
            await this.DealDamage(raid.Item1, this.RaidTotalDamage + (this.RaidPerViewDamage * raid.Item2));
        }

        private async void EventService_OnSubscribeOccurred(object sender, SubscriptionDetailsModel subscription)
        {
            if (subscription.Platform == StreamingPlatformTypeEnum.Twitch)
            {
                if (this.TwitchSubscriptionsDamage.TryGetValue(subscription.TwitchSubscriptionTier, out double damage))
                {
                    await this.DealDamage(subscription.User, damage);
                }
            }
            else if (subscription.Platform == StreamingPlatformTypeEnum.YouTube)
            {
                if (this.YouTubeMembershipsDamage.TryGetValue(subscription.YouTubeMembershipTier, out double damage))
                {
                    await this.DealDamage(subscription.User, damage);
                }
            }
            else if (subscription.Platform == StreamingPlatformTypeEnum.Trovo)
            {
                await this.DealDamage(subscription.User, this.TrovoSubscriptionDamage);
            }
        }

        private async void EventService_OnMassSubscriptionsGiftedOccurred(object sender, IEnumerable<SubscriptionDetailsModel> subscriptions)
        {
            if (subscriptions.Count() > 0)
            {
                double totalDamage = 0;
                foreach (SubscriptionDetailsModel subscription in subscriptions)
                {
                    if (subscription.Platform == StreamingPlatformTypeEnum.Twitch)
                    {
                        if (this.TwitchSubscriptionsDamage.TryGetValue(subscription.TwitchSubscriptionTier, out double damage))
                        {
                            totalDamage += damage;
                        }
                    }
                    else if (subscription.Platform == StreamingPlatformTypeEnum.YouTube)
                    {
                        if (this.YouTubeMembershipsDamage.TryGetValue(subscription.YouTubeMembershipTier, out double damage))
                        {
                            totalDamage += damage;
                        }
                    }
                    else if (subscription.Platform == StreamingPlatformTypeEnum.Trovo)
                    {
                        totalDamage += this.TrovoSubscriptionDamage;
                    }
                }

                await this.DealDamage(subscriptions.First().User, totalDamage);
            }
        }

        private async void EventService_OnDonationOccurred(object sender, UserDonationModel donation)
        {
            await this.DealDamage(donation.User, this.DonationDamage * donation.Amount);
        }

        private async void EventService_OnTwitchBitsCheeredOccurred(object sender, TwitchUserBitsCheeredModel bitsCheered)
        {
            await this.DealDamage(bitsCheered.User, this.TwitchBitsDamage * bitsCheered.Amount);
        }

        private async void EventService_OnYouTubeSuperChatOccurred(object sender, YouTubeSuperChatViewModel superChat)
        {
            await this.DealDamage(superChat.User, this.YouTubeSuperChatDamage * superChat.Amount);
        }

        private async void EventService_OnTrovoSpellCastOccurred(object sender, TrovoChatSpellViewModel spell)
        {
            if (spell.IsElixir)
            {
                await this.DealDamage(spell.User, this.TrovoSpellDamage * spell.ValueTotal);
            }
        }

        private async Task DealDamage(UserV2ViewModel user, double amount)
        {
            if (amount > 0)
            {
                int damage = (int)Math.Round(amount);

                if (this.CurrentBoss == user.ID && this.BossSelfHealingMultiplier > 0)
                {
                    this.CurrentHealth += damage;
                    await this.CallFunction("heal", this.GetDataProperties());

                    await ServiceManager.Get<CommandService>().Queue(this.HealingOccurredCommandID, new CommandParametersModel(user));
                }
                else
                {
                    this.CurrentHealth -= damage;
                    if (this.CurrentHealth > 0)
                    {
                        await this.CallFunction("damage", this.GetDataProperties());

                        await ServiceManager.Get<CommandService>().Queue(this.DamageOccurredCommandID, new CommandParametersModel(user));
                    }
                    else
                    {
                        this.CurrentMaxHealth += this.KillBonusHealth;
                        this.CurrentMaxHealth += Math.Abs(this.CurrentHealth) * this.OverkillBonusMultiplier;
                        this.CurrentHealth = this.CurrentMaxHealth;

                        await this.CallFunction("newboss", new Dictionary<string, string>()
                        {
                            { BossImageProperty, user.AvatarLink },
                            { BossNameProperty, user.DisplayName },
                            { BossHealthProperty, this.CurrentMaxHealth.ToString() },
                            { BossMaxHealthProperty, this.CurrentMaxHealth.ToString() },
                            { BossHealthBarRemainingProperty, "100" },
                        });

                        await ServiceManager.Get<CommandService>().Queue(this.NewBossCommandID, new CommandParametersModel(user));
                    }
                }
            }
        }

        private Dictionary<string, string> GetDataProperties()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data[BossHealthProperty] = this.CurrentHealth.ToString();
            data[BossHealthBarRemainingProperty] = this.HealthRemainingPercentage.ToString();
            return data;
        }
    }
}
