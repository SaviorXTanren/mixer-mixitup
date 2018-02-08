using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Import;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel.User
{
    [DataContract]
    public class UserCurrencyDataViewModel : IEquatable<UserCurrencyDataViewModel>
    {
        [JsonIgnore]
        public UserDataViewModel User { get; set; }

        [JsonIgnore]
        public UserCurrencyViewModel Currency { get; set; }

        [JsonIgnore]
        private int _Amount;
        [DataMember]
        public int Amount
        {
            get { return _Amount; }
            set
            {
                UserRankViewModel prevRank = this.GetRank();
                this._Amount = value;
                UserRankViewModel newRank = this.GetRank();
                if (prevRank != newRank)
                {
                    GlobalEvents.RankChanged(this);
                }
            }
        }

        public UserCurrencyDataViewModel() { }

        public UserCurrencyDataViewModel(UserDataViewModel user, UserCurrencyViewModel currency, int amount = 0)
        {
            this.User = user;
            this.Currency = currency;
            this.Amount = amount;
        }

        public UserRankViewModel GetRank() { return this.Currency.GetRankForPoints(this.Amount); }

        public override bool Equals(object obj)
        {
            if (obj is UserCurrencyDataViewModel)
            {
                return this.Equals((UserCurrencyDataViewModel)obj);
            }
            return false;
        }

        public bool Equals(UserCurrencyDataViewModel other)
        {
            return this.Currency.Equals(other.Currency);
        }

        public override int GetHashCode()
        {
            return this.Currency.GetHashCode();
        }

        public override string ToString()
        {
            UserRankViewModel rank = this.Currency.GetRankForPoints(this.Amount);
            return string.Format("{0} - {1}", rank.Name, this.Amount);
        }
    }

    [DataContract]
    public class UserDataViewModel : IEquatable<UserDataViewModel>
    {
        [DataMember]
        public uint ID { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public int ViewingMinutes { get; set; }

        [DataMember]
        public LockedDictionary<UserCurrencyViewModel, UserCurrencyDataViewModel> CurrencyAmounts { get; set; }

        public UserDataViewModel()
        {
            this.CurrencyAmounts = new LockedDictionary<UserCurrencyViewModel, UserCurrencyDataViewModel>();
        }

        public UserDataViewModel(uint id, string username)
            : this()
        {
            this.ID = id;
            this.UserName = username;
        }

        public UserDataViewModel(UserViewModel user) : this(user.ID, user.UserName) { }

        public UserDataViewModel(ScorpBotViewer viewer)
            : this(viewer.ID, viewer.UserName)
        {
            this.ViewingMinutes = (int)(viewer.Hours * 60.0);
        }

        public UserDataViewModel(DbDataReader dataReader, IChannelSettings settings)
            : this(uint.Parse(dataReader["ID"].ToString()), dataReader["UserName"].ToString())
        {
            this.ViewingMinutes = int.Parse(dataReader["ViewingMinutes"].ToString());

            Dictionary<Guid, int> currencyAmounts = JsonConvert.DeserializeObject<Dictionary<Guid, int>>(dataReader["CurrencyAmounts"].ToString());
            if (currencyAmounts != null)
            {
                foreach (var kvp in currencyAmounts)
                {
                    if (settings.Currencies.ContainsKey(kvp.Key))
                    {
                        this.SetCurrencyAmount(settings.Currencies[kvp.Key], kvp.Value);
                    }
                }
            }
        }

        [JsonIgnore]
        public string ViewingHoursString { get { return (this.ViewingMinutes / 60).ToString(); } }

        [JsonIgnore]
        public string ViewingMinutesString { get { return (this.ViewingMinutes % 60).ToString(); } }

        [JsonIgnore]
        public string ViewingTimeString { get { return string.Format("{0} Hours & {1} Mins", this.ViewingHoursString, this.ViewingMinutesString); } }

        [JsonIgnore]
        public int PrimaryCurrency
        {
            get
            {
                UserCurrencyDataViewModel currency = this.CurrencyAmounts.Values.FirstOrDefault(c => !c.Currency.IsRank);
                if (currency != null)
                {
                    return currency.Amount;
                }
                return 0;
            }
        }

        [JsonIgnore]
        public UserRankViewModel Rank
        {
            get
            {
                UserRankViewModel rank = UserCurrencyViewModel.NoRank;
                UserCurrencyDataViewModel currency = this.CurrencyAmounts.Values.FirstOrDefault(c => c.Currency.IsRank);
                if (currency != null)
                {
                    rank = currency.GetRank();
                }
                return rank;
            }
        }

        [JsonIgnore]
        public string RankPoints
        {
            get
            {
                UserCurrencyDataViewModel currency = this.CurrencyAmounts.Values.FirstOrDefault(c => c.Currency.IsRank);
                if (currency != null)
                {
                    return currency.Amount.ToString();
                }
                return "0";
            }
        }

        [JsonIgnore]
        public string RankName
        {
            get
            {
                return this.Rank.Name;
            }
        }

        [JsonIgnore]
        public string RankNameAndPoints
        {
            get
            {
                return string.Format("{0} - {1}", this.RankName, this.RankPoints);
            }
        }

        public UserCurrencyDataViewModel GetCurrency(Guid currencyID)
        {
            if (ChannelSession.Settings.Currencies.ContainsKey(currencyID))
            {
                return this.GetCurrency(ChannelSession.Settings.Currencies[currencyID]);
            }
            return null;
        }

        public UserCurrencyDataViewModel GetCurrency(UserCurrencyViewModel currency)
        {
            return this.CurrencyAmounts.GetValueIfExists(currency, new UserCurrencyDataViewModel(this, currency));
        }

        public int GetCurrencyAmount(UserCurrencyViewModel currency)
        {
            return this.GetCurrency(currency).Amount;
        }

        public void SetCurrencyAmount(UserCurrencyViewModel currency, int amount)
        {
            this.GetCurrency(currency).Amount = Math.Min(amount, this.GetCurrency(currency).Currency.MaxAmount);
        }

        public void AddCurrencyAmount(UserCurrencyViewModel currency, int amount)
        {
            this.SetCurrencyAmount(currency, this.GetCurrencyAmount(currency) + amount);
        }

        public void SubtractCurrencyAmount(UserCurrencyViewModel currency, int amount)
        {
            this.AddCurrencyAmount(currency, -1 * amount);
        }

        public void ResetCurrency(UserCurrencyViewModel currency)
        {
            UserCurrencyDataViewModel currencyData = this.CurrencyAmounts.Values.FirstOrDefault(c => c.Currency.Equals(currency));
            if (currencyData != null)
            {
                currencyData.Amount = 0;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is UserDataViewModel)
            {
                return this.Equals((UserDataViewModel)obj);
            }
            return false;
        }

        public bool Equals(UserDataViewModel other)
        {
            return this.ID.Equals(other.ID);
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }

        public override string ToString()
        {
            return this.UserName;
        }

        internal string GetCurrencyAmountsString()
        {
            Dictionary<Guid, int> amounts = new Dictionary<Guid, int>();
            foreach (UserCurrencyDataViewModel currencyData in this.CurrencyAmounts.Values.ToList())
            {
                amounts.Add(currencyData.Currency.ID, currencyData.Amount);
            }
            return JsonConvert.SerializeObject(amounts);
        }
    }
}