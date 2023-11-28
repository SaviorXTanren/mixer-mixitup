using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.ViewModel.Overlay.Widget
{
    public class OverlayLabelWidgetV3ViewModel : OverlayWidgetV3ViewModelBase
    {
        //public const string UpdatedAnimationName = "Updated";

        public OverlayLabelV3Type SelectedLabelType
        {
            get { return selectedLabelType; }
            set
            {
                this.selectedLabelType = value;
                this.NotifyPropertyChanged();

                string newHTML = string.Empty;
                switch (this.SelectedLabelType)
                {
                    case OverlayLabelV3Type.ViewerCount:
                    case OverlayLabelV3Type.ChatterCount:
                    case OverlayLabelV3Type.Counter:
                    case OverlayLabelV3Type.TotalFollowers:
                    case OverlayLabelV3Type.TotalSubscribers:
                        newHTML = OverlayLabelV3Model.DefaultAmountHTML;
                        break;
                    case OverlayLabelV3Type.LastestFollower:
                        newHTML = OverlayLabelV3Model.DefaultUsernameHTML;
                        break;
                    case OverlayLabelV3Type.LatestRaid:
                    case OverlayLabelV3Type.LatestSubscriber:
                    case OverlayLabelV3Type.LatestDonation:
                    case OverlayLabelV3Type.LatestTwitchBits:
                    case OverlayLabelV3Type.LatestTrovoElixir:
                        newHTML = OverlayLabelV3Model.DefaultUsernameAmountHTML;
                        break;
                }

                //this.Item.SetPositionWrappedHTML(newHTML);

                this.NotifyPropertyChanged(nameof(this.CounterTypeSelected));
            }
        }
        private OverlayLabelV3Type selectedLabelType;

        public IEnumerable<OverlayLabelV3Type> LabelTypes { get; private set; } = EnumHelper.GetEnumList<OverlayLabelV3Type>();

        public bool CounterTypeSelected { get { return this.SelectedLabelType == OverlayLabelV3Type.Counter; } }

        public CounterModel SelectedCounter
        {
            get { return this.selectedCounter; }
            set
            {
                this.selectedCounter = value;
                NotifyPropertyChanged();
            }
        }
        private CounterModel selectedCounter;

        public List<CounterModel> Counters { get; private set; } = new List<CounterModel>();

        public OverlayLabelWidgetV3ViewModel()
            : base()
        {
            Initialize();

            this.Item = new OverlayLabelV3ViewModel();

            this.SelectedLabelType = OverlayLabelV3Type.LastestFollower;

            //AddAnimations(new List<string>() { UpdatedAnimationName });
        }

        public OverlayLabelWidgetV3ViewModel(OverlayLabelWidgetV3Model widget)
            : base(widget)
        {
            Initialize();

            this.Item = new OverlayLabelV3ViewModel((OverlayLabelV3Model)widget.Item);

            this.SelectedLabelType = widget.LabelType;
            if (this.SelectedLabelType == OverlayLabelV3Type.Counter)
            {
                this.SelectedCounter = this.Counters.FirstOrDefault(c => string.Equals(c.Name, widget.CounterName, StringComparison.OrdinalIgnoreCase));
            }
        }

        public override Result Validate()
        {
            Result result = base.Validate();

            if (result.Success)
            {
                if (this.SelectedLabelType == OverlayLabelV3Type.Counter)
                {
                    if (this.SelectedCounter == null)
                    {
                        return new Result(Resources.OverlayLabelCounterNotSelected);
                    }
                }
            }

            return result;
        }

        public override OverlayWidgetV3ModelBase GetWidget()
        {
            OverlayLabelWidgetV3Model widget = new OverlayLabelWidgetV3Model(this.SelectedLabelType, (OverlayLabelV3Model)this.GetItem());

            if (SelectedLabelType == OverlayLabelV3Type.Counter)
            {
                widget.CounterName = this.SelectedCounter.Name;
            }

            return widget;
        }

        private void Initialize()
        {
            foreach (var counter in ChannelSession.Settings.Counters)
            {
                this.Counters.Add(counter.Value);
            }
            this.SelectedCounter = this.Counters.FirstOrDefault();
        }
    }
}
