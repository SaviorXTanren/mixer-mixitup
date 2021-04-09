using MixItUp.Base.Model.Actions;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Commands
{
    public enum CommandInstanceStateEnum
    {
        Pending,
        Running,
        Completed,
        Failed,
        Canceled
    }

    [DataContract]
    public class CommandInstanceModel
    {
        [DataMember]
        public Guid ID { get; set; } = Guid.NewGuid();
        
        [DataMember]
        public Guid CommandID { get; set; }

        [DataMember]
        public List<ActionModelBase> Actions { get; set; } = new List<ActionModelBase>();

        [DataMember]
        public CommandInstanceStateEnum State { get; set; } = CommandInstanceStateEnum.Pending;

        [DataMember]
        public CommandParametersModel Parameters { get; set; }

        [DataMember]
        public DateTimeOffset DateTime { get; set; } = DateTimeOffset.Now;

        [DataMember]
        public string ErrorMessage { get; set; }

        [JsonIgnore]
        public CommandModelBase Command { get { return ChannelSession.Settings.GetCommand(this.CommandID); } }

        [JsonIgnore]
        public CommandTypeEnum QueueCommandType
        {
            get
            {
                CommandModelBase command = this.Command;
                if (command != null)
                {
                    switch (command.Type)
                    {
                        case CommandTypeEnum.PreMade: return CommandTypeEnum.Chat;
                        case CommandTypeEnum.UserOnlyChat: return CommandTypeEnum.Chat;
                        default: return command.Type;
                    }
                }
                return CommandTypeEnum.Custom;
            }
        }

        [JsonIgnore]
        public bool DontQueue
        {
            get
            {
                CommandModelBase command = this.Command;
                if (command != null)
                {
                    return command.IsUnlocked || ChannelSession.Settings.UnlockAllCommands;
                }
                return true;
            }
        }

        public CommandInstanceModel(CommandModelBase command) : this(command, new CommandParametersModel()) { }

        public CommandInstanceModel(CommandModelBase command, CommandParametersModel parameters)
        {
            this.CommandID = command.ID;
            this.Parameters = parameters;
        }

        public CommandInstanceModel(IEnumerable<ActionModelBase> actions, CommandParametersModel parameters)
        {
            this.Actions = new List<ActionModelBase>(actions);
            this.Parameters = parameters;
        }

        public List<ActionModelBase> GetActions()
        {
            List<ActionModelBase> actions = new List<ActionModelBase>();

            CommandModelBase command = this.Command;
            if (command != null)
            {
                if (command is ActionGroupCommandModel && ((ActionGroupCommandModel)command).RunOneRandomly)
                {
                    actions.Add(command.Actions.Random());
                }
                else
                {
                    actions.AddRange(command.Actions);
                }
            }
            else
            {
                actions.AddRange(this.Actions);
            }

            return actions;
        }

        public override string ToString()
        {
            CommandModelBase command = this.Command;
            if (command != null)
            {
                return command.ToString();
            }
            else
            {
                return "Action List";
            }
        }
    }
}
