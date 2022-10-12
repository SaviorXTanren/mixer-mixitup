﻿using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayTimerItemV3ViewModel : OverlayVisualTextItemV3ViewModelBase
    {
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

        public OverlayTimerItemV3ViewModel()
            : base(OverlayItemV3Type.Timer)
        {
            this.DisplayFormat = OverlayTimerItemV3Model.DefaultDisplayFormat;
        }

        public OverlayTimerItemV3ViewModel(OverlayTimerItemV3Model item)
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
            OverlayTimerItemV3Model result = new OverlayTimerItemV3Model()
            {
                HTML = this.HTML,
                CSS = this.CSS,
                Javascript = this.Javascript,

                Text = this.Text,
                FontSize = this.FontSize,
                FontName = this.FontName,
                FontColor = this.FontColor,
                Bold = this.Bold,
                Italics = this.Italics,
                Underline = this.Underline,
                ShadowColor = this.ShadowColor,
                Width = this.width,

                DisplayFormat = this.DisplayFormat,
                CountUp = this.CountUp,
            };

            if (this.LeftAlignment)
            {
                result.TextAlignment = OverlayVisualTextItemV3AlignmentTypeEnum.Left;
            }
            else if (this.CenterAlignment)
            {
                result.TextAlignment = OverlayVisualTextItemV3AlignmentTypeEnum.Center;
            }
            else if (this.RightAlignment)
            {
                result.TextAlignment = OverlayVisualTextItemV3AlignmentTypeEnum.Right;
            }
            else if (this.JustifyAlignment)
            {
                result.TextAlignment = OverlayVisualTextItemV3AlignmentTypeEnum.Justify;
            }

            return result;
        }
    }
}