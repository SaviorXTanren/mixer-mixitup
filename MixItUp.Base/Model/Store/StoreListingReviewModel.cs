using Mixer.Base.Model.User;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Store
{
    [DataContract]
    public class StoreListingReviewModel
    {
        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public Guid ListingID { get; set; }
        [DataMember]
        public int UserID { get; set; }

        [DataMember]
        public int Rating { get; set; }
        [DataMember]
        public string Review { get; set; }

        [DataMember]
        public DateTimeOffset LastUpdatedDate { get; set; }

        [DataMember]
        public UserModel User { get; set; }

        [JsonIgnore]
        public string UserName { get { return (this.User != null) ? this.User.username : "Unknown"; } }

        [JsonIgnore]
        public string LastUpdatedDateString { get { return this.LastUpdatedDate.ToString("G"); } }
    }
}
