using Mixer.Base.ViewModel.Chat;
using System.Runtime.Serialization;

namespace MixItUp.Base.Commands
{
    [DataContract]
    public class ChatCommand : CommandBase
    {
        [DataMember]
        public UserRole LowestAllowedRole { get; set; }

        public ChatCommand() { }

        public ChatCommand(string name, string command, UserRole lowestAllowedRole)
            : base(name, CommandTypeEnum.Chat, command)
        {
            this.LowestAllowedRole = lowestAllowedRole;
        }
    }
}
