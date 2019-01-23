using System.Runtime.Serialization;

namespace MixItUp.Base.Commands
{
    [DataContract]
    public class CommandGroupSettings
    {
        public string Name { get; set; }

        public int TimerInterval { get; set; }
    }
}
