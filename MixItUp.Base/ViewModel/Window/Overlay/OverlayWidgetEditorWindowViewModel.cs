using Mixer.Base.Util;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Window.Overlay
{
    public class OverlayTypeListing
    {
        public OverlayItemModelTypeEnum Type { get; set; }
        public string Description { get; set; }

        public string Name { get { return EnumHelper.GetEnumName(this.Type); } }

        public OverlayTypeListing(OverlayItemModelTypeEnum type, string description)
        {
            this.Type = type;
            this.Description = description;
        }
    }

    public class OverlayWidgetEditorWindowViewModel : WindowViewModelBase
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
                this.selectedOverlayEndpoint = value;
                this.NotifyPropertyChanged();
            }
        }
        private string selectedOverlayEndpoint;

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

            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.ChatMessages, "Shows the last X many chat messages from your channel. Chat messages are added as they occur."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.EndCredits, "Shows a scrolling list of text like movie credits based on user interactions throughout the stream. Showing the widget triggers the credits, hiding resets it."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.EventList, "Shows the last X many events that have occurred in your channel. Events are added as they occur."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.GameQueue, "Shows a block of text. Events are added as they occur."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.ProgressBar, "Shows a progress bar for a specified goal. Progress is updated as events occur or on user-defined refresh Interval."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.HTML, "Shows HTML code directly on the overlay. Refreshes based on user-defined Refresh Interval."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.Image, "Shows an image. Refreshes based on user-defined Refresh Interval."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.Leaderboard, "Shows the top X users in a specified category. Leaderboard positions are updated as events occur or on user-defined refresh interval"));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.StreamClip, "Shows the video & audio footage of a Stream Clip when it is taken."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.SparkCrystal, "Shows the current progress on the channel's spark crystal patronage. Progress is updated as spark patronage occurs."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.StreamBoss, "Shows a user from your channel as a \"boss\" that can be damaged by performing actions in your channel until they are defeated and a new boss is selected. Damage is added as it occurs. The Stream Boss Special Identifiers can be used in parallel with this overlay widget."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.Text, "Shows a block of text. Refreshes based on user-defined Refresh Interval."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.TickerTape, "Shows a scrolling list of text of the last X many users that caused a specified event to occur. Users are added to the list as the event occurs."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.Timer, "Shows a timer that counts down while it is visible. Hiding the timer resets the amount."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.TimerTrain, "Shows a timer that counts down and can be increased by performing certain actions in your channel. It is only shown when total time exceeds the Minimum Seconds To Show value. Time updates as events occur."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.Video, "Shows a video. Refreshes based on user-defined Refresh Interval."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.WebPage, "Shows a web page. Refreshes based on user-defined Refresh Interval."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayItemModelTypeEnum.YouTube, "Shows a YouTube video. Refreshes based on user-defined Refresh Interval."));

            this.OverlayTypeSelectedCommand = this.CreateCommand((parameter) =>
            {
                this.OverlayTypeToMake = this.SelectedOverlayType;

                this.NotifyPropertyChanged("OverlayTypeIsSelected");
                this.NotifyPropertyChanged("OverlayTypeIsNotSelected");

                this.OverlayTypeSelected(this, this.OverlayTypeToMake);

                return Task.FromResult(0);
            });
        }

        public async Task<bool> Validate()
        {
            if (string.IsNullOrEmpty(this.name))
            {
                await DialogHelper.ShowMessage("A name must be specified");
                return false;
            }

            if (string.IsNullOrEmpty(this.SelectedOverlayEndpoint))
            {
                await DialogHelper.ShowMessage("An overlay to use must be selected");
                return false;
            }

            return true;
        }

        private void Initialize()
        {
            foreach (string overlayEndpoint in ChannelSession.Services.Overlay.GetOverlayNames())
            {
                this.OverlayEndpoints.Add(overlayEndpoint);
            }
            this.SelectedOverlayEndpoint = ChannelSession.Services.Overlay.DefaultOverlayName;
        }
    }
}
