using MixItUp.Base.Remote.Models;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Remote;
using MixItUp.Base.ViewModel.Remote.Items;
using MixItUp.Base.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.MainControls
{
    public class RemoteMainControlViewModel : ViewModelBase
    {
        public ObservableCollection<RemoteProfileViewModel> Profiles { get; private set; } = new ObservableCollection<RemoteProfileViewModel>();

        public RemoteProfileViewModel Profile
        {
            get { return this.profile; }
            private set
            {
                this.profile = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsProfileSelected");
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
                this.NotifyPropertyChanged("IsBoardSelected");
            }
        }
        private RemoteBoardViewModel board;

        public bool IsProfileSelected { get { return this.Profile != null; } }

        public ICommand AddProfileCommand { get; private set; }
        public ICommand DeleteProfileCommand { get; private set; }

        public RemoteMainControlViewModel()
        {
            this.AddProfileCommand = this.CreateCommand(async (x) =>
            {
                string name = await DialogHelper.ShowTextEntry("Name of Profile:");
                if (!string.IsNullOrEmpty(name))
                {
                    if (ChannelSession.Settings.RemoteProfiles.Keys.Any(p => p.Name.Equals(name)))
                    {
                        await DialogHelper.ShowMessage("A profile with the same name already exists");
                        return;
                    }

                    ChannelSession.Settings.RemoteProfiles[new RemoteProfileModel(name.ToString())] = new RemoteBoardModel();
                    this.RefreshProfiles();
                }
            });

            this.DeleteProfileCommand = this.CreateCommand(async (x) =>
            {
                if (this.Profile != null)
                {
                    if (await DialogHelper.ShowConfirmation("Are you sure you want to delete this profile?"))
                    {
                        ChannelSession.Settings.RemoteProfiles.Remove(this.Profile.GetModel());
                        this.RefreshProfiles();
                        this.ProfileSelected(null);
                    }
                }
            });

            MessageCenter.Register<RemoteCommandItemViewModel>(RemoteCommandItemViewModel.NewRemoteCommandEventName, (command) =>
            {
                if (this.Board != null)
                {
                    this.Board.AddItem(command);
                }
            });

            MessageCenter.Register<RemoteFolderItemViewModel>(RemoteFolderItemViewModel.NewRemoteFolderEventName, (folder) =>
            {
                if (this.Board != null)
                {
                    this.Board.AddItem(folder);
                }
            });
        }

        public void RefreshProfiles()
        {
            this.Profiles.Clear();
            foreach (RemoteProfileModel profile in ChannelSession.Settings.RemoteProfiles.Keys)
            {
                this.Profiles.Add(new RemoteProfileViewModel(profile));
            }
        }

        public void ProfileSelected(RemoteProfileViewModel profile)
        {
            this.Profile = null;
            this.Board = null;
            if (ChannelSession.Settings.RemoteProfiles.ContainsKey(profile.GetModel()))
            {
                this.Profile = profile;
                this.Board = new RemoteBoardViewModel(ChannelSession.Settings.RemoteProfiles[profile.GetModel()]);
            }
        }
    }
}
