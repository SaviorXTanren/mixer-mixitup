using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.Actions;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Store
{
    [DataContract]
    public class StoreListingModel
    {
        public const string FollowTag = "Follow";
        public const string HostTag = "Host";
        public const string DonationTag = "Donation";
        public const string SubscribeTag = "Subscribe";
        public const string ResubscribeTag = "Resubscribe";
        public const string RaidTag = "Raid";
        public const string ShoutoutTag = "Shoutout";
        public const string SocialMediaTag = "Social Media";

        public static IEnumerable<string> ValidTags
        {
            get
            {
                List<string> tags = new List<string>();
                tags.AddRange(EnumHelper.GetEnumNames<ActionTypeEnum>());
                tags.Add(StoreListingModel.FollowTag);
                tags.Add(StoreListingModel.HostTag);
                tags.Add(StoreListingModel.DonationTag);
                tags.Add(StoreListingModel.SubscribeTag);
                tags.Add(StoreListingModel.ResubscribeTag);
                tags.Add(StoreListingModel.RaidTag);
                tags.Add(StoreListingModel.ShoutoutTag);
                tags.Add(StoreListingModel.SocialMediaTag);
                return tags.OrderBy(s => s);
            }
        }

        public const string DefaultDisplayImage = "https://mixitupapp.com/img/bg-img/logo-sm.png";

        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public uint UserID { get; set; }
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
        public long TotalUses { get; set; }

        [DataMember]
        public bool AssetsIncluded { get; set; }

        [DataMember]
        public string MetadataString { get; set; }

        [DataMember]
        public DateTimeOffset LastUpdatedDate { get; set; }

        [DataMember]
        public double AverageRating { get; set; }

        public StoreListingModel()
        {
            this.Tags = new List<string>();
        }

        [JsonIgnore]
        public JObject Metadata
        {
            get { return (!string.IsNullOrEmpty(this.MetadataString)) ? JObject.Parse(this.MetadataString) : new JObject(); }
            set { this.MetadataString = (value != null) ? SerializerHelper.SerializeToString(value) : null; }
        }

        [JsonIgnore]
        public UserModel User { get; set; }

        [JsonIgnore]
        public bool IsCommandOwnedByUser { get { return (this.User != null && this.User.id.Equals(ChannelSession.User.id)); } }

        [JsonIgnore]
        public string UserName { get { return (this.User != null) ? this.User.username : "Unknown"; } }
        [JsonIgnore]
        public string UserAvatar { get { return (this.User != null && !string.IsNullOrEmpty(this.User.avatarUrl)) ? this.User.avatarUrl : UserViewModel.DefaultAvatarLink; } }

        [JsonIgnore]
        public string DisplayImage { get { return (!string.IsNullOrEmpty(this.DisplayImageLink)) ? this.DisplayImageLink : StoreListingModel.DefaultDisplayImage; } }

        [JsonIgnore]
        public string AverageRatingDisplayString { get { return Math.Round(this.AverageRating, 1).ToString(); } }

        [JsonIgnore]
        public string AssetsIncludedString { get { return (this.AssetsIncluded) ? "Yes" : "No"; } }

        [JsonIgnore]
        public string TagsString
        {
            get { return string.Join(",", this.Tags); }
            set { this.Tags = (!string.IsNullOrEmpty(value)) ? new List<string>(value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) : new List<string>(); }
        }

        [JsonIgnore]
        public string TagsDisplayString { get { return string.Join(", ", this.Tags); } }

        [JsonIgnore]
        public string SearchText
        {
            get
            {
                StringBuilder searchText = new StringBuilder();
                searchText.Append(this.Name + " ");
                searchText.Append(this.Description + " ");
                foreach (string tag in this.Tags)
                {
                    searchText.Append(tag + " ");
                }
                return searchText.ToString().Trim();
            }
        }

        [JsonIgnore]
        public string LastUpdatedString { get { return this.LastUpdatedDate.ToLocalTime().ToString("G"); } }

        public async Task SetUser()
        {
            if (this.UserID > 0)
            {
                this.User = await ChannelSession.Connection.GetUser(this.UserID);
            }
        }
    }
}
