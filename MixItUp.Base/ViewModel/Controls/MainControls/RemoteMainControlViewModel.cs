using MixItUp.Base.Model.Remote.Authentication;
using MixItUp.Base.Remote.Models;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Controls.Remote.Items;
using MixItUp.Base.ViewModel.Remote;
using MixItUp.Base.ViewModel.Remote.Items;
using MixItUp.Base.ViewModel.Window;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.MainControls
{
    public class RemoteMainControlViewModel : WindowControlViewModelBase
    {
        public const string StreamerProfileType = "Streamer";
        public const string NormalProfileType = "Normal";

        private RemoteItemControlViewModelBase[,] items = new RemoteItemControlViewModelBase[RemoteBoardModel.BoardWidth, RemoteBoardModel.BoardHeight];

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

        public RemoteItemControlViewModelBase Item00 { get { return this.items[0, 0]; } }
        public RemoteItemControlViewModelBase Item10 { get { return this.items[1, 0]; } }
        public RemoteItemControlViewModelBase Item20 { get { return this.items[2, 0]; } }
        public RemoteItemControlViewModelBase Item30 { get { return this.items[3, 0]; } }
        public RemoteItemControlViewModelBase Item40 { get { return this.items[4, 0]; } }

        public RemoteItemControlViewModelBase Item01 { get { return this.items[0, 1]; } }
        public RemoteItemControlViewModelBase Item11 { get { return this.items[1, 1]; } }
        public RemoteItemControlViewModelBase Item21 { get { return this.items[2, 1]; } }
        public RemoteItemControlViewModelBase Item31 { get { return this.items[3, 1]; } }
        public RemoteItemControlViewModelBase Item41 { get { return this.items[4, 1]; } }

        public RemoteItemControlViewModelBase Item02 { get { return this.items[0, 2]; } }
        public RemoteItemControlViewModelBase Item12 { get { return this.items[1, 2]; } }
        public RemoteItemControlViewModelBase Item22 { get { return this.items[2, 2]; } }
        public RemoteItemControlViewModelBase Item32 { get { return this.items[3, 2]; } }
        public RemoteItemControlViewModelBase Item42 { get { return this.items[4, 2]; } }

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

        public RemoteItemControlViewModelBase Item
        {
            get { return this.item; }
            private set
            {
                this.item = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsItemSelected");
            }
        }
        private RemoteItemControlViewModelBase item;

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
        public ICommand DownloadAppleCommand { get; private set; }
        public ICommand DownloadAndroidCommand { get; private set; }

        public RemoteMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            this.AddProfileCommand = this.CreateCommand(async (x) =>
            {
                string name = await DialogHelper.ShowTextEntry($"{MixItUp.Base.Resources.NameOfProfile}:");
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
                        this.Item = null;
                    }
                }
            });

            this.ConnectDeviceCommand = this.CreateCommand(async (parameter) =>
            {
                if (ChannelSession.Settings.RemoteHostConnection == null || (!await ChannelSession.Services.RemoteService.ValidateConnection(ChannelSession.Settings.RemoteHostConnection)))
                {
                    ChannelSession.Settings.RemoteHostConnection = await ChannelSession.Services.RemoteService.NewHost(ChannelSession.MixerChannel.token);
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
                            await DialogHelper.ShowMessage(string.Format("The device {0} has been approved." + Environment.NewLine + Environment.NewLine +
                                "To configure it, head to Settings -> Remote.", clientConnection.Name));
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

            this.DownloadAppleCommand = this.CreateCommand((x) =>
            {
                ProcessHelper.LaunchLink("https://itunes.apple.com/us/app/mix-it-up-remote/id1459364145");
                return Task.FromResult(0);
            });

            this.DownloadAndroidCommand = this.CreateCommand((x) =>
            {
                ProcessHelper.LaunchLink("https://play.google.com/store/apps/details?id=com.MixItUpApp.Remote.Beta");
                return Task.FromResult(0);
            });

            MessageCenter.Register<RemoteCommandItemViewModel>(RemoteEmptyItemControlViewModel.NewRemoteCommandEventName, this, (command) =>
            {
                if (this.Board != null)
                {
                    this.Board.AddItem(command);
                    this.RefreshBoardItem(command.XPosition, command.YPosition);
                    this.Item = this.GetItem(command.XPosition, command.YPosition);
                }
            });

            MessageCenter.Register<RemoteFolderItemViewModel>(RemoteEmptyItemControlViewModel.NewRemoteFolderEventName, this, async (folder) =>
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
                    this.RefreshBoardItem(folder.XPosition, folder.YPosition);
                    this.Item = this.GetItem(folder.XPosition, folder.YPosition);
                }
            });

            MessageCenter.Register<RemoteCommandItemControlViewModel>(RemoteCommandItemControlViewModel.RemoteCommandDetailsEventName, this, (command) =>
            {
                this.Item = command;
            });

            MessageCenter.Register<RemoteFolderItemControlViewModel>(RemoteFolderItemControlViewModel.RemoteFolderDetailsEventName, this, (folder) =>
            {
                this.Item = folder;
            });

            MessageCenter.Register<RemoteFolderItemViewModel>(RemoteFolderItemControlViewModel.RemoteFolderNavigationEventName, this, (folder) =>
            {
                if (ChannelSession.Settings.RemoteProfileBoards[this.Profile.ID].Boards.ContainsKey(folder.BoardID))
                {
                    this.Board = new RemoteBoardViewModel(ChannelSession.Settings.RemoteProfileBoards[this.Profile.ID].Boards[folder.BoardID], this.Board);
                    this.RefreshBoard();
                    this.AddRemoveNavigationName(folder.Name);
                    this.Item = null;
                }
            });

            MessageCenter.Register<RemoteBoardViewModel>(RemoteBackItemControlViewModel.RemoteBackNavigationEventName, this, (board) =>
            {
                this.Board = board;
                this.RefreshBoard();
                this.AddRemoveNavigationName(null);
                this.Item = null;
            });

            MessageCenter.Register<RemoteItemViewModelBase>(RemoteItemControlViewModelBase.RemoteDeleteItemEventName, this, (item) =>
            {
                this.Board.RemoveItem(item.XPosition, item.YPosition);
                this.RefreshBoardItem(item.XPosition, item.YPosition);
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
            this.Board = null;

            this.NavigationNames.Clear();
            this.NotifyPropertyChanged("NavigationName");

            if (profile != null && ChannelSession.Settings.RemoteProfileBoards.ContainsKey(profile.ID))
            {
                this.Profile = profile;
                RemoteProfileBoardsModel profileBoards = ChannelSession.Settings.RemoteProfileBoards[profile.ID];
                this.Board = new RemoteBoardViewModel(profileBoards.Boards[Guid.Empty]);
                this.AddRemoveNavigationName(this.Profile.Name);
            }
            this.RefreshBoard();
        }

        public void RefreshBoard()
        {
            for (int x = 0; x < RemoteBoardModel.BoardWidth; x++)
            {
                for (int y = 0; y < RemoteBoardModel.BoardHeight; y++)
                {
                    this.RefreshBoardItem(x, y);
                }
            }
        }

        public void RefreshBoardItem(int x, int y)
        {
            this.items[x, y] = null;

            if (this.Board != null)
            {
                RemoteItemViewModelBase item = this.Board.GetItem(x, y);
                if (item != null)
                {
                    if (item is RemoteCommandItemViewModel)
                    {
                        this.items[x, y] = new RemoteCommandItemControlViewModel((RemoteCommandItemViewModel)item);
                    }
                    else if (item is RemoteFolderItemViewModel)
                    {
                        this.items[x, y] = new RemoteFolderItemControlViewModel((RemoteFolderItemViewModel)item);
                    }
                    else if (item is RemoteBackItemViewModel)
                    {
                        this.items[x, y] = new RemoteBackItemControlViewModel((RemoteBackItemViewModel)item, this.Board.ParentBoard);
                    }
                    else if (item is RemoteEmptyItemViewModel)
                    {
                        this.items[x, y] = new RemoteEmptyItemControlViewModel((RemoteEmptyItemViewModel)item);
                    }
                }
            }

            this.NotifyPropertyChanged("Item" + x + y);
        }

        public RemoteItemControlViewModelBase GetItem(int xPosition, int yPosition) { return this.items[xPosition, yPosition]; }

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
