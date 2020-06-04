using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Currency
{
    public class StreamPassModel : IEquatable<StreamPassModel>
    {
        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public string Name { get; set; }        
        [DataMember]
        public string SpecialIdentifier { get; set; }
        [DataMember]
        public UserRoleEnum Permission { get; set; }
        [DataMember]
        public int MaxLevel { get; set; }
        [DataMember]
        public int PointsForLevelUp { get; set; }
        [DataMember]
        public double SubMultiplier { get; set; }

        [DataMember]
        public DateTimeOffset StartDate { get; set; }
        [DataMember]
        public DateTimeOffset EndDate { get; set; }

        [DataMember]
        public int ViewingRateAmount { get; set; }
        [DataMember]
        public int ViewingRateMinutes { get; set; }
        [DataMember]
        public int MinimumActiveRate { get; set; }
        [DataMember]
        public int FollowBonus { get; set; }
        [DataMember]
        public int HostBonus { get; set; }
        [DataMember]
        public int SubscribeBonus { get; set; }
        [DataMember]
        public int DonationBonus { get; set; }
        [DataMember]
        public int SparkBonus { get; set; }
        [DataMember]
        public int EmberBonus { get; set; }

        [DataMember]
        public CustomCommand DefaultLevelUpCommand { get; set; }
        [DataMember]
        public Dictionary<int, CustomCommand> CustomLevelUpCommands { get; set; } = new Dictionary<int, CustomCommand>();

        public StreamPassModel()
        {
            this.ID = Guid.NewGuid();
        }

        public StreamPassModel(StreamPassModel copy)
            : this()
        {
            this.Name = copy.Name + " COPY";
            this.SpecialIdentifier = SpecialIdentifierStringBuilder.ConvertToSpecialIdentifier(this.Name, maxLength: 15);
            this.Permission = copy.Permission;
            this.MaxLevel = copy.MaxLevel;
            this.PointsForLevelUp = copy.PointsForLevelUp;
            this.SubMultiplier = copy.SubMultiplier;

            this.StartDate = copy.StartDate;
            this.EndDate = copy.EndDate;

            this.ViewingRateAmount = copy.ViewingRateAmount;
            this.ViewingRateMinutes = copy.ViewingRateMinutes;
            this.MinimumActiveRate = copy.MinimumActiveRate;
            this.FollowBonus = copy.FollowBonus;
            this.HostBonus = copy.HostBonus;
            this.SubscribeBonus = copy.SubscribeBonus;
            this.DonationBonus = copy.DonationBonus;
            this.SparkBonus = copy.SparkBonus;
            this.EmberBonus = copy.EmberBonus;

            this.DefaultLevelUpCommand = JSONSerializerHelper.DeserializeFromString<CustomCommand>(JSONSerializerHelper.SerializeToString(this.DefaultLevelUpCommand));
            this.CustomLevelUpCommands.Clear();
            foreach (var kvp in copy.CustomLevelUpCommands)
            {
                this.CustomLevelUpCommands[kvp.Key] = JSONSerializerHelper.DeserializeFromString<CustomCommand>(JSONSerializerHelper.SerializeToString(kvp.Value));
            }
        }

        [JsonIgnore]
        public int MaxPoints { get { return this.MaxLevel * this.PointsForLevelUp; } }

        [JsonIgnore]
        public string DateRangeString { get { return string.Format("{0} - {1}", this.StartDate.ToFriendlyDateString(), this.EndDate.ToFriendlyDateString()); } }

        [JsonIgnore]
        public string UserAmountSpecialIdentifier { get { return string.Format("{0}{1}", SpecialIdentifierStringBuilder.UserSpecialIdentifierHeader, this.SpecialIdentifier); } }

        [JsonIgnore]
        public string UserLevelSpecialIdentifier { get { return string.Format("{0}level", this.UserAmountSpecialIdentifier); } }

        [JsonIgnore]
        public string UserPointsDisplaySpecialIdentifier { get { return string.Format("{0}display", this.UserAmountSpecialIdentifier); } }

        [JsonIgnore]
        public string SpecialIdentifiersReferenceDisplay
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(SpecialIdentifierStringBuilder.SpecialIdentifierHeader + this.UserAmountSpecialIdentifier);
                stringBuilder.AppendLine(SpecialIdentifierStringBuilder.SpecialIdentifierHeader + this.UserLevelSpecialIdentifier);
                return stringBuilder.ToString().Trim(new char[] { '\r', '\n' });
            }
            set { }
        }

        public int GetAmount(UserDataModel user)
        {
            if (user.StreamPassAmounts.ContainsKey(this.ID))
            {
                return user.StreamPassAmounts[this.ID];
            }
            return 0;
        }

        public int GetLevel(UserDataModel user) { return (this.GetAmount(user) / this.PointsForLevelUp); }

        public bool HasAmount(UserDataModel user, int amount)
        {
            return (user.IsCurrencyRankExempt || this.GetAmount(user) >= amount);
        }

        public void SetAmount(UserDataModel user, int amount)
        {
            user.StreamPassAmounts[this.ID] = Math.Min(Math.Max(amount, 0), this.MaxPoints);
            if (ChannelSession.Settings != null)
            {
                ChannelSession.Settings.UserData.ManualValueChanged(user.ID);
            }
        }

        public void AddAmount(UserDataModel user, int amount)
        {
            if (!user.IsCurrencyRankExempt && amount > 0)
            {
                int currentLevel = this.GetLevel(user);

                this.SetAmount(user, this.GetAmount(user) + amount);

                int newLevel = this.GetLevel(user);

                if (newLevel > currentLevel)
                {
                    for (int level = (currentLevel + 1); level <= newLevel; level++)
                    {
                        if (this.CustomLevelUpCommands.ContainsKey(level))
                        {
                            this.CustomLevelUpCommands[level].Perform(ChannelSession.Services.User.GetUserByID(user.ID)).Wait();
                        }
                        else if (this.DefaultLevelUpCommand != null)
                        {
                            this.DefaultLevelUpCommand.Perform(ChannelSession.Services.User.GetUserByID(user.ID)).Wait();
                        }
                    }
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

        public void UpdateUserData()
        {
            DateTime date = DateTimeOffset.Now.Date;
            if (ChannelSession.MixerChannel.online && this.StartDate.Date <= date && date <= this.EndDate && this.ViewingRateMinutes > 0)
            {
                DateTimeOffset minActiveTime = DateTimeOffset.Now.Subtract(TimeSpan.FromMinutes(this.MinimumActiveRate));
                foreach (UserViewModel user in ChannelSession.Services.User.GetAllWorkableUsers())
                {
                    if (!user.Data.IsCurrencyRankExempt && (this.MinimumActiveRate == 0 || user.LastActivity > minActiveTime))
                    {
                        if (user.Data.ViewingMinutes % this.ViewingRateMinutes == 0)
                        {
                            int amount = this.ViewingRateAmount;
                            if (this.SubMultiplier > 1.0 && user.HasPermissionsTo(UserRoleEnum.Subscriber))
                            {
                                amount = (int)Math.Ceiling(((double)amount) * this.SubMultiplier);
                            }
                            this.AddAmount(user.Data, amount);
                            ChannelSession.Settings.UserData.ManualValueChanged(user.ID);
                        }
                    }
                }
            }
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
            await ChannelSession.SaveSettings();
        }

        public override bool Equals(object obj)
        {
            if (obj is StreamPassModel)
            {
                return this.Equals((StreamPassModel)obj);
            }
            return false;
        }

        public bool Equals(StreamPassModel other)
        {
            return this.ID.Equals(other.ID);
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }
    }
}