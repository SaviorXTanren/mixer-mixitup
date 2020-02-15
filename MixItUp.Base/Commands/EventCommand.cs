using Mixer.Base.Clients;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.User;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Commands
{
    [Obsolete]
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

        [Name("Patreon Subscribed")]
        PatreonSubscribed = 5,

        [Name("Streamlabs Donation")]
        StreamlabsDonation = 10,
        [Name("GawkBox Donation")]
        GawkBoxDonation = 11,
        [Name("Tiltify Donation")]
        TiltifyDonation = 12,
        [Name("Extra Life Donation")]
        ExtraLifeDonation = 13,
        [Name("TipeeeStream Donation")]
        TipeeeStreamDonation = 14,
        [Name("TreatStream Donation")]
        TreatStreamDonation = 15,
        [Name("StreamJar Donation")]
        StreamJarDonation = 16,
        [Name("JustGiving Donation")]
        JustGivingDonation = 17,

        [Obsolete]
        [Name("Stream Tweet Retweet")]
        TwitterStreamTweetRetweet = 20,

        [Name("Chat New User Joined")]
        ChatUserFirstJoin = 30,
        [Name("Channel Unfollowed")]
        ChatUserUnfollow = 31,
        [Name("Chat User Purged")]
        ChatUserPurge = 32,
        [Name("Chat User Timed Out")]
        [Obsolete]
        ChatUserTimeout = 33,
        [Name("Chat User Banned")]
        ChatUserBan = 34,
        [Name("Chat Message Received")]
        ChatMessageReceived = 35,
        [Name("Chat User Joined")]
        ChatUserJoined = 36,
        [Name("Chat User Left")]
        ChatUserLeft = 37,
        [Name("Chat Message Deleted")]
        ChatMessageDeleted = 38,

        [Name("Channel Stream Start")]
        MixerChannelStreamStart = 40,
        [Name("Channel Stream Stop")]
        MixerChannelStreamStop = 41,

        [Name("Channel Milestone Reached")]
        MixerMilestoneReached = 50,
        [Name("Channel Skill Used")]
        MixerSkillUsed = 51,
        [Name("Channel Sparks Spent")]
        MixerSparksUsed = 52,
        [Name("Channel Embers Spent")]
        MixerEmbersUsed = 53,

        [Name("Streamloots Card Redeemed")]
        StreamlootsCardRedeemed = 60,
        [Name("Streamloots Pack Purchased")]
        StreamlootsPackPurchased = 61,
        [Name("Streamloots Pack Gifted")]
        StreamlootsPackGifted = 62,
    }

    public class EventCommand : CommandBase, IEquatable<EventCommand>
    {
        private static SemaphoreSlim eventCommandPerformSemaphore = new SemaphoreSlim(1);

        private static HashSet<EventTypeEnum> ignoreUserTracking = new HashSet<EventTypeEnum>()
        {
            EventTypeEnum.ChatUserPurge, EventTypeEnum.ChatMessageReceived, EventTypeEnum.ChatMessageDeleted,

            EventTypeEnum.MixerChannelSubscriptionGifted, EventTypeEnum.MixerSparksUsed, EventTypeEnum.MixerEmbersUsed, EventTypeEnum.MixerSkillUsed, EventTypeEnum.MixerMilestoneReached, EventTypeEnum.MixerFanProgressionLevelUp,
        };

        private LockedHashSet<Guid> userEventTracking = new LockedHashSet<Guid>();

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

        public bool CanRun(UserViewModel user)
        {
            if (EventCommand.ignoreUserTracking.Contains(this.EventCommandType))
            {
                return true;
            }
            return !this.userEventTracking.Contains(user.ID);
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

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers, CancellationToken token)
        {
            this.userEventTracking.Add(user.ID);

            await base.PerformInternal(user, arguments, extraSpecialIdentifiers, token);
        }
    }
}
