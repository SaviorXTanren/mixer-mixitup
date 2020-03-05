using Mixer.Base.Clients;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Runtime.Serialization;
using System.Threading;

namespace MixItUp.Base.Commands
{
    public class EventCommand : CommandBase, IEquatable<EventCommand>
    {
        private static SemaphoreSlim eventCommandPerformSemaphore = new SemaphoreSlim(1);

        [Obsolete]
        [DataMember]
        public ConstellationEventTypeEnum EventType { get; set; }
        [DataMember]
        public uint EventID { get; set; }

        [Obsolete]
        [DataMember]
        public OtherEventTypeEnum OtherEventType { get; set; }

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

    #region Obsolete Event Types

    [Obsolete]
    public enum OtherEventTypeEnum
    {
        None = 0,

        [Name("Donation Received")]
        [Obsolete]
        Donation,

        [Name("GameWisp Subscribed")]
        [Obsolete]
        GameWispSubscribed = 2,
        [Name("GameWisp Resubscribed")]
        [Obsolete]
        GameWispResubscribed = 3,

        [Name("Patreon Subscribed")]
        [Obsolete]
        PatreonSubscribed = 5,

        [Name("Streamlabs Donation")]
        [Obsolete]
        StreamlabsDonation = 10,
        [Name("GawkBox Donation")]
        [Obsolete]
        GawkBoxDonation = 11,
        [Name("Tiltify Donation")]
        [Obsolete]
        TiltifyDonation = 12,
        [Name("Extra Life Donation")]
        [Obsolete]
        ExtraLifeDonation = 13,
        [Name("TipeeeStream Donation")]
        [Obsolete]
        TipeeeStreamDonation = 14,
        [Name("TreatStream Donation")]
        [Obsolete]
        TreatStreamDonation = 15,
        [Name("StreamJar Donation")]
        [Obsolete]
        StreamJarDonation = 16,
        [Name("JustGiving Donation")]
        [Obsolete]
        JustGivingDonation = 17,

        [Name("Stream Tweet Retweet")]
        [Obsolete]
        TwitterStreamTweetRetweet = 20,

        [Name("Chat New User Joined")]
        [Obsolete]
        ChatUserFirstJoin = 30,
        [Name("Channel Unfollowed")]
        [Obsolete]
        ChatUserUnfollow = 31,
        [Name("Chat User Purged")]
        [Obsolete]
        ChatUserPurge = 32,
        [Name("Chat User Timed Out")]
        [Obsolete]
        ChatUserTimeout = 33,
        [Name("Chat User Banned")]
        [Obsolete]
        ChatUserBan = 34,
        [Name("Chat Message Received")]
        [Obsolete]
        ChatMessageReceived = 35,
        [Name("Chat User Joined")]
        [Obsolete]
        ChatUserJoined = 36,
        [Name("Chat User Left")]
        [Obsolete]
        ChatUserLeft = 37,
        [Name("Chat Message Deleted")]
        [Obsolete]
        ChatMessageDeleted = 38,
        [Name("Channel Stream Start")]
        [Obsolete]
        MixerChannelStreamStart = 40,
        [Name("Channel Stream Stop")]
        [Obsolete]
        MixerChannelStreamStop = 41,

        [Name("Channel Milestone Reached")]
        [Obsolete]
        MixerMilestoneReached = 50,
        [Name("Channel Skill Used")]
        [Obsolete]
        MixerSkillUsed = 51,
        [Name("Channel Sparks Spent")]
        [Obsolete]
        MixerSparksUsed = 52,
        [Name("Channel Embers Spent")]
        [Obsolete]
        MixerEmbersUsed = 53,

        [Name("Streamloots Card Redeemed")]
        [Obsolete]
        StreamlootsCardRedeemed = 60,
        [Name("Streamloots Pack Purchased")]
        [Obsolete]
        StreamlootsPackPurchased = 61,
        [Name("Streamloots Pack Gifted")]
        [Obsolete]
        StreamlootsPackGifted = 62,
    }

    #endregion Obsolete Event Types
}
