using Mixer.Base.Clients;
using Mixer.Base.Model.MixPlay;
using MixItUp.Base.Commands;
using MixItUp.Base.Model;
using MixItUp.Base.Model.MixPlay;
using MixItUp.Base.Services.External;
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

namespace MixItUp.Base.Services.Mixer
{
    public class MixPlayInputEvent
    {
        public UserViewModel User { get; set; }
        public MixPlayGiveInputModel Input { get; set; }
        public MixPlayControlModel Control { get; set; }

        public MixPlayInputEvent(UserViewModel user, MixPlayGiveInputModel input, MixPlayControlModel control)
        {
            this.User = user;
            this.Input = input;
            this.Control = control;
        }
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

    public interface IMixerMixPlayService
    {
        event EventHandler<MixPlayInputEvent> OnControlUsed;

        bool IsConnected { get; }

        MixPlayGameModel SelectedGame { get; }
        MixPlayGameVersionModel SelectedVersion { get; }
        MixPlaySharedProjectModel SharedProject { get; }

        Dictionary<string, MixPlayConnectedSceneModel> Scenes { get; }
        Dictionary<string, MixPlayControlModel> Controls { get; }
        LockedDictionary<string, MixPlayParticipantModel> Participants { get; }

        Task<IEnumerable<MixPlayGameModel>> GetAllGames();
        Task SetGame(MixPlayGameModel game);

        Task<Result> Connect();
        Task Disconnect();

        Task<MixPlayConnectedSceneGroupCollectionModel> GetScenes();

        Task<bool> AddGroup(string groupName, string sceneID);
        Task<MixPlayGroupCollectionModel> GetGroups();
        Task UpdateGroup(string groupName, string sceneID);
        Task DeleteGroup(MixPlayGroupModel groupToDelete, MixPlayGroupModel groupToReplace);

        Task UpdateControls(MixPlayConnectedSceneModel scene, IEnumerable<MixPlayControlModel> controls);

        Task<IEnumerable<MixPlayParticipantModel>> GetRecentParticipants();
        Task UpdateParticipant(MixPlayParticipantModel participant);
        Task UpdateParticipants(IEnumerable<MixPlayParticipantModel> participants);
        Task AddUserToGroup(UserViewModel user, string groupName);

        Task CaptureSparkTransaction(string transactionID);
        Task BroadcastEvent(IEnumerable<string> scopes, JObject data);

        Task TimeoutUser(UserViewModel user, int amountInSeconds);
        Task SetUserDisabledState(UserViewModel user, bool disabled);

        Task CooldownButton(string controlID, long cooldownTimestamp);
        Task CooldownGroup(string groupName, long cooldownTimestamp);
        Task CooldownScene(string sceneID, long cooldownTimestamp);

        MixPlayCommand GetInteractiveCommandForControl(uint gameID, string controlID);
    }

    public class MixerMixPlayService : MixerPlatformServiceBase, IMixerMixPlayService
    {
        public event EventHandler<MixPlayInputEvent> OnControlUsed = delegate { };

        public MixPlayGameModel SelectedGame { get; private set; }
        public MixPlayGameVersionModel SelectedVersion { get; private set; }
        public MixPlaySharedProjectModel SharedProject { get; private set; }

