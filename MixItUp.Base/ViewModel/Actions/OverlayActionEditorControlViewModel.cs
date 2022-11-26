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
        WebPage,
        Timer,
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
                this.selectedActionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(OverlayNotEnabled));
                this.NotifyPropertyChanged(nameof(OverlayEnabled));
                this.NotifyPropertyChanged(nameof(ShowTextItem));
                this.NotifyPropertyChanged(nameof(ShowImageItem));
                this.NotifyPropertyChanged(nameof(ShowVideoItem));
                this.NotifyPropertyChanged(nameof(ShowYouTubeItem));
                this.NotifyPropertyChanged(nameof(ShowHTMLItem));
                this.NotifyPropertyChanged(nameof(ShowWebPageItem));
                this.NotifyPropertyChanged(nameof(ShowTimerItem));
            }
        }
        private OverlayActionTypeEnum selectedActionType;

        public bool OverlayNotEnabled { get { return !ServiceManager.Get<OverlayService>().IsConnected; } }

        public bool OverlayEnabled { get { return !this.OverlayNotEnabled; } }

        public IEnumerable<OverlayEndpointV3Model> OverlayEndpoints { get { return ServiceManager.Get<OverlayService>().GetOverlayEndpoints(); } }

        public OverlayEndpointV3Model SelectedOverlayEndpoint
        {
            get { return this.selectedOverlayEndpoint; }
            set
            {
                var overlays = ServiceManager.Get<OverlayService>().GetOverlayEndpoints();
                if (overlays.Contains(value))
                {
                    this.selectedOverlayEndpoint = value;
                }
                else
                {
                    this.selectedOverlayEndpoint = ServiceManager.Get<OverlayService>().GetDefaultOverlayEndpoint();
                }
                this.NotifyPropertyChanged();
            }
        }
        private OverlayEndpointV3Model selectedOverlayEndpoint;

        public bool ShowTextItem { get { return this.SelectedActionType == OverlayActionTypeEnum.Text; } }

        public OverlayTextV3ViewModel TextItemViewModel
        {
            get { return this.textItemViewModel; }
            set
            {
                this.textItemViewModel = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayTextV3ViewModel textItemViewModel = new OverlayTextV3ViewModel(addDefaultAnimation: true);

        public bool ShowImageItem { get { return this.SelectedActionType == OverlayActionTypeEnum.Image; } }

        public OverlayImageV3ViewModel ImageItemViewModel
        {
            get { return this.imageItemViewModel; }
            set
            {
                this.imageItemViewModel = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayImageV3ViewModel imageItemViewModel = new OverlayImageV3ViewModel(addDefaultAnimation: true);

        public bool ShowVideoItem { get { return this.SelectedActionType == OverlayActionTypeEnum.Video; } }

        public OverlayVideoV3ViewModel VideoItemViewModel
        {
            get { return this.videoItemViewModel; }
            set
            {
                this.videoItemViewModel = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayVideoV3ViewModel videoItemViewModel = new OverlayVideoV3ViewModel(addDefaultAnimation: true);

        public bool ShowYouTubeItem { get { return this.SelectedActionType == OverlayActionTypeEnum.YouTube; } }

        public OverlayYouTubeV3ViewModel YouTubeItemViewModel
        {
            get { return this.youTubeItemViewModel; }
            set
            {
                this.youTubeItemViewModel = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayYouTubeV3ViewModel youTubeItemViewModel = new OverlayYouTubeV3ViewModel(addDefaultAnimation: true);

        public bool ShowHTMLItem { get { return this.SelectedActionType == OverlayActionTypeEnum.HTML; } }

        public OverlayHTMLV3ViewModel HTMLItemViewModel
        {
            get { return this.htmlItemViewModel; }
            set
            {
                this.htmlItemViewModel = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayHTMLV3ViewModel htmlItemViewModel = new OverlayHTMLV3ViewModel(addDefaultAnimation: true);

        public bool ShowWebPageItem { get { return this.SelectedActionType == OverlayActionTypeEnum.WebPage; } }

        public OverlayWebPageV3ViewModel WebPageItemViewModel
        {
            get { return this.webPageItemViewModel; }
            set
            {
                this.webPageItemViewModel = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayWebPageV3ViewModel webPageItemViewModel = new OverlayWebPageV3ViewModel(addDefaultAnimation: true);

        public bool ShowTimerItem { get { return this.SelectedActionType == OverlayActionTypeEnum.Timer; } }

        public OverlayTimerV3ViewModel TimerItemViewModel
        {
            get { return this.timerItemViewModel; }
            set
            {
                this.timerItemViewModel = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayTimerV3ViewModel timerItemViewModel = new OverlayTimerV3ViewModel(addDefaultAnimation: true);

        public OverlayActionEditorControlViewModel(OverlayActionModel action)
            : base(action)
        {
            this.SelectedOverlayEndpoint = ServiceManager.Get<OverlayService>().GetOverlayEndpoint(action.OverlayEndpointID);
            if (this.SelectedOverlayEndpoint == null)
            {
                this.SelectedOverlayEndpoint = ServiceManager.Get<OverlayService>().GetDefaultOverlayEndpoint();
            }

            if (action.OverlayItemV3 != null)
            {
                if (action.OverlayItemV3.Type == OverlayItemV3Type.Text)
                {
                    this.SelectedActionType = OverlayActionTypeEnum.Text;
                    this.TextItemViewModel = new OverlayTextV3ViewModel((OverlayTextV3Model)action.OverlayItemV3);
                }
                else if (action.OverlayItemV3.Type == OverlayItemV3Type.Image)
                {
                    this.SelectedActionType = OverlayActionTypeEnum.Image;
                    this.ImageItemViewModel = new OverlayImageV3ViewModel((OverlayImageV3Model)action.OverlayItemV3);
                }
                else if (action.OverlayItemV3.Type == OverlayItemV3Type.Video)
                {
                    this.SelectedActionType = OverlayActionTypeEnum.Video;
                    this.VideoItemViewModel = new OverlayVideoV3ViewModel((OverlayVideoV3Model)action.OverlayItemV3);
                }
                else if (action.OverlayItemV3.Type == OverlayItemV3Type.YouTube)
                {
                    this.SelectedActionType = OverlayActionTypeEnum.YouTube;
                    this.YouTubeItemViewModel = new OverlayYouTubeV3ViewModel((OverlayYouTubeV3Model)action.OverlayItemV3);
                }
                else if (action.OverlayItemV3.Type == OverlayItemV3Type.HTML)
                {
                    this.SelectedActionType = OverlayActionTypeEnum.HTML;
                    this.HTMLItemViewModel = new OverlayHTMLV3ViewModel((OverlayHTMLV3Model)action.OverlayItemV3);
                }
                else if (action.OverlayItemV3.Type == OverlayItemV3Type.WebPage)
                {
                    this.SelectedActionType = OverlayActionTypeEnum.WebPage;
                    this.WebPageItemViewModel = new OverlayWebPageV3ViewModel((OverlayWebPageV3Model)action.OverlayItemV3);
                }
                else if (action.OverlayItemV3.Type == OverlayItemV3Type.Timer)
                {
                    this.SelectedActionType = OverlayActionTypeEnum.Timer;
                    this.TimerItemViewModel = new OverlayTimerV3ViewModel((OverlayTimerV3Model)action.OverlayItemV3);
                }
            }
        }

        public OverlayActionEditorControlViewModel()
            : base()
        {
            this.SelectedOverlayEndpoint = ServiceManager.Get<OverlayService>().GetDefaultOverlayEndpoint();
        }

        public override Task<Result> Validate()
        {
            if (false)
            {

            }
            else
            {
                OverlayItemV3ViewModelBase itemViewModel = this.GetItemViewModel();
                if (itemViewModel != null)
                {
                    Result result = itemViewModel.Validate();
                    if (!result.Success)
                    {
                        return Task.FromResult<Result>(new Result(Resources.OverlayActionValidationErrorHeader + result.Message));
                    }

                    result = itemViewModel.Position.Validate();
                    if (!result.Success)
                    {
                        return Task.FromResult<Result>(result);
                    }
                }
            }

            return Task.FromResult(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            if (false)
            {

            }
            else
            {
                OverlayItemV3ViewModelBase itemViewModel = this.GetItemViewModel();
                if (itemViewModel != null)
                {
                    OverlayItemV3ModelBase item = itemViewModel.GetItem();
                    if (item != null)
                    {
                        return Task.FromResult<ActionModelBase>(new OverlayActionModel(this.SelectedOverlayEndpoint.ID, item));
                    }
                }
            }
            return Task.FromResult<ActionModelBase>(null);
        }

        private OverlayItemV3ViewModelBase GetItemViewModel()
        {
            if (this.SelectedActionType == OverlayActionTypeEnum.Text)
            {
                return this.TextItemViewModel;
            }
            else if (this.SelectedActionType == OverlayActionTypeEnum.Image)
            {
                return this.ImageItemViewModel;
            }
            else if (this.SelectedActionType == OverlayActionTypeEnum.Video)
            {
                return this.VideoItemViewModel;
            }
            else if (this.SelectedActionType == OverlayActionTypeEnum.YouTube)
            {
                return this.YouTubeItemViewModel;
            }
            else if (this.SelectedActionType == OverlayActionTypeEnum.HTML)
            {
                return this.HTMLItemViewModel;
            }
            else if (this.SelectedActionType == OverlayActionTypeEnum.WebPage)
            {
                return this.WebPageItemViewModel;
            }
            else if (this.SelectedActionType == OverlayActionTypeEnum.Timer)
            {
                return this.TimerItemViewModel;
            }
            return null;
        }
    }
}
