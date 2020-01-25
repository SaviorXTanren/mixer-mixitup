using Mixer.Base.Model.User;
using MixItUp.Base.Commands;
using MixItUp.Base.Model;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services.Mixer;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public enum EventTypeEnum
    {
        None = 0,

        // Platform-agnostic = 1

        [Name("Channel Stream Start")]
        ChannelStreamStart = 1,
        [Name("Channel Stream Stop")]
        ChannelStreamStop = 2,
        [Name("Channel Hosted")]
        ChannelHosted = 3,

        [Name("Channel Followed")]
        ChannelFollowed = 10,
        [Name("Channel Unfollowed")]
        ChannelUnfollowed = 11,

        [Name("Channel Subscribed")]
        ChannelSubscribed = 20,
        [Name("Channel Resubscribed")]
        ChannelResubscribed = 21,
        [Name("Channel Subscription Gifted")]
        ChannelSubscriptionGifted = 22,

        [Name("Chat New User Joined")]
        ChatUserFirstJoin = 50,
        [Name("Chat User Purged")]
        ChatUserPurge = 51,
        [Name("Chat User Banned")]
        ChatUserBan = 52,
        [Name("Chat Message Received")]
        ChatMessageReceived = 53,
        [Name("Chat User Joined")]
        ChatUserJoined = 54,
        [Name("Chat User Left")]
        ChatUserLeft = 55,
        [Name("Chat Message Deleted")]
        ChatMessageDeleted = 56,

        // Mixer = 100

        [Name("Mixer Channel Stream Start")]
        MixerChannelStreamStart = 100,
        [Name("Mixer Channel Stream Stop")]
        MixerChannelStreamStop = 101,
        [Name("Mixer Channel Hosted")]
        MixerChannelHosted = 102,

        [Name("Mixer Channel Followed")]
        MixerChannelFollowed = 110,
        [Name("Mixer Channel Unfollowed")]
        MixerChannelUnfollowed = 111,

        [Name("Mixer Channel Subscribed")]
        MixerChannelSubscribed = 120,
        [Name("Mixer Channel Resubscribed")]
        MixerChannelResubscribed = 121,
        [Name("Mixer Channel Subscription Gifted")]
        MixerChannelSubscriptionGifted = 122,

        [Name("Mixer Chat New User Joined")]
        MixerChatUserFirstJoin = 150,
        [Name("Mixer Chat User Purged")]
        MixerChatUserPurge = 151,
        [Name("Mixer Chat User Banned")]
        MixerChatUserBan = 152,
        [Name("Mixer Chat Message Received")]
        MixerChatMessageReceived = 153,
        [Name("Mixer Chat User Joined")]
        MixerChatUserJoined = 154,
        [Name("Mixer Chat User Left")]
        MixerChatUserLeft = 155,
        [Name("Mixer Chat Message Deleted")]
        MixerChatMessageDeleted = 156,

        [Name("Mixer Channel Sparks Spent")]
        MixerSparksUsed = 170,
        [Name("Mixer Channel Embers Spent")]
        MixerEmbersUsed = 171,
        [Name("Mixer Channel Skill Used")]
        MixerSkillUsed = 172,
        [Name("Mixer Channel Milestone Reached")]
        MixerMilestoneReached = 173,
        [Name("Mixer Channel Fan Progression Level-Up")]
        MixerFanProgressionLevelUp = 174,

        // 200

        // 300

        // External Services = 1000

        [Name("Streamlabs Donation")]
        StreamlabsDonation = 1000,
        [Name("GawkBox Donation")]
        GawkBoxDonation = 1010,
        [Name("Tiltify Donation")]
        TiltifyDonation = 1020,
        [Name("Extra Life Donation")]
        ExtraLifeDonation = 1030,
        [Name("TipeeeStream Donation")]
        TipeeeStreamDonation = 1040,
        [Name("TreatStream Donation")]
        TreatStreamDonation = 1050,
        [Name("Patreon Subscribed")]
        PatreonSubscribed = 1060,
        [Name("StreamJar Donation")]
        StreamJarDonation = 1070,
        [Name("JustGiving Donation")]
        JustGivingDonation = 1080,
        [Name("Streamloots Card Redeemed")]
        StreamlootsCardRedeemed = 1090,
        [Name("Streamloots Pack Purchased")]
        StreamlootsPackPurchased = 1091,
        [Name("Streamloots Pack Gifted")]
        StreamlootsPackGifted = 1092,
    }

    public class EventTrigger
    {
        public EventTypeEnum Type { get; set; }
        public StreamingPlatformTypeEnum Platform { get; set; } = StreamingPlatformTypeEnum.None;
        public UserViewModel User { get; set; }
        public List<string> Arguments { get; set; } = new List<string>();
        public Dictionary<string, string> SpecialIdentifiers { get; set; } = new Dictionary<string, string>();

        public EventTrigger(EventTypeEnum type)
        {
            this.Type = type;
        }

        public EventTrigger(EventTypeEnum type, UserViewModel user)
            : this(type)
        {
            this.User = user;
            this.Platform = this.User.Platform;
        }

        public EventTrigger(EventTypeEnum type, UserViewModel user, Dictionary<string, string> specialIdentifiers)
            : this(type, user)
        {
            this.SpecialIdentifiers = specialIdentifiers;
        }
    }

    public interface IEventService
    {
        IMixerEventService MixerEventService { get; }

        Task Initialize(IMixerEventService mixerEventService);

        EventCommand GetEventCommand(EventTypeEnum type);

        bool DoesCommandExist(EventTypeEnum type);

        bool CanPerformEvent(EventTrigger trigger);

        Task PerformEvent(EventTrigger trigger);
    }

    public class EventService : IEventService
    {
        public static Task<EventTrigger> ProcessDonationEvent(EventTypeEnum type, UserDonationModel donation, Dictionary<string, string> additionalSpecialIdentifiers = null)
        {
            GlobalEvents.DonationOccurred(donation);

            EventTrigger trigger = new EventTrigger(type, donation.User);
            trigger.User.Data.TotalAmountDonated += donation.Amount;

            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestDonationUserData] = trigger.User;
            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestDonationAmountData] = donation.AmountText;

            trigger.SpecialIdentifiers = donation.GetSpecialIdentifiers();
            if (additionalSpecialIdentifiers != null)
            {
                foreach (var kvp in additionalSpecialIdentifiers)
                {
                    trigger.SpecialIdentifiers[kvp.Key] = kvp.Value;
                }
            }

            return Task.FromResult(trigger);
        }

        public IMixerEventService MixerEventService { get; private set; }

        public Task Initialize(IMixerEventService mixerEventService)
        {
            this.MixerEventService = mixerEventService;
            return Task.FromResult(0);
        }

        public EventCommand GetEventCommand(EventTypeEnum type)
        {
            foreach (EventCommand command in ChannelSession.Settings.EventCommands)
            {
                if (command.EventCommandType == type)
                {
                    return command;
                }
            }
            return null;
        }

        public bool DoesCommandExist(EventTypeEnum type) { return this.GetEventCommand(type) != null; }

        public bool CanPerformEvent(EventTrigger trigger)
        {
            EventCommand command = this.GetEventCommand(trigger.Type);
            if (command != null)
            {
                return command.CanRun(trigger.User);
            }
            return false;
        }

        public async Task PerformEvent(EventTrigger trigger)
        {
            EventCommand command = this.GetEventCommand(trigger.Type);
            if (command != null)
            {
                if (trigger.User != null)
                {
                    await command.Perform(trigger.User, platform: trigger.Platform, arguments: trigger.Arguments, extraSpecialIdentifiers: trigger.SpecialIdentifiers);
                }
                else
                {
                    await command.Perform(await ChannelSession.GetCurrentUser(), platform: trigger.Platform, arguments: trigger.Arguments, extraSpecialIdentifiers: trigger.SpecialIdentifiers);
                }
            }
        }
    }
}
