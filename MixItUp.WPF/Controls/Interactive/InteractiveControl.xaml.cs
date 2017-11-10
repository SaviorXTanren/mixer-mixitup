using Mixer.Base.Model.Interactive;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.MixerAPI;
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

        public InteractiveConnectedButtonControlModel ConnectedButton
        {
            get { return (this.Button != null && this.Button is InteractiveConnectedButtonControlModel) ? (InteractiveConnectedButtonControlModel)this.Button : null; }
        }
        public InteractiveConnectedJoystickControlModel ConnectedJoystick
        {
            get { return (this.Joystick != null && this.Joystick is InteractiveConnectedJoystickControlModel) ? (InteractiveConnectedJoystickControlModel)this.Joystick : null; }
        }

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

        private ObservableCollection<InteractiveControlCommandItem> currentSceneControlItems = new ObservableCollection<InteractiveControlCommandItem>();

        private List<InteractiveConnectedSceneGroupModel> connectedSceneGroups = new List<InteractiveConnectedSceneGroupModel>();
        private Dictionary<string, List<InteractiveControlCommandItem>> connectedSceneControls = new Dictionary<string, List<InteractiveControlCommandItem>>();

        public InteractiveControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            GlobalEvents.OnChatUserJoined += GlobalEvents_OnChatUserJoined;

            this.InteractiveGamesComboBox.ItemsSource = this.interactiveGames;
            this.InteractiveScenesComboBox.ItemsSource = this.interactiveScenes;
            this.InteractiveControlsGridView.ItemsSource = this.currentSceneControlItems;

            await this.RefreshAllInteractiveGames();
        }

        private async Task RefreshAllInteractiveGames()
        {
            this.GroupsButton.IsEnabled = false;
            this.RefreshButton.IsEnabled = false;
            this.ConnectButton.IsEnabled = false;

            IEnumerable<InteractiveGameListingModel> gameListings = await this.Window.RunAsyncOperation(async () =>
            {
                return await ChannelSession.Connection.GetOwnedInteractiveGames(ChannelSession.Channel);
            });

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
            foreach (InteractiveSceneModel scene in this.selectedGameVersion.controls.scenes)
            {
                this.interactiveScenes.Add(scene);
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
            this.RefreshButton.IsEnabled = true;
            this.ConnectButton.IsEnabled = true;
        }

        private void RefreshSelectedScene()
        {
            this.currentSceneControlItems.Clear();
            foreach (InteractiveButtonControlModel button in this.selectedScene.buttons)
            {
                this.currentSceneControlItems.Add(this.CreateControlItem(this.selectedScene.sceneID, button));
            }

            foreach (InteractiveJoystickControlModel joystick in this.selectedScene.joysticks)
            {
                this.currentSceneControlItems.Add(this.CreateControlItem(this.selectedScene.sceneID, joystick));
            }
        }

        private InteractiveControlCommandItem CreateControlItem(string sceneID, InteractiveControlModel control)
        {
            InteractiveCommand command = this.FindCommandForControl(sceneID, control);

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

        private InteractiveCommand FindCommandForControl(string sceneID, InteractiveControlModel control)
        {
            return ChannelSession.Settings.InteractiveControls.FirstOrDefault(c => c.GameID.Equals(this.selectedGame.id) &&
                c.SceneID.Equals(sceneID) && c.Control.controlID.Equals(control.controlID));
        }

        private void AssignDefaultGroupForParticipant(InteractiveParticipantModel participant)
        {
            if (ChatClientWrapper.ChatUsers.ContainsKey(participant.userID))
            {
                UserRole role = ChatClientWrapper.ChatUsers[participant.userID].PrimaryRole;
                InteractiveUserGroupViewModel group = ChannelSession.Settings.InteractiveUserGroups[this.selectedGame.id].FirstOrDefault(g => g.AssociatedUserRole == role);
                if (group != null)
                {
                    participant.groupID = group.GroupName;
                }
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

        private async void GroupsButton_Click(object sender, RoutedEventArgs e)
        {
            InteractiveSceneUserGroupsDialogControl dialogControl = new InteractiveSceneUserGroupsDialogControl(this.selectedGame, this.selectedScene);
            await this.Window.RunAsyncOperation(async () =>
            {
                await MessageBoxHelper.ShowCustomDialog(dialogControl);
            });
        }

        private async void InteractiveGamesComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.InteractiveGamesComboBox.SelectedIndex >= 0)
            {
                this.selectedGame = (InteractiveGameListingModel)this.InteractiveGamesComboBox.SelectedItem;
                await this.RefreshSelectedGame();
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

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RefreshAllInteractiveGames();
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
            try
            {
                await this.Window.RunAsyncOperation(async () =>
                {
                    return await ChannelSession.ConnectInteractive(this.selectedGame);
                });

                List<InteractiveGroupModel> groupsToAdd = new List<InteractiveGroupModel>();
                foreach (InteractiveUserGroupViewModel userGroup in ChannelSession.Settings.InteractiveUserGroups[this.selectedGame.id].Skip(1))
                {
                    groupsToAdd.Add(new InteractiveGroupModel() { groupID = userGroup.GroupName, sceneID = userGroup.DefaultScene });
                }

                await this.Window.RunAsyncOperation(async () =>
                {
                    return await ChannelSession.Interactive.CreateGroups(groupsToAdd);
                });

                InteractiveParticipantCollectionModel participants = await this.Window.RunAsyncOperation(async () =>
                {
                    return await ChannelSession.Interactive.GetAllParticipants();
                });

                InteractiveClientWrapper.InteractiveUsers.Clear();
                if (participants != null)
                {
                    foreach (InteractiveParticipantModel participant in participants.participants)
                    {
                        InteractiveClientWrapper.InteractiveUsers.Add(participant.sessionID, participant);
                    }
                }

                List<InteractiveParticipantModel> participantsToLookUp = new List<InteractiveParticipantModel>();
                foreach (InteractiveParticipantModel participant in InteractiveClientWrapper.InteractiveUsers.Values)
                {
                    if (ChatClientWrapper.ChatUsers.ContainsKey(participant.userID))
                    {
                        this.AssignDefaultGroupForParticipant(participant);
                    }
                    else
                    {
                        participantsToLookUp.Add(participant);
                    }
                }

                if (InteractiveClientWrapper.InteractiveUsers.Values.Count > 0)
                {
                    await this.Window.RunAsyncOperation(async () =>
                    {
                        return await ChannelSession.Interactive.UpdateParticipants(InteractiveClientWrapper.InteractiveUsers.Values);
                    });
                }

                InteractiveConnectedSceneGroupCollectionModel scenes = await this.Window.RunAsyncOperation(async () =>
                {
                    return await ChannelSession.Interactive.GetScenes();
                });

                this.connectedSceneGroups = scenes.scenes;

                this.connectedSceneControls.Clear();
                foreach (InteractiveConnectedSceneModel scene in this.connectedSceneGroups)
                {
                    this.connectedSceneControls.Add(scene.sceneID, new List<InteractiveControlCommandItem>());

                    foreach (InteractiveConnectedButtonControlModel button in scene.buttons)
                    {
                        this.connectedSceneControls[scene.sceneID].Add(this.CreateControlItem(scene.sceneID, button));
                    }

                    foreach (InteractiveConnectedJoystickControlModel joystick in scene.joysticks)
                    {
                        this.connectedSceneControls[scene.sceneID].Add(this.CreateControlItem(scene.sceneID, joystick));
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
            catch (Exception ex)
            {
                Logger.Log(ex);

                await MessageBoxHelper.ShowMessageDialog("Failed to connect to interactive with selected game. Please try again.");
                return;
            }
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

            this.connectedSceneGroups.Clear();
            this.connectedSceneControls.Clear();

            this.GameSelectionGrid.IsEnabled = true;
            this.GameDetailsGrid.IsEnabled = true;
            this.ConnectButton.Visibility = Visibility.Visible;
            this.DisconnectButton.Visibility = Visibility.Collapsed;
        }

        private async void InteractiveClient_OnParticipantJoin(object sender, InteractiveParticipantCollectionModel participants)
        {
            if (participants != null)
            {
                foreach (InteractiveParticipantModel participant in participants.participants)
                {
                    InteractiveClientWrapper.InteractiveUsers[participant.sessionID] = participant;
                    this.AssignDefaultGroupForParticipant(participant);
                    await ChannelSession.Interactive.UpdateParticipants(new List<InteractiveParticipantModel>() { participant });
                }
            }
        }

        private async void InteractiveClient_OnParticipantUpdate(object sender, InteractiveParticipantCollectionModel participants)
        {
            if (participants != null)
            {
                foreach (InteractiveParticipantModel participant in participants.participants)
                {
                    InteractiveClientWrapper.InteractiveUsers[participant.sessionID] = participant;
                    this.AssignDefaultGroupForParticipant(participant);
                    await ChannelSession.Interactive.UpdateParticipants(new List<InteractiveParticipantModel>() { participant });
                }
            }
        }

        private void InteractiveClient_OnParticipantLeave(object sender, InteractiveParticipantCollectionModel participants)
        {
            if (participants != null)
            {
                foreach (InteractiveParticipantModel participant in participants.participants)
                {
                    InteractiveClientWrapper.InteractiveUsers.Remove(participant.sessionID);
                }
            }
        }

        private async void InteractiveClient_OnGiveInput(object sender, InteractiveGiveInputModel interactiveInput)
        {
            if (interactiveInput != null && interactiveInput.input != null)
            {
                InteractiveControlCommandItem controlCommand = null;
                foreach (var kvp in this.connectedSceneControls)
                {
                    controlCommand = kvp.Value.FirstOrDefault(ci => ci.Control.controlID.Equals(interactiveInput.input.controlID));
                    if (controlCommand != null)
                    {
                        break;
                    }
                }

                if (controlCommand != null)
                {
                    InteractiveConnectedSceneModel scene = this.connectedSceneGroups.FirstOrDefault(s => s.sceneID.Equals(controlCommand.Scene.sceneID));

                    if (controlCommand.ConnectedButton != null && !controlCommand.TriggerTransactionString.Equals(interactiveInput.input.eventType))
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

                    GlobalEvents.InteractiveControlUsed(controlCommand.Control);

                    UserViewModel user = ChannelSession.GetCurrentUser();
                    if (InteractiveClientWrapper.InteractiveUsers.ContainsKey(interactiveInput.participantID))
                    {
                        InteractiveParticipantModel participant = InteractiveClientWrapper.InteractiveUsers[interactiveInput.participantID];
                        if (ChatClientWrapper.ChatUsers.ContainsKey(participant.userID))
                        {
                            user = ChatClientWrapper.ChatUsers[participant.userID];
                        }
                        else
                        {
                            user = new UserViewModel(participant.userID, participant.username);
                        }
                    }

                    if (controlCommand.ConnectedButton != null)
                    {
                        await this.Window.RunAsyncOperation(async () =>
                        {
                            controlCommand.ConnectedButton.cooldown = controlCommand.Command.GetCooldownTimestamp();

                            List<InteractiveConnectedButtonControlModel> buttons = new List<InteractiveConnectedButtonControlModel>();
                            if (!string.IsNullOrEmpty(controlCommand.Command.CooldownGroup))
                            {
                                var otherItems = this.connectedSceneControls[scene.sceneID].Where(i => i.Command != null && i.ConnectedButton != null);
                                otherItems = otherItems.Where(i => controlCommand.Command.CooldownGroup.Equals(i.Command.CooldownGroup));

                                foreach (var otherItem in otherItems)
                                {
                                    otherItem.ConnectedButton.cooldown = controlCommand.ConnectedButton.cooldown;
                                }
                                buttons.AddRange(otherItems.Select(i => i.ConnectedButton));
                            }
                            else
                            {
                                buttons.Add(controlCommand.ConnectedButton);
                            }

                            await ChannelSession.Interactive.UpdateControls(scene, buttons);
                        });
                    }
                    else if (controlCommand.ConnectedJoystick != null)
                    {

                    }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    controlCommand.Command.Perform(user);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    return;
                }
            }
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                await this.RefreshSelectedGame();
            });
        }

        private async void GlobalEvents_OnChatUserJoined(object sender, UserViewModel e)
        {
            if (ChannelSession.Interactive != null && ChannelSession.Interactive.Client.Authenticated)
            {
                InteractiveParticipantModel participant = InteractiveClientWrapper.InteractiveUsers.Values.FirstOrDefault(u => u.userID.Equals(e.ID));
                if (participant != null)
                {
                    this.AssignDefaultGroupForParticipant(participant);
                    await this.Window.RunAsyncOperation(async () =>
                    {
                        return await ChannelSession.Interactive.UpdateParticipants(new List<InteractiveParticipantModel>() { participant });
                    });
                }
            }
        }
    }
}
