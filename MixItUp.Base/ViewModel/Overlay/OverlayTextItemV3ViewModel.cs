using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayTextItemV3ViewModel : OverlayVisualTextItemV3ViewModelBase
    {
        public OverlayTextItemV3ViewModel() : base(OverlayItemV3Type.Text) { }

        public OverlayTextItemV3ViewModel(OverlayTextItemV3Model item) : base(item) { }

        public override Result Validate()
        {
            if (string.IsNullOrWhiteSpace(this.Text))
            {
                return new Result(Resources.OverlayTextMissingText);
            }

            return new Result();
        }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            OverlayTextItemV3Model result = new OverlayTextItemV3Model()
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
