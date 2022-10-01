using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;

namespace MixItUp.Base.ViewModel.Overlay
{
    public abstract class OverlayItemV3ViewModelBase : UIViewModelBase
    {
        public OverlayItemPositionV3ViewModel ItemPosition
        {
            get { return this.itemPosition; }
            set
            {
                this.itemPosition = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayItemPositionV3ViewModel itemPosition = new OverlayItemPositionV3ViewModel();

        public OverlayItemAnimationV3ViewModel EntranceAnimation
        {
            get { return this.entranceAnimation; }
            set
            {
                this.entranceAnimation = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayItemAnimationV3ViewModel entranceAnimation = new OverlayItemAnimationV3ViewModel();

        public OverlayItemAnimationV3ViewModel VisibleAnimation
        {
            get { return this.visibleAnimation; }
            set
            {
                this.visibleAnimation = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayItemAnimationV3ViewModel visibleAnimation = new OverlayItemAnimationV3ViewModel();

        public OverlayItemAnimationV3ViewModel ExitAnimation
        {
            get { return this.exitAnimation; }
            set
            {
                this.exitAnimation = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayItemAnimationV3ViewModel exitAnimation = new OverlayItemAnimationV3ViewModel();

        public string ItemDuration
        {
            get { return this.itemDuration; }
            set
            {
                this.itemDuration = value;
                this.NotifyPropertyChanged();
            }
        }
        private string itemDuration;

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
                case OverlayItemV3Type.Text:
                    this.HTML = OverlayTextItemV3Model.DefaultHTML;
                    this.CSS = OverlayTextItemV3Model.DefaultCSS;
                    this.Javascript = OverlayTextItemV3Model.DefaultJavascript;
                    break;
                case OverlayItemV3Type.Image:
                    this.HTML = OverlayImageItemV3Model.DefaultHTML;
                    this.CSS = OverlayImageItemV3Model.DefaultCSS;
                    this.Javascript = OverlayImageItemV3Model.DefaultJavascript;
                    break;
                case OverlayItemV3Type.Video:
                    this.HTML = OverlayVideoItemV3Model.DefaultHTML;
                    this.CSS = OverlayVideoItemV3Model.DefaultCSS;
                    this.Javascript = OverlayVideoItemV3Model.DefaultJavascript;
                    break;
                case OverlayItemV3Type.YouTube:
                    this.HTML = OverlayYouTubeItemV3Model.DefaultHTML;
                    this.CSS = OverlayYouTubeItemV3Model.DefaultCSS;
                    this.Javascript = OverlayYouTubeItemV3Model.DefaultJavascript;
                    break;
                case OverlayItemV3Type.HTML:
                    this.HTML = OverlayHTMLItemV3Model.DefaultHTML;
                    this.CSS = OverlayHTMLItemV3Model.DefaultCSS;
                    this.Javascript = OverlayHTMLItemV3Model.DefaultJavascript;
                    break;
                case OverlayItemV3Type.WebPage:
                    this.HTML = OverlayWebPageItemV3Model.DefaultHTML;
                    this.CSS = OverlayWebPageItemV3Model.DefaultCSS;
                    this.Javascript = OverlayWebPageItemV3Model.DefaultJavascript;
                    break;
                case OverlayItemV3Type.Timer:
                    this.HTML = OverlayTimerItemV3Model.DefaultHTML;
                    this.CSS = OverlayTimerItemV3Model.DefaultCSS;
                    this.Javascript = OverlayTimerItemV3Model.DefaultJavascript;
                    break;
            }

            if (!string.IsNullOrWhiteSpace(this.HTML))
            {
                this.HTML = OverlayItemV3ModelBase.ReplaceProperty(OverlayItemV3ModelBase.PositionedHTML, OverlayItemV3ModelBase.InnerHTMLProperty, this.HTML);
                this.CSS = OverlayItemV3ModelBase.PositionedCSS + this.CSS;
            }
        }

        public OverlayItemV3ViewModelBase(OverlayItemV3ModelBase item)
        {
            this.HTML = item.HTML;
            this.CSS = item.CSS;
            this.Javascript = item.Javascript;

            this.ItemPosition = new OverlayItemPositionV3ViewModel(item);

            this.EntranceAnimation = new OverlayItemAnimationV3ViewModel(item.EntranceAnimation);
            this.VisibleAnimation = new OverlayItemAnimationV3ViewModel(item.VisibleAnimation);
            this.ExitAnimation = new OverlayItemAnimationV3ViewModel(item.ExitAnimation);
        }

        public virtual Result Validate() { return new Result(); }

        public OverlayItemV3ModelBase GetItem()
        {
            OverlayItemV3ModelBase item = this.GetItemInternal();

            this.ItemPosition.SetPosition(item);

            this.EntranceAnimation.SetAnimation(item.EntranceAnimation);
            this.VisibleAnimation.SetAnimation(item.VisibleAnimation);
            this.ExitAnimation.SetAnimation(item.ExitAnimation);

            return item;
        }

        protected abstract OverlayItemV3ModelBase GetItemInternal();
    }
}
