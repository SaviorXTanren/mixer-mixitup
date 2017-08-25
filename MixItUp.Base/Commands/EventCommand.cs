using Mixer.Base.Util;
using MixItUp.Base.ViewModel;

namespace MixItUp.Base.Commands
{
    public class EventCommand : CommandBase
    {
        private const string EventCommandStringFormat = "{0} - {1}";

        public SubscribedEventViewModel SubscribedEvent { get; set; }

        public EventCommand() { }

        public EventCommand(string name, SubscribedEventViewModel subscribedEvent)
            : base(name, CommandTypeEnum.Event, string.Format(EventCommandStringFormat, EnumHelper.GetEnumName(subscribedEvent.Type), subscribedEvent.Name))
        {
            this.SubscribedEvent = subscribedEvent;
        }
    }
}
