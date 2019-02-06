using Mixer.Base.Model.Interactive;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Interactive;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Interactive;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Controls.Dialogs;
using MixItUp.WPF.Controls.Interactive;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.MainControls
{
    public class InteractiveControlCommandItem : NotifyPropertyChangedBase
    {
        public InteractiveSceneModel Scene { get; set; }

        public InteractiveButtonControlModel Button { get; set; }
        public InteractiveJoystickControlModel Joystick { get; set; }
        public InteractiveTextBoxControlModel TextBox { get; set; }

        public InteractiveCommand Command { get; set; }
        public Visibility NewCommandButtonVisibility { get { return (this.Command == null) ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility ExistingCommandButtonVisibility { get { return (this.Command != null) ? Visibility.Visible : Visibility.Collapsed; } }

        public InteractiveControlCommandItem(InteractiveButtonControlModel button) { this.Button = button; }
        public InteractiveControlCommandItem(InteractiveJoystickControlModel joystick) { this.Joystick = joystick; }
        public InteractiveControlCommandItem(InteractiveTextBoxControlModel textBox) { this.TextBox = textBox; }

        public InteractiveControlCommandItem(InteractiveCommand command)
        {
            this.Command = command;
            if (command.Control is InteractiveButtonControlModel)
            {
                this.Button = (InteractiveButtonControlModel)command.Control;
            }
            else if(command.Control is InteractiveJoystickControlModel)
            {
                this.Joystick = (InteractiveJoystickControlModel)command.Control;
            }
            else if (command.Control is InteractiveTextBoxControlModel)
            {
                this.TextBox = (InteractiveTextBoxControlModel)command.Control;
            }
        }

        public InteractiveControlModel Control
        {
            get
            {
                if (this.Button != null) { return this.Button; }
                if (this.Joystick != null) { return this.Joystick; }
                if (this.TextBox != null) { return this.TextBox; }
                return null;
            }
        }

        public string Name { get { return this.Control.controlID; } }

        public string Type
        {
            get
            {
                if (this.Control is InteractiveButtonControlModel) { return "Button"; }
                if (this.Control is InteractiveJoystickControlModel) { return "Joystick"; }
                if (this.Control is InteractiveTextBoxControlModel) { return "Text Box"; }
                return "Unknown";
            }
        }

        public string SparkCost
        {
            get
            {
                if (this.Control is InteractiveButtonControlModel) { return this.Button.cost.ToString(); }
                if (this.Control is InteractiveTextBoxControlModel) { return this.TextBox.cost.ToString(); }
                return string.Empty;
            }
        }

        public string Cooldown
        {
            get
            {
                if (this.Command != null && this.Command.Requirements.Cooldown != null)
                {
                    if (this.Command.Requirements.Cooldown.IsGroup)
                    {
                        return this.Command.Requirements.Cooldown.GroupName;
                    }
                    return this.Command.Requirements.Cooldown.CooldownAmount.ToString();
                }
                return string.Empty;
            }
        }

        public string EventTypeString { get { return (this.Command != null) ? this.Command.EventTypeString : string.Empty; } }
    }

    /// <summary>
    /// Interaction logic for InteractiveControl.xaml
    /// </summary>
    public partial class InteractiveControl : MainControlBase
    {
        private ObservableCollection<InteractiveGameModel> interactiveGames = new ObservableCollection<InteractiveGameModel>();
        private InteractiveGameModel selectedGame = null;
        public InteractiveGameVersionModel selectedGameVersion = null;

        private ObservableCollection<InteractiveSceneModel> interactiveScenes = new ObservableCollection<InteractiveSceneModel>();
        private InteractiveSceneModel selectedScene = null;

        private ObservableCollection<InteractiveControlCommandItem> currentSceneControlItems = new ObservableCollection<InteractiveControlCommandItem>();

        public InteractiveControl()
        {
            InitializeComponent();

            GlobalEvents.OnInteractiveSharedProjectAdded += GlobalEvents_OnInteractiveSharedProjectAdded;
            GlobalEvents.OnInteractiveConnected += GlobalEvents_OnInteractiveConnected;
            GlobalEvents.OnInteractiveDisconnected += GlobalEvents_OnInteractiveDisconnected;
        }

        public bool IsCustomInteractiveGame { get { return (this.CustomInteractiveContentControl.Content != null); } }

        protected override async Task InitializeInternal()
        {
            this.InteractiveGamesComboBox.ItemsSource = this.interactiveGames;
            this.InteractiveScenesComboBox.ItemsSource = this.interactiveScenes;
            this.InteractiveControlsGridView.ItemsSource = this.currentSceneControlItems;

            await this.RefreshAllInteractiveGames();

            if (ChannelSession.Settings.DefaultInteractiveGame > 0)
            {
                InteractiveGameModel game = this.interactiveGames.FirstOrDefault(g => g.id.Equals(ChannelSession.Settings.DefaultInteractiveGame));
                if (game != null)
                {
                    this.InteractiveGamesComboBox.SelectedItem = game;
                }
            }
        }

        private async Task RefreshAllInteractiveGames(bool forceRefresh = false)
        {
            this.GroupsButton.IsEnabled = false;
            this.ConnectButton.IsEnabled = false;

            this.interactiveGames.Clear();

            IEnumerable<InteractiveGameModel> games = await this.Window.RunAsyncOperation(async () =>
            {
                return await ChannelSession.Interactive.GetAllConnectableGames(forceRefresh);
            });

            foreach (InteractiveGameModel game in games)
            {
                this.interactiveGames.Add(game);
            }

            if (this.selectedGame != null)
            {
                this.InteractiveGamesComboBox.SelectedItem = this.interactiveGames.FirstOrDefault(g => g.id.Equals(this.selectedGame.id));
            }
        }

        private async Task RefreshSelectedGame()
        {
            if (!ChannelSession.Settings.InteractiveUserGroups.ContainsKey(this.selectedGame.id))
            {
                ChannelSession.Settings.InteractiveUserGroups[this.selectedGame.id] = new List<InteractiveUserGroupViewModel>();
            }

            foreach (MixerRoleEnum role in UserViewModel.SelectableBasicUserRoles())
            {
                if (!ChannelSession.Settings.InteractiveUserGroups[this.selectedGame.id].Any(ug => ug.AssociatedUserRole == role))
                {
                    ChannelSession.Settings.InteractiveUserGroups[this.selectedGame.id].Add(new InteractiveUserGroupViewModel(role));
                }
            }

            this.selectedGameVersion = await this.Window.RunAsyncOperation(async () =>
            {
                IEnumerable<InteractiveGameVersionModel> versions = await ChannelSession.Connection.GetInteractiveGameVersions(this.selectedGame);
                if (versions != null && versions.Count() > 0)
                {
                    return await ChannelSession.Connection.GetInteractiveGameVersion(versions.First());
                }
                return null;
            });

            this.GroupsButton.IsEnabled = false;
            if (this.selectedGame.id == InteractiveSharedProjectModel.FortniteDropMap.GameID)
            {
                this.SetCustomInteractiveGame(new FortniteDropMapInteractiveControl(this.selectedGame, this.selectedGameVersion));
            }
            else if (this.selectedGame.id == InteractiveSharedProjectModel.PUBGDropMap.GameID)
            {
                this.SetCustomInteractiveGame(new PUBGDropMapInteractiveControl(this.selectedGame, this.selectedGameVersion));
            }
            else if (this.selectedGame.id == InteractiveSharedProjectModel.RealmRoyaleDropMap.GameID)
            {
                this.SetCustomInteractiveGame(new RealmRoyaleDropMapInteractiveControl(this.selectedGame, this.selectedGameVersion));
            }
            else if (this.selectedGame.id == InteractiveSharedProjectModel.BlackOps4DropMap.GameID)
            {
                this.SetCustomInteractiveGame(new BlackOps4DropMapInteractiveControl(this.selectedGame, this.selectedGameVersion));
            }
            else if (this.selectedGame.id == InteractiveSharedProjectModel.MixerPaint.GameID)
            {
                this.SetCustomInteractiveGame(new MixerPaintInteractiveControl(this.selectedGame, this.selectedGameVersion));
            }
            else
            {
                this.GroupsButton.IsEnabled = true;

                this.CustomInteractiveContentControl.Visibility = Visibility.Collapsed;
                this.InteractiveControlsGridView.Visibility = Visibility.Visible;

                this.CustomInteractiveContentControl.Content = null;

                this.interactiveScenes.Clear();
                if (this.selectedGameVersion != null)
                {
                    foreach (InteractiveSceneModel scene in this.selectedGameVersion.controls.scenes)
                    {
                        this.interactiveScenes.Add(scene);
                    }
                }

                if (this.selectedScene != null && this.interactiveScenes.Any(s => s.sceneID.Equals(this.selectedScene.sceneID)))
                {
                    this.InteractiveScenesComboBox.SelectedItem = this.interactiveScenes.First(s => s.sceneID.Equals(this.selectedScene.sceneID));
                }
                else
                {
                    this.InteractiveScenesComboBox.SelectedIndex = 0;
                }

                this.InteractiveScenesComboBox.IsEnabled = true;
                this.GroupsButton.IsEnabled = true;
            }

            this.ConnectButton.IsEnabled = true;

            if (ChannelSession.Interactive.IsConnected())
            {
                await this.InteractiveGameConnected();
            }
        }

        private void SetCustomInteractiveGame(CustomInteractiveGameControl control)
        {
            this.CustomInteractiveContentControl.Visibility = Visibility.Visible;
            this.InteractiveControlsGridView.Visibility = Visibility.Collapsed;

            this.CustomInteractiveContentControl.Content = control;

            this.InteractiveScenesComboBox.IsEnabled = false;
            this.GroupsButton.IsEnabled = false;
        }

        private void RefreshSelectedScene()
        {
            this.currentSceneControlItems.Clear();
            foreach (InteractiveButtonControlModel button in this.selectedScene.buttons.OrderBy(b => b.controlID))
            {
                this.AddControlItem(this.selectedScene.sceneID, button);
            }

            foreach (InteractiveJoystickControlModel joystick in this.selectedScene.joysticks.OrderBy(j => j.controlID))
            {
                this.AddControlItem(this.selectedScene.sceneID, joystick);
            }

            foreach (InteractiveTextBoxControlModel textBox in this.selectedScene.textBoxes.OrderBy(j => j.controlID))
            {
                this.AddControlItem(this.selectedScene.sceneID, textBox);
            }
        }

        private void AddControlItem(string sceneID, InteractiveControlModel control)
        {
            InteractiveCommand command = ChannelSession.Settings.InteractiveCommands.FirstOrDefault(c => c.GameID.Equals(this.selectedGame.id) &&
                c.SceneID.Equals(sceneID) && c.Control.controlID.Equals(control.controlID));

            InteractiveControlCommandItem item = null;
            if (command != null)
            {
                command.UpdateWithLatestControl(control);
                item = new InteractiveControlCommandItem(command);
            }
            else if (control is InteractiveButtonControlModel)
            {
                item = new InteractiveControlCommandItem((InteractiveButtonControlModel)control);
            }
            else if (control is InteractiveJoystickControlModel)
            {
                item = new InteractiveControlCommandItem((InteractiveJoystickControlModel)control);
            }
            else if (control is InteractiveTextBoxControlModel)
            {
                item = new InteractiveControlCommandItem((InteractiveTextBoxControlModel)control);
            }

            if (item != null)
            {
                this.currentSceneControlItems.Add(item);
            }
        }

        private void NewGameButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //ChannelSession.SelectedGame = null;
            //this.interactiveGameScenes.Clear();

            //this.GameNameTextBox.Clear();

            //this.selectedGameScene = InteractiveGameHelper.CreateDefaultScene();
            //this.interactiveGameScenes.Add(this.selectedGameScene);
            //this.SceneComboBox.SelectedIndex = 0;
            //this.BoardSizeComboBox.SelectedIndex = 0;

            //this.RefreshScene();

            //this.SaveChangedButton.IsEnabled = true;
            //this.GameDetailsGrid.IsEnabled = true;
            //this.ConnectDisconectGrid.IsEnabled = true;
        }

        private void MixerLabButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", "https://mixer.com/lab/");
        }

        private async void GroupsButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedGame != null && this.selectedScene != null)
            {
                InteractiveSceneUserGroupsDialogControl dialogControl = new InteractiveSceneUserGroupsDialogControl(this.selectedGame, this.selectedScene);
                await this.Window.RunAsyncOperation(async () =>
                {
                    await MessageBoxHelper.ShowCustomDialog(dialogControl);
                });
            }
        }

        private async void InteractiveGamesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.InteractiveGamesComboBox.SelectedIndex >= 0)
            {
                this.selectedGame = (InteractiveGameModel)this.InteractiveGamesComboBox.SelectedItem;
                await this.RefreshSelectedGame();
            }
        }

        private void InteractiveScenesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.InteractiveScenesComboBox.SelectedIndex >= 0)
            {
                this.selectedScene = (InteractiveSceneModel)this.InteractiveScenesComboBox.SelectedItem;
                this.RefreshSelectedScene();
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RefreshAllInteractiveGames(true);
        }

        private void NewInteractiveCommandButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            InteractiveControlCommandItem command = (InteractiveControlCommandItem)button.DataContext;
            CommandWindow window = null;
            if (command.Control is InteractiveButtonControlModel)
            {
                window = new CommandWindow(new InteractiveButtonCommandDetailsControl(this.selectedGame, this.selectedGameVersion, this.selectedScene, (InteractiveButtonControlModel)command.Control));
            }
            else if (command.Control is InteractiveJoystickControlModel)
            {
                window = new CommandWindow(new InteractiveJoystickCommandDetailsControl(this.selectedGame, this.selectedGameVersion, this.selectedScene, (InteractiveJoystickControlModel)command.Control));
            }
            else if (command.Control is InteractiveTextBoxControlModel)
            {
                window = new CommandWindow(new InteractiveTextBoxCommandDetailsControl(this.selectedGame, this.selectedGameVersion, this.selectedScene, (InteractiveTextBoxControlModel)command.Control));
            }

            if (window != null)
            {
                window.Closed += Window_Closed;
                window.Show();
            }
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            InteractiveCommand command = commandButtonsControl.GetCommandFromCommandButtons<InteractiveCommand>(sender);
            if (command != null)
            {
                CommandWindow window = null;
                if (command is InteractiveButtonCommand)
                {
                    window = new CommandWindow(new InteractiveButtonCommandDetailsControl(this.selectedGame, this.selectedGameVersion, (InteractiveButtonCommand)command));

                }
                else if (command is InteractiveJoystickCommand)
                {
                    window = new CommandWindow(new InteractiveJoystickCommandDetailsControl(this.selectedGame, this.selectedGameVersion, (InteractiveJoystickCommand)command));
                }
                else if (command is InteractiveTextBoxCommand)
                {
                    window = new CommandWindow(new InteractiveTextBoxCommandDetailsControl(this.selectedGame, this.selectedGameVersion, (InteractiveTextBoxCommand)command));
                }

                if (window != null)
                {
                    window.Closed += Window_Closed;
                    window.Show();
                }
            }
        }

        private async void CommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
                InteractiveCommand command = commandButtonsControl.GetCommandFromCommandButtons<InteractiveCommand>(sender);
                if (command != null)
                {
                    ChannelSession.Settings.InteractiveCommands.Remove(command);
                    await ChannelSession.SaveSettings();
                    this.RefreshSelectedScene();
                }
            });
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.RefreshSelectedScene();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool result = await this.Window.RunAsyncOperation(async () =>
                {
                    if (!this.IsCustomInteractiveGame)
                    {
                        await ChannelSession.Interactive.DisableAllControlsWithoutCommands(this.selectedGameVersion);
                    }
                    return await ChannelSession.Interactive.Connect(this.selectedGame, this.selectedGameVersion);
                });

                if (result)
                {
                    await this.InteractiveGameConnected();
                    return;
                }
                else
                {
                    await this.Window.RunAsyncOperation(async () =>
                    {
                        await ChannelSession.Interactive.Disconnect();
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            await MessageBoxHelper.ShowMessageDialog("Failed to connect to MixPlay with selected game. Please try again.");
        }

        private async void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                await ChannelSession.Interactive.Disconnect();

                await this.InteractiveGameDisconnected();
            });
        }

        private async void GlobalEvents_OnInteractiveSharedProjectAdded(object sender, InteractiveSharedProjectModel e)
        {
            await this.Dispatcher.InvokeAsync(async () =>
            {
                await this.RefreshAllInteractiveGames();
            });
        }

        private void GlobalEvents_OnInteractiveConnected(object sender, InteractiveGameModel game)
        {
            this.Dispatcher.InvokeAsync(() =>
            {
                this.InteractiveGamesComboBox.SelectedItem = this.interactiveGames.FirstOrDefault(g => g.id.Equals(game.id));
                return Task.FromResult(0);
            });
        }

        private async void GlobalEvents_OnInteractiveDisconnected(object sender, EventArgs e)
        {
            await this.Dispatcher.InvokeAsync(async () => await this.InteractiveGameDisconnected());
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                await this.RefreshSelectedGame();
            });
        }

        private async Task InteractiveGameConnected()
        {
            this.InteractiveGamesComboBox.IsEnabled = false;
            this.GroupsButton.IsEnabled = false;
            this.RefreshButton.IsEnabled = false;

            this.ConnectButton.Visibility = Visibility.Collapsed;
            this.DisconnectButton.Visibility = Visibility.Visible;

            if (this.IsCustomInteractiveGame)
            {
                CustomInteractiveGameControl gameControl = (CustomInteractiveGameControl)this.CustomInteractiveContentControl.Content;
                if (!await gameControl.GameConnected())
                {
                    await this.InteractiveGameDisconnected();
                }
            }
        }

        private async Task InteractiveGameDisconnected()
        {
            this.InteractiveGamesComboBox.IsEnabled = true;
            this.GroupsButton.IsEnabled = true;
            this.RefreshButton.IsEnabled = true;

            this.ConnectButton.Visibility = Visibility.Visible;
            this.DisconnectButton.Visibility = Visibility.Collapsed;

            if (this.IsCustomInteractiveGame)
            {
                CustomInteractiveGameControl gameControl = (CustomInteractiveGameControl)this.CustomInteractiveContentControl.Content;
                await gameControl.GameDisconnected();
            }
        }
    }
}
