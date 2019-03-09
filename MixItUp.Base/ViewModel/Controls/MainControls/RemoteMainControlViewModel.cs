using MixItUp.Base.Model.Remote.Authentication;
using MixItUp.Base.Remote.Models;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Remote;
using MixItUp.Base.ViewModel.Remote.Items;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.MainControls
{
    public class RemoteMainControlViewModel : ViewModelBase
    {
        public const string StreamerProfileType = "Streamer";
        public const string NormalProfileType = "Normal";

        public ObservableCollection<RemoteProfileViewModel> Profiles { get; private set; } = new ObservableCollection<RemoteProfileViewModel>();

        public RemoteProfileViewModel Profile
        {
            get { return this.profile; }
            private set
            {
                this.profile = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsProfileSelected");
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
        public ICommand ConnectDeviceCommand { get; private set; }

        public RemoteMainControlViewModel()
        {
            this.AddProfileCommand = this.CreateCommand(async (x) =>
            {
                string name = await DialogHelper.ShowTextEntry("Name of Profile:");
                if (!string.IsNullOrEmpty(name))
                {
                    if (ChannelSession.Settings.RemoteProfiles.Any(p => p.Name.Equals(name)))
                    {
                        await DialogHelper.ShowMessage("A profile with the same name already exists");
                        return;
                    }

                    RemoteProfileModel profile = new RemoteProfileModel(name.ToString());
                    ChannelSession.Settings.RemoteProfiles.Add(profile);
                    ChannelSession.Settings.RemoteProfileBoards[profile.ID] = new RemoteProfileBoardsModel(profile.ID);

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
                        ChannelSession.Settings.RemoteProfiles.Remove(this.Profile.GetModel());
                        ChannelSession.Settings.RemoteProfileBoards.Remove(this.profile.ID);
                        this.RefreshProfiles();
                        this.ProfileSelected(null);
                    }
                }
            });

            this.ConnectDeviceCommand = this.CreateCommand(async (parameter) =>
            {
                if (ChannelSession.Settings.RemoteHostConnection == null || (!await ChannelSession.Services.RemoteService.ValidateConnection(ChannelSession.Settings.RemoteHostConnection)))
                {
                    ChannelSession.Settings.RemoteHostConnection = await ChannelSession.Services.RemoteService.NewHost(ChannelSession.User.username);
                    ChannelSession.Settings.RemoteClientConnections.Clear();
                }

                if (ChannelSession.Settings.RemoteHostConnection != null)
                {
                    if (!ChannelSession.Services.RemoteService.IsConnected)
                    {
                        if (!await ChannelSession.Services.RemoteService.InitializeConnection(ChannelSession.Settings.RemoteHostConnection))
                        {
                            await DialogHelper.ShowMessage("Could not connect to Remote service, please try again");
                            return;
                        }
                    }

                    string shortCode = await DialogHelper.ShowTextEntry("Device 6-Digit Code:");
                    if (!string.IsNullOrEmpty(shortCode))
                    {
                        if (shortCode.Length != 6)
                        {
                            await DialogHelper.ShowMessage("The code entered is not valid");
                            return;
                        }

                        RemoteConnectionModel clientConnection = await ChannelSession.Services.RemoteService.ApproveClient(ChannelSession.Settings.RemoteHostConnection, shortCode, rememberClient: true);
                        if (clientConnection != null)
                        {
                            if (!clientConnection.IsTemporary)
                            {
                                ChannelSession.Settings.RemoteClientConnections.Add(clientConnection);
                            }
                            await DialogHelper.ShowMessage(string.Format("The client device {0} has been approved", clientConnection.Name));
                        }
                        else
                        {
                            await DialogHelper.ShowMessage("A client device could not be found with the specified code");
                            return;
                        }
                    }
                }
                else
                {
                    await DialogHelper.ShowMessage("Could not connect to Remote service, please try again");
                }
            });

            MessageCenter.Register<RemoteCommandItemViewModel>(RemoteCommandItemViewModel.NewRemoteCommandEventName, this, (command) =>
            {
                if (this.Board != null)
                {
                    this.Board.AddItem(command);
                    this.Item = this.Board.GetItem(command.ID);
                }
            });

            MessageCenter.Register<RemoteFolderItemViewModel>(RemoteFolderItemViewModel.NewRemoteFolderEventName, this, async (folder) =>
            {
                if (this.NavigationNames.Count() > 2)
                {
                    await DialogHelper.ShowMessage("Boards can only be up to 2 layers deep");
                    return;
                }

                if (this.Board != null)
                {
                    RemoteBoardModel newBoard = new RemoteBoardModel(isSubBoard: true);
                    folder.BoardID = newBoard.ID;
                    ChannelSession.Settings.RemoteProfileBoards[this.Profile.ID].Boards[folder.BoardID] = newBoard;

                    this.Board.AddItem(folder);
                    this.Item = this.Board.GetItem(folder.ID);
                }
            });

            MessageCenter.Register<RemoteCommandItemViewModel>(RemoteCommandItemViewModel.RemoteCommandDetailsEventName, this, (command) =>
            {
                this.Item = command;
            });

            MessageCenter.Register<RemoteFolderItemViewModel>(RemoteFolderItemViewModel.RemoteFolderDetailsEventName, this, (folder) =>
            {
                this.Item = folder;
            });

            MessageCenter.Register<RemoteFolderItemViewModel>(RemoteFolderItemViewModel.RemoteFolderNavigationEventName, this, (folder) =>
            {
                if (ChannelSession.Settings.RemoteProfileBoards[this.Profile.ID].Boards.ContainsKey(folder.BoardID))
                {
                    this.Board = new RemoteBoardViewModel(ChannelSession.Settings.RemoteProfileBoards[this.Profile.ID].Boards[folder.BoardID], this.Board);
                    this.AddRemoveNavigationName(folder.Name);
                    this.Item = null;
                }
            });

            MessageCenter.Register<RemoteBoardViewModel>(RemoteBackItemViewModel.RemoteBackNavigationEventName, this, (board) =>
            {
                this.Board = board;
                this.AddRemoveNavigationName(null);
                this.Item = null;
            });

            MessageCenter.Register<RemoteItemViewModelBase>(RemoteItemViewModelBase.RemoteDeleteItemEventName, this, (item) =>
            {
                this.Board.RemoveItem(item.XPosition, item.YPosition);
                this.Item = null;
            });
        }

        public void RefreshProfiles()
        {
            this.Profiles.Clear();
            foreach (RemoteProfileModel profile in ChannelSession.Settings.RemoteProfiles)
            {
                this.Profiles.Add(new RemoteProfileViewModel(profile));
            }
        }

        public void ProfileSelected(RemoteProfileViewModel profile)
        {
            this.Profile = null;
            this.NavigationNames.Clear();
            if (profile != null && ChannelSession.Settings.RemoteProfileBoards.ContainsKey(profile.ID))
            {
                this.Profile = profile;
                RemoteProfileBoardsModel profileBoards = ChannelSession.Settings.RemoteProfileBoards[profile.ID];
                this.Board = new RemoteBoardViewModel(profileBoards.Boards[Guid.Empty]);

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
