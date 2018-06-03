using Mixer.Base.Model.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Store
{
    [DataContract]
    public class StoreListingModel
    {
        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public UserModel User { get; set; }

        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public List<string> Tags { get; set; }

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

        public StoreListingModel()
        {
            this.Tags = new List<string>();
            this.MainImage = null;
        }

        [JsonIgnore]
        public string Star1
        {
            get
            {
                if (this.AverageRating >= 1.0) { return "Star"; }
                else if (this.AverageRating >= 0.5) { return "StarHalf"; }
                return "StarOutline";
            }
        }

        [JsonIgnore]
        public string Star2
        {
            get
            {
                if (this.AverageRating >= 2.0) { return "Star"; }
                else if (this.AverageRating >= 1.5) { return "StarHalf"; }
                return "StarOutline";
            }
        }

        [JsonIgnore]
        public string Star3
        {
            get
            {
                if (this.AverageRating >= 3.0) { return "Star"; }
                else if (this.AverageRating >= 2.5) { return "StarHalf"; }
                return "StarOutline";
            }
        }

        [JsonIgnore]
        public string Star4
        {
            get
            {
                if (this.AverageRating >= 4.0) { return "Star"; }
                else if (this.AverageRating >= 3.5) { return "StarHalf"; }
                return "StarOutline";
            }
        }

        [JsonIgnore]
        public string Star5
        {
            get
            {
                if (this.AverageRating >= 5.0) { return "Star"; }
                else if (this.AverageRating >= 4.5) { return "StarHalf"; }
                return "StarOutline";
            }
        }
    }
}
