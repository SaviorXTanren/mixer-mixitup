using Mixer.Base.Util;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Window.Overlay
{
    public enum OverlayWidgetTypeEnum
    {
        Text,
        Image,
        Video,
        YouTube,
        HTML,
        [Name("Web Page")]
        WebPage,
        [Name("Goal/Progress Bar")]
        ProgressBar,
        [Name("Event List")]
        EventList,
        [Name("Game Queue")]
        GameQueue,
        [Name("Chat Messages")]
        ChatMessages,
        [Name("Mixer Clip Playback")]
        MixerClip,
        Leaderboard,
        Timer,
        [Name("Timer Train")]
        TimerTrain,
        [Name("Stream Boss")]
        StreamBoss,
        [Name("Song Requests")]
        SongRequests
    }

    public class OverlayTypeListing
    {
        public OverlayWidgetTypeEnum Type { get; set; }
        public string Description { get; set; }

        public string Name { get { return EnumHelper.GetEnumName(this.Type); } }

        public OverlayTypeListing(OverlayWidgetTypeEnum type, string description)
        {
            this.Type = type;
            this.Description = description;
        }
    }

    public class OverlayWidgetEditorWindowViewModel : WindowViewModelBase
    {
        public event EventHandler<OverlayTypeListing> OverlayTypeSelected;

        public OverlayWidget OverlayWidget { get; private set; }

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

        public bool DontRefresh
        {
            get { return this.dontRefresh; }
            set
            {
                this.dontRefresh = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool dontRefresh;


        public OverlayWidgetEditorWindowViewModel(OverlayWidget widget)
        {
            this.Initialize();

            this.OverlayWidget = widget;

            this.Name = this.OverlayWidget.Name;
            this.SelectedOverlayEndpoint = this.OverlayWidget.OverlayName;
            this.DontRefresh = this.OverlayWidget.DontRefresh;
        }

        public OverlayWidgetEditorWindowViewModel()
        {
            this.Initialize();

            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayWidgetTypeEnum.ChatMessages, "Shows the last X many chat messages from your channel. Refreshes based on user-defined Refresh Interval & as chat messages occur."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayWidgetTypeEnum.EventList, "Shows the last X many events that have occurred in your channel. Refreshes based on user-defined Refresh Interval & as events occur."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayWidgetTypeEnum.GameQueue, "Shows a block of text. Refreshes based on user-defined Refresh Interval."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayWidgetTypeEnum.ProgressBar, "Shows a progress bar for a specified goal. Refreshes based on user-defined Refresh Interval."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayWidgetTypeEnum.HTML, "Shows HTML code directly on the overlay. Refreshes based on user-defined Refresh Interval."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayWidgetTypeEnum.Image, "Shows an image. Refreshes based on user-defined Refresh Interval."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayWidgetTypeEnum.Leaderboard, "Shows the top X users in a specified category. Currency/Ranks refresh once per minute, all other types refresh based on user-defined Refresh Interval."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayWidgetTypeEnum.MixerClip, "Shows the video & audio footage of a Mixer Clip when it is taken. Refreshes based on user-defined Refresh Interval."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayWidgetTypeEnum.SongRequests, "how the top X songs currently in the song request queue. Refreshes based on user-defined Refresh Interval & as song requests are added/removed."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayWidgetTypeEnum.StreamBoss, "Shows a user from your channel as a \"boss\" that can be damaged by performing actions in your channel until they are defeated and a new boss is selected. Refreshes based on user-defined Refresh Interval. The Stream Boss Special Identifiers can be used in parallel with this overlay widget."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayWidgetTypeEnum.Text, "Shows a block of text. Refreshes based on user-defined Refresh Interval."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayWidgetTypeEnum.Timer, "Shows a timer that counts down while it is visible. Hiding the timer resets the amount. Refreshes based on user-defined Refresh Interval."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayWidgetTypeEnum.TimerTrain, "Shows a timer that counts down and can be increased by performing certain actions in your channel. Refreshes based on user-defined Refresh Interval & is only shown when total time exceeds the Minimum Seconds To Show value."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayWidgetTypeEnum.Video, "Shows a video. Refreshes based on user-defined Refresh Interval."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayWidgetTypeEnum.WebPage, "Shows a web page. Refreshes based on user-defined Refresh Interval."));
            this.OverlayTypeListings.Add(new OverlayTypeListing(OverlayWidgetTypeEnum.YouTube, "Shows a YouTube video. Refreshes based on user-defined Refresh Interval."));

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
            foreach (string overlayEndpoint in ChannelSession.Services.OverlayServers.GetOverlayNames())
            {
                this.OverlayEndpoints.Add(overlayEndpoint);
            }
            this.SelectedOverlayEndpoint = ChannelSession.Services.OverlayServers.DefaultOverlayName;
        }
    }
}
