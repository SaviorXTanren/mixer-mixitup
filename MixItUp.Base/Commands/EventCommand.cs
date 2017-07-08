using Mixer.Base.Clients;
using Mixer.Base.Util;

namespace MixItUp.Base.Commands
{
    public class EventCommand : CommandBase
    {
        public ConstellationEventTypeEnum EventType { get; private set; }

        public EventCommand(string name, string description, ConstellationEventTypeEnum eventType)
            : base(name, "Event", EnumHelper.EnumToString(eventType), description)
        {
            this.EventType = eventType;
        }
    }
}
