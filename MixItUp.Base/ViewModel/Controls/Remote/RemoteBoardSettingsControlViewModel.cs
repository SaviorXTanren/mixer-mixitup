using MixItUp.Base.ViewModel.Remote;
using MixItUp.Base.ViewModels;
using System.Collections.Generic;

namespace MixItUp.Base.ViewModel.Controls.Remote
{
    public class RemoteBoardSettingsControlViewModel : ControlViewModelBase
    {
        public const string StreamerProfileType = "Streamer";
        public const string NormalProfileType = "Normal";

        public RemoteBoardSettingsControlViewModel(RemoteProfileViewModel profile, RemoteBoardViewModel board)
        {
            this.Profile = profile;
            this.Board = board;
        }

        public RemoteProfileViewModel Profile
        {
            get { return this.profile; }
            private set
            {
                this.profile = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ProfileType");
            }
        }
        private RemoteProfileViewModel profile;

        public RemoteBoardViewModel Board
        {
            get { return this.board; }
            private set
            {
                this.board = value;
                this.NotifyPropertyChanged();
            }
        }
        private RemoteBoardViewModel board;

        public IEnumerable<string> ProfileTypes { get { return new List<string>() { NormalProfileType, StreamerProfileType }; } }

        public string ProfileType
        {
            get
            {
                if (this.Profile != null)
                {
                    return (this.Profile.IsStreamer) ? StreamerProfileType : NormalProfileType;
                }
                return null;
            }
            set
            {
                if (this.Profile != null)
                {
                    if (!string.IsNullOrEmpty(value) && value.Equals(StreamerProfileType))
                    {
                        this.Profile.IsStreamer = true;
                    }
                    else
                    {
                        this.Profile.IsStreamer = false;
                    }
                }
                this.NotifyPropertyChanged();
            }
        }
    }
}
