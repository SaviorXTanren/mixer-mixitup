using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Util;
using System;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Overlay
{
    public enum OverlayStreamBossV3TestType
    {
        Damage,
        Heal,
        NewBoss,
    }

    public class OverlayStreamBossV3ViewModel : OverlayEventTrackingV3ViewModelBase
    {
        public override string DefaultHTML { get { return OverlayStreamBossV3Model.DefaultHTML; } }
        public override string DefaultCSS { get { return OverlayStreamBossV3Model.DefaultCSS; } }
        public override string DefaultJavascript { get { return OverlayStreamBossV3Model.DefaultJavascript; } }

        public override bool IsTestable { get { return true; } }

        public override string EquationUnits { get { return Resources.Damage; } }

        public string Height
        {
            get { return this.height > 0 ? this.height.ToString() : string.Empty; }
            set
            {
                this.height = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        protected int height;

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

        public bool CompoundPreviousBossHealth
        {
            get { return this.compoundPreviousBossHealth; }
            set
            {
                this.compoundPreviousBossHealth = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool compoundPreviousBossHealth;

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
            this.width = 450;
            this.height = 125;

            this.FontSize = 16;

            this.BorderColor = "Black";
            this.BackgroundColor = "Azure";
            this.HealthColor = "Green";
            this.DamageColor = "Red";

            this.BaseHealth = 1000;
            this.KillBonusHealth = 100;
            this.OverkillBonusHealthMultiplier = 1.5;
            this.SelfHealingMultiplier = 1.5;
            this.CompoundPreviousBossHealth = true;

            this.FollowAmount = 10;

            this.RaidAmount = 10;
            this.RaidPerViewAmount = 5.0;

            this.TwitchSubscriptionTier1Amount = 100;
            this.TwitchSubscriptionTier2Amount = 200;
            this.TwitchSubscriptionTier3Amount = 300;
            this.TwitchBitsAmount = 0.1;

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
            this.width = item.Width;
            this.height = item.Height;

            this.BorderColor = item.BorderColor;
            this.BackgroundColor = item.BackgroundColor;
            this.HealthColor = item.HealthColor;
            this.DamageColor = item.DamageColor;

            this.BaseHealth = item.BaseHealth;
            this.KillBonusHealth = item.KillBonusHealth;
            this.OverkillBonusHealthMultiplier = item.OverkillBonusHealthMultiplier;
            this.SelfHealingMultiplier = item.SelfHealingMultiplier;
            this.CompoundPreviousBossHealth = item.CompoundPreviousBossHealth;

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

        public override async Task TestWidget(OverlayWidgetV3Model widget)
        {
            OverlayStreamBossV3Model streamBoss = (OverlayStreamBossV3Model)widget.Item;

            object result = await DialogHelper.ShowEnumDropDown(EnumHelper.GetEnumList<OverlayStreamBossV3TestType>());
            if (result != null)
            {
                OverlayStreamBossV3TestType type = (OverlayStreamBossV3TestType)result;
                if (type == OverlayStreamBossV3TestType.Damage)
                {
                    await streamBoss.ProcessEvent(ChannelSession.User, this.BaseHealth / 2, forceDamage: true);
                }
                else if (type == OverlayStreamBossV3TestType.Heal)
                {
                    await streamBoss.ProcessEvent(ChannelSession.User, this.BaseHealth / 2);
                }
                else if (type == OverlayStreamBossV3TestType.NewBoss)
                {
                    await streamBoss.ProcessEvent(ChannelSession.User, streamBoss.CurrentHealth, forceDamage: true);
                }
            }

            await base.TestWidget(widget);
        }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayStreamBossV3Model result = new OverlayStreamBossV3Model();

            this.AssignProperties(result);

            result.Height = this.height;

            result.BorderColor = this.BorderColor;
            result.BackgroundColor = this.BackgroundColor;
            result.HealthColor = this.HealthColor;
            result.DamageColor = this.DamageColor;

            result.BaseHealth = this.BaseHealth;
            result.KillBonusHealth = this.KillBonusHealth;
            result.OverkillBonusHealthMultiplier = this.OverkillBonusHealthMultiplier;
            result.SelfHealingMultiplier = this.SelfHealingMultiplier;
            result.CompoundPreviousBossHealth = this.CompoundPreviousBossHealth;

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
