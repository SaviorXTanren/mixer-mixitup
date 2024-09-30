using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Util;
using System;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayPersistentTimerV3ViewModel : OverlayEventTrackingV3ViewModelBase
    {
        public override string DefaultHTML { get { return OverlayTimerV3Model.DefaultHTML; } }
        public override string DefaultCSS { get { return OverlayTimerV3Model.DefaultCSS; } }
        public override string DefaultJavascript { get { return OverlayPersistentTimerV3Model.DefaultJavascript; } }

        public override bool IsTestable { get { return true; } }

        public override string EquationUnits { get { return Resources.Seconds; } }

        public int InitialAmount
        {
            get { return this.initialAmount; }
            set
            {
                this.initialAmount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        private int initialAmount;

        public string DisplayFormat
        {
            get { return this.displayFormat; }
            set
            {
                this.displayFormat = value;
                this.NotifyPropertyChanged();
            }
        }
        private string displayFormat;

        public int MaxAmount
        {
            get { return this.maxAmount; }
            set
            {
                this.maxAmount = Math.Max(value, 0);
                this.NotifyPropertyChanged();
            }
        }
        private int maxAmount;

        public bool DisableOnCompletion
        {
            get { return this.disableOnCompletion; }
            set
            {
                this.disableOnCompletion = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool disableOnCompletion;
        public bool ResetOnEnable
        {
            get { return this.resetOnEnable; }
            set
            {
                this.resetOnEnable = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool resetOnEnable;

        public bool AllowAdjustmentWhilePaused
        {
            get { return this.allowAdjustmentWhilePaused; }
            set
            {
                this.allowAdjustmentWhilePaused = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool allowAdjustmentWhilePaused;

        public CustomCommandModel TimerAdjustedCommand
        {
            get { return this.timerAdjustedCommand; }
            set
            {
                this.timerAdjustedCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel timerAdjustedCommand;

        public CustomCommandModel TimerCompletedCommand
        {
            get { return this.timerCompletedCommand; }
            set
            {
                this.timerCompletedCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel timerCompletedCommand;

        public OverlayAnimationV3ViewModel TimerAdjustedAnimation;
        public OverlayAnimationV3ViewModel TimerCompletedAnimation;

        public OverlayPersistentTimerV3ViewModel()
            : base(OverlayItemV3Type.PersistentTimer)
        {
            this.DisplayFormat = OverlayTimerV3Model.DefaultDisplayFormat;
            this.DisableOnCompletion = true;
            this.ResetOnEnable = true;

            this.TimerAdjustedCommand = this.CreateEmbeddedCommand(Resources.TimerAdjusted);
            this.TimerCompletedCommand = this.CreateEmbeddedCommand(Resources.TimerCompleted);

            this.TimerAdjustedAnimation = new OverlayAnimationV3ViewModel(Resources.TimerAdjusted, new OverlayAnimationV3Model());
            this.TimerCompletedAnimation = new OverlayAnimationV3ViewModel(Resources.TimerCompleted, new OverlayAnimationV3Model());

            this.Animations.Add(this.TimerAdjustedAnimation);
            this.Animations.Add(this.TimerCompletedAnimation);
        }

        public OverlayPersistentTimerV3ViewModel(OverlayPersistentTimerV3Model item)
            : base(item)
        {
            this.InitialAmount = item.InitialAmount;
            this.DisplayFormat = item.DisplayFormat;
            this.MaxAmount = item.MaxAmount;
            this.DisableOnCompletion = item.DisableOnCompletion;
            this.ResetOnEnable = item.ResetOnEnable;
            this.AllowAdjustmentWhilePaused = item.AllowAdjustmentWhilePaused;

            this.TimerAdjustedCommand = this.GetEmbeddedCommand(item.TimerAdjustedCommandID, Resources.TimerAdjusted);
            this.TimerCompletedCommand = this.GetEmbeddedCommand(item.TimerCompletedCommandID, Resources.TimerCompleted);

            this.TimerAdjustedAnimation = new OverlayAnimationV3ViewModel(Resources.TimerAdjusted, item.TimerAdjustedAnimation);
            this.TimerCompletedAnimation = new OverlayAnimationV3ViewModel(Resources.TimerCompleted, item.TimerCompletedAnimation);

            this.Animations.Add(this.TimerAdjustedAnimation);
            this.Animations.Add(this.TimerCompletedAnimation);
        }

        public override Result Validate()
        {
            if (string.IsNullOrWhiteSpace(this.DisplayFormat))
            {
                return new Result(Resources.OverlayTimerMissingDisplayFormat);
            }

            return new Result();
        }

        public override async Task TestWidget(OverlayWidgetV3Model widget)
        {
            OverlayPersistentTimerV3Model goal = (OverlayPersistentTimerV3Model)widget.Item;

            await goal.ProcessEvent(ChannelSession.User, 10);

            await base.TestWidget(widget);
        }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayPersistentTimerV3Model result = new OverlayPersistentTimerV3Model()
            {
                InitialAmount = this.InitialAmount,
                DisplayFormat = this.DisplayFormat,
                MaxAmount = this.MaxAmount,
                DisableOnCompletion = this.DisableOnCompletion,
                ResetOnEnable = this.ResetOnEnable,
                AllowAdjustmentWhilePaused = this.AllowAdjustmentWhilePaused,
            };

            this.AssignProperties(result);

            result.TimerAdjustedCommandID = this.TimerAdjustedCommand.ID;
            ChannelSession.Settings.SetCommand(this.TimerAdjustedCommand);

            result.TimerCompletedCommandID = this.TimerCompletedCommand.ID;
            ChannelSession.Settings.SetCommand(this.TimerCompletedCommand);

            result.TimerAdjustedAnimation = this.TimerAdjustedAnimation.GetAnimation();
            result.TimerCompletedAnimation = this.TimerCompletedAnimation.GetAnimation();

            return result;
        }
    }
}
