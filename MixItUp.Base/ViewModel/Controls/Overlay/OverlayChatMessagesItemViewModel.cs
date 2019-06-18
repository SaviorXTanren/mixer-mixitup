using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;

namespace MixItUp.Base.ViewModel.Controls.Overlay
{
    public class OverlayChatMessagesItemViewModel : OverlayListItemViewModelBase
    {
        public string TextHeightString
        {
            get { return this.textHeight.ToString(); }
            set
            {
                this.textHeight = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        protected int textHeight;

        public OverlayChatMessagesItemViewModel()
            : base()
        {
            this.HTML = OverlayChatMessages.HTMLTemplate;
        }

        public OverlayChatMessagesItemViewModel(OverlayChatMessages item)
            : base(item.TotalToShow, item.Width, 1, item.TextFont, item.TextColor, item.BorderColor, item.BackgroundColor, item.AddEventAnimation, OverlayEffectExitAnimationTypeEnum.None, item.HTMLText)
        {
            this.textHeight = item.TextSize;
        }

        public override OverlayItemBase GetItem()
        {
            if (this.Validate() && this.textHeight > 0)
            {
                this.TextColor = ColorSchemes.GetColorCode(this.TextColor);
                this.BorderColor = ColorSchemes.GetColorCode(this.BorderColor);
                this.BackgroundColor = ColorSchemes.GetColorCode(this.BackgroundColor);

                return new OverlayChatMessages(this.HTML, totalToShow, this.width, this.BorderColor, this.BackgroundColor, this.TextColor, this.Font, this.textHeight, this.entranceAnimation);
            }
            return null;
        }
    }
}
