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
        WebPage,
        HTML,
        ShowHideWidget,
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
                this.NotifyPropertyChanged("ShowShowHideWidgetGrid");
                this.NotifyPropertyChanged("ShowItemGrid");
                this.NotifyPropertyChanged("ShowTextItem");
                this.NotifyPropertyChanged("ShowImageItem");
                this.NotifyPropertyChanged("ShowVideoItem");
                this.NotifyPropertyChanged("ShowYouTubeItem");
                this.NotifyPropertyChanged("ShowWebPageItem");
                this.NotifyPropertyChanged("ShowHTMLItem");
                this.NotifyPropertyChanged("OverlayNotEnabled");
                this.NotifyPropertyChanged("OverlayEnabled");
            }
        }
        private OverlayActionTypeEnum selectedActionType;

        public bool OverlayNotEnabled { get { return !ServiceManager.Get<OverlayService>().IsConnected; } }

        public bool OverlayEnabled { get { return !this.OverlayNotEnabled &&  SelectedActionType != OverlayActionTypeEnum.ShowHideWidget; } }

        public IEnumerable<string> OverlayEndpoints { get { return ServiceManager.Get<OverlayService>().GetOverlayNames(); } }

        public string SelectedOverlayEndpoint
        {
            get { return this.selectedOverlayEndpoint; }
            set
            {
                var overlays = ServiceManager.Get<OverlayService>().GetOverlayNames();
                if (overlays.Contains(value))
                {
                    this.selectedOverlayEndpoint = value;
                }
                else
                {
                    this.selectedOverlayEndpoint = ServiceManager.Get<OverlayService>().DefaultOverlayName;
                }
                this.NotifyPropertyChanged();
            }
        }
        private string selectedOverlayEndpoint;

        public bool ShowShowHideWidgetGrid { get { return this.SelectedActionType == OverlayActionTypeEnum.ShowHideWidget; } }

        public IEnumerable<OverlayWidgetModel> Widgets { get { return ChannelSession.Settings.OverlayWidgets.ToList(); } }

        public OverlayWidgetModel SelectedWidget
        {
            get { return this.selectedWidget; }
            set
            {
                this.selectedWidget = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayWidgetModel selectedWidget;

        public bool WidgetVisible
        {
            get { return this.widgetVisible; }
            set
            {
                this.widgetVisible = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool widgetVisible;

        public bool ShowItemGrid { get { return this.SelectedActionType != OverlayActionTypeEnum.ShowHideWidget; } }

        public bool ShowTextItem { get { return this.SelectedActionType == OverlayActionTypeEnum.Text; } }

        public bool ShowImageItem { get { return this.SelectedActionType == OverlayActionTypeEnum.Image; } }

        public bool ShowVideoItem { get { return this.SelectedActionType == OverlayActionTypeEnum.Video; } }

        public bool ShowYouTubeItem { get { return this.SelectedActionType == OverlayActionTypeEnum.YouTube; } }

        public bool ShowWebPageItem { get { return this.SelectedActionType == OverlayActionTypeEnum.WebPage; } }

        public bool ShowHTMLItem { get { return this.SelectedActionType == OverlayActionTypeEnum.HTML; } }

        public OverlayItemViewModelBase SelectedItemViewModel
        {
            get
            {
                if (this.ShowImageItem)
                {
                    return this.ImageItemViewModel;
                }
                else if (this.ShowTextItem)
                {
                    return this.TextItemViewModel;
                }
                else if (this.ShowYouTubeItem)
                {
                    return this.YouTubeItemViewModel;
                }
                else if (this.ShowVideoItem)
                {
                    return this.VideoItemViewModel;
                }
                else if (this.ShowWebPageItem)
                {
                    return this.WebPageItemViewModel;
                }
                else if (this.ShowHTMLItem)
                {
                    return this.HTMLItemViewModel;
                }
                return null;
            }
        }

        public OverlayImageItemViewModel ImageItemViewModel
        {
            get { return this.imageItemViewModel; }
            set
            {
                this.imageItemViewModel = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayImageItemViewModel imageItemViewModel = new OverlayImageItemViewModel();

        public OverlayTextItemViewModel TextItemViewModel
        {
            get { return this.textItemViewModel; }
            set
            {
                this.textItemViewModel = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayTextItemViewModel textItemViewModel = new OverlayTextItemViewModel();

        public OverlayYouTubeItemViewModel YouTubeItemViewModel
        {
            get { return this.youTubeItemViewModel; }
            set
            {
                this.youTubeItemViewModel = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayYouTubeItemViewModel youTubeItemViewModel = new OverlayYouTubeItemViewModel();

        public OverlayVideoItemViewModel VideoItemViewModel
        {
            get { return this.videoItemViewModel; }
            set
            {
                this.videoItemViewModel = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayVideoItemViewModel videoItemViewModel = new OverlayVideoItemViewModel();

        public OverlayWebPageItemViewModel WebPageItemViewModel
        {
            get { return this.webPageItemViewModel; }
            set
            {
                this.webPageItemViewModel = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayWebPageItemViewModel webPageItemViewModel = new OverlayWebPageItemViewModel();

        public OverlayHTMLItemViewModel HTMLItemViewModel
        {
            get { return this.htmlItemViewModel; }
            set
            {
                this.htmlItemViewModel = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayHTMLItemViewModel htmlItemViewModel = new OverlayHTMLItemViewModel();

        public OverlayItemPositionViewModel ItemPosition
        {
            get { return this.itemPosition; }
            set
            {
                this.itemPosition = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayItemPositionViewModel itemPosition = new OverlayItemPositionViewModel();

        public double ItemDuration
        {
            get { return this.itemDuration; }
            set
            {
                if (value > 0)
                {
                    this.itemDuration = value;
                }
                else
                {
                    this.itemDuration = 0;
                }
                this.NotifyPropertyChanged();
            }
        }
        private double itemDuration;

        public IEnumerable<OverlayItemEffectEntranceAnimationTypeEnum> EntranceAnimations { get { return EnumHelper.GetEnumList<OverlayItemEffectEntranceAnimationTypeEnum>(); } }

        public OverlayItemEffectEntranceAnimationTypeEnum SelectedEntranceAnimation
        {
            get { return this.selectedEntranceAnimation; }
            set
            {
                this.selectedEntranceAnimation = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayItemEffectEntranceAnimationTypeEnum selectedEntranceAnimation;

        public IEnumerable<OverlayItemEffectExitAnimationTypeEnum> ExitAnimations { get { return EnumHelper.GetEnumList<OverlayItemEffectExitAnimationTypeEnum>(); } }

        public OverlayItemEffectExitAnimationTypeEnum SelectedExitAnimation
        {
            get { return this.selectedExitAnimation; }
            set
            {
                this.selectedExitAnimation = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayItemEffectExitAnimationTypeEnum selectedExitAnimation;

        public IEnumerable<OverlayItemEffectVisibleAnimationTypeEnum> VisibleAnimations { get { return EnumHelper.GetEnumList<OverlayItemEffectVisibleAnimationTypeEnum>(); } }

        public OverlayItemEffectVisibleAnimationTypeEnum SelectedVisibleAnimation
        {
            get { return this.selectedVisibleAnimation; }
            set
            {
                this.selectedVisibleAnimation = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayItemEffectVisibleAnimationTypeEnum selectedVisibleAnimation;

        public OverlayActionEditorControlViewModel(OverlayActionModel action)
            : base(action)
        {
            if (!string.IsNullOrEmpty(action.OverlayName))
            {
                this.SelectedOverlayEndpoint = action.OverlayName;
            }
            else
            {
                this.SelectedOverlayEndpoint = ServiceManager.Get<OverlayService>().DefaultOverlayName;
            }

            if (action.WidgetID != Guid.Empty)
            {
                this.SelectedActionType = OverlayActionTypeEnum.ShowHideWidget;
                this.SelectedWidget = ChannelSession.Settings.OverlayWidgets.FirstOrDefault(w => w.Item.ID.Equals(action.WidgetID));
                this.WidgetVisible = action.ShowWidget;
            }
            else
            {
                if (action.OverlayItem != null)
                {
                    if (action.OverlayItem.Effects != null)
                    {
                        this.ItemDuration = action.OverlayItem.Effects.Duration;
                        this.SelectedEntranceAnimation = action.OverlayItem.Effects.EntranceAnimation;
                        this.SelectedVisibleAnimation = action.OverlayItem.Effects.VisibleAnimation;
                        this.SelectedExitAnimation = action.OverlayItem.Effects.ExitAnimation;
                    }

                    if (action.OverlayItem.Position != null)
                    {
                        this.ItemPosition.SetPosition(action.OverlayItem.Position);
                    }

                    if (action.OverlayItem is OverlayImageItemModel)
                    {
                        this.SelectedActionType = OverlayActionTypeEnum.Image;
                        this.ImageItemViewModel = new OverlayImageItemViewModel((OverlayImageItemModel)action.OverlayItem);
                    }
                    else if (action.OverlayItem is OverlayTextItemModel)
                    {
                        this.SelectedActionType = OverlayActionTypeEnum.Text;
                        this.TextItemViewModel = new OverlayTextItemViewModel((OverlayTextItemModel)action.OverlayItem);
                    }
                    else if (action.OverlayItem is OverlayYouTubeItemModel)
                    {
                        this.SelectedActionType = OverlayActionTypeEnum.YouTube;
                        this.YouTubeItemViewModel = new OverlayYouTubeItemViewModel((OverlayYouTubeItemModel)action.OverlayItem);
                    }
                    else if (action.OverlayItem is OverlayVideoItemModel)
                    {
                        this.SelectedActionType = OverlayActionTypeEnum.Video;
                        this.VideoItemViewModel = new OverlayVideoItemViewModel((OverlayVideoItemModel)action.OverlayItem);
                    }
                    else if (action.OverlayItem is OverlayWebPageItemModel)
                    {
                        this.SelectedActionType = OverlayActionTypeEnum.WebPage;
                        this.WebPageItemViewModel = new OverlayWebPageItemViewModel((OverlayWebPageItemModel)action.OverlayItem);
                    }
                    else if (action.OverlayItem is OverlayHTMLItemModel)
                    {
                        this.SelectedActionType = OverlayActionTypeEnum.HTML;
                        this.HTMLItemViewModel = new OverlayHTMLItemViewModel((OverlayHTMLItemModel)action.OverlayItem);
                    }
                }
            }
        }

        public OverlayActionEditorControlViewModel()
            : base()
        {
            this.SelectedOverlayEndpoint = ServiceManager.Get<OverlayService>().DefaultOverlayName;
        }

        public override Task<Result> Validate()
        {
            if (this.SelectedActionType == OverlayActionTypeEnum.ShowHideWidget)
            {
                if (this.SelectedWidget == null)
                {
                    return Task.FromResult(new Result(MixItUp.Base.Resources.OverlayActionMissingWidget));
                }
            }
            else
            {
                if (this.ItemDuration <= 0)
                {
                    return Task.FromResult(new Result(MixItUp.Base.Resources.OverlayActionDurationInvalid));
                }

                if (this.SelectedItemViewModel == null)
                {
                    return Task.FromResult(new Result(MixItUp.Base.Resources.OverlayActionItemInvalid));
                }

                OverlayItemModelBase overlayItem = this.SelectedItemViewModel.GetOverlayItem();
                if (overlayItem == null)
                {
                    return Task.FromResult(new Result(MixItUp.Base.Resources.OverlayActionItemInvalid));
                }
            }
            return Task.FromResult(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            if (this.SelectedActionType == OverlayActionTypeEnum.ShowHideWidget)
            {
                return Task.FromResult<ActionModelBase>(new OverlayActionModel(this.SelectedWidget.Item.ID, this.WidgetVisible));
            }
            else
            {
                OverlayItemModelBase overlayItem = this.SelectedItemViewModel.GetOverlayItem();
                if (overlayItem != null)
                {
                    overlayItem.Position = this.ItemPosition.GetPosition();
                    overlayItem.Effects = new OverlayItemEffectsModel(this.SelectedEntranceAnimation, this.SelectedVisibleAnimation, this.SelectedExitAnimation, this.ItemDuration);
                    return Task.FromResult<ActionModelBase>(new OverlayActionModel(this.SelectedOverlayEndpoint, overlayItem));
                }
            }
            return Task.FromResult<ActionModelBase>(null);
        }
    }
}
