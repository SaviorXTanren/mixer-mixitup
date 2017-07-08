using Mixer.Base.Clients;

namespace MixItUp.Base.Commands
{
    public class EventCommand : CommandBase
    {
        public ConstellationEventTypeEnum EventType { get; private set; }

        public EventCommand(string name, ConstellationEventTypeEnum eventType)
            : base(name)
        {
            this.EventType = eventType;
        }
    }
}
