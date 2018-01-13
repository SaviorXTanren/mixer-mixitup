using MixItUp.Base.Actions;
using MixItUp.Base.ViewModel.Import;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;

namespace MixItUp.Base.Commands
{
    [DataContract]
    public class BasicChatCommand : ChatCommand
    {
        public BasicChatCommand() { }

        public BasicChatCommand(string command, UserRole lowestAllowedRole, int cooldown)
            : this(new List<string>() { command }, lowestAllowedRole, cooldown)
        { }

        public BasicChatCommand(IEnumerable<string> commands, UserRole lowestAllowedRole, int cooldown)
            : base(commands.First(), commands, lowestAllowedRole, cooldown, null)
        { }

        public BasicChatCommand(ScorpBotCommand command)
            : this(command.Command, command.Permission, command.Cooldown)
        {
            this.Actions.Add(new ChatAction(command.Text, isWhisper: false, sendAsStreamer: false));
            this.IsEnabled = command.Enabled;
        }
    }

    [DataContract]
    public class ChatCommand : PermissionsCommandBase
    {
        private static SemaphoreSlim chatCommandPerformSemaphore = new SemaphoreSlim(1);

        [Obsolete]
        [DataMember]
        public int CurrencyCost { get; set; }

        public ChatCommand() { }

        public ChatCommand(string name, string command, UserRole lowestAllowedRole, int cooldown, UserCurrencyRequirementViewModel currencyRequirement)
            : this(name, new List<string>() { command }, lowestAllowedRole, cooldown, currencyRequirement)
        { }

        public ChatCommand(string name, IEnumerable<string> commands, UserRole lowestAllowedRole, int cooldown, UserCurrencyRequirementViewModel currencyRequirement)
            : base(name, CommandTypeEnum.Chat, commands, lowestAllowedRole, cooldown, currencyRequirement, null)
        { }

        public ChatCommand(ScorpBotCommand command)
            : this(command.Command, command.Command, command.Permission, command.Cooldown, null)
        {
            this.Actions.Add(new ChatAction(command.Text, isWhisper: false, sendAsStreamer: false));
            this.IsEnabled = command.Enabled;
        }

        protected override SemaphoreSlim AsyncSemaphore { get { return ChatCommand.chatCommandPerformSemaphore; } }
    }
}
