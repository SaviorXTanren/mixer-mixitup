using MixItUp.Base.Model.Commands;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum CommandActionTypeEnum
    {
        RunCommand,
        DisableCommand,
        EnableCommand,
        DisableCommandGroup,
        EnableCommandGroup,
        CancelAllCommands,
    }

    [DataContract]
    public class CommandActionModel : ActionModelBase
    {
        [DataMember]
        public CommandActionTypeEnum ActionType { get; set; }

        [DataMember]
        public Guid CommandID { get; set; }
        [DataMember]
        public Type PreMadeType { get; set; }

        [DataMember]
        public string Arguments { get; set; }
        [DataMember]
        public bool WaitForCommandToFinish { get; set; }

        [DataMember]
        public string CommandGroupName { get; set; }

        public CommandActionModel(CommandActionTypeEnum commandActionType, CommandModelBase command, string commandArguments, bool waitForCommandToFinish)
            : this(commandActionType)
        {
            if (command is PreMadeChatCommandModelBase)
            {
                this.PreMadeType = command.GetType();
                this.CommandID = Guid.Empty;
            }
            else
            {
                this.CommandID = command.ID;
                this.PreMadeType = null;
            }
            this.Arguments = commandArguments;
            this.WaitForCommandToFinish = waitForCommandToFinish;
        }

        public CommandActionModel(CommandActionTypeEnum commandActionType, string groupName)
            : this(commandActionType)
        {
            this.CommandGroupName = groupName;
        }

        public CommandActionModel(CommandActionTypeEnum commandActionType)
            : base(ActionTypeEnum.Command)
        {
            this.ActionType = commandActionType;
        }

#pragma warning disable CS0612 // Type or member is obsolete
        internal CommandActionModel(MixItUp.Base.Actions.CommandAction action)
            : base(ActionTypeEnum.Command)
        {
            this.ActionType = (CommandActionTypeEnum)(int)action.CommandActionType;
            this.CommandID = action.CommandID;
            if (!string.IsNullOrEmpty(action.PreMadeType))
            {
                string typeName = action.PreMadeType.Replace("ChatCommand", "PreMadeChatCommandModel");
                typeName = typeName.Replace("MixItUp.Base.Commands", "MixItUp.Base.Model.Commands");
                try
                {
                    Type type = System.Type.GetType(typeName);
                    if (type != null)
                    {
                        this.PreMadeType = type;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            }
            this.Arguments = action.CommandArguments;
            this.WaitForCommandToFinish = false;
            this.CommandGroupName = action.GroupName;
        }
#pragma warning restore CS0612 // Type or member is obsolete

        private CommandActionModel() { }

        public CommandModelBase Command
        {
            get
            {
                if (this.PreMadeType != null)
                {
                    return ChannelSession.Services.Command.PreMadeChatCommands.FirstOrDefault(c => c.GetType().Equals(this.PreMadeType));
                }
                else
                {
                    return ChannelSession.Services.Command.AllCommands.FirstOrDefault(c => c.ID.Equals(this.CommandID));
                }
            }
        }

        public IEnumerable<CommandModelBase> CommandGroup
        {
            get
            {
                return ChannelSession.Services.Command.AllCommands.Where(c => string.Equals(this.CommandGroupName, c.GroupName)).ToList();
            }
        }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            CommandModelBase command = this.Command;
            if (this.ActionType == CommandActionTypeEnum.RunCommand)
            {
                if (command != null)
                {
                    List<string> newArguments = new List<string>();
                    if (!string.IsNullOrEmpty(this.Arguments))
                    {
                        string processedMessage = await ReplaceStringWithSpecialModifiers(this.Arguments, parameters);
                        newArguments = processedMessage.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    }
                    else
                    {
                        newArguments = parameters.Arguments;
                    }

                    CommandParametersModel copyParameters = parameters.Duplicate();
                    copyParameters.Arguments = newArguments;

                    CommandInstanceModel commandInstance = new CommandInstanceModel(command, copyParameters);
                    if (this.WaitForCommandToFinish)
                    {
                        await ChannelSession.Services.Command.RunDirectlyWithValidation(commandInstance);
                    }
                    else
                    {
                        await ChannelSession.Services.Command.Queue(commandInstance);
                    }
                }
            }
            else if (this.ActionType == CommandActionTypeEnum.DisableCommand || this.ActionType == CommandActionTypeEnum.EnableCommand)
            {
                if (command != null)
                {
                    command.IsEnabled = (this.ActionType == CommandActionTypeEnum.EnableCommand) ? true : false;
                    ChannelSession.Services.Chat.RebuildCommandTriggers();
                }
            }
            else if (this.ActionType == CommandActionTypeEnum.DisableCommandGroup || this.ActionType == CommandActionTypeEnum.EnableCommandGroup)
            {
                IEnumerable<CommandModelBase> commands = this.CommandGroup;
                if (commands != null)
                {
                    foreach (CommandModelBase cmd in commands)
                    {
                        cmd.IsEnabled = (this.ActionType == CommandActionTypeEnum.EnableCommandGroup) ? true : false;
                        ChannelSession.Settings.Commands.ManualValueChanged(cmd.ID);
                    }
                    ChannelSession.Services.Chat.RebuildCommandTriggers();
                }
            }
            else if (this.ActionType == CommandActionTypeEnum.CancelAllCommands)
            {
                foreach (CommandInstanceModel commandInstance in ChannelSession.Services.Command.CommandInstances.Where(c => c.State == CommandInstanceStateEnum.Pending || c.State == CommandInstanceStateEnum.Running))
                {
                    ChannelSession.Services.Command.Cancel(commandInstance);
                }
            }
        }
    }
}
