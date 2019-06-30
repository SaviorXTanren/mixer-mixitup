using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;

namespace MixItUp.Base.ViewModel.Controls.Overlay
{
    public class OverlayChatMessagesItemViewModel : OverlayListItemViewModelBase
    {
        public OverlayChatMessagesItemViewModel()
            : base()
        {
            this.HTML = OverlayChatMessagesListItemModel.HTMLTemplate;
            this.height = 24;
        }

        public OverlayChatMessagesItemViewModel(OverlayChatMessages item)
            : base(item.TotalToShow, item.Width, 1, item.TextFont, item.TextColor, item.BorderColor, item.BackgroundColor, OverlayItemEffectEntranceAnimationTypeEnum.None, OverlayItemEffectExitAnimationTypeEnum.None, item.HTMLText)
        { }

        public OverlayChatMessagesItemViewModel(OverlayChatMessagesListItemModel item)
            : base(item.TotalToShow, item.Width, item.Height, item.TextFont, item.TextColor, item.BorderColor, item.BackgroundColor, item.Effects.EntranceAnimation, item.Effects.ExitAnimation, item.HTML)
        { }

        public override OverlayItemBase GetItem()
        {
            if (this.Validate())
            {
                this.TextColor = ColorSchemes.GetColorCode(this.TextColor);
                this.BorderColor = ColorSchemes.GetColorCode(this.BorderColor);
                this.BackgroundColor = ColorSchemes.GetColorCode(this.BackgroundColor);

                return new OverlayChatMessages(this.HTML, totalToShow, this.width, this.BorderColor, this.BackgroundColor, this.TextColor, this.Font, 0, OverlayEffectEntranceAnimationTypeEnum.None);
            }
            return null;
        }

        public override OverlayItemModelBase GetOverlayItem()
        {
            if (this.Validate())
            {
                this.TextColor = ColorSchemes.GetColorCode(this.TextColor);
                this.BorderColor = ColorSchemes.GetColorCode(this.BorderColor);
                this.BackgroundColor = ColorSchemes.GetColorCode(this.BackgroundColor);

                return new OverlayChatMessagesListItemModel(this.HTML, totalToShow, this.Font, this.width, this.height, this.BorderColor, this.BackgroundColor, this.TextColor, this.entranceAnimation, this.exitAnimation);
            }
            return null;
        }
    }
}
