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
    public class ActionList
    {
        [DataMember]
        public List<ActionModelBase> Actions { get; set; } = new List<ActionModelBase>();

        public ActionList() { }

        public ActionList(CommandModelBase command)
        {
            this.Actions = new List<ActionModelBase>(command.Actions);
        }
    }

    [DataContract]
    public abstract class CommandModelBase : IEquatable<CommandModelBase>, IComparable<CommandModelBase>
    {
        public static async Task RunActions(IEnumerable<ActionModelBase> actions, CommandParametersModel parameters)
        {
            List<ActionModelBase> actionsToRun = new List<ActionModelBase>(actions);
            for (int i = 0; i < actionsToRun.Count; i++)
            {
                ActionModelBase action = actionsToRun[i];
                if (action is OverlayActionModel && ChannelSession.Services.Overlay.IsConnected)
                {
                    ChannelSession.Services.Overlay.StartBatching();
                }

                await action.Perform(parameters);

                if (action is OverlayActionModel && ChannelSession.Services.Overlay.IsConnected)
                {
                    if (i == (actionsToRun.Count - 1) || !(actionsToRun[i + 1] is OverlayActionModel))
                    {
                        await ChannelSession.Services.Overlay.EndBatching();
                    }
                }
            }
        }

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
        public string GroupName { get; set; }

        [DataMember]
        public HashSet<string> Triggers { get; set; } = new HashSet<string>();

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
                this.Actions.AddRange(ActionModelBase.UpgradeAction(action));
            }
#pragma warning restore CS0612 // Type or member is obsolete
        }

        [JsonIgnore]
        protected abstract SemaphoreSlim CommandLockSemaphore { get; }

        public string TriggersString { get { return string.Join(" ", this.Triggers); } }

        protected bool IsUnlocked { get { return this.Unlocked || ChannelSession.Settings.UnlockAllCommands; } }

        public virtual Dictionary<string, string> GetUniqueSpecialIdentifiers() { return new Dictionary<string, string>(); }

        public virtual async Task TestPerform()
        {
            await this.Perform(new CommandParametersModel(ChannelSession.GetCurrentUser(), StreamingPlatformTypeEnum.All, new List<string>() { "@" + ChannelSession.GetCurrentUser().Username }, this.GetUniqueSpecialIdentifiers()));
            if (this.Requirements.Cooldown != null)
            {
                this.Requirements.Reset();
            }
        }

        public async Task Perform() { await this.Perform(new CommandParametersModel()); }

        public async Task Perform(CommandParametersModel parameters)
        {
            bool lockPerformed = false;
            try
            {
                if (this.IsEnabled && this.DoesCommandHaveWork)
                {
                    Logger.Log(LogLevel.Debug, $"Starting command performing: {this}");

                    ChannelSession.Services.Telemetry.TrackCommand(this.Type);

                    if (!this.IsUnlocked && !parameters.DontLockCommand)
                    {
                        lockPerformed = true;
                        await this.CommandLockSemaphore.WaitAsync();
                    }

                    if (!await this.ValidateRequirements(parameters))
                    {
                        return;
                    }
                    IEnumerable<CommandParametersModel> parameterList = await this.PerformRequirements(parameters);

                    this.TrackTelemetry();

                    if (parameters.WaitForCommandToFinish)
                    {
                        await this.PerformTask(parameterList, lockPerformed);
                    }
                    else
                    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        Task.Run(async () => await this.PerformTask(parameterList, lockPerformed));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            finally
            {
                if (lockPerformed || this.CommandLockSemaphore.CurrentCount == 0)
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

        protected virtual async Task<bool> ValidateRequirements(CommandParametersModel parameters)
        {
            if (this.Requirements != null)
            {
                if (!await this.Requirements.Validate(parameters))
                {
                    return false;
                }
            }
            return true;
        }

        protected virtual async Task<IEnumerable<CommandParametersModel>> PerformRequirements(CommandParametersModel parameters)
        {
            List<CommandParametersModel> users = new List<CommandParametersModel>() { parameters };
            if (this.Requirements != null)
            {
                await this.Requirements.Perform(parameters);
                users = new List<CommandParametersModel>(this.Requirements.GetPerformingUsers(parameters));
            }
            return users;
        }

        protected virtual async Task PerformInternal(CommandParametersModel parameters)
        {
            await CommandModelBase.RunActions(this.Actions, parameters);
        }

        protected virtual void TrackTelemetry() { ChannelSession.Services.Telemetry.TrackCommand(this.Type); }

        private async Task PerformTask(IEnumerable<CommandParametersModel> parameterList, bool lockPerformed)
        {
            try
            {
                foreach (CommandParametersModel parameters in parameterList)
                {
                    parameters.User.Data.TotalCommandsRun++;
                    await this.PerformInternal(parameters);
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex) { Logger.Log(ex); }
            finally
            {
                if (lockPerformed)
                {
                    lockPerformed = false;
                    this.CommandLockSemaphore.Release();
                }
            }
        }
    }
}
