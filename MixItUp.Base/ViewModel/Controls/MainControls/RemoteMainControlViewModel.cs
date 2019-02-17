using MixItUp.Base.Remote.Models;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Remote;
using MixItUp.Base.ViewModel.Remote.Items;
using MixItUp.Base.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;
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
            }
        }
        private RemoteBoardViewModel board;

        public RemoteItemViewModelBase Item
        {
            get { return this.item; }
            private set
            {
                this.item = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsItemSelected");
            }
        }
        private RemoteItemViewModelBase item;

        public bool IsProfileSelected { get { return this.Profile != null; } }
        public bool IsItemSelected { get { return this.Item != null; } }

        public ICommand AddProfileCommand { get; private set; }
        public ICommand DeleteProfileCommand { get; private set; }

        public RemoteMainControlViewModel()
        {
            this.AddProfileCommand = this.CreateCommand(async (x) =>
            {
                string name = await DialogHelper.ShowTextEntry("Name of Profile:");
                if (!string.IsNullOrEmpty(name))
                {
                    if (ChannelSession.Settings.RemoteProfiles.Values.Any(p => p.Profile.Name.Equals(name)))
                    {
                        await DialogHelper.ShowMessage("A profile with the same name already exists");
                        return;
                    }

                    RemoteProfileModel profile = new RemoteProfileModel(name.ToString());
                    ChannelSession.Settings.RemoteProfiles[profile.ID] = new RemoteProfileBoardModel(profile);

                    this.RefreshProfiles();
                    this.ProfileSelected(this.Profiles.FirstOrDefault(p => p.ID.Equals(profile.ID)));
                }
            });

            this.DeleteProfileCommand = this.CreateCommand(async (x) =>
            {
                if (this.Profile != null)
                {
                    if (await DialogHelper.ShowConfirmation("Are you sure you want to delete this profile?"))
                    {
                        ChannelSession.Settings.RemoteProfiles.Remove(this.Profile.ID);
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
                    this.Item = this.Board.GetItem(command.ID);
                }
            });

            MessageCenter.Register<RemoteFolderItemViewModel>(RemoteFolderItemViewModel.NewRemoteFolderEventName, (folder) =>
            {
                if (this.Board != null)
                {
                    this.Board.AddItem(folder);
                    this.Item = this.Board.GetItem(folder.ID);
                }
            });

            MessageCenter.Register<RemoteCommandItemViewModel>(RemoteCommandItemViewModel.RemoteCommandDetailsEventName, (command) =>
            {
                this.Item = command;
            });

            MessageCenter.Register<RemoteFolderItemViewModel>(RemoteFolderItemViewModel.RemoteFolderDetailsEventName, (folder) =>
            {
                this.Item = folder;
            });
        }

        public void RefreshProfiles()
        {
            this.Profiles.Clear();
            foreach (RemoteProfileBoardModel profileBoard in ChannelSession.Settings.RemoteProfiles.Values)
            {
                this.Profiles.Add(new RemoteProfileViewModel(profileBoard.Profile));
            }
        }

        public void ProfileSelected(RemoteProfileViewModel profile)
        {
            this.Profile = null;
            this.Board = null;
            if (ChannelSession.Settings.RemoteProfiles.ContainsKey(profile.ID))
            {
                this.Profile = profile;
                RemoteProfileBoardModel profileBoard = ChannelSession.Settings.RemoteProfiles[profile.ID];
                this.Board = new RemoteBoardViewModel(profileBoard.Board);
            }
        }
    }
}
