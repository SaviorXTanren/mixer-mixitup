using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Currency
{
    public class StreamPassModel
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
        public double FollowBonus { get; set; }
        [DataMember]
        public double HostBonus { get; set; }
        [DataMember]
        public double SubscribeBonus { get; set; }
        [DataMember]
        public double DonationBonus { get; set; }
        [DataMember]
        public double SparkBonus { get; set; }
        [DataMember]
        public double EmberBonus { get; set; }

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
        public string DateRangeString { get { return string.Format("{0} - {1}", this.StartDate.ToFriendlyDateString(), this.EndDate.ToFriendlyDateString()); } }

        [JsonIgnore]
        public string UserLevelSpecialIdentifier { get { return string.Format("{0}level", this.BaseUserSpecialIdentifier); } }

        [JsonIgnore]
        public string UserPointsSpecialIdentifier { get { return string.Format("{0}points", this.BaseUserSpecialIdentifier); } }

        [JsonIgnore]
        private string BaseUserSpecialIdentifier { get { return string.Format("{0}{1}", SpecialIdentifierStringBuilder.UserSpecialIdentifierHeader, this.SpecialIdentifier); } }

        [JsonIgnore]
        public string SpecialIdentifiersReferenceDisplay
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(SpecialIdentifierStringBuilder.SpecialIdentifierHeader + this.UserLevelSpecialIdentifier);
                stringBuilder.AppendLine(SpecialIdentifierStringBuilder.SpecialIdentifierHeader + this.UserPointsSpecialIdentifier);
                return stringBuilder.ToString().Trim(new char[] { '\r', '\n' });
            }
            set { }
        }

        public async Task Reset()
        {

        }
    }
}