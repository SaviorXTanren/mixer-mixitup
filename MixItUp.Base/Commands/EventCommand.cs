using Mixer.Base.Clients;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using System;
using System.Runtime.Serialization;
using System.Threading;

namespace MixItUp.Base.Commands
{
    public enum OtherEventTypeEnum
    {
        None = 0,

        [Name("Donation Received")]
        [Obsolete]
        Donation,

        [Name("GameWisp Subscribed")]
        GameWispSubscribed = 2,
        [Name("GameWisp Resubscribed")]
        GameWispResubscribed = 3,

        [Name("Streamlabs Donation")]
        StreamlabsDonation = 10,
        [Name("GawkBox Donation")]
        GawkBoxDonation = 11,
        [Name("Tiltify Donation")]
        TiltifyDonation = 12,

        [Name("User First Joined")]
        MixerUserFirstJoin = 30,
        [Name("User Unfollowed")]
        MixerUserUnfollow = 31,
        [Name("User Purged")]
        MixerUserPurge = 32,
        [Name("User Timed Out")]
        [Obsolete]
        MixerUserTimeout = 33,
        [Name("User Banned")]
        MixerUserBan = 34,

        [Name("Channel Stream Start")]
        MixerChannelStreamStart = 40,
        [Name("Channel Stream Stop")]
        MixerChannelStreamStop = 41,
    }

    public class EventCommand : CommandBase, IEquatable<EventCommand>
    {
        private static SemaphoreSlim eventCommandPerformSemaphore = new SemaphoreSlim(1);

        [DataMember]
        public ConstellationEventTypeEnum EventType { get; set; }
        [DataMember]
        public uint EventID { get; set; }

        [DataMember]
        public OtherEventTypeEnum OtherEventType { get; set; }

        public EventCommand()
        {
            this.OtherEventType = OtherEventTypeEnum.None;
        }

        public EventCommand(ConstellationEventTypeEnum type) : this(type, 0, string.Empty) { }

        public EventCommand(ConstellationEventTypeEnum type, ChannelAdvancedModel channel) : this(type, channel.id, channel.user.username) { }

        public EventCommand(ConstellationEventTypeEnum type, UserModel user) : this(type, user.id, user.username) { }

        public EventCommand(ConstellationEventTypeEnum type, uint id, string name)
            : base(EnumHelper.GetEnumName(type), CommandTypeEnum.Event, name)
        {
            this.EventType = type;
            this.EventID = id;
        }

        public EventCommand(OtherEventTypeEnum otherEventType, string name)
            : base(EnumHelper.GetEnumName(otherEventType), CommandTypeEnum.Event, name)
        {
            this.OtherEventType = otherEventType;
        }

        public bool IsOtherEventType { get { return this.OtherEventType != OtherEventTypeEnum.None; } }

        public string UniqueEventID
        {
            get
            {
                if (this.IsOtherEventType)
                {
                    return EnumHelper.GetEnumName(this.OtherEventType);
                }
                return this.GetEventType().ToString();
            }
        }

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

        public bool Equals(EventCommand other)
        {
            if (this.IsOtherEventType)
            {
                return this.OtherEventType == other.OtherEventType;
            }
            return this.EventType.Equals(other.EventType) && this.EventID.Equals(other.EventID);
        }

        public override int GetHashCode() { return this.GetEventType().GetHashCode(); }

        protected override SemaphoreSlim AsyncSemaphore { get { return EventCommand.eventCommandPerformSemaphore; } }
    }
}
