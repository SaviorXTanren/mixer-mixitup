using Mixer.Base.Model.OAuth;
using MixItUp.Base.Model.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public class GawkBoxAlert
    {
        [JsonProperty("creatorid")]
        public string CreatorID { get; set; }

        [JsonProperty("userid")]
        public string UserID { get; set; }
        [JsonProperty("username")]
        public string UserName { get; set; }

        [JsonProperty("totalamount")]
        public double TotalAmount { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("SubscribeURL")]
        public string SubscribeURL { get; set; }

        [JsonProperty("gifts")]
        public List<GawkBoxGift> Gifts { get; set; }

        public UserDonationModel ToGenericDonation()
        {
            return new UserDonationModel()
            {
                Source = UserDonationSourceEnum.GawkBox,

                ID = this.CreatorID.ToString(),
                UserName = this.UserName,
                Message = this.Message,
                ImageLink = this.Gifts.FirstOrDefault().ImageLink,

                Amount = Math.Round(this.TotalAmount, 2),
                AmountText = string.Format("{0:C}", this.TotalAmount),

                DateTime = DateTimeOffset.Now,
            };
        }
    }

    public class GawkBoxGift
    {
        [JsonProperty("giftid")]
        public string GiftID { get; set; }
        [JsonProperty("amount")]
        public double Amount { get; set; }
        [JsonProperty("asset")]
        public string ImageLink { get; set; }
    }

    public interface IGawkBoxService
    {
        Task<bool> Connect();

        Task Disconnect();

        OAuthTokenModel GetOAuthTokenCopy();
    }
}
