using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayLabelDisplayV3ViewModel : UIViewModelBase
    {
        public OverlayLabelDisplayV3TypeEnum Type { get; private set; }

        public string TypeString { get { return Resources.ResourceManager.GetSafeString(this.Type.ToString()); } }

        public bool IsCounterType { get { return this.Type == OverlayLabelDisplayV3TypeEnum.Counter; } }

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

        public ObservableCollection<CounterModel> Counters { get; set; } = new ObservableCollection<CounterModel>();

        public CounterModel SelectedCounter
        {
            get { return this.selectedCounter; }
            set
            {
                this.selectedCounter = value;
                this.NotifyPropertyChanged();
            }
        }
        private CounterModel selectedCounter;

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
                case OverlayLabelDisplayV3TypeEnum.LatestSubscriber:
                    this.Format = OverlayResources.OverlayLabelUsernameDefaultFormat;
                    break;

                case OverlayLabelDisplayV3TypeEnum.LatestRaid:
                case OverlayLabelDisplayV3TypeEnum.LatestDonation:
                case OverlayLabelDisplayV3TypeEnum.LatestTwitchBits:
                case OverlayLabelDisplayV3TypeEnum.LatestTrovoElixir:
                case OverlayLabelDisplayV3TypeEnum.LatestYouTubeSuperChat:
                    this.Format = OverlayResources.OverlayLabelUsernameAmountDefaultFormat;
                    break;
            }

            this.LoadCounters();
        }

        public OverlayLabelDisplayV3ViewModel(OverlayLabelDisplayV3Model model)
        {
            this.Model = model;

            this.Type = model.Type;
            this.IsEnabled = model.IsEnabled;
            this.Format = model.Format;

            this.LoadCounters();
            if (this.Type == OverlayLabelDisplayV3TypeEnum.Counter && !string.IsNullOrEmpty(this.Model.CounterName))
            {
                this.SelectedCounter = this.Counters.FirstOrDefault(c => string.Equals(c.Name, this.Model.CounterName, StringComparison.OrdinalIgnoreCase));
            }
        }

        public Result Validate()
        {
            if (!this.IsEnabled)
            {
                return new Result();
            }

            if (string.IsNullOrEmpty(this.Format))
            {
                return new Result(Resources.OverlayLabelDisplayFormatMustHaveValidValue);
            }

            if (this.Type == OverlayLabelDisplayV3TypeEnum.Counter && this.SelectedCounter == null)
            {
                return new Result(Resources.OverlayLabelCounterNotSelected);
            }

            return new Result();
        }

        private void LoadCounters()
        {
            if (this.Type == OverlayLabelDisplayV3TypeEnum.Counter)
            {
                foreach (var counter in ChannelSession.Settings.Counters)
                {
                    this.Counters.Add(counter.Value);
                }
            }
        }
    }

    public class OverlayLabelV3ViewModel : OverlayVisualTextV3ViewModelBase
    {
        public override string DefaultHTML { get { return OverlayLabelV3Model.DefaultHTML; } }
        public override string DefaultCSS { get { return OverlayLabelV3Model.DefaultCSS; } }
        public override string DefaultJavascript { get { return OverlayLabelV3Model.DefaultJavascript; } }

        public IEnumerable<OverlayLabelDisplayV3SettingTypeEnum> DisplaySettings { get; private set; } = EnumHelper.GetEnumList<OverlayLabelDisplayV3SettingTypeEnum>();

        public OverlayLabelDisplayV3SettingTypeEnum SelectedDisplaySetting
        {
            get { return this.selectedDisplaySetting; }
            set
            {
                this.selectedDisplaySetting = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.IsRotationDisplaySetting));
            }
        }
        private OverlayLabelDisplayV3SettingTypeEnum selectedDisplaySetting;

        public bool IsRotationDisplaySetting { get { return this.SelectedDisplaySetting == OverlayLabelDisplayV3SettingTypeEnum.RotatingDisplays; } }

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

        public OverlayLabelV3ViewModel()
            : base(OverlayItemV3Type.Label)
        {
            this.Initialize();
        }

        public OverlayLabelV3ViewModel(OverlayLabelV3Model item)
            : base(item)
        {
            foreach (var display in item.Displays)
            {
                this.Displays.Add(new OverlayLabelDisplayV3ViewModel(display.Value));
            }
            this.Initialize();
        }

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

            foreach (var display in this.Displays)
            {
                Result result = display.Validate();
                if (!result.Success)
                {
                    return result;
                }
            }

            return new Result();
        }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayLabelV3Model result = new OverlayLabelV3Model();

            this.AssignProperties(result);

            result.DisplaySetting = this.SelectedDisplaySetting;
            result.DisplayRotationSeconds = this.DisplayRotationSeconds;

            foreach (OverlayLabelDisplayV3ViewModel display in this.Displays)
            {
                result.Displays[display.Type] = new OverlayLabelDisplayV3Model()
                {
                    Type = display.Type,
                    IsEnabled = display.IsEnabled,
                    Format = display.Format,

                    UserID = display.Model?.UserID ?? Guid.Empty,
                    Amount = display.Model?.Amount ?? 0,

                    CounterName = display.SelectedCounter?.Name ?? null
                };
            }

            return result;
        }

        private void Initialize()
        {
            foreach (OverlayLabelDisplayV3TypeEnum labelType in EnumHelper.GetEnumList<OverlayLabelDisplayV3TypeEnum>())
            {
                if (!this.Displays.Any(d => d.Type == labelType))
                {
                    this.Displays.Add(new OverlayLabelDisplayV3ViewModel(labelType));
                }
            }
        }
    }
}
