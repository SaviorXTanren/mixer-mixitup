using Mixer.Base.Clients;
using Mixer.Base.Model.MixPlay;
using MixItUp.Base.Commands;
using MixItUp.Base.Model;
using MixItUp.Base.Model.MixPlay;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.MixerAPI
{
    public enum MixPlayConnectionResult
    {
        Success = 0,
        DuplicateControlIDs,
        Unknown,
    }

    public static class MixPlayConnectedButtonControlModelExtensions
    {
        public static void SetCooldownTimestamp(this MixPlayConnectedButtonControlModel button, long cooldown)
        {
            if (ChannelSession.Settings.PreventSmallerMixPlayCooldowns)
            {
                button.cooldown = Math.Max(button.cooldown, cooldown);
            }
            else
            {
                button.cooldown = cooldown;
            }
        }

        public static void SetProgress(this MixPlayConnectedButtonControlModel button, float value)
        {
            button.progress = value;
        }
    }

    public abstract class InteractiveConnectedControlCommand
    {
        public MixPlayConnectedSceneModel Scene { get; set; }

        public MixPlayControlModel Control { get; set; }

        public MixPlayCommand Command { get; set; }

        public InteractiveConnectedControlCommand(MixPlayConnectedSceneModel scene, MixPlayControlModel control, MixPlayCommand command)
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

        public virtual bool DoesInputMatchCommand(MixPlayGiveInputModel input)
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
            
            if (this.Control is MixPlayButtonControlModel)
            {
                extraSpecialIdentifiers.Add("mixplaycontroltext", ((MixPlayButtonControlModel)this.Control).text);
                extraSpecialIdentifiers.Add("mixplaycontrolprogress", ((MixPlayConnectedButtonControlModel)this.Control).progress.ToString());
            }
            else if (this.Control is MixPlayTextBoxControlModel)
            {
                extraSpecialIdentifiers.Add("mixplaycontroltext", ((MixPlayTextBoxControlModel)this.Control).placeholder);
            }
            else if (this.Control is MixPlayLabelControlModel)
            {
                extraSpecialIdentifiers.Add("mixplaycontroltext", ((MixPlayLabelControlModel)this.Control).text);
            }

            await this.Command.Perform(user, StreamingPlatformTypeEnum.Mixer, arguments, extraSpecialIdentifiers);
        }
    }

    public class InteractiveConnectedButtonCommand : InteractiveConnectedControlCommand
    {
        public InteractiveConnectedButtonCommand(MixPlayConnectedSceneModel scene, MixPlayConnectedButtonControlModel button, MixPlayCommand command)
            : base(scene, button, command)
        {
            this.ButtonCommand.OnCommandStart += ButtonCommand_OnCommandStart;
        }

        public MixPlayConnectedButtonControlModel Button { get { return (MixPlayConnectedButtonControlModel)this.Control; } set { this.Control = value; } }

        public MixPlayButtonCommand ButtonCommand { get { return (MixPlayButtonCommand)this.Command; } }

        public override int SparkCost { get { return this.Button.cost.GetValueOrDefault(); } }

        public override bool HasCooldown { get { return this.ButtonCommand.HasCooldown; } }
        public override long CooldownTimestamp { get { return this.ButtonCommand.GetCooldownTimestamp(); } }

        public override bool DoesInputMatchCommand(MixPlayGiveInputModel input)
        {
            string inputEvent = input?.input?.eventType;
            if (!string.IsNullOrEmpty(inputEvent))
            {
                if (this.ButtonCommand.Trigger == MixPlayButtonCommandTriggerType.MouseKeyDown)
                {
                    return inputEvent.Equals("mousedown") || inputEvent.Equals("keydown");
                }
                else if (this.ButtonCommand.Trigger == MixPlayButtonCommandTriggerType.MouseKeyUp)
                {
                    return inputEvent.Equals("mouseup") || inputEvent.Equals("keyup");
                }
                else if (this.ButtonCommand.Trigger == MixPlayButtonCommandTriggerType.MouseKeyHeld)
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

                    Dictionary<MixPlayConnectedSceneModel, List<InteractiveConnectedButtonCommand>> sceneButtons = new Dictionary<MixPlayConnectedSceneModel, List<InteractiveConnectedButtonCommand>>();

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
        public InteractiveConnectedJoystickCommand(MixPlayConnectedSceneModel scene, MixPlayConnectedJoystickControlModel joystick, MixPlayCommand command) : base(scene, joystick, command) { }

        public MixPlayConnectedJoystickControlModel Joystick { get { return (MixPlayConnectedJoystickControlModel)this.Control; } set { this.Control = value; } }

        public override int SparkCost { get { return 0; } }

        public override bool HasCooldown { get { return false; } }
        public override long CooldownTimestamp { get { return DateTimeOffset.Now.ToUnixTimeMilliseconds(); } }
    }

    public class InteractiveConnectedTextBoxCommand : InteractiveConnectedControlCommand
    {
        public InteractiveConnectedTextBoxCommand(MixPlayConnectedSceneModel scene, MixPlayConnectedTextBoxControlModel textBox, MixPlayCommand command) : base(scene, textBox, command) { }

        public MixPlayConnectedTextBoxControlModel TextBox { get { return (MixPlayConnectedTextBoxControlModel)this.Control; } set { this.Control = value; } }

        public MixPlayTextBoxCommand TextBoxCommand { get { return (MixPlayTextBoxCommand)this.Command; } }

        public override int SparkCost { get { return this.TextBox.cost.GetValueOrDefault(); } }

        public override bool HasCooldown { get { return false; } }
        public override long CooldownTimestamp { get { return DateTimeOffset.Now.ToUnixTimeMilliseconds(); } }
    }

    public class InteractiveInputEvent
    {
        public UserViewModel User { get; set; }
        public MixPlayGiveInputModel Input { get; set; }
        public InteractiveConnectedControlCommand Command { get; set; }

        public InteractiveInputEvent(UserViewModel user, MixPlayGiveInputModel input, InteractiveConnectedControlCommand command)
            : this(user, input)
        {
            this.Command = command;
        }

        public InteractiveInputEvent(UserViewModel user, MixPlayGiveInputModel input)
        {
            this.User = user;
            this.Input = input;
        }
    }

    public class MixPlayClientWrapper : MixerWebSocketWrapper
    {
        public event EventHandler<MixPlayGiveInputModel> OnGiveInput = delegate { };
        public event EventHandler<MixPlayConnectedSceneModel> OnControlDelete = delegate { };
        public event EventHandler<MixPlayConnectedSceneModel> OnControlCreate = delegate { };
        public event EventHandler<MixPlayConnectedSceneModel> OnControlUpdate = delegate { };
        public event EventHandler<MixPlayConnectedSceneCollectionModel> OnSceneUpdate = delegate { };
        public event EventHandler<Tuple<MixPlayConnectedSceneModel, MixPlayConnectedSceneModel>> OnSceneDelete = delegate { };
        public event EventHandler<MixPlayConnectedSceneCollectionModel> OnSceneCreate = delegate { };
        public event EventHandler<MixPlayGroupCollectionModel> OnGroupUpdate = delegate { };
        public event EventHandler<Tuple<MixPlayGroupModel, MixPlayGroupModel>> OnGroupDelete = delegate { };
        public event EventHandler<MixPlayGroupCollectionModel> OnGroupCreate = delegate { };
        public event EventHandler<MixPlayParticipantCollectionModel> OnParticipantUpdate = delegate { };
        public event EventHandler<MixPlayParticipantCollectionModel> OnParticipantJoin = delegate { };
        public event EventHandler<MixPlayParticipantCollectionModel> OnParticipantLeave = delegate { };
        public event EventHandler<MixPlayIssueMemoryWarningModel> OnIssueMemoryWarning = delegate { };

        public event EventHandler<InteractiveInputEvent> OnInteractiveControlUsed = delegate { };

        public MixPlaySharedProjectModel SharedProject { get; private set; }
        public MixPlayGameModel Game { get; private set; }
        public MixPlayGameVersionModel Version { get; private set; }
        public MixPlayClient Client { get; private set; }

        public List<string> DuplicatedControls { get; private set; } = new List<string>();
        public List<MixPlayConnectedSceneModel> Scenes { get; private set; } = new List<MixPlayConnectedSceneModel>();
        public Dictionary<string, MixPlayControlModel> Controls { get; private set; } = new Dictionary<string, MixPlayControlModel>(StringComparer.InvariantCultureIgnoreCase);
        public Dictionary<string, MixPlayConnectedSceneModel> ControlScenes { get; private set; } = new Dictionary<string, MixPlayConnectedSceneModel>(StringComparer.InvariantCultureIgnoreCase);
        public Dictionary<string, InteractiveConnectedControlCommand> ControlCommands { get; private set; } = new Dictionary<string, InteractiveConnectedControlCommand>(StringComparer.InvariantCultureIgnoreCase);
        public LockedDictionary<string, MixPlayParticipantModel> Participants { get; private set; } = new LockedDictionary<string, MixPlayParticipantModel>(StringComparer.InvariantCultureIgnoreCase);

        private List<MixPlayGameModel> games = new List<MixPlayGameModel>();
        private DateTimeOffset lastRefresh = DateTimeOffset.MinValue;
        private SemaphoreSlim refreshLock = new SemaphoreSlim(1);

        private SemaphoreSlim giveInputLock = new SemaphoreSlim(1);

        private MixPlayConnectionResult connectionResult = MixPlayConnectionResult.Unknown;

        public MixPlayClientWrapper()
        {
            this.Scenes = new List<MixPlayConnectedSceneModel>();
            this.Controls = new Dictionary<string, MixPlayControlModel>();
            this.ControlCommands = new Dictionary<string, InteractiveConnectedControlCommand>();
            this.Participants = new LockedDictionary<string, MixPlayParticipantModel>();
        }

        public async Task<IEnumerable<MixPlayGameModel>> GetAllConnectableGames(bool forceRefresh = false)
        {
            await this.refreshLock.WaitAndRelease(async () =>
            {
                if (forceRefresh || this.lastRefresh < DateTimeOffset.Now)
                {
                    this.lastRefresh = DateTimeOffset.Now.AddMinutes(1);
                    this.games.Clear();

                    this.games.AddRange(await ChannelSession.MixerUserConnection.GetOwnedMixPlayGames(ChannelSession.MixerChannel));
                    games.RemoveAll(g => g.name.Equals("Soundwave Interactive Soundboard"));

                    foreach (MixPlaySharedProjectModel project in ChannelSession.Settings.CustomMixPlayProjectIDs)
                    {
                        MixPlayGameVersionModel version = await ChannelSession.MixerUserConnection.GetMixPlayGameVersion(project.VersionID);
                        if (version != null)
                        {
                            MixPlayGameModel game = await ChannelSession.MixerUserConnection.GetMixPlayGame(version.gameId);
                            if (game != null)
                            {
                                games.Add(game);
                            }
                        }
                    }

                    foreach (MixPlaySharedProjectModel project in MixPlaySharedProjectModel.AllMixPlayProjects)
                    {
                        MixPlayGameVersionModel version = await ChannelSession.MixerUserConnection.GetMixPlayGameVersion(project.VersionID);
                        if (version != null)
                        {
                            MixPlayGameModel game = await ChannelSession.MixerUserConnection.GetMixPlayGame(version.gameId);
                            if (game != null)
                            {
                                game.name += " (MixPlay)";
                                games.Add(game);
                            }
                        }
                    }
                }
            });
            return games;
        }

        public async Task<MixPlayConnectionResult> Connect(MixPlayGameListingModel game)
        {
            return await this.Connect(game, game.versions.First());
        }

        public async Task<MixPlayConnectionResult> Connect(MixPlayGameModel game)
        {
            IEnumerable<MixPlayGameVersionModel> versions = await ChannelSession.MixerUserConnection.GetMixPlayGameVersions(game);
            return await this.Connect(game, versions.First());
        }

        public async Task<MixPlayConnectionResult> Connect(MixPlayGameModel game, MixPlayGameVersionModel version)
        {
            this.Game = game;
            this.Version = version;

            this.Scenes.Clear();
            this.Controls.Clear();
            this.ControlCommands.Clear();
            this.Participants.Clear();

            await this.AttemptConnect();
            return connectionResult;
        }

        public async Task Disconnect()
        {
            await this.RunAsync(async () =>
            {
                if (this.Client != null)
                {
                    this.Client.OnDisconnectOccurred -= MixPlayClient_OnDisconnectOccurred;
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
                this.connectionResult = MixPlayConnectionResult.Unknown;
            });
        }

        public bool IsConnected()
        {
            MixPlayClient client = this.Client;
            return client != null && client.Authenticated;
        }

        public async Task<MixPlayConnectedSceneGroupCollectionModel> GetScenes() { return await this.RunAsync(this.Client.GetScenes()); }

        public async Task<bool> AddGroup(string groupName, string sceneID)
        {
            if (!string.IsNullOrEmpty(groupName) && !string.IsNullOrEmpty(sceneID))
            {
                MixPlayGroupCollectionModel groups = await ChannelSession.Interactive.GetGroups();
                if (groups != null && groups.groups != null)
                {
                    if (!groups.groups.Any(g => g.groupID.Equals(groupName)))
                    {
                        return await this.RunAsync(this.Client.CreateGroupsWithResponse(new List<MixPlayGroupModel>() { new MixPlayGroupModel() { groupID = groupName, sceneID = sceneID } }));
                    }
                    return true;
                }
            }
            return false;
        }

        public async Task<MixPlayGroupCollectionModel> GetGroups() { return await this.RunAsync(this.Client.GetGroups()); }

        public async Task UpdateGroup(string groupName, string sceneID)
        {
            MixPlayGroupCollectionModel groups = await ChannelSession.Interactive.GetGroups();
            if (groups != null && groups.groups != null)
            {
                MixPlayGroupModel group = groups.groups.FirstOrDefault(g => g.groupID.Equals(groupName));
                if (group != null)
                {
                    group.sceneID = sceneID;
                    await this.RunAsync(this.Client.UpdateGroups(new List<MixPlayGroupModel>() { group }));
                }
            }
        }

        public async Task DeleteGroup(MixPlayGroupModel groupToDelete, MixPlayGroupModel groupToReplace) { await this.RunAsync(this.Client.DeleteGroup(groupToDelete, groupToReplace)); }

        public async Task<IEnumerable<MixPlayParticipantModel>> GetRecentParticipants()
        {
            Dictionary<uint, MixPlayParticipantModel> participants = new Dictionary<uint, MixPlayParticipantModel>();

            DateTimeOffset startTime = DateTimeOffset.Now.Subtract(TimeSpan.FromMinutes(10));
            MixPlayParticipantCollectionModel collection = null;
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
                        foreach (MixPlayParticipantModel participant in collection.participants)
                        {
                            participants[participant.userID] = participant;
                        }

                        if (collection.participants.Count() > 0)
                        {
                            startTime = StreamingClient.Base.Util.DateTimeOffsetExtensions.FromUTCUnixTimeMilliseconds(collection.participants.Last().connectedAt);
                        }
                    }
                } while (collection != null && collection.participants.Count > 0 && collection.hasMore);
            });

            return participants.Values;
        }

        public async Task UpdateParticipant(MixPlayParticipantModel participant) { await this.UpdateParticipants(new List<MixPlayParticipantModel>() { participant }); }
        public async Task UpdateParticipants(IEnumerable<MixPlayParticipantModel> participants) { await this.RunAsync(this.Client.UpdateParticipantsWithResponse(participants)); }

        public async Task AddUserToGroup(UserViewModel user, string groupName)
        {
            if (!string.IsNullOrEmpty(groupName) && user.IsInteractiveParticipant)
            {
                user.InteractiveGroupID = groupName;
                foreach (MixPlayParticipantModel participant in user.GetMixerMixPlayParticipantModels())
                {
                    await ChannelSession.Interactive.UpdateParticipant(participant);
                }
            }
        }

        public async Task UpdateControls(MixPlayConnectedSceneModel scene, IEnumerable<MixPlayControlModel> controls)
        {
            List<MixPlayControlModel> updatedControls = new List<MixPlayControlModel>();

            foreach (MixPlayControlModel control in controls)
            {
                if (control is MixPlayConnectedButtonControlModel) { updatedControls.Add(SerializerHelper.Clone<MixPlayConnectedButtonControlModel>(control)); }
                else if (control is MixPlayConnectedJoystickControlModel) { updatedControls.Add(SerializerHelper.Clone<MixPlayConnectedJoystickControlModel>(control)); }
                else if (control is MixPlayConnectedTextBoxControlModel) { updatedControls.Add(SerializerHelper.Clone<MixPlayConnectedTextBoxControlModel>(control)); }
                else if (control is MixPlayConnectedLabelControlModel) { updatedControls.Add(SerializerHelper.Clone<MixPlayConnectedLabelControlModel>(control)); }
                else if (control is MixPlayButtonControlModel) { updatedControls.Add(SerializerHelper.Clone<MixPlayButtonControlModel>(control)); }
                else if (control is MixPlayJoystickControlModel) { updatedControls.Add(SerializerHelper.Clone<MixPlayJoystickControlModel>(control)); }
                else if (control is MixPlayTextBoxControlModel) { updatedControls.Add(SerializerHelper.Clone<MixPlayTextBoxControlModel>(control)); }
                else if (control is MixPlayLabelControlModel) { updatedControls.Add(SerializerHelper.Clone<MixPlayLabelControlModel>(control)); }
                else { updatedControls.Add(SerializerHelper.Clone<MixPlayControlModel>(control)); }
            }

            foreach (MixPlayControlModel control in updatedControls)
            {
                control.position = null;
            }

            await this.RunAsync(this.Client.UpdateControls(scene, updatedControls));
        }

        public async Task CaptureSparkTransaction(string transactionID) { await this.RunAsync(this.Client.CaptureSparkTransaction(transactionID)); }

        public async Task BroadcastEvent(IEnumerable<string> scopes, JObject data) { await this.RunAsync(this.Client.BroadcastEvent(scopes, data)); }

        public async Task DisableAllControlsWithoutCommands(MixPlayGameVersionModel version)
        {
            // Disable all controls that do not have an associated Interactive Command or the Interactive Command is disabled
            foreach (MixPlaySceneModel scene in version.controls.scenes)
            {
                foreach (MixPlayControlModel control in scene.allControls)
                {
                    MixPlayCommand command = this.GetInteractiveCommandForControl(version.gameId, control);
                    control.disabled = (command == null || !command.IsEnabled);
                }
            }
            await ChannelSession.MixerUserConnection.UpdateMixPlayGameVersion(version);
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
                catch (Exception ex) { Logger.Log(ex); }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public async Task SetUserDisabledState(UserViewModel user, bool disabled)
        {
            if (user != null && user.IsInteractiveParticipant)
            {
                user.IsInInteractiveTimeout = disabled;
                foreach (MixPlayParticipantModel participant in user.GetMixerMixPlayParticipantModels())
                {
                    await ChannelSession.Interactive.UpdateParticipant(participant);
                }
            }
        }

        public async Task RefreshCachedControls()
        {
            this.DuplicatedControls.Clear();
            this.Scenes.Clear();
            this.Controls.Clear();
            this.ControlCommands.Clear();

            // Initialize Scenes
            MixPlayConnectedSceneGroupCollectionModel scenes = await this.GetScenes();
            if (scenes != null)
            {
                foreach (MixPlayConnectedSceneModel scene in scenes.scenes)
                {
                    this.Scenes.Add(scene);

                    MixPlaySceneModel dataScene = this.Version.controls.scenes.FirstOrDefault(s => s.sceneID.Equals(scene.sceneID));

                    foreach (MixPlayConnectedButtonControlModel button in scene.buttons)
                    {
                        if (dataScene != null)
                        {
                            MixPlayButtonControlModel dataButton = dataScene.buttons.FirstOrDefault(b => b.controlID.Equals(button.controlID));
                            if (dataButton != null)
                            {
                                button.text = dataButton.text;
                                button.tooltip = dataButton.tooltip;
                            }
                        }

                        CheckDuplicatedControls(scene.sceneID, button.controlID);
                        this.Controls[button.controlID] = button;
                        this.ControlScenes[button.controlID] = scene;
                        this.AddConnectedControl(scene, button);
                    }

                    foreach (MixPlayConnectedJoystickControlModel joystick in scene.joysticks)
                    {
                        CheckDuplicatedControls(scene.sceneID, joystick.controlID);
                        this.Controls[joystick.controlID] = joystick;
                        this.ControlScenes[joystick.controlID] = scene;
                        this.AddConnectedControl(scene, joystick);
                    }

                    foreach (MixPlayConnectedTextBoxControlModel textBox in scene.textBoxes)
                    {
                        if (dataScene != null)
                        {
                            MixPlayTextBoxControlModel dataTextBox = dataScene.textBoxes.FirstOrDefault(b => b.controlID.Equals(textBox.controlID));
                            if (dataTextBox != null)
                            {
                                textBox.placeholder = dataTextBox.placeholder;
                                textBox.submitText = dataTextBox.submitText;
                            }
                        }

                        CheckDuplicatedControls(scene.sceneID, textBox.controlID);
                        this.Controls[textBox.controlID] = textBox;
                        this.ControlScenes[textBox.controlID] = scene;
                        this.AddConnectedControl(scene, textBox);
                    }
                }
            }
        }

        private void CheckDuplicatedControls(string sceneID, string controlID)
        {
            if (this.Controls.ContainsKey(controlID))
            {
                this.DuplicatedControls.Add($"Scene: {this.ControlScenes[controlID].sceneID}  Control: {controlID}");
                this.DuplicatedControls.Add($"Scene: {sceneID}  Control: {controlID}");
                this.connectionResult = MixPlayConnectionResult.DuplicateControlIDs;
            }
        }

        protected override async Task<bool> ConnectInternal()
        {
            this.connectionResult = MixPlayConnectionResult.Unknown;
            this.ShouldRetry = true;
            this.SharedProject = ChannelSession.Settings.CustomMixPlayProjectIDs.FirstOrDefault(p => p.VersionID == this.Version.id);
            if (this.SharedProject == null)
            {
                this.SharedProject = MixPlaySharedProjectModel.AllMixPlayProjects.FirstOrDefault(p => p.GameID == this.Game.id && p.VersionID == this.Version.id);
            }

            if (this.SharedProject != null)
            {
                this.Client = await this.RunAsync(MixPlayClient.CreateFromChannel(ChannelSession.MixerUserConnection.Connection, ChannelSession.MixerChannel, this.Game, this.Version, this.SharedProject.ShareCode));
            }
            else
            {
                this.Client = await this.RunAsync(MixPlayClient.CreateFromChannel(ChannelSession.MixerUserConnection.Connection, ChannelSession.MixerChannel, this.Game, this.Version));
            }

            return await this.RunAsync(async () =>
            {
                if (this.Client != null)
                {
                    this.backgroundThreadCancellationTokenSource = new CancellationTokenSource();

                    if (await this.RunAsync(this.Client.Connect()) && await this.RunAsync(this.Client.Ready()))
                    {
                        this.Client.OnDisconnectOccurred += MixPlayClient_OnDisconnectOccurred;
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

                        if (this.SharedProject != null && MixPlaySharedProjectModel.AllMixPlayProjects.Contains(this.SharedProject))
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

            this.Version = await ChannelSession.MixerUserConnection.GetMixPlayGameVersion(this.Version);
            foreach (MixPlaySceneModel scene in this.Version.controls.scenes)
            {
                if (scene.allControls.Count() > 0)
                {
                    await this.UpdateControls(new MixPlayConnectedSceneModel() { sceneID = scene.sceneID }, scene.allControls);
                }
            }

            await this.RefreshCachedControls();
            if (this.Scenes.Count == 0)
            {
                this.ShouldRetry = false;
                return false;
            }

            if (this.DuplicatedControls.Count > 0)
            {
                this.ShouldRetry = false;
                return false;
            }

            // Initialize Groups
            List<MixPlayGroupModel> groupsToAdd = new List<MixPlayGroupModel>();
            if (ChannelSession.Settings.MixPlayUserGroups.ContainsKey(this.Client.Game.id))
            {
                foreach (MixPlayUserGroupModel userGroup in ChannelSession.Settings.MixPlayUserGroups[this.Client.Game.id])
                {
                    if (!userGroup.DefaultScene.Equals(MixPlayUserGroupModel.DefaultName))
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

            this.connectionResult = MixPlayConnectionResult.Success;
            return true;
        }

        private void AddConnectedControl(MixPlayConnectedSceneModel scene, MixPlayControlModel control)
        {
            MixPlayCommand command = this.GetInteractiveCommandForControl(this.Client.Game.id, control);
            if (command != null)
            {
                command.UpdateWithLatestControl(control);
                if (control is MixPlayConnectedButtonControlModel)
                {
                    this.ControlCommands[control.controlID] = new InteractiveConnectedButtonCommand(scene, (MixPlayConnectedButtonControlModel)control, command);
                }
                else if (control is MixPlayConnectedJoystickControlModel)
                {
                    this.ControlCommands[control.controlID] = new InteractiveConnectedJoystickCommand(scene, (MixPlayConnectedJoystickControlModel)control, command);
                }
                else if (control is MixPlayConnectedTextBoxControlModel)
                {
                    this.ControlCommands[control.controlID] = new InteractiveConnectedTextBoxCommand(scene, (MixPlayConnectedTextBoxControlModel)control, command);
                }
            }
        }

        private async Task AddParticipants(IEnumerable<MixPlayParticipantModel> participants)
        {
            if (participants != null && participants.Count() > 0)
            {
                List<MixPlayParticipantModel> participantsToUpdate = new List<MixPlayParticipantModel>();

                foreach (MixPlayParticipantModel participant in participants)
                {
                    await ChannelSession.Services.User.AddOrUpdateUser(participant);
                }

                List<MixPlayUserGroupModel> gameGroups = new List<MixPlayUserGroupModel>();
                if (this.Client != null && this.Client.Game != null && ChannelSession.Settings.MixPlayUserGroups.ContainsKey(this.Client.Game.id))
                {
                    gameGroups = new List<MixPlayUserGroupModel>(ChannelSession.Settings.MixPlayUserGroups[this.Client.Game.id].OrderByDescending(g => g.AssociatedUserRole));
                }

                foreach (MixPlayParticipantModel participant in participants)
                {
                    if (participant != null && !string.IsNullOrEmpty(participant.sessionID))
                    {
                        this.Participants[participant.sessionID] = participant;

                        UserViewModel user = ChannelSession.Services.User.GetUserByMixerID(participant.userID);
                        if (user != null)
                        {
                            MixPlayUserGroupModel group = gameGroups.FirstOrDefault(g => user.HasPermissionsTo(g.AssociatedUserRole));
                            if (group != null && !string.IsNullOrEmpty(group.DefaultScene))
                            {
                                bool updateParticipant = !group.DefaultScene.Equals(user.InteractiveGroupID);
                                user.InteractiveGroupID = group.GroupName;
                                if (updateParticipant)
                                {
                                    participantsToUpdate.AddRange(user.GetMixerMixPlayParticipantModels());
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

        private MixPlayCommand GetInteractiveCommandForControl(uint gameID, MixPlayControlModel control)
        {
            return ChannelSession.Settings.MixPlayCommands.FirstOrDefault(c => c.GameID.Equals(gameID) && c.Control.controlID.Equals(control.controlID));
        }

        #endregion Interactive Update Methods

        #region Interactive Event Handlers

        private void Client_OnIssueMemoryWarning(object sender, MixPlayIssueMemoryWarningModel e) { this.OnIssueMemoryWarning(this, e); }

        private void Client_OnGroupCreate(object sender, MixPlayGroupCollectionModel e) { this.OnGroupCreate(this, e); }

        private void Client_OnGroupDelete(object sender, Tuple<MixPlayGroupModel, MixPlayGroupModel> e) { this.OnGroupDelete(this, e); }

        private void Client_OnGroupUpdate(object sender, MixPlayGroupCollectionModel e) { this.OnGroupUpdate(this, e); }

        private void Client_OnSceneCreate(object sender, MixPlayConnectedSceneCollectionModel e) { this.OnSceneCreate(this, e); }

        private void Client_OnSceneDelete(object sender, Tuple<MixPlayConnectedSceneModel, MixPlayConnectedSceneModel> e) { this.OnSceneDelete(this, e); }

        private void Client_OnSceneUpdate(object sender, MixPlayConnectedSceneCollectionModel e) { this.OnSceneUpdate(this, e); }

        private void Client_OnControlCreate(object sender, MixPlayConnectedSceneModel e) { this.OnControlCreate(this, e); }

        private void Client_OnControlDelete(object sender, MixPlayConnectedSceneModel e) { this.OnControlDelete(this, e); }

        private void Client_OnControlUpdate(object sender, MixPlayConnectedSceneModel e) { this.OnControlUpdate(this, e); }

        private async void Client_OnParticipantLeave(object sender, MixPlayParticipantCollectionModel e)
        {
            if (e != null)
            {
                foreach (MixPlayParticipantModel participant in e.participants)
                {
                    if (!string.IsNullOrEmpty(participant.sessionID))
                    {
                        await ChannelSession.Services.User.RemoveUser(participant);
                        this.Participants.Remove(participant.sessionID);
                    }
                }
            }
            this.OnParticipantLeave(this, e);
        }

        private async void Client_OnParticipantJoin(object sender, MixPlayParticipantCollectionModel e)
        {
            if (e != null)
            {
                await this.AddParticipants(e.participants);
            }
            this.OnParticipantJoin(this, e);
        }

        private void Client_OnParticipantUpdate(object sender, MixPlayParticipantCollectionModel e)
        {
            if (e.participants != null)
            {
                foreach (MixPlayParticipantModel participant in e.participants)
                {
                    if (!string.IsNullOrEmpty(participant.sessionID))
                    {
                        this.Participants[participant.sessionID] = participant;
                    }
                }
            }
            this.OnParticipantUpdate(this, e);
        }

        private async void Client_OnGiveInput(object sender, MixPlayGiveInputModel e)
        {
            try
            {
                if (e != null && e.input != null && this.Controls.ContainsKey(e.input.controlID))
                {
                    MixPlayControlModel control = this.Controls[e.input.controlID];
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
                        user = ChannelSession.Services.User.GetUserByMixPlayID(e.participantID);
                        if (user == null)
                        {
                            MixPlayParticipantModel participant = null;
                            if (this.Participants.TryGetValue(e.participantID, out participant))
                            {
                                user = await ChannelSession.Services.User.AddOrUpdateUser(participant);
                            }
                            else
                            {
                                IEnumerable<MixPlayParticipantModel> recentParticipants = await this.GetRecentParticipants();
                                participant = recentParticipants.FirstOrDefault(p => p.sessionID.Equals(e.participantID));
                                if (participant != null)
                                {
                                    user = await ChannelSession.Services.User.AddOrUpdateUser(participant);
                                }
                            }
                        }
                    }

                    if (user == null)
                    {
                        user = new UserViewModel("Unknown User");
                        user.InteractiveIDs[e.participantID] = new MixPlayParticipantModel() { sessionID = e.participantID, anonymous = true };
                    }
                    else
                    {
                        await user.RefreshDetails();
                    }
                    user.UpdateLastActivity();

                    if (ChannelSession.Settings.PreventUnknownMixPlayUsers && user.IsAnonymous)
                    {
                        return;
                    }

                    if (user.IsInInteractiveTimeout)
                    {
                        await ChannelSession.Services.Chat.Whisper(user.Username, "You currently timed out from MixPlay.");
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
                        uint sparkCost = 0;

                        await this.giveInputLock.WaitAndRelease(async () =>
                        {
                            if (await connectedControl.CheckAllRequirements(user))
                            {
                                if (!string.IsNullOrEmpty(e.transactionID) && !user.Data.IsSparkExempt)
                                {
                                    Logger.Log(LogLevel.Debug, "Sending Spark Transaction Capture - " + e.transactionID);

                                    await this.CaptureSparkTransaction(e.transactionID);

                                    if (control is MixPlayButtonControlModel)
                                    {
                                        sparkCost = (uint)((MixPlayButtonControlModel)control).cost.GetValueOrDefault();
                                    }
                                    else if (control is MixPlayTextBoxControlModel)
                                    {
                                        sparkCost = (uint)((MixPlayTextBoxControlModel)control).cost.GetValueOrDefault();
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
                            GlobalEvents.SparkUseOccurred(new Tuple<UserViewModel, uint>(user, sparkCost));
                        }
                    }

                    this.OnGiveInput(this, e);

                    this.OnInteractiveControlUsed(this, new InteractiveInputEvent(user, e, connectedControl));

                    if (ChannelSession.Settings.ChatShowMixPlayAlerts && user != null && !user.IsAnonymous)
                    {
                        await ChannelSession.Services.Chat.AddMessage(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Mixer, user,
                            string.Format("{0} Used The \"{1}\" Interactive Control", user.Username, connectedControl.Command.Name), ChannelSession.Settings.ChatMixPlayAlertsColorScheme));
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        private async void MixPlayClient_OnDisconnectOccurred(object sender, WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred("MixPlay");

            do
            {
                await Task.Delay(2500);
            }
            while (await this.Connect(this.Game, this.Version) != MixPlayConnectionResult.Success);

            ChannelSession.ReconnectionOccurred("MixPlay");
        }

        #endregion Interactive Event Handlers
    }
}
