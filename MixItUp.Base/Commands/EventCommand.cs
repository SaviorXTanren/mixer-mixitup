using MixItUp.Base.Services;
using System;
using System.Runtime.Serialization;
using System.Threading;

namespace MixItUp.Base.Commands
{
    public class EventCommand : CommandBase, IEquatable<EventCommand>
    {
        private static SemaphoreSlim eventCommandPerformSemaphore = new SemaphoreSlim(1);

        [DataMember]
        public EventTypeEnum EventCommandType { get; set; }

        public EventCommand() { }

        public EventCommand(EventTypeEnum eventType)
            : base(eventType.ToString(), CommandTypeEnum.Event, eventType.ToString())
        {
            this.EventCommandType = eventType;
        }

        public override bool Equals(object obj)
        {
            if (obj is EventCommand)
            {
                return this.Equals((EventCommand)obj);
            }
            return false;
        }

        public bool Equals(EventCommand other) { return this.EventCommandType == other.EventCommandType; }

        public override int GetHashCode() { return this.EventCommandType.GetHashCode(); }

        protected override SemaphoreSlim AsyncSemaphore { get { return EventCommand.eventCommandPerformSemaphore; } }
    }
}
