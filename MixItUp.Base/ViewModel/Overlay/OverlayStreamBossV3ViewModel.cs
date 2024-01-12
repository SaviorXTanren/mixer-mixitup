using Google.Apis.YouTube.v3.Data;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using MixItUp.Base.Services.YouTube;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.ObjectModel;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayStreamBossYouTubeMembershipViewModel : UIViewModelBase
    {
        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                this.NotifyPropertyChanged();
            }
        }
        private string name;

        public int DamageAmount
        {
            get { return this.damageAmount; }
            set
            {
                this.damageAmount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        private int damageAmount;

        public OverlayStreamBossYouTubeMembershipViewModel(string name, int damageAmount)
        {
            this.Name = name;
            this.DamageAmount = damageAmount;
        }
    }

    public class OverlayStreamBossV3ViewModel : OverlayVisualTextV3ViewModelBase
    {
        private const double SampleIntegerAmount = 123;
        private const double SampleDecimalAmount = 12.34;

        public override string DefaultHTML { get { return OverlayStreamBossV3Model.DefaultHTML; } }
        public override string DefaultCSS { get { return OverlayStreamBossV3Model.DefaultCSS; } }
        public override string DefaultJavascript { get { return OverlayStreamBossV3Model.DefaultJavascript; } }

        public string BorderColor
        {
            get { return this.borderColor; }
            set
            {
                this.borderColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private string borderColor;

        public string BackgroundColor
        {
            get { return this.backgroundColor; }
            set
            {
                this.backgroundColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private string backgroundColor;

        public string HealthColor
        {
            get { return this.healthColor; }
            set
            {
                this.healthColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private string healthColor;

        public string DamageColor
        {
            get { return this.damageColor; }
            set
            {
                this.damageColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private string damageColor;

        public int BaseHealth
        {
            get { return this.baseHealth; }
            set
            {
                this.baseHealth = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        private int baseHealth;

        public int KillBonusHealth
        {
            get { return this.killBonusHealth; }
            set
            {
                this.killBonusHealth = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        private int killBonusHealth;

        public double OverkillBonusHealthMultiplier
        {
            get { return this.overkillBonusHealthMultiplier; }
            set
            {
                this.overkillBonusHealthMultiplier = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        private double overkillBonusHealthMultiplier;

        public double SelfHealingMultiplier
        {
            get { return this.selfHealingMultiplier; }
            set
            {
                this.selfHealingMultiplier = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        private double selfHealingMultiplier;

        public int FollowDamage
        {
            get { return this.followDamage; }
            set
            {
                this.followDamage = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        private int followDamage;

        public int RaidDamage
        {
            get { return this.raidDamage; }
            set
            {
                this.raidDamage = Math.Max(value, 0);
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.RaidDamageEquation));
            }
        }
        private int raidDamage;

        public double RaidPerViewDamage
        {
            get { return this.raidPerViewDamage; }
            set
            {
                this.raidPerViewDamage = Math.Max(value, 0);
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.RaidDamageEquation));
            }
        }
        private double raidPerViewDamage;

        public string RaidDamageEquation
        {
            get
            {
                int total = (int)Math.Round(this.RaidDamage + (this.RaidPerViewDamage * SampleIntegerAmount));
                return $"{this.RaidDamage} + ({this.RaidPerViewDamage} * {SampleIntegerAmount} {Resources.Viewers}) = {total} {Resources.Damage}";
            }
        }

        public int TwitchSubscriptionTier1Damage
        {
            get { return this.twitchSubscriptionTier1Damage; }
            set
            {
                this.twitchSubscriptionTier1Damage = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        private int twitchSubscriptionTier1Damage;

        public int TwitchSubscriptionTier2Damage
        {
            get { return this.twitchSubscriptionTier2Damage; }
            set
            {
                this.twitchSubscriptionTier2Damage = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        private int twitchSubscriptionTier2Damage;

        public int TwitchSubscriptionTier3Damage
        {
            get { return this.twitchSubscriptionTier3Damage; }
            set
            {
                this.twitchSubscriptionTier3Damage = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        private int twitchSubscriptionTier3Damage;

        public ObservableCollection<OverlayStreamBossYouTubeMembershipViewModel> YouTubeMemberships { get; set; } = new ObservableCollection<OverlayStreamBossYouTubeMembershipViewModel>();

        public int TrovoSubscriptionTier1Damage
        {
            get { return this.trovoSubscriptionTier1Damage; }
            set
            {
                this.trovoSubscriptionTier1Damage = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        private int trovoSubscriptionTier1Damage;

        public double TwitchBitsDamage
        {
            get { return this.twitchBitsDamage; }
            set
            {
                this.twitchBitsDamage = Math.Max(value, 0);
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.TwitchBitsDamageEquation));
            }
        }
        private double twitchBitsDamage;

        public string TwitchBitsDamageEquation
        {
            get
            {
                int total = (int)Math.Round(this.TwitchBitsDamage * SampleIntegerAmount);
                return $"{this.TwitchBitsDamage} * {SampleIntegerAmount} {Resources.Bits} = {total} {Resources.Damage}";
            }
        }

        public double YouTubeSuperChatDamage
        {
            get { return this.youTubeSuperChatDamage; }
            set
            {
                this.youTubeSuperChatDamage = Math.Max(value, 0);
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.YouTubeSuperChatDamageEquation));
            }
        }
        private double youTubeSuperChatDamage;

        public string YouTubeSuperChatDamageEquation
        {
            get
            {
                int total = (int)Math.Round(this.YouTubeSuperChatDamage * SampleDecimalAmount);
                return $"{this.YouTubeSuperChatDamage} * {CurrencyHelper.ToCurrencyString(SampleDecimalAmount)} = {total} {Resources.Damage}";
            }
        }

        public double TrovoElixirSpellDamage
        {
            get { return this.trovoElixirSpellDamage; }
            set
            {
                this.trovoElixirSpellDamage = Math.Max(value, 0);
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.TrovoElixirSpellDamageEquation));
            }
        }
        private double trovoElixirSpellDamage;

        public string TrovoElixirSpellDamageEquation
        {
            get
            {
                int total = (int)Math.Round(this.TrovoElixirSpellDamage * SampleIntegerAmount);
                return $"{this.TrovoElixirSpellDamage} * {SampleIntegerAmount} {Resources.Elixir} = {total} {Resources.Damage}";
            }
        }

        public double DonationDamage
        {
            get { return this.donationDamage; }
            set
            {
                this.donationDamage = Math.Max(value, 0);
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(DonationDamageEquation));
            }
        }
        private double donationDamage;

        public string DonationDamageEquation
        {
            get
            {
                int total = (int)Math.Round(this.DonationDamage * SampleDecimalAmount);
                return $"{this.DonationDamage} * {CurrencyHelper.ToCurrencyString(SampleDecimalAmount)} = {total} {Resources.Damage}";
            }
        }

        public CustomCommandModel DamageOccurredCommand
        {
            get { return this.damageOccurredCommand; }
            set
            {
                this.damageOccurredCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel damageOccurredCommand;

        public CustomCommandModel HealingOccurredCommand
        {
            get { return this.healingOccurredCommand; }
            set
            {
                this.healingOccurredCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel healingOccurredCommand;

        public CustomCommandModel NewBossCommand
        {
            get { return this.newBossCommand; }
            set
            {
                this.newBossCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel newBossCommand;

        public OverlayAnimationV3ViewModel DamageOcurredAnimation;
        public OverlayAnimationV3ViewModel HealingOcurredAnimation;
        public OverlayAnimationV3ViewModel NewBossAnimation;

        public OverlayStreamBossV3ViewModel()
            : base(OverlayItemV3Type.StreamBoss)
        {
            this.FontSize = 16;

            this.BorderColor = "Black";
            this.BackgroundColor = "Azure";
            this.HealthColor = "Green";
            this.DamageColor = "Red";

            this.BaseHealth = 1000;
            this.KillBonusHealth = 100;
            this.OverkillBonusHealthMultiplier = 1.5;
            this.SelfHealingMultiplier = 1.5;

            this.FollowDamage = 10;

            this.RaidDamage = 10;
            this.RaidPerViewDamage = 5.0;

            this.TwitchSubscriptionTier1Damage = 100;
            this.TwitchSubscriptionTier2Damage = 200;
            this.TwitchSubscriptionTier3Damage = 300;
            this.TwitchBitsDamage = 0.1;

            if (ServiceManager.Get<YouTubeSessionService>().IsConnected)
            {
                foreach (MembershipsLevel membershipsLevel in ServiceManager.Get<YouTubeSessionService>().MembershipLevels)
                {
                    this.YouTubeMemberships.Add(new OverlayStreamBossYouTubeMembershipViewModel(membershipsLevel.Snippet.LevelDetails.DisplayName, 0));
                }
            }
            this.YouTubeSuperChatDamage = 10;

            this.TrovoSubscriptionTier1Damage = 100;
            this.TrovoElixirSpellDamage = 0.1;

            this.DonationDamage = 10;

            this.DamageOccurredCommand = this.CreateEmbeddedCommand(Resources.DamageOccurred);
            this.HealingOccurredCommand = this.CreateEmbeddedCommand(Resources.HealingOccurred);
            this.NewBossCommand = this.CreateEmbeddedCommand(Resources.NewBoss);

            this.DamageOcurredAnimation = new OverlayAnimationV3ViewModel(Resources.DamageOccurred, new OverlayAnimationV3Model());
            this.HealingOcurredAnimation = new OverlayAnimationV3ViewModel(Resources.HealingOccurred, new OverlayAnimationV3Model());
            this.NewBossAnimation = new OverlayAnimationV3ViewModel(Resources.NewBoss, new OverlayAnimationV3Model());

            this.Animations.Add(this.DamageOcurredAnimation);
            this.Animations.Add(this.HealingOcurredAnimation);
            this.Animations.Add(this.NewBossAnimation);
        }

        public OverlayStreamBossV3ViewModel(OverlayStreamBossV3Model item)
            : base(item)
        {
            this.BorderColor = item.BorderColor;
            this.BackgroundColor = item.BackgroundColor;
            this.HealthColor = item.HealthColor;
            this.DamageColor = item.DamageColor;

            this.BaseHealth = item.BaseHealth;
            this.KillBonusHealth = item.KillBonusHealth;
            this.OverkillBonusHealthMultiplier = item.OverkillBonusHealthMultiplier;
            this.SelfHealingMultiplier = item.SelfHealingMultiplier;

            this.FollowDamage = item.FollowDamage;
            
            this.RaidDamage = item.RaidDamage;
            this.RaidPerViewDamage = item.RaidPerViewDamage;

            this.TwitchSubscriptionTier1Damage = item.TwitchSubscriptionsDamage[1];
            this.TwitchSubscriptionTier2Damage = item.TwitchSubscriptionsDamage[2];
            this.TwitchSubscriptionTier3Damage = item.TwitchSubscriptionsDamage[3];
            this.TwitchBitsDamage = item.TwitchBitsDamage;

            if (ServiceManager.Get<YouTubeSessionService>().IsConnected)
            {
                foreach (MembershipsLevel membershipsLevel in ServiceManager.Get<YouTubeSessionService>().MembershipLevels)
                {
                    if (item.YouTubeMembershipsDamage.TryGetValue(membershipsLevel.Snippet.LevelDetails.DisplayName, out int damageAmount))
                    {
                        this.YouTubeMemberships.Add(new OverlayStreamBossYouTubeMembershipViewModel(membershipsLevel.Snippet.LevelDetails.DisplayName, damageAmount));
                    }
                    else
                    {
                        this.YouTubeMemberships.Add(new OverlayStreamBossYouTubeMembershipViewModel(membershipsLevel.Snippet.LevelDetails.DisplayName, 0));
                    }
                }
            }
            this.YouTubeSuperChatDamage = item.YouTubeSuperChatDamage;

            this.TrovoSubscriptionTier1Damage = item.TrovoSubscriptionsDamage[1];
            this.TrovoElixirSpellDamage = item.TrovoElixirSpellDamage;

            this.DonationDamage = item.DonationDamage;

            this.DamageOccurredCommand = this.GetEmbeddedCommand(item.DamageOccurredCommandID, Resources.DamageOccurred);
            this.HealingOccurredCommand = this.GetEmbeddedCommand(item.HealingOccurredCommandID, Resources.HealingOccurred);
            this.NewBossCommand = this.GetEmbeddedCommand(item.NewBossCommandID, Resources.NewBoss);

            this.DamageOcurredAnimation = new OverlayAnimationV3ViewModel(Resources.DamageOccurred, item.DamageAnimation);
            this.HealingOcurredAnimation = new OverlayAnimationV3ViewModel(Resources.HealingOccurred, item.HealingAnimation);
            this.NewBossAnimation = new OverlayAnimationV3ViewModel(Resources.NewBoss, item.NewBossAnimation);

            this.Animations.Add(this.DamageOcurredAnimation);
            this.Animations.Add(this.HealingOcurredAnimation);
            this.Animations.Add(this.NewBossAnimation);
        }

        public override Result Validate()
        {
            return new Result();
        }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayStreamBossV3Model result = new OverlayStreamBossV3Model();

            this.AssignProperties(result);

            result.BorderColor = this.BorderColor;
            result.BackgroundColor = this.BackgroundColor;
            result.HealthColor = this.HealthColor;
            result.DamageColor = this.DamageColor;

            result.BaseHealth = this.BaseHealth;
            result.KillBonusHealth = this.KillBonusHealth;
            result.OverkillBonusHealthMultiplier = this.OverkillBonusHealthMultiplier;
            result.SelfHealingMultiplier = this.SelfHealingMultiplier;

            result.FollowDamage = this.FollowDamage;

            result.RaidDamage = this.RaidDamage;
            result.RaidPerViewDamage = this.RaidPerViewDamage;

            result.TwitchSubscriptionsDamage[1] = this.TwitchSubscriptionTier1Damage;
            result.TwitchSubscriptionsDamage[2] = this.TwitchSubscriptionTier2Damage;
            result.TwitchSubscriptionsDamage[3] = this.TwitchSubscriptionTier3Damage;
            result.TwitchBitsDamage = this.TwitchBitsDamage;

            result.YouTubeMembershipsDamage.Clear();
            foreach (OverlayStreamBossYouTubeMembershipViewModel membership in this.YouTubeMemberships)
            {
                result.YouTubeMembershipsDamage[membership.Name] = membership.DamageAmount;
            }
            result.YouTubeSuperChatDamage = this.YouTubeSuperChatDamage;

            result.TrovoSubscriptionsDamage[1] = this.TrovoSubscriptionTier1Damage;
            result.TrovoElixirSpellDamage = this.TrovoElixirSpellDamage;

            result.DonationDamage = this.DonationDamage;

            result.DamageOccurredCommandID = this.DamageOccurredCommand.ID;
            ChannelSession.Settings.SetCommand(this.DamageOccurredCommand);

            result.HealingOccurredCommandID = this.HealingOccurredCommand.ID;
            ChannelSession.Settings.SetCommand(this.HealingOccurredCommand);

            result.NewBossCommandID = this.NewBossCommand.ID;
            ChannelSession.Settings.SetCommand(this.NewBossCommand);

            result.DamageAnimation = this.DamageOcurredAnimation.GetAnimation();
            result.HealingAnimation = this.HealingOcurredAnimation.GetAnimation();
            result.NewBossAnimation = this.NewBossAnimation.GetAnimation();

            return result;
        }
    }
}
