using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Commands
{
    public enum CommandTypeEnum
    {
        Custom = 0,
        Chat = 1,
        Event = 2,
        Timer = 3,
        ActionGroup = 4,
        Game = 5,
        Remote = 6,
        TwitchChannelPoints = 7,
        PreMade = 8,
    }

    [DataContract]
    public class CommandGroupSettingsModel
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public bool IsMinimized { get; set; }

        [DataMember]
        public int TimerInterval { get; set; }

        public CommandGroupSettingsModel() { }

        public CommandGroupSettingsModel(string name) { this.Name = name; }
    }

    [DataContract]
    public abstract class CommandModelBase
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public CommandTypeEnum Type { get; set; }

        [DataMember]
        public bool IsEnabled { get; set; }

        [DataMember]
        public bool Unlocked { get; set; }

        [DataMember]
        public RequirementsSetModel Requirements { get; set; } = new RequirementsSetModel();

        [DataMember]
        public List<ActionModelBase> Actions { get; set; } = new List<ActionModelBase>();

        public CommandModelBase(string name, CommandTypeEnum type)
        {
            this.ID = Guid.NewGuid();
            this.IsEnabled = true;
            this.Name = name;
            this.Type = type;
        }

        protected CommandModelBase(MixItUp.Base.Commands.CommandBase command)
        {
            this.ID = command.ID;
            this.IsEnabled = command.IsEnabled;
            this.Unlocked = command.Unlocked;

            if (command is MixItUp.Base.Commands.PermissionsCommandBase)
            {
                MixItUp.Base.Commands.PermissionsCommandBase pCommand = (MixItUp.Base.Commands.PermissionsCommandBase)command;
                this.Requirements = new RequirementsSetModel(pCommand.Requirements);
            }

#pragma warning disable CS0612 // Type or member is obsolete
            foreach (MixItUp.Base.Actions.ActionBase action in command.Actions)
            {
                switch (action.Type)
                {
                    case Base.Actions.ActionTypeEnum.Chat:
                        this.Actions.Add(new ChatActionModel((MixItUp.Base.Actions.ChatAction)action));
                        break;
                    case Base.Actions.ActionTypeEnum.Clips:
                        this.Actions.Add(new ClipsActionModel((MixItUp.Base.Actions.ClipsAction)action));
                        break;
                    case Base.Actions.ActionTypeEnum.Command:
                        this.Actions.Add(new CommandActionModel((MixItUp.Base.Actions.CommandAction)action));
                        break;
                    case Base.Actions.ActionTypeEnum.Conditional:
                        this.Actions.Add(new ConditionalActionModel((MixItUp.Base.Actions.ConditionalAction)action));
                        break;
                    case Base.Actions.ActionTypeEnum.Counter:
                        this.Actions.Add(new CounterActionModel((MixItUp.Base.Actions.CounterAction)action));
                        break;
                    case Base.Actions.ActionTypeEnum.Currency:
                        this.Actions.Add(new CurrencyActionModel((MixItUp.Base.Actions.CurrencyAction)action));
                        break;
                    case Base.Actions.ActionTypeEnum.Discord:
                        this.Actions.Add(new DiscordActionModel((MixItUp.Base.Actions.DiscordAction)action));
                        break;
                    case Base.Actions.ActionTypeEnum.ExternalProgram:
                        this.Actions.Add(new ExternalProgramActionModel((MixItUp.Base.Actions.ExternalProgramAction)action));
                        break;
                    case Base.Actions.ActionTypeEnum.File:
                        this.Actions.Add(new FileActionModel((MixItUp.Base.Actions.FileAction)action));
                        break;
                    case Base.Actions.ActionTypeEnum.GameQueue:
                        this.Actions.Add(new GameQueueActionModel((MixItUp.Base.Actions.GameQueueAction)action));
                        break;
                    case Base.Actions.ActionTypeEnum.IFTTT:
                        this.Actions.Add(new IFTTTActionModel((MixItUp.Base.Actions.IFTTTAction)action));
                        break;
                    case Base.Actions.ActionTypeEnum.Input:
                        this.Actions.Add(new InputActionModel((MixItUp.Base.Actions.InputAction)action));
                        break;
                    case Base.Actions.ActionTypeEnum.Moderation:
                        MixItUp.Base.Actions.ModerationAction mAction = (MixItUp.Base.Actions.ModerationAction)action;
                        if (mAction.ModerationType == Base.Actions.ModerationActionTypeEnum.VIPUser)
                        {
                            this.Actions.Add(new TwitchActionModel(TwitchActionType.VIPUser, username: mAction.UserName));
                        }
                        else if (mAction.ModerationType == Base.Actions.ModerationActionTypeEnum.UnVIPUser)
                        {
                            this.Actions.Add(new TwitchActionModel(TwitchActionType.UnVIPUser, username: mAction.UserName));
                        }
                        else
                        {
                            this.Actions.Add(new ModerationActionModel(mAction));
                        }
                        break;
                    case Base.Actions.ActionTypeEnum.Overlay:
                        this.Actions.Add(new OverlayActionModel((MixItUp.Base.Actions.OverlayAction)action));
                        break;
                    case Base.Actions.ActionTypeEnum.OvrStream:
                        this.Actions.Add(new OvrStreamActionModel((MixItUp.Base.Actions.OvrStreamAction)action));
                        break;
                    case Base.Actions.ActionTypeEnum.Serial:
                        this.Actions.Add(new SerialActionModel((MixItUp.Base.Actions.SerialAction)action));
                        break;
                    case Base.Actions.ActionTypeEnum.Sound:
                        this.Actions.Add(new SoundActionModel((MixItUp.Base.Actions.SoundAction)action));
                        break;
                    case Base.Actions.ActionTypeEnum.SpecialIdentifier:
                        this.Actions.Add(new SpecialIdentifierActionModel((MixItUp.Base.Actions.SpecialIdentifierAction)action));
                        break;
                    case Base.Actions.ActionTypeEnum.StreamingPlatform:
                        MixItUp.Base.Actions.StreamingPlatformAction spAction = (MixItUp.Base.Actions.StreamingPlatformAction)action;
                        if (spAction.ActionType == Base.Actions.StreamingPlatformActionType.Host)
                        {
                            this.Actions.Add(new TwitchActionModel(TwitchActionType.Host, channelName: spAction.HostChannelName));
                        }
                        else if (spAction.ActionType == Base.Actions.StreamingPlatformActionType.Raid)
                        {
                            this.Actions.Add(new TwitchActionModel(TwitchActionType.Raid, channelName: spAction.HostChannelName));
                        }
                        else if (spAction.ActionType == Base.Actions.StreamingPlatformActionType.RunAd)
                        {
                            this.Actions.Add(new TwitchActionModel(TwitchActionType.RunAd, adLength: spAction.AdLength));
                        }
                        break;
                    case Base.Actions.ActionTypeEnum.StreamingSoftware:
                        this.Actions.Add(new StreamingSoftwareActionModel((MixItUp.Base.Actions.StreamingSoftwareAction)action));
                        break;
                    case Base.Actions.ActionTypeEnum.Streamlabs:
                        this.Actions.Add(new StreamlabsActionModel((MixItUp.Base.Actions.StreamlabsAction)action));
                        break;
                    case Base.Actions.ActionTypeEnum.TextToSpeech:
                        this.Actions.Add(new TextToSpeechActionModel((MixItUp.Base.Actions.TextToSpeechAction)action));
                        break;
                    case Base.Actions.ActionTypeEnum.Translation:
                        MixItUp.Base.Actions.TranslationAction tAction = (MixItUp.Base.Actions.TranslationAction)action;
                        this.Actions.Add(new TranslationActionModel(tAction));
                        if (tAction.ResponseAction == Base.Actions.TranslationResponseActionTypeEnum.Chat)
                        {
                            this.Actions.Add(new ChatActionModel(tAction.ResponseChatText));
                        }
                        else if (tAction.ResponseAction == Base.Actions.TranslationResponseActionTypeEnum.SpecialIdentifier)
                        {
                            this.Actions.Add(new SpecialIdentifierActionModel(tAction.SpecialIdentifierName, "$" + TranslationActionModel.ResponseSpecialIdentifier, false, false));
                        }
                        else if (tAction.ResponseAction == Base.Actions.TranslationResponseActionTypeEnum.Command)
                        {
                            CommandActionModel cAction = new CommandActionModel(CommandActionTypeEnum.RunCommand, null);
                            cAction.CommandID = tAction.ResponseCommandID;
                            cAction.Arguments = tAction.ResponseCommandArgumentsText;
                            this.Actions.Add(cAction);
                        }
                        break;
                    case Base.Actions.ActionTypeEnum.Twitter:
                        this.Actions.Add(new TwitterActionModel((MixItUp.Base.Actions.TwitterAction)action));
                        break;
                    case Base.Actions.ActionTypeEnum.Wait:
                        this.Actions.Add(new WaitActionModel((MixItUp.Base.Actions.WaitAction)action));
                        break;
                    case Base.Actions.ActionTypeEnum.WebRequest:
                        MixItUp.Base.Actions.WebRequestAction wbAction = (MixItUp.Base.Actions.WebRequestAction)action;
                        this.Actions.Add(new WebRequestActionModel(wbAction));
                        if (wbAction.ResponseAction == Base.Actions.WebRequestResponseActionTypeEnum.Chat)
                        {
                            this.Actions.Add(new ChatActionModel(wbAction.ResponseChatText));
                        }
                        else if (wbAction.ResponseAction == Base.Actions.WebRequestResponseActionTypeEnum.Command)
                        {
                            CommandActionModel cAction = new CommandActionModel(CommandActionTypeEnum.RunCommand, null);
                            cAction.CommandID = wbAction.ResponseCommandID;
                            cAction.Arguments = wbAction.ResponseCommandArgumentsText;
                            this.Actions.Add(cAction);
                        }
                        else if (wbAction.ResponseAction == Base.Actions.WebRequestResponseActionTypeEnum.SpecialIdentifier)
                        {
                            this.Actions.Add(new SpecialIdentifierActionModel(wbAction.SpecialIdentifierName, "$" + TranslationActionModel.ResponseSpecialIdentifier, false, false));
                        }
                        break;
                }
            }
#pragma warning restore CS0612 // Type or member is obsolete
        }

        [JsonIgnore]
        protected abstract SemaphoreSlim CommandLockSemaphore { get; }

        protected bool IsUnlocked { get { return this.Unlocked || ChannelSession.Settings.UnlockAllCommands; } }

        public void PerformBackground() { this.PerformBackground(ChannelSession.GetCurrentUser()); }

        public void PerformBackground(UserViewModel user) { this.PerformBackground(user, StreamingPlatformTypeEnum.None, null, null); }

        public void PerformBackground(UserViewModel user, IEnumerable<string> arguments) { this.PerformBackground(user, StreamingPlatformTypeEnum.None, arguments, null); }

        public void PerformBackground(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers) { this.PerformBackground(user, StreamingPlatformTypeEnum.None, arguments, specialIdentifiers); }

        public void PerformBackground(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            Task.Run(() => this.Perform(user, platform, arguments, specialIdentifiers));
        }

        public async Task Perform() { await this.Perform(ChannelSession.GetCurrentUser()); }

        public async Task Perform(UserViewModel user) { await this.Perform(user, StreamingPlatformTypeEnum.None, null, null); }

        public async Task Perform(UserViewModel user, IEnumerable<string> arguments) { await this.Perform(user, StreamingPlatformTypeEnum.None, arguments, null); }

        public async Task Perform(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers) { await this.Perform(user, StreamingPlatformTypeEnum.None, arguments, specialIdentifiers); }

        public async Task Perform(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            try
            {
                if (this.IsEnabled && this.DoesCommandHaveWork)
                {
                    Logger.Log(LogLevel.Debug, $"Starting command performing: {this}");

                    ChannelSession.Services.Telemetry.TrackCommand(this.Type);

                    if (user == null)
                    {
                        user = ChannelSession.GetCurrentUser();
                    }

                    if (arguments == null)
                    {
                        arguments = new List<string>();
                    }

                    if (specialIdentifiers == null)
                    {
                        specialIdentifiers = new Dictionary<string, string>();
                    }

                    if (platform == StreamingPlatformTypeEnum.None)
                    {
                        platform = user.Platform;
                    }

                    await this.CommandLockSemaphore.WaitAsync();

                    if (!await this.ValidateRequirements(user, platform, arguments, specialIdentifiers))
                    {
                        return;
                    }
                    IEnumerable<UserViewModel> users = await this.PerformRequirements(user, platform, arguments, specialIdentifiers);

                    if (this.IsUnlocked)
                    {
                        this.CommandLockSemaphore.Release();
                    }

                    this.TrackTelemetry();

                    foreach (UserViewModel u in users)
                    {
                        u.Data.TotalCommandsRun++;
                        await this.PerformInternal(u, platform, arguments, specialIdentifiers);
                    }

                    if (!this.IsUnlocked)
                    {
                        this.CommandLockSemaphore.Release();
                    }
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex) { Logger.Log(ex); }
            finally
            {
                if (this.CommandLockSemaphore.CurrentCount == 0)
                {
                    this.CommandLockSemaphore.Release();
                }
            }
        }

        public override string ToString() { return string.Format("{0} - {1}", this.ID, this.Name); }

        public int CompareTo(object obj)
        {
            if (obj is CommandModelBase)
            {
                return this.CompareTo((CommandModelBase)obj);
            }
            return 0;
        }

        public int CompareTo(CommandModelBase other) { return this.Name.CompareTo(other.Name); }

        public override bool Equals(object obj)
        {
            if (obj is CommandModelBase)
            {
                return this.Equals((CommandModelBase)obj);
            }
            return false;
        }

        public bool Equals(CommandModelBase other) { return this.ID.Equals(other.ID); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }

        public virtual bool DoesCommandHaveWork { get { return this.Actions.Count > 0; } }

        protected virtual async Task<bool> ValidateRequirements(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            if (this.Requirements != null)
            {
                if (!await this.Requirements.Validate(user, platform, arguments, specialIdentifiers))
                {
                    return false;
                }
            }
            return true;
        }

        protected virtual async Task<IEnumerable<UserViewModel>> PerformRequirements(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            List<UserViewModel> users = new List<UserViewModel>() { user };
            if (this.Requirements != null)
            {
                await this.Requirements.Perform(user, platform, arguments, specialIdentifiers);
                users = new List<UserViewModel>(this.Requirements.GetPerformingUsers(user, platform, arguments, specialIdentifiers));
            }
            return users;
        }

        protected virtual async Task PerformInternal(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            List<ActionModelBase> actionsToRun = new List<ActionModelBase>(this.Actions);
            for (int i = 0; i < actionsToRun.Count; i++)
            {
                ActionModelBase action = actionsToRun[i];
                //if (action is OverlayAction && ChannelSession.Services.Overlay.IsConnected)
                //{
                //    ChannelSession.Services.Overlay.StartBatching();
                //}

                await action.Perform(user, platform, arguments, specialIdentifiers);

                //if (action is OverlayAction && ChannelSession.Services.Overlay.IsConnected)
                //{
                //    if (i == (actionsToRun.Count - 1) || !(actionsToRun[i + 1] is OverlayAction))
                //    {
                //        await ChannelSession.Services.Overlay.EndBatching();
                //    }
                //}
            }
        }

        protected virtual void TrackTelemetry() { ChannelSession.Services.Telemetry.TrackCommand(this.Type); }
    }
}
