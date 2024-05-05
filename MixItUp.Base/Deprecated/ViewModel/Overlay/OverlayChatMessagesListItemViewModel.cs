using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using System;

namespace MixItUp.Base.ViewModel.Overlay
{
    [Obsolete]
    public class OverlayChatMessagesListItemViewModel : OverlayListItemViewModelBase
    {
        public OverlayChatMessagesListItemViewModel()
            : base()
        {
            this.HTML = OverlayChatMessagesListItemModel.HTMLTemplate;
            this.height = 24;
        }

        public OverlayChatMessagesListItemViewModel(OverlayChatMessagesListItemModel item)
            : base(item.TotalToShow, item.FadeOut, item.Width, item.Height, item.TextFont, item.TextColor, item.BorderColor, item.BackgroundColor, item.Alignment, item.Effects.EntranceAnimation, item.Effects.ExitAnimation, item.HTML)
        { }

        public override OverlayItemModelBase GetOverlayItem()
        {
            if (this.Validate())
            {
                //this.TextColor = ColorSchemes.GetColorCode(this.TextColor);
                //this.BorderColor = ColorSchemes.GetColorCode(this.BorderColor);
                //this.BackgroundColor = ColorSchemes.GetColorCode(this.BackgroundColor);

                return new OverlayChatMessagesListItemModel(this.HTML, totalToShow, fadeOut, this.Font, this.width, this.height, this.BorderColor, this.BackgroundColor, this.TextColor, this.alignment, this.entranceAnimation, this.exitAnimation);
            }
            return null;
        }
    }
}
