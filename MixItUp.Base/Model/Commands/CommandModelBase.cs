using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json;
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
        StreamlootsCard = 9,
        Webhook = 10,
        TrovoSpell = 11,
        TwitchBits = 12,
        CrowdControlEffect = 13,

        // Specialty Command Types
        UserOnlyChat = 1000,

        [Obsolete]
        All = 99999999,
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
            types.Remove(CommandTypeEnum.Webhook);
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

        [JsonIgnore]
        public string GroupDisplayName { get { return (!string.IsNullOrEmpty(this.GroupName)) ? this.GroupName : MixItUp.Base.Resources.Ungrouped; } }

        [DataMember]
        public HashSet<string> Triggers { get; set; } = new HashSet<string>();

        [DataMember]
        public RequirementsSetModel Requirements { get; set; } = new RequirementsSetModel();

        [DataMember]
        public List<ActionModelBase> Actions { get; set; } = new List<ActionModelBase>();

        [JsonIgnore]
        public DateTimeOffset CommandErrorCooldown = DateTimeOffset.MinValue;

        public CommandModelBase(string name, CommandTypeEnum type)
        {
            this.ID = Guid.NewGuid();
            this.IsEnabled = true;
            this.Name = name;
            this.Type = type;
        }

        [Obsolete]
        public CommandModelBase() { }

        public string TriggersString { get { return string.Join(" ", this.Triggers); } }

        public virtual IEnumerable<string> GetFullTriggers() { return this.Triggers; }

        public CommandGroupSettingsModel CommandGroupSettings { get { return (!string.IsNullOrEmpty(this.GroupName) && ChannelSession.Settings.CommandGroups.ContainsKey(this.GroupName)) ? ChannelSession.Settings.CommandGroups[this.GroupName] : null; } }

        public bool IsUnlocked { get { return this.Unlocked; } }

        public bool HasWork { get { return this.Actions.Count > 0 || this.HasCustomRun; } }

        public virtual bool HasCustomRun { get { return false; } }

        public virtual Dictionary<string, string> GetTestSpecialIdentifiers() { return CommandModelBase.GetGeneralTestSpecialIdentifiers(); }

        public virtual void TrackTelemetry() { ServiceManager.Get<ITelemetryService>().TrackCommand(this.Type); }

        public virtual HashSet<ActionTypeEnum> GetActionTypesInCommand(HashSet<Guid> commandIDs = null)
        {
            HashSet<ActionTypeEnum> actionTypes = new HashSet<ActionTypeEnum>();

            if (commandIDs == null)
            {
                commandIDs = new HashSet<Guid>();
            }

            if (commandIDs.Contains(this.ID))
            {
                return actionTypes;
            }
            commandIDs.Add(this.ID);

            return CommandService.GetActionTypesForActions(this.Actions, commandIDs);
        }

        public virtual Task<Result> CustomValidation(CommandParametersModel parameters) { return Task.FromResult(new Result()); }

        public virtual async Task<Result> ValidateRequirements(CommandParametersModel parameters)
        {
            if (this.Requirements != null)
            {
                if (ChannelSession.Settings.RequirementErrorsCooldownType == RequirementErrorCooldownTypeEnum.PerCommand)
                {
                    this.Requirements.SetIndividualErrorCooldown(this.CommandErrorCooldown);
                }
                return await this.Requirements.Validate(parameters);
            }
            return new Result();
        }

        public async Task PerformRequirements(CommandParametersModel parameters) { await this.Requirements.Perform(parameters); }

        public IEnumerable<CommandParametersModel> GetPerformingUsers(CommandParametersModel parameters) { return this.Requirements.GetPerformingUsers(parameters); }

        public virtual Task PreRun(CommandParametersModel parameters) { return Task.CompletedTask; }

        public virtual Task CustomRun(CommandParametersModel parameters) { return Task.CompletedTask; }

        public virtual Task PostRun(CommandParametersModel parameters) { return Task.CompletedTask; }

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
    }
}
