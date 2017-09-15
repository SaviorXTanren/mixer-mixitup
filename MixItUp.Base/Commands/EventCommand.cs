using Mixer.Base.Clients;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.Actions;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Commands
{
    public class EventCommand : CommandBase, IEquatable<EventCommand>
    {
        public static IEnumerable<ActionTypeEnum> AllowedActions
        {
            get
            {
                return new List<ActionTypeEnum>()
                {
                    ActionTypeEnum.Chat, ActionTypeEnum.Currency, ActionTypeEnum.ExternalProgram, ActionTypeEnum.Input, ActionTypeEnum.Overlay, ActionTypeEnum.Sound, ActionTypeEnum.Wait
                };
            }
        }

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

        public override string ToString() { return this.CommandsString; }

        public override bool Equals(object obj)
        {
            if (obj is EventCommand)
            {
                return this.Equals((EventCommand)obj);
            }
            return false;
        }

        public bool Equals(EventCommand other) { return this.Type.Equals(other.Type) && this.EventID.Equals(other.EventID); }

        public override int GetHashCode() { return this.GetEventType().GetHashCode(); }
    }
}
