using Mixer.Base.Model.User;
using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Store
{
    public enum StoreCategoryType
    {
        Follows,
        Hosts,
    }

    [DataContract]
    public class StoreListingModel
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public StoreCategoryType Category { get; set; }

        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public UserModel User { get; set; }

        [DataMember]
        public double AverageRating { get; set; }
        [DataMember]
        public int TotalDownloads { get; set; }

        [DataMember]
        public byte[] MainImage { get; set; }

        [DataMember]
        public DateTimeOffset CreatedDate { get; set; }
        [DataMember]
        public DateTimeOffset LastUpdatedDate { get; set; }
    }
}
