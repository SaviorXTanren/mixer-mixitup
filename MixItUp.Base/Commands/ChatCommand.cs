using Mixer.Base.Util;
using MixItUp.Base.ViewModel;
using MixItUp.Base.ViewModel.Chat;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Commands
{
    [DataContract]
    public class ChatCommand : CommandBase
    {
        public static IEnumerable<string> PermissionsAllowedValues
        {
            get
            {
                List<string> roles = EnumHelper.GetEnumNames<UserRole>().ToList();
                roles.Remove(EnumHelper.GetEnumName<UserRole>(UserRole.Banned));
                return roles;
            }
        }

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
        }
    }
}
