using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;

namespace MixItUp.Base.ViewModel.Overlay
{
    [Obsolete]
    public class OverlayTickerTapeListItemViewModel : OverlayListItemViewModelBase
    {
        public IEnumerable<OverlayTickerTapeItemTypeEnum> TickerTapeTypes { get; set; } = EnumHelper.GetEnumList<OverlayTickerTapeItemTypeEnum>();
        public OverlayTickerTapeItemTypeEnum TickerTapeType
        {
            get { return this.tickerTapeType; }
            set
            {
                this.tickerTapeType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ShowMinimumAmountRequiredToShow");
                if (this.tickerTapeType == OverlayTickerTapeItemTypeEnum.Donations)
                {
                    this.minimumAmountRequiredToShow = 1.0;
                }
                else
                {
                    this.minimumAmountRequiredToShow = 0;
                }
                this.NotifyPropertyChanged("MinimumAmountRequiredToShowString");
            }
        }
        private OverlayTickerTapeItemTypeEnum tickerTapeType;

        public string MinimumAmountRequiredToShowString
        {
            get { return this.minimumAmountRequiredToShow.ToString(); }
            set
            {
                this.minimumAmountRequiredToShow = this.GetPositiveDoubleFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        protected double minimumAmountRequiredToShow;

        public bool ShowMinimumAmountRequiredToShow
        {
            get
            {
                return this.tickerTapeType == OverlayTickerTapeItemTypeEnum.Donations;
            }
        }

        public OverlayTickerTapeListItemViewModel()
            : base()
        {
            this.width = 1920;
            this.height = 40;

            //this.BackgroundColor = ColorSchemes.HTMLColorSchemeDictionary.First().Key;
            //this.BorderColor = ColorSchemes.HTMLColorSchemeDictionary.First().Key;

            this.HTML = OverlayTickerTapeListItemModel.HTMLTemplate;
        }

        public OverlayTickerTapeListItemViewModel(OverlayTickerTapeListItemModel item)
            : base(item.TotalToShow, item.FadeOut, item.Width, item.Height, item.TextFont, item.TextColor, item.BorderColor, item.BackgroundColor, OverlayListItemAlignmentTypeEnum.None, item.Effects.EntranceAnimation, item.Effects.ExitAnimation, item.HTML)
        {
            this.tickerTapeType = item.TickerTapeType;
            this.minimumAmountRequiredToShow = item.MinimumAmountRequiredToShow;

            //this.BackgroundColor = ColorSchemes.HTMLColorSchemeDictionary.First().Key;
            //this.BorderColor = ColorSchemes.HTMLColorSchemeDictionary.First().Key;
        }

        public override OverlayItemModelBase GetOverlayItem()
        {
            if (this.Validate() && this.minimumAmountRequiredToShow >= 0)
            {
                //this.TextColor = ColorSchemes.GetColorCode(this.TextColor);

                if (!this.ShowMinimumAmountRequiredToShow)
                {
                    this.minimumAmountRequiredToShow = 0;
                }

                return new OverlayTickerTapeListItemModel(this.HTML, totalToShow, this.tickerTapeType, this.minimumAmountRequiredToShow, this.TextColor, this.Font, this.width, this.height);
            }
            return null;
        }
    }
}
