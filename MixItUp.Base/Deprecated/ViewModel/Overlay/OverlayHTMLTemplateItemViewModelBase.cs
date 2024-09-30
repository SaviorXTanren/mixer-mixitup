using MixItUp.Base.Model.Overlay;
using System;

namespace MixItUp.Base.ViewModel.Overlay
{
    [Obsolete]
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
