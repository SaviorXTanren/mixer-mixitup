using MixItUp.Base.Model.Overlay;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayTimerBasicItemV3ViewModel : OverlayVisualTextItemV3ViewModelBase
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

        public OverlayTimerBasicItemV3ViewModel()
            : base(OverlayItemV3Type.Timer)
        {
            this.DisplayFormat = OverlayTimerBasicItemV3Model.DefaultDisplayFormat;
        }

        public OverlayTimerBasicItemV3ViewModel(OverlayTimerBasicItemV3Model item) : base(item) { }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayTimerBasicItemV3Model result = new OverlayTimerBasicItemV3Model()
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
