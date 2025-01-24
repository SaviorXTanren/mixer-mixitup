using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
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

        public override string ToString() { return this.Name; }
    }

    [DataContract]
    public class CurrencyModel : IEquatable<CurrencyModel>
    {
        public static RankModel NoRank = new RankModel(MixItUp.Base.Resources.NoRank, 0);

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
        public Guid RankDownCommandID { get; set; }

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
        public bool IsActive { get { return !this.IsOnlineIntervalDisabled; } }

        [JsonIgnore]
        public bool IsRank { get { return this.Ranks.Count > 0; } }

        [JsonIgnore]
        public bool IsOnlineIntervalMinutes { get { return this.AcquireAmount == 1 && this.AcquireInterval == 1; } }

        [JsonIgnore]
        public bool IsOnlineIntervalHours { get { return this.AcquireAmount == 1 && this.AcquireInterval == 60; } }

        [JsonIgnore]
        public bool IsOnlineIntervalDisabled { get { return this.AcquireAmount == 0 && this.AcquireInterval == 0; } }

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
        public string AllTotalAmountSpecialIdentifier { get { return string.Format("{0}alltotal", this.SpecialIdentifier); } }

        [JsonIgnore]
        public string AllTotalAmountDisplaySpecialIdentifier { get { return string.Format("{0}display", this.AllTotalAmountSpecialIdentifier); } }

        [JsonIgnore]
        public string TopRegexSpecialIdentifier { get { return string.Format("{0}\\d+{1}", SpecialIdentifierStringBuilder.TopSpecialIdentifierHeader, this.SpecialIdentifier); } }

        [JsonIgnore]
        public string Top10SpecialIdentifier { get { return string.Format("{0}10{1}", SpecialIdentifierStringBuilder.TopSpecialIdentifierHeader, this.SpecialIdentifier); } }

        [JsonIgnore]
        public string TopSpecialIdentifier { get { return string.Format("{0}{1}", SpecialIdentifierStringBuilder.TopSpecialIdentifierHeader, this.SpecialIdentifier); } }

        [JsonIgnore]
        public string TopUserSpecialIdentifier { get { return string.Format("{0}{1}", this.TopSpecialIdentifier, SpecialIdentifierStringBuilder.UserSpecialIdentifierHeader); } }

        [JsonIgnore]
        public CommandModelBase RankChangedCommand
        {
            get { return ChannelSession.Settings.GetCommand(this.RankChangedCommandID); }
            set
            {
                if (value != null)
                {
                    this.RankChangedCommandID = value.ID;
                    ChannelSession.Settings.SetCommand(value);
                }
                else
                {
                    ChannelSession.Settings.RemoveCommand(this.RankChangedCommandID);
                    this.RankChangedCommandID = Guid.Empty;
                }
            }
        }

        [JsonIgnore]
        public CommandModelBase RankDownCommand
        {
            get { return ChannelSession.Settings.GetCommand(this.RankDownCommandID); }
            set
            {
                if (value != null)
                {
                    this.RankDownCommandID = value.ID;
                    ChannelSession.Settings.SetCommand(value);
                }
                else
                {
                    ChannelSession.Settings.RemoveCommand(this.RankDownCommandID);
                    this.RankDownCommandID = Guid.Empty;
                }
            }
        }

        public int GetAmount(UserV2ViewModel user)
        {
            if (user.CurrencyAmounts.ContainsKey(this.ID))
            {
                return user.CurrencyAmounts[this.ID];
            }
            return 0;
        }

        public int GetAmount(UserV2Model user)
        {
            if (user.CurrencyAmounts.ContainsKey(this.ID))
            {
                return user.CurrencyAmounts[this.ID];
            }
            return 0;
        }

        public bool HasAmount(UserV2ViewModel user, int amount)
        {
            return (user.IsSpecialtyExcluded || this.GetAmount(user) >= amount);
        }

        public void SetAmount(UserV2ViewModel user, int amount)
        {
            Logger.Log(LogLevel.Debug, $"Setting {amount} amount of {this.Name} for {user.ID}");

            RankModel prevRank = this.GetRank(user);

            user.CurrencyAmounts[this.ID] = Math.Min(Math.Max(amount, 0), this.MaxAmount);
            if (ChannelSession.Settings != null)
            {
                ChannelSession.Settings.Users.ManualValueChanged(user.ID);
            }

            RankModel newRank = this.GetRank(user);

            if (newRank.Amount > prevRank.Amount && this.RankChangedCommand != null)
            {
                AsyncRunner.RunAsyncBackground((cancellationToken) => ServiceManager.Get<CommandService>().Queue(this.RankChangedCommand, new CommandParametersModel(user)), new CancellationToken());
            }
            else if (newRank.Amount < prevRank.Amount && this.RankDownCommand != null)
            {
                AsyncRunner.RunAsyncBackground((cancellationToken) => ServiceManager.Get<CommandService>().Queue(this.RankDownCommand, new CommandParametersModel(user)), new CancellationToken());
            }
        }

        public void SetAmount(UserV2Model user, int amount)
        {
            Logger.Log(LogLevel.Debug, $"Setting {amount} amount of {this.Name} for {user.ID}");

            RankModel prevRank = this.GetRank(user);

            user.CurrencyAmounts[this.ID] = Math.Min(Math.Max(amount, 0), this.MaxAmount);
            if (ChannelSession.Settings != null)
            {
                ChannelSession.Settings.Users.ManualValueChanged(user.ID);
            }

            RankModel newRank = this.GetRank(user);

            if (newRank.Amount > prevRank.Amount && this.RankChangedCommand != null)
            {
                AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                {
                    UserV2ViewModel userVM = await ServiceManager.Get<UserService>().GetUserByID(StreamingPlatformTypeEnum.All, user.ID);
                    await ServiceManager.Get<CommandService>().Queue(this.RankChangedCommand, new CommandParametersModel(userVM));
                }, new CancellationToken());
            }
            else if (newRank.Amount < prevRank.Amount && this.RankDownCommand != null)
            {
                AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                {
                    UserV2ViewModel userVM = await ServiceManager.Get<UserService>().GetUserByID(StreamingPlatformTypeEnum.All, user.ID);
                    await ServiceManager.Get<CommandService>().Queue(this.RankDownCommand, new CommandParametersModel(userVM));
                }, new CancellationToken());
            }
        }

        public void AddAmount(UserV2ViewModel user, int amount)
        {
            if (!user.IsSpecialtyExcluded && amount > 0)
            {
                this.SetAmount(user, this.GetAmount(user) + amount);
            }
        }

        public void AddAmount(UserV2Model user, int amount)
        {
            if (!user.IsSpecialtyExcluded && amount > 0)
            {
                this.SetAmount(user, this.GetAmount(user) + amount);
            }
        }

        public void SubtractAmount(UserV2ViewModel user, int amount)
        {
            if (!user.IsSpecialtyExcluded)
            {
                this.SetAmount(user, this.GetAmount(user) - amount);
            }
        }

        public void SubtractAmount(UserV2Model user, int amount)
        {
            if (!user.IsSpecialtyExcluded)
            {
                this.SetAmount(user, this.GetAmount(user) - amount);
            }
        }

        public void ResetAmount(UserV2ViewModel user) { this.SetAmount(user, 0); }

        public RankModel GetRank(UserV2ViewModel user)
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

        public RankModel GetRank(UserV2Model user)
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

        public RankModel GetNextRank(UserV2ViewModel user)
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

        public void UpdateUserData(Dictionary<StreamingPlatformTypeEnum, bool> liveStreams)
        {
            if (this.IsActive)
            {
                if (this.SpecialTracking == CurrencySpecialTrackingEnum.None)
                {
                    DateTimeOffset minActiveTime = DateTimeOffset.Now.Subtract(TimeSpan.FromMinutes(this.MinimumActiveRate));
                    foreach (UserV2ViewModel user in ServiceManager.Get<UserService>().GetActiveUsers())
                    {
                        if (liveStreams.TryGetValue(user.Platform, out bool active) && active)
                        {
                            if (!user.IsSpecialtyExcluded && (!this.HasMinimumActiveRate || user.LastActivity > minActiveTime))
                            {
                                if (user.OnlineViewingMinutes % this.AcquireInterval == 0)
                                {
                                    this.AddAmount(user, this.AcquireAmount);

                                    int bonus = 0;
                                    if (this.RegularBonus > 0 && user.HasRole(UserRoleEnum.Regular))
                                    {
                                        bonus = Math.Max(this.RegularBonus, bonus);
                                    }
                                    if (this.SubscriberBonus > 0 && user.IsSubscriber)
                                    {
                                        bonus = Math.Max(this.SubscriberBonus, bonus);
                                    }
                                    if (this.ModeratorBonus > 0 && user.MeetsRole(UserRoleEnum.Moderator))
                                    {
                                        bonus = Math.Max(this.ModeratorBonus, bonus);
                                    }

                                    if (bonus > 0)
                                    {
                                        this.AddAmount(user, bonus);
                                    }

                                    ChannelSession.Settings.Users.ManualValueChanged(user.ID);
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
                if (this.LastReset == DateTimeOffset.MinValue)
                {
                    return true;
                }

                DateTimeOffset newResetDate = DateTimeOffset.MinValue;
                if (this.ResetInterval == CurrencyResetRateEnum.Daily)
                {
                    newResetDate = this.LastReset.AddDays(1);
                }
                else if (this.ResetStartCadence != DateTimeOffset.MinValue)
                {
                    if (this.ResetInterval == CurrencyResetRateEnum.Weekly)
                    {
                        newResetDate = new DateTime(this.LastReset.Year, this.LastReset.Month, this.LastReset.Day);
                        do
                        {
                            newResetDate = newResetDate.AddDays(1);
                        } while (newResetDate.DayOfWeek != this.ResetStartCadence.DayOfWeek);
                    }
                    else if (this.ResetInterval == CurrencyResetRateEnum.Monthly)
                    {
                        int day = Math.Min(this.ResetStartCadence.Day, DateTime.DaysInMonth(this.LastReset.Year, this.LastReset.Month));
                        newResetDate = new DateTime(this.LastReset.Year, this.LastReset.Month, day);
                        newResetDate = newResetDate.AddMonths(1);
                    }
                    else if (this.ResetInterval == CurrencyResetRateEnum.Yearly)
                    {
                        int day = Math.Min(this.ResetStartCadence.Day, DateTime.DaysInMonth(this.LastReset.Year, this.ResetStartCadence.Month));
                        newResetDate = new DateTime(this.LastReset.Year, this.ResetStartCadence.Month, day);
                        newResetDate = newResetDate.AddYears(1);
                    }
                }
                else
                {
                    if (this.ResetInterval == CurrencyResetRateEnum.Weekly){ newResetDate = this.LastReset.AddDays(7); }
                    else if (this.ResetInterval == CurrencyResetRateEnum.Monthly) { newResetDate = this.LastReset.AddMonths(1); }
                    else if (this.ResetInterval == CurrencyResetRateEnum.Yearly) { newResetDate = this.LastReset.AddYears(1); }
                }

                if (newResetDate != DateTimeOffset.MinValue)
                {
                    return (newResetDate.Date <= DateTimeOffset.Now.Date);
                }
            }
            return false;
        }

        public async Task Reset()
        {
            await ServiceManager.Get<UserService>().LoadAllUserData();

            foreach (UserV2Model user in ChannelSession.Settings.Users.Values.ToList())
            {
                if (user.CurrencyAmounts.ContainsKey(this.ID) && user.CurrencyAmounts[this.ID] > 0)
                {
                    user.CurrencyAmounts[this.ID] = 0;
                    ChannelSession.Settings.Users.ManualValueChanged(user.ID);
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
