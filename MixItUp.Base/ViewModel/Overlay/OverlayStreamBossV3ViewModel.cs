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

        public int Amount
        {
            get { return this.damageAmount; }
            set
            {
                this.damageAmount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        private int damageAmount;

        public OverlayStreamBossYouTubeMembershipViewModel(string name, int amount)
        {
            this.Name = name;
            this.Amount = amount;
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

        public int FollowAmount
        {
            get { return this.followAmount; }
            set
            {
                this.followAmount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        private int followAmount;

        public int RaidAmount
        {
            get { return this.raidAmount; }
            set
            {
                this.raidAmount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.RaidEquation));
            }
        }
        private int raidAmount;

        public double RaidPerViewAmount
        {
            get { return this.raidPerViewAmount; }
            set
            {
                this.raidPerViewAmount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.RaidEquation));
            }
        }
        private double raidPerViewAmount;

        public string RaidEquation
        {
            get
            {
                int total = (int)Math.Round(this.RaidAmount + (this.RaidPerViewAmount * SampleIntegerAmount));
                return $"{this.RaidAmount} + ({this.RaidPerViewAmount} * {SampleIntegerAmount} {Resources.Viewers}) = {total} {Resources.Damage}";
            }
        }

        public int TwitchSubscriptionTier1Amount
        {
            get { return this.twitchSubscriptionTier1Amount; }
            set
            {
                this.twitchSubscriptionTier1Amount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        private int twitchSubscriptionTier1Amount;

        public int TwitchSubscriptionTier2Amount
        {
            get { return this.twitchSubscriptionTier2Amount; }
            set
            {
                this.twitchSubscriptionTier2Amount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        private int twitchSubscriptionTier2Amount;

        public int TwitchSubscriptionTier3Amount
        {
            get { return this.twitchSubscriptionTier3Amount; }
            set
            {
                this.twitchSubscriptionTier3Amount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        private int twitchSubscriptionTier3Amount;

        public ObservableCollection<OverlayStreamBossYouTubeMembershipViewModel> YouTubeMemberships { get; set; } = new ObservableCollection<OverlayStreamBossYouTubeMembershipViewModel>();

        public int TrovoSubscriptionTier1Amount
        {
            get { return this.trovoSubscriptionTier1Amount; }
            set
            {
                this.trovoSubscriptionTier1Amount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        private int trovoSubscriptionTier1Amount;

        public int TrovoSubscriptionTier2Amount
        {
            get { return this.trovoSubscriptionTier2Amount; }
            set
            {
                this.trovoSubscriptionTier2Amount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        private int trovoSubscriptionTier2Amount;

        public int TrovoSubscriptionTier3Amount
        {
            get { return this.trovoSubscriptionTier3Amount; }
            set
            {
                this.trovoSubscriptionTier3Amount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        private int trovoSubscriptionTier3Amount;

        public double TwitchBitsAmount
        {
            get { return this.twitchBitsAmount; }
            set
            {
                this.twitchBitsAmount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.TwitchBitsEquation));
            }
        }
        private double twitchBitsAmount;

        public string TwitchBitsEquation
        {
            get
            {
                int total = (int)Math.Round(this.TwitchBitsAmount * SampleIntegerAmount);
                return $"{this.TwitchBitsAmount} * {SampleIntegerAmount} {Resources.Bits} = {total} {Resources.Damage}";
            }
        }

        public double YouTubeSuperChatAmount
        {
            get { return this.youTubeSuperChatAmount; }
            set
            {
                this.youTubeSuperChatAmount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.YouTubeSuperChatEquation));
            }
        }
        private double youTubeSuperChatAmount;

        public string YouTubeSuperChatEquation
        {
            get
            {
                int total = (int)Math.Round(this.YouTubeSuperChatAmount * SampleDecimalAmount);
                return $"{this.YouTubeSuperChatAmount} * {CurrencyHelper.ToCurrencyString(SampleDecimalAmount)} = {total} {Resources.Damage}";
            }
        }

        public double TrovoElixirSpellAmount
        {
            get { return this.trovoElixirSpellAmount; }
            set
            {
                this.trovoElixirSpellAmount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.TrovoElixirSpellEquation));
            }
        }
        private double trovoElixirSpellAmount;

        public string TrovoElixirSpellEquation
        {
            get
            {
                int total = (int)Math.Round(this.TrovoElixirSpellAmount * SampleIntegerAmount);
                return $"{this.TrovoElixirSpellAmount} * {SampleIntegerAmount} {Resources.Elixir} = {total} {Resources.Damage}";
            }
        }

        public double DonationAmount
        {
            get { return this.donationAmount; }
            set
            {
                this.donationAmount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(DonationEquation));
            }
        }
        private double donationAmount;

        public string DonationEquation
        {
            get
            {
                int total = (int)Math.Round(this.DonationAmount * SampleDecimalAmount);
                return $"{this.DonationAmount} * {CurrencyHelper.ToCurrencyString(SampleDecimalAmount)} = {total} {Resources.Damage}";
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

            this.FollowAmount = 10;

            this.RaidAmount = 10;
            this.RaidPerViewAmount = 5.0;

            this.TwitchSubscriptionTier1Amount = 100;
            this.TwitchSubscriptionTier2Amount = 200;
            this.TwitchSubscriptionTier3Amount = 300;
            this.TwitchBitsAmount = 0.1;

            if (ServiceManager.Get<YouTubeSessionService>().IsConnected)
            {
                foreach (MembershipsLevel membershipsLevel in ServiceManager.Get<YouTubeSessionService>().MembershipLevels)
                {
                    this.YouTubeMemberships.Add(new OverlayStreamBossYouTubeMembershipViewModel(membershipsLevel.Snippet.LevelDetails.DisplayName, 0));
                }
            }
            this.YouTubeSuperChatAmount = 10;

            this.TrovoSubscriptionTier1Amount = 100;
            this.TrovoSubscriptionTier2Amount = 200;
            this.TrovoSubscriptionTier3Amount = 300;
            this.TrovoElixirSpellAmount = 0.1;

            this.DonationAmount = 10;

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

            this.FollowAmount = item.FollowAmount;
            
            this.RaidAmount = item.RaidAmount;
            this.RaidPerViewAmount = item.RaidPerViewAmount;

            this.TwitchSubscriptionTier1Amount = item.TwitchSubscriptionsAmount[1];
            this.TwitchSubscriptionTier2Amount = item.TwitchSubscriptionsAmount[2];
            this.TwitchSubscriptionTier3Amount = item.TwitchSubscriptionsAmount[3];
            this.TwitchBitsAmount = item.TwitchBitsAmount;

            if (ServiceManager.Get<YouTubeSessionService>().IsConnected)
            {
                foreach (MembershipsLevel membershipsLevel in ServiceManager.Get<YouTubeSessionService>().MembershipLevels)
                {
                    if (item.YouTubeMembershipsAmount.TryGetValue(membershipsLevel.Snippet.LevelDetails.DisplayName, out int damageAmount))
                    {
                        this.YouTubeMemberships.Add(new OverlayStreamBossYouTubeMembershipViewModel(membershipsLevel.Snippet.LevelDetails.DisplayName, damageAmount));
                    }
                    else
                    {
                        this.YouTubeMemberships.Add(new OverlayStreamBossYouTubeMembershipViewModel(membershipsLevel.Snippet.LevelDetails.DisplayName, 0));
                    }
                }
            }
            this.YouTubeSuperChatAmount = item.YouTubeSuperChatAmount;

            this.TrovoSubscriptionTier1Amount = item.TrovoSubscriptionsAmount[1];
            this.TrovoSubscriptionTier2Amount = item.TrovoSubscriptionsAmount[2];
            this.TrovoSubscriptionTier3Amount = item.TrovoSubscriptionsAmount[3];
            this.TrovoElixirSpellAmount = item.TrovoElixirSpellAmount;

            this.DonationAmount = item.DonationAmount;

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

            result.FollowAmount = this.FollowAmount;

            result.RaidAmount = this.RaidAmount;
            result.RaidPerViewAmount = this.RaidPerViewAmount;

            result.TwitchSubscriptionsAmount[1] = this.TwitchSubscriptionTier1Amount;
            result.TwitchSubscriptionsAmount[2] = this.TwitchSubscriptionTier2Amount;
            result.TwitchSubscriptionsAmount[3] = this.TwitchSubscriptionTier3Amount;
            result.TwitchBitsAmount = this.TwitchBitsAmount;

            result.YouTubeMembershipsAmount.Clear();
            foreach (OverlayStreamBossYouTubeMembershipViewModel membership in this.YouTubeMemberships)
            {
                result.YouTubeMembershipsAmount[membership.Name] = membership.Amount;
            }
            result.YouTubeSuperChatAmount = this.YouTubeSuperChatAmount;

            result.TrovoSubscriptionsAmount[1] = this.TrovoSubscriptionTier1Amount;
            result.TrovoSubscriptionsAmount[2] = this.TrovoSubscriptionTier2Amount;
            result.TrovoSubscriptionsAmount[3] = this.TrovoSubscriptionTier3Amount;
            result.TrovoElixirSpellAmount = this.TrovoElixirSpellAmount;

            result.DonationAmount = this.DonationAmount;

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
