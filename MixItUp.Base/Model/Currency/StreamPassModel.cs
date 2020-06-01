using MixItUp.Base.Util;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Currency
{
    public class StreamPassModel
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }
        
        [DataMember]
        public bool IsEnabled { get; set; }

        [DataMember]
        public string SpecialIdentifier { get; set; }

        [DataMember]
        public DateTimeOffset StartDate { get; set; }

        [DataMember]
        public DateTimeOffset EndDate { get; set; }

        public StreamPassModel()
        {
            this.ID = Guid.NewGuid();
        }

        public StreamPassModel(StreamPassModel copy)
            : this()
        {

        }

        [JsonIgnore]
        public string DateRangeString { get { return string.Format("{0} - {1}", this.StartDate.ToFriendlyDateString(), this.EndDate.ToFriendlyDateString()); } }

        [JsonIgnore]
        public string UserLevelSpecialIdentifier { get { return string.Format("{0}level", this.BaseUserSpecialIdentifier); } }

        [JsonIgnore]
        public string UserPointsSpecialIdentifier { get { return string.Format("{0}points", this.BaseUserSpecialIdentifier); } }

        [JsonIgnore]
        private string BaseUserSpecialIdentifier { get { return string.Format("{0}{1}", SpecialIdentifierStringBuilder.UserSpecialIdentifierHeader, this.SpecialIdentifier); } }
    }
}