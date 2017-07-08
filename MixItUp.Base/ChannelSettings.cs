using Mixer.Base.Model.Channel;
using MixItUp.Base.Commands;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base
{
    [DataContract]
    public class ChannelSettings
    {
        public ChannelSettings()
        {
            this.ChatCommands = new List<ChatCommand>();
            this.InteractiveCommands = new List<InteractiveCommand>();
            this.EventCommands = new List<EventCommand>();
        }

        [DataMember]
        public ChannelModel Channel { get; set; }

        [DataMember]
        public List<ChatCommand> ChatCommands { get; set; }
        
        [DataMember]
        public List<InteractiveCommand> InteractiveCommands { get; set; }

        [DataMember]
        public List<EventCommand> EventCommands { get; set; }
    }
}