        public Dictionary<string, MixPlayConnectedSceneModel> Scenes { get; private set; } = new Dictionary<string, MixPlayConnectedSceneModel>();
        public Dictionary<string, MixPlayControlModel> Controls { get; private set; } = new Dictionary<string, MixPlayControlModel>(StringComparer.InvariantCultureIgnoreCase);
        public LockedDictionary<string, MixPlayParticipantModel> Participants { get; private set; } = new LockedDictionary<string, MixPlayParticipantModel>(StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<string, string> controlToScene = new Dictionary<string, string>();

        public MixPlayClient Client { get; private set; }

        private SemaphoreSlim controlCooldownSemaphore = new SemaphoreSlim(1);
        private CancellationTokenSource backgroundThreadCancellationTokenSource;

        public MixerMixPlayService() { }

        public bool IsConnected { get { return this.Client != null && this.Client.Connected && this.Client.Authenticated; } }

        public async Task<IEnumerable<MixPlayGameModel>> GetAllGames()
        {
            List<MixPlayGameModel> results = new List<MixPlayGameModel>();

            Task<IEnumerable<MixPlayGameListingModel>> ownedGames = ChannelSession.MixerUserConnection.GetOwnedMixPlayGames(ChannelSession.MixerChannel);

            List<Task<MixPlayGameModel>> sharedGameTasks = new List<Task<MixPlayGameModel>>();
            foreach (MixPlaySharedProjectModel project in ChannelSession.Settings.CustomMixPlayProjectIDs)
            {
                sharedGameTasks.Add(this.GetGameByVersionID(project.VersionID));
            }

            foreach (MixPlaySharedProjectModel project in MixPlaySharedProjectModel.AllMixPlayProjects)
            {
                sharedGameTasks.Add(this.GetGameByVersionID(project.VersionID));
            }

            List<Task> tasks = new List<Task>() { ownedGames };
            tasks.AddRange(sharedGameTasks);
            await Task.WhenAll(tasks);

            foreach (MixPlayGameModel game in ownedGames.Result)
            {
                if (game != null)
                {
                    results.Add(game);
                }
            }

            foreach (MixPlayGameModel game in sharedGameTasks.Select(t => t.Result))
            {
                if (game != null)
                {
                    results.Add(game);
                }
            }

            return results.OrderBy(g => g.name);
        }

        public async Task SetGame(MixPlayGameModel game)
        {
            this.SelectedGame = game;

            IEnumerable<MixPlayGameVersionModel> versions = await ChannelSession.MixerUserConnection.GetMixPlayGameVersions(game);
            this.SelectedVersion = versions.FirstOrDefault();

            this.SharedProject = ChannelSession.Settings.CustomMixPlayProjectIDs.FirstOrDefault(p => p.VersionID == this.SelectedVersion.id);
            if (this.SharedProject == null)
            {
                this.SharedProject = MixPlaySharedProjectModel.AllMixPlayProjects.FirstOrDefault(p => p.GameID == this.SelectedGame.id && p.VersionID == this.SelectedVersion.id);
            }
        }

        public async Task<Result> Connect()
        {
            if (ChannelSession.MixerUserConnection != null)
            {
                Result result = await this.RunAsync(async () =>
                {
                    await this.Disconnect();

                    if (this.SharedProject != null)
                    {
                        this.Client = await this.RunAsync(MixPlayClient.CreateFromChannel(ChannelSession.MixerUserConnection.Connection, ChannelSession.MixerChannel, this.SelectedGame, this.SelectedVersion, this.SharedProject.ShareCode));
                    }
                    else
                    {
                        this.Client = await this.RunAsync(MixPlayClient.CreateFromChannel(ChannelSession.MixerUserConnection.Connection, ChannelSession.MixerChannel, this.SelectedGame, this.SelectedVersion));
                    }

                    this.backgroundThreadCancellationTokenSource = new CancellationTokenSource();

                    if (this.Client != null && await this.RunAsync(this.Client.Connect()))
                    {
                        if (await this.RunAsync(this.Client.Ready()))
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
                            this.Client.OnParticipantUpdate += Client_OnParticipantUpdate;
                            this.Client.OnParticipantJoin += Client_OnParticipantJoin;
                            this.Client.OnParticipantLeave += Client_OnParticipantLeave;
                            this.Client.OnSceneUpdate += Client_OnSceneUpdate;
                            this.Client.OnControlUpdate += Client_OnControlUpdate;

                            // Disable all controls that do not have an associated Interactive Command or the Interactive Command is disabled
                            foreach (MixPlaySceneModel scene in this.SelectedVersion.controls.scenes)
                            {
                                foreach (MixPlayControlModel control in scene.allControls)
                                {
                                    MixPlayCommand command = this.GetInteractiveCommandForControl(this.SelectedVersion.gameId, control.controlID);
                                    control.disabled = (command == null || !command.IsEnabled);
                                }
                            }
                            await ChannelSession.MixerUserConnection.UpdateMixPlayGameVersion(this.SelectedVersion);

                            // Initialize Scenes
                            MixPlayConnectedSceneGroupCollectionModel scenes = await this.GetScenes();
                            if (scenes != null)
                            {
                                List<MixPlayControlModel> duplicatedControls = new List<MixPlayControlModel>();
                                foreach (MixPlayConnectedSceneModel scene in scenes.scenes)
                                {
                                    this.Scenes[scene.sceneID] = scene;

                                    List<MixPlayControlModel> controls = new List<MixPlayControlModel>();
                                    controls.AddRange(scene.buttons);
                                    controls.AddRange(scene.joysticks);
                                    controls.AddRange(scene.textBoxes);

                                    foreach (MixPlayControlModel control in controls)
                                    {
                                        if (!this.Controls.ContainsKey(control.controlID))
                                        {
                                            this.Controls[control.controlID] = control;
                                            this.controlToScene[control.controlID] = scene.sceneID;
                                        }
                                        else
                                        {
                                            duplicatedControls.Add(control);
                                        }
                                    }
                                }

                                if (duplicatedControls.Count > 0)
                                {
                                    return new Result("The following controls exist on multiple scenes, please visit the interactive lab and correct this problem:" + Environment.NewLine +
                                        Environment.NewLine + string.Join(", ", duplicatedControls.Select(c => c.controlID)));
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
                                                return new Result("Failed to add MixPlay groups");
                                            }
                                        }
                                    }
                                }

                                // Initialize Participants
                                await this.AddParticipants(await this.GetRecentParticipants());

                                return new Result();
                            }
                            return new Result("Failed to MixPlay scene data");
                        }
                        else
                        {
                            return new Result("Failed to authenticate and ready to Mixer MixPlay");
                        }
                    }
                    return new Result("Failed to connect to Mixer MixPlay");
                });

                if (!result.Success)
                {
                    await this.Disconnect();
                }
                return result;
            }
            return new Result("Mixer connection has not been established");
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
                    this.Client.OnParticipantUpdate -= Client_OnParticipantUpdate;
                    this.Client.OnParticipantJoin -= Client_OnParticipantJoin;
                    this.Client.OnParticipantLeave -= Client_OnParticipantLeave;
                    this.Client.OnSceneUpdate -= Client_OnSceneUpdate;
                    this.Client.OnControlUpdate -= Client_OnControlUpdate;

                    await this.RunAsync(this.Client.Disconnect());

                    this.backgroundThreadCancellationTokenSource.Cancel();
                }
                this.Client = null;
                this.backgroundThreadCancellationTokenSource = null;

                this.Scenes.Clear();
                this.Controls.Clear();
                this.Participants.Clear();
                this.controlToScene.Clear();

                foreach (MixPlayCommand command in ChannelSession.Settings.MixPlayCommands)
                {
                    command.ResetCooldown();
                }
            });
        }

        public async Task<MixPlayConnectedSceneGroupCollectionModel> GetScenes() { return await this.RunAsync(this.Client.GetScenes()); }

        public async Task<bool> AddGroup(string groupName, string sceneID)
        {
            if (!string.IsNullOrEmpty(groupName) && !string.IsNullOrEmpty(sceneID))
            {
                MixPlayGroupCollectionModel groups = await this.GetGroups();
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
            MixPlayGroupCollectionModel groups = await this.GetGroups();
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

        public async Task UpdateControls(MixPlayConnectedSceneModel scene, IEnumerable<MixPlayControlModel> controls)
        {
            List<MixPlayControlModel> updatedControls = new List<MixPlayControlModel>();

            foreach (MixPlayControlModel control in controls)
            {
                if (control is MixPlayConnectedButtonControlModel) { updatedControls.Add(JSONSerializerHelper.Clone<MixPlayConnectedButtonControlModel>(control)); }
                else if (control is MixPlayConnectedJoystickControlModel) { updatedControls.Add(JSONSerializerHelper.Clone<MixPlayConnectedJoystickControlModel>(control)); }
                else if (control is MixPlayConnectedTextBoxControlModel) { updatedControls.Add(JSONSerializerHelper.Clone<MixPlayConnectedTextBoxControlModel>(control)); }
                else if (control is MixPlayConnectedLabelControlModel) { updatedControls.Add(JSONSerializerHelper.Clone<MixPlayConnectedLabelControlModel>(control)); }
                else if (control is MixPlayButtonControlModel) { updatedControls.Add(JSONSerializerHelper.Clone<MixPlayButtonControlModel>(control)); }
                else if (control is MixPlayJoystickControlModel) { updatedControls.Add(JSONSerializerHelper.Clone<MixPlayJoystickControlModel>(control)); }
                else if (control is MixPlayTextBoxControlModel) { updatedControls.Add(JSONSerializerHelper.Clone<MixPlayTextBoxControlModel>(control)); }
                else if (control is MixPlayLabelControlModel) { updatedControls.Add(JSONSerializerHelper.Clone<MixPlayLabelControlModel>(control)); }
                else { updatedControls.Add(JSONSerializerHelper.Clone<MixPlayControlModel>(control)); }
            }

            foreach (MixPlayControlModel control in updatedControls)
            {
                control.position = null;
            }

            await this.RunAsync(this.Client.UpdateControls(scene, updatedControls));
        }

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
            if (!string.IsNullOrEmpty(groupName) && user.IsMixerMixPlayParticipant)
            {
                user.InteractiveGroupID = groupName;
                foreach (MixPlayParticipantModel participant in user.GetMixerMixPlayParticipantModels())
                {
                    await this.UpdateParticipant(participant);
                }
            }
        }

        public async Task CaptureSparkTransaction(string transactionID) { await this.RunAsync(this.Client.CaptureSparkTransaction(transactionID)); }

        public async Task BroadcastEvent(IEnumerable<string> scopes, JObject data) { await this.RunAsync(this.Client.BroadcastEvent(scopes, data)); }

        public async Task TimeoutUser(UserViewModel user, int amountInSeconds)
        {
            await this.SetUserDisabledState(user, disabled: true);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(amountInSeconds * 1000);
                    if (this.IsConnected)
                    {
                        await this.SetUserDisabledState(user, disabled: false);
                    }
                }
                catch (Exception ex) { Logger.Log(ex); }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public async Task SetUserDisabledState(UserViewModel user, bool disabled)
        {
            if (user != null && user.IsMixerMixPlayParticipant)
            {
                user.IsInInteractiveTimeout = disabled;
                foreach (MixPlayParticipantModel participant in user.GetMixerMixPlayParticipantModels())
                {
                    await this.UpdateParticipant(participant);
                }
            }
        }

        public async Task CooldownButton(string controlID, long cooldownTimestamp)
        {
            if (this.Controls.ContainsKey(controlID) && this.Controls[controlID] is MixPlayConnectedButtonControlModel)
            {
                MixPlayConnectedButtonControlModel button = (MixPlayConnectedButtonControlModel)this.Controls[controlID];
                button.SetCooldownTimestamp(cooldownTimestamp);
                MixPlayConnectedSceneModel scene = this.Scenes[this.controlToScene[button.controlID]];
                await this.UpdateControls(scene, new List<MixPlayConnectedButtonControlModel>() { button });
            }
        }

        public async Task CooldownGroup(string groupName, long cooldownTimestamp)
        {
            Dictionary<MixPlayConnectedSceneModel, List<MixPlayConnectedButtonControlModel>> sceneButtons = new Dictionary<MixPlayConnectedSceneModel, List<MixPlayConnectedButtonControlModel>>();

            IEnumerable<MixPlayConnectedButtonControlModel> buttons = this.Controls.Values.Where(c => c is MixPlayConnectedButtonControlModel).Select(c => (MixPlayConnectedButtonControlModel)c);
            foreach (MixPlayConnectedButtonControlModel button in buttons)
            {
                MixPlayCommand command = this.GetInteractiveCommandForControl(this.SelectedGame.id, button.controlID);
                if (command != null && string.Equals(groupName, command.CooldownGroupName))
                {
                    button.SetCooldownTimestamp(cooldownTimestamp);
                    MixPlayConnectedSceneModel scene = this.Scenes[this.controlToScene[button.controlID]];

                    if (!sceneButtons.ContainsKey(scene))
                    {
                        sceneButtons[scene] = new List<MixPlayConnectedButtonControlModel>();
                    }
                    sceneButtons[scene].Add(button);
                }
            }

            foreach (var kvp in sceneButtons)
            {
                await this.UpdateControls(kvp.Key, kvp.Value);
            }
        }

        public async Task CooldownScene(string sceneID, long cooldownTimestamp)
        {
            if (this.Scenes.ContainsKey(sceneID))
            {
                MixPlayConnectedSceneModel scene = this.Scenes[sceneID];
                List<MixPlayConnectedButtonControlModel> controls = new List<MixPlayConnectedButtonControlModel>();
                foreach (var kvp in controlToScene)
                {
                    if (kvp.Value.Equals(scene) && this.Controls[kvp.Key] is MixPlayConnectedButtonControlModel)
                    {
                        controls.Add((MixPlayConnectedButtonControlModel)this.Controls[kvp.Key]);
                    }
                }

                foreach (MixPlayConnectedButtonControlModel control in controls)
                {
                    control.SetCooldownTimestamp(cooldownTimestamp);
                }

                await this.UpdateControls(scene, controls);
            }
        }

        public MixPlayCommand GetInteractiveCommandForControl(uint gameID, string controlID)
        {
            return ChannelSession.Settings.MixPlayCommands.FirstOrDefault(c => c.GameID.Equals(gameID) && c.Name.Equals(controlID));
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
                    await this.UpdateParticipants(participantsToUpdate);
                }
            }
        }

        private async Task<MixPlayGameModel> GetGameByVersionID(uint versionID)
        {
            MixPlayGameVersionModel version = await ChannelSession.MixerUserConnection.GetMixPlayGameVersion(versionID);
            if (version != null)
            {
                MixPlayGameModel game = await ChannelSession.MixerUserConnection.GetMixPlayGame(version.gameId);
                if (game != null)
                {
                    return game;
                }
            }
            return null;
        }

        #region MixPlay Event Handlers

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
        }

        private async void Client_OnParticipantJoin(object sender, MixPlayParticipantCollectionModel e)
        {
            if (e != null)
            {
                await this.AddParticipants(e.participants);
            }
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
        }

        private void Client_OnSceneUpdate(object sender, MixPlayConnectedSceneCollectionModel e)
        {
            foreach (MixPlayConnectedSceneModel scene in e.scenes)
            {
                if (this.Scenes.ContainsKey(scene.sceneID))
                {
                    this.Scenes[scene.sceneID] = scene;
                }
            }
        }

        private void Client_OnControlUpdate(object sender, MixPlayConnectedSceneModel e)
        {
            List<MixPlayControlModel> controls = new List<MixPlayControlModel>();
            controls.AddRange(e.buttons);
            controls.AddRange(e.joysticks);
            controls.AddRange(e.textBoxes);

            foreach (MixPlayControlModel control in controls)
            {
                if (this.Controls.ContainsKey(control.controlID))
                {
                    this.Controls[control.controlID] = control;
                }
            }
        }

        private async void Client_OnGiveInput(object sender, MixPlayGiveInputModel e)
        {
            try
            {
                if (e != null && e.input != null && this.Controls.ContainsKey(e.input.controlID))
                {
                    MixPlayControlModel control = this.Controls[e.input.controlID];
                    MixPlayCommand command = this.GetInteractiveCommandForControl(this.SelectedGame.id, control.controlID);
                    if (command == null || !command.IsEnabled || !command.DoesInputMatchCommand(e))
                    {
                        return;
                    }

                    UserViewModel user = null;
                    if (!string.IsNullOrEmpty(e.participantID))
                    {
                        user = ChannelSession.Services.User.GetUserByMixPlayID(e.participantID);
                        if (user == null)
                        {
                            MixPlayParticipantModel participant = null;
                            if (!this.Participants.TryGetValue(e.participantID, out participant))
                            {
                                IEnumerable<MixPlayParticipantModel> recentParticipants = await this.GetRecentParticipants();
                                participant = recentParticipants.FirstOrDefault(p => p.sessionID.Equals(e.participantID));
                            }

                            if (participant != null && !participant.anonymous.GetValueOrDefault())
                            {
                                user = await ChannelSession.Services.User.AddOrUpdateUser(participant);
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
                        await ChannelSession.Services.Chat.Whisper(user, "You are currently timed out from MixPlay.");
                        return;
                    }

                    if (!ChannelSession.Services.Moderation.MeetsChatInteractiveParticipationRequirement(user))
                    {
                        await ChannelSession.Services.Moderation.SendChatInteractiveParticipationWhisper(user, isInteractive: true);
                        return;
                    }

                    List<string> arguments = new List<string>();
                    if (control is MixPlayJoystickControlModel)
                    {
                        arguments.Add(e.input.x.ToString());
                        arguments.Add(e.input.y.ToString());
                    }
                    else if (control is MixPlayTextBoxControlModel)
                    {
                        arguments.Add(e.input.value);
                    }

                    uint sparkCost = 0;
                    string text = string.Empty;
                    if (control is MixPlayButtonControlModel)
                    {
                        MixPlayButtonControlModel button = (MixPlayButtonControlModel)control;
                        sparkCost = (uint)button.cost.GetValueOrDefault();
                        text = button.text;
                    }
                    else if (control is MixPlayTextBoxControlModel)
                    {
                        MixPlayTextBoxControlModel textBox = (MixPlayTextBoxControlModel)control;
                        sparkCost = (uint)textBox.cost.GetValueOrDefault();
                        text = textBox.submitText;
                    }

                    Dictionary<string, string> extraSpecialIdentifiers = new Dictionary<string, string>();
                    extraSpecialIdentifiers["mixplaycontrolid"] = command.Name;
                    extraSpecialIdentifiers["mixplaycontrolcost"] = sparkCost.ToString();
                    extraSpecialIdentifiers["mixplaycontroltext"] = text;

                    bool commandRun = false;
                    await this.controlCooldownSemaphore.WaitAndRelease(async () =>
                    {
                        if (await command.CheckAllRequirements(user))
                        {
                            await command.Perform(user, StreamingPlatformTypeEnum.Mixer, arguments, extraSpecialIdentifiers);
                            commandRun = true;
                        }
                    });

                    if (commandRun)
                    {
                        if (!string.IsNullOrEmpty(e.transactionID) && !user.Data.IsSparkExempt)
                        {
                            Logger.Log(LogLevel.Debug, "Sending Spark Transaction Capture - " + e.transactionID);

                            await this.CaptureSparkTransaction(e.transactionID);
                            if (sparkCost > 0)
                            {
                                GlobalEvents.SparkUseOccurred(new Tuple<UserViewModel, uint>(user, sparkCost));
                            }
                        }

                        if (control is MixPlayConnectedButtonControlModel && command.HasCooldown)
                        {
                            MixPlayConnectedButtonControlModel button = (MixPlayConnectedButtonControlModel)control;
                            if (command.IsGroupCooldown && !string.IsNullOrEmpty(command.CooldownGroupName))
                            {
                                await this.CooldownGroup(command.CooldownGroupName, command.GetCooldownTimestamp());
                            }
                            else if (command.IsIndividualCooldown)
                            {
                                await this.CooldownButton(control.controlID, command.GetCooldownTimestamp());
                            }
                        }

                        this.OnControlUsed(this, new MixPlayInputEvent(user, e, control));

                        if (ChannelSession.Settings.ChatShowMixPlayAlerts)
                        {
                            await ChannelSession.Services.Chat.AddMessage(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Mixer,
                                string.Format("{0} Used The \"{1}\" Interactive Control", user.Username, control.controlID), ChannelSession.Settings.ChatMixPlayAlertsColorScheme));
                        }
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        private async void MixPlayClient_OnDisconnectOccurred(object sender, WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred("MixPlay");

            Result result;
            do
            {
                await Task.Delay(2500);

                result = await this.Connect();
            }
            while (!result.Success);

            ChannelSession.ReconnectionOccurred("MixPlay");
        }

        #endregion MixPlay Event Handlers
    }
}
