using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;

namespace MixItUp.Base.ViewModel.Overlay
{
    public abstract class OverlayItemV3ViewModelBase : UIViewModelBase
    {
        public OverlayItemV3Type Type
        {
            get { return this.type; }
            set
            {
                this.type = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayItemV3Type type;

        public OverlayPositionV3ViewModel Position
        {
            get { return this.position; }
            set
            {
                this.position = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayPositionV3ViewModel position = new OverlayPositionV3ViewModel();

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

        public List<OverlayAnimationV3ViewModel> Animations { get; private set; } = new List<OverlayAnimationV3ViewModel>();

        public bool HasAnimations { get { return this.Animations.Count > 0; } }

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
            this.Type = type;
            switch (this.Type)
            {
                case OverlayItemV3Type.Text:
                    this.HTML = OverlayTextV3Model.DefaultHTML;
                    this.CSS = OverlayTextV3Model.DefaultCSS;
                    this.Javascript = OverlayTextV3Model.DefaultJavascript;
                    break;
                case OverlayItemV3Type.Image:
                    this.HTML = OverlayImageV3Model.DefaultHTML;
                    this.CSS = OverlayImageV3Model.DefaultCSS;
                    this.Javascript = OverlayImageV3Model.DefaultJavascript;
                    break;
                case OverlayItemV3Type.Video:
                    this.HTML = OverlayVideoV3Model.DefaultHTML;
                    this.CSS = OverlayVideoV3Model.DefaultCSS;
                    this.Javascript = OverlayVideoV3Model.DefaultJavascript;
                    break;
                case OverlayItemV3Type.YouTube:
                    this.HTML = OverlayYouTubeV3Model.DefaultHTML;
                    this.CSS = OverlayYouTubeV3Model.DefaultCSS;
                    this.Javascript = OverlayYouTubeV3Model.DefaultJavascript;
                    break;
                case OverlayItemV3Type.HTML:
                    this.HTML = OverlayHTMLV3Model.DefaultHTML;
                    this.CSS = OverlayHTMLV3Model.DefaultCSS;
                    this.Javascript = OverlayHTMLV3Model.DefaultJavascript;
                    break;
                case OverlayItemV3Type.Timer:
                    this.HTML = OverlayTimerV3Model.DefaultHTML;
                    this.CSS = OverlayTimerV3Model.DefaultCSS;
                    this.Javascript = OverlayTimerV3Model.DefaultJavascript;
                    break;
                case OverlayItemV3Type.Label:
                    this.HTML = OverlayLabelV3Model.DefaultNameHTML;
                    this.CSS = OverlayLabelV3Model.DefaultCSS;
                    this.Javascript = OverlayLabelV3Model.DefaultJavascript;
                    break;
                case OverlayItemV3Type.EventList:
                    this.HTML = OverlayEventListV3Model.DefaultHTML;
                    this.CSS = OverlayEventListV3Model.DefaultCSS;
                    this.Javascript = OverlayEventListV3Model.DefaultJavascript;
                    break;
                case OverlayItemV3Type.Goal:
                    this.HTML = OverlayGoalV3Model.DefaultHTML;
                    this.CSS = OverlayGoalV3Model.DefaultCSS;
                    this.Javascript = OverlayGoalV3Model.DefaultJavascript;
                    break;
            }

            this.SetPositionWrappedHTML(this.HTML);
            this.SetPositionWrappedCSS(this.CSS);

            this.Duration = "5";
        }

        public void SetPositionWrappedHTML(string innerHTML)
        {
            if (!string.IsNullOrEmpty(innerHTML))
            {
                this.HTML = OverlayItemV3ModelBase.ReplaceProperty(OverlayItemV3ModelBase.PositionedHTML, OverlayItemV3ModelBase.InnerHTMLProperty, innerHTML);
            }
        }

        public void SetPositionWrappedCSS(string innerCSS)
        {
            if (!string.IsNullOrEmpty(innerCSS))
            {
                this.CSS = OverlayItemV3ModelBase.PositionedCSS + Environment.NewLine + Environment.NewLine + innerCSS;
            }
        }

        public OverlayItemV3ViewModelBase(OverlayItemV3ModelBase item)
        {
            this.HTML = item.HTML;
            this.CSS = item.CSS;
            this.Javascript = item.Javascript;

            this.Position = new OverlayPositionV3ViewModel(item);
            this.Duration = item.Duration;

            foreach (var animation in item.Animations)
            {
                this.Animations.Add(new OverlayAnimationV3ViewModel(animation));
            }
            this.NotifyPropertyChanged(nameof(this.HasAnimations));
        }

        public void AddOverlayActionAnimations()
        {
            this.AddAnimations(new List<string>() { Resources.Entrance, Resources.Visible, Resources.Exit });
        }

        public void AddEntranceExitAnimations()
        {
            this.AddAnimations(new List<string>() { Resources.Entrance, Resources.Exit });
        }

        public void AddAnimations(IEnumerable<string> animationNames)
        {
            foreach (string animationName in animationNames)
            {
                this.Animations.Add(new OverlayAnimationV3ViewModel(animationName));
            }
            this.NotifyPropertyChanged(nameof(this.HasAnimations));
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

            this.Position.SetPosition(item);
            item.Duration = this.Duration;

            foreach (var animation in this.Animations)
            {
                item.Animations.Add(animation.GetAnimation());
            }

            return item;
        }

        protected abstract OverlayItemV3ModelBase GetItemInternal();
    }
}
