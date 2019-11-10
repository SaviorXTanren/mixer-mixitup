using Mixer.Base.Clients;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.User;
using MixItUp.Base.Model.User;
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
        private static LockedDictionary<string, LockedHashSet<uint>> userEventTracking = new LockedDictionary<string, LockedHashSet<uint>>();

        public static bool CanUserRunEvent(UserViewModel user, string eventType)
        {
            if ((!EventCommand.userEventTracking.ContainsKey(eventType) || !EventCommand.userEventTracking[eventType].Contains(user.ID)))
            {
                if (!EventCommand.userEventTracking.ContainsKey(eventType))
                {
                    EventCommand.userEventTracking[eventType] = new LockedHashSet<uint>();
                }

                if (user != null)
                {
                    EventCommand.userEventTracking[eventType].Add(user.ID);
                }
                return true;
            }
            return false;
        }

        public static EventCommand FindEventCommand(string eventDetails)
        {
            foreach (EventCommand command in ChannelSession.Settings.EventCommands)
            {
                if (command.MatchesEvent(eventDetails))
                {
                    return command;
                }
            }
            return null;
        }

        public static async Task RunEventCommand(EventCommand command, UserViewModel user, IEnumerable<string> arguments = null, Dictionary<string, string> extraSpecialIdentifiers = null)
        {
            if (user != null)
            {
                await command.Perform(user, arguments: arguments, extraSpecialIdentifiers: extraSpecialIdentifiers);
            }
            else
            {
                await command.Perform(await ChannelSession.GetCurrentUser(), arguments: arguments, extraSpecialIdentifiers: extraSpecialIdentifiers);
            }
        }

        public static async Task FindAndRunEventCommand(string eventDetails, UserViewModel user, IEnumerable<string> arguments = null, Dictionary<string, string> extraSpecialIdentifiers = null)
        {
            EventCommand command = EventCommand.FindEventCommand(eventDetails);
            if (command != null)
            {
                await EventCommand.RunEventCommand(command, user, arguments, extraSpecialIdentifiers);
            }
        }

        public static async Task ProcessDonationEventCommand(UserDonationModel donation, OtherEventTypeEnum eventType, Dictionary<string, string> additionalSpecialIdentifiers = null)
        {
            GlobalEvents.DonationOccurred(donation);

            UserViewModel user = new UserViewModel(0, donation.UserName);

            UserModel userModel = await ChannelSession.MixerStreamerConnection.GetUser(user.UserName);
            if (userModel != null)
            {
                user = new UserViewModel(userModel);
            }

            user.Data.TotalAmountDonated += donation.Amount;

            Dictionary<string, string> specialIdentifiers = donation.GetSpecialIdentifiers();
            if (additionalSpecialIdentifiers != null)
            {
                foreach (var kvp in additionalSpecialIdentifiers)
                {
                    specialIdentifiers[kvp.Key] = kvp.Value;
                }
            }

            await EventCommand.FindAndRunEventCommand(EnumHelper.GetEnumName(eventType), user, arguments: null, extraSpecialIdentifiers: specialIdentifiers);
        }

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

        public EventCommand(ConstellationEventTypeEnum type, ChannelAdvancedModel channel) : this(type, channel.id, channel.user.id.ToString()) { }

        public EventCommand(ConstellationEventTypeEnum type, UserModel user) : this(type, user.id, user.id.ToString()) { }

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
