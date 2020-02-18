using Mixer.Base.Model.MixPlay;
using MixItUp.Base.Model.MixPlay;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.MixPlay;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModel.Window;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.MainControls
{
    public class MixPlayMainControlViewModel : WindowControlViewModelBase
    {
        public ICommand MixerLabCommand { get; private set; }

        public ObservableCollection<MixPlayGameModel> Games { get; private set; } = new ObservableCollection<MixPlayGameModel>();
        public MixPlayGameModel SelectedGame
        {
            get { return this.selectedGame; }
            set
            {
                this.selectedGame = value;

                this.Scenes.Clear();
                this.Controls.Clear();

                if (this.SelectedGame != null)
                {
                    this.LoadSelectedGame.Execute(null);
                }

                this.NotifyAllProperties();
            }
        }
        private MixPlayGameModel selectedGame;
        public bool IsGameSelected { get { return this.SelectedGame != null; } }

        public ICommand LoadSelectedGame { get; private set; }

        public MixPlayGameVersionModel SelectedGameVersion { get; private set; }

        public ObservableCollection<MixPlaySceneModel> Scenes { get; private set; } = new ObservableCollection<MixPlaySceneModel>();
        public MixPlaySceneModel SelectedScene
        {
            get { return this.selectedScene; }
            set
            {
                this.selectedScene = value;
                this.RefreshControls();
            }
        }
        private MixPlaySceneModel selectedScene;

        public ObservableCollection<MixPlayControlViewModel> Controls { get; private set; } = new ObservableCollection<MixPlayControlViewModel>();

        public ICommand RefreshCommand { get; private set; }

        public bool IsNormalMixPlay { get { return this.CustomMixPlayControl == null; } }

        public event EventHandler CustomMixPlaySelected = delegate { };
        public object CustomMixPlayControl
        {
            get { return this.customMixPlayControl; }
            set
            {
                this.customMixPlayControl = value;
                this.NotifyAllProperties();
            }
        }
        private object customMixPlayControl;
        public bool IsCustomMixPlay { get { return this.CustomMixPlayControl != null; } }

        public string ContentDisconnectContent { get { return (ChannelSession.Services.MixPlay.IsConnected) ? Resources.Disconnect : Resources.Connect; } }
        public ICommand ConnectDisconnectCommand { get; private set; }

        public MixPlayMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            GlobalEvents.OnInteractiveSharedProjectAdded += GlobalEvents_OnInteractiveSharedProjectAdded;
            GlobalEvents.OnInteractiveConnected += GlobalEvents_OnInteractiveConnected;
            GlobalEvents.OnInteractiveDisconnected += GlobalEvents_OnInteractiveDisconnected;

            this.MixerLabCommand = this.CreateCommand((parameter) =>
            {
                ProcessHelper.LaunchLink("https://mixer.com/lab/");
                return Task.FromResult(0);
            });

            this.LoadSelectedGame = this.CreateCommand(async (parameter) =>
            {
                if (!ChannelSession.Settings.MixPlayUserGroups.ContainsKey(this.selectedGame.id))
                {
                    ChannelSession.Settings.MixPlayUserGroups[this.selectedGame.id] = new List<MixPlayUserGroupModel>();
                }

                foreach (UserRoleEnum role in UserViewModel.SelectableBasicUserRoles())
                {
                    if (!ChannelSession.Settings.MixPlayUserGroups[this.selectedGame.id].Any(ug => ug.AssociatedUserRole == role))
                    {
                        ChannelSession.Settings.MixPlayUserGroups[this.selectedGame.id].Add(new MixPlayUserGroupModel(role));
                    }
                }

                IEnumerable<MixPlayGameVersionModel> versions = await ChannelSession.MixerUserConnection.GetMixPlayGameVersions(this.selectedGame);
                if (versions != null && versions.Count() > 0)
                {
                    this.SelectedGameVersion = await ChannelSession.MixerUserConnection.GetMixPlayGameVersion(versions.First());
                    if (this.SelectedGameVersion != null)
                    {
                        if (MixPlaySharedProjectModel.AllMixPlayProjects.Any(p => p.GameID.Equals(this.SelectedGame.id)))
                        {
                            this.CustomMixPlaySelected(this, new EventArgs());
                        }
                        else
                        {
                            this.CustomMixPlayControl = null;
                            foreach (MixPlaySceneModel scene in this.SelectedGameVersion.controls.scenes.OrderBy(s => s.sceneID))
                            {
                                this.Scenes.Add(scene);
                            }

                            if (this.Scenes.Count > 0)
                            {
                                this.SelectedScene = this.Scenes.FirstOrDefault(s => s.sceneID.Equals("default"));
                                if (this.SelectedScene == null)
                                {
                                    this.SelectedScene = this.Scenes.FirstOrDefault();
                                }
                            }
                        }
                    }
                }
            });

            this.RefreshCommand = this.CreateCommand(async (parameter) =>
            {
                await this.RefreshGames();
            });

            this.ConnectDisconnectCommand = this.CreateCommand(async (parameter) =>
            {
                if (!ChannelSession.Services.MixPlay.IsConnected)
                {
                    await ChannelSession.Services.MixPlay.SetGame(this.SelectedGame);
                    ExternalServiceResult result = await ChannelSession.Services.MixPlay.Connect();
                    if (!result.Success)
                    {
                        await DialogHelper.ShowMessage(result.Message);
                    }
                }
                else
                {
                    await ChannelSession.Services.MixPlay.Disconnect();
                }
                this.NotifyAllProperties();
            });
        }

        public void RefreshControls()
        {
            this.Controls.Clear();
            if (this.SelectedScene != null)
            {
                List<MixPlayControlViewModel> controls = new List<MixPlayControlViewModel>();
                foreach (MixPlayButtonControlModel control in this.SelectedScene.buttons)
                {
                    controls.Add(new MixPlayControlViewModel(this.SelectedGame, control));
                }
                foreach (MixPlayJoystickControlModel control in this.SelectedScene.joysticks)
                {
                    controls.Add(new MixPlayControlViewModel(this.SelectedGame, control));
                }
                foreach (MixPlayTextBoxControlModel control in this.SelectedScene.textBoxes)
                {
                    controls.Add(new MixPlayControlViewModel(this.SelectedGame, control));
                }

                foreach (MixPlayControlViewModel control in controls.OrderBy(c => c.Name))
                {
                    this.Controls.Add(control);
                }
            }
            this.NotifyAllProperties();
        }

        protected override async Task OnLoadedInternal()
        {
            await this.RefreshGames();

            await base.OnVisibleInternal();
        }

        protected override async Task OnVisibleInternal()
        {
            this.NotifyAllProperties();

            await base.OnVisibleInternal();
        }

        private async Task RefreshGames()
        {
            this.SelectedGame = null;

            this.Games.Clear();
            foreach (MixPlayGameModel game in await ChannelSession.Services.MixPlay.GetAllGames())
            {
                this.Games.Add(game);
            }

            MixPlayGameModel defaultGame = this.Games.FirstOrDefault(g => g.id.Equals(ChannelSession.Settings.DefaultMixPlayGame));
            if (defaultGame != null)
            {
                this.SelectedGame = defaultGame;
            }

            this.NotifyAllProperties();
        }

        private void NotifyAllProperties()
        {
            this.NotifyPropertyChanged("SelectedGame");
            this.NotifyPropertyChanged("IsGameSelected");
            this.NotifyPropertyChanged("IsNormalMixPlay");
            this.NotifyPropertyChanged("IsCustomMixPlay");
            this.NotifyPropertyChanged("CustomMixPlayControl");
            this.NotifyPropertyChanged("SelectedScene");
            this.NotifyPropertyChanged("ContentDisconnectContent");
        }

        private async void GlobalEvents_OnInteractiveSharedProjectAdded(object sender, MixPlaySharedProjectModel e)
        {
            await DispatcherHelper.InvokeDispatcher(async () =>
            {
                await this.RefreshGames();
            });
        }

        private async void GlobalEvents_OnInteractiveConnected(object sender, MixPlayGameModel game)
        {
            await DispatcherHelper.InvokeDispatcher(() =>
            {
                this.SelectedGame = game;
                this.NotifyAllProperties();
                return Task.FromResult(0);
            });
        }

        private void GlobalEvents_OnInteractiveDisconnected(object sender, EventArgs e)
        {
            this.NotifyAllProperties();
        }
    }
}
