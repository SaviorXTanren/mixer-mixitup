using Mixer.Base.Clients;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.Commands;
using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel
{
    [DataContract]
    public class SubscribedEventViewModel : IEquatable<SubscribedEventViewModel>
    {
        [DataMember]
        public ConstellationEventTypeEnum Type { get; set; }

        [DataMember]
        public uint ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public EventCommand Command { get; set; }

        public SubscribedEventViewModel(ConstellationEventTypeEnum type, ChannelAdvancedModel channel) : this(type, channel.id, channel.user.username) { }

        public SubscribedEventViewModel(ConstellationEventTypeEnum type, UserModel user) : this(type, user.id, user.username) { }

        public SubscribedEventViewModel(ConstellationEventTypeEnum type, uint id, string name)
        {
            this.Type = type;
            this.ID = id;
            this.Name = name;
        }

        public SubscribedEventViewModel() { }

        public string UniqueEventID { get { return this.GetEventType().ToString(); } }

        public ConstellationEventType GetEventType() { return new ConstellationEventType(this.Type, this.ID); }

        public override string ToString() { return EnumHelper.GetEnumName(this.Type) + " - " + this.Name; }

        public override bool Equals(object obj)
        {
            if (obj is SubscribedEventViewModel)
            {
                return this.Equals((SubscribedEventViewModel)obj);
            }
            return false;
        }

        public bool Equals(SubscribedEventViewModel other) { return this.Type.Equals(other.Type) && this.ID.Equals(other.ID); }

        public override int GetHashCode() { return this.GetEventType().GetHashCode(); }
    }
}
