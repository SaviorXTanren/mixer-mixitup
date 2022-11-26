using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayLabelV3ViewModel : OverlayItemV3ViewModelBase
    {
        public const string UpdatedAnimationName = "Updated";

        public OverlayLabelWidgetV3Type SelectedLabelType
        {
            get { return this.selectedLabelType; }
            set
            {
                this.selectedLabelType = value;
                this.NotifyPropertyChanged();

                switch (this.SelectedLabelType)
                {
                    case OverlayLabelWidgetV3Type.Viewers:
                    case OverlayLabelWidgetV3Type.Chatters:
                    case OverlayLabelWidgetV3Type.Counter:
                    case OverlayLabelWidgetV3Type.TotalFollowers:
                    case OverlayLabelWidgetV3Type.TotalSubscribers:
                        this.HTML = OverlayLabelV3Model.DefaultAmountHTML;
                        break;
                    case OverlayLabelWidgetV3Type.LastestFollower:
                        this.HTML = OverlayLabelV3Model.DefaultNameHTML;
                        break;
                    case OverlayLabelWidgetV3Type.LatestRaid:
                    case OverlayLabelWidgetV3Type.LatestSubscriber:
                    case OverlayLabelWidgetV3Type.LatestDonation:
                    case OverlayLabelWidgetV3Type.LatestTwitchBits:
                    case OverlayLabelWidgetV3Type.LatestTrovoElixir:
                        this.HTML = OverlayLabelV3Model.DefaultNameAmountHTML;
                        break;
                }

                this.NotifyPropertyChanged(nameof(CounterTypeSelected));
            }
        }
        private OverlayLabelWidgetV3Type selectedLabelType;

        public IEnumerable<OverlayLabelWidgetV3Type> LabelTypes { get; private set; } = EnumHelper.GetEnumList<OverlayLabelWidgetV3Type>();

        public bool CounterTypeSelected { get { return this.SelectedLabelType == OverlayLabelWidgetV3Type.Counter; } }

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

        public List<CounterModel> Counters { get; private set; } = new List<CounterModel>();

        public OverlayLabelV3ViewModel()
            : base(OverlayItemV3Type.Label)
        {
            this.Initialize();

            this.SelectedLabelType = OverlayLabelWidgetV3Type.LastestFollower;

            this.AddAnimations(new List<string>() { OverlayLabelV3ViewModel.UpdatedAnimationName });
        }

        public OverlayLabelV3ViewModel(OverlayLabelV3Model item)
            : base(item)
        {
            this.Initialize();

            this.SelectedLabelType = item.LabelType;
            if (this.SelectedLabelType == OverlayLabelWidgetV3Type.Counter)
            {
                this.SelectedCounter = this.Counters.FirstOrDefault(c => string.Equals(c.Name, item.CounterName, StringComparison.OrdinalIgnoreCase));
            }
        }

        public override Result Validate()
        {
            Result result = base.Validate();

            if (result.Success)
            {
                if (this.SelectedLabelType == OverlayLabelWidgetV3Type.Counter)
                {
                    if (this.SelectedCounter == null)
                    {
                        return new Result(Resources.OverlayLabelCounterNotSelected);
                    }
                }
            }

            return result;
        }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayLabelV3Model item = (OverlayLabelV3Model)this.GetItem();

            item.LabelType = this.SelectedLabelType;
            if (this.SelectedLabelType == OverlayLabelWidgetV3Type.Counter)
            {
                item.CounterName = this.SelectedCounter.Name;
            }

            return item;
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
