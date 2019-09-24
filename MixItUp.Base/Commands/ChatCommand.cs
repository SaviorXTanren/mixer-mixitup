using MixItUp.Base.Model.Import;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Requirement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;

namespace MixItUp.Base.Commands
{
    [DataContract]
    public class ChatCommand : PermissionsCommandBase
    {
        private static SemaphoreSlim chatCommandPerformSemaphore = new SemaphoreSlim(1);

        private const string CommandMatchingRegexFormat = "^{0}*";

        [DataMember]
        public bool IncludeExclamationInCommands { get; set; }

        [DataMember]
        public bool Wildcards { get; set; }

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
            this.IncludeExclamationInCommands = command.ContainsExclamation;
            this.IsEnabled = command.Enabled;
        }

        public ChatCommand(StreamlabsChatBotCommand command)
            : this(command.Command, command.Command, command.Requirements)
        {
            this.Actions.AddRange(command.Actions);
            this.IncludeExclamationInCommands = false;
            this.IsEnabled = command.Enabled;
        }

        [JsonIgnore]
        public override HashSet<string> CommandTriggers
        {
            get
            {
                HashSet<string> commandsToCheck = this.Commands;
                if (this.IncludeExclamationInCommands)
                {
                    commandsToCheck = new HashSet<string>(commandsToCheck.Select(c => "!" + c));
                }
                return commandsToCheck;
            }
        }

        protected override SemaphoreSlim AsyncSemaphore { get { return ChatCommand.chatCommandPerformSemaphore; } }

        public bool DoesTextMatchCommand(string text, out IEnumerable<string> arguments)
        {
            arguments = null;
            foreach (string command in this.CommandTriggers)
            {
                Match match = Regex.Match(text, string.Format(CommandMatchingRegexFormat, command));
                if (match != null && match.Success)
                {
                    arguments = text.Substring(match.Index + match.Length).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    return true;
                }
            }
            return false;
        }
    }
}
