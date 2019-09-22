using Mixer.Base.Model.MixPlay;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.MixerAPI;
using MixItUp.Base.Model.Interactive;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Interactive;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Controls.Dialogs;
using MixItUp.WPF.Controls.Interactive;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using StreamingClient.Base.Util;
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
        public MixPlaySceneModel Scene { get; set; }

        public MixPlayButtonControlModel Button { get; set; }
        public MixPlayJoystickControlModel Joystick { get; set; }
        public MixPlayTextBoxControlModel TextBox { get; set; }

        public MixPlayCommand Command { get; set; }
        public Visibility NewCommandButtonVisibility { get { return (this.Command == null) ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility ExistingCommandButtonVisibility { get { return (this.Command != null) ? Visibility.Visible : Visibility.Collapsed; } }

        public InteractiveControlCommandItem(MixPlayButtonControlModel button) { this.Button = button; }
        public InteractiveControlCommandItem(MixPlayJoystickControlModel joystick) { this.Joystick = joystick; }
        public InteractiveControlCommandItem(MixPlayTextBoxControlModel textBox) { this.TextBox = textBox; }

        public InteractiveControlCommandItem(MixPlayCommand command)
        {
            this.Command = command;
            if (command.Control is MixPlayButtonControlModel)
            {
                this.Button = (MixPlayButtonControlModel)command.Control;
            }
            else if(command.Control is MixPlayJoystickControlModel)
            {
                this.Joystick = (MixPlayJoystickControlModel)command.Control;
            }
            else if (command.Control is MixPlayTextBoxControlModel)
            {
                this.TextBox = (MixPlayTextBoxControlModel)command.Control;
            }
        }

        public MixPlayControlModel Control
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
                if (this.Control is MixPlayButtonControlModel) { return "Button"; }
                if (this.Control is MixPlayJoystickControlModel) { return "Joystick"; }
                if (this.Control is MixPlayTextBoxControlModel) { return "Text Box"; }
                return "Unknown";
            }
        }

        public string SparkCost
        {
            get
            {
                if (this.Control is MixPlayButtonControlModel) { return this.Button.cost.ToString(); }
                if (this.Control is MixPlayTextBoxControlModel) { return this.TextBox.cost.ToString(); }
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
        private ObservableCollection<MixPlayGameModel> interactiveGames = new ObservableCollection<MixPlayGameModel>();
        private MixPlayGameModel selectedGame = null;
        public MixPlayGameVersionModel selectedGameVersion = null;

        private ObservableCollection<MixPlaySceneModel> interactiveScenes = new ObservableCollection<MixPlaySceneModel>();
        private MixPlaySceneModel selectedScene = null;

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
                MixPlayGameModel game = this.interactiveGames.FirstOrDefault(g => g.id.Equals(ChannelSession.Settings.DefaultInteractiveGame));
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

            IEnumerable<MixPlayGameModel> games = await this.Window.RunAsyncOperation(async () =>
            {
                return await ChannelSession.Interactive.GetAllConnectableGames(forceRefresh);
            });

            foreach (MixPlayGameModel game in games)
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
                IEnumerable<MixPlayGameVersionModel> versions = await ChannelSession.MixerStreamerConnection.GetMixPlayGameVersions(this.selectedGame);
                if (versions != null && versions.Count() > 0)
                {
                    return await ChannelSession.MixerStreamerConnection.GetMixPlayGameVersion(versions.First());
                }
                return null;
            });

            this.GroupsButton.IsEnabled = false;
            if (this.selectedGame.id == InteractiveSharedProjectModel.FortniteDropMap.GameID)
            {
                this.SetCustomInteractiveGame(new DropMapInteractiveControl(DropMapTypeEnum.Fortnite, this.selectedGame, this.selectedGameVersion));
            }
            else if (this.selectedGame.id == InteractiveSharedProjectModel.PUBGDropMap.GameID)
            {
                this.SetCustomInteractiveGame(new DropMapInteractiveControl(DropMapTypeEnum.PUBG, this.selectedGame, this.selectedGameVersion));
            }
            else if (this.selectedGame.id == InteractiveSharedProjectModel.RealmRoyaleDropMap.GameID)
            {
                this.SetCustomInteractiveGame(new DropMapInteractiveControl(DropMapTypeEnum.RealmRoyale, this.selectedGame, this.selectedGameVersion));
            }
            else if (this.selectedGame.id == InteractiveSharedProjectModel.BlackOps4DropMap.GameID)
            {
                this.SetCustomInteractiveGame(new DropMapInteractiveControl(DropMapTypeEnum.BlackOps4, this.selectedGame, this.selectedGameVersion));
            }
            else if (this.selectedGame.id == InteractiveSharedProjectModel.ApexLegendsDropMap.GameID)
            {
                this.SetCustomInteractiveGame(new DropMapInteractiveControl(DropMapTypeEnum.ApexLegends, this.selectedGame, this.selectedGameVersion));
            }
            else if (this.selectedGame.id == InteractiveSharedProjectModel.SuperAnimalRoyaleDropMap.GameID)
            {
                this.SetCustomInteractiveGame(new DropMapInteractiveControl(DropMapTypeEnum.SuperAnimalRoyale, this.selectedGame, this.selectedGameVersion));
            }
            else if (this.selectedGame.id == InteractiveSharedProjectModel.MixerPaint.GameID)
            {
                this.SetCustomInteractiveGame(new MixerPaintInteractiveControl(this.selectedGame, this.selectedGameVersion));
            }
            else if (this.selectedGame.id == InteractiveSharedProjectModel.FlySwatter.GameID)
            {
                this.SetCustomInteractiveGame(new FlySwatterInteractiveControl(this.selectedGame, this.selectedGameVersion));
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
                    foreach (MixPlaySceneModel scene in this.selectedGameVersion.controls.scenes)
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
            foreach (MixPlayButtonControlModel button in this.selectedScene.buttons.OrderBy(b => b.controlID))
            {
                this.AddControlItem(this.selectedScene.sceneID, button);
            }

            foreach (MixPlayJoystickControlModel joystick in this.selectedScene.joysticks.OrderBy(j => j.controlID))
            {
                this.AddControlItem(this.selectedScene.sceneID, joystick);
            }

            foreach (MixPlayTextBoxControlModel textBox in this.selectedScene.textBoxes.OrderBy(j => j.controlID))
            {
                this.AddControlItem(this.selectedScene.sceneID, textBox);
            }
        }

        private void AddControlItem(string sceneID, MixPlayControlModel control)
        {
            MixPlayCommand command = ChannelSession.Settings.MixPlayCommands.FirstOrDefault(c => c.GameID.Equals(this.selectedGame.id) &&
                c.SceneID.Equals(sceneID) && c.Control.controlID.Equals(control.controlID));

            InteractiveControlCommandItem item = null;
            if (command != null)
            {
                command.UpdateWithLatestControl(control);
                item = new InteractiveControlCommandItem(command);
            }
            else if (control is MixPlayButtonControlModel)
            {
                item = new InteractiveControlCommandItem((MixPlayButtonControlModel)control);
            }
            else if (control is MixPlayJoystickControlModel)
            {
                item = new InteractiveControlCommandItem((MixPlayJoystickControlModel)control);
            }
            else if (control is MixPlayTextBoxControlModel)
            {
                item = new InteractiveControlCommandItem((MixPlayTextBoxControlModel)control);
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
            ProcessHelper.LaunchLink("https://mixer.com/lab/");
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
                this.selectedGame = (MixPlayGameModel)this.InteractiveGamesComboBox.SelectedItem;
                await this.RefreshSelectedGame();
            }
        }

        private void InteractiveScenesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.InteractiveScenesComboBox.SelectedIndex >= 0)
            {
                this.selectedScene = (MixPlaySceneModel)this.InteractiveScenesComboBox.SelectedItem;
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
            if (command.Control is MixPlayButtonControlModel)
            {
                window = new CommandWindow(new InteractiveButtonCommandDetailsControl(this.selectedGame, this.selectedGameVersion, this.selectedScene, (MixPlayButtonControlModel)command.Control));
            }
            else if (command.Control is MixPlayJoystickControlModel)
            {
                window = new CommandWindow(new InteractiveJoystickCommandDetailsControl(this.selectedGame, this.selectedGameVersion, this.selectedScene, (MixPlayJoystickControlModel)command.Control));
            }
            else if (command.Control is MixPlayTextBoxControlModel)
            {
                window = new CommandWindow(new InteractiveTextBoxCommandDetailsControl(this.selectedGame, this.selectedGameVersion, this.selectedScene, (MixPlayTextBoxControlModel)command.Control));
            }

            if (window != null)
            {
                window.Closed += Window_Closed;
                window.Show();
            }
        }

        private async void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            MixPlayCommand command = commandButtonsControl.GetCommandFromCommandButtons<MixPlayCommand>(sender);
            if (command != null)
            {
                if (!this.ValidateCommandMatchesControl(command))
                {
                    string oldControlType = string.Empty;
                    if (command is MixPlayButtonCommand) { oldControlType = "Button"; }
                    if (command is MixPlayJoystickCommand) { oldControlType = "Joystick"; }
                    if (command is MixPlayTextBoxCommand) { oldControlType = "Text Box"; }

                    string newControlType = string.Empty;
                    if (command.Control is MixPlayButtonControlModel) { newControlType = "Button"; }
                    if (command.Control is MixPlayJoystickControlModel) { newControlType = "Joystick"; }
                    if (command.Control is MixPlayTextBoxControlModel) { newControlType = "Text Box"; }

                    await this.Window.RunAsyncOperation(async () =>
                    {
                        await MessageBoxHelper.ShowMessageDialog(string.Format("The control you are trying to edit has been changed from a {0} to a {1} and can not be edited. You must either change this control back to its previous type, change the control ID to something different, or delete the command to start fresh.", oldControlType, newControlType));
                    });
                    return;
                }

                CommandWindow window = null;
                if (command is MixPlayButtonCommand)
                {
                    window = new CommandWindow(new InteractiveButtonCommandDetailsControl(this.selectedGame, this.selectedGameVersion, (MixPlayButtonCommand)command));
                }
                else if (command is MixPlayJoystickCommand)
                {
                    window = new CommandWindow(new InteractiveJoystickCommandDetailsControl(this.selectedGame, this.selectedGameVersion, (MixPlayJoystickCommand)command));
                }
                else if (command is MixPlayTextBoxCommand)
                {
                    window = new CommandWindow(new InteractiveTextBoxCommandDetailsControl(this.selectedGame, this.selectedGameVersion, (MixPlayTextBoxCommand)command));
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
            await this.Window.RunAsyncOperation((Func<Task>)(async () =>
            {
                CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
                MixPlayCommand command = commandButtonsControl.GetCommandFromCommandButtons<MixPlayCommand>(sender);
                if (command != null)
                {
                    ChannelSession.Settings.MixPlayCommands.Remove(command);
                    await ChannelSession.SaveSettings();
                    this.RefreshSelectedScene();
                }
            }));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.RefreshSelectedScene();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MixPlayConnectionResult result = await this.Window.RunAsyncOperation(async () =>
                {
                    if (!this.IsCustomInteractiveGame)
                    {
                        await ChannelSession.Interactive.DisableAllControlsWithoutCommands(this.selectedGameVersion);
                    }

                    return await ChannelSession.Interactive.Connect(this.selectedGame, this.selectedGameVersion);
                });

                switch (result)
                {
                    case MixPlayConnectionResult.Success:
                        await this.InteractiveGameConnected();
                        return;
                    case MixPlayConnectionResult.DuplicateControlIDs:
                        await this.Window.RunAsyncOperation(async () =>
                        {
                            await ChannelSession.Interactive.Disconnect();
                        });

                        await MessageBoxHelper.ShowMessageDialog("This MixPlay game configuration is invalid. It has multiple controls with the same control ID on different scenes.  Please visit the interactive lab and correct this problem."
                            + Environment.NewLine
                            + Environment.NewLine
                            + string.Join(Environment.NewLine, ChannelSession.Interactive.DuplicatedControls));
                        return;
                    case MixPlayConnectionResult.Unknown:
                        await this.Window.RunAsyncOperation(async () =>
                        {
                            await ChannelSession.Interactive.Disconnect();
                        });
                        break;
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

        private void GlobalEvents_OnInteractiveConnected(object sender, MixPlayGameModel game)
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

        private bool ValidateCommandMatchesControl(MixPlayCommand command)
        {
            return (command is MixPlayButtonCommand && command.Control is MixPlayButtonControlModel) ||
                (command is MixPlayJoystickCommand && command.Control is MixPlayJoystickControlModel) ||
                (command is MixPlayTextBoxCommand&& command.Control is MixPlayTextBoxControlModel);
        }
    }
}
