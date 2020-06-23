using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Currency
{
    public enum CurrencyResetRateEnum
    {
        Never,
        Yearly,
        Monthly,
        Weekly,
        Daily,
    }

    public enum CurrencySpecialTrackingEnum
    {
        None = 0,
        [Obsolete]
        Sparks = 1,
        [Obsolete]
        Embers = 2,
        [Obsolete]
        FanProgression = 3,
        Bits = 4,
    }

    [DataContract]
    public class RankModel : IEquatable<RankModel>, IComparable<RankModel>, IComparable, IComparer<RankModel>
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Amount { get; set; }

        public RankModel() { }

        public RankModel(string name, int amount)
        {
            this.Name = name;
            this.Amount = amount;
        }

        public override bool Equals(object other)
        {
            if (other is RankModel)
            {
                return this.Equals((RankModel)other);
            }
            return false;
        }

        public bool Equals(RankModel other) { return string.Equals(this.Name, other.Name) && this.Amount == other.Amount; }

        public override int GetHashCode() { return this.Name.GetHashCode() + this.Amount.GetHashCode(); }

        public int CompareTo(object obj)
        {
            if (obj is RankModel)
            {
                return this.CompareTo((RankModel)obj);
            }
            return 0;
        }

        public int CompareTo(RankModel other)
        {
            if (this.Amount < other.Amount) { return -1; }
            else if (this.Amount > other.Amount) { return 1; }
            return 0;
        }

        public int Compare(RankModel x, RankModel y) { return x.CompareTo(y); }
    }

    [DataContract]
    public class CurrencyModel : IEquatable<CurrencyModel>
    {
        public static RankModel NoRank = new RankModel("No Rank", 0);

        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public CurrencySpecialTrackingEnum SpecialTracking { get; set; }

        [DataMember]
        public int AcquireAmount { get; set; }
        [DataMember]
        public int AcquireInterval { get; set; }
        [DataMember]
        public int MinimumActiveRate { get; set; }

        [DataMember]
        public int OfflineAcquireAmount { get; set; }
        [DataMember]
        public int OfflineAcquireInterval { get; set; }

        [DataMember]
        public int MaxAmount { get; set; }

        [DataMember]
        public string SpecialIdentifier { get; set; }

        [DataMember]
        public int RegularBonus { get; set; }
        [DataMember]
        public int SubscriberBonus { get; set; }
        [DataMember]
        public int ModeratorBonus { get; set; }

        [DataMember]
        public int OnFollowBonus { get; set; }
        [DataMember]
        public int OnHostBonus { get; set; }
        [DataMember]
        public int OnSubscribeBonus { get; set; }

        [DataMember]
        public CurrencyResetRateEnum ResetInterval { get; set; }
        [DataMember]
        public DateTimeOffset ResetStartCadence { get; set; }
        [DataMember]
        public DateTimeOffset LastReset { get; set; }

        [DataMember]
        public List<RankModel> Ranks { get; set; } = new List<RankModel>();
        [DataMember]
        public Guid RankChangedCommandID { get; set; }

        [DataMember]
        public bool IsPrimary { get; set; }

        public CurrencyModel()
        {
            this.ID = Guid.NewGuid();
            this.MinimumActiveRate = 0;
            this.MaxAmount = int.MaxValue;
            this.SpecialIdentifier = string.Empty;
            this.ResetInterval = CurrencyResetRateEnum.Never;
            this.LastReset = DateTimeOffset.MinValue;
        }

        [JsonIgnore]
        public bool IsActive { get { return !(this.IsOnlineIntervalDisabled && this.IsOfflineIntervalDisabled); } }

        [JsonIgnore]
        public bool IsRank { get { return this.Ranks.Count > 0; } }

        [JsonIgnore]
        public bool IsOnlineIntervalMinutes { get { return this.AcquireAmount == 1 && this.AcquireInterval == 1; } }

        [JsonIgnore]
        public bool IsOnlineIntervalHours { get { return this.AcquireAmount == 1 && this.AcquireInterval == 60; } }

        [JsonIgnore]
        public bool IsOnlineIntervalDisabled { get { return this.AcquireAmount == 0 && this.AcquireInterval == 0; } }

        [JsonIgnore]
        public bool IsOfflineIntervalMinutes { get { return this.OfflineAcquireAmount == 1 && this.OfflineAcquireInterval == 1; } }

        [JsonIgnore]
        public bool IsOfflineIntervalHours { get { return this.OfflineAcquireAmount == 1 && this.OfflineAcquireInterval == 60; } }

        [JsonIgnore]
        public bool IsOfflineIntervalDisabled { get { return this.OfflineAcquireAmount == 0 && this.OfflineAcquireInterval == 0; } }

        [JsonIgnore]
        public bool HasMinimumActiveRate { get { return this.MinimumActiveRate > 0; } }

        [JsonIgnore]
        public string UserAmountSpecialIdentifier { get { return string.Format("{0}{1}", SpecialIdentifierStringBuilder.UserSpecialIdentifierHeader, this.SpecialIdentifier); } }

        [JsonIgnore]
        public string UserAmountDisplaySpecialIdentifier { get { return string.Format("{0}display", this.UserAmountSpecialIdentifier); } }

        [JsonIgnore]
        public string UserPositionSpecialIdentifier { get { return string.Format("{0}position", this.UserAmountSpecialIdentifier); } }

        [JsonIgnore]
        public string UserRankNameSpecialIdentifier { get { return string.Format("{0}rank", this.UserAmountSpecialIdentifier); } }

        [JsonIgnore]
        public string UserAmountNextSpecialIdentifier { get { return string.Format("{0}next", this.UserAmountSpecialIdentifier); } }

        [JsonIgnore]
        public string UserAmountNextDisplaySpecialIdentifier { get { return string.Format("{0}display", this.UserAmountNextSpecialIdentifier); } }

        [JsonIgnore]
        public string UserRankNextNameSpecialIdentifier { get { return string.Format("{0}nextrank", this.UserAmountSpecialIdentifier); } }

        [JsonIgnore]
        public string TopRegexSpecialIdentifier { get { return string.Format("{0}\\d+{1}", SpecialIdentifierStringBuilder.TopSpecialIdentifierHeader, this.SpecialIdentifier); } }

        [JsonIgnore]
        public string Top10SpecialIdentifier { get { return string.Format("{0}10{1}", SpecialIdentifierStringBuilder.TopSpecialIdentifierHeader, this.SpecialIdentifier); } }

        [JsonIgnore]
        public string TopSpecialIdentifier { get { return string.Format("{0}{1}", SpecialIdentifierStringBuilder.TopSpecialIdentifierHeader, this.SpecialIdentifier); } }

        [JsonIgnore]
        public string TopUserSpecialIdentifier { get { return string.Format("{0}{1}", this.TopSpecialIdentifier, SpecialIdentifierStringBuilder.UserSpecialIdentifierHeader); } }

        [JsonIgnore]
        public CustomCommand RankChangedCommand
        {
            get { return ChannelSession.Settings.GetCustomCommand(this.RankChangedCommandID); }
            set
            {
                if (value != null)
                {
                    this.RankChangedCommandID = value.ID;
                    ChannelSession.Settings.SetCustomCommand(value);
                }
                else
                {
                    ChannelSession.Settings.CustomCommands.Remove(this.RankChangedCommandID);
                    this.RankChangedCommandID = Guid.Empty;
                }
            }
        }

        public int GetAmount(UserDataModel user)
        {
            if (user.CurrencyAmounts.ContainsKey(this.ID))
            {
                return user.CurrencyAmounts[this.ID];
            }
            return 0;
        }

        public bool HasAmount(UserDataModel user, int amount)
        {
            return (user.IsCurrencyRankExempt || this.GetAmount(user) >= amount);
        }

        public void SetAmount(UserDataModel user, int amount)
        {
            user.CurrencyAmounts[this.ID] = Math.Min(Math.Max(amount, 0), this.MaxAmount);
            if (ChannelSession.Settings != null)
            {
                ChannelSession.Settings.UserData.ManualValueChanged(user.ID);
            }
        }

        public void AddAmount(UserDataModel user, int amount)
        {
            if (!user.IsCurrencyRankExempt && amount > 0)
            {
                RankModel prevRank = this.GetRank(user);

                this.SetAmount(user, this.GetAmount(user) + amount);

                RankModel newRank = this.GetRank(user);

                if (prevRank != newRank && this.RankChangedCommand != null)
                {
                    this.RankChangedCommand.Perform(ChannelSession.Services.User.GetUserByID(user.ID)).Wait();
                }
            }
        }

        public void SubtractAmount(UserDataModel user, int amount)
        {
            if (!user.IsCurrencyRankExempt)
            {
                this.SetAmount(user, this.GetAmount(user) - amount);
            }
        }

        public void ResetAmount(UserDataModel user) { this.SetAmount(user, 0); }

        public RankModel GetRank(UserDataModel user)
        {
            if (this.Ranks.Count > 0)
            {
                int amount = this.GetAmount(user);
                RankModel rank = this.Ranks.Where(r => r.Amount <= amount).Max();
                if (rank != null)
                {
                    return rank;
                }
            }
            return CurrencyModel.NoRank;
        }

        public RankModel GetNextRank(UserDataModel user)
        {
            if (this.Ranks.Count > 0)
            {
                int amount = this.GetAmount(user);
                RankModel rank = this.Ranks.Where(r => r.Amount > amount).Min();
                if (rank != null)
                {
                    return rank;
                }
            }
            return CurrencyModel.NoRank;
        }

        public void UpdateUserData()
        {
            if (this.IsActive)
            {
                if (this.SpecialTracking == CurrencySpecialTrackingEnum.None)
                {
                    int interval = ChannelSession.MixerChannel.online ? this.AcquireInterval : this.OfflineAcquireInterval;
                    if (interval > 0)
                    {
                        DateTimeOffset minActiveTime = DateTimeOffset.Now.Subtract(TimeSpan.FromMinutes(this.MinimumActiveRate));
                        bool bonusesCanBeApplied = (ChannelSession.MixerChannel.online || this.OfflineAcquireAmount > 0);
                        foreach (UserViewModel user in ChannelSession.Services.User.GetAllWorkableUsers())
                        {
                            if (!user.Data.IsCurrencyRankExempt && (!this.HasMinimumActiveRate || user.LastActivity > minActiveTime))
                            {
                                int minutes = ChannelSession.MixerChannel.online ? user.Data.ViewingMinutes : user.Data.OfflineViewingMinutes;
                                if (minutes % interval == 0)
                                {
                                    this.AddAmount(user.Data, ChannelSession.MixerChannel.online ? this.AcquireAmount : this.OfflineAcquireAmount);
                                    if (bonusesCanBeApplied)
                                    {
                                        int bonus = 0;

                                        if (this.RegularBonus > 0 && user.UserRoles.Contains(UserRoleEnum.Regular))
                                        {
                                            bonus = Math.Max(this.RegularBonus, bonus);
                                        }
                                        if (this.SubscriberBonus > 0 && user.IsSubscriber)
                                        {
                                            bonus = Math.Max(this.SubscriberBonus, bonus);
                                        }
                                        if (this.ModeratorBonus > 0 && user.HasPermissionsTo(UserRoleEnum.Mod))
                                        {
                                            bonus = Math.Max(this.ModeratorBonus, bonus);
                                        }

                                        if (bonus > 0)
                                        {
                                            this.AddAmount(user.Data, bonus);
                                        }
                                    }
                                    ChannelSession.Settings.UserData.ManualValueChanged(user.ID);
                                }
                            }
                        }
                    }
                }
            }
        }

        public bool ShouldBeReset()
        {
            if (this.ResetInterval != CurrencyResetRateEnum.Never)
            {
                DateTimeOffset newResetDate = DateTimeOffset.MinValue;

                if (this.ResetInterval == CurrencyResetRateEnum.Daily)
                {
                    newResetDate = this.LastReset.AddDays(1);
                }
                else if (this.ResetStartCadence == DateTimeOffset.MinValue)
                {
                    if (this.ResetInterval == CurrencyResetRateEnum.Weekly) { newResetDate = this.LastReset.AddDays(7); }
                    else if (this.ResetInterval == CurrencyResetRateEnum.Monthly) { newResetDate = this.LastReset.AddMonths(1); }
                    else if (this.ResetInterval == CurrencyResetRateEnum.Yearly) { newResetDate = this.LastReset.AddYears(1); }
                    return (newResetDate < DateTimeOffset.Now);
                }
                else
                {
                    if (this.LastReset.Date != DateTimeOffset.Now.Date)
                    {
                        if (this.ResetInterval == CurrencyResetRateEnum.Weekly)
                        {
                            DateTimeOffset walkbackDate = DateTimeOffset.Now;
                            while (walkbackDate > this.LastReset)
                            {
                                if (walkbackDate.DayOfWeek == this.ResetStartCadence.DayOfWeek)
                                {
                                    return true;
                                }
                                walkbackDate = walkbackDate.Subtract(TimeSpan.FromDays(1));
                            }
                        }
                        else if (this.ResetInterval == CurrencyResetRateEnum.Monthly)
                        {
                            int resetDay = DateTime.DaysInMonth(DateTimeOffset.Now.Year, DateTimeOffset.Now.Month);
                            resetDay = Math.Min(resetDay, this.ResetStartCadence.Day);

                            DateTime newResetTime = new DateTime(DateTimeOffset.Now.Year, DateTimeOffset.Now.Month, resetDay);
                            return (DateTimeOffset.Now.Date >= newResetTime && this.LastReset < newResetTime);
                        }
                        else if (this.ResetInterval == CurrencyResetRateEnum.Yearly)
                        {
                            int resetDay = DateTime.DaysInMonth(DateTimeOffset.Now.Year, this.ResetStartCadence.Month);
                            resetDay = Math.Min(resetDay, this.ResetStartCadence.Day);

                            DateTime newResetTime = new DateTime(DateTimeOffset.Now.Year, this.ResetStartCadence.Month, resetDay);
                            return (DateTimeOffset.Now.Date >= newResetTime && this.LastReset < newResetTime);
                        }
                    }
                }
            }
            return false;
        }

        public async Task Reset()
        {
            foreach (UserDataModel user in ChannelSession.Settings.UserData.Values.ToList())
            {
                if (this.GetAmount(user) > 0)
                {
                    this.SetAmount(user, 0);
                    ChannelSession.Settings.UserData.ManualValueChanged(user.ID);
                }
            }
            this.LastReset = new DateTimeOffset(DateTimeOffset.Now.Date);
            await ChannelSession.SaveSettings();
        }

        public override bool Equals(object obj)
        {
            if (obj is CurrencyModel)
            {
                return this.Equals((CurrencyModel)obj);
            }
            return false;
        }

        public bool Equals(CurrencyModel other)
        {
            return this.ID.Equals(other.ID);
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }
    }
}
