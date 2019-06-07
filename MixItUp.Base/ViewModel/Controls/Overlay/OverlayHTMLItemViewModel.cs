using MixItUp.Base.Model.Overlay;

namespace MixItUp.Base.ViewModel.Controls.Overlay
{
    public class OverlayHTMLItemViewModel : OverlayItemViewModelBase
    {
        public string HTML
        {
            get { return this.html; }
            set
            {
                this.html = value;
                this.NotifyPropertyChanged();
            }
        }
        private string html;

        public OverlayHTMLItemViewModel() { }

        public OverlayHTMLItemViewModel(OverlayHTMLItem item)
            : this()
        {
            this.HTML = item.HTMLText;
        }

        public override OverlayItemBase GetItem()
        {
            if (!string.IsNullOrEmpty(this.HTML))
            {
                return new OverlayHTMLItem(this.HTML);
            }
            return null;
        }
    }
}
