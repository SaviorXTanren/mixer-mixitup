using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
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
        StreamEnd,
        StreamUpdated,
        Viewers,
        Chatters,
        UserJoined,
        UserLeft,
        ChatMessage,

        Donation = 3000,
        StreamlootsCardRedeemed,
        StreamlootsPackPurchased,
        StreamlootsPackGifted,
        CrowdControlEffect,

        TwitchFollow = 4000,
        TwitchRaid,
        TwitchSubscription,
        TwitchSubscriptionGifted,
        TwitchBits,
        TwitchChannelPoints,
        TwitchHypeTrainStart,
        TwitchHypeTrainLevelUp,
        TwitchHypeTrainEnd,

        YouTubeSubscriber = 5000,
        YouTubeMembership,
        YouTubeMembershipGifted,
        YouTubeSuperChat,
        YouTubeSuperSticker,

        TrovoFollow = 6000,
        TrovoRaid,
        TrovoSubscription,
        TrovoSubscriptionGifted,
        TrovoSpell,
        TrovoMagicChat,
    }

    [DataContract]
    public class StatisticModel
    {
        [DataMember]
        public string ID { get; set; }

        [DataMember]
        public DateTimeOffset DateTime { get; set; }

        [DataMember]
        public StatisticItemTypeEnum Type { get; set; }

        [DataMember]
        public JObject Data { get; set; }

        [Obsolete]
        public StatisticModel() { }

        public StatisticModel(StatisticItemTypeEnum type, JObject data)
        {
            this.ID = Guid.NewGuid().ToString();
            this.DateTime = DateTimeOffset.Now;
            this.Type = type;
            this.Data = data;
        }
    }

    public class StatisticsService
    {
        public const string AmountProperty = "Amount";
        public const string UserProperty = "User";

        public List<StatisticModel> CurrentSessionStatistics { get; private set; } = new List<StatisticModel>();

        private List<StatisticModel> statisticsToSave = new List<StatisticModel>();

        public StatisticsService() { }

        public Task Initialize()
        {
            return Task.CompletedTask;
        }

        public void LogEvent(StatisticItemTypeEnum type) { this.LogEvent(type, new JObject()); }

        public void LogEvent(StatisticItemTypeEnum type, double amount)
        {
            this.LogEvent(type, new JObject()
            {
                { AmountProperty, amount }
            });
        }

        public void LogEvent(StatisticItemTypeEnum type, UserV2ViewModel user)
        {
            this.LogEvent(type, new JObject()
            {
                { UserProperty, user.ID }
            });
        }

        public void LogEvent(StatisticItemTypeEnum type, JObject data)
        {
            StatisticModel statistic = new StatisticModel(type, data);
            this.CurrentSessionStatistics.Add(statistic);
            this.statisticsToSave.Add(statistic);
        }
    }
}