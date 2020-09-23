using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.Actions
{
    public enum OverlayActionTypeEnum
    {
        Text,
        Image,
        Video,
        YouTube,
        WebPage,
        HTML,
        ShowHideWidget
    }

    public class OverlayActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        private const string ShowHideWidgetOption = "Show/Hide Widget";

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
            }
        }
        private OverlayActionTypeEnum selectedActionType;

        public bool OverlayNotEnabled { get { return !ChannelSession.Services.Overlay.IsConnected; } }

        public bool OverlayEnabled { get { return !this.OverlayNotEnabled; } }

        public IEnumerable<string> OverlayEndpoints { get { return ChannelSession.Services.Overlay.GetOverlayNames(); } }

        public string SelectedOverlayEndpoint
        {
            get { return this.selectedOverlayEndpoint; }
            set
            {
                this.selectedOverlayEndpoint = value;
                this.NotifyPropertyChanged();
            }
        }
        private string selectedOverlayEndpoint;

        public bool ShowShowHideWidgetGrid { get { return this.SelectedActionType == OverlayActionTypeEnum.ShowHideWidget; } }

        public IEnumerable<OverlayWidgetModel> Widgets { get { return ChannelSession.Settings.OverlayWidgets; } }

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

        public bool ShowTextItem { get { return this.SelectedActionType != OverlayActionTypeEnum.Text; } }

        public bool ShowImageItem { get { return this.SelectedActionType != OverlayActionTypeEnum.Image; } }

        public bool ShowVideoItem { get { return this.SelectedActionType != OverlayActionTypeEnum.Video; } }

        public bool ShowYouTubeItem { get { return this.SelectedActionType != OverlayActionTypeEnum.YouTube; } }

        public bool ShowWebPageItem { get { return this.SelectedActionType != OverlayActionTypeEnum.WebPage; } }

        public bool ShowHTMLItem { get { return this.SelectedActionType != OverlayActionTypeEnum.HTML; } }

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
        {
            // TODO
        }

        public OverlayActionEditorControlViewModel() { }

        public override Task<Result> Validate()
        {
            // TODO
            return base.Validate();
        }

        public override Task<ActionModelBase> GetAction()
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}
