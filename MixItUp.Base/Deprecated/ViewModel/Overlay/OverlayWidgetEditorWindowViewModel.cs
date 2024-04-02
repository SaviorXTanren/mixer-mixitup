using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Overlay
{
    [Obsolete]
    public class OverlayTypeListing
    {
        public OverlayItemModelTypeEnum Type { get; set; }
        public string Description { get; set; }

        public OverlayTypeListing(OverlayItemModelTypeEnum type, string description)
        {
            this.Type = type;
            this.Description = description;
        }
    }

    [Obsolete]
    public class OverlayWidgetEditorWindowViewModel : UIViewModelBase
    {
        public event EventHandler<OverlayTypeListing> OverlayTypeSelected;

        public OverlayWidgetModel OverlayWidget { get; private set; }

        public ObservableCollection<OverlayTypeListing> OverlayTypeListings { get; private set; } = new ObservableCollection<OverlayTypeListing>();
        public OverlayTypeListing SelectedOverlayType
        {
            get { return this.selectedOverlayType; }
            set
            {
                this.selectedOverlayType = value;
                this.NotifyPropertyChanged();
            }
        }
        private OverlayTypeListing selectedOverlayType;

        public OverlayTypeListing OverlayTypeToMake { get; private set; }

        public bool OverlayTypeIsSelected { get { return this.OverlayWidget != null || this.OverlayTypeToMake != null; } }
        public bool OverlayTypeIsNotSelected { get { return !this.OverlayTypeIsSelected; } }

        public ICommand OverlayTypeSelectedCommand { get; private set; }

        // Widget Editor Properties

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

        public ObservableCollection<string> OverlayEndpoints { get; set; } = new ObservableCollection<string>();

        public string SelectedOverlayEndpoint
        {
            get { return this.selectedOverlayEndpoint; }
            set
            {
                //var overlays = ServiceManager.Get<OverlayService>().GetOverlayNames();
                //if (overlays.Contains(value))
                //{
                //    this.selectedOverlayEndpoint = value;
                //}
                //else
                //{
                //    this.selectedOverlayEndpoint = ServiceManager.Get<OverlayService>().DefaultOverlayName;
                //}
                this.NotifyPropertyChanged();
            }
        }
        private string selectedOverlayEndpoint = null;

        public string RefreshTimeString
        {
            get { return this.RefreshTime.ToString(); }
            set
            {
                this.RefreshTime = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }

        public int RefreshTime = 0;

        public bool SupportsRefreshUpdating
        {
            get { return this.supportsRefreshUpdating; }
            set
            {
                this.supportsRefreshUpdating = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool supportsRefreshUpdating;

        public OverlayWidgetEditorWindowViewModel(OverlayWidgetModel widget)
        {
            this.Initialize();

            this.OverlayWidget = widget;

            this.Name = this.OverlayWidget.Name;
            this.SelectedOverlayEndpoint = this.OverlayWidget.OverlayName;

            this.RefreshTime = this.OverlayWidget.RefreshTime;
        }

        public OverlayWidgetEditorWindowViewModel()
        {
            this.Initialize();

            List<OverlayTypeListing> widgets = new List<OverlayTypeListing>();

            widgets.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.ChatMessages, Resources.OverlayWidgetChatMessagesDescription));
            widgets.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.EndCredits, Resources.OverlayWidgetEndCreditsDescription));
            widgets.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.EventList, Resources.OverlayWidgetEventListDescription));
            widgets.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.GameQueue, Resources.OverlayWidgetGameQueueDescription));
            widgets.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.ProgressBar, Resources.OverlayWidgetProgressBarDescription));
            widgets.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.HTML, Resources.OverlayWidgetHTMLDescription));
            widgets.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.Image, Resources.OverlayWidgetImageDescription));
            widgets.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.Leaderboard, Resources.OverlayWidgetLeaderboardDescription));
            widgets.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.StreamBoss, Resources.OverlayWidgetStreamBossDescription));
            widgets.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.Text, Resources.OverlayWidgetTextDescription));
            widgets.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.TickerTape, Resources.OverlayWidgetTickerTapeDescription));
            widgets.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.Timer, Resources.OverlayWidgetTimerDescription));
            widgets.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.TimerTrain, Resources.OverlayWidgetTimerTrainDescription));
            widgets.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.Video, Resources.OverlayWidgetVideoDescription));
            widgets.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.WebPage, Resources.OverlayWidgetWebPageDescription));
            widgets.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.YouTube, Resources.OverlayWidgetYouTubeDescription));

            this.OverlayTypeListings.AddRange(widgets);

            this.OverlayTypeSelectedCommand = this.CreateCommand(() =>
            {
                this.OverlayTypeToMake = this.SelectedOverlayType;

                this.NotifyPropertyChanged("OverlayTypeIsSelected");
                this.NotifyPropertyChanged("OverlayTypeIsNotSelected");

                this.OverlayTypeSelected(this, this.OverlayTypeToMake);
            });
        }

        public async Task<bool> Validate()
        {
            if (string.IsNullOrEmpty(this.name))
            {
                await DialogHelper.ShowMessage(Resources.NameRequired);
                return false;
            }

            if (string.IsNullOrEmpty(this.SelectedOverlayEndpoint))
            {
                await DialogHelper.ShowMessage(Resources.OverlayRequired);
                return false;
            }

            return true;
        }

        private void Initialize()
        {
            //this.OverlayEndpoints.AddRange(ServiceManager.Get<OverlayService>().GetOverlayNames());
            //this.SelectedOverlayEndpoint = ServiceManager.Get<OverlayService>().DefaultOverlayName;
        }
    }
}
