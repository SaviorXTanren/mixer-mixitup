using MixItUp.Base.Model.Remote.Authentication;
using MixItUp.Base.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Settings
{
    public class RemoteConnectionUIViewModel : ViewModelBase
    {
        public const string StreamerDeviceType = "Streamer";
        public const string NormalDeviceType = "Normal";

        public RemoteConnectionModel Connection { get; private set; }

        public string Name { get { return this.Connection.Name; } }

        public string Status { get { return this.Connection.IsTemporary ? "1-Time" : "Permanent"; } }

        public IEnumerable<string> DeviceTypes { get { return new List<string>() { NormalDeviceType, StreamerDeviceType }; } }

        public string DeviceType
        {
            get { return (this.Connection.IsStreamer) ? StreamerDeviceType : NormalDeviceType; }
            set
            {
                if (!string.IsNullOrEmpty(value) && value.Equals(StreamerDeviceType))
                {
                    this.Connection.IsStreamer = true;
                }
                else
                {
                    this.Connection.IsStreamer = false;
                }
                this.NotifyPropertyChanged();
            }
        }

        public ICommand DeleteCommand { get; set; }

        private RemoteSettingsControlViewModel parent;

        public RemoteConnectionUIViewModel(RemoteSettingsControlViewModel parent, RemoteConnectionModel connection)
        {
            this.parent = parent;
            this.Connection = connection;

            this.DeleteCommand = this.CreateCommand(async (parameter) =>
            {
                await ChannelSession.Services.RemoteService.RemoveClient(ChannelSession.Settings.RemoteHostConnection, this.Connection);
                ChannelSession.Settings.RemoteClientConnections.Remove(this.Connection);
                this.parent.RefreshList();
            });
        }
    }

    public class RemoteSettingsControlViewModel : ViewModelBase
    {
        public ObservableCollection<RemoteConnectionUIViewModel> Connections { get; set; } = new ObservableCollection<RemoteConnectionUIViewModel>();

        public RemoteSettingsControlViewModel()
        {
            this.RefreshList();
        }

        public void RefreshList()
        {
            this.Connections.Clear();
            foreach (RemoteConnectionModel connection in ChannelSession.Settings.RemoteClientConnections)
            {
                this.Connections.Add(new RemoteConnectionUIViewModel(this, connection));
            }
        }
    }
}
