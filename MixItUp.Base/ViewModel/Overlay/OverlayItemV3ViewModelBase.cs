using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System.Collections.Generic;

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

        public string Duration
        {
            get { return this.duration; }
            set
            {
                this.duration = value;
                this.NotifyPropertyChanged();
            }
        }
        private string duration;

        public OverlayItemAnimationV3ViewModel EntranceAnimation
        {
            get { return this.entranceAnimation; }
            set
            {
                this.entranceAnimation = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayItemAnimationV3ViewModel entranceAnimation = new OverlayItemAnimationV3ViewModel(Resources.Entrance);

        public OverlayItemAnimationV3ViewModel VisibleAnimation
        {
            get { return this.visibleAnimation; }
            set
            {
                this.visibleAnimation = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayItemAnimationV3ViewModel visibleAnimation = new OverlayItemAnimationV3ViewModel(Resources.Visible);

        public OverlayItemAnimationV3ViewModel ExitAnimation
        {
            get { return this.exitAnimation; }
            set
            {
                this.exitAnimation = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayItemAnimationV3ViewModel exitAnimation = new OverlayItemAnimationV3ViewModel(Resources.Exit);

        public List<OverlayItemAnimationV3ViewModel> Animations { get; private set; } = new List<OverlayItemAnimationV3ViewModel>();

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
                case OverlayItemV3Type.Label:
                    this.HTML = OverlayLabelItemV3Model.DefaultNameHTML;
                    this.CSS = OverlayLabelItemV3Model.DefaultCSS;
                    this.Javascript = OverlayLabelItemV3Model.DefaultJavascript;
                    break;
            }

            if (!string.IsNullOrWhiteSpace(this.HTML))
            {
                this.HTML = OverlayItemV3ModelBase.ReplaceProperty(OverlayItemV3ModelBase.PositionedHTML, OverlayItemV3ModelBase.InnerHTMLProperty, this.HTML);
                this.CSS = OverlayItemV3ModelBase.PositionedCSS + this.CSS;
            }

            this.Animations.Add(this.EntranceAnimation);
            this.Animations.Add(this.VisibleAnimation);
            this.Animations.Add(this.ExitAnimation);

            this.Duration = "5";
        }

        public OverlayItemV3ViewModelBase(OverlayItemV3ModelBase item)
        {
            this.HTML = item.HTML;
            this.CSS = item.CSS;
            this.Javascript = item.Javascript;

            this.ItemPosition = new OverlayItemPositionV3ViewModel(item);
            this.Duration = item.Duration;
            this.EntranceAnimation = new OverlayItemAnimationV3ViewModel(Resources.Entrance, item.EntranceAnimation);
            this.VisibleAnimation = new OverlayItemAnimationV3ViewModel(Resources.Visible, item.VisibleAnimation);
            this.ExitAnimation = new OverlayItemAnimationV3ViewModel(Resources.Exit, item.ExitAnimation);

            this.Animations.Add(this.EntranceAnimation);
            this.Animations.Add(this.VisibleAnimation);
            this.Animations.Add(this.ExitAnimation);
        }

        public virtual Result Validate()
        {
            if (string.IsNullOrWhiteSpace(this.Duration))
            {
                return new Result(Resources.OverlayActionDurationInvalid);
            }
            return new Result();
        }

        public OverlayItemV3ModelBase GetItem()
        {
            OverlayItemV3ModelBase item = this.GetItemInternal();

            this.ItemPosition.SetPosition(item);
            item.Duration = this.Duration;
            this.EntranceAnimation.SetAnimation(item.EntranceAnimation);
            this.VisibleAnimation.SetAnimation(item.VisibleAnimation);
            this.ExitAnimation.SetAnimation(item.ExitAnimation);

            return item;
        }

        protected abstract OverlayItemV3ModelBase GetItemInternal();
    }
}
