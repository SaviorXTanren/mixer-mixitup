using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayTimerV3ViewModel : OverlayVisualTextV3ViewModelBase
    {
        public override string DefaultHTML { get { return OverlayTimerV3Model.DefaultHTML; } }
        public override string DefaultCSS { get { return OverlayTimerV3Model.DefaultCSS; } }
        public override string DefaultJavascript { get { return OverlayTimerV3Model.DefaultJavascript; } }

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

        public bool CountUp
        {
            get { return this.countUp; }
            set
            {
                this.countUp = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool countUp;

        public OverlayTimerV3ViewModel()
            : base(OverlayItemV3Type.Timer)
        {
            this.DisplayFormat = OverlayTimerV3Model.DefaultDisplayFormat;
        }

        public OverlayTimerV3ViewModel(OverlayTimerV3Model item)
            : base(item)
        {
            this.DisplayFormat = item.DisplayFormat;
            this.CountUp = item.CountUp;
        }

        public override Result Validate()
        {
            if (string.IsNullOrWhiteSpace(this.DisplayFormat))
            {
                return new Result(Resources.OverlayTimerMissingDisplayFormat);
            }

            return new Result();
        }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayTimerV3Model result = new OverlayTimerV3Model()
            {
                DisplayFormat = this.DisplayFormat,
                CountUp = this.CountUp,
            };
            this.AssignProperties(result);
            return result;
        }
    }
}
