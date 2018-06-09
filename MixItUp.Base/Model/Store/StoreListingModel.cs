using Mixer.Base.Model.User;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Store
{
    [DataContract]
    public class StoreListingModel
    {
        public const string DefaultDisplayImage = "https://mixitupapp.com/img/bg-img/logo-sm.png";

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
        public bool IsCommandOwnedByUser { get { return (this.User != null && this.User.id.Equals(ChannelSession.User.id)); } }

        [JsonIgnore]
        public string UserAvatar { get { return (this.User != null && !string.IsNullOrEmpty(this.User.avatarUrl)) ? this.User.avatarUrl : UserViewModel.DefaultAvatarLink; } }
        [JsonIgnore]
        public string UserName { get { return (this.User != null) ? this.User.username : "Unknown"; } }

        [JsonIgnore]
        public string AverageRatingDisplayString { get { return Math.Round(this.AverageRating, 1).ToString(); } }

        [JsonIgnore]
        public string TagsString { get { return string.Join(", ", this.Tags); } }

        [JsonIgnore]
        public string CreateDateString { get { return this.CreatedDate.ToString("G"); } }

        [JsonIgnore]
        public string LastUpdatedString { get { return this.LastUpdatedDate.ToString("G"); } }
    }
}
