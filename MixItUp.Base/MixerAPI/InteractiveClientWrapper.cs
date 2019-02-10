using Mixer.Base.Clients;
using Mixer.Base.Model.Interactive;
using Mixer.Base.Util;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Interactive;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Interactive;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.MixerAPI
{
    public static class InteractiveConnectedButtonControlModelExtensions
    {
        public static void SetCooldownTimestamp(this InteractiveConnectedButtonControlModel button, long cooldown)
        {
            if (ChannelSession.Settings.PreventSmallerCooldowns)
            {
                button.cooldown = Math.Max(button.cooldown, cooldown);
            }
            else
            {
                button.cooldown = cooldown;
            }
        }
    }

    public abstract class InteractiveConnectedControlCommand
    {
        public InteractiveConnectedSceneModel Scene { get; set; }

        public InteractiveControlModel Control { get; set; }

        public InteractiveCommand Command { get; set; }

        public InteractiveConnectedControlCommand(InteractiveConnectedSceneModel scene, InteractiveControlModel control, InteractiveCommand command)
        {
            this.Scene = scene;
            this.Control = control;
            this.Command = command;
        }

        public string Name { get { return this.Control.controlID; } }
        public string EventTypeString { get { return (this.Command != null) ? this.Command.EventTypeString : string.Empty; } }

        public abstract int SparkCost { get; }

        public abstract bool HasCooldown { get; }
        public abstract long CooldownTimestamp { get; }

        public virtual bool DoesInputMatchCommand(InteractiveGiveInputModel input)
        {
            return this.EventTypeString.Equals(input.input.eventType);
        }

        public virtual async Task<bool> CheckAllRequirements(UserViewModel user) { return await this.Command.CheckAllRequirements(user); }

        public virtual async Task Perform(UserViewModel user, IEnumerable<string> arguments = null)
        {
            Dictionary<string, string> extraSpecialIdentifiers = new Dictionary<string, string>()
            {
                { "mixplaycontrolid", this.Name },
                { "mixplaycontrolcost", this.SparkCost.ToString() },
            };

            await this.Command.Perform(user, arguments, extraSpecialIdentifiers);
        }
    }

    public class InteractiveConnectedButtonCommand : InteractiveConnectedControlCommand
    {
        public InteractiveConnectedButtonCommand(InteractiveConnectedSceneModel scene, InteractiveConnectedButtonControlModel button, InteractiveCommand command)
            : base(scene, button, command)
        {
            this.ButtonCommand.OnCommandStart += ButtonCommand_OnCommandStart;
        }

        public InteractiveConnectedButtonControlModel Button { get { return (InteractiveConnectedButtonControlModel)this.Control; } set { this.Control = value; } }

        public InteractiveButtonCommand ButtonCommand { get { return (InteractiveButtonCommand)this.Command; } }

        public override int SparkCost { get { return this.Button.cost.GetValueOrDefault(); } }

        public override bool HasCooldown { get { return this.ButtonCommand.HasCooldown; } }
        public override long CooldownTimestamp { get { return this.ButtonCommand.GetCooldownTimestamp(); } }

        public override bool DoesInputMatchCommand(InteractiveGiveInputModel input)
        {
            string inputEvent = input?.input?.eventType;
            if (!string.IsNullOrEmpty(inputEvent))
            {
                if (this.ButtonCommand.Trigger == InteractiveButtonCommandTriggerType.MouseKeyDown)
                {
                    return inputEvent.Equals("mousedown") || inputEvent.Equals("keydown");
                }
                else if (this.ButtonCommand.Trigger == InteractiveButtonCommandTriggerType.MouseKeyUp)
                {
                    return inputEvent.Equals("mouseup") || inputEvent.Equals("keyup");
                }
                else if (this.ButtonCommand.Trigger == InteractiveButtonCommandTriggerType.MouseKeyHeld)
                {
                    if (inputEvent.Equals("mousedown") || inputEvent.Equals("keydown"))
                    {
                        this.ButtonCommand.IsBeingHeld = true;
                        return true;
                    }
                    else if (inputEvent.Equals("mouseup") || inputEvent.Equals("keyup"))
                    {
                        this.ButtonCommand.IsBeingHeld = false;
                    }
                }
            }
            return false;
        }

        private async void ButtonCommand_OnCommandStart(object sender, EventArgs e)
        {
            if (ChannelSession.Interactive.IsConnected())
            {
                if (this.HasCooldown)
                {
                    this.Button.SetCooldownTimestamp(this.CooldownTimestamp);

                    Dictionary<InteractiveConnectedSceneModel, List<InteractiveConnectedButtonCommand>> sceneButtons = new Dictionary<InteractiveConnectedSceneModel, List<InteractiveConnectedButtonCommand>>();

                    if (!string.IsNullOrEmpty(this.ButtonCommand.CooldownGroupName))
                    {
                        var otherButtons = ChannelSession.Interactive.ControlCommands.Values.Where(c => c is InteractiveConnectedButtonCommand).Select(c => (InteractiveConnectedButtonCommand)c);
                        otherButtons = otherButtons.Where(c => this.ButtonCommand.CooldownGroupName.Equals(c.ButtonCommand.CooldownGroupName));
                        foreach (var otherItem in otherButtons)
                        {
                            otherItem.Button.SetCooldownTimestamp(this.Button.cooldown);
                            if (!sceneButtons.ContainsKey(otherItem.Scene))
                            {
                                sceneButtons[otherItem.Scene] = new List<InteractiveConnectedButtonCommand>();
                            }
                            sceneButtons[otherItem.Scene].Add(otherItem);
                        }
                    }
                    else
                    {
                        sceneButtons[this.Scene] = new List<InteractiveConnectedButtonCommand>();
                        sceneButtons[this.Scene].Add(this);
                    }

                    foreach (var kvp in sceneButtons)
                    {
                        await ChannelSession.Interactive.UpdateControls(kvp.Key, kvp.Value.Select(b => b.Button));
                    }
                }
            }
        }
    }

    public class InteractiveConnectedJoystickCommand : InteractiveConnectedControlCommand
    {
        public InteractiveConnectedJoystickCommand(InteractiveConnectedSceneModel scene, InteractiveConnectedJoystickControlModel joystick, InteractiveCommand command) : base(scene, joystick, command) { }

        public InteractiveConnectedJoystickControlModel Joystick { get { return (InteractiveConnectedJoystickControlModel)this.Control; } set { this.Control = value; } }

        public override int SparkCost { get { return 0; } }

        public override bool HasCooldown { get { return false; } }
        public override long CooldownTimestamp { get { return DateTimeHelper.DateTimeOffsetToUnixTimestamp(DateTimeOffset.Now); } }
    }

    public class InteractiveConnectedTextBoxCommand : InteractiveConnectedControlCommand
    {
        public InteractiveConnectedTextBoxCommand(InteractiveConnectedSceneModel scene, InteractiveConnectedTextBoxControlModel textBox, InteractiveCommand command) : base(scene, textBox, command) { }

        public InteractiveConnectedTextBoxControlModel TextBox { get { return (InteractiveConnectedTextBoxControlModel)this.Control; } set { this.Control = value; } }

        public InteractiveTextBoxCommand TextBoxCommand { get { return (InteractiveTextBoxCommand)this.Command; } }

        public override int SparkCost { get { return this.TextBox.cost.GetValueOrDefault(); } }

        public override bool HasCooldown { get { return false; } }
        public override long CooldownTimestamp { get { return DateTimeHelper.DateTimeOffsetToUnixTimestamp(DateTimeOffset.Now); } }
    }

    public class InteractiveInputEvent
    {
        public UserViewModel User { get; set; }
        public InteractiveGiveInputModel Input { get; set; }
        public InteractiveConnectedControlCommand Command { get; set; }

        public InteractiveInputEvent(UserViewModel user, InteractiveGiveInputModel input, InteractiveConnectedControlCommand command)
            : this(user, input)
        {
            this.Command = command;
        }

        public InteractiveInputEvent(UserViewModel user, InteractiveGiveInputModel input)
        {
            this.User = user;
            this.Input = input;
        }
    }

    public class InteractiveClientWrapper : MixerWebSocketWrapper
    {
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

        public event EventHandler<InteractiveInputEvent> OnInteractiveControlUsed = delegate { };

        public InteractiveSharedProjectModel SharedProject { get; private set; }
        public InteractiveGameModel Game { get; private set; }
        public InteractiveGameVersionModel Version { get; private set; }
        public InteractiveClient Client { get; private set; }

        public List<InteractiveConnectedSceneModel> Scenes { get; private set; }
        public Dictionary<string, InteractiveControlModel> Controls { get; private set; }
        public Dictionary<string, InteractiveConnectedControlCommand> ControlCommands { get; private set; }
        public LockedDictionary<string, InteractiveParticipantModel> Participants { get; private set; }

        private List<InteractiveGameModel> games = new List<InteractiveGameModel>();
        private DateTimeOffset lastRefresh = DateTimeOffset.MinValue;

        private SemaphoreSlim giveInputLock = new SemaphoreSlim(1);

        public InteractiveClientWrapper()
        {
            this.Scenes = new List<InteractiveConnectedSceneModel>();
            this.Controls = new Dictionary<string, InteractiveControlModel>();
            this.ControlCommands = new Dictionary<string, InteractiveConnectedControlCommand>();
            this.Participants = new LockedDictionary<string, InteractiveParticipantModel>();
        }

        public async Task<IEnumerable<InteractiveGameModel>> GetAllConnectableGames(bool forceRefresh = false)
        {
            if (forceRefresh || this.lastRefresh < DateTimeOffset.Now)
            {
                this.lastRefresh = DateTimeOffset.Now.AddMinutes(1);
                this.games.Clear();

                this.games.AddRange(await ChannelSession.Connection.GetOwnedInteractiveGames(ChannelSession.Channel));
                games.RemoveAll(g => g.name.Equals("Soundwave Interactive Soundboard"));

                foreach (InteractiveSharedProjectModel project in ChannelSession.Settings.CustomInteractiveProjectIDs)
                {
                    InteractiveGameVersionModel version = await ChannelSession.Connection.GetInteractiveGameVersion(project.VersionID);
                    if (version != null)
                    {
                        InteractiveGameModel game = await ChannelSession.Connection.GetInteractiveGame(version.gameId);
                        if (game != null)
                        {
                            games.Add(game);
                        }
                    }
                }

                foreach (InteractiveSharedProjectModel project in InteractiveSharedProjectModel.AllMixPlayProjects)
                {
                    InteractiveGameVersionModel version = await ChannelSession.Connection.GetInteractiveGameVersion(project.VersionID);
                    if (version != null)
                    {
                        InteractiveGameModel game = await ChannelSession.Connection.GetInteractiveGame(version.gameId);
                        if (game != null)
                        {
                            game.name += " (MixPlay)";
                            games.Add(game);
                        }
                    }
                }
            }

            return games;
        }

        public async Task<bool> Connect(InteractiveGameListingModel game)
        {
            return await this.Connect(game, game.versions.First());
        }

        public async Task<bool> Connect(InteractiveGameModel game)
        {
            IEnumerable<InteractiveGameVersionModel> versions = await ChannelSession.Connection.GetInteractiveGameVersions(game);
            return await this.Connect(game, versions.First());
        }

        public async Task<bool> Connect(InteractiveGameModel game, InteractiveGameVersionModel version)
        {
            this.Game = game;
            this.Version = version;

            this.Scenes.Clear();
            this.Controls.Clear();
            this.ControlCommands.Clear();
            this.Participants.Clear();

            return await this.AttemptConnect();
        }

        public async Task Disconnect()
        {
            await this.RunAsync(async () =>
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

                this.Scenes.Clear();
                this.Controls.Clear();
                this.ControlCommands.Clear();
                this.Participants.Clear();
            });
        }

        public bool IsConnected()
        {
            InteractiveClient client = this.Client;
            return client != null && client.Authenticated;
        }

        public async Task<InteractiveConnectedSceneGroupCollectionModel> GetScenes() { return await this.RunAsync(this.Client.GetScenes()); }

        public async Task<bool> AddGroup(string groupName, string sceneID)
        {
            if (!string.IsNullOrEmpty(groupName) && !string.IsNullOrEmpty(sceneID))
            {
                InteractiveGroupCollectionModel groups = await ChannelSession.Interactive.GetGroups();
                if (groups != null && groups.groups != null)
                {
                    if (!groups.groups.Any(g => g.groupID.Equals(groupName)))
                    {
                        return await this.RunAsync(this.Client.CreateGroupsWithResponse(new List<InteractiveGroupModel>() { new InteractiveGroupModel() { groupID = groupName, sceneID = sceneID } }));
                    }
                    return true;
                }
            }
            return false;
        }

        public async Task<InteractiveGroupCollectionModel> GetGroups() { return await this.RunAsync(this.Client.GetGroups()); }

        public async Task UpdateGroup(string groupName, string sceneID)
        {
            InteractiveGroupCollectionModel groups = await ChannelSession.Interactive.GetGroups();
            if (groups != null && groups.groups != null)
            {
                InteractiveGroupModel group = groups.groups.FirstOrDefault(g => g.groupID.Equals(groupName));
                if (group != null)
                {
                    group.sceneID = sceneID;
                    await this.RunAsync(this.Client.UpdateGroups(new List<InteractiveGroupModel>() { group }));
                }
            }
        }

        public async Task DeleteGroup(InteractiveGroupModel groupToDelete, InteractiveGroupModel groupToReplace) { await this.RunAsync(this.Client.DeleteGroup(groupToDelete, groupToReplace)); }

        public async Task<IEnumerable<InteractiveParticipantModel>> GetRecentParticipants()
        {
            Dictionary<uint, InteractiveParticipantModel> participants = new Dictionary<uint, InteractiveParticipantModel>();

            DateTimeOffset startTime = DateTimeOffset.Now.Subtract(TimeSpan.FromMinutes(10));
            InteractiveParticipantCollectionModel collection = null;
            await this.RunAsync(async () =>
            {
                do
                {
                    if (this.Client == null)
                    {
                        break;
                    }

                    collection = await this.Client.GetAllParticipants(startTime);
                    if (collection != null && collection.participants != null)
                    {
                        foreach (InteractiveParticipantModel participant in collection.participants)
                        {
                            participants[participant.userID] = participant;
                        }

                        if (collection.participants.Count() > 0)
                        {
                            startTime = DateTimeHelper.UnixTimestampToDateTimeOffset(collection.participants.Last().connectedAt);
                        }
                    }
                } while (collection != null && collection.participants.Count > 0 && collection.hasMore);
            });

            return participants.Values;
        }

        public async Task UpdateParticipant(InteractiveParticipantModel participant) { await this.UpdateParticipants(new List<InteractiveParticipantModel>() { participant }); }
        public async Task UpdateParticipants(IEnumerable<InteractiveParticipantModel> participants) { await this.RunAsync(this.Client.UpdateParticipantsWithResponse(participants)); }

        public async Task AddUserToGroup(UserViewModel user, string groupName)
        {
            if (!string.IsNullOrEmpty(groupName) && user.IsInteractiveParticipant)
            {
                user.InteractiveGroupID = groupName;
                foreach (InteractiveParticipantModel participant in user.GetParticipantModels())
                {
                    await ChannelSession.Interactive.UpdateParticipant(participant);
                }
            }
        }

        public async Task UpdateControls(InteractiveConnectedSceneModel scene, IEnumerable<InteractiveControlModel> controls)
        {
            List<InteractiveControlModel> updatedControls = new List<InteractiveControlModel>();

            foreach (InteractiveControlModel control in controls)
            {
                if (control is InteractiveConnectedButtonControlModel) { updatedControls.Add(SerializerHelper.Clone<InteractiveConnectedButtonControlModel>(control)); }
                else if (control is InteractiveConnectedJoystickControlModel) { updatedControls.Add(SerializerHelper.Clone<InteractiveConnectedJoystickControlModel>(control)); }
                else if (control is InteractiveConnectedTextBoxControlModel) { updatedControls.Add(SerializerHelper.Clone<InteractiveConnectedTextBoxControlModel>(control)); }
                else if (control is InteractiveConnectedLabelControlModel) { updatedControls.Add(SerializerHelper.Clone<InteractiveConnectedLabelControlModel>(control)); }
                else if (control is InteractiveButtonControlModel) { updatedControls.Add(SerializerHelper.Clone<InteractiveButtonControlModel>(control)); }
                else if (control is InteractiveJoystickControlModel) { updatedControls.Add(SerializerHelper.Clone<InteractiveJoystickControlModel>(control)); }
                else if (control is InteractiveTextBoxControlModel) { updatedControls.Add(SerializerHelper.Clone<InteractiveTextBoxControlModel>(control)); }
                else if (control is InteractiveLabelControlModel) { updatedControls.Add(SerializerHelper.Clone<InteractiveLabelControlModel>(control)); }
                else { updatedControls.Add(SerializerHelper.Clone<InteractiveControlModel>(control)); }
            }

            foreach (InteractiveControlModel control in updatedControls)
            {
                control.position = null;
            }

            await this.RunAsync(this.Client.UpdateControls(scene, updatedControls));
        }

        public async Task CaptureSparkTransaction(string transactionID) { await this.RunAsync(this.Client.CaptureSparkTransaction(transactionID)); }

        public async Task BroadcastEvent(IEnumerable<string> scopes, JObject data) { await this.RunAsync(this.Client.BroadcastEvent(scopes, data)); }

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

        public async Task TimeoutUser(UserViewModel user, int amountInSeconds)
        {
            await ChannelSession.Interactive.SetUserDisabledState(user, disabled: true);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(amountInSeconds * 1000);
                    if (ChannelSession.Interactive != null)
                    {
                        await ChannelSession.Interactive.SetUserDisabledState(user, disabled: false);
                    }
                }
                catch (Exception ex) { Util.Logger.Log(ex); }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public async Task SetUserDisabledState(UserViewModel user, bool disabled)
        {
            if (user != null && user.IsInteractiveParticipant)
            {
                user.IsInInteractiveTimeout = disabled;
                foreach (InteractiveParticipantModel participant in user.GetParticipantModels())
                {
                    await ChannelSession.Interactive.UpdateParticipant(participant);
                }
            }
        }

        public async Task RefreshCachedControls()
        {
            this.Scenes.Clear();
            this.Controls.Clear();
            this.ControlCommands.Clear();

            // Initialize Scenes
            InteractiveConnectedSceneGroupCollectionModel scenes = await this.GetScenes();
            if (scenes != null)
            {
                foreach (InteractiveConnectedSceneModel scene in scenes.scenes)
                {
                    this.Scenes.Add(scene);

                    InteractiveSceneModel dataScene = this.Version.controls.scenes.FirstOrDefault(s => s.sceneID.Equals(scene.sceneID));

                    foreach (InteractiveConnectedButtonControlModel button in scene.buttons)
                    {
                        if (dataScene != null)
                        {
                            InteractiveButtonControlModel dataButton = dataScene.buttons.FirstOrDefault(b => b.controlID.Equals(button.controlID));
                            if (dataButton != null)
                            {
                                button.text = dataButton.text;
                                button.tooltip = dataButton.tooltip;
                            }
                        }

                        this.Controls[button.controlID] = button;

                        this.AddConnectedControl(scene, button);
                    }

                    foreach (InteractiveConnectedJoystickControlModel joystick in scene.joysticks)
                    {
                        this.Controls[joystick.controlID] = joystick;

                        this.AddConnectedControl(scene, joystick);
                    }

                    foreach (InteractiveConnectedTextBoxControlModel textBox in scene.textBoxes)
                    {
                        if (dataScene != null)
                        {
                            InteractiveTextBoxControlModel dataTextBox = dataScene.textBoxes.FirstOrDefault(b => b.controlID.Equals(textBox.controlID));
                            if (dataTextBox != null)
                            {
                                textBox.placeholder = dataTextBox.placeholder;
                                textBox.submitText = dataTextBox.submitText;
                            }
                        }

                        this.Controls[textBox.controlID] = textBox;

                        this.AddConnectedControl(scene, textBox);
                    }
                }
            }
        }

        protected override async Task<bool> ConnectInternal()
        {
            this.SharedProject = ChannelSession.Settings.CustomInteractiveProjectIDs.FirstOrDefault(p => p.VersionID == this.Version.id);
            if (this.SharedProject == null)
            {
                this.SharedProject = InteractiveSharedProjectModel.AllMixPlayProjects.FirstOrDefault(p => p.GameID == this.Game.id && p.VersionID == this.Version.id);
            }

            if (this.SharedProject != null)
            {
                this.Client = await this.RunAsync(InteractiveClient.CreateFromChannel(ChannelSession.Connection.Connection, ChannelSession.Channel, this.Game, this.Version, this.SharedProject.ShareCode));
            }
            else
            {
                this.Client = await this.RunAsync(InteractiveClient.CreateFromChannel(ChannelSession.Connection.Connection, ChannelSession.Channel, this.Game, this.Version));
            }

            return await this.RunAsync(async () =>
            {
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

                        if (this.SharedProject != null && InteractiveSharedProjectModel.AllMixPlayProjects.Contains(this.SharedProject))
                        {
                            ChannelSession.Services.Telemetry.TrackInteractiveGame(this.Game);
                        }

                        return await this.Initialize();
                    }
                }
                return false;
            });
        }

        #region Interactive Update Methods

        private async Task<bool> Initialize()
        {
            this.Scenes.Clear();
            this.ControlCommands.Clear();

            this.Version = await ChannelSession.Connection.GetInteractiveGameVersion(this.Version);
            foreach (InteractiveSceneModel scene in this.Version.controls.scenes)
            {
                if (scene.allControls.Count() > 0)
                {
                    await this.UpdateControls(new InteractiveConnectedSceneModel() { sceneID = scene.sceneID }, scene.allControls);
                }
            }

            await this.RefreshCachedControls();
            if (this.Scenes.Count == 0)
            {
                return false;
            }

            // Initialize Groups
            List<InteractiveGroupModel> groupsToAdd = new List<InteractiveGroupModel>();
            if (ChannelSession.Settings.InteractiveUserGroups.ContainsKey(this.Client.InteractiveGame.id))
            {
                foreach (InteractiveUserGroupViewModel userGroup in ChannelSession.Settings.InteractiveUserGroups[this.Client.InteractiveGame.id])
                {
                    if (!userGroup.DefaultScene.Equals(InteractiveUserGroupViewModel.DefaultName))
                    {
                        if (!await this.AddGroup(userGroup.GroupName, userGroup.DefaultScene))
                        {
                            return false;
                        }
                    }
                }
            }

            // Initialize Participants
            await this.AddParticipants(await this.GetRecentParticipants());

            return true;
        }

        private void AddConnectedControl(InteractiveConnectedSceneModel scene, InteractiveControlModel control)
        {
            InteractiveCommand command = this.GetInteractiveCommandForControl(this.Client.InteractiveGame.id, control);
            if (command != null)
            {
                command.UpdateWithLatestControl(control);
                if (control is InteractiveConnectedButtonControlModel)
                {
                    this.ControlCommands[control.controlID] = new InteractiveConnectedButtonCommand(scene, (InteractiveConnectedButtonControlModel)control, command);
                }
                else if (control is InteractiveConnectedJoystickControlModel)
                {
                    this.ControlCommands[control.controlID] = new InteractiveConnectedJoystickCommand(scene, (InteractiveConnectedJoystickControlModel)control, command);
                }
                else if (control is InteractiveConnectedTextBoxControlModel)
                {
                    this.ControlCommands[control.controlID] = new InteractiveConnectedTextBoxCommand(scene, (InteractiveConnectedTextBoxControlModel)control, command);
                }
            }
        }

        private async Task AddParticipants(IEnumerable<InteractiveParticipantModel> participants)
        {
            if (participants != null && participants.Count() > 0)
            {
                List<InteractiveParticipantModel> participantsToUpdate = new List<InteractiveParticipantModel>();

                await ChannelSession.ActiveUsers.AddOrUpdateUsers(participants);

                List<InteractiveUserGroupViewModel> gameGroups = new List<InteractiveUserGroupViewModel>();
                if (this.Client != null && this.Client.InteractiveGame != null && ChannelSession.Settings.InteractiveUserGroups.ContainsKey(this.Client.InteractiveGame.id))
                {
                    gameGroups = new List<InteractiveUserGroupViewModel>(ChannelSession.Settings.InteractiveUserGroups[this.Client.InteractiveGame.id].OrderByDescending(g => g.AssociatedUserRole));
                }

                foreach (InteractiveParticipantModel participant in participants)
                {
                    if (participant != null && !string.IsNullOrEmpty(participant.sessionID))
                    {
                        this.Participants[participant.sessionID] = participant;

                        UserViewModel user = await ChannelSession.ActiveUsers.GetUserByID(participant.userID);
                        if (user != null)
                        {
                            InteractiveUserGroupViewModel group = gameGroups.FirstOrDefault(g => user.HasPermissionsTo(g.AssociatedUserRole));
                            if (group != null && !string.IsNullOrEmpty(group.DefaultScene))
                            {
                                bool updateParticipant = !group.DefaultScene.Equals(user.InteractiveGroupID);
                                user.InteractiveGroupID = group.GroupName;
                                if (updateParticipant)
                                {
                                    participantsToUpdate.AddRange(user.GetParticipantModels());
                                }
                            }
                        }
                    }
                }

                if (participantsToUpdate.Count > 0)
                {
                    await ChannelSession.Interactive.UpdateParticipants(participantsToUpdate);
                }
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

        private async void Client_OnParticipantLeave(object sender, InteractiveParticipantCollectionModel e)
        {
            if (e != null)
            {
                foreach (InteractiveParticipantModel participant in e.participants)
                {
                    if (!string.IsNullOrEmpty(participant.sessionID))
                    {
                        await ChannelSession.ActiveUsers.RemoveInteractiveUser(participant);
                        this.Participants.Remove(participant.sessionID);
                    }
                }
            }
            this.OnParticipantLeave(this, e);
        }

        private async void Client_OnParticipantJoin(object sender, InteractiveParticipantCollectionModel e)
        {
            if (e != null)
            {
                await this.AddParticipants(e.participants);
            }
            this.OnParticipantJoin(this, e);
        }

        private void Client_OnParticipantUpdate(object sender, InteractiveParticipantCollectionModel e)
        {
            if (e.participants != null)
            {
                foreach (InteractiveParticipantModel participant in e.participants)
                {
                    if (!string.IsNullOrEmpty(participant.sessionID))
                    {
                        this.Participants[participant.sessionID] = participant;
                    }
                }
            }
            this.OnParticipantUpdate(this, e);
        }

        private async void Client_OnGiveInput(object sender, InteractiveGiveInputModel e)
        {
            try
            {
                if (e != null && e.input != null)
                {
                    InteractiveControlModel control = this.Controls[e.input.controlID];
                    InteractiveConnectedControlCommand connectedControl = null;
                    if (this.ControlCommands.ContainsKey(e.input.controlID))
                    {
                        connectedControl = this.ControlCommands[e.input.controlID];

                        if (!connectedControl.DoesInputMatchCommand(e))
                        {
                            return;
                        }

                        if (!connectedControl.Command.IsEnabled)
                        {
                            return;
                        }
                    }

                    UserViewModel user = null;
                    if (!string.IsNullOrEmpty(e.participantID))
                    {
                        user = await ChannelSession.ActiveUsers.GetUserByParticipantID(e.participantID);
                        if (user == null)
                        {
                            InteractiveParticipantModel participant = null;
                            if (this.Participants.TryGetValue(e.participantID, out participant))
                            {
                                user = new UserViewModel(participant);
                            }
                            else
                            {
                                IEnumerable<InteractiveParticipantModel> recentParticipants = await this.GetRecentParticipants();
                                participant = recentParticipants.FirstOrDefault(p => p.sessionID.Equals(e.participantID));
                                if (participant != null)
                                {
                                    user = await ChannelSession.ActiveUsers.AddOrUpdateUser(participant);
                                }
                            }
                        }
                    }

                    if (user == null)
                    {
                        user = new UserViewModel(0, "Unknown User");
                        user.InteractiveIDs[e.participantID] = new InteractiveParticipantModel() { sessionID = e.participantID, anonymous = true };
                    }
                    else
                    {
                        await user.RefreshDetails();
                    }
                    user.UpdateLastActivity();

                    if (ChannelSession.Settings.PreventUnknownInteractiveUsers && user.IsAnonymous)
                    {
                        return;
                    }

                    if (user.IsInInteractiveTimeout)
                    {
                        return;
                    }

                    if (!ModerationHelper.MeetsChatInteractiveParticipationRequirement(user))
                    {
                        await ModerationHelper.SendChatInteractiveParticipationWhisper(user, isInteractive: true);
                        return;
                    }

                    if (!this.Controls.ContainsKey(e.input.controlID))
                    {
                        return;
                    }

                    if (connectedControl != null)
                    {
                        int sparkCost = 0;

                        await this.giveInputLock.WaitAndRelease(async () =>
                        {
                            if (await connectedControl.CheckAllRequirements(user))
                            {
                                if (!string.IsNullOrEmpty(e.transactionID) && !user.Data.IsSparkExempt)
                                {
                                    Util.Logger.LogDiagnostic("Sending Spark Transaction Capture - " + e.transactionID);

                                    await this.CaptureSparkTransaction(e.transactionID);

                                    if (control is InteractiveButtonControlModel)
                                    {
                                        sparkCost = ((InteractiveButtonControlModel)control).cost.GetValueOrDefault();
                                    }
                                    else if (control is InteractiveTextBoxControlModel)
                                    {
                                        sparkCost = ((InteractiveTextBoxControlModel)control).cost.GetValueOrDefault();
                                    }
                                }

                                List<string> arguments = new List<string>();

                                if (connectedControl is InteractiveConnectedJoystickCommand)
                                {
                                    arguments.Add(e.input.x.ToString());
                                    arguments.Add(e.input.y.ToString());
                                }
                                else if (connectedControl is InteractiveConnectedTextBoxCommand)
                                {
                                    arguments.Add(e.input.value);
                                }

                                await connectedControl.Perform(user, arguments);
                            }
                        });

                        if (sparkCost > 0)
                        {
                            GlobalEvents.SparkUseOccurred(new Tuple<UserViewModel, int>(user, sparkCost));
                        }
                    }

                    this.OnGiveInput(this, e);

                    this.OnInteractiveControlUsed(this, new InteractiveInputEvent(user, e, connectedControl));
                }
            }
            catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }
        }

        private async void InteractiveClient_OnDisconnectOccurred(object sender, WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred("Interactive");

            do
            {
                await Task.Delay(2500);
            }
            while (!await this.Connect(this.Game, this.Version));

            ChannelSession.ReconnectionOccurred("Interactive");
        }

        #endregion Interactive Event Handlers
    }
}
