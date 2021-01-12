using MixItUp.Base.Model.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;

namespace MixItUp.Base.Commands
{
    [Obsolete]
    [DataContract]
    public class PreMadeChatCommandSettings
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public bool IsEnabled { get; set; }
        [DataMember]
        public UserRoleEnum Permissions { get; set; }
        [DataMember]
        public int Cooldown { get; set; }

        public PreMadeChatCommandSettings() { }
    }

    [Obsolete]
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
                return CommandBase.DoesTextMatchCommand(text, ChatCommand.CommandWildcardMatchingRegexFormat, this.CommandTriggers, out arguments);
            }
            else
            {
                return base.DoesTextMatchCommand(text, out arguments);
            }
        }

        protected override SemaphoreSlim AsyncSemaphore { get { return ChatCommand.chatCommandPerformSemaphore; } }
    }

    [Obsolete]
    public class NewAutoChatCommand
    {
        public bool AddCommand { get; set; }
        public string Description { get; set; }
        public ChatCommand Command { get; set; }

        public NewAutoChatCommand(string description, ChatCommand command)
        {
            this.AddCommand = true;
            this.Description = description;
            this.Command = command;
        }
    }
}
