using Mixer.Base.Util;
using Mixer.Base.ViewModel.Chat;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Commands
{
    [DataContract]
    public class ChatCommand : CommandBase
    {
        [DataMember]
        public UserRole LowestAllowedRole { get; set; }

        public ChatCommand() { }

        public ChatCommand(string name, List<string> commands, UserRole lowestAllowedRole)
            : base(name, CommandTypeEnum.Chat, commands)
        {
            this.LowestAllowedRole = lowestAllowedRole;
        }

        [JsonIgnore]
        public string LowestAllowedRoleString { get { return EnumHelper.GetEnumName(this.LowestAllowedRole); } }
    }
}
