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
        public const string DefaultDisplayImage = "https://uploads.beam.pro/avatar/6442tw1b-7207866.jpg";

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
        public string DisplayImage { get; set; }

        [DataMember]
        public DateTimeOffset CreatedDate { get; set; }
        [DataMember]
        public DateTimeOffset LastUpdatedDate { get; set; }

        public StoreListingModel()
        {
            this.Tags = new List<string>();
        }

        [JsonIgnore]
        public string AverageRatingDisplayString { get { return Math.Round(this.AverageRating, 1).ToString(); } }
    }
}
