using MixItUp.Base.Model.Actions;
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

        [JsonIgnore]
        public CommandModelBase Command { get { return ChannelSession.Settings.GetCommand(this.CommandID); } }

        [JsonIgnore]
        public CommandTypeEnum CommandType
        {
            get
            {
                CommandModelBase command = this.Command;
                if (command != null)
                {
                    return command.Type;
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
