using MixItUp.Base.Services;
using System.Runtime.Serialization;
using System.Threading;

namespace MixItUp.Base.Model.Commands
{
    [DataContract]
    public class EventCommandModel : CommandModelBase
    {
        private static SemaphoreSlim commandLockSemaphore = new SemaphoreSlim(1);

        [DataMember]
        public EventTypeEnum EventType { get; set; }

        public EventCommandModel(EventTypeEnum eventType) : base(eventType.ToString(), CommandTypeEnum.Event) { this.EventType = eventType; }

        internal EventCommandModel(MixItUp.Base.Commands.EventCommand command)
            : base(command)
        {
            this.Name = command.EventCommandType.ToString();
            this.Type = CommandTypeEnum.Event;
        }

        protected override SemaphoreSlim CommandLockSemaphore { get { return EventCommandModel.commandLockSemaphore; } }
    }
}
