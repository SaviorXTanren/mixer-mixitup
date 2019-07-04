using MixItUp.Base.Model.Overlay;

namespace MixItUp.Base.ViewModel.Controls.Overlay
{
    public abstract class OverlayHTMLTemplateItemViewModelBase : OverlayItemViewModelBase
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

        public OverlayHTMLTemplateItemViewModelBase() { }

        public OverlayHTMLTemplateItemViewModelBase(OverlayHTMLTemplateItemModelBase item)
            : this()
        {
            this.HTML = item.HTML;
        }
    }
}
