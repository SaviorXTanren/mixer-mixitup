using Mixer.Base.Model.User;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

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
        public uint UserID { get; set; }

        [DataMember]
        public int Rating { get; set; }
        [DataMember]
        public string Review { get; set; }

        [DataMember]
        public DateTimeOffset LastUpdatedDate { get; set; }

        public StoreListingReviewModel() { }

        public StoreListingReviewModel(StoreListingModel listing, int rating, string review)
        {
            this.ListingID = listing.ID;
            this.UserID = ChannelSession.MixerStreamerUser.id;
            this.Rating = rating;
            this.Review = review;
        }

        [JsonIgnore]
        public UserModel User { get; set; }

        [JsonIgnore]
        public string UserName { get { return (this.User != null) ? this.User.username : "Unknown"; } }
        [JsonIgnore]
        public string UserAvatar { get { return (this.User != null && !string.IsNullOrEmpty(this.User.avatarUrl)) ? this.User.avatarUrl : UserViewModel.DefaultAvatarLink; } }

        [JsonIgnore]
        public string LastUpdatedDateString { get { return this.LastUpdatedDate.ToLocalTime().ToString("G"); } }

        public async Task SetUser()
        {
            if (this.UserID > 0)
            {
                this.User = await ChannelSession.MixerStreamerConnection.GetUser(this.UserID);
            }
        }
    }
}
