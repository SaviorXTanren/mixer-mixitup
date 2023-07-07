using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services.Mock;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat.Trovo;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public enum StatisticItemTypeEnum
    {
        [Obsolete]
        None = 0,

        Command = 1000,
        Action,

        StreamStart = 2000,
        StreamStop,
        StreamUpdated,
        Viewers,
        Chatters,
        UserJoined,
        UserLeft,
        ChatMessage,
        Follow,
        Raid,
        Subscription,
        Resubscription,
        SubscriptionGifted,

        TwitchBits = 3000,
        TwitchChannelPoints,
        TwitchHypeTrainStart,
        TwitchHypeTrainLevelUp,
        TwitchHypeTrainEnd,

        YouTubeSuperChat = 4000,

        TrovoSpell = 5000,
        TrovoMagicChat,

        Donation = 10000,
        StreamlootsCardRedeemed,
        StreamlootsPackPurchased,
        StreamlootsPackGifted,
        CrowdControlEffect,
    }

    [DataContract]
    public class StatisticModel
    {
        public const string AmountProperty = "Amount";
        public const string DescriptionProperty = "Description";

        [DataMember]
        public string ID { get; set; }

        [DataMember]
        public DateTimeOffset DateTime { get; set; }

        [DataMember]
        public StatisticItemTypeEnum Type { get; set; }

        [DataMember]
        public Guid UserID { get; set; }

        [DataMember]
        public StreamingPlatformTypeEnum Platform { get; set; }

        [DataMember]
        public JObject Data { get; set; }

        [Obsolete]
        public StatisticModel() { }

        [JsonIgnore]
        public int AmountInt
        {
            get
            {
                if (this.Data.TryGetValue(AmountProperty, out JToken value) && value != null)
                {
                    return (int)value;
                }
                return 0;
            }
        }

        [JsonIgnore]
        public double AmountDouble
        {
            get
            {
                if (this.Data.TryGetValue(AmountProperty, out JToken value) && value != null)
                {
                    return (double)value;
                }
                return 0.0;
            }
        }

        [JsonIgnore]
        public string Description
        {
            get
            {
                if (this.Data.TryGetValue(DescriptionProperty, out JToken value) && value != null)
                {
                    return value.ToString();
                }
                return string.Empty;
            }
        }

        public StatisticModel(StatisticItemTypeEnum type, UserV2ViewModel user, StreamingPlatformTypeEnum platform, JObject data, DateTimeOffset? dateTime = null)
        {
            this.ID = Guid.NewGuid().ToString();
            this.DateTime = (dateTime != null) ? dateTime.GetValueOrDefault() : DateTimeOffset.Now;
            this.Type = type;
            this.UserID = (user != null) ? user.ID : Guid.Empty;
            this.Platform = platform;
            this.Data = data;
        }
    }

    public class StatisticsService
    {
        private List<StatisticModel> sessionStatistics = new List<StatisticModel>();
        private List<StatisticModel> statisticsToSave = new List<StatisticModel>();

        public StatisticsService() { }

        public Task Initialize()
        {
            if (ChannelSession.IsDebug())
            {
                (new MockSessionService()).AddMockViewerStatistics();
            }

            return Task.CompletedTask;
        }

        public void LogEventStatistic(EventTypeEnum eventType, CommandParametersModel parameters)
        {
            try
            {
                switch (eventType)
                {
                    case EventTypeEnum.ChatUserJoined:
                        this.LogStatistic(StatisticItemTypeEnum.UserJoined, parameters.User, parameters.User.Platform); break;
                    case EventTypeEnum.ChatUserLeft:
                        this.LogStatistic(StatisticItemTypeEnum.UserLeft, parameters.User, parameters.User.Platform); break;
                    case EventTypeEnum.ChatMessageReceived:
                        this.LogStatistic(StatisticItemTypeEnum.ChatMessage, parameters.User, parameters.User.Platform); break;

                    case EventTypeEnum.TwitchChannelStreamStart:
                        this.LogStatistic(StatisticItemTypeEnum.StreamStart, platform: StreamingPlatformTypeEnum.Twitch); break;
                    case EventTypeEnum.TwitchChannelStreamStop:
                        this.LogStatistic(StatisticItemTypeEnum.StreamStop, platform: StreamingPlatformTypeEnum.Twitch); break;
                    case EventTypeEnum.TwitchChannelRaided:
                        this.LogStatistic(StatisticItemTypeEnum.Raid, parameters.User, StreamingPlatformTypeEnum.Twitch, this.GetAmountValue(parameters.SpecialIdentifiers, "raidviewercount")); break;
                    case EventTypeEnum.TwitchChannelFollowed:
                        this.LogStatistic(StatisticItemTypeEnum.Follow, parameters.User, StreamingPlatformTypeEnum.Twitch); break;
                    case EventTypeEnum.TwitchChannelSubscribed:
                        this.LogStatistic(StatisticItemTypeEnum.Subscription, parameters.User, StreamingPlatformTypeEnum.Twitch); break;
                    case EventTypeEnum.TwitchChannelResubscribed:
                        this.LogStatistic(StatisticItemTypeEnum.Resubscription, parameters.User, StreamingPlatformTypeEnum.Twitch, this.GetAmountValue(parameters.SpecialIdentifiers, "usersubmonths")); break;
                    case EventTypeEnum.TwitchChannelSubscriptionGifted:
                        this.LogStatistic(StatisticItemTypeEnum.SubscriptionGifted, parameters.User, StreamingPlatformTypeEnum.Twitch, 1); break;
                    case EventTypeEnum.TwitchChannelMassSubscriptionsGifted:
                        this.LogStatistic(StatisticItemTypeEnum.SubscriptionGifted, parameters.User, StreamingPlatformTypeEnum.Twitch, this.GetAmountValue(parameters.SpecialIdentifiers, "subsgiftedamount")); break;
                    case EventTypeEnum.TwitchChannelBitsCheered:
                        this.LogStatistic(StatisticItemTypeEnum.TwitchBits, parameters.User, StreamingPlatformTypeEnum.Twitch, this.GetAmountValue(parameters.SpecialIdentifiers, "bitscheered")); break;
                    case EventTypeEnum.TwitchChannelPointsRedeemed:
                        this.LogStatistic(StatisticItemTypeEnum.TwitchChannelPoints, parameters.User, StreamingPlatformTypeEnum.Twitch, this.GetAmountValue(parameters.SpecialIdentifiers, "rewardcost"), parameters.SpecialIdentifiers["rewardname"]); break;
                    case EventTypeEnum.TwitchChannelHypeTrainBegin:
                        this.LogStatistic(StatisticItemTypeEnum.TwitchHypeTrainStart, platform: StreamingPlatformTypeEnum.Twitch); break;
                    case EventTypeEnum.TwitchChannelHypeTrainEnd:
                        this.LogStatistic(StatisticItemTypeEnum.TwitchHypeTrainLevelUp, platform: StreamingPlatformTypeEnum.Twitch, amount: this.GetAmountValue(parameters.SpecialIdentifiers, "hypetrainlevel")); break;
                    case EventTypeEnum.TwitchChannelHypeTrainLevelUp:
                        this.LogStatistic(StatisticItemTypeEnum.TwitchHypeTrainEnd, platform: StreamingPlatformTypeEnum.Twitch); break;

                    case EventTypeEnum.YouTubeChannelStreamStart:
                        this.LogStatistic(StatisticItemTypeEnum.StreamStart, platform: StreamingPlatformTypeEnum.YouTube); break;
                    case EventTypeEnum.YouTubeChannelStreamStop:
                        this.LogStatistic(StatisticItemTypeEnum.StreamStop, platform: StreamingPlatformTypeEnum.YouTube); break;
                    case EventTypeEnum.YouTubeChannelNewMember:
                        this.LogStatistic(StatisticItemTypeEnum.Subscription, parameters.User, StreamingPlatformTypeEnum.YouTube); break;
                    case EventTypeEnum.YouTubeChannelMemberMilestone:
                        this.LogStatistic(StatisticItemTypeEnum.Resubscription, parameters.User, StreamingPlatformTypeEnum.YouTube, this.GetAmountValue(parameters.SpecialIdentifiers, "usersubmonths")); break;
                    case EventTypeEnum.YouTubeChannelMembershipGifted:
                        this.LogStatistic(StatisticItemTypeEnum.SubscriptionGifted, parameters.User, StreamingPlatformTypeEnum.YouTube, 1); break;
                    case EventTypeEnum.YouTubeChannelMassMembershipGifted:
                        this.LogStatistic(StatisticItemTypeEnum.SubscriptionGifted, parameters.User, StreamingPlatformTypeEnum.YouTube, this.GetAmountValue(parameters.SpecialIdentifiers, "subsgiftedamount")); break;
                    case EventTypeEnum.YouTubeChannelSuperChat:
                        this.LogStatistic(StatisticItemTypeEnum.YouTubeSuperChat, parameters.User, StreamingPlatformTypeEnum.YouTube, this.GetAmountValue(parameters.SpecialIdentifiers, "amountnumber")); break;

                    case EventTypeEnum.TrovoChannelStreamStart:
                        this.LogStatistic(StatisticItemTypeEnum.StreamStart, platform: StreamingPlatformTypeEnum.Trovo); break;
                    case EventTypeEnum.TrovoChannelStreamStop:
                        this.LogStatistic(StatisticItemTypeEnum.StreamStop, platform: StreamingPlatformTypeEnum.Trovo); break;
                    case EventTypeEnum.TrovoChannelRaided:
                        this.LogStatistic(StatisticItemTypeEnum.Raid, parameters.User, StreamingPlatformTypeEnum.Trovo, this.GetAmountValue(parameters.SpecialIdentifiers, "raidviewercount")); break;
                    case EventTypeEnum.TrovoChannelFollowed:
                        this.LogStatistic(StatisticItemTypeEnum.Follow, parameters.User, StreamingPlatformTypeEnum.Trovo); break;
                    case EventTypeEnum.TrovoChannelSubscribed:
                        this.LogStatistic(StatisticItemTypeEnum.Subscription, parameters.User, StreamingPlatformTypeEnum.Trovo); break;
                    case EventTypeEnum.TrovoChannelResubscribed:
                        this.LogStatistic(StatisticItemTypeEnum.Resubscription, parameters.User, StreamingPlatformTypeEnum.Trovo, this.GetAmountValue(parameters.SpecialIdentifiers, "usersubmonths")); break;
                    case EventTypeEnum.TrovoChannelSubscriptionGifted:
                        this.LogStatistic(StatisticItemTypeEnum.SubscriptionGifted, parameters.User, StreamingPlatformTypeEnum.Trovo, 1); break;
                    case EventTypeEnum.TrovoChannelMassSubscriptionsGifted:
                        this.LogStatistic(StatisticItemTypeEnum.SubscriptionGifted, parameters.User, StreamingPlatformTypeEnum.Trovo, this.GetAmountValue(parameters.SpecialIdentifiers, "subsgiftedamount")); break;
                    case EventTypeEnum.TrovoChannelSpellCast:
                        this.LogStatistic(StatisticItemTypeEnum.TrovoSpell, parameters.User, StreamingPlatformTypeEnum.Trovo, this.GetAmountValue(parameters.SpecialIdentifiers, TrovoChatSpellViewModel.SpellTotalValueSpecialIdentifier),
                            parameters.SpecialIdentifiers[TrovoChatSpellViewModel.SpellValueTypeSpecialIdentifier]); break;
                    case EventTypeEnum.TrovoChannelMagicChat:
                        this.LogStatistic(StatisticItemTypeEnum.TrovoSpell, parameters.User, StreamingPlatformTypeEnum.Trovo); break;

                    case EventTypeEnum.StreamlabsDonation:
                    case EventTypeEnum.TiltifyDonation:
                    case EventTypeEnum.ExtraLifeDonation:
                    case EventTypeEnum.TipeeeStreamDonation:
                    case EventTypeEnum.TreatStreamDonation:
                    case EventTypeEnum.PatreonSubscribed:
                    case EventTypeEnum.RainmakerDonation:
                    case EventTypeEnum.JustGivingDonation:
                    case EventTypeEnum.StreamElementsDonation:
                    case EventTypeEnum.StreamElementsMerchPurchase:
                        this.LogStatistic(StatisticItemTypeEnum.Donation, parameters.User, amount: this.GetAmountValue(parameters.SpecialIdentifiers, SpecialIdentifierStringBuilder.DonationAmountNumberSpecialIdentifier),
                            description: parameters.SpecialIdentifiers[SpecialIdentifierStringBuilder.DonationSourceSpecialIdentifier]); break;

                    case EventTypeEnum.StreamlootsCardRedeemed:
                        this.LogStatistic(StatisticItemTypeEnum.StreamlootsCardRedeemed, parameters.User, description: parameters.SpecialIdentifiers["streamlootscardname"]); break;
                    case EventTypeEnum.StreamlootsPackPurchased:
                        this.LogStatistic(StatisticItemTypeEnum.StreamlootsPackPurchased, parameters.User, amount: this.GetAmountValue(parameters.SpecialIdentifiers, "streamlootspurchasequantity")); break;
                    case EventTypeEnum.StreamlootsPackGifted:
                        this.LogStatistic(StatisticItemTypeEnum.StreamlootsPackGifted, parameters.User, amount: this.GetAmountValue(parameters.SpecialIdentifiers, "streamlootspurchasequantity")); break;
                    case EventTypeEnum.CrowdControlEffectRedeemed:
                        this.LogStatistic(StatisticItemTypeEnum.CrowdControlEffect, parameters.User, description: parameters.SpecialIdentifiers["crowdcontroleffectname"]); break;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public void LogStatistic(StatisticItemTypeEnum statisticType, UserV2ViewModel user = null, StreamingPlatformTypeEnum platform = StreamingPlatformTypeEnum.None, double amount = 0.0, string description = null, DateTimeOffset? dateTime = null)
        {
            JObject data = new JObject();
            if (amount != 0.0)
            {
                data[StatisticModel.AmountProperty] = amount;
            }
            if (!string.IsNullOrEmpty(description))
            {
                data[StatisticModel.DescriptionProperty] = description;
            }

            StatisticModel statistic = new StatisticModel(statisticType, user, platform, data, dateTime);
            this.sessionStatistics.Add(statistic);

            lock (this.statisticsToSave)
            {
                this.statisticsToSave.Add(statistic);
            }
        }

        public IEnumerable<StatisticModel> GetCurrentSessionStatistics() { return this.sessionStatistics.ToList(); }

        public IEnumerable<StatisticModel> GetStatisticsToSave()
        {
            IEnumerable<StatisticModel> results;
            lock (this.statisticsToSave)
            {
                results = this.statisticsToSave.ToList();
                this.statisticsToSave.Clear();
            }
            return results;
        }

        private double GetAmountValue(Dictionary<string, string> specialIdentifiers, string key)
        {
            if (specialIdentifiers.TryGetValue(key, out string value) && double.TryParse(value, out double result))
            {
                return result;
            }
            return 0.0;
        }
    }
}