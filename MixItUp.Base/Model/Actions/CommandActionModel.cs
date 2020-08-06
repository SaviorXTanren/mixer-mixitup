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
        public CommandActionTypeEnum CommandActionType { get; set; }

        [DataMember]
        public string CommandArguments { get; set; }

        [DataMember]
        public Guid CommandID { get; set; }
        [DataMember]
        public Type PreMadeType { get; set; }

        [DataMember]
        public string GroupName { get; set; }

        public CommandActionModel(CommandActionTypeEnum commandActionType, CommandModelBase command, string commandArguments)
            : base(ActionTypeEnum.Command)
        {
            this.CommandActionType = commandActionType;
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
            this.CommandArguments = commandArguments;
        }

        public CommandActionModel(CommandActionTypeEnum commandActionType, string groupName)
            : base(ActionTypeEnum.Command)
        {
            this.CommandActionType = commandActionType;
            this.GroupName = groupName;
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
            if (this.CommandActionType == CommandActionTypeEnum.RunCommand)
            {
                if (command != null)
                {
                    IEnumerable<string> newArguments = null;
                    if (!string.IsNullOrEmpty(this.CommandArguments))
                    {
                        string processedMessage = await this.ReplaceStringWithSpecialModifiers(this.CommandArguments, user, platform, arguments, specialIdentifiers);
                        newArguments = processedMessage.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    else
                    {
                        newArguments = arguments;
                    }

                    await command.Perform(user, platform, newArguments, specialIdentifiers);
                }
            }
            else if (this.CommandActionType == CommandActionTypeEnum.DisableCommand || this.CommandActionType == CommandActionTypeEnum.EnableCommand)
            {
                if (command != null)
                {
                    command.IsEnabled = (this.CommandActionType == CommandActionTypeEnum.EnableCommand) ? true : false;
                    ChannelSession.Services.Chat.RebuildCommandTriggers();
                }
            }
            else if (this.CommandActionType == CommandActionTypeEnum.DisableCommandGroup || this.CommandActionType == CommandActionTypeEnum.EnableCommandGroup)
            {
                IEnumerable<CommandModelBase> commands = this.CommandGroup;
                if (commands != null)
                {
                    foreach (CommandModelBase cmd in commands)
                    {
                        cmd.IsEnabled = (this.CommandActionType == CommandActionTypeEnum.EnableCommandGroup) ? true : false;
                    }
                    ChannelSession.Services.Chat.RebuildCommandTriggers();
                }
            }
        }
    }
}
