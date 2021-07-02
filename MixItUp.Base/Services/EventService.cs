using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public enum EventTypeEnum
    {
        None = 0,

        // Platform-agnostic = 1

        //ChannelStreamStart = 1,
        //ChannelStreamStop = 2,
        //ChannelHosted = 3,

        //ChannelFollowed = 10,
        //ChannelUnfollowed = 11,

        //ChannelSubscribed = 20,
        //ChannelResubscribed = 21,
        //ChannelSubscriptionGifted = 22,

        ChatUserFirstJoin = 50,
        ChatUserPurge = 51,
        ChatUserBan = 52,
        ChatMessageReceived = 53,
        ChatUserJoined = 54,
        ChatUserLeft = 55,
        ChatMessageDeleted = 56,
        ChatUserTimeout = 57,
        ChatWhisperReceived = 58,

        // Mixer = 100

        [Obsolete]
        MixerChannelStreamStart = 100,
        [Obsolete]
        MixerChannelStreamStop = 101,
        [Obsolete]
        MixerChannelHosted = 102,

        [Obsolete]
        MixerChannelFollowed = 110,
        [Obsolete]
        MixerChannelUnfollowed = 111,

        [Obsolete]
        MixerChannelSubscribed = 120,
        [Obsolete]
        MixerChannelResubscribed = 121,
        [Obsolete]
        MixerChannelSubscriptionGifted = 122,

        //MixerChatUserFirstJoin = 150,
        //MixerChatUserPurge = 151,
        //MixerChatUserBan = 152,
        //MixerChatMessageReceived = 153,
        //MixerChatUserJoined = 154,
        //MixerChatUserLeft = 155,
        //MixerChatMessageDeleted = 156,
        //MixerChatUserTimeout = 156,

        [Obsolete]
        MixerChannelSparksUsed = 170,
        [Obsolete]
        MixerChannelEmbersUsed = 171,
        [Obsolete]
        MixerChannelSkillUsed = 172,
        [Obsolete]
        MixerChannelMilestoneReached = 173,
        [Obsolete]
        MixerChannelFanProgressionLevelUp = 174,

        // Twitch = 200

        TwitchChannelStreamStart = 200,
        TwitchChannelStreamStop = 201,
        TwitchChannelHosted = 202,
        TwitchChannelRaided = 203,

        TwitchChannelFollowed = 210,
        [Obsolete]
        TwitchChannelUnfollowed = 211,

        TwitchChannelSubscribed = 220,
        TwitchChannelResubscribed = 221,
        TwitchChannelSubscriptionGifted = 222,
        TwitchChannelMassSubscriptionsGifted = 223,

        //TwitchChatUserFirstJoin = 250,
        //TwitchChatUserPurge = 251,
        //TwitchChatUserBan = 252,
        //TwitchChatMessageReceived = 253,
        //TwitchChatUserJoined = 254,
        //TwitchChatUserLeft = 255,
        //TwitchChatMessageDeleted = 256,

        TwitchChannelBitsCheered = 270,
        TwitchChannelPointsRedeemed = 271,

        TwitchChannelHypeTrainBegin = 280,
        [Obsolete]
        TwitchChannelHypeTrainProgress = 281,
        TwitchChannelHypeTrainEnd = 282,

        // 300
        // 300 = YouTube

        // 400 = Trovo

        [Name("Trovo Channel Raided")]
        TrovoChannelRaided = 203,

        [Name("Trovo Channel Followed")]
        TrovoChannelFollowed = 210,

        [Name("Trovo Channel Subscribed")]
        TrovoChannelSubscribed = 220,
        [Name("Trovo Channel Resubscribed")]
        TrovoChannelResubscribed = 221,
        [Name("Trovo Channel Subscription Gifted")]
        TrovoChannelSubscriptionGifted = 222,
        [Name("Trovo Channel Mass Subscriptions Gifted")]
        TrovoChannelMassSubscriptionsGifted = 223,

        // 500 = Glimesh

        [Name("Glimesh Channel Stream Start")]
        GlimeshChannelStreamStart = 500,
        [Name("Glimesh Channel Stream Stop")]
        GlimeshChannelStreamStop = 501,

        [Name("Glimesh Channel Followed")]
        GlimeshChannelFollowed = 510,

        // External Services = 1000

        StreamlabsDonation = 1000,
        TiltifyDonation = 1020,
        ExtraLifeDonation = 1030,
        TipeeeStreamDonation = 1040,
        TreatStreamDonation = 1050,
        PatreonSubscribed = 1060,
        RainmakerDonation = 1070,
        JustGivingDonation = 1080,
        StreamlootsCardRedeemed = 1090,
        StreamlootsPackPurchased = 1091,
        StreamlootsPackGifted = 1092,
        StreamElementsDonation = 1100,
        StreamElementsMerchPurchase = 1101,
    }

    public class EventService
    {
        private static HashSet<EventTypeEnum> singleUseTracking = new HashSet<EventTypeEnum>()
        {
            EventTypeEnum.ChatUserFirstJoin, EventTypeEnum.ChatUserJoined, EventTypeEnum.ChatUserLeft,

            EventTypeEnum.TwitchChannelStreamStart, EventTypeEnum.TwitchChannelStreamStop, EventTypeEnum.TwitchChannelFollowed, EventTypeEnum.TwitchChannelUnfollowed, EventTypeEnum.TwitchChannelHosted, EventTypeEnum.TwitchChannelRaided, EventTypeEnum.TwitchChannelSubscribed, EventTypeEnum.TwitchChannelResubscribed,

            EventTypeEnum.TrovoChannelFollowed,EventTypeEnum.TrovoChannelRaided, EventTypeEnum.TrovoChannelSubscribed, EventTypeEnum.TrovoChannelResubscribed,

            EventTypeEnum.GlimeshChannelStreamStart, EventTypeEnum.GlimeshChannelStreamStop, EventTypeEnum.GlimeshChannelFollowed,
        };

        private LockedDictionary<EventTypeEnum, HashSet<Guid>> userEventTracking = new LockedDictionary<EventTypeEnum, HashSet<Guid>>();

        public EventService()
        {
            foreach (EventTypeEnum type in singleUseTracking)
            {
                this.userEventTracking[type] = new HashSet<Guid>();
            }
        }

        public static async Task ProcessDonationEvent(EventTypeEnum type, UserDonationModel donation, List<string> arguments = null, Dictionary<string, string> additionalSpecialIdentifiers = null)
        {
            CommandParametersModel parameters = new CommandParametersModel(donation.User, donation.Platform, arguments, donation.GetSpecialIdentifiers());

            if (additionalSpecialIdentifiers != null)
            {
                foreach (var kvp in additionalSpecialIdentifiers)
                {
                    parameters.SpecialIdentifiers[kvp.Key] = kvp.Value;
                }
            }

            foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
            {
                if (parameters.User.HasPermissionsTo(streamPass.Permission))
                {
                    streamPass.AddAmount(donation.User.Data, (int)Math.Ceiling(streamPass.DonationBonus * donation.Amount));
                }
            }

            parameters.User.Data.TotalAmountDonated += donation.Amount;
            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestDonationUserData] = parameters.User.ID;
            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestDonationAmountData] = donation.AmountText;

            await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.All, parameters.User, string.Format("{0} Donated {1}", parameters.User.FullDisplayName, donation.AmountText), ChannelSession.Settings.AlertDonationColor));

            await ServiceManager.Get<EventService>().PerformEvent(type, parameters);

            try
            {
                GlobalEvents.DonationOccurred(donation);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public Task Initialize()
        {
            return Task.FromResult(0);
        }

        public EventCommandModel GetEventCommand(EventTypeEnum type)
        {
            foreach (EventCommandModel command in ServiceManager.Get<CommandService>().EventCommands.ToList())
            {
                if (command.EventType == type)
                {
                    return command;
                }
            }
            return null;
        }

        public bool CanPerformEvent(EventTypeEnum type, CommandParametersModel parameters)
        {
            UserViewModel user = (parameters.User != null) ? parameters.User : ChannelSession.GetCurrentUser();
            if (EventService.singleUseTracking.Contains(type) && this.userEventTracking.ContainsKey(type))
            {
                return !this.userEventTracking[type].Contains(user.ID);
            }
            return true;
        }

        public async Task PerformEvent(EventTypeEnum type, CommandParametersModel parameters)
        {
            if (this.CanPerformEvent(type, parameters))
            {
                UserViewModel user = parameters.User;
                if (user == null)
                {
                    user = ChannelSession.GetCurrentUser();
                }

                if (this.userEventTracking.ContainsKey(type))
                {
                    lock (this.userEventTracking)
                    {
                        this.userEventTracking[type].Add(user.ID);
                    }
                }

                await ServiceManager.Get<UserService>().AddOrUpdateActiveUser(user);
                user.UpdateLastActivity();

                EventCommandModel command = this.GetEventCommand(type);
                if (command != null)
                {
                    Logger.Log(LogLevel.Debug, $"Performing event trigger: {type}");

                    await ServiceManager.Get<CommandService>().Queue(command, parameters);
                }
            }
        }
    }
}