using MixItUp.Base.Model.SongRequests;

namespace MixItUp.Base.ViewModel.Controls.Settings
{
    public class SongRequestsSettingsControlViewModel : ControlViewModelBase
    {
        public bool SpotifyProvider
        {
            get { return ChannelSession.Settings.SongRequestServiceTypes.Contains(SongRequestServiceTypeEnum.Spotify); }
            set
            {
                if (value)
                {
                    ChannelSession.Settings.SongRequestServiceTypes.Add(SongRequestServiceTypeEnum.Spotify);
                }
                else
                {
                    ChannelSession.Settings.SongRequestServiceTypes.Remove(SongRequestServiceTypeEnum.Spotify);
                }
                this.NotifyPropertyChanged();
            }
        }

        public bool YouTubeProvider
        {
            get { return ChannelSession.Settings.SongRequestServiceTypes.Contains(SongRequestServiceTypeEnum.YouTube); }
            set
            {
                if (value)
                {
                    ChannelSession.Settings.SongRequestServiceTypes.Add(SongRequestServiceTypeEnum.YouTube);
                }
                else
                {
                    ChannelSession.Settings.SongRequestServiceTypes.Remove(SongRequestServiceTypeEnum.YouTube);
                }
                this.NotifyPropertyChanged();
            }
        }

        public bool SpotifyAllowExplicit
        {
            get { return ChannelSession.Settings.SpotifyAllowExplicit; }
            set
            {
                ChannelSession.Settings.SpotifyAllowExplicit = value;
                this.NotifyPropertyChanged();
            }
        }

        public string DefaultPlaylist
        {
            get { return ChannelSession.Settings.DefaultPlaylist; }
            set
            {
                ChannelSession.Settings.DefaultPlaylist = value;
                this.NotifyPropertyChanged();
            }
        }

        public bool SubPriority
        {
            get { return ChannelSession.Settings.SongRequestSubPriority; }
            set
            {
                ChannelSession.Settings.SongRequestSubPriority = value;
                this.NotifyPropertyChanged();
            }
        }

        public string MaxRequests
        {
            get { return (ChannelSession.Settings.SongRequestsMaxRequests > 0) ? ChannelSession.Settings.SongRequestsMaxRequests.ToString() : string.Empty; }
            set
            {
                if (int.TryParse(value, out int max) && max > 0)
                {
                    ChannelSession.Settings.SongRequestsMaxRequests = max;
                }
                else
                {
                    ChannelSession.Settings.SongRequestsMaxRequests = 0;
                }
                this.NotifyPropertyChanged();
            }
        }

        public bool SaveRequestQueue
        {
            get { return ChannelSession.Settings.SongRequestsSaveRequestQueue; }
            set
            {
                ChannelSession.Settings.SongRequestsSaveRequestQueue = value;
                this.NotifyPropertyChanged();
            }
        }

        public SongRequestsSettingsControlViewModel() { }
    }
}
