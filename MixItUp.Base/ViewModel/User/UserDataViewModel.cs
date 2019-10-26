using Mixer.Base.Model.User;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Import;
using MixItUp.Base.Util;
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

        public UserRankViewModel GetNextRank() { return this.Currency.GetNextRankForPoints(this.Amount); }

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
    public class UserInventoryDataViewModel : IEquatable<UserInventoryDataViewModel>
    {
        [JsonIgnore]
        public UserDataViewModel User { get; set; }

        [JsonIgnore]
        public UserInventoryViewModel Inventory { get; set; }

        [DataMember]
        public Dictionary<string, int> Amounts { get; set; }

        public UserInventoryDataViewModel()
        {
            this.Amounts = new Dictionary<string, int>();
        }

        public UserInventoryDataViewModel(UserDataViewModel user, UserInventoryViewModel inventory) : this(user, inventory, new Dictionary<string, int>()) { }

        public UserInventoryDataViewModel(UserDataViewModel user, UserInventoryViewModel inventory, IDictionary<string, int> amounts)
        {
            this.User = user;
            this.Inventory = inventory;
            this.Amounts = new Dictionary<string, int>(amounts);
        }

        public int GetAmount(UserInventoryItemViewModel item) { return this.GetAmount(item.Name); }

        public int GetAmount(string itemName)
        {
            if (this.Amounts.ContainsKey(itemName))
            {
                return this.Amounts[itemName];
            }
            return 0;
        }

        public override bool Equals(object obj)
        {
            if (obj is UserInventoryDataViewModel)
            {
                return this.Equals((UserInventoryDataViewModel)obj);
            }
            return false;
        }

        public bool Equals(UserInventoryDataViewModel other)
        {
            return this.User.Equals(other.User) && this.Inventory.Equals(other.Inventory);
        }

        public override int GetHashCode()
        {
            return this.Inventory.GetHashCode();
        }
    }

    [DataContract]
    public class UserDataViewModel : NotifyPropertyChangedBase, IEquatable<UserDataViewModel>
    {
        [DataMember]
        public uint ID { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string CustomTitle { get; set; }

        [DataMember]
        public int ViewingMinutes { get; set; }

        [DataMember]
        public int OfflineViewingMinutes { get; set; }

        [DataMember]
        public LockedDictionary<UserCurrencyViewModel, UserCurrencyDataViewModel> CurrencyAmounts { get; set; } = new LockedDictionary<UserCurrencyViewModel, UserCurrencyDataViewModel>();

        [DataMember]
        public LockedDictionary<UserInventoryViewModel, UserInventoryDataViewModel> InventoryAmounts { get; set; } = new LockedDictionary<UserInventoryViewModel, UserInventoryDataViewModel>();

        [DataMember]
        public LockedList<ChatCommand> CustomCommands { get; set; } = new LockedList<ChatCommand>();

        [DataMember]
        public CustomCommand EntranceCommand { get; set; }

        [DataMember]
        public bool IsCurrencyRankExempt { get; set; }

        [DataMember]
        public bool IsSparkExempt { get; set; }

        [DataMember]
        public uint GameWispUserID { get; set; }

        [DataMember]
        public string PatreonUserID { get; set; }

        [DataMember]
        public uint ModerationStrikes { get; set; }

        [DataMember]
        public uint TotalStreamsWatched { get; set; }

        [DataMember]
        public double TotalAmountDonated { get; set; }
        
        [DataMember]
        public uint TotalSparksSpent { get; set; }

        [DataMember]
        public uint TotalEmbersSpent { get; set; }

        [DataMember]
        public uint TotalSubsGifted { get; set; }

        [DataMember]
        public uint TotalSubsReceived { get; set; }

        [DataMember]
        public uint TotalChatMessageSent { get; set; }

        [DataMember]
        public uint TotalTimesTagged { get; set; }

        [DataMember]
        public uint TotalSkillsUsed { get; set; }

        [DataMember]
        public uint TotalCommandsRun { get; set; }

        [DataMember]
        public uint TotalMonthsSubbed { get; set; }

        public UserDataViewModel() { }

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

        public UserDataViewModel(StreamlabsChatBotViewer viewer)
            : this(viewer.ID, viewer.Name)
        {
            this.ViewingMinutes = (int)(viewer.Hours * 60.0);
        }

        public UserDataViewModel(DbDataReader dataReader, IChannelSettings settings)
            : this(uint.Parse(dataReader["ID"].ToString()), dataReader["UserName"].ToString())
        {
            this.ViewingMinutes = int.Parse(dataReader["ViewingMinutes"].ToString());

            if (dataReader.ColumnExists("CurrencyAmounts"))
            {
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

            if (dataReader.ColumnExists("InventoryAmounts"))
            {
                Dictionary<Guid, Dictionary<string, int>> inventoryAmounts = JsonConvert.DeserializeObject<Dictionary<Guid, Dictionary<string, int>>>(dataReader["InventoryAmounts"].ToString());
                if (inventoryAmounts != null)
                {
                    foreach (var kvp in inventoryAmounts)
                    {
                        if (settings.Inventories.ContainsKey(kvp.Key))
                        {
                            UserInventoryViewModel inventory = settings.Inventories[kvp.Key];
                            this.InventoryAmounts[inventory] = new UserInventoryDataViewModel(this, inventory, kvp.Value);
                        }
                    }
                }
            }

            if (dataReader.ColumnExists("CustomCommands") && !string.IsNullOrEmpty(dataReader["CustomCommands"].ToString()))
            {
                this.CustomCommands.AddRange(SerializerHelper.DeserializeFromString<List<ChatCommand>>(dataReader["CustomCommands"].ToString()));
            }

            if (dataReader.ColumnExists("Options") && !string.IsNullOrEmpty(dataReader["Options"].ToString()))
            {
                JObject optionsJObj = JObject.Parse(dataReader["Options"].ToString());
                if (optionsJObj.ContainsKey("EntranceCommand") && optionsJObj["EntranceCommand"] != null)
                {
                    this.EntranceCommand = SerializerHelper.DeserializeFromString<CustomCommand>(optionsJObj["EntranceCommand"].ToString());
                }
                this.IsSparkExempt = this.GetOptionValue<bool>(optionsJObj, "IsSparkExempt");
                this.IsCurrencyRankExempt = this.GetOptionValue<bool>(optionsJObj, "IsCurrencyRankExempt");
                this.GameWispUserID = this.GetOptionValue<uint>(optionsJObj, "GameWispUserID");
                this.PatreonUserID = this.GetOptionValue<string>(optionsJObj, "PatreonUserID");
                this.ModerationStrikes = this.GetOptionValue<uint>(optionsJObj, "ModerationStrikes");
                this.CustomTitle = this.GetOptionValue<string>(optionsJObj, "CustomTitle");
                this.TotalStreamsWatched = this.GetOptionValue<uint>(optionsJObj, "TotalStreamsWatched");
                this.TotalAmountDonated = this.GetOptionValue<double>(optionsJObj, "TotalAmountDonated");
                this.TotalSparksSpent = this.GetOptionValue<uint>(optionsJObj, "TotalSparksSpent");
                this.TotalEmbersSpent = this.GetOptionValue<uint>(optionsJObj, "TotalEmbersSpent");
                this.TotalSubsGifted = this.GetOptionValue<uint>(optionsJObj, "TotalSubsGifted");
                this.TotalSubsReceived = this.GetOptionValue<uint>(optionsJObj, "TotalSubsReceived");
                this.TotalChatMessageSent = this.GetOptionValue<uint>(optionsJObj, "TotalChatMessageSent");
                this.TotalTimesTagged = this.GetOptionValue<uint>(optionsJObj, "TotalTimesTagged");
                this.TotalSkillsUsed = this.GetOptionValue<uint>(optionsJObj, "TotalSkillsUsed");
                this.TotalCommandsRun = this.GetOptionValue<uint>(optionsJObj, "TotalCommandsRun");
                this.TotalMonthsSubbed = this.GetOptionValue<uint>(optionsJObj, "TotalMonthsSubbed");
            }
        }

        [JsonIgnore]
        public string ViewingHoursString { get { return (this.ViewingMinutes / 60).ToString(); } }

        [JsonIgnore]
        public string ViewingMinutesString { get { return (this.ViewingMinutes % 60).ToString(); } }

        [JsonIgnore]
        public int ViewingHoursPart
        {
            get
            {
                return this.ViewingMinutes / 60;
            }
            set
            {
                this.ViewingMinutes = value * 60 + this.ViewingMinutesPart;
            }
        }

        [JsonIgnore]
        public int ViewingMinutesPart
        {
            get
            {
                return this.ViewingMinutes % 60;
            }
            set
            {
                int extraHours = value / 60;
                this.ViewingHoursPart += extraHours;
                this.ViewingMinutes = ViewingHoursPart * 60 + (value % 60);
                this.NotifyPropertyChanged(nameof(ViewingHoursPart));
            }
        }

        [JsonIgnore]
        public string ViewingTimeString { get { return string.Format("{0} Hours & {1} Mins", this.ViewingHoursString, this.ViewingMinutesString); } }

        [JsonIgnore]
        public string ViewingTimeShortString { get { return string.Format("{0}H & {1}M", this.ViewingHoursString, this.ViewingMinutesString); } }

        [JsonIgnore]
        public int PrimaryCurrency
        {
            get
            {
                UserCurrencyDataViewModel currency = this.CurrencyAmounts.Values.FirstOrDefault(c => !c.Currency.IsRank && c.Currency.IsPrimary);
                if (currency == null)
                {
                    currency = this.CurrencyAmounts.Values.FirstOrDefault(c => !c.Currency.IsRank);
                }

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
                UserCurrencyDataViewModel currency = this.CurrencyAmounts.Values.FirstOrDefault(c => c.Currency.IsRank && c.Currency.IsPrimary);
                if (currency == null)
                {
                    currency = this.CurrencyAmounts.Values.FirstOrDefault(c => c.Currency.IsRank);
                }

                if (currency != null)
                {
                    rank = currency.GetRank();
                }
                return rank;
            }
        }

        [JsonIgnore]
        public int PrimaryRankPoints
        {
            get
            {
                UserCurrencyDataViewModel currency = this.CurrencyAmounts.Values.FirstOrDefault(c => c.Currency.IsRank);
                if (currency != null)
                {
                    return currency.Amount;
                }
                return 0;
            }
        }

        [JsonIgnore]
        public string PrimaryRankName
        {
            get
            {
                return this.Rank.Name;
            }
        }

        [JsonIgnore]
        public string PrimaryRankNameAndPoints
        {
            get
            {
                return string.Format("{0} - {1}", this.PrimaryRankName, this.PrimaryRankPoints);
            }
        }

        public void UpdateData(UserViewModel user)
        {
            this.UserName = user.UserName;
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

        public bool HasCurrencyAmount(UserCurrencyViewModel currency, int amount)
        {
            return (this.IsCurrencyRankExempt || this.GetCurrencyAmount(currency) >= amount);
        }

        public void SetCurrencyAmount(UserCurrencyViewModel currency, int amount)
        {
            UserCurrencyDataViewModel currencyData = this.GetCurrency(currency);
            currencyData.Amount = Math.Min(amount, currencyData.Currency.MaxAmount);
        }

        public void AddCurrencyAmount(UserCurrencyViewModel currency, int amount)
        {
            if (!this.IsCurrencyRankExempt)
            {
                this.SetCurrencyAmount(currency, this.GetCurrencyAmount(currency) + amount);
            }
        }

        public void SubtractCurrencyAmount(UserCurrencyViewModel currency, int amount)
        {
            if (!this.IsCurrencyRankExempt)
            {
                this.SetCurrencyAmount(currency, Math.Max(this.GetCurrencyAmount(currency) - amount, 0));
            }
        }

        public void ResetCurrencyAmount(UserCurrencyViewModel currency)
        {
            this.CurrencyAmounts[currency] = new UserCurrencyDataViewModel(this, currency);
        }

        public UserInventoryDataViewModel GetInventory(Guid inventoryID)
        {
            if (ChannelSession.Settings.Inventories.ContainsKey(inventoryID))
            {
                return this.GetInventory(ChannelSession.Settings.Inventories[inventoryID]);
            }
            return null;
        }

        public UserInventoryDataViewModel GetInventory(UserInventoryViewModel inventory)
        {
            return this.InventoryAmounts.GetValueIfExists(inventory, new UserInventoryDataViewModel(this, inventory));
        }

        public int GetInventoryAmount(UserInventoryViewModel inventory, string item)
        {
            UserInventoryDataViewModel inventoryData = this.GetInventory(inventory);
            if (inventoryData.Amounts.ContainsKey(item))
            {
                return inventoryData.Amounts[item];
            }
            return 0;
        }

        public bool HasInventoryAmount(UserInventoryViewModel inventory, string item, int amount)
        {
            return (this.IsCurrencyRankExempt || this.GetInventoryAmount(inventory, item) >= amount);
        }

        public void SetInventoryAmount(UserInventoryViewModel inventory, string itemName, int amount)
        {
            if (inventory.Items.ContainsKey(itemName))
            {
                UserInventoryItemViewModel item = inventory.Items[itemName];
                UserInventoryDataViewModel inventoryData = this.GetInventory(inventory);
                inventoryData.Amounts[itemName] = Math.Min(amount, item.HasMaxAmount ? item.MaxAmount : inventoryData.Inventory.DefaultMaxAmount);
            }
        }

        public void AddInventoryAmount(UserInventoryViewModel inventory, string item, int amount)
        {
            if (!this.IsCurrencyRankExempt)
            {
                this.SetInventoryAmount(inventory, item, this.GetInventoryAmount(inventory, item) + amount);
            }
        }

        public void SubtractInventoryAmount(UserInventoryViewModel inventory, string item, int amount)
        {
            if (!this.IsCurrencyRankExempt)
            {
                this.SetInventoryAmount(inventory, item, Math.Max(this.GetInventoryAmount(inventory, item) - amount, 0));
            }
        }

        public void ResetInventoryAmount(UserInventoryViewModel inventory)
        {
            this.InventoryAmounts[inventory] = new UserInventoryDataViewModel(this, inventory);
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

        internal string GetInventoryAmountsString()
        {
            Dictionary<Guid, Dictionary<string, int>> amounts = new Dictionary<Guid, Dictionary<string, int>>();
            foreach (UserInventoryDataViewModel inventoryData in this.InventoryAmounts.Values.ToList())
            {
                amounts[inventoryData.Inventory.ID] = inventoryData.Amounts;
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
            options["PatreonUserID"] = this.PatreonUserID;
            options["ModerationStrikes"] = this.ModerationStrikes;
            options["CustomTitle"] = this.CustomTitle;
            options["TotalStreamsWatched"] = this.TotalStreamsWatched.ToString();
            options["TotalAmountDonated"] = this.TotalAmountDonated.ToString();
            options["TotalSparksSpent"] = this.TotalSparksSpent.ToString();
            options["TotalEmbersSpent"] = this.TotalEmbersSpent.ToString();
            options["TotalSubsGifted"] = this.TotalSubsGifted.ToString();
            options["TotalSubsReceived"] = this.TotalSubsReceived.ToString();
            options["TotalChatMessageSent"] = this.TotalChatMessageSent.ToString();
            options["TotalTimesTagged"] = this.TotalTimesTagged.ToString();
            options["TotalSkillsUsed"] = this.TotalSkillsUsed.ToString();
            options["TotalCommandsRun"] = this.TotalCommandsRun.ToString();
            options["TotalMonthsSubbed"] = this.TotalMonthsSubbed.ToString();
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