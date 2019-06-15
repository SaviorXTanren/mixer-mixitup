using MixItUp.Base.Model.Overlay;

namespace MixItUp.Base.ViewModel.Controls.Overlay
{
    public abstract class OverlayCustomHTMLItemViewModelBase : OverlayItemViewModelBase
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

        public OverlayCustomHTMLItemViewModelBase() { }

        public OverlayCustomHTMLItemViewModelBase(OverlayCustomHTMLItem item)
            : this()
        {
            this.HTML = item.HTMLText;
        }
    }
}
