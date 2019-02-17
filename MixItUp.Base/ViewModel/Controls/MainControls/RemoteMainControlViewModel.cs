using MixItUp.Base.Remote.Models;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Remote;
using MixItUp.Base.ViewModel.Remote.Items;
using MixItUp.Base.ViewModels;
using System.Collections.Generic;
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

        public List<string> NavigationNames
        {
            get { return this.navigationNames; }
            set
            {
                this.navigationNames = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("NavigationName");
            }
        }
        private List<string> navigationNames = new List<string>();

        public string NavigationName { get { return string.Join(" > ", this.NavigationNames); } }

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

            MessageCenter.Register<RemoteFolderItemViewModel>(RemoteFolderItemViewModel.NewRemoteFolderEventName, async (folder) =>
            {
                if (this.NavigationNames.Count() > 2)
                {
                    await DialogHelper.ShowMessage("Boards can only be up to 2 layers deep");
                    return;
                }

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

            MessageCenter.Register<RemoteFolderItemViewModel>(RemoteFolderItemViewModel.RemoteFolderNavigationEventName, (folder) =>
            {
                this.Board = new RemoteBoardViewModel(folder.Board.GetModel(), this.Board);
                this.AddRemoveNavigationName(folder.Name);
                this.Item = null;
            });

            MessageCenter.Register<RemoteBoardViewModel>(RemoteBackItemViewModel.RemoteBackNavigationEventName, (board) =>
            {
                this.Board = board;
                this.AddRemoveNavigationName(null);
                this.Item = null;
            });

            MessageCenter.Register<RemoteItemViewModelBase>(RemoteItemViewModelBase.RemoteDeleteItemEventName, (item) =>
            {
                this.Board.RemoveItem(item.XPosition, item.YPosition);
                this.Item = null;
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
            this.NavigationNames.Clear();
            if (profile != null && ChannelSession.Settings.RemoteProfiles.ContainsKey(profile.ID))
            {
                this.Profile = profile;
                RemoteProfileBoardModel profileBoard = ChannelSession.Settings.RemoteProfiles[profile.ID];
                this.Board = new RemoteBoardViewModel(profileBoard.Board);

                this.AddRemoveNavigationName(this.Profile.Name);
            }
        }

        public void AddRemoveNavigationName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                this.NavigationNames.Add(name);
            }
            else
            {
                this.NavigationNames.RemoveAt(this.NavigationNames.Count - 1);
            }
            this.NotifyPropertyChanged("NavigationName");
        }
    }
}
