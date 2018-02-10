using Mixer.Base.Clients;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Constellation;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Runtime.Serialization;
using System.Threading;

namespace MixItUp.Base.Commands
{
    public class EventCommand : CommandBase, IEquatable<EventCommand>
    {
        private static SemaphoreSlim eventCommandPerformSemaphore = new SemaphoreSlim(1);

        [DataMember]
        public ConstellationEventTypeEnum EventType { get; set; }

        [DataMember]
        public uint EventID { get; set; }

        public EventCommand() { }

        public EventCommand(ConstellationEventTypeEnum type) : this(type, 0, string.Empty) { }

        public EventCommand(ConstellationEventTypeEnum type, ChannelAdvancedModel channel) : this(type, channel.id, channel.user.username) { }

        public EventCommand(ConstellationEventTypeEnum type, UserModel user) : this(type, user.id, user.username) { }

        public EventCommand(ConstellationEventTypeEnum type, uint id, string name)
            : base(EnumHelper.GetEnumName(type), CommandTypeEnum.Event, name)
        {
            this.EventType = type;
            this.EventID = id;
        }

        public string UniqueEventID { get { return this.GetEventType().ToString(); } }

        public ConstellationEventType GetEventType() { return new ConstellationEventType(this.EventType, this.EventID); }

        public bool MatchesEvent(string eventID) { return this.UniqueEventID.Equals(eventID); }

        public override string ToString() { return this.CommandsString; }

        public override bool Equals(object obj)
        {
            if (obj is EventCommand)
            {
                return this.Equals((EventCommand)obj);
            }
            return false;
        }

        public bool Equals(EventCommand other) { return this.EventType.Equals(other.EventType) && this.EventID.Equals(other.EventID); }

        public override int GetHashCode() { return this.GetEventType().GetHashCode(); }

        protected override SemaphoreSlim AsyncSemaphore { get { return EventCommand.eventCommandPerformSemaphore; } }
    }
}
