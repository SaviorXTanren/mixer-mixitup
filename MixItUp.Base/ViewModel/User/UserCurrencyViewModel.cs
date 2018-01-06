using Mixer.Base.Clients;
using MixItUp.Base.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel.User
{
    [DataContract]
    public class UserCurrencyViewModel : IEquatable<UserCurrencyViewModel>
    {
        [JsonIgnore]
        public static ConstellationEventType ChannelFollowEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__followed, ChannelSession.Channel.id); } }
        [JsonIgnore]
        public static ConstellationEventType ChannelHostedEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__hosted, ChannelSession.Channel.id); } }
        [JsonIgnore]
        public static ConstellationEventType ChannelSubscribedEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__subscribed, ChannelSession.Channel.id); } }
        [JsonIgnore]
        public static ConstellationEventType ChannelResubscribedEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__resubscribed, ChannelSession.Channel.id); } }
        [JsonIgnore]
        public static ConstellationEventType ChannelResubscribedSharedEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__resubShared, ChannelSession.Channel.id); } }

        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int AcquireAmount { get; set; }
        [DataMember]
        public int AcquireInterval { get; set; }
        [DataMember]
        public string SpecialIdentifier { get; set; }

        [DataMember]
        public int SubscriberBonus { get; set; }

        [DataMember]
        public int OnFollowBonus { get; set; }
        [DataMember]
        public int OnHostBonus { get; set; }
        [DataMember]
        public int OnSubscribeBonus { get; set; }

        [DataMember]
        public string ResetInterval { get; set; }
        [DataMember]
        public DateTimeOffset LastReset { get; set; }

        [DataMember]
        public List<UserRankViewModel> Ranks { get; set; }
        [DataMember]
        public CustomCommand RankChangedCommand { get; set; }

        [DataMember]
        public bool Enabled { get; set; }

        [JsonIgnore]
        public bool IsRank { get { return this.Ranks.Count > 0; } }

        public UserCurrencyViewModel()
        {
            this.SpecialIdentifier = string.Empty;
            this.ResetInterval = "Never";
            this.LastReset = DateTimeOffset.MinValue;
            this.Ranks = new List<UserRankViewModel>();
        }

        public string SpecialIdentifierName { get { return string.Format("{0}name", this.SpecialIdentifier); } }

        public string SpecialIdentifierRank { get { return string.Format("{0}rank", this.SpecialIdentifier); } }

        public bool ShouldBeReset()
        {
            if (this.Enabled && !this.ResetInterval.Equals("Never"))
            {
                DateTimeOffset newResetDate = DateTimeOffset.MinValue;
                switch (this.ResetInterval)
                {
                    case "Daily":
                        newResetDate = this.LastReset.AddDays(1);
                        break;
                    case "Weekly":
                        newResetDate = this.LastReset.AddDays(7);
                        break;
                    case "Monthly":
                        newResetDate = this.LastReset.AddMonths(1);
                        break;
                    case "Yearly":
                        newResetDate = this.LastReset.AddYears(1);
                        break;
                }
                return (newResetDate < DateTimeOffset.Now);
            }
            return false;
        }

        public UserRankViewModel GetRankForPoints(int points)
        {
            UserRankViewModel rank = null;
            if (this.Ranks.Count > 0)
            {
                rank = this.Ranks.Where(r => r.MinimumPoints <= points).OrderByDescending(r => r.MinimumPoints).FirstOrDefault();
            }
            return rank;
        }

        public override bool Equals(object obj)
        {
            if (obj is UserCurrencyViewModel)
            {
                return this.Equals((UserCurrencyViewModel)obj);
            }
            return false;
        }

        public bool Equals(UserCurrencyViewModel other)
        {
            return (!string.IsNullOrEmpty(this.Name)) ? this.Name.Equals(other.Name) : false;
        }

        public override int GetHashCode()
        {
            return (!string.IsNullOrEmpty(this.Name)) ? this.Name.GetHashCode() : string.Empty.GetHashCode();
        }
    }
}
