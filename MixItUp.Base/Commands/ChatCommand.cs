using MixItUp.Base.ViewModel.Import;
using MixItUp.Base.ViewModel.Requirement;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;

namespace MixItUp.Base.Commands
{
    [DataContract]
    public class ChatCommand : PermissionsCommandBase
    {
        private static SemaphoreSlim chatCommandPerformSemaphore = new SemaphoreSlim(1);

        [DataMember]
        public bool IncludeExclamationInCommands { get; set; }

        public ChatCommand() { }

        public ChatCommand(string name, string command, RequirementViewModel requirements)
            : this(name, new List<string>() { command }, requirements)
        { }

        public ChatCommand(string name, IEnumerable<string> commands, RequirementViewModel requirements)
            : base(name, CommandTypeEnum.Chat, commands, requirements)
        {
            this.IncludeExclamationInCommands = true;
        }

        public ChatCommand(ScorpBotCommand command)
            : this(command.Command, command.Command, command.Requirements)
        {
            this.Actions.AddRange(command.Actions);
            this.IncludeExclamationInCommands = false;
            this.IsEnabled = command.Enabled;
        }

        public override bool ContainsCommand(string command)
        {
            var commandsToCheck = this.Commands;
            if (this.IncludeExclamationInCommands)
            {
                commandsToCheck = commandsToCheck.Select(c => "!" + c).ToList();
            }
            return commandsToCheck.Contains(command);
        }

        protected override SemaphoreSlim AsyncSemaphore { get { return ChatCommand.chatCommandPerformSemaphore; } }
    }
}
