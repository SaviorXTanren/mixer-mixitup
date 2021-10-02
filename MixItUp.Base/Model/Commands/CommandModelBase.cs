using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Util;
using Newtonsoft.Json;
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
        StreamlootsCard = 9,
        Webhook = 10,

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

        public bool IsUnlocked { get { return this.Unlocked; } }

        public bool HasWork { get { return this.Actions.Count > 0 || this.HasCustomRun; } }

        public virtual bool HasCustomRun { get { return false; } }

        public virtual Dictionary<string, string> GetTestSpecialIdentifiers() { return CommandModelBase.GetGeneralTestSpecialIdentifiers(); }

        public virtual void TrackTelemetry() { ChannelSession.Services.Telemetry.TrackCommand(this.Type); }

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

            foreach (ActionModelBase action in this.Actions)
            {
                if (action.Type == ActionTypeEnum.Command)
                {
                    CommandActionModel commandAction = (CommandActionModel)action;
                    CommandModelBase subCommand = ChannelSession.Settings.GetCommand(commandAction.CommandID);
                    if (subCommand != null)
                    {
                        foreach (ActionTypeEnum subActionType in subCommand.GetActionTypesInCommand(commandIDs))
                        {
                            actionTypes.Add(subActionType);
                        }
                    }
                }
                else if (action.Type == ActionTypeEnum.Overlay)
                {
                    OverlayActionModel overlayAction = (OverlayActionModel)action;
                    if (overlayAction.OverlayItem.ItemType == Overlay.OverlayItemModelTypeEnum.Video || overlayAction.OverlayItem.ItemType == Overlay.OverlayItemModelTypeEnum.YouTube)
                    {
                        actionTypes.Add(ActionTypeEnum.Sound);
                    }
                }
                else if (action.Type == ActionTypeEnum.TextToSpeech)
                {
                    actionTypes.Add(ActionTypeEnum.Sound);
                }
                else if (action.Type == ActionTypeEnum.OvrStream)
                {
                    actionTypes.Add(ActionTypeEnum.Sound);
                    actionTypes.Add(ActionTypeEnum.Overlay);
                }
                else if (action.Type == ActionTypeEnum.StreamingSoftware)
                {
                    actionTypes.Add(ActionTypeEnum.Sound);
                    actionTypes.Add(ActionTypeEnum.Overlay);
                }
                actionTypes.Add(action.Type);
            }

            actionTypes.Remove(ActionTypeEnum.Command);
            actionTypes.Remove(ActionTypeEnum.Wait);

            return actionTypes;
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

        public virtual Task PreRun(CommandParametersModel parameters) { return Task.FromResult(0); }

        public virtual Task CustomRun(CommandParametersModel parameters) { return Task.FromResult(0); }

        public virtual Task PostRun(CommandParametersModel parameters) { return Task.FromResult(0); }

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
