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
            : base(EnumHelper.GetEnumName(eventType), CommandTypeEnum.Event)
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

        protected override Task<bool> PerformPreChecks(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            return Task.FromResult(this.CanRun(user));
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers, CancellationToken token)
        {
            this.userEventTracking.Add(user.ID);

            await base.PerformInternal(user, arguments, extraSpecialIdentifiers, token);
        }
    }
}