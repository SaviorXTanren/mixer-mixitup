using Mixer.Base.Util;
using MixItUp.Base.Actions;
using MixItUp.Base.ViewModel.Chat;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Commands
{
    [DataContract]
    public class ChatCommand : CommandBase
    {
        public static IEnumerable<ActionTypeEnum> AllowedActions
        {
            get
            {
                return new List<ActionTypeEnum>()
                {
                    ActionTypeEnum.Chat, ActionTypeEnum.Currency, ActionTypeEnum.ExternalProgram, ActionTypeEnum.Input, ActionTypeEnum.Overlay, ActionTypeEnum.Sound, ActionTypeEnum.Wait, ActionTypeEnum.OBSStudio
                };
            }
        }

        [DataMember]
        public UserRole LowestAllowedRole { get; set; }

        public ChatCommand() { }

        public ChatCommand(string name, string command, UserRole lowestAllowedRole) : this(name, new List<string>() { command }, lowestAllowedRole) { }

        public ChatCommand(string name, List<string> commands, UserRole lowestAllowedRole)
            : base(name, CommandTypeEnum.Chat, commands)
        {
            this.LowestAllowedRole = lowestAllowedRole;
        }

        [JsonIgnore]
        public string LowestAllowedRoleString { get { return EnumHelper.GetEnumName(this.LowestAllowedRole); } }
    }
}
