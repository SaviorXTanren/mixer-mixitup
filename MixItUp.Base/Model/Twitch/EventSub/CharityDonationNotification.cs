using MixItUp.Base.Model.User;
using Newtonsoft.Json.Linq;
using System;

namespace MixItUp.Base.Model.Twitch.EventSub
{
    public class CharityDonationNotification
    {
        public string UserID { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }

        public string CharityName { get; set; }
        public string CharityImage { get; set; }

        public double Amount { get; set; }
        public double AmountDecimalPlaces { get; set; }

        public CharityDonationNotification(JObject payload)
        {
            this.UserID = payload["user_id"].Value<string>();
            this.Username = payload["user_login"].Value<string>();
            this.DisplayName = payload["user_name"].Value<string>();

            this.CharityName = payload["charity_name"].Value<string>();
            this.CharityImage = payload["charity_logo"].Value<string>();

            JObject donationAmountJObj = payload["amount"] as JObject;
            if (donationAmountJObj != null)
            {
                this.Amount = donationAmountJObj["value"].Value<int>();
                this.AmountDecimalPlaces = donationAmountJObj["decimal_places"].Value<int>();
                if (this.AmountDecimalPlaces > 0)
                {
                    this.Amount = this.Amount / Math.Pow(10, this.AmountDecimalPlaces);
                }
            }
        }

        public UserDonationModel ToGenericDonation()
        {
            return new UserDonationModel()
            {
                Source = UserDonationSourceEnum.Twitch,
                Platform = StreamingPlatformTypeEnum.Twitch,

                ID = Guid.NewGuid().ToString(),
                Username = this.Username,

                Amount = this.Amount,

                DateTime = DateTimeOffset.Now,
            };
        }
    }
}
