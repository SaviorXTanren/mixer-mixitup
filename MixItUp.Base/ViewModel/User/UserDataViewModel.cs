using Mixer.Base.Model.User;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Import.ScorpBot;
using MixItUp.Base.Model.Import.Streamlabs;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel.User
{
    [DataContract]
    public class UserDataViewModel : NotifyPropertyChangedBase, IEquatable<UserDataViewModel>
    {
        [DataMember]
        public Guid ID { get; set; } = Guid.NewGuid();

        [DataMember]
        public uint MixerID { get; set; }
        [DataMember]
        public string MixerUsername { get; set; }

        [DataMember]
        public string CustomTitle { get; set; }

        [DataMember]
        public int ViewingMinutes { get; set; }

        [DataMember]
        public int OfflineViewingMinutes { get; set; }

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
            this.MixerID = id;
            this.MixerUsername = username;
        }

        public UserDataViewModel(UserModel user) : this(user.id, user.username) { }

        public UserDataViewModel(UserViewModel user) : this(user.MixerID, user.MixerUsername) { }

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
                UserCurrencyModel currency = ChannelSession.Settings.Currencies.Values.FirstOrDefault(c => !c.IsRank && c.IsPrimary);
                if (currency != null)
                {
                    return currency.GetAmount(this);
                }
                return 0;
            }
        }

        [JsonIgnore]
        public UserRankViewModel Rank
        {
            get
            {
                UserCurrencyModel currency = ChannelSession.Settings.Currencies.Values.FirstOrDefault(c => !c.IsRank && c.IsPrimary);
                if (currency != null)
                {
                    return currency.GetRank(this);
                }
                return UserCurrencyModel.NoRank;
            }
        }

        [JsonIgnore]
        public int PrimaryRankPoints
        {
            get
            {
                UserCurrencyModel currency = ChannelSession.Settings.Currencies.Values.FirstOrDefault(c => c.IsRank && c.IsPrimary);
                if (currency != null)
                {
                    return currency.GetAmount(this);
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
            this.MixerUsername = user.MixerUsername;
        }

        public UserCurrencyDataViewModel GetCurrency(UserCurrencyModel currency)
        {
            return new UserCurrencyDataViewModel(this, currency);
        }

        public int GetCurrencyAmount(UserCurrencyModel currency)
        {
            return currency.GetAmount(this);
        }

        public bool HasCurrencyAmount(UserCurrencyModel currency, int amount)
        {
            return (this.IsCurrencyRankExempt || this.GetCurrencyAmount(currency) >= amount);
        }

        public void SetCurrencyAmount(UserCurrencyModel currency, int amount)
        {
            currency.SetAmount(this, amount);
        }

        public void AddCurrencyAmount(UserCurrencyModel currency, int amount)
        {
            currency.AddAmount(this, amount);
        }

        public void SubtractCurrencyAmount(UserCurrencyModel currency, int amount)
        {
            currency.SubtractAmount(this, amount);
        }

        public void ResetCurrencyAmount(UserCurrencyModel currency)
        {
            currency.ResetAmount(this);
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
            return this.MixerID.Equals(other.MixerID);
        }

        public override int GetHashCode()
        {
            return this.MixerID.GetHashCode();
        }

        public override string ToString()
        {
            return this.MixerUsername;
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