using Mixer.Base.Interactive;
using Mixer.Base.Model.Interactive;
using Mixer.Base.ViewModel;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Interactive;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Interactive
{
    public class InteractiveControlCommandItem
    {
        public InteractiveControlModel Control { get; set; }
        public InteractiveCommand Command { get; set; }

        public InteractiveControlCommandItem(InteractiveControlModel control, InteractiveCommand command)
        {
            this.Control = control;
            this.Command = command;
        }

        public string Name { get { return this.Control.controlID; } }
        public string Type { get { return (this.Control is InteractiveButtonControlModel) ? "Button" : "Joystick"; } }
    }

    /// <summary>
    /// Interaction logic for InteractiveControl.xaml
    /// </summary>
    public partial class InteractiveControl : MainControlBase
    {
        private ObservableCollection<InteractiveGameListingModel> interactiveGames = new ObservableCollection<InteractiveGameListingModel>();

        private ObservableCollection<InteractiveSceneModel> interactiveScenes = new ObservableCollection<InteractiveSceneModel>();
        private InteractiveSceneModel selectedScene = null;

        private ObservableCollection<InteractiveControlCommandItem> interactiveItems = new ObservableCollection<InteractiveControlCommandItem>();

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
                return await MixerAPIHandler.MixerConnection.Interactive.GetOwnedInteractiveGames(ChannelSession.Channel);
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

            this.interactiveScenes.Clear();

            ChannelSession.SelectedGameVersion = await this.Window.RunAsyncOperation(async () =>
            {
                ChannelSession.SelectedGame = this.interactiveGames.First(g => g.id.Equals(ChannelSession.SelectedGame.id));

                return await MixerAPIHandler.MixerConnection.Interactive.GetInteractiveGameVersion(ChannelSession.SelectedGame.versions.First());
            });

            if (ChannelSession.SelectedGame != null)
            {
                this.InteractiveGamesComboBox.SelectedItem = ChannelSession.SelectedGame;
            }

            foreach (InteractiveSceneModel scene in ChannelSession.SelectedGameVersion.controls.scenes)
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

            this.SaveChangedButton.IsEnabled = true;
            this.GameDetailsGrid.IsEnabled = true;
        }

        private void RefreshSelectedScene()
        {
            this.interactiveItems.Clear();
            if (this.selectedScene != null)
            {
                foreach (InteractiveButtonControlModel button in this.selectedScene.buttons)
                {
                    this.interactiveItems.Add(new InteractiveControlCommandItem(button, this.FindCommand(button)));
                }

                foreach (InteractiveJoystickControlModel joystick in this.selectedScene.joysticks)
                {
                    this.interactiveItems.Add(new InteractiveControlCommandItem(joystick, this.FindCommand(joystick)));
                }
            }
        }

        private InteractiveCommand FindCommand(InteractiveControlModel control)
        {
            return ChannelSession.Settings.InteractiveControls.FirstOrDefault(c => c.GameID.Equals(ChannelSession.SelectedGame.id) && c.SceneID.Equals(this.selectedScene.sceneID) && c.Name.Equals(control.controlID));
        }

        private async void InteractiveGamesComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.InteractiveGamesComboBox.SelectedIndex >= 0)
            {
                InteractiveGameListingModel newSelectedGame = (InteractiveGameListingModel)this.InteractiveGamesComboBox.SelectedItem;
                if (ChannelSession.SelectedGame != newSelectedGame)
                {
                    ChannelSession.SelectedGame = newSelectedGame;
                    await this.RefreshSelectedInteractiveGame();
                    this.ConnectButton.IsEnabled = true;
                }
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

        private async void CommandTestButton_Click(object sender, RoutedEventArgs e)
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

        private void CommandEditButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            InteractiveControlCommandItem command = (InteractiveControlCommandItem)button.DataContext;

            InteractiveCommandWindow window = (command.Command == null) ? new InteractiveCommandWindow() : new InteractiveCommandWindow(command.Command);
            window.Closed += Window_Closed;
            window.Show();
        }

        private async void CommandDeleteButton_Click(object sender, RoutedEventArgs e)
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

        private async void Window_Closed(object sender, System.EventArgs e)
        {
            this.RefreshSelectedScene();
            await ChannelSession.Settings.SaveSettings();
        }

        private async void SaveChangedButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (InteractiveSceneModel scene in this.interactiveScenes)
            {
                if (scene.buttons.Count == 0 && scene.joysticks.Count == 0)
                {
                    MessageBoxHelper.ShowError("The following scene does not contain any controls: " + scene.sceneID);
                    return;
                }
            }

            if (ChannelSession.SelectedGame == null)
            {
                await this.Window.RunAsyncOperation(async () =>
                {
                    await MixerAPIHandler.MixerConnection.Interactive.UpdateInteractiveGame(ChannelSession.SelectedGame);
                    await MixerAPIHandler.MixerConnection.Interactive.UpdateInteractiveGameVersion(ChannelSession.SelectedGameVersion);
                });
            }

            await this.RefreshSelectedInteractiveGame();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            bool result = await this.Window.RunAsyncOperation(async () =>
            {
                return await MixerAPIHandler.ConnectInteractiveClient(ChannelSession.Channel, ChannelSession.SelectedGame);
            });

            if (!result)
            {
                MessageBoxHelper.ShowError("Unable to connect to interactive with selected game. Please try again.");
                return;
            }

            InteractiveConnectedSceneGroupCollectionModel scenes = await this.Window.RunAsyncOperation(async () =>
            {
                return await MixerAPIHandler.InteractiveClient.GetScenes();
            });
            ChannelSession.ConnectedGameScenes = scenes.scenes;
            ChannelSession.ConnectedScene = ChannelSession.ConnectedGameScenes.First();

            InteractiveParticipantCollectionModel participants = await this.Window.RunAsyncOperation(async () =>
            {
                return await MixerAPIHandler.InteractiveClient.GetAllParticipants();
            });

            ChannelSession.InteractiveUsers.Clear();
            foreach (InteractiveParticipantModel participant in participants.participants)
            {
                ChannelSession.InteractiveUsers.Add(participant.sessionID, participant);
            }

            MixerAPIHandler.InteractiveClient.OnParticipantJoin += InteractiveClient_OnParticipantJoin;
            MixerAPIHandler.InteractiveClient.OnParticipantUpdate += InteractiveClient_OnParticipantUpdate;
            MixerAPIHandler.InteractiveClient.OnParticipantLeave += InteractiveClient_OnParticipantLeave;
            MixerAPIHandler.InteractiveClient.OnGiveInput += InteractiveClient_OnGiveInput;

            this.GameSelectionGrid.IsEnabled = false;
            this.GameDetailsGrid.IsEnabled = false;
            this.ConnectButton.Visibility = Visibility.Collapsed;
            this.DisconnectButton.Visibility = Visibility.Visible;
        }

        private async void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            MixerAPIHandler.InteractiveClient.OnParticipantJoin -= InteractiveClient_OnParticipantJoin;
            MixerAPIHandler.InteractiveClient.OnParticipantUpdate -= InteractiveClient_OnParticipantUpdate;
            MixerAPIHandler.InteractiveClient.OnParticipantLeave -= InteractiveClient_OnParticipantLeave;
            MixerAPIHandler.InteractiveClient.OnGiveInput -= InteractiveClient_OnGiveInput;

            await this.Window.RunAsyncOperation(async () =>
            {
                await MixerAPIHandler.DisconnectInteractiveClient();
            });

            ChannelSession.ConnectedGameScenes.Clear();
            ChannelSession.ConnectedScene = null;

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

        private async void InteractiveClient_OnGiveInput(object sender, InteractiveGiveInputModel sparkTransaction)
        {
            if (sparkTransaction != null && sparkTransaction.input != null)
            {
                foreach (InteractiveCommand command in ChannelSession.Settings.InteractiveControls)
                {
                    if (command.Name.Equals(sparkTransaction.input.controlID) && command.EventTypeTransactionString.Equals(sparkTransaction.input.eventType))
                    {
                        UserViewModel user = ChannelSession.GetCurrentUser();
                        if (ChannelSession.InteractiveUsers.ContainsKey(sparkTransaction.participantID))
                        {
                            InteractiveParticipantModel participant = ChannelSession.InteractiveUsers[sparkTransaction.participantID];
                            user = new UserViewModel(participant.userID, participant.username);
                        }

                        if (!string.IsNullOrEmpty(sparkTransaction.transactionID))
                        {
                            bool result = await this.Window.RunAsyncOperation(async () =>
                            {
                                return await MixerAPIHandler.InteractiveClient.CaptureSparkTransaction(sparkTransaction.transactionID);
                            });

                            if (!result)
                            {
                                MessageBoxHelper.ShowError("Failed to capture spark transaction for the following command: " + sparkTransaction.input.controlID);
                                return;
                            }
                        }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        command.Perform(user, new List<string>() { command.Command });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                        return;
                    }
                }
            }
        }
    }
}
