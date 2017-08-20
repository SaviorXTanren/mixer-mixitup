using MixItUp.Base;
using MixItUp.Base.Commands;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using Mixer.Base.Model.Interactive;
using MixItUp.WPF.Util;
using System.Collections.Generic;
using System.Linq;
using Mixer.Base.ViewModel;

namespace MixItUp.WPF.Controls.Commands
{
    /// <summary>
    /// Interaction logic for CommandsControl.xaml
    /// </summary>
    public partial class CommandsControl : MainControlBase
    {
        private ObservableCollection<InteractiveGameListingModel> interactiveGames = new ObservableCollection<InteractiveGameListingModel>();

        public CommandsControl()
        {
            InitializeComponent();

            this.InteractiveGameComboBox.ItemsSource = this.interactiveGames;
        }

        protected override async Task InitializeInternal()
        {
            this.RefreshList();

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

        private void RefreshList()
        {
            this.CommandsListView.ItemsSource = ChannelSession.ActiveCommands;
            ChannelSession.ActiveCommands.Clear();

            foreach (CommandBase command in ChannelSession.Settings.ChatCommands)
            {
                ChannelSession.ActiveCommands.Add(command);
            }

            foreach (CommandBase command in ChannelSession.Settings.EventCommands)
            {
                ChannelSession.ActiveCommands.Add(command);
            }

            foreach (CommandBase command in ChannelSession.Settings.TimerCommands)
            {
                ChannelSession.ActiveCommands.Add(command);
            }

            if (ChannelSession.SelectedGameVersion != null)
            {
                foreach (InteractiveCommand command in ChannelSession.Settings.InteractiveCommands)
                {
                    foreach (InteractiveSceneModel scene in ChannelSession.SelectedGameVersion.controls.scenes)
                    {
                        InteractiveControlModel control = scene.allControls.FirstOrDefault(c => c.controlID.Equals(command.Command));
                        if (control != null)
                        {
                            ChannelSession.ActiveCommands.Add(command);
                        }
                    }
                }
            }
        }

        private async void CommandTestButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            CommandBase command = (CommandBase)button.DataContext;

            UserViewModel testUser = new UserViewModel(1, "Test User");

            await this.Window.RunAsyncOperation(async () =>
            {
                await command.Perform(testUser);
            });
        }

        private void CommandEditButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            CommandBase command = (CommandBase)button.DataContext;

            CommandDetailsWindow window = new CommandDetailsWindow(command);
            window.Closed += Window_Closed;
            window.Show();
        }

        private void CommandDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            CommandBase command = (CommandBase)button.DataContext;
            ChannelSession.ActiveCommands.Remove(command);

            ChannelSession.Settings.RemoveCommand(command);

            this.CommandsListView.SelectedIndex = -1;

            this.RefreshList();
        }

        private void AddCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandDetailsWindow window = new CommandDetailsWindow();
            window.Closed += Window_Closed;
            window.Show();
        }

        private async void InteractiveGameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.InteractiveGameComboBox.SelectedIndex >= 0)
            {
                this.InteractiveGameConnectButton.IsEnabled = true;

                ChannelSession.SelectedGame = (InteractiveGameListingModel)this.InteractiveGameComboBox.SelectedItem;
                ChannelSession.SelectedGameVersion = await this.Window.RunAsyncOperation(async () =>
                {
                    ChannelSession.SelectedGame = this.interactiveGames.First(g => g.id.Equals(ChannelSession.SelectedGame.id));

                    return await MixerAPIHandler.MixerConnection.Interactive.GetInteractiveGameVersion(ChannelSession.SelectedGame.versions.First());
                });

                this.RefreshList();
            }
        }

        private async void Window_Closed(object sender, System.EventArgs e)
        {
            this.RefreshList();
            await ChannelSession.Settings.SaveSettings();
        }

        private async void InteractiveGameConnectButton_Click(object sender, RoutedEventArgs e)
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
            ChannelSession.SelectedScenes = scenes.scenes;
            ChannelSession.SelectedScene = ChannelSession.SelectedScenes.First();

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

            this.CommandsListView.IsEnabled = false;
            this.AddCommandButton.IsEnabled = false;
            this.InteractiveGameComboBox.IsEnabled = false;
            this.InteractiveGameConnectButton.Visibility = Visibility.Collapsed;
            this.InteractiveGameDisconnectButton.Visibility = Visibility.Visible;
        }

        private async void InteractiveGameDisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            MixerAPIHandler.InteractiveClient.OnParticipantJoin -= InteractiveClient_OnParticipantJoin;
            MixerAPIHandler.InteractiveClient.OnParticipantUpdate -= InteractiveClient_OnParticipantUpdate;
            MixerAPIHandler.InteractiveClient.OnParticipantLeave -= InteractiveClient_OnParticipantLeave;
            MixerAPIHandler.InteractiveClient.OnGiveInput -= InteractiveClient_OnGiveInput;

            await this.Window.RunAsyncOperation(async () =>
            {
                await MixerAPIHandler.DisconnectInteractiveClient();
            });

            ChannelSession.SelectedScenes.Clear();
            ChannelSession.SelectedScene = null;

            this.CommandsListView.IsEnabled = true;
            this.AddCommandButton.IsEnabled = true;
            this.InteractiveGameComboBox.IsEnabled = true;
            this.InteractiveGameDisconnectButton.Visibility = Visibility.Collapsed;
            this.InteractiveGameConnectButton.Visibility = Visibility.Visible;
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
                foreach (InteractiveCommand command in ChannelSession.GetCommands<InteractiveCommand>())
                {
                    if (command.Command.Equals(sparkTransaction.input.controlID) && command.EventTypeTransactionString.Equals(sparkTransaction.input.eventType))
                    {
                        UserViewModel user = new UserViewModel();
                        if (ChannelSession.InteractiveUsers.ContainsKey(sparkTransaction.participantID))
                        {
                            InteractiveParticipantModel participant = ChannelSession.InteractiveUsers[sparkTransaction.participantID];
                            user = new UserViewModel(participant.userID, participant.username);
                        }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        command.Perform(user, new List<string>() { command.Command });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

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

                        return;
                    }
                }
            }
        }
    }
}
