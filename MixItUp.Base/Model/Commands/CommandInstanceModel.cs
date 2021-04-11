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
        public event EventHandler<CommandInstanceStateEnum> OnStateUpdated = delegate { };

        [DataMember]
        public Guid ID { get; set; } = Guid.NewGuid();
        
        [DataMember]
        public Guid CommandID { get; set; }

        [DataMember]
        public List<ActionModelBase> Actions { get; set; } = new List<ActionModelBase>();

        [DataMember]
        public CommandInstanceStateEnum State
        {
            get { return this.state; }
            set
            {
                this.state = value;
                this.OnStateUpdated(null, this.State);
            }
        }
        private CommandInstanceStateEnum state;

        [DataMember]
        public CommandParametersModel Parameters { get; set; }

        [DataMember]
        public DateTimeOffset DateTime { get; set; } = DateTimeOffset.Now;

        [DataMember]
        public string ErrorMessage { get; set; }

        [JsonIgnore]
        public CommandModelBase Command
        {
            get
            {
                if (this.command != null)
                {
                    return this.command;
                }
                return ChannelSession.Settings.GetCommand(this.CommandID);
            }
        }
        private CommandModelBase command;

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
            if (command is PreMadeChatCommandModelBase)
            {
                this.command = command;
            }
            else
            {
                this.CommandID = command.ID;
            }
            this.Parameters = parameters;
        }

        public CommandInstanceModel(Guid commandID, CommandParametersModel parameters)
        {
            this.CommandID = commandID;
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

        public CommandInstanceModel Duplicate()
        {
            if (this.command != null)
            {
                return new CommandInstanceModel(command, this.Parameters);
            }
            else if (this.CommandID != Guid.Empty)
            {
                return new CommandInstanceModel(this.CommandID, this.Parameters);
            }
            else
            {
                return new CommandInstanceModel(this.Actions, this.Parameters);
            }
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
