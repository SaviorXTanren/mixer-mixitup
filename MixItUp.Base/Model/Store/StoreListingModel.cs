using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.Actions;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Store
{
    [DataContract]
    public class StoreListingModel
    {
        public static IEnumerable<string> ValidTags
        {
            get
            {
                List<string> tags = new List<string>();
                tags.AddRange(EnumHelper.GetEnumNames<ActionTypeEnum>());
                tags.Add("Host");
                tags.Add("Follow");
                tags.Add("Donation");
                tags.Add("Subscribe");
                tags.Add("Resubscribe");
                tags.Add("Raid");
                tags.Add("Shoutout");
                tags.Add("Social Media");
                tags.Add("Host");
                return tags.OrderBy(s => s);
            }
        }

        public const string DefaultDisplayImage = "https://mixitupapp.com/img/bg-img/logo-sm.png";

        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public int UserID { get; set; }
        [DataMember]
        public string AppVersion { get; set; }

        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public List<string> Tags { get; set; }

        [DataMember]
        public string DisplayImageLink { get; set; }

        [DataMember]
        public int TotalDownloads { get; set; }

        [DataMember]
        public bool AssetsIncluded { get; set; }

        [DataMember]
        public string MetadataString { get; set; }

        [DataMember]
        public DateTimeOffset LastUpdatedDate { get; set; }

        [DataMember]
        public double AverageRating { get; set; }

        [DataMember]
        public UserModel User { get; set; }

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
        public string TagsString
        {
            get { return string.Join(", ", this.Tags); }
            set { this.Tags = (!string.IsNullOrEmpty(value)) ? new List<string>(value.Split(new char[] { ' ' })) : new List<string>(); }
        }

        [JsonIgnore]
        public string LastUpdatedString { get { return this.LastUpdatedDate.ToString("G"); } }
    }
}
