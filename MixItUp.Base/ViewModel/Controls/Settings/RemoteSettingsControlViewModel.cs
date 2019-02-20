using MixItUp.Base.Model.Remote.Authentication;
using MixItUp.Base.ViewModels;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Settings
{
    public class RemoteConnectionUIViewModel : ViewModelBase
    {
        public RemoteConnectionModel Connection { get; private set; }

        public string Name { get { return this.Connection.Name; } }

        public string Status { get { return this.Connection.IsTemporary ? "1-Time" : "Permanent"; } }

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
