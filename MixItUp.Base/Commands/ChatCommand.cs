using MixItUp.Base.Actions;
using MixItUp.Base.ViewModel.Import;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;

namespace MixItUp.Base.Commands
{
    [DataContract]
    public class ChatCommand : PermissionsCommandBase
    {
        private static SemaphoreSlim chatCommandPerformSemaphore = new SemaphoreSlim(1);

        [Obsolete]
        [DataMember]
        public int CurrencyCost { get; set; }

        public ChatCommand() { }

        public ChatCommand(string name, string command, int cooldown, RequirementViewModel requirements)
            : this(name, new List<string>() { command }, cooldown, requirements)
        { }

        public ChatCommand(string name, IEnumerable<string> commands, int cooldown, RequirementViewModel requirements)
            : base(name, CommandTypeEnum.Chat, commands, cooldown, requirements)
        { }

        public ChatCommand(ScorpBotCommand command)
            : this(command.Command, command.Command, command.Cooldown, command.Requirements)
        {
            this.Actions.Add(new ChatAction(command.Text));
            this.IsEnabled = command.Enabled;
        }

        protected override SemaphoreSlim AsyncSemaphore { get { return ChatCommand.chatCommandPerformSemaphore; } }
    }
}
