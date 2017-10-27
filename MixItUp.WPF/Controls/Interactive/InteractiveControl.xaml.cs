using Mixer.Base.Model.Interactive;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MixItUp.WPF.Controls.Interactive
{
    public class InteractiveControlCommandItem
    {
        public InteractiveSceneModel Scene { get; set; }

        public InteractiveButtonControlModel Button { get; set; }
        public InteractiveJoystickControlModel Joystick { get; set; }

        public InteractiveConnectedButtonControlModel ConnectedButton { get; set; }
        public InteractiveConnectedJoystickControlModel ConnectedJoystick { get; set; }

        public InteractiveCommand Command { get; set; }

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

        private ObservableCollection<InteractiveControlCommandItem> interactiveItems = new ObservableCollection<InteractiveControlCommandItem>();

        private List<InteractiveConnectedSceneGroupModel> connectedGameScenes = new List<InteractiveConnectedSceneGroupModel>();
        private InteractiveConnectedSceneGroupModel connectedScene = null;

        public InteractiveControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.InteractiveGamesComboBox.ItemsSource = this.interactiveGames;
            this.InteractiveScenesComboBox.ItemsSource = this.interactiveScenes;
            this.InteractiveControlsGridView.ItemsSource = this.interactiveItems;

            await this.RefreshInteractiveGames();
        }

        private async Task RefreshInteractiveGames()
        {
            IEnumerable<InteractiveGameListingModel> gameListings = await this.Window.RunAsyncOperation(async () =>
            {
                return await ChannelSession.Connection.GetOwnedInteractiveGames(ChannelSession.Channel);
            });

            this.interactiveGames.Clear();
            foreach (InteractiveGameListingModel game in gameListings)
            {
                this.interactiveGames.Add(game);
            }
        }

        private async Task RefreshSelectedInteractiveGame()
        {
            await this.RefreshInteractiveGames();

            if (this.selectedGame != null)
            {
                this.interactiveScenes.Clear();

                this.selectedGameVersion = await this.Window.RunAsyncOperation(async () =>
                {
                    this.selectedGame = this.interactiveGames.First(g => g.id.Equals(this.selectedGame.id));

                    return await ChannelSession.Connection.GetInteractiveGameVersion(this.selectedGame.versions.First());
                });

                if (this.selectedGame != null)
                {
                    this.InteractiveGamesComboBox.SelectedItem = this.selectedGame;
                }

                foreach (InteractiveSceneModel scene in this.selectedGameVersion.controls.scenes)
                {
                    this.interactiveScenes.Add(scene);
                }

                if (this.selectedScene != null)
                {
                    this.InteractiveScenesComboBox.SelectedItem = this.interactiveScenes.FirstOrDefault(s => s.sceneID.Equals(this.selectedScene.sceneID));
                }
                else
                {
                    this.InteractiveScenesComboBox.SelectedIndex = 0;
                }

                this.GameDetailsGrid.IsEnabled = true;
            }
        }

        private void RefreshSelectedScene()
        {
            this.interactiveItems.Clear();
            if (this.selectedScene != null)
            {
                foreach (InteractiveButtonControlModel button in this.selectedScene.buttons)
                {
                    this.interactiveItems.Add(this.CreateItem(button));
                }

                foreach (InteractiveJoystickControlModel joystick in this.selectedScene.joysticks)
                {
                    this.interactiveItems.Add(this.CreateItem(joystick));
                }
            }
        }

        private InteractiveControlCommandItem CreateItem(InteractiveControlModel control)
        {
            InteractiveCommand command = this.FindCommandFromSettings(this.selectedScene, control);
            if (command != null)
            {
                command.UpdateWithLatestControl(control);
                return new InteractiveControlCommandItem(command);
            }
            else if (control is InteractiveButtonControlModel) { return new InteractiveControlCommandItem((InteractiveButtonControlModel)control); }
            else { return new InteractiveControlCommandItem((InteractiveJoystickControlModel)control); }
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
        }

        private async void InteractiveGamesComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.RefreshSelectedGameButton.IsEnabled = false;
            if (this.InteractiveGamesComboBox.SelectedIndex >= 0)
            {
                InteractiveGameListingModel newSelectedGame = (InteractiveGameListingModel)this.InteractiveGamesComboBox.SelectedItem;
                if (this.selectedGame != newSelectedGame)
                {
                    this.selectedGame = newSelectedGame;
                    await this.RefreshSelectedInteractiveGame();
                    this.ConnectButton.IsEnabled = true;
                }
                this.RefreshSelectedGameButton.IsEnabled = true;
            }
        }

        private void InteractiveScenesComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.InteractiveScenesComboBox.SelectedIndex >= 0)
            {
                this.selectedScene = (InteractiveSceneModel)this.InteractiveScenesComboBox.SelectedItem;
                this.RefreshSelectedScene();
            }
        }

        private async void RefreshSelectedGameButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RefreshSelectedInteractiveGame();
        }

        private async void TestButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            InteractiveControlCommandItem command = (InteractiveControlCommandItem)button.DataContext;

            if (command.Command != null)
            {
                await this.Window.RunAsyncOperation(async () =>
                {
                    await command.Command.Perform();
                });
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            InteractiveControlCommandItem command = (InteractiveControlCommandItem)button.DataContext;

            CommandWindow window = (command.Command == null) ?
                new CommandWindow(new InteractiveCommandDetailsControl(this.selectedGame, this.selectedGameVersion, this.selectedScene, command.Control)) :
                new CommandWindow(new InteractiveCommandDetailsControl(this.selectedGame, this.selectedGameVersion, this.selectedScene, command.Command));
            window.Closed += Window_Closed;
            window.Show();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            InteractiveControlCommandItem command = (InteractiveControlCommandItem)button.DataContext;

            if (command.Command != null)
            {
                ChannelSession.Settings.InteractiveControls.Remove(command.Command);
                command.Command = null;

                await this.Window.RunAsyncOperation(async () => { await ChannelSession.SaveSettings(); });

                this.InteractiveControlsGridView.SelectedIndex = -1;

                this.RefreshSelectedScene();
            }
        }

        private async void EnableDisableToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton button = (ToggleButton)sender;
            InteractiveControlCommandItem command = (InteractiveControlCommandItem)button.DataContext;
            if (command.Command != null)
            {
                command.Command.IsEnabled = true;
                await this.Window.RunAsyncOperation(async () => { await ChannelSession.SaveSettings(); });
            }
            else
            {
                button.IsChecked = false;
            }
        }

        private async void EnableDisableToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            ToggleButton button = (ToggleButton)sender;
            InteractiveControlCommandItem command = (InteractiveControlCommandItem)button.DataContext;
            if (command.Command != null)
            {
                command.Command.IsEnabled = false;
                await this.Window.RunAsyncOperation(async () => { await ChannelSession.SaveSettings(); });
            }
            else
            {
                button.IsChecked = false;
            }
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            this.RefreshSelectedScene();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            bool result = await this.Window.RunAsyncOperation(async () =>
            {
                return await ChannelSession.ConnectInteractive(this.selectedGame);
            });

            if (!result)
            {
                await MessageBoxHelper.ShowMessageDialog("Unable to connect to interactive with selected game. Please try again.");
                return;
            }

            InteractiveConnectedSceneGroupCollectionModel scenes = await this.Window.RunAsyncOperation(async () =>
            {
                return await ChannelSession.Interactive.GetScenes();
            });
            this.connectedGameScenes = scenes.scenes;
            this.connectedScene = this.connectedGameScenes.First();

            foreach (InteractiveConnectedButtonControlModel button in this.connectedScene.buttons)
            {
                InteractiveControlCommandItem item = this.FindCommandFromItems(button);
                if (item != null)
                {
                    item.ConnectedButton = button;
                }
            }

            foreach (InteractiveConnectedJoystickControlModel joystick in this.connectedScene.joysticks)
            {
                InteractiveControlCommandItem item = this.FindCommandFromItems(joystick);
                if (item != null)
                {
                    item.ConnectedJoystick = joystick;
                }
            }

            InteractiveParticipantCollectionModel participants = await this.Window.RunAsyncOperation(async () =>
            {
                return await ChannelSession.Interactive.GetAllParticipants();
            });

            ChannelSession.InteractiveUsers.Clear();
            if (participants != null)
            {
                foreach (InteractiveParticipantModel participant in participants.participants)
                {
                    ChannelSession.InteractiveUsers.Add(participant.sessionID, participant);
                }
            }

            ChannelSession.Interactive.Client.OnParticipantJoin += InteractiveClient_OnParticipantJoin;
            ChannelSession.Interactive.Client.OnParticipantUpdate += InteractiveClient_OnParticipantUpdate;
            ChannelSession.Interactive.Client.OnParticipantLeave += InteractiveClient_OnParticipantLeave;
            ChannelSession.Interactive.Client.OnGiveInput += InteractiveClient_OnGiveInput;

            this.GameSelectionGrid.IsEnabled = false;
            this.GameDetailsGrid.IsEnabled = false;
            this.ConnectButton.Visibility = Visibility.Collapsed;
            this.DisconnectButton.Visibility = Visibility.Visible;
        }

        private async void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            ChannelSession.Interactive.Client.OnParticipantJoin -= InteractiveClient_OnParticipantJoin;
            ChannelSession.Interactive.Client.OnParticipantUpdate -= InteractiveClient_OnParticipantUpdate;
            ChannelSession.Interactive.Client.OnParticipantLeave -= InteractiveClient_OnParticipantLeave;
            ChannelSession.Interactive.Client.OnGiveInput -= InteractiveClient_OnGiveInput;

            await this.Window.RunAsyncOperation(async () =>
            {
                await ChannelSession.DisconnectInteractive();
            });

            foreach (InteractiveControlCommandItem item in this.interactiveItems)
            {
                item.ConnectedButton = null;
                item.ConnectedJoystick = null;
            }

            this.connectedGameScenes.Clear();
            this.connectedScene = null;

            this.GameSelectionGrid.IsEnabled = true;
            this.GameDetailsGrid.IsEnabled = true;
            this.ConnectButton.Visibility = Visibility.Visible;
            this.DisconnectButton.Visibility = Visibility.Collapsed;
        }

        private void InteractiveClient_OnParticipantJoin(object sender, InteractiveParticipantCollectionModel participants)
        {
            if (participants != null)
            {
                foreach (InteractiveParticipantModel participant in participants.participants)
                {
                    ChannelSession.InteractiveUsers[participant.sessionID] = participant;
                }
            }
        }

        private void InteractiveClient_OnParticipantUpdate(object sender, InteractiveParticipantCollectionModel participants)
        {
            if (participants != null)
            {
                foreach (InteractiveParticipantModel participant in participants.participants)
                {
                    ChannelSession.InteractiveUsers[participant.sessionID] = participant;
                }
            }
        }

        private void InteractiveClient_OnParticipantLeave(object sender, InteractiveParticipantCollectionModel participants)
        {
            if (participants != null)
            {
                foreach (InteractiveParticipantModel participant in participants.participants)
                {
                    ChannelSession.InteractiveUsers.Remove(participant.sessionID);
                }
            }
        }

        private async void InteractiveClient_OnGiveInput(object sender, InteractiveGiveInputModel interactiveInput)
        {
            if (interactiveInput != null && interactiveInput.input != null)
            {
                foreach (InteractiveControlCommandItem item in this.interactiveItems)
                {
                    if (item.Name.Equals(interactiveInput.input.controlID) && item.Command != null)
                    {
                        if (item.ConnectedButton != null && !item.TriggerTransactionString.Equals(interactiveInput.input.eventType))
                        {
                            return;
                        }

                        if (!string.IsNullOrEmpty(interactiveInput.transactionID))
                        {
                            await this.Window.RunAsyncOperation(async () =>
                            {
                                try
                                {
                                    await ChannelSession.Interactive.CaptureSparkTransaction(interactiveInput.transactionID);
                                }
                                catch (Exception) { }
                            });
                        }

                        GlobalEvents.InteractiveControlUsed(item.Control);

                        UserViewModel user = ChannelSession.GetCurrentUser();
                        if (ChannelSession.InteractiveUsers.ContainsKey(interactiveInput.participantID))
                        {
                            InteractiveParticipantModel participant = ChannelSession.InteractiveUsers[interactiveInput.participantID];
                            if (ChannelSession.ChatUsers.ContainsKey(participant.userID))
                            {
                                user = ChannelSession.ChatUsers[participant.userID];
                            }
                            else
                            {
                                user = new UserViewModel(participant.userID, participant.username);
                            }
                        }

                        if (item.ConnectedButton != null)
                        {
                            await this.Window.RunAsyncOperation(async () =>
                            {
                                item.ConnectedButton.cooldown = item.Command.GetCooldownTimestamp();

                                List<InteractiveConnectedButtonControlModel> buttons = new List<InteractiveConnectedButtonControlModel>();                             
                                if (!string.IsNullOrEmpty(item.Command.CooldownGroup))
                                {
                                    var otherItems = this.interactiveItems.Where(i => i.Command != null && i.ConnectedButton != null);
                                    otherItems = otherItems.Where(i => item.Command.CooldownGroup.Equals(i.Command.CooldownGroup));
                                    foreach (var otherItem in otherItems)
                                    {
                                        otherItem.ConnectedButton.cooldown = item.ConnectedButton.cooldown;
                                    }
                                    buttons.AddRange(otherItems.Select(i => i.ConnectedButton));
                                }
                                else
                                {
                                    buttons.Add(item.ConnectedButton);
                                }

                                await ChannelSession.Interactive.UpdateControls(this.connectedScene, buttons);
                            });
                        }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        item.Command.Perform(user);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                        return;
                    }
                }
            }
        }

        public InteractiveCommand FindCommandFromSettings(InteractiveSceneModel scene, InteractiveControlModel control)
        {
            return ChannelSession.Settings.InteractiveControls.FirstOrDefault(c => c.GameID.Equals(this.selectedGame.id) && c.SceneID.Equals(scene.sceneID) && c.Name.Equals(control.controlID));
        }

        public InteractiveControlCommandItem FindCommandFromItems(InteractiveControlModel control)
        {
            return this.interactiveItems.FirstOrDefault(i => i.Control.controlID.Equals(control.controlID));
        }
    }
}
