using System.Runtime.Serialization;

namespace MixItUp.Base.Commands
{
    [DataContract]
    public class CommandGroupSettings
    {
        public string Name { get; set; }

        public int TimerInterval { get; set; }
        
        public CommandGroupSettings(string name) { this.Name = name; }
    }
}
