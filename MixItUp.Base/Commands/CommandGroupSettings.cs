using System.Runtime.Serialization;

namespace MixItUp.Base.Commands
{
    [DataContract]
    public class CommandGroupSettings
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public bool IsMinimized { get; set; }

        [DataMember]
        public int TimerInterval { get; set; }

        public CommandGroupSettings() { }

        public CommandGroupSettings(string name) { this.Name = name; }
    }
}
