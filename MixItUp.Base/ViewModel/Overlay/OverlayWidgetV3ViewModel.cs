using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayWidgetV3ViewModel : UIViewModelBase
    {
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

        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                NotifyPropertyChanged();
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
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(this.IsDisplayOptionOverlayEndpoint));
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
                NotifyPropertyChanged();
            }
        }
        private OverlayEndpointV3Model selectedOverlayEndpoint;

        public int RefreshTime
        {
            get { return this.refreshTime; }
            set
            {
                this.refreshTime = value;
                NotifyPropertyChanged();
            }
        }
        private int refreshTime;

        public OverlayItemV3ViewModelBase Item
        {
            get { return this.item; }
            set
            {
                this.item = value;
                NotifyPropertyChanged();
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

        public bool IsTestable { get { return this.Item.IsTestable; } }

        public ICommand SaveCommand { get; set; }
        public ICommand TestCommand { get; set; }
        public ICommand ExportCommand { get; set; }

        public event EventHandler OnCloseRequested = delegate { };

        private OverlayWidgetV3Model existingWidget;

        private OverlayWidgetV3Model testWidget;

        private OverlayWidgetV3Model newWidget;

        public OverlayWidgetV3ViewModel(OverlayItemV3Type type)
        {
            this.ID = Guid.NewGuid();
            this.Name = EnumLocalizationHelper.GetLocalizedName(type);
            this.SelectedOverlayEndpoint = ServiceManager.Get<OverlayV3Service>().GetDefaultOverlayEndpoint();
            this.SelectedDisplayOption = OverlayItemV3DisplayOptionsType.OverlayEndpoint;

            switch (type)
            {
                case OverlayItemV3Type.Text: this.Item = new OverlayTextV3ViewModel(); break;
                case OverlayItemV3Type.Image: this.Item = new OverlayImageV3ViewModel(); break;
                case OverlayItemV3Type.Video: this.Item = new OverlayVideoV3ViewModel(); break;
                case OverlayItemV3Type.YouTube: this.Item = new OverlayYouTubeV3ViewModel(); break;
                case OverlayItemV3Type.HTML: this.Item = new OverlayHTMLV3ViewModel(); break;
                case OverlayItemV3Type.Timer:  this.Item = new OverlayTimerV3ViewModel(); break;
                case OverlayItemV3Type.TwitchClip:  this.Item = new OverlayTwitchClipV3ViewModel(); break;
                case OverlayItemV3Type.Label: this.Item = new OverlayLabelV3ViewModel(); break;
                case OverlayItemV3Type.StreamBoss: this.Item = new OverlayStreamBossV3ViewModel(); break;
                case OverlayItemV3Type.Goal: this.Item = new OverlayGoalV3ViewModel(); break;
                case OverlayItemV3Type.PersistentTimer: this.Item = new OverlayPersistentTimerV3ViewModel(); break;
            }

            this.HTML = OverlayItemV3ModelBase.GetPositionWrappedHTML(this.Item.DefaultHTML);
            this.CSS = OverlayItemV3ModelBase.GetPositionWrappedCSS(this.Item.DefaultCSS);
            this.Javascript = this.Item.DefaultJavascript;

            // Add Widget-unique Animations
            if (type == OverlayItemV3Type.Text || type == OverlayItemV3Type.Image || type == OverlayItemV3Type.Video ||
                type == OverlayItemV3Type.YouTube || type == OverlayItemV3Type.HTML || type == OverlayItemV3Type.TwitchClip)
            {
                
            }

            this.Initialize();
        }

        public OverlayWidgetV3ViewModel(OverlayWidgetV3Model widget)
        {
            this.existingWidget = widget;

            this.ID = widget.ID;
            this.Name = widget.Name;
            this.RefreshTime = widget.RefreshTime;

            this.SelectedDisplayOption = widget.Item.DisplayOption;

            this.SelectedOverlayEndpoint = ServiceManager.Get<OverlayV3Service>().GetDefaultOverlayEndpoint();
            if (this.SelectedDisplayOption == OverlayItemV3DisplayOptionsType.OverlayEndpoint)
            {
                OverlayEndpointV3Model overlayEndpoint = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpoint(widget.Item.OverlayEndpointID);
                if (overlayEndpoint == null)
                {
                    this.SelectedOverlayEndpoint = overlayEndpoint;
                }
            }

            switch (widget.Item.Type)
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
            }

            this.HTML = widget.Item.HTML;
            this.CSS = widget.Item.CSS;
            this.Javascript = widget.Item.Javascript;

            this.testWidget = widget;

            this.Initialize();
        }

        public Result Validate()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                return new Result(Resources.ANameMustBeSpecified);
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

        public OverlayWidgetV3Model GetWidget()
        {
            OverlayItemV3ModelBase item = this.Item.GetItem();
            item.ID = this.ID;
            item.OverlayEndpointID = this.SelectedOverlayEndpoint.ID;
            item.HTML = this.HTML;
            item.CSS = this.CSS;
            item.Javascript = this.Javascript;
            item.DisplayOption = this.SelectedDisplayOption;
            item.Position = this.Position.GetPosition();

            OverlayWidgetV3Model widget = new OverlayWidgetV3Model(item);
            widget.Name = this.Name;
            return widget;
        }

        public async Task DisableTestWidget()
        {
            if (this.testWidget != null)
            {
                await this.testWidget.Disable();
            }
        }

        protected override async Task OnOpenInternal()
        {
            if (this.existingWidget != null)
            {
                await this.existingWidget.Disable();
            }

            this.Item.PropertyChanged += Item_PropertyChanged;

            await this.EnableTestWidget();
        }

        protected override async Task OnClosedInternal()
        {
            await this.DisableTestWidget();

            if (this.newWidget == null && this.existingWidget != null)
            {
                await this.existingWidget.Enable();
            }

            await base.OnClosedInternal();
        }

        private void Initialize()
        {
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

                this.newWidget = this.GetWidget();
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

                OverlayWidgetV3Model widget = this.GetWidget();
            });
        }

        private void Animation_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Item_PropertyChanged(sender, e);
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (this.Validate().Success)
            {
                Task.Run(async () =>
                {
                    await this.RefreshTestWidget();
                });
            }
        }

        private async Task EnableTestWidget()
        {
            OverlayWidgetV3Model widget = this.GetWidget();
            this.testWidget = widget;
            await widget.Enable();
        }

        private async Task RefreshTestWidget()
        {
            await this.DisableTestWidget();

            await this.EnableTestWidget();
        }
    }
}
