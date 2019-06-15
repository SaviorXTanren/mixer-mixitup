using MixItUp.Base.Model.Overlay;

namespace MixItUp.Base.ViewModel.Controls.Overlay
{
    public class OverlayGameQueueItemViewModel : OverlayListItemViewModelBase
    {
        public OverlayGameQueueItemViewModel()
            : base()
        {
            this.HTML = OverlayGameQueue.HTMLTemplate;
        }

        public OverlayGameQueueItemViewModel(OverlayGameQueue item)
            : base(item.TotalToShow, item.Width, item.Height, item.TextFont, item.TextColor, item.BorderColor, item.BackgroundColor, item.AddEventAnimation, item.RemoveEventAnimation, item.HTMLText)
        { }

        public override OverlayItemBase GetItem()
        {
            if (this.Validate())
            {
                return new OverlayGameQueue(this.HTML, totalToShow, this.Font, this.width, this.height, this.BorderColor, this.BackgroundColor, this.TextColor, this.entranceAnimation, this.exitAnimation);
            }
            return null;
        }
    }
}
