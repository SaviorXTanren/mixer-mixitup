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
        private ObservableCollection<CommandBase> commands = new ObservableCollection<CommandBase>();

        private ObservableCollection<InteractiveGameListingModel> interactiveGames = new ObservableCollection<InteractiveGameListingModel>();
        private InteractiveGameListingModel selectedGame;
        private InteractiveVersionModel selectedGameVersion;

        private List<InteractiveCommand> selectedGameCommands = new List<InteractiveCommand>();
        private Dictionary<string, InteractiveParticipantModel> participants = new Dictionary<string, InteractiveParticipantModel>();

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
                return await MixerAPIHandler.MixerConnection.Interactive.GetOwnedInteractiveGames(this.Window.Channel);
            });

            this.interactiveGames.Clear();
            foreach (InteractiveGameListingModel game in gameListings)
            {
                this.interactiveGames.Add(game);
            }
        }

        private void RefreshList()
        {
            if (MixerAPIHandler.Settings != null)
            {
                this.CommandsListView.ItemsSource = this.commands;
                this.commands.Clear();

                foreach (CommandBase command in MixerAPIHandler.Settings.ChatCommands)
                {
                    this.commands.Add(command);
                }

                foreach (CommandBase command in MixerAPIHandler.Settings.EventCommands)
                {
                    this.commands.Add(command);
                }

                foreach (CommandBase command in MixerAPIHandler.Settings.TimerCommands)
                {
                    this.commands.Add(command);
                }

                this.selectedGameCommands.Clear();
                if (this.selectedGameVersion != null)
                {
                    foreach (InteractiveCommand command in MixerAPIHandler.Settings.InteractiveCommands)
                    {
                        foreach (InteractiveSceneModel scene in this.selectedGameVersion.controls.scenes)
                        {
                            InteractiveControlModel control = scene.allControls.FirstOrDefault(c => c.controlID.Equals(command.Command));
                            if (control != null)
                            {
                                this.selectedGameCommands.Add(command);
                                this.commands.Add(command);
                            }
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

            CommandDetailsWindow window = new CommandDetailsWindow(this.selectedGameVersion, command);
            window.Closed += Window_Closed;
            window.Show();
        }

        private void CommandDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            CommandBase command = (CommandBase)button.DataContext;
            this.commands.Remove(command);

            MixerAPIHandler.Settings.ChatCommands.Remove((ChatCommand)command);
            MixerAPIHandler.Settings.InteractiveCommands.Remove((InteractiveCommand)command);
            MixerAPIHandler.Settings.EventCommands.Remove((EventCommand)command);
            MixerAPIHandler.Settings.TimerCommands.Remove((TimerCommand)command);

            this.CommandsListView.SelectedIndex = -1;

            this.RefreshList();
        }

        private void AddCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandDetailsWindow window = new CommandDetailsWindow(this.selectedGameVersion);
            window.Closed += Window_Closed;
            window.Show();
        }

        private async void InteractiveGameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.InteractiveGameComboBox.SelectedIndex >= 0)
            {
                this.InteractiveGameConnectButton.IsEnabled = true;

                this.selectedGame = (InteractiveGameListingModel)this.InteractiveGameComboBox.SelectedItem;
                this.selectedGameVersion = await this.Window.RunAsyncOperation(async () =>
                {
                    this.selectedGame = this.interactiveGames.First(g => g.id.Equals(this.selectedGame.id));

                    return await MixerAPIHandler.MixerConnection.Interactive.GetInteractiveGameVersion(this.selectedGame.versions.First());
                });

                this.RefreshList();
            }
        }

        private async void Window_Closed(object sender, System.EventArgs e)
        {
            this.RefreshList();
            await MixerAPIHandler.SaveSettings();
        }

        private async void InteractiveGameConnectButton_Click(object sender, RoutedEventArgs e)
        {
            bool result = await this.Window.RunAsyncOperation(async () =>
            {
                return await MixerAPIHandler.ConnectInteractiveClient(this.Window.Channel, this.selectedGame);
            });

            if (!result)
            {
                MessageBoxHelper.ShowError("Unable to connect to interactive with selected game. Please try again.");
                return;
            }

            InteractiveParticipantCollectionModel participants = await this.Window.RunAsyncOperation(async () =>
            {
                return await MixerAPIHandler.InteractiveClient.GetAllParticipants();
            });

            this.participants.Clear();
            foreach (InteractiveParticipantModel participant in participants.participants)
            {
                this.participants.Add(participant.sessionID, participant);
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
                    this.participants[participant.sessionID] = participant;
                }
            }
        }

        private void InteractiveClient_OnParticipantUpdate(object sender, InteractiveParticipantCollectionModel participants)
        {
            if (participants != null)
            {
                foreach (InteractiveParticipantModel participant in participants.participants)
                {
                    this.participants[participant.sessionID] = participant;
                }
            }
        }

        private void InteractiveClient_OnParticipantLeave(object sender, InteractiveParticipantCollectionModel participants)
        {
            if (participants != null)
            {
                foreach (InteractiveParticipantModel participant in participants.participants)
                {
                    this.participants.Remove(participant.sessionID);
                }
            }
        }

        private async void InteractiveClient_OnGiveInput(object sender, InteractiveGiveInputModel sparkTransaction)
        {
            if (sparkTransaction != null && sparkTransaction.input != null)
            {
                foreach (InteractiveCommand command in this.selectedGameCommands)
                {
                    if (command.Command.Equals(sparkTransaction.input.controlID) && command.EventTypeTransactionString.Equals(sparkTransaction.input.eventType))
                    {
                        UserViewModel user = new UserViewModel();
                        if (this.participants.ContainsKey(sparkTransaction.participantID))
                        {
                            InteractiveParticipantModel participant = this.participants[sparkTransaction.participantID];
                            user = new UserViewModel(participant.userID, participant.username);
                        }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        command.Perform(user);
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
