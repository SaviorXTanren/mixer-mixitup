using MixItUp.Base.Util;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.User
{
    [DataContract]
    public class UserInventoryViewModel : IEquatable<UserInventoryViewModel>
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int MaxAmount { get; set; }

        [DataMember]
        public string SpecialIdentifier { get; set; }

        public UserInventoryViewModel()
        {
            this.ID = Guid.NewGuid();
            this.MaxAmount = int.MaxValue;
            this.SpecialIdentifier = string.Empty;
        }

        [JsonIgnore]
        public string UserAmountSpecialIdentifierExample { get { return string.Format("{0}{1}<ITEM NAME>", SpecialIdentifierStringBuilder.UserSpecialIdentifierHeader, this.SpecialIdentifier); } }

        [JsonIgnore]
        public string UserAmountSpecialIdentifierRegex { get { return string.Format("{0}{1}\\w+", SpecialIdentifierStringBuilder.UserSpecialIdentifierHeader, this.SpecialIdentifier); } }

        [JsonIgnore]
        public string Top10SpecialIdentifierExample { get { return string.Format("{0}10{1}<ITEM NAME>", SpecialIdentifierStringBuilder.TopSpecialIdentifierHeader, this.SpecialIdentifier); } }

        [JsonIgnore]
        public string TopRegexSpecialIdentifier { get { return string.Format("{0}\\d+{1}\\w+", SpecialIdentifierStringBuilder.TopSpecialIdentifierHeader, this.SpecialIdentifier); } }

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
