using MixItUp.Base.Actions;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Commands
{
    [DataContract]
    public class ChatCommand : CommandBase
    {
        [DataMember]
        public string Description { get; set; }

        public ChatCommand() { }

        public ChatCommand(string name, string command, IEnumerable<ActionBase> actions, string description)
            : base(name, CommandTypeEnum.Chat, command, actions)
        {
            this.Description = description;
        }
    }
}
