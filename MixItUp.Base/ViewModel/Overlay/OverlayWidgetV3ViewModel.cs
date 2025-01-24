using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayWidgetV3ViewModel : UIViewModelBase
    {
        public const string MixItUpOverlayWidgetFileExtension = ".miuoverlay";

        public static Dictionary<Guid, OverlayWidgetV3ViewModel> WidgetsInEditing = new Dictionary<Guid, OverlayWidgetV3ViewModel>();

        public Guid ID
        {
            get { return this.id; }
            set
            {
                this.id = value;
                this.NotifyPropertyChanged();
            }
        }
        private Guid id;

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

        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                this.NotifyPropertyChanged();
            }
        }
        private string name;

        public bool IsEnabled
        {
            get { return this.isEnabled; }
            set
            {
                this.isEnabled = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool isEnabled;

        public IEnumerable<OverlayItemV3DisplayOptionsType> DisplayOptions { get; set; } = EnumHelper.GetEnumList<OverlayItemV3DisplayOptionsType>();

        public OverlayItemV3DisplayOptionsType SelectedDisplayOption
        {
            get { return this.selectedDisplayOption; }
            set
            {
                this.selectedDisplayOption = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.IsDisplayOptionOverlayEndpoint));
            }
        }
        private OverlayItemV3DisplayOptionsType selectedDisplayOption;

        public bool IsDisplayOptionOverlayEndpoint { get { return this.SelectedDisplayOption == OverlayItemV3DisplayOptionsType.OverlayEndpoint; } }

        public IEnumerable<OverlayEndpointV3Model> OverlayEndpoints { get; set; } = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpoints();

        public OverlayEndpointV3Model SelectedOverlayEndpoint
        {
            get { return this.selectedOverlayEndpoint; }
            set
            {
                this.selectedOverlayEndpoint = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayEndpointV3Model selectedOverlayEndpoint;

        public bool IsBasicWidget
        {
            get
            {
                return this.Type == OverlayItemV3Type.Text || this.Type == OverlayItemV3Type.Image || this.Type == OverlayItemV3Type.Video ||
                    this.Type == OverlayItemV3Type.YouTube || this.Type == OverlayItemV3Type.HTML;
            }
        }

        public int RefreshTime
        {
            get { return this.refreshTime; }
            set
            {
                this.refreshTime = value;
                this.NotifyPropertyChanged();
                this.RefreshWidgetPreview();
            }
        }
        private int refreshTime;

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

        public ObservableCollection<OverlayAnimationV3ViewModel> Animations { get; set; } = new ObservableCollection<OverlayAnimationV3ViewModel>();

        public string HTML
        {
            get { return this.html; }
            set
            {
                this.html = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.HTMLHeader));
                this.RefreshWidgetPreview();
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
                this.RefreshWidgetPreview();
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
                this.RefreshWidgetPreview();
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

        public bool IsTestable { get { return this.Item.IsTestable; } }

        public string OldCustomHTML { get; private set; }
        public bool ShowOldCustomHTML { get { return !string.IsNullOrEmpty(this.OldCustomHTML); } }

        public ICommand ShowOldHTMLCustomCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand TestCommand { get; set; }
        public ICommand ExportCommand { get; set; }

        public event EventHandler OnCloseRequested = delegate { };

        private OverlayWidgetV3Model existingWidget;
        private bool existingWidgetState;

        private OverlayWidgetV3Model testWidget;

        private OverlayWidgetV3Model newWidget;

        private bool loaded = false;
        private DelayedCountSemaphore delayedRefreshPreviewSemaphore = new DelayedCountSemaphore(100);
        private SemaphoreSlim refreshPreviewSemaphoreSlim = new SemaphoreSlim(1);

        public OverlayWidgetV3ViewModel(OverlayItemV3Type type)
        {
            this.ID = Guid.NewGuid();
            this.Type = type;
            this.Name = EnumLocalizationHelper.GetLocalizedName(type);

            this.SelectedOverlayEndpoint = ServiceManager.Get<OverlayV3Service>().GetDefaultOverlayEndpoint();
            this.SelectedDisplayOption = OverlayItemV3DisplayOptionsType.OverlayEndpoint;

            switch (this.Type)
            {
                case OverlayItemV3Type.Text: this.Item = new OverlayTextV3ViewModel(); break;
                case OverlayItemV3Type.Image: this.Item = new OverlayImageV3ViewModel(); break;
                case OverlayItemV3Type.Video: this.Item = new OverlayVideoV3ViewModel(); break;
                case OverlayItemV3Type.YouTube: this.Item = new OverlayYouTubeV3ViewModel(); break;
                case OverlayItemV3Type.HTML: this.Item = new OverlayHTMLV3ViewModel(); break;

                case OverlayItemV3Type.PersistentTimer: this.Item = new OverlayPersistentTimerV3ViewModel(); break;
                case OverlayItemV3Type.Label: this.Item = new OverlayLabelV3ViewModel(); break;
                case OverlayItemV3Type.StreamBoss: this.Item = new OverlayStreamBossV3ViewModel(); break;
                case OverlayItemV3Type.Goal: this.Item = new OverlayGoalV3ViewModel(); break;
                case OverlayItemV3Type.Chat: this.Item = new OverlayChatV3ViewModel(); break;
                case OverlayItemV3Type.EndCredits: this.Item = new OverlayEndCreditsV3ViewModel(); break;
                case OverlayItemV3Type.GameQueue: this.Item = new OverlayGameQueueV3ViewModel(); break;
                case OverlayItemV3Type.EventList: this.Item = new OverlayEventListV3ViewModel(); break;
                case OverlayItemV3Type.Leaderboard: this.Item = new OverlayLeaderboardV3ViewModel(); break;
                case OverlayItemV3Type.Wheel: this.Item = new OverlayWheelV3ViewModel(); break;
                case OverlayItemV3Type.EmoteEffect: this.Item = new OverlayEmoteEffectV3ViewModel(); break;
                case OverlayItemV3Type.PersistentEmoteEffect: this.Item = new OverlayPersistentEmoteEffectV3ViewModel(); break;
                case OverlayItemV3Type.Poll: this.Item = new OverlayPollV3ViewModel(); break;
                case OverlayItemV3Type.DiscordReactiveVoice: this.Item = new OverlayDiscordReactiveVoiceV3ViewModel(); break;

                case OverlayItemV3Type.Custom: this.Item = new OverlayCustomV3ViewModel(); break;
            }

            this.HTML = this.GetDefaultHTML(this.Item);
            this.CSS = this.GetDefaultCSS(this.Item);
            this.Javascript = this.GetDefaultJavascript(this.Item);

            if (this.Type == OverlayItemV3Type.EndCredits)
            {
                this.Position.XPosition = 0;
                this.Position.YPosition = 0;
                this.Position.SelectedPositionType = OverlayPositionV3Type.Pixel;
            }

            this.Initialize();
        }

        public OverlayWidgetV3ViewModel(OverlayWidgetV3Model widget)
        {
            this.existingWidget = widget;
            this.existingWidgetState = this.existingWidget.IsEnabled;

            this.ID = widget.ID;
            this.Type = widget.Item.Type;
            this.Name = widget.Name;
            this.RefreshTime = widget.RefreshTime;

#pragma warning disable CS0612 // Type or member is obsolete
            this.OldCustomHTML = widget.Item.OldCustomHTML;
#pragma warning restore CS0612 // Type or member is obsolete

            this.SelectedDisplayOption = widget.Item.DisplayOption;
            this.SelectedOverlayEndpoint = ServiceManager.Get<OverlayV3Service>().GetDefaultOverlayEndpoint();
            if (this.SelectedDisplayOption == OverlayItemV3DisplayOptionsType.OverlayEndpoint)
            {
                OverlayEndpointV3Model overlayEndpoint = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpoint(widget.Item.OverlayEndpointID);
                if (overlayEndpoint != null)
                {
                    this.SelectedOverlayEndpoint = overlayEndpoint;
                }
            }

            switch (this.Type)
            {
                case OverlayItemV3Type.Text: this.Item = new OverlayTextV3ViewModel((OverlayTextV3Model)widget.Item); break;
                case OverlayItemV3Type.Image: this.Item = new OverlayImageV3ViewModel((OverlayImageV3Model)widget.Item); break;
                case OverlayItemV3Type.Video: this.Item = new OverlayVideoV3ViewModel((OverlayVideoV3Model)widget.Item); break;
                case OverlayItemV3Type.YouTube: this.Item = new OverlayYouTubeV3ViewModel((OverlayYouTubeV3Model)widget.Item); break;
                case OverlayItemV3Type.HTML: this.Item = new OverlayHTMLV3ViewModel((OverlayHTMLV3Model)widget.Item); break;

                case OverlayItemV3Type.Timer: this.Item = new OverlayTimerV3ViewModel((OverlayTimerV3Model)widget.Item); break;
                case OverlayItemV3Type.TwitchClip: this.Item = new OverlayTwitchClipV3ViewModel((OverlayTwitchClipV3Model)widget.Item); break;
                case OverlayItemV3Type.Label: this.Item = new OverlayLabelV3ViewModel((OverlayLabelV3Model)widget.Item); break;
                case OverlayItemV3Type.StreamBoss: this.Item = new OverlayStreamBossV3ViewModel((OverlayStreamBossV3Model)widget.Item); break;
                case OverlayItemV3Type.Goal: this.Item = new OverlayGoalV3ViewModel((OverlayGoalV3Model)widget.Item); break;
                case OverlayItemV3Type.PersistentTimer: this.Item = new OverlayPersistentTimerV3ViewModel((OverlayPersistentTimerV3Model)widget.Item); break;
                case OverlayItemV3Type.Chat: this.Item = new OverlayChatV3ViewModel((OverlayChatV3Model)widget.Item); break;
                case OverlayItemV3Type.EndCredits: this.Item = new OverlayEndCreditsV3ViewModel((OverlayEndCreditsV3Model)widget.Item); break;
                case OverlayItemV3Type.GameQueue: this.Item = new OverlayGameQueueV3ViewModel((OverlayGameQueueV3Model)widget.Item); break;
                case OverlayItemV3Type.EventList: this.Item = new OverlayEventListV3ViewModel((OverlayEventListV3Model)widget.Item); break;
                case OverlayItemV3Type.Leaderboard: this.Item = new OverlayLeaderboardV3ViewModel((OverlayLeaderboardV3Model)widget.Item); break;
                case OverlayItemV3Type.Wheel: this.Item = new OverlayWheelV3ViewModel((OverlayWheelV3Model)widget.Item); break;
                case OverlayItemV3Type.EmoteEffect: this.Item = new OverlayEmoteEffectV3ViewModel((OverlayEmoteEffectV3Model)widget.Item); break;
                case OverlayItemV3Type.PersistentEmoteEffect: this.Item = new OverlayPersistentEmoteEffectV3ViewModel((OverlayPersistentEmoteEffectV3Model)widget.Item); break;
                case OverlayItemV3Type.Poll: this.Item = new OverlayPollV3ViewModel((OverlayPollV3Model)widget.Item); break;
                case OverlayItemV3Type.DiscordReactiveVoice: this.Item = new OverlayDiscordReactiveVoiceV3ViewModel((OverlayDiscordReactiveVoiceV3Model)widget.Item); break;

                case OverlayItemV3Type.Custom: this.Item = new OverlayCustomV3ViewModel((OverlayCustomV3Model)widget.Item); break;
            }

            this.HTML = widget.Item.HTML;
            this.CSS = widget.Item.CSS;
            this.Javascript = widget.Item.Javascript;

            this.Position = new OverlayPositionV3ViewModel(widget.Item);

            this.Initialize();
        }

        public Result Validate()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                return new Result(Resources.ANameMustBeSpecified);
            }

            if (this.IsBasicWidget && this.RefreshTime < 0)
            {
                return new Result(Resources.OverlayWidgetAValidRefreshTimeMustBeSpecified);
            }

            Result result = this.Item.Validate();
            if (!result.Success)
            {
                return result;
            }

            result = this.Position.Validate();
            if (!result.Success)
            {
                return result;
            }

            return new Result();
        }

        public async Task<OverlayWidgetV3Model> GetWidget()
        {
            OverlayItemV3ModelBase item = this.Item.GetItem();
            item.ID = this.ID;
            item.OverlayEndpointID = this.SelectedOverlayEndpoint.ID;
            item.HTML = this.HTML;
            item.CSS = this.CSS;
            item.Javascript = this.Javascript;
            item.DisplayOption = this.SelectedDisplayOption;
            this.Position.SetPosition(item);

            await item.Reset();

            OverlayWidgetV3Model widget = new OverlayWidgetV3Model(item);
            widget.Name = this.Name;
            widget.RefreshTime = this.RefreshTime;
            return widget;
        }

        public async Task ProcessPacket(OverlayV3Packet packet)
        {
            try
            {
                if (this.testWidget != null)
                {
                    await this.testWidget.Item.ProcessPacket(packet);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        protected override async Task OnOpenInternal()
        {
            await base.OnOpenInternal();

            this.Item.PropertyChanged += Item_PropertyChanged;

            if (this.existingWidget != null)
            {
                await this.existingWidget.Disable();
            }

            this.loaded = true;
            this.RefreshWidgetPreview();
        }

        protected override async Task OnClosedInternal()
        {
            await base.OnClosedInternal();

            await this.RemoveWidgetPreview();

            if (this.newWidget == null && this.existingWidget != null && this.existingWidgetState)
            {
                await this.existingWidget.Enable();
            }
        }

        private void Initialize()
        {
            this.delayedRefreshPreviewSemaphore.Completed += DelayedRefreshPreviewSemaphore_Completed;

            this.Position.PositionUpdated += (sender, e) =>
            {
                this.RefreshWidgetPreview();
            };

            this.defaultHTML = this.GetDefaultHTML(this.Item);
            this.defaultCSS = this.GetDefaultCSS(this.Item);
            this.defaultJavascript = this.GetDefaultJavascript(this.Item);

            foreach (OverlayAnimationV3ViewModel animation in this.Item.Animations)
            {
                this.Animations.Add(animation);
                animation.PropertyChanged += Animation_PropertyChanged;
            }

            this.SaveCommand = this.CreateCommand(async () =>
            {
                Result result = this.Validate();
                if (!result.Success)
                {
                    await DialogHelper.ShowFailedResults(new List<Result>() { result });
                    return;
                }

                if (this.testWidget != null)
                {
                    await this.testWidget.Disable();
                    this.testWidget = null;
                }

                if (this.existingWidget != null)
                {
                    await ServiceManager.Get<OverlayV3Service>().RemoveWidget(this.existingWidget);
                }

                this.newWidget = await this.GetWidget();
                await ServiceManager.Get<OverlayV3Service>().AddWidget(this.newWidget);

                this.OnCloseRequested(this, new EventArgs());
            });

            this.TestCommand = this.CreateCommand(async () =>
            {
                if (this.testWidget != null)
                {
                    await this.Item.TestWidget(this.testWidget);
                }
            });

            this.ExportCommand = this.CreateCommand(async () =>
            {
                Result result = this.Validate();
                if (!result.Success)
                {
                    await DialogHelper.ShowFailedResults(new List<Result>() { result });
                    return;
                }

                OverlayWidgetV3Model widget = await this.GetWidget();
                if (widget != null)
                {
                    string fileName = ServiceManager.Get<IFileService>().ShowSaveFileDialog(widget.Name + MixItUpOverlayWidgetFileExtension, MixItUp.Base.Resources.MixItUpOverlayFileFormatFilter);
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        await FileSerializerHelper.SerializeToFile(fileName, widget);
                    }
                }
            });
        }

        private void Animation_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.RefreshWidgetPreview();
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.RefreshWidgetPreview();
        }

        private string GetDefaultHTML(OverlayItemV3ViewModelBase item) { return OverlayItemV3ModelBase.GetPositionWrappedHTML(item.DefaultHTML); }

        private string GetDefaultCSS(OverlayItemV3ViewModelBase item) { return OverlayItemV3ModelBase.GetPositionWrappedCSS(item.DefaultCSS); }

        private string GetDefaultJavascript(OverlayItemV3ViewModelBase item)
        {
            // Add Widget-unique Javascript
            if (item.Type == OverlayItemV3Type.Text)
            {
                return OverlayResources.OverlayTextWidgetDefaultJavascript;
            }
            else if (item.Type == OverlayItemV3Type.Image)
            {
                return OverlayResources.OverlayImageWidgetDefaultJavascript;
            }
            else if (item.Type == OverlayItemV3Type.Video)
            {
                return OverlayResources.OverlayVideoWidgetDefaultJavascript;
            }
            else if (item.Type == OverlayItemV3Type.YouTube)
            {
                return OverlayResources.OverlayYouTubeWidgetDefaultJavascript;
            }
            else if (item.Type == OverlayItemV3Type.HTML)
            {
                return OverlayResources.OverlayHTMLWidgetDefaultJavascript;
            }
            return item.DefaultJavascript;
        }

        private void RefreshWidgetPreview()
        {
            if (this.loaded && this.Validate().Success)
            {
                this.delayedRefreshPreviewSemaphore.Add();
            }
        }

        private void DelayedRefreshPreviewSemaphore_Completed(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                await this.refreshPreviewSemaphoreSlim.WaitAsync();

                await this.RemoveWidgetPreview();

                OverlayWidgetV3Model widget = await this.GetWidget();
                this.testWidget = widget;
                this.testWidget.Item.IsLivePreview = true;
                OverlayWidgetV3ViewModel.WidgetsInEditing[this.ID] = this;
                await this.testWidget.Enable();

                this.refreshPreviewSemaphoreSlim.Release();
            });
        }

        private async Task RemoveWidgetPreview()
        {
            OverlayWidgetV3ViewModel.WidgetsInEditing.Remove(this.ID);
            if (this.testWidget != null)
            {
                await this.testWidget.Disable();
                this.testWidget = null;
            }
        }
    }
}
