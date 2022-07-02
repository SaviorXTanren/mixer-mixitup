using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModels;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayItemV3ViewModelBase : UIViewModelBase
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

        public string CSS
        {
            get { return this.css; }
            set
            {
                this.css = value;
                this.NotifyPropertyChanged();
            }
        }
        private string css;

        public string Javascript
        {
            get { return this.javascript; }
            set
            {
                this.javascript = value;
                this.NotifyPropertyChanged();
            }
        }
        private string javascript;

        public OverlayItemV3ViewModelBase(OverlayItemV3Type type)
        {
            switch (type)
            {
                case OverlayItemV3Type.Text: this.HTML = OverlayTextItemV3Model.DefaultHTML; break;
                case OverlayItemV3Type.Image: this.HTML = OverlayImageItemV3Model.DefaultHTML; break;
                case OverlayItemV3Type.Video: this.HTML = OverlayVideoItemV3Model.DefaultHTML; break;
                case OverlayItemV3Type.YouTube: this.HTML = OverlayYouTubeItemV3Model.DefaultHTML; break;
                case OverlayItemV3Type.HTML: this.HTML = OverlayHTMLItemV3Model.DefaultHTML; break;
                case OverlayItemV3Type.WebPage: this.HTML = OverlayWebPageItemV3Model.DefaultHTML; break;
            }

            if (!string.IsNullOrEmpty(this.HTML))
            {
                this.HTML = OverlayItemV3ModelBase.ReplaceProperty(OverlayItemV3ModelBase.PositionedHTML, OverlayItemV3ModelBase.InnerHTMLProperty, this.HTML);
            }
        }

        public OverlayItemV3ViewModelBase(OverlayItemV3ModelBase item)
        {
            this.HTML = item.HTML;
            this.CSS = item.CSS;
            this.Javascript = item.Javascript;
        }
    }
}
