using Mixer.Base.Util;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
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
        [Name("Enable/Disable Command")]
        EnableDisableCommand,
    }

    [DataContract]
    public class CommandAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return CommandAction.asyncSemaphore; } }

        [DataMember]
        public CommandActionTypeEnum CommandActionType { get; set; }

        [DataMember]
        public string CommandName { get; set; }

        [DataMember]
        public string CommandArguments { get; set; }

        public CommandAction() : base(ActionTypeEnum.Command) { }

        public CommandAction(CommandActionTypeEnum commandActionType, PermissionsCommandBase command, string commandArguments)
            : this()
        {
            this.CommandActionType = commandActionType;
            this.CommandName = command.Name;
            this.CommandArguments = commandArguments;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (this.CommandActionType == CommandActionTypeEnum.RunCommand)
            {
                PermissionsCommandBase command = ChannelSession.AllEnabledChatCommands.FirstOrDefault(c => c.Name.Equals(this.CommandName));
                if (command != null)
                {
                    command.AddSpecialIdentifiers(this.GetAdditiveSpecialIdentifiers());

                    // Do we need to apply special identifiers to arguments now?
                    if (!string.IsNullOrEmpty(this.CommandArguments))
                    {
                        string processedMessage = await this.ReplaceStringWithSpecialModifiers(this.CommandArguments, user, arguments);
                        arguments = arguments.Union(processedMessage.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
                    }

                    // Consider trying to prevent recursion here a bit?
                    await command.Perform(user, arguments);
                }
            }
            else if (this.CommandActionType == CommandActionTypeEnum.EnableDisableCommand)
            {
                PermissionsCommandBase command = ChannelSession.AllChatCommands.FirstOrDefault(c => c.Name.Equals(this.CommandName));
                if (command != null)
                {
                    command.IsEnabled = !command.IsEnabled;
                }
            }
        }
    }
}
