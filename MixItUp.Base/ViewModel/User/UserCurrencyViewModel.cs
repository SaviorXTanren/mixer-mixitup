using Mixer.Base.Clients;
using MixItUp.Base.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel.User
{
    public enum CurrencyResetRateEnum
    {
        Never,
        Yearly,
        Monthly,
        Weekly,
        Daily,
    }

    [DataContract]
    public class UserCurrencyViewModel : IEquatable<UserCurrencyViewModel>
    {
        public static UserRankViewModel NoRank = new UserRankViewModel("No Rank", 0);

        public static ConstellationEventType ChannelFollowEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__followed, ChannelSession.Channel.id); } }
        public static ConstellationEventType ChannelHostedEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__hosted, ChannelSession.Channel.id); } }
        public static ConstellationEventType ChannelSubscribedEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__subscribed, ChannelSession.Channel.id); } }
        public static ConstellationEventType ChannelResubscribedEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__resubscribed, ChannelSession.Channel.id); } }
        public static ConstellationEventType ChannelResubscribedSharedEvent { get { return new ConstellationEventType(ConstellationEventTypeEnum.channel__id__resubShared, ChannelSession.Channel.id); } }

        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int AcquireAmount { get; set; }
        [DataMember]
        public int AcquireInterval { get; set; }
        [DataMember]
        public int MaxAmount { get; set; }
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
        public CurrencyResetRateEnum ResetInterval { get; set; }
        [DataMember]
        public DateTimeOffset LastReset { get; set; }

        [DataMember]
        public List<UserRankViewModel> Ranks { get; set; }
        [DataMember]
        public CustomCommand RankChangedCommand { get; set; }

        [JsonIgnore]
        public bool IsRank { get { return this.Ranks.Count > 0; } }

        public UserCurrencyViewModel()
        {
            this.ID = Guid.NewGuid();
            this.MaxAmount = int.MaxValue;
            this.SpecialIdentifier = string.Empty;
            this.ResetInterval = CurrencyResetRateEnum.Never;
            this.LastReset = DateTimeOffset.MinValue;
            this.Ranks = new List<UserRankViewModel>();
        }

        [JsonIgnore]
        public bool IsActive { get { return this.AcquireAmount != 0 && this.AcquireInterval != 0; } }

        [JsonIgnore]
        public bool IsMinutesInterval { get { return this.AcquireAmount == 1 && this.AcquireInterval == 1; } }

        [JsonIgnore]
        public bool IsHoursInterval { get { return this.AcquireAmount == 1 && this.AcquireInterval == 60; } }

        [JsonIgnore]
        public string UserAmountSpecialIdentifier { get { return string.Format("user{0}", this.SpecialIdentifier); } }

        [JsonIgnore]
        public string UserRankNameSpecialIdentifier { get { return string.Format("user{0}rank", this.SpecialIdentifier); } }

        public bool ShouldBeReset()
        {
            if (!this.ResetInterval.Equals("Never"))
            {
                DateTimeOffset newResetDate = DateTimeOffset.MinValue;

                if (this.ResetInterval == CurrencyResetRateEnum.Daily) { newResetDate = this.LastReset.AddDays(1); }
                else if (this.ResetInterval == CurrencyResetRateEnum.Weekly) { newResetDate = this.LastReset.AddDays(7); }
                else if (this.ResetInterval == CurrencyResetRateEnum.Monthly) { newResetDate = this.LastReset.AddMonths(1); }
                else if (this.ResetInterval == CurrencyResetRateEnum.Yearly) { newResetDate = this.LastReset.AddYears(1); }

                return (newResetDate < DateTimeOffset.Now);
            }
            return false;
        }

        public UserRankViewModel GetRankForPoints(int points)
        {
            UserRankViewModel rank = UserCurrencyViewModel.NoRank;
            if (this.Ranks.Count > 0)
            {
                rank = this.Ranks.Where(r => r.MinimumPoints <= points).OrderByDescending(r => r.MinimumPoints).FirstOrDefault();
            }
            return rank;
        }

        public void Reset()
        {
            foreach (var kvp in ChannelSession.Settings.UserData.ToDictionary())
            {
                kvp.Value.ResetCurrency(this);
            }
            this.LastReset = new DateTimeOffset(DateTimeOffset.Now.Date);
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
