using MixItUp.Base.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.User
{
    [DataContract]
    public class UserInventoryItemViewModel : IEquatable<UserInventoryItemViewModel>
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int MaxAmount { get; set; }

        [JsonIgnore]
        public bool HasMaxAmount { get { return this.MaxAmount > 0; } }

        [JsonIgnore]
        public string MaxAmountString
        {
            get
            {
                if (this.HasMaxAmount)
                {
                    return this.MaxAmount.ToString();
                }
                return "Default";
            }
        }

        [JsonIgnore]
        public string SpecialIdentifier { get { return SpecialIdentifierStringBuilder.ConvertToSpecialIdentifier(this.Name); } }

        public UserInventoryItemViewModel(string name, int maxAmount = -1)
        {
            this.Name = name;
            this.MaxAmount = maxAmount;
        }

        public override bool Equals(object obj)
        {
            if (obj is UserInventoryItemViewModel)
            {
                return this.Equals((UserInventoryItemViewModel)obj);
            }
            return false;
        }

        public bool Equals(UserInventoryItemViewModel other)
        {
            return this.Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }

    [DataContract]
    public class UserInventoryViewModel : IEquatable<UserInventoryViewModel>
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int DefaultMaxAmount { get; set; }

        [DataMember]
        public CurrencyResetRateEnum ResetInterval { get; set; }
        [DataMember]
        public DateTimeOffset LastReset { get; set; }

        [DataMember]
        public string SpecialIdentifier { get; set; }

        [DataMember]
        public Dictionary<string, UserInventoryItemViewModel> Items { get; set; }

        public UserInventoryViewModel()
        {
            this.ID = Guid.NewGuid();
            this.DefaultMaxAmount = 99;
            this.SpecialIdentifier = string.Empty;
            this.ResetInterval = CurrencyResetRateEnum.Never;
            this.LastReset = DateTimeOffset.MinValue;

            this.Items = new Dictionary<string, UserInventoryItemViewModel>();
        }

        [JsonIgnore]
        public string UserAmountSpecialIdentifierExample { get { return string.Format("{0}{1}<ITEM>", SpecialIdentifierStringBuilder.UserSpecialIdentifierHeader, this.SpecialIdentifier); } }

        [JsonIgnore]
        public string UserAmountSpecialIdentifierHeader { get { return string.Format("{0}{1}", SpecialIdentifierStringBuilder.UserSpecialIdentifierHeader, this.SpecialIdentifier); } }

        [JsonIgnore]
        public string UserAllAmountSpecialIdentifier { get { return string.Format("{0}{1}all", SpecialIdentifierStringBuilder.UserSpecialIdentifierHeader, this.SpecialIdentifier); } }

        public async Task Reset()
        {
            foreach (UserDataViewModel userData in ChannelSession.Settings.UserData.Values)
            {
                userData.ResetInventoryAmount(this);
                ChannelSession.Settings.UserData.ManualValueChanged(userData.ID);
            }
            await ChannelSession.SaveSettings();
        }

        public override bool Equals(object obj)
        {
            if (obj is UserInventoryViewModel)
            {
                return this.Equals((UserInventoryViewModel)obj);
            }
            return false;
        }

        public bool Equals(UserInventoryViewModel other)
        {
            return this.ID.Equals(other.ID);
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }
    }
}
