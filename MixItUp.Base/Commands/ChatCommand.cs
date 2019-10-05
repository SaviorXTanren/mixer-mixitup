using MixItUp.Base.Model.Import;
using MixItUp.Base.ViewModel.Requirement;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;

namespace MixItUp.Base.Commands
{
    [DataContract]
    public class ChatCommand : PermissionsCommandBase
    {
        public const string CommandWildcardMatchingRegexFormat = "\\s?{0}\\s?";

        private static SemaphoreSlim chatCommandPerformSemaphore = new SemaphoreSlim(1);

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
        public override string CommandsString
        {
            get
            {
                if (this.Commands.Any(s => s.Contains(" ")))
                {
                    if (this.Commands.Count > 1)
                    {
                        return string.Join(";", this.Commands);
                    }
                    return this.Commands.First() + ";";
                }
                return base.CommandsString;
            }
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

        public override bool DoesTextMatchCommand(string text, out IEnumerable<string> arguments)
        {
            if (this.Wildcards)
            {
                return this.DoesTextMatchCommand(text, ChatCommand.CommandWildcardMatchingRegexFormat, out arguments);
            }
            else
            {
                return base.DoesTextMatchCommand(text, out arguments);
            }
        }

        protected override SemaphoreSlim AsyncSemaphore { get { return ChatCommand.chatCommandPerformSemaphore; } }
    }
}
