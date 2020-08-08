using MixItUp.Base.Commands;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
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
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return CommandActionModel.asyncSemaphore; } }

        [DataMember]
        public CommandActionTypeEnum ActionType { get; set; }

        [DataMember]
        public Guid CommandID { get; set; }
        [DataMember]
        public Type PreMadeType { get; set; }

        [DataMember]
        public string Arguments { get; set; }

        [DataMember]
        public string GroupName { get; set; }

        public CommandActionModel(CommandActionTypeEnum commandActionType, CommandModelBase command, string commandArguments)
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
        }

        public CommandActionModel(CommandActionTypeEnum commandActionType, string groupName)
            : base(ActionTypeEnum.Command)
        {
            this.ActionType = commandActionType;
            this.GroupName = groupName;
        }

        internal CommandActionModel(MixItUp.Base.Actions.CommandAction action)
            : base(ActionTypeEnum.Command)
        {
            this.ActionType = (CommandActionTypeEnum)(int)action.CommandActionType;
            this.CommandID = action.CommandID;
            this.PreMadeType = action.PreMadeType;
            this.Arguments = action.CommandArguments;
            this.GroupName = action.GroupName;
        }

        public CommandModelBase Command
        {
            get
            {
                // TODO
                return null;
                //if (this.PreMadeType != null)
                //{
                //    return ChannelSession.AllCommands.FirstOrDefault(c => c.GetType().Equals(this.PreMadeType));
                //}
                //else
                //{
                //    return ChannelSession.AllCommands.FirstOrDefault(c => c.ID.Equals(this.CommandID));
                //}
            }
        }

        public IEnumerable<CommandModelBase> CommandGroup
        {
            get
            {
                // TODO
                return null;
                // return CommandModelBase.AllCommands.Where(c => !string.IsNullOrEmpty(c.GroupName) && c.GroupName.Equals(this.GroupName));
            }
        }

        protected override async Task PerformInternal(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            CommandModelBase command = this.Command;
            if (this.ActionType == CommandActionTypeEnum.RunCommand)
            {
                if (command != null)
                {
                    IEnumerable<string> newArguments = null;
                    if (!string.IsNullOrEmpty(this.Arguments))
                    {
                        string processedMessage = await this.ReplaceStringWithSpecialModifiers(this.Arguments, user, platform, arguments, specialIdentifiers);
                        newArguments = processedMessage.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    else
                    {
                        newArguments = arguments;
                    }

                    await command.Perform(user, platform, newArguments, specialIdentifiers);
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
                    }
                    ChannelSession.Services.Chat.RebuildCommandTriggers();
                }
            }
        }
    }
}
