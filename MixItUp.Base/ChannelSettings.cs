using Mixer.Base.Model.Channel;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base
{
    [DataContract]
    public class ChannelSettings
    {
        public const string ChannelSettingsFileName = "ChannelSettings.xml";

        public ChannelSettings()
        {
            this.UserData = new List<UserDataViewModel>();
            this.ChatCommands = new List<ChatCommand>();
            this.InteractiveCommands = new List<InteractiveCommand>();
            this.EventCommands = new List<EventCommand>();
            this.TimerCommands = new List<TimerCommand>();
        }

        [DataMember]
        public ChannelModel Channel { get; set; }

        [DataMember]
        public List<UserDataViewModel> UserData { get; set; }

        [DataMember]
        public List<ChatCommand> ChatCommands { get; set; }

        [DataMember]
        public List<InteractiveCommand> InteractiveCommands { get; set; }

        [DataMember]
        public List<EventCommand> EventCommands { get; set; }

        [DataMember]
        public List<TimerCommand> TimerCommands { get; set; }
    }
}
