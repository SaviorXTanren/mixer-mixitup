using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
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
            : base(ActionTypeEnum.Command)
        {
            this.ActionType = commandActionType;
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
            : base(ActionTypeEnum.Command)
        {
            this.ActionType = commandActionType;
            this.CommandGroupName = groupName;
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
                    return ChannelSession.PreMadeChatCommands.FirstOrDefault(c => c.GetType().Equals(this.PreMadeType));
                }
                else
                {
                    return ChannelSession.AllCommands.FirstOrDefault(c => c.ID.Equals(this.CommandID));
                }
            }
        }

        public IEnumerable<CommandModelBase> CommandGroup
        {
            get
            {
                return ChannelSession.AllCommands.Where(c => string.Equals(this.CommandGroupName, c.GroupName)).ToList();
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
                        string processedMessage = await this.ReplaceStringWithSpecialModifiers(this.Arguments, parameters);
                        newArguments = processedMessage.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    }
                    else
                    {
                        newArguments = parameters.Arguments;
                    }

                    CommandParametersModel copyParameters = parameters.Duplicate();
                    copyParameters.Arguments = newArguments;
                    copyParameters.WaitForCommandToFinish = copyParameters.DontLockCommand = this.WaitForCommandToFinish;
                    await command.Perform(copyParameters);
                }
            }
            else if (this.ActionType == CommandActionTypeEnum.DisableCommand || this.ActionType == CommandActionTypeEnum.EnableCommand)
            {
                if (command != null)
                {
                    command.IsEnabled = (this.ActionType == CommandActionTypeEnum.EnableCommand) ? true : false;
                    ServiceManager.Get<ChatService>().RebuildCommandTriggers();
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
                    ServiceManager.Get<ChatService>().RebuildCommandTriggers();
                }
            }
        }
    }
}
