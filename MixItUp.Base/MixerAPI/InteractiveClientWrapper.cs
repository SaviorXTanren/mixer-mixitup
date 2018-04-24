using Mixer.Base.Clients;
using Mixer.Base.Model.Interactive;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Interactive;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.MixerAPI
{
    public class InteractiveConnectedControlCommand
    {
        public InteractiveConnectedSceneModel Scene { get; set; }

        public InteractiveConnectedButtonControlModel Button { get; set; }
        public InteractiveConnectedJoystickControlModel Joystick { get; set; }

        public InteractiveControlModel Control { get { return (this.Button != null) ? (InteractiveControlModel)this.Button : (InteractiveControlModel)this.Joystick; } }

        public InteractiveCommand Command { get; set; }

        public InteractiveConnectedControlCommand(InteractiveConnectedSceneModel scene, InteractiveControlModel control, InteractiveCommand command)
        {
            this.Scene = scene;
            this.Command = command;
            if (control is InteractiveConnectedButtonControlModel)
            {
                this.Button = (InteractiveConnectedButtonControlModel)control;
            }
            else
            {
                this.Joystick = (InteractiveConnectedJoystickControlModel)control;
            }
        }

        public string Name { get { return this.Control.controlID; } }
        public int SparkCost { get { return (this.Button != null) ? this.Button.cost.GetValueOrDefault() : 0; } }
        public long CooldownTimestamp { get { return this.Command.GetCooldownTimestamp(); } }
        public string TriggerTransactionString { get { return (this.Command != null) ? this.Command.TriggerTransactionString : string.Empty; } }
    }

    public class InteractiveClientWrapper : MixerWebSocketWrapper
    {
        private static SemaphoreSlim reconnectionLock = new SemaphoreSlim(1);

        public event EventHandler<InteractiveGiveInputModel> OnGiveInput = delegate { };
        public event EventHandler<InteractiveConnectedSceneModel> OnControlDelete = delegate { };
        public event EventHandler<InteractiveConnectedSceneModel> OnControlCreate = delegate { };
        public event EventHandler<InteractiveConnectedSceneModel> OnControlUpdate = delegate { };
        public event EventHandler<InteractiveConnectedSceneCollectionModel> OnSceneUpdate = delegate { };
        public event EventHandler<Tuple<InteractiveConnectedSceneModel, InteractiveConnectedSceneModel>> OnSceneDelete = delegate { };
        public event EventHandler<InteractiveConnectedSceneCollectionModel> OnSceneCreate = delegate { };
        public event EventHandler<InteractiveGroupCollectionModel> OnGroupUpdate = delegate { };
        public event EventHandler<Tuple<InteractiveGroupModel, InteractiveGroupModel>> OnGroupDelete = delegate { };
        public event EventHandler<InteractiveGroupCollectionModel> OnGroupCreate = delegate { };
        public event EventHandler<InteractiveParticipantCollectionModel> OnParticipantUpdate = delegate { };
        public event EventHandler<InteractiveParticipantCollectionModel> OnParticipantJoin = delegate { };
        public event EventHandler<InteractiveParticipantCollectionModel> OnParticipantLeave = delegate { };
        public event EventHandler<InteractiveIssueMemoryWarningModel> OnIssueMemoryWarning = delegate { };

        public event EventHandler<Tuple<UserViewModel, InteractiveConnectedControlCommand>> OnInteractiveControlUsed = delegate { };

        public InteractiveGameListingModel Game { get; private set; }
        public InteractiveClient Client { get; private set; }

        public List<InteractiveConnectedSceneGroupModel> SceneGroups { get; private set; }
        public Dictionary<string, InteractiveConnectedControlCommand> Controls { get; private set; }
        public LockedDictionary<string, InteractiveParticipantModel> InteractiveUsers { get; private set; }

        private SemaphoreSlim interactiveUserLock = new SemaphoreSlim(1);

        public InteractiveClientWrapper()
        {
            this.SceneGroups = new List<InteractiveConnectedSceneGroupModel>();
            this.Controls = new Dictionary<string, InteractiveConnectedControlCommand>();
            this.InteractiveUsers = new LockedDictionary<string, InteractiveParticipantModel>();
        }

        public async Task<bool> Connect(InteractiveGameListingModel game)
        {
            this.Game = game;

            this.SceneGroups.Clear();
            this.Controls.Clear();
            this.InteractiveUsers.Clear();

            return await this.AttemptConnect();
        }

        public async Task Disconnect()
        {
            if (this.Client != null)
            {
                this.Client.OnDisconnectOccurred -= InteractiveClient_OnDisconnectOccurred;
                if (ChannelSession.Settings.DiagnosticLogging)
                {
                    this.Client.OnPacketSentOccurred -= WebSocketClient_OnPacketSentOccurred;
                    this.Client.OnMethodOccurred -= WebSocketClient_OnMethodOccurred;
                    this.Client.OnReplyOccurred -= WebSocketClient_OnReplyOccurred;
                    this.Client.OnEventOccurred -= WebSocketClient_OnEventOccurred;
                }

                ChannelSession.Chat.OnUserJoinOccurred -= Chat_OnUserJoinOccurred;

                this.Client.OnGiveInput -= Client_OnGiveInput;
                this.Client.OnControlDelete -= Client_OnControlDelete;
                this.Client.OnControlCreate -= Client_OnControlCreate;
                this.Client.OnControlUpdate -= Client_OnControlUpdate;
                this.Client.OnSceneUpdate -= Client_OnSceneUpdate;
                this.Client.OnSceneDelete -= Client_OnSceneDelete;
                this.Client.OnSceneCreate -= Client_OnSceneCreate;
                this.Client.OnGroupUpdate -= Client_OnGroupUpdate;
                this.Client.OnGroupDelete -= Client_OnGroupDelete;
                this.Client.OnGroupCreate -= Client_OnGroupCreate;
                this.Client.OnParticipantUpdate -= Client_OnParticipantUpdate;
                this.Client.OnParticipantJoin -= Client_OnParticipantJoin;
                this.Client.OnParticipantLeave -= Client_OnParticipantLeave;
                this.Client.OnIssueMemoryWarning -= Client_OnIssueMemoryWarning;

                await this.RunAsync(this.Client.Disconnect());

                this.backgroundThreadCancellationTokenSource.Cancel();
            }
            this.Client = null;

            this.SceneGroups.Clear();
            this.Controls.Clear();
            this.InteractiveUsers.Clear();
        }

        public bool IsConnected() { return this.Client != null && this.Client.Authenticated; }

        public async Task<bool> CreateGroups(IEnumerable<InteractiveGroupModel> groups) { return await this.RunAsync(this.Client.CreateGroupsWithResponse(groups)); }
        public async Task<InteractiveGroupCollectionModel> GetGroups() { return await this.RunAsync(this.Client.GetGroups()); }
        public async Task UpdateGroups(IEnumerable<InteractiveGroupModel> groups) { await this.RunAsync(this.Client.UpdateGroups(groups)); }
        public async Task DeleteGroup(InteractiveGroupModel groupToDelete, InteractiveGroupModel groupToReplace) { await this.RunAsync(this.Client.DeleteGroup(groupToDelete, groupToReplace)); }

        public async Task<InteractiveParticipantCollectionModel> GetAllParticipants() { return await this.RunAsync(this.Client.GetAllParticipants()); }
        public async Task UpdateParticipants(IEnumerable<InteractiveParticipantModel> participants) { await this.RunAsync(this.Client.UpdateParticipants(participants)); }

        public async Task<InteractiveConnectedSceneGroupCollectionModel> GetScenes() { return await this.RunAsync(this.Client.GetScenes()); }

        public async Task UpdateControls(InteractiveConnectedSceneModel scene, IEnumerable<InteractiveConnectedButtonControlModel> controls) { await this.RunAsync(this.Client.UpdateControls(scene, controls)); }

        public async Task CaptureSparkTransaction(string transactionID) { await this.RunAsync(this.Client.CaptureSparkTransaction(transactionID)); }

        public async Task DisableAllControlsWithoutCommands(InteractiveGameVersionModel version)
        {
            // Disable all controls that do not have an associated Interactive Command or the Interactive Command is disabled
            foreach (InteractiveSceneModel scene in version.controls.scenes)
            {
                foreach (InteractiveControlModel control in scene.allControls)
                {
                    InteractiveCommand command = this.GetInteractiveCommandForControl(version.gameId, control);
                    control.disabled = (command == null || !command.IsEnabled);
                }
            }
            await ChannelSession.Connection.UpdateInteractiveGameVersion(version);
        }

        protected override async Task<bool> ConnectInternal()
        {
            this.Client = await this.RunAsync(InteractiveClient.CreateFromChannel(ChannelSession.Connection.Connection, ChannelSession.Channel, this.Game));
            if (this.Client != null)
            {
                this.backgroundThreadCancellationTokenSource = new CancellationTokenSource();

                if (await this.RunAsync(this.Client.Connect()) && await this.RunAsync(this.Client.Ready()))
                {
                    this.Client.OnDisconnectOccurred += InteractiveClient_OnDisconnectOccurred;
                    if (ChannelSession.Settings.DiagnosticLogging)
                    {
                        this.Client.OnPacketSentOccurred += WebSocketClient_OnPacketSentOccurred;
                        this.Client.OnMethodOccurred += WebSocketClient_OnMethodOccurred;
                        this.Client.OnReplyOccurred += WebSocketClient_OnReplyOccurred;
                        this.Client.OnEventOccurred += WebSocketClient_OnEventOccurred;
                    }

                    ChannelSession.Chat.OnUserJoinOccurred += Chat_OnUserJoinOccurred;

                    this.Client.OnGiveInput += Client_OnGiveInput;
                    this.Client.OnControlDelete += Client_OnControlDelete;
                    this.Client.OnControlCreate += Client_OnControlCreate;
                    this.Client.OnControlUpdate += Client_OnControlUpdate;
                    this.Client.OnSceneUpdate += Client_OnSceneUpdate;
                    this.Client.OnSceneDelete += Client_OnSceneDelete;
                    this.Client.OnSceneCreate += Client_OnSceneCreate;
                    this.Client.OnGroupUpdate += Client_OnGroupUpdate;
                    this.Client.OnGroupDelete += Client_OnGroupDelete;
                    this.Client.OnGroupCreate += Client_OnGroupCreate;
                    this.Client.OnParticipantUpdate += Client_OnParticipantUpdate;
                    this.Client.OnParticipantJoin += Client_OnParticipantJoin;
                    this.Client.OnParticipantLeave += Client_OnParticipantLeave;
                    this.Client.OnIssueMemoryWarning += Client_OnIssueMemoryWarning;

                    return await this.Initialize();
                }
            }
            return false;
        }

        #region Interactive Update Methods

        private async Task<bool> Initialize()
        {
            this.SceneGroups.Clear();
            this.Controls.Clear();
            this.InteractiveUsers.Clear();

            // Initialize Scenes
            InteractiveConnectedSceneGroupCollectionModel scenes = await ChannelSession.Interactive.GetScenes();
            if (scenes == null)
            {
                return false;
            }

            this.SceneGroups = scenes.scenes;
            foreach (InteractiveConnectedSceneModel scene in this.SceneGroups)
            {
                foreach (InteractiveConnectedButtonControlModel button in scene.buttons)
                {
                    this.AddConnectedControl(scene, button);
                }

                foreach (InteractiveConnectedJoystickControlModel joystick in scene.joysticks)
                {
                    this.AddConnectedControl(scene, joystick);
                }
            }

            // Initialize Groups
            List<InteractiveGroupModel> groupsToAdd = new List<InteractiveGroupModel>();
            foreach (InteractiveUserGroupViewModel userGroup in ChannelSession.Settings.InteractiveUserGroups[this.Client.InteractiveGame.id])
            {
                if (userGroup.IsEnabled)
                {
                    groupsToAdd.Add(new InteractiveGroupModel() { groupID = userGroup.GroupName, sceneID = userGroup.DefaultScene });
                }
            }

            if (groupsToAdd.Count > 0 && !await this.CreateGroups(groupsToAdd))
            {
                return false;
            }

            // Initialize Participants
            List<InteractiveParticipantModel> participantsToAdd = new List<InteractiveParticipantModel>();
            List<InteractiveParticipantModel> participantsToLookUp = new List<InteractiveParticipantModel>();

            InteractiveParticipantCollectionModel participants = await this.GetAllParticipants();
            if (participants != null)
            {
                foreach (InteractiveParticipantModel participant in participants.participants)
                {
                    if (ChannelSession.Chat.ChatUsers.ContainsKey(participant.userID))
                    {
                        participantsToAdd.Add(participant);
                    }
                    else
                    {
                        participantsToLookUp.Add(participant);
                    }
                }
            }

            await this.AddParticipants(participantsToAdd);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                foreach (InteractiveParticipantModel participant in participantsToLookUp)
                {
                    await ChannelSession.Chat.GetAndAddUser(participant.userID);
                }
                this.AddParticipants(participantsToLookUp);
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return true;
        }

        private void AddConnectedControl(InteractiveConnectedSceneModel scene, InteractiveControlModel control)
        {
            InteractiveCommand command = this.GetInteractiveCommandForControl(this.Client.InteractiveGame.id, control);
            if (command != null)
            {
                command.UpdateWithLatestControl(control);
                this.Controls[control.controlID] = new InteractiveConnectedControlCommand(scene, control, command);
            }
        }

        private async Task AddParticipants(IEnumerable<InteractiveParticipantModel> participants)
        {
            List<InteractiveParticipantModel> participantsToUpdate = new List<InteractiveParticipantModel>();
            foreach (InteractiveParticipantModel participant in participants)
            {
                if (!string.IsNullOrEmpty(participant.sessionID) && !this.InteractiveUsers.ContainsKey(participant.sessionID))
                {
                    participantsToUpdate.Add(participant);
                    if (ChannelSession.Chat.ChatUsers.ContainsKey(participant.userID))
                    {
                        UserRole role = ChannelSession.Chat.ChatUsers[participant.userID].PrimaryRole;
                        InteractiveUserGroupViewModel group = ChannelSession.Settings.InteractiveUserGroups[this.Client.InteractiveGame.id].FirstOrDefault(g => g.AssociatedUserRole == role);
                        if (group != null)
                        {
                            participant.groupID = group.GroupName;
                        }
                    }
                }
                this.InteractiveUsers[participant.sessionID] = participant;
            }

            if (participantsToUpdate.Any(p => !p.groupID.Equals(InteractiveUserGroupViewModel.DefaultName)))
            {
                await ChannelSession.Interactive.UpdateParticipants(participantsToUpdate);
            }
        }

        private InteractiveCommand GetInteractiveCommandForControl(uint gameID, InteractiveControlModel control)
        {
            return ChannelSession.Settings.InteractiveCommands.FirstOrDefault(c => c.GameID.Equals(gameID) && c.Control.controlID.Equals(control.controlID));
        }

        #endregion Interactive Update Methods

        #region Interactive Event Handlers

        private void Client_OnIssueMemoryWarning(object sender, InteractiveIssueMemoryWarningModel e) { this.OnIssueMemoryWarning(this, e); }

        private void Client_OnGroupCreate(object sender, InteractiveGroupCollectionModel e) { this.OnGroupCreate(this, e); }

        private void Client_OnGroupDelete(object sender, Tuple<InteractiveGroupModel, InteractiveGroupModel> e) { this.OnGroupDelete(this, e); }

        private void Client_OnGroupUpdate(object sender, InteractiveGroupCollectionModel e) { this.OnGroupUpdate(this, e); }

        private void Client_OnSceneCreate(object sender, InteractiveConnectedSceneCollectionModel e) { this.OnSceneCreate(this, e); }

        private void Client_OnSceneDelete(object sender, Tuple<InteractiveConnectedSceneModel, InteractiveConnectedSceneModel> e) { this.OnSceneDelete(this, e); }

        private void Client_OnSceneUpdate(object sender, InteractiveConnectedSceneCollectionModel e) { this.OnSceneUpdate(this, e); }

        private void Client_OnControlCreate(object sender, InteractiveConnectedSceneModel e) { this.OnControlCreate(this, e); }

        private void Client_OnControlDelete(object sender, InteractiveConnectedSceneModel e) { this.OnControlDelete(this, e); }

        private void Client_OnControlUpdate(object sender, InteractiveConnectedSceneModel e) { this.OnControlUpdate(this, e); }

        private void Client_OnParticipantLeave(object sender, InteractiveParticipantCollectionModel e)
        {
            if (e != null)
            {
                foreach (InteractiveParticipantModel participant in e.participants)
                {
                    this.InteractiveUsers.Remove(participant.sessionID);
                }
            }
            this.OnParticipantLeave(this, e);
        }

        private async void Client_OnParticipantJoin(object sender, InteractiveParticipantCollectionModel e)
        {
            if (e.participants != null)
            {
                await this.AddParticipants(e.participants);
            }
            this.OnParticipantJoin(this, e);
        }

        private async void Client_OnParticipantUpdate(object sender, InteractiveParticipantCollectionModel e)
        {
            if (e.participants != null)
            {
                await this.AddParticipants(e.participants);
            }
            this.OnParticipantUpdate(this, e);
        }

        private async void Client_OnGiveInput(object sender, InteractiveGiveInputModel e)
        {
            if (e != null && e.input != null)
            {
                if (this.Controls.ContainsKey(e.input.controlID))
                {
                    InteractiveConnectedControlCommand connectedControl = this.Controls[e.input.controlID];

                    if (!connectedControl.Command.IsEnabled)
                    {
                        return;
                    }

                    if (connectedControl.Button != null && !connectedControl.TriggerTransactionString.Equals(e.input.eventType))
                    {
                        return;
                    }

                    if (connectedControl.Button != null)
                    {
                        connectedControl.Button.cooldown = connectedControl.Command.GetCooldownTimestamp();

                        List<InteractiveConnectedButtonControlModel> buttons = new List<InteractiveConnectedButtonControlModel>();
                        if (!string.IsNullOrEmpty(connectedControl.Command.CooldownGroup))
                        {
                            var otherItems = this.Controls.Values.Where(c => c.Button != null && connectedControl.Command.CooldownGroup.Equals(c.Command.CooldownGroup));
                            foreach (var otherItem in otherItems)
                            {
                                otherItem.Button.cooldown = connectedControl.Button.cooldown;
                            }
                            buttons.AddRange(otherItems.Select(i => i.Button));
                        }
                        else
                        {
                            buttons.Add(connectedControl.Button);
                        }

                        await this.UpdateControls(connectedControl.Scene, buttons);
                    }
                    else if (connectedControl.Joystick != null)
                    {

                    }

                    if (!string.IsNullOrEmpty(e.transactionID))
                    {
                        await this.CaptureSparkTransaction(e.transactionID);
                    }

                    UserViewModel user = ChannelSession.GetCurrentUser();
                    if (this.InteractiveUsers.ContainsKey(e.participantID))
                    {
                        InteractiveParticipantModel participant = this.InteractiveUsers[e.participantID];
                        if (ChannelSession.Chat.ChatUsers.ContainsKey(participant.userID))
                        {
                            user = ChannelSession.Chat.ChatUsers[participant.userID];
                        }
                        else
                        {
                            user = new UserViewModel(participant.userID, participant.username);
                        }
                    }

                    await connectedControl.Command.Perform(user);

                    if (this.OnInteractiveControlUsed != null)
                    {
                        this.OnInteractiveControlUsed(this, new Tuple<UserViewModel, InteractiveConnectedControlCommand>(user, connectedControl));
                    }
                }
            }

            this.OnGiveInput(this, e);
        }

        private async void Chat_OnUserJoinOccurred(object sender, UserViewModel user)
        {
            InteractiveParticipantModel participant = this.InteractiveUsers.Values.ToList().FirstOrDefault(u => u.userID.Equals(user.ID));
            if (participant != null)
            {
                await this.AddParticipants(new List<InteractiveParticipantModel>() { participant });
            }
        }

        private async void InteractiveClient_OnDisconnectOccurred(object sender, WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred("Interactive");

            do
            {
                await InteractiveClient.Reconnect(this.Client);
            } while (!await this.RunAsync(this.Client.Ready()));

            ChannelSession.ReconnectionOccurred("Interactive");
        }

        #endregion Interactive Event Handlers
    }
}
