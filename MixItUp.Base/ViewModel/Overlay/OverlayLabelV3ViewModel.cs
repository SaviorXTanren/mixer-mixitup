using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayLabelDisplayV3ViewModel : UIViewModelBase
    {
        public OverlayLabelDisplayV3TypeEnum Type { get; private set; }

        public bool IsEnabled
        {
            get { return this.isEnabled; }
            set
            {
                this.isEnabled = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool isEnabled;

        public string Format
        {
            get { return this.format; }
            set
            {
                this.format = value;
                this.NotifyPropertyChanged();
            }
        }
        private string format;

        public OverlayLabelDisplayV3Model Model { get; private set; }

        public OverlayLabelDisplayV3ViewModel(OverlayLabelDisplayV3TypeEnum type)
        {
            this.Type = type;
            switch (this.Type)
            {
                case OverlayLabelDisplayV3TypeEnum.ViewerCount:
                case OverlayLabelDisplayV3TypeEnum.ChatterCount:
                case OverlayLabelDisplayV3TypeEnum.Counter:
                case OverlayLabelDisplayV3TypeEnum.TotalFollowers:
                case OverlayLabelDisplayV3TypeEnum.TotalSubscribers:
                    this.Format = OverlayResources.OverlayLabelAmountDefaultFormat;
                    break;

                case OverlayLabelDisplayV3TypeEnum.LatestFollower:
                    this.Format = OverlayResources.OverlayLabelUsernameDefaultFormat;
                    break;

                case OverlayLabelDisplayV3TypeEnum.LatestRaid:
                case OverlayLabelDisplayV3TypeEnum.LatestSubscriber:
                case OverlayLabelDisplayV3TypeEnum.LatestDonation:
                case OverlayLabelDisplayV3TypeEnum.LatestTwitchBits:
                case OverlayLabelDisplayV3TypeEnum.LatestTrovoElixir:
                case OverlayLabelDisplayV3TypeEnum.LatestYouTubeSuperChat:
                    this.Format = OverlayResources.OverlayLabelUsernameAmountDefaultFormat;
                    break;
            }
        }

        public OverlayLabelDisplayV3ViewModel(OverlayLabelDisplayV3Model model)
        {
            this.Model = model;

            this.Type = model.Type;
            this.IsEnabled = model.IsEnabled;
            this.Format = model.Format;
        }
    }

    public class OverlayLabelV3ViewModel : OverlayVisualTextV3ViewModelBase
    {
        public override string DefaultHTML { get { return OverlayLabelV3Model.DefaultHTML; } }
        public override string DefaultCSS { get { return OverlayLabelV3Model.DefaultCSS; } }
        public override string DefaultJavascript { get { return OverlayLabelV3Model.DefaultJavascript; } }

        public int DisplayRotationSeconds
        {
            get { return this.displayRotationSeconds; }
            set
            {
                this.displayRotationSeconds = value;
                this.NotifyPropertyChanged();
            }
        }
        private int displayRotationSeconds = 5;

        public ObservableCollection<OverlayLabelDisplayV3ViewModel> Displays { get; set; } = new ObservableCollection<OverlayLabelDisplayV3ViewModel>();

        public OverlayLabelV3ViewModel() : base(OverlayItemV3Type.Label) { }

        public OverlayLabelV3ViewModel(OverlayLabelV3Model item) : base(item) { }

        public override Result Validate()
        {
            if (this.DisplayRotationSeconds <= 0)
            {
                return new Result(Resources.OverlayLabelErrorDisplayRotationMustBePositiveNumber);
            }

            if (this.Displays.All(d => !d.IsEnabled))
            {
                return new Result(Resources.OverlayLabelErrorAtLeastOneDisplayTypeMustBeEnabled);
            }

            return new Result();
        }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayLabelV3Model result = new OverlayLabelV3Model();

            this.AssignProperties(result);
            result.DisplayRotationSeconds = this.DisplayRotationSeconds;

            foreach (OverlayLabelDisplayV3ViewModel display in this.Displays)
            {
                result.Displays[display.Type] = new OverlayLabelDisplayV3Model()
                {
                    Type = display.Type,
                    IsEnabled = display.IsEnabled,
                    Format = display.Format,

                    UserID = display.Model?.UserID ?? Guid.Empty,
                    Amount = display.Model?.Amount ?? 0
                };
            }

            return result;
        }
    }
}
