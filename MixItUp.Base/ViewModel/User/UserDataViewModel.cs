using Mixer.Base.Model.User;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Import;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                if (value < 0)
                {
                    value = 0;
                }

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
            return this.User.Equals(other.User) && this.Currency.Equals(other.Currency);
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
        public int OfflineViewingMinutes { get; set; }

        [DataMember]
        public LockedDictionary<UserCurrencyViewModel, UserCurrencyDataViewModel> CurrencyAmounts { get; set; }

        [DataMember]
        public LockedList<ChatCommand> CustomCommands { get; set; }

        [DataMember]
        public CustomCommand EntranceCommand { get; set; }

        [DataMember]
        public bool IsCurrencyRankExempt { get; set; }

        [DataMember]
        public bool IsSparkExempt { get; set; }

        [DataMember]
        public uint GameWispUserID { get; set; }

        public UserDataViewModel()
        {
            this.CurrencyAmounts = new LockedDictionary<UserCurrencyViewModel, UserCurrencyDataViewModel>();
            this.CustomCommands = new LockedList<ChatCommand>();
        }

        public UserDataViewModel(uint id, string username)
            : this()
        {
            this.ID = id;
            this.UserName = username;
        }

        public UserDataViewModel(UserModel user) : this(user.id, user.username) { }

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

            if (dataReader["CustomCommands"] != null && !string.IsNullOrEmpty(dataReader["CustomCommands"].ToString()))
            {
                this.CustomCommands.AddRange(SerializerHelper.DeserializeFromString<List<ChatCommand>>(dataReader["CustomCommands"].ToString()));
            }

            if (dataReader["Options"] != null && !string.IsNullOrEmpty(dataReader["Options"].ToString()))
            {
                JObject optionsJObj = JObject.Parse(dataReader["Options"].ToString());
                if (optionsJObj["EntranceCommand"] != null)
                {
                    this.EntranceCommand = SerializerHelper.DeserializeFromString<CustomCommand>(optionsJObj["EntranceCommand"].ToString());
                }
                this.IsSparkExempt = this.GetOptionValue<bool>(optionsJObj, "IsSparkExempt");
                this.IsCurrencyRankExempt = this.GetOptionValue<bool>(optionsJObj, "IsCurrencyRankExempt");
                this.GameWispUserID = this.GetOptionValue<uint>(optionsJObj, "GameWispUserID");
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

        public void UpdateData(UserModel user)
        {
            this.UserName = user.username;
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
            if (!this.IsCurrencyRankExempt)
            {
                UserCurrencyDataViewModel currencyData = this.GetCurrency(currency);
                currencyData.Amount = Math.Min(amount, currencyData.Currency.MaxAmount);
            }
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
                amounts[currencyData.Currency.ID] = currencyData.Amount;
            }
            return SerializerHelper.SerializeToString(amounts);
        }

        internal string GetCustomCommandsString()
        {
            return SerializerHelper.SerializeToString(this.CustomCommands.ToList());
        }

        internal string GetOptionsString()
        {
            JObject options = new JObject();
            options["EntranceCommand"] = SerializerHelper.SerializeToString(this.EntranceCommand);
            options["IsSparkExempt"] = this.IsSparkExempt;
            options["IsCurrencyRankExempt"] = this.IsCurrencyRankExempt;
            options["GameWispUserID"] = this.GameWispUserID;
            return options.ToString();
        }

        private T GetOptionValue<T>(JObject jobj, string key)
        {
            if (jobj[key] != null)
            {
                return jobj[key].ToObject<T>();
            }
            return default(T);
        }
    }
}