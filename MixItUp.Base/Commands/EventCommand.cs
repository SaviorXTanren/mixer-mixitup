using Mixer.Base.Clients;
using MixItUp.Base.Actions;
using System.Collections.Generic;

namespace MixItUp.Base.Commands
{
    public class EventCommand : CommandBase
    {
        public ConstellationEventTypeEnum EventType { get; set; }

        public EventCommand() { }

        public EventCommand(string name, string command, IEnumerable<ActionBase> actions, ConstellationEventTypeEnum eventType)
            : base(name, CommandTypeEnum.Event, command, actions)
        {
            this.EventType = eventType;
        }
    }
}
