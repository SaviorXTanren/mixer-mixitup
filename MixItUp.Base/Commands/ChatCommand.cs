using Mixer.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Commands
{
    [DataContract]
    public class ChatCommand : CommandBase
    {
        private static SemaphoreSlim chatCommandPerformSemaphore = new SemaphoreSlim(1);

        public static IEnumerable<string> PermissionsAllowedValues { get { return EnumHelper.GetEnumNames(UserViewModel.SelectableUserRoles()); } }

        [DataMember]
        public UserRole Permissions { get; set; }

        [DataMember]
        public int Cooldown { get; set; }

        [JsonIgnore]
        private DateTimeOffset lastRun = DateTimeOffset.MinValue;

        public ChatCommand() { }

        public ChatCommand(string name, string command, UserRole lowestAllowedRole, int cooldown) : this(name, new List<string>() { command }, lowestAllowedRole, cooldown) { }

        public ChatCommand(string name, List<string> commands, UserRole lowestAllowedRole, int cooldown)
            : base(name, CommandTypeEnum.Chat, commands)
        {
            this.Permissions = lowestAllowedRole;
            this.Cooldown = cooldown;
        }

        [JsonIgnore]
        public string PermissionsString { get { return EnumHelper.GetEnumName(this.Permissions); } }

        public override async Task Perform(UserViewModel user, IEnumerable<string> arguments = null)
        {
            if (this.lastRun.AddSeconds(this.Cooldown) < DateTimeOffset.Now)
            {
                this.lastRun = DateTimeOffset.Now;
                await base.Perform(user, arguments);
            }
            else if (ChannelSession.BotChat != null)
            {
                TimeSpan timeLeft = this.lastRun.AddSeconds(this.Cooldown) - DateTimeOffset.Now;
                await ChannelSession.BotChat.Whisper(user.UserName, string.Format("This command is currently on cooldown, please wait another {0} second(s).", (int)timeLeft.TotalSeconds));
            }
        }

        protected override SemaphoreSlim AsyncSempahore { get { return ChatCommand.chatCommandPerformSemaphore; } }
    }
}
