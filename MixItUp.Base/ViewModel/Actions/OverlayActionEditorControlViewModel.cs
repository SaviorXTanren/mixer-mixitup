using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Overlay;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public enum OverlayActionTypeEnum
    {
        Text,
        Image,
        Video,
        YouTube,
        HTML,
        Timer,
        TwitchClip,
    }

    public class OverlayActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.Overlay; } }

        public IEnumerable<OverlayActionTypeEnum> ActionTypes { get { return EnumHelper.GetEnumList<OverlayActionTypeEnum>(); } }

        public OverlayActionTypeEnum SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                if (this.selectedActionType != value)
                {
                    this.selectedActionType = value;
                    this.NotifyPropertyChanged();
                    this.NotifyPropertyChanged(nameof(this.OverlayNotEnabled));
                    this.NotifyPropertyChanged(nameof(this.OverlayEnabled));
                    this.NotifyPropertyChanged(nameof(this.ShowItem));

                    if (this.ShowItem)
                    {
                        if (this.SelectedActionType == OverlayActionTypeEnum.Text)
                        {
                            this.Item = new OverlayTextV3ViewModel();
                        }
                        else if (this.SelectedActionType == OverlayActionTypeEnum.Image)
                        {
                            this.Item = new OverlayImageV3ViewModel();
                        }
                        else if (this.SelectedActionType == OverlayActionTypeEnum.Video)
                        {
                            this.Item = new OverlayVideoV3ViewModel();
                        }
                        else if (this.SelectedActionType == OverlayActionTypeEnum.YouTube)
                        {
                            this.Item = new OverlayYouTubeV3ViewModel();
                        }
                        else if (this.SelectedActionType == OverlayActionTypeEnum.HTML)
                        {
                            this.Item = new OverlayHTMLV3ViewModel();
                        }
                        else if (this.SelectedActionType == OverlayActionTypeEnum.Timer)
                        {
                            this.Item = new OverlayTimerV3ViewModel();
                        }
                        else if (this.SelectedActionType == OverlayActionTypeEnum.TwitchClip)
                        {
                            this.Item = new OverlayTwitchClipV3ViewModel();
                        }

                        this.SetPositionWrappedHTML(this.Item.DefaultHTML);
                        this.SetPositionWrappedCSS(this.Item.DefaultCSS);
                        this.Javascript = this.Item.DefaultJavascript;
                    }
                }
            }
        }
        private OverlayActionTypeEnum selectedActionType;

        public bool OverlayNotEnabled { get { return !ServiceManager.Get<OverlayV3Service>().IsConnected; } }

        public bool OverlayEnabled { get { return !this.OverlayNotEnabled; } }

        public IEnumerable<OverlayEndpointV3Model> OverlayEndpoints { get { return ServiceManager.Get<OverlayV3Service>().GetOverlayEndpoints(); } }

        public OverlayEndpointV3Model SelectedOverlayEndpoint
        {
            get { return this.selectedOverlayEndpoint; }
            set
            {
                var overlays = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpoints();
                if (overlays.Contains(value))
                {
                    this.selectedOverlayEndpoint = value;
                }
                else
                {
                    this.selectedOverlayEndpoint = ServiceManager.Get<OverlayV3Service>().GetDefaultOverlayEndpoint();
                }
                this.NotifyPropertyChanged();
            }
        }
        private OverlayEndpointV3Model selectedOverlayEndpoint;

        public bool ShowItem
        {
            get
            {
                return this.SelectedActionType == OverlayActionTypeEnum.Text || this.SelectedActionType == OverlayActionTypeEnum.Image ||
                    this.SelectedActionType == OverlayActionTypeEnum.Video || this.SelectedActionType == OverlayActionTypeEnum.YouTube ||
                    this.SelectedActionType == OverlayActionTypeEnum.HTML || this.SelectedActionType == OverlayActionTypeEnum.Timer ||
                    this.SelectedActionType == OverlayActionTypeEnum.TwitchClip;
            }
        }

        public OverlayItemV3ViewModelBase Item
        {
            get { return this.item; }
            set
            {
                this.item = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayItemV3ViewModelBase item;

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

        public OverlayAnimationV3ViewModel EntranceAnimation
        {
            get { return this.entranceAnimation; }
            set
            {
                this.entranceAnimation = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayAnimationV3ViewModel entranceAnimation;

        public OverlayAnimationV3ViewModel ExitAnimation
        {
            get { return this.exitAnimation; }
            set
            {
                this.exitAnimation = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayAnimationV3ViewModel exitAnimation;

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

        public OverlayActionEditorControlViewModel(OverlayActionModel action)
            : base(action)
        {
            this.SelectedOverlayEndpoint = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpoint(action.OverlayEndpointID);
            if (this.SelectedOverlayEndpoint == null)
            {
                this.SelectedOverlayEndpoint = ServiceManager.Get<OverlayV3Service>().GetDefaultOverlayEndpoint();
            }

            if (action.OverlayItemV3 != null)
            {
                if (action.OverlayItemV3.Type == OverlayItemV3Type.Text)
                {
                    this.SelectedActionType = OverlayActionTypeEnum.Text;
                    this.Item = new OverlayTextV3ViewModel((OverlayTextV3Model)action.OverlayItemV3);
                }
                else if (action.OverlayItemV3.Type == OverlayItemV3Type.Image)
                {
                    this.SelectedActionType = OverlayActionTypeEnum.Image;
                    this.Item = new OverlayImageV3ViewModel((OverlayImageV3Model)action.OverlayItemV3);
                }
                else if (action.OverlayItemV3.Type == OverlayItemV3Type.Video)
                {
                    this.SelectedActionType = OverlayActionTypeEnum.Video;
                    this.Item = new OverlayVideoV3ViewModel((OverlayVideoV3Model)action.OverlayItemV3);
                }
                else if (action.OverlayItemV3.Type == OverlayItemV3Type.YouTube)
                {
                    this.SelectedActionType = OverlayActionTypeEnum.YouTube;
                    this.Item = new OverlayYouTubeV3ViewModel((OverlayYouTubeV3Model)action.OverlayItemV3);
                }
                else if (action.OverlayItemV3.Type == OverlayItemV3Type.HTML)
                {
                    this.SelectedActionType = OverlayActionTypeEnum.HTML;
                    this.Item = new OverlayHTMLV3ViewModel((OverlayHTMLV3Model)action.OverlayItemV3);
                }
                else if (action.OverlayItemV3.Type == OverlayItemV3Type.Timer)
                {
                    this.SelectedActionType = OverlayActionTypeEnum.Timer;
                    this.Item = new OverlayTimerV3ViewModel((OverlayTimerV3Model)action.OverlayItemV3);
                }
                else if (action.OverlayItemV3.Type == OverlayItemV3Type.TwitchClip)
                {
                    this.SelectedActionType = OverlayActionTypeEnum.TwitchClip;
                    this.Item = new OverlayTwitchClipV3ViewModel((OverlayTwitchClipV3Model)action.OverlayItemV3);
                }

                this.Position = new OverlayPositionV3ViewModel(action.OverlayItemV3.Position);
                this.Duration = action.Duration;
                this.EntranceAnimation = new OverlayAnimationV3ViewModel(Resources.Entrance, action.EntranceAnimation);
                this.ExitAnimation = new OverlayAnimationV3ViewModel(Resources.Exit, action.ExitAnimation);

                this.HTML = action.OverlayItemV3.HTML;
                this.CSS = action.OverlayItemV3.CSS;
                this.Javascript = action.OverlayItemV3.Javascript;
            }
        }

        public OverlayActionEditorControlViewModel()
            : base()
        {
            this.SelectedOverlayEndpoint = ServiceManager.Get<OverlayV3Service>().GetDefaultOverlayEndpoint();
            this.SelectedActionType = OverlayActionTypeEnum.Text;

            this.Item = new OverlayTextV3ViewModel();
            this.SetPositionWrappedHTML(this.Item.DefaultHTML);
            this.SetPositionWrappedCSS(this.Item.DefaultCSS);
            this.Javascript = this.Item.DefaultJavascript;

            this.EntranceAnimation = new OverlayAnimationV3ViewModel(Resources.Entrance);
            this.ExitAnimation = new OverlayAnimationV3ViewModel(Resources.Exit);
        }

        public void SetPositionWrappedHTML(string innerHTML)
        {
            if (!string.IsNullOrEmpty(innerHTML))
            {
                this.HTML = OverlayV3Service.ReplaceProperty(OverlayItemV3ModelBase.PositionedHTML, OverlayItemV3ModelBase.InnerHTMLProperty, innerHTML);
            }
        }

        public void SetPositionWrappedCSS(string innerCSS)
        {
            if (!string.IsNullOrEmpty(innerCSS))
            {
                this.CSS = OverlayItemV3ModelBase.PositionedCSS + Environment.NewLine + Environment.NewLine + innerCSS;
            }
        }

        public override Task<Result> Validate()
        {
            if (this.ShowItem)
            {
                Result result = this.Item.Validate();
                if (!result.Success)
                {
                    return Task.FromResult<Result>(new Result(Resources.OverlayActionValidationErrorHeader + result.Message));
                }

                if (string.IsNullOrWhiteSpace(this.Duration))
                {
                    if (this.SelectedActionType != OverlayActionTypeEnum.Video && this.SelectedActionType != OverlayActionTypeEnum.YouTube &&
                        this.SelectedActionType != OverlayActionTypeEnum.TwitchClip)
                    {
                        return Task.FromResult<Result>(new Result(Resources.OverlayActionDurationInvalid));
                    }
                }

                result = this.Position.Validate();
                if (!result.Success)
                {
                    return Task.FromResult<Result>(result);
                }
            }

            return Task.FromResult(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            if (this.ShowItem)
            {
                OverlayItemV3ModelBase item = this.Item.GetItem();
                if (item != null)
                {
                    item.HTML = this.HTML;
                    item.CSS = this.CSS;
                    item.Javascript = this.Javascript;
                    item.Position = this.Position.GetPosition();
                    return Task.FromResult<ActionModelBase>(new OverlayActionModel(this.SelectedOverlayEndpoint.ID, item, this.Duration, this.EntranceAnimation.GetAnimation(), this.ExitAnimation.GetAnimation()));
                }
            }
            return Task.FromResult<ActionModelBase>(null);
        }
    }
}
