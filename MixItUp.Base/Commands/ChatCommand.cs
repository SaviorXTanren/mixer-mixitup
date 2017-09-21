using Mixer.Base.Util;
using MixItUp.Base.Actions;
using MixItUp.Base.ViewModel.Chat;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MixItUp.Base.ViewModel;
using System.Threading.Tasks;
using System;
using System.Linq;

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

        public static IEnumerable<ActionTypeEnum> AllowedActions
        {
            get
            {
                return new List<ActionTypeEnum>()
                {
                    ActionTypeEnum.Chat, ActionTypeEnum.Currency, ActionTypeEnum.ExternalProgram, ActionTypeEnum.Input, ActionTypeEnum.Overlay,
                    ActionTypeEnum.Sound, ActionTypeEnum.Wait, ActionTypeEnum.OBSStudio, ActionTypeEnum.XSplit
                };
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
