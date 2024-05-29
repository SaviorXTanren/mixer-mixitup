using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Overlay;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        EmoteEffect,

        EnableDisableWidget,

        DamageStreamBoss,
        AddToGoal,
        AddToPersistentTimer,
        AddToEndCredits,
        PlayEndCredits,
        AddToEventList,
        SpinWheel,
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
                    this.NotifyPropertyChanged(nameof(this.ShowWidget));
                    this.NotifyPropertyChanged(nameof(this.ShowDamageStreamBoss));
                    this.NotifyPropertyChanged(nameof(this.ShowAddGoal));
                    this.NotifyPropertyChanged(nameof(this.ShowAddPersistTimer));
                    this.NotifyPropertyChanged(nameof(this.ShowAddToEndCredits));
                    this.NotifyPropertyChanged(nameof(this.ShowPlayEndCredits));
                    this.NotifyPropertyChanged(nameof(this.ShowAddToEventList));
                    this.NotifyPropertyChanged(nameof(this.ShowSpinWheel));

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
                        else if (this.SelectedActionType == OverlayActionTypeEnum.EmoteEffect)
                        {
                            this.Item = new OverlayEmoteEffectV3ViewModel();
                        }

                        this.HTML = this.GetDefaultHTML(this.Item);
                        this.CSS = this.GetDefaultCSS(this.Item);
                        this.Javascript = this.GetDefaultJavascript(this.Item);

                        this.defaultHTML = this.GetDefaultHTML(this.Item);
                        this.defaultCSS = this.GetDefaultCSS(this.Item);
                        this.defaultJavascript = this.GetDefaultJavascript(this.Item);

                        this.NotifyPropertyChanged(nameof(this.HTMLHeader));
                        this.NotifyPropertyChanged(nameof(this.CSSHeader));
                        this.NotifyPropertyChanged(nameof(this.JavascriptHeader));
                    }

                    this.NotifyPropertyChanged(nameof(this.SupportsStandardActionPositioning));
                    this.NotifyPropertyChanged(nameof(this.SupportsStandardActionAnimations));
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
                    this.SelectedActionType == OverlayActionTypeEnum.TwitchClip || this.SelectedActionType == OverlayActionTypeEnum.EmoteEffect;
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

        public bool SupportsStandardActionPositioning { get { return this.Item != null && this.Item.SupportsStandardActionPositioning; } }

        public bool SupportsStandardActionAnimations { get { return this.Item != null && this.Item.SupportsStandardActionAnimations; } }

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

        public string HTML
        {
            get { return this.html; }
            set
            {
                this.html = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.HTMLHeader));
            }
        }
        private string html;

        private string defaultHTML;

        public string HTMLHeader
        {
            get { return this.IsHTMLModified ? Resources.HTML + "*" : Resources.HTML; }
        }

        public bool IsHTMLModified
        {
            get { return !string.Equals(this.HTML, this.defaultHTML); }
        }

        public string CSS
        {
            get { return this.css; }
            set
            {
                this.css = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.CSSHeader));
            }
        }
        private string css;

        private string defaultCSS;

        public string CSSHeader
        {
            get { return this.IsCSSModified ? Resources.CSS + "*" : Resources.CSS; }
        }

        public bool IsCSSModified
        {
            get { return !string.Equals(this.CSS, this.defaultCSS); }
        }

        public string Javascript
        {
            get { return this.javascript; }
            set
            {
                this.javascript = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.JavascriptHeader));
            }
        }
        private string javascript;

        private string defaultJavascript;

        public string JavascriptHeader
        {
            get { return this.IsJavascriptModified ? Resources.Javascript + "*" : Resources.Javascript; }
        }

        public bool IsJavascriptModified
        {
            get { return !string.Equals(this.Javascript, this.defaultJavascript); }
        }

        public bool ShowWidget { get { return this.SelectedActionType == OverlayActionTypeEnum.EnableDisableWidget; } }

        public ObservableCollection<OverlayWidgetV3Model> Widgets { get; set; } = new ObservableCollection<OverlayWidgetV3Model>();

        public OverlayWidgetV3Model SelectedWidget
        {
            get { return this.selectedWidget; }
            set
            {
                this.selectedWidget = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayWidgetV3Model selectedWidget;

        public bool EnableDisableWidgetValue
        {
            get { return this.enableDisableWidgetValue; }
            set
            {
                this.enableDisableWidgetValue = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool enableDisableWidgetValue;

        public bool ShowDamageStreamBoss { get { return this.SelectedActionType == OverlayActionTypeEnum.DamageStreamBoss; } }

        public ObservableCollection<OverlayWidgetV3Model> StreamBosses { get; set; } = new ObservableCollection<OverlayWidgetV3Model>();

        public OverlayWidgetV3Model SelectedStreamBoss
        {
            get { return this.selectedStreamBoss; }
            set
            {
                this.selectedStreamBoss = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayWidgetV3Model selectedStreamBoss;

        public string StreamBossDamageAmount
        {
            get { return this.streamBossDamageAmount; }
            set
            {
                this.streamBossDamageAmount = value;
                this.NotifyPropertyChanged();
            }
        }
        private string streamBossDamageAmount;

        public bool StreamBossForceDamage
        {
            get { return this.streamBossForceDamage; }
            set
            {
                this.streamBossForceDamage = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool streamBossForceDamage = true;

        public bool ShowAddGoal { get { return this.SelectedActionType == OverlayActionTypeEnum.AddToGoal; } }

        public ObservableCollection<OverlayWidgetV3Model> Goals { get; set; } = new ObservableCollection<OverlayWidgetV3Model>();

        public OverlayWidgetV3Model SelectedGoal
        {
            get { return this.selectedGoal; }
            set
            {
                this.selectedGoal = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayWidgetV3Model selectedGoal;

        public string GoalAmount
        {
            get { return this.goalAmount; }
            set
            {
                this.goalAmount = value;
                this.NotifyPropertyChanged();
            }
        }
        private string goalAmount;

        public bool ShowAddPersistTimer { get { return this.SelectedActionType == OverlayActionTypeEnum.AddToPersistentTimer; } }

        public ObservableCollection<OverlayWidgetV3Model> PersistentTimers { get; set; } = new ObservableCollection<OverlayWidgetV3Model>();

        public OverlayWidgetV3Model SelectedPersistentTimer
        {
            get { return this.selectedPersistentTimer; }
            set
            {
                this.selectedPersistentTimer = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayWidgetV3Model selectedPersistentTimer;

        public string TimeAmount
        {
            get { return this.timeAmount; }
            set
            {
                this.timeAmount = value;
                this.NotifyPropertyChanged();
            }
        }
        private string timeAmount;

        public bool ShowAddToEndCredits { get { return this.SelectedActionType == OverlayActionTypeEnum.AddToEndCredits; } }

        public bool ShowPlayEndCredits { get { return this.SelectedActionType == OverlayActionTypeEnum.PlayEndCredits; } }

        public ObservableCollection<OverlayWidgetV3Model> EndCredits { get; set; } = new ObservableCollection<OverlayWidgetV3Model>();

        public OverlayWidgetV3Model SelectedEndCredits
        {
            get { return this.selectedEndCredits; }
            set
            {
                this.selectedEndCredits = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayWidgetV3Model selectedEndCredits;

        public ObservableCollection<OverlayEndCreditsSectionV3Model> EndCreditsSections { get; set; } = new ObservableCollection<OverlayEndCreditsSectionV3Model>();

        public OverlayEndCreditsSectionV3Model SelectedEndCreditsSection
        {
            get { return this.selectedEndCreditsSection; }
            set
            {
                this.selectedEndCreditsSection = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayEndCreditsSectionV3Model selectedEndCreditsSection;
        private Guid endCreditsSectionID;

        public string EndCreditsItemText
        {
            get { return this.endCreditsItemText; }
            set
            {
                this.endCreditsItemText = value;
                this.NotifyPropertyChanged();
            }
        }
        private string endCreditsItemText;

        public bool ShowAddToEventList { get { return this.SelectedActionType == OverlayActionTypeEnum.AddToEventList; } }

        public ObservableCollection<OverlayWidgetV3Model> EventLists { get; set; } = new ObservableCollection<OverlayWidgetV3Model>();

        public OverlayWidgetV3Model SelectedEventList
        {
            get { return this.selectedEventList; }
            set
            {
                this.selectedEventList = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayWidgetV3Model selectedEventList;

        public string EventListDetails
        {
            get { return this.eventListDetails; }
            set
            {
                this.eventListDetails = value;
                this.NotifyPropertyChanged();
            }
        }
        private string eventListDetails;

        public bool ShowSpinWheel { get { return this.SelectedActionType == OverlayActionTypeEnum.SpinWheel; } }

        public ObservableCollection<OverlayWidgetV3Model> Wheels { get; set; } = new ObservableCollection<OverlayWidgetV3Model>();

        public OverlayWidgetV3Model SelectedWheel
        {
            get { return this.selectedWheel; }
            set
            {
                this.selectedWheel = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayWidgetV3Model selectedWheel;

        private Guid widgetID;

        public OverlayActionEditorControlViewModel(OverlayActionModel action)
            : base(action)
        {
            this.SelectedOverlayEndpoint = ServiceManager.Get<OverlayV3Service>().GetDefaultOverlayEndpoint();

            if (action.OverlayItemV3 != null)
            {
                OverlayEndpointV3Model overlayEndpoint = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpoint(action.OverlayItemV3.OverlayEndpointID);
                if (overlayEndpoint != null)
                {
                    this.SelectedOverlayEndpoint = overlayEndpoint;
                }

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
                else if (action.OverlayItemV3.Type == OverlayItemV3Type.EmoteEffect)
                {
                    this.SelectedActionType = OverlayActionTypeEnum.EmoteEffect;
                    this.Item = new OverlayEmoteEffectV3ViewModel((OverlayEmoteEffectV3Model)action.OverlayItemV3);
                }

                this.Position = new OverlayPositionV3ViewModel(action.OverlayItemV3);
                this.Duration = action.Duration;
                this.EntranceAnimation = new OverlayAnimationV3ViewModel(Resources.Entrance, action.EntranceAnimation);
                this.ExitAnimation = new OverlayAnimationV3ViewModel(Resources.Exit, action.ExitAnimation);

                this.HTML = action.OverlayItemV3.HTML;
                this.CSS = action.OverlayItemV3.CSS;
                this.Javascript = action.OverlayItemV3.Javascript;

                this.defaultHTML = this.GetDefaultHTML(this.Item);
                this.defaultCSS = this.GetDefaultCSS(this.Item);
                this.defaultJavascript = this.GetDefaultJavascript(this.Item);
            }
            else if (action.WidgetID != Guid.Empty)
            {
                this.SelectedActionType = OverlayActionTypeEnum.EnableDisableWidget;
                this.widgetID = action.WidgetID;
                this.EnableDisableWidgetValue = action.ShowWidget;
            }
            else if (action.StreamBossID != Guid.Empty)
            {
                this.SelectedActionType = OverlayActionTypeEnum.DamageStreamBoss;
                this.widgetID = action.StreamBossID;
                this.StreamBossDamageAmount = action.StreamBossDamageAmount;
                this.StreamBossForceDamage = action.StreamBossForceDamage;
            }
            else if (action.GoalID != Guid.Empty)
            {
                this.SelectedActionType = OverlayActionTypeEnum.AddToGoal;
                this.widgetID = action.GoalID;
                this.GoalAmount = action.GoalAmount;
            }
            else if (action.PersistentTimerID != Guid.Empty)
            {
                this.SelectedActionType = OverlayActionTypeEnum.AddToPersistentTimer;
                this.widgetID = action.PersistentTimerID;
                this.TimeAmount = action.TimeAmount;
            }
            else if (action.EndCreditsID != Guid.Empty)
            {
                if (action.EndCreditsSectionID != Guid.Empty)
                {
                    this.SelectedActionType = OverlayActionTypeEnum.AddToEndCredits;
                    this.widgetID = action.EndCreditsID;
                    this.endCreditsSectionID = action.EndCreditsSectionID;
                    this.EndCreditsItemText = action.EndCreditsItemText;
                }
                else
                {
                    this.SelectedActionType = OverlayActionTypeEnum.PlayEndCredits;
                    this.widgetID = action.EndCreditsID;
                }
            }
            else if (action.EventListID != Guid.Empty)
            {
                this.SelectedActionType = OverlayActionTypeEnum.AddToEventList;
                this.widgetID = action.EventListID;
                this.EventListDetails = action.EventListDetails;
            }
            else if (action.WheelID != Guid.Empty)
            {
                this.SelectedActionType = OverlayActionTypeEnum.SpinWheel;
                this.widgetID = action.WheelID;
            }
        }

        public OverlayActionEditorControlViewModel()
            : base()
        {
            this.SelectedOverlayEndpoint = ServiceManager.Get<OverlayV3Service>().GetDefaultOverlayEndpoint();
            this.SelectedActionType = OverlayActionTypeEnum.Text;

            this.Item = new OverlayTextV3ViewModel();
            this.HTML = this.GetDefaultHTML(this.Item);
            this.CSS = this.GetDefaultCSS(this.Item);
            this.Javascript = this.GetDefaultJavascript(this.Item);

            this.EntranceAnimation = new OverlayAnimationV3ViewModel(Resources.Entrance);
            this.ExitAnimation = new OverlayAnimationV3ViewModel(Resources.Exit);

            this.defaultHTML = this.GetDefaultHTML(this.Item);
            this.defaultCSS = this.GetDefaultCSS(this.Item);
            this.defaultJavascript = this.GetDefaultJavascript(this.Item);
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
            else if (this.ShowWidget)
            {
                if (this.SelectedWidget == null)
                {
                    return Task.FromResult<Result>(new Result(Resources.ValidValueMustBeSpecified));
                }
            }
            else if (this.ShowDamageStreamBoss)
            {
                if (this.SelectedStreamBoss == null)
                {
                    return Task.FromResult<Result>(new Result(Resources.ValidValueMustBeSpecified));
                }

                if (string.IsNullOrEmpty(this.StreamBossDamageAmount))
                {
                    return Task.FromResult<Result>(new Result(Resources.ValidValueMustBeSpecified));
                }
            }
            else if (this.ShowAddGoal)
            {
                if (this.SelectedGoal == null)
                {
                    return Task.FromResult<Result>(new Result(Resources.ValidValueMustBeSpecified));
                }

                if (string.IsNullOrEmpty(this.GoalAmount))
                {
                    return Task.FromResult<Result>(new Result(Resources.ValidValueMustBeSpecified));
                }
            }
            else if (this.ShowAddPersistTimer)
            {
                if (this.SelectedPersistentTimer == null)
                {
                    return Task.FromResult<Result>(new Result(Resources.ValidValueMustBeSpecified));
                }

                if (string.IsNullOrEmpty(this.TimeAmount))
                {
                    return Task.FromResult<Result>(new Result(Resources.ValidValueMustBeSpecified));
                }
            }
            else if (this.ShowAddToEndCredits)
            {
                if (this.SelectedEndCredits == null)
                {
                    return Task.FromResult<Result>(new Result(Resources.ValidValueMustBeSpecified));
                }

                if (this.SelectedEndCreditsSection == null)
                {
                    return Task.FromResult<Result>(new Result(Resources.ValidValueMustBeSpecified));
                }

                if (string.IsNullOrEmpty(this.EndCreditsItemText))
                {
                    return Task.FromResult<Result>(new Result(Resources.ValidValueMustBeSpecified));
                }
            }
            else if (this.ShowPlayEndCredits)
            {
                if (this.SelectedEndCredits == null)
                {
                    return Task.FromResult<Result>(new Result(Resources.ValidValueMustBeSpecified));
                }
            }
            else if (this.ShowAddToEventList)
            {
                if (this.SelectedEventList == null)
                {
                    return Task.FromResult<Result>(new Result(Resources.ValidValueMustBeSpecified));
                }

                if (string.IsNullOrEmpty(this.EventListDetails))
                {
                    return Task.FromResult<Result>(new Result(Resources.ValidValueMustBeSpecified));
                }
            }
            else if (this.ShowSpinWheel)
            {
                if (this.SelectedWheel == null)
                {
                    return Task.FromResult<Result>(new Result(Resources.ValidValueMustBeSpecified));
                }
            }

            return Task.FromResult(new Result());
        }

        protected override async Task OnOpenInternal()
        {
            await base.OnOpenInternal();

            foreach (OverlayWidgetV3Model widget in ChannelSession.Settings.OverlayWidgetsV3)
            {
                this.Widgets.Add(widget);
                if (widget.Type == OverlayItemV3Type.StreamBoss)
                {
                    this.StreamBosses.Add(widget);
                }
                else if (widget.Type == OverlayItemV3Type.Goal)
                {
                    this.Goals.Add(widget);
                }
                else if (widget.Type == OverlayItemV3Type.PersistentTimer)
                {
                    this.PersistentTimers.Add(widget);
                }
                else if (widget.Type == OverlayItemV3Type.EndCredits)
                {
                    this.EndCredits.Add(widget);
                }
                else if (widget.Type == OverlayItemV3Type.EventList)
                {
                    this.EventLists.Add(widget);
                }
                else if (widget.Type == OverlayItemV3Type.Wheel)
                {
                    this.Wheels.Add(widget);
                }
            }

            if (this.ShowWidget)
            {
                this.SelectedWidget = this.Widgets.FirstOrDefault(w => w.ID.Equals(this.widgetID));
            }
            else if (this.ShowDamageStreamBoss)
            {
                this.SelectedStreamBoss = this.StreamBosses.FirstOrDefault(w => w.ID.Equals(this.widgetID));
            }
            else if (this.ShowAddGoal)
            {
                this.SelectedGoal = this.Goals.FirstOrDefault(w => w.ID.Equals(this.widgetID));
            }
            else if (this.ShowAddPersistTimer)
            {
                this.SelectedPersistentTimer = this.PersistentTimers.FirstOrDefault(w => w.ID.Equals(this.widgetID));
            }
            else if (this.ShowAddToEndCredits)
            {
                this.SelectedEndCredits = this.EndCredits.FirstOrDefault(w => w.ID.Equals(this.widgetID));
                if (this.SelectedEndCredits != null)
                {
                    OverlayEndCreditsV3Model endCredits = (OverlayEndCreditsV3Model)this.SelectedEndCredits.Item;
                    this.EndCreditsSections.AddRange(endCredits.Sections.Where(s => s.Type == OverlayEndCreditsSectionV3Type.CustomSection));
                    this.SelectedEndCreditsSection = this.EndCreditsSections.FirstOrDefault(s => s.ID == this.endCreditsSectionID);
                }
            }
            else if (this.ShowPlayEndCredits)
            {
                this.SelectedEndCredits = this.EndCredits.FirstOrDefault(w => w.ID.Equals(this.widgetID));
            }
            else if (this.ShowAddToEventList)
            {
                this.SelectedEventList = this.EventLists.FirstOrDefault(w => w.ID.Equals(this.widgetID));
            }
            else if (this.ShowSpinWheel)
            {
                this.SelectedWheel = this.Wheels.FirstOrDefault(w => w.ID.Equals(this.widgetID));
            }
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            if (this.ShowItem)
            {
                OverlayItemV3ModelBase item = this.Item.GetItem();
                if (item != null)
                {
                    item.OverlayEndpointID = this.SelectedOverlayEndpoint.ID;
                    item.HTML = this.HTML;
                    item.CSS = this.CSS;
                    item.Javascript = this.Javascript;
                    this.Position.SetPosition(item);
                    return Task.FromResult<ActionModelBase>(new OverlayActionModel(item, this.Duration, this.EntranceAnimation.GetAnimation(), this.ExitAnimation.GetAnimation()));
                }
            }
            else if (this.ShowWidget)
            {
                return Task.FromResult<ActionModelBase>(new OverlayActionModel(this.SelectedWidget, this.EnableDisableWidgetValue));
            }
            else if (this.ShowDamageStreamBoss)
            {
                return Task.FromResult<ActionModelBase>(new OverlayActionModel((OverlayStreamBossV3Model)this.SelectedStreamBoss.Item, this.StreamBossDamageAmount, this.StreamBossForceDamage));
            }
            else if (this.ShowAddGoal)
            {
                return Task.FromResult<ActionModelBase>(new OverlayActionModel((OverlayGoalV3Model)this.SelectedGoal.Item, this.GoalAmount));
            }
            else if (this.ShowAddPersistTimer)
            {
                return Task.FromResult<ActionModelBase>(new OverlayActionModel((OverlayPersistentTimerV3Model)this.SelectedPersistentTimer.Item, this.TimeAmount));
            }
            else if (this.ShowAddToEndCredits)
            {
                return Task.FromResult<ActionModelBase>(new OverlayActionModel((OverlayEndCreditsV3Model)this.SelectedEndCredits.Item, this.SelectedEndCreditsSection, this.EndCreditsItemText));
            }
            else if (this.ShowPlayEndCredits)
            {
                return Task.FromResult<ActionModelBase>(new OverlayActionModel((OverlayEndCreditsV3Model)this.SelectedEndCredits.Item));
            }
            else if (this.ShowAddToEventList)
            {
                return Task.FromResult<ActionModelBase>(new OverlayActionModel((OverlayEventListV3Model)this.SelectedEventList.Item, this.EventListDetails));
            }
            else if (this.ShowSpinWheel)
            {
                return Task.FromResult<ActionModelBase>(new OverlayActionModel((OverlayWheelV3Model)this.SelectedWheel.Item));
            }
            return Task.FromResult<ActionModelBase>(null);
        }

        private string GetDefaultHTML(OverlayItemV3ViewModelBase item) { return item.AddPositionedWrappedHTMLCSS ? OverlayItemV3ModelBase.GetPositionWrappedHTML(item.DefaultHTML) : item.DefaultHTML; }

        private string GetDefaultCSS(OverlayItemV3ViewModelBase item) { return item.AddPositionedWrappedHTMLCSS ? OverlayItemV3ModelBase.GetPositionWrappedCSS(item.DefaultCSS) : item.DefaultCSS; }

        private string GetDefaultJavascript(OverlayItemV3ViewModelBase item) { return item.DefaultJavascript; }
    }
}
