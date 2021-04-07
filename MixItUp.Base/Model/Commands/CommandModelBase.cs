using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
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
        [Obsolete]
        Remote = 6,
        TwitchChannelPoints = 7,
        PreMade = 8,

        // Specialty Command Types
        UserOnlyChat = 1000,
    }

    [DataContract]
    public class CommandGroupSettingsModel
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int TimerInterval { get; set; }

        public CommandGroupSettingsModel() { }

        public CommandGroupSettingsModel(string name) { this.Name = name; }

#pragma warning disable CS0612 // Type or member is obsolete
        internal CommandGroupSettingsModel(MixItUp.Base.Commands.CommandGroupSettings oldGroupSettings)
        {
            this.Name = oldGroupSettings.Name;
            this.TimerInterval = oldGroupSettings.TimerInterval;
        }
#pragma warning restore CS0612 // Type or member is obsolete
    }

    [DataContract]
    public abstract class CommandModelBase : IEquatable<CommandModelBase>, IComparable<CommandModelBase>
    {
        public const string CommandNameSpecialIdentifier = "commandname";

        public static IEnumerable<CommandTypeEnum> GetSelectableCommandTypes()
        {
            List<CommandTypeEnum> types = new List<CommandTypeEnum>(EnumHelper.GetEnumList<CommandTypeEnum>());
            types.Remove(CommandTypeEnum.PreMade);
            types.Remove(CommandTypeEnum.UserOnlyChat);
            types.Remove(CommandTypeEnum.Custom);
            return types;
        }

        public static Dictionary<string, string> GetGeneralTestSpecialIdentifiers() { return new Dictionary<string, string>(); }

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
        public bool IsEmbedded { get; set; }

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

#pragma warning disable CS0612 // Type or member is obsolete
        protected CommandModelBase(MixItUp.Base.Commands.CommandBase command)
        {
            if (command != null)
            {
                this.ID = command.ID;
                this.GroupName = command.GroupName;
                this.IsEnabled = command.IsEnabled;
                this.Unlocked = command.Unlocked;

                if (command is MixItUp.Base.Commands.PermissionsCommandBase)
                {
                    MixItUp.Base.Commands.PermissionsCommandBase pCommand = (MixItUp.Base.Commands.PermissionsCommandBase)command;
                    this.Requirements = new RequirementsSetModel(pCommand.Requirements);
                }

                foreach (MixItUp.Base.Actions.ActionBase action in command.Actions)
                {
                    this.Actions.AddRange(ActionModelBase.UpgradeAction(action));
                }
            }
            else
            {
                this.ID = Guid.NewGuid();
            }
        }
#pragma warning restore CS0612 // Type or member is obsolete

        protected CommandModelBase() { }

        public string TriggersString { get { return string.Join(" ", this.Triggers); } }

        public virtual IEnumerable<string> GetFullTriggers() { return this.Triggers; }

        public CommandGroupSettingsModel CommandGroupSettings { get { return (!string.IsNullOrEmpty(this.GroupName) && ChannelSession.Settings.CommandGroups.ContainsKey(this.GroupName)) ? ChannelSession.Settings.CommandGroups[this.GroupName] : null; } }

        public bool IsUnlocked { get { return this.Unlocked || ChannelSession.Settings.UnlockAllCommands; } }

        public virtual Dictionary<string, string> GetTestSpecialIdentifiers() { return CommandModelBase.GetGeneralTestSpecialIdentifiers(); }

        public virtual Task<bool> CustomValidation() { return Task.FromResult(true); }

        public virtual async Task TestPerform()
        {
            await this.Perform(CommandParametersModel.GetTestParameters(this.GetTestSpecialIdentifiers()));
            if (this.Requirements.Cooldown != null)
            {
                this.Requirements.Reset();
            }
        }

        public async Task Perform() { await this.Perform(new CommandParametersModel()); }

        public virtual async Task Perform(CommandParametersModel parameters)
        {

        }

        public virtual Task CustomPerform(CommandParametersModel parameters) { return Task.FromResult(0); }

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

        public bool DoesCommandHaveWork { get { return this.Actions.Count > 0 || this.HasCustomPerform; } }

        public virtual bool HasCustomPerform { get { return false; } }

        public virtual async Task<bool> ValidateRequirements(CommandParametersModel parameters)
        {
            if (this.Requirements != null)
            {
                Result result = await this.Requirements.Validate(parameters);
                return result.Success;
            }
            return true;
        }

        public async Task PerformRequirements(CommandParametersModel parameters)
        {
            await this.Requirements.Perform(parameters);
        }

        public IEnumerable<CommandParametersModel> GetPerformingUsers(CommandParametersModel parameters)
        {
            return this.Requirements.GetPerformingUsers(parameters);
        }

        protected virtual async Task PerformInternal(CommandParametersModel parameters)
        {

        }
    }
}
