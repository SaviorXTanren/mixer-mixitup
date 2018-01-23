using Mixer.Base.Model.Interactive;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Interactive;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Controls.Dialogs;
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

        public InteractiveCommand Command { get; set; }
        public Visibility NewCommandButtonVisibility { get { return (this.Command == null) ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility ExistingCommandButtonVisibility { get { return (this.Command != null) ? Visibility.Visible : Visibility.Collapsed; } }

        public InteractiveControlCommandItem(InteractiveButtonControlModel button) { this.Button = button; }
        public InteractiveControlCommandItem(InteractiveJoystickControlModel joystick) { this.Joystick = joystick; }

        public InteractiveControlCommandItem(InteractiveCommand command)
        {
            this.Command = command;
            if (command.Control is InteractiveButtonControlModel)
            {
                this.Button = (InteractiveButtonControlModel)command.Control;
            }
            else
            {
                this.Joystick = (InteractiveJoystickControlModel)command.Control;
            }
        }

        public InteractiveControlModel Control { get { return (this.Button != null) ? (InteractiveControlModel)this.Button : (InteractiveControlModel)this.Joystick; } }

        public string Name { get { return this.Control.controlID; } }
        public string Type { get { return (this.Control is InteractiveButtonControlModel) ? "Button" : "Joystick"; } }
        public string SparkCost { get { return (this.Button != null) ? this.Button.cost.ToString() : string.Empty; } }
        public string Cooldown
        {
            get
            {
                if (this.Command != null)
                {
                    return (!string.IsNullOrEmpty(this.Command.CooldownGroup)) ? this.Command.CooldownGroup : this.Command.CooldownAmount.ToString();
                }
                return string.Empty;
            }
        }
        public string TriggerTransactionString { get { return (this.Command != null) ? this.Command.TriggerTransactionString : string.Empty; } }
    }

    /// <summary>
    /// Interaction logic for InteractiveControl.xaml
    /// </summary>
    public partial class InteractiveControl : MainControlBase
    {
        private ObservableCollection<InteractiveGameListingModel> interactiveGames = new ObservableCollection<InteractiveGameListingModel>();
        private InteractiveGameListingModel selectedGame = null;
        public InteractiveGameVersionModel selectedGameVersion = null;

        private ObservableCollection<InteractiveSceneModel> interactiveScenes = new ObservableCollection<InteractiveSceneModel>();
        private InteractiveSceneModel selectedScene = null;

        private ObservableCollection<InteractiveControlCommandItem> currentSceneControlItems = new ObservableCollection<InteractiveControlCommandItem>();

        public InteractiveControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.InteractiveGamesComboBox.ItemsSource = this.interactiveGames;
            this.InteractiveScenesComboBox.ItemsSource = this.interactiveScenes;
            this.InteractiveControlsGridView.ItemsSource = this.currentSceneControlItems;

            await this.RefreshAllInteractiveGames();
        }

        private async Task RefreshAllInteractiveGames()
        {
            this.GroupsButton.IsEnabled = false;
            this.ConnectButton.IsEnabled = false;

            IEnumerable<InteractiveGameListingModel> gameListings = await this.Window.RunAsyncOperation(async () =>
            {
                return await ChannelSession.Connection.GetOwnedInteractiveGames(ChannelSession.Channel);
            });

            gameListings = gameListings.Where(g => !g.name.Equals("Soundwave Interactive Soundboard"));

            this.interactiveGames.Clear();
            foreach (InteractiveGameListingModel game in gameListings)
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

            foreach (UserRole role in UserViewModel.SelectableUserRoles())
            {
                if (!ChannelSession.Settings.InteractiveUserGroups[this.selectedGame.id].Any(ug => ug.AssociatedUserRole == role))
                {
                    ChannelSession.Settings.InteractiveUserGroups[this.selectedGame.id].Add(new InteractiveUserGroupViewModel(role));
                }
            }

            this.selectedGameVersion = await this.Window.RunAsyncOperation(async () =>
            {
                return await ChannelSession.Connection.GetInteractiveGameVersion(this.selectedGame.versions.First());
            });

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

            this.GroupsButton.IsEnabled = true;
            this.ConnectButton.IsEnabled = true;
        }

        private void RefreshSelectedScene()
        {
            this.currentSceneControlItems.Clear();
            foreach (InteractiveButtonControlModel button in this.selectedScene.buttons.OrderBy(b => b.controlID))
            {
                this.currentSceneControlItems.Add(this.CreateControlItem(this.selectedScene.sceneID, button));
            }

            foreach (InteractiveJoystickControlModel joystick in this.selectedScene.joysticks.OrderBy(j => j.controlID))
            {
                this.currentSceneControlItems.Add(this.CreateControlItem(this.selectedScene.sceneID, joystick));
            }
        }

        private InteractiveControlCommandItem CreateControlItem(string sceneID, InteractiveControlModel control)
        {
            InteractiveCommand command = ChannelSession.Settings.InteractiveCommands.FirstOrDefault(c => c.GameID.Equals(this.selectedGame.id) &&
                c.SceneID.Equals(sceneID) && c.Control.controlID.Equals(control.controlID));

            if (command != null)
            {
                command.UpdateWithLatestControl(control);
                return new InteractiveControlCommandItem(command);
            }
            else if (control is InteractiveButtonControlModel)
            {
                return new InteractiveControlCommandItem((InteractiveButtonControlModel)control);
            }
            else
            {
                return new InteractiveControlCommandItem((InteractiveJoystickControlModel)control);
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
            Process.Start("https://mixer.com/lab/");
        }

        private async void GroupsButton_Click(object sender, RoutedEventArgs e)
        {
            InteractiveSceneUserGroupsDialogControl dialogControl = new InteractiveSceneUserGroupsDialogControl(this.selectedGame, this.selectedScene);
            await this.Window.RunAsyncOperation(async () =>
            {
                await MessageBoxHelper.ShowCustomDialog(dialogControl);
            });
        }

        private async void InteractiveGamesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.InteractiveGamesComboBox.SelectedIndex >= 0)
            {
                this.selectedGame = (InteractiveGameListingModel)this.InteractiveGamesComboBox.SelectedItem;
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
            await this.RefreshAllInteractiveGames();
        }

        private void NewInteractiveCommandButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            InteractiveControlCommandItem command = (InteractiveControlCommandItem)button.DataContext;

            CommandWindow window = new CommandWindow(new InteractiveCommandDetailsControl(this.selectedGame, this.selectedGameVersion, this.selectedScene, command.Control));
            window.Closed += Window_Closed;
            window.Show();
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            InteractiveCommand command = commandButtonsControl.GetCommandFromCommandButtons<InteractiveCommand>(sender);
            if (command != null)
            {
                CommandWindow window = new CommandWindow(new InteractiveCommandDetailsControl(command));
                window.Closed += Window_Closed;
                window.Show();
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
                    await ChannelSession.Interactive.DisableAllControlsWithoutCommands(this.selectedGameVersion);

                    return await ChannelSession.Interactive.Connect(this.selectedGame);
                });

                if (result)
                {
                    this.GameSelectionGrid.IsEnabled = false;
                    this.ConnectButton.Visibility = Visibility.Collapsed;
                    this.DisconnectButton.Visibility = Visibility.Visible;
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

            await MessageBoxHelper.ShowMessageDialog("Failed to connect to interactive with selected game. Please try again.");
        }

        private async void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                await ChannelSession.Interactive.Disconnect();
            });

            this.GameSelectionGrid.IsEnabled = true;
            this.ConnectButton.Visibility = Visibility.Visible;
            this.DisconnectButton.Visibility = Visibility.Collapsed;
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                await this.RefreshSelectedGame();
            });
        }
    }
}
