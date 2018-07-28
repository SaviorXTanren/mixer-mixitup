using Mixer.Base.Util;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public enum CommandActionTypeEnum
    {
        [Name("Run Command")]
        RunCommand,
        [Name("Disable Command")]
        DisableCommand,
        [Name("Enable Command")]
        EnableCommand,
    }

    [DataContract]
    public class CommandAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return CommandAction.asyncSemaphore; } }

        [DataMember]
        public CommandActionTypeEnum CommandActionType { get; set; }

        [DataMember]
        public Guid CommandID { get; set; }

        [DataMember]
        public Type PreMadeType { get; set; }

        [DataMember]
        public string CommandArguments { get; set; }

        public CommandAction() : base(ActionTypeEnum.Command) { }

        public CommandAction(CommandActionTypeEnum commandActionType, CommandBase command, string commandArguments)
            : this()
        {
            this.CommandActionType = commandActionType;
            if (command is PreMadeChatCommand)
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

        public CommandBase Command
        {
            get
            {
                if (this.PreMadeType != null)
                {
                    return ChannelSession.AllCommands.FirstOrDefault(c => c.GetType().Equals(this.PreMadeType));
                }
                else
                {
                    return ChannelSession.AllCommands.FirstOrDefault(c => c.ID.Equals(this.CommandID));
                }
            }
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            CommandBase command = this.Command;
            if (this.CommandActionType == CommandActionTypeEnum.RunCommand)
            {
                if (command != null)
                {
                    IEnumerable<string> newArguments = null;
                    if (!string.IsNullOrEmpty(this.CommandArguments))
                    {
                        string processedMessage = await this.ReplaceStringWithSpecialModifiers(this.CommandArguments, user, arguments);
                        newArguments = processedMessage.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    }

                    await command.Perform(user, newArguments, this.GetExtraSpecialIdentifiers());
                }
            }
            else if (this.CommandActionType == CommandActionTypeEnum.DisableCommand || this.CommandActionType == CommandActionTypeEnum.EnableCommand)
            {
                if (command != null)
                {
                    command.IsEnabled = (this.CommandActionType == CommandActionTypeEnum.EnableCommand) ? true : false;
                }
            }
        }
    }
}
