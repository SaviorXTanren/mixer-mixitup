using MixItUp.Base.Model.User;
using Newtonsoft.Json;
using StreamingClient.Base.Model.OAuth;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StreamingClient.Base.Util;

namespace MixItUp.Base.Services
{
    public class StreamlabsDonation
    {
        [JsonProperty("donation_id")]
        public int ID { get; set; }

        [JsonProperty("name")]
        public string UserName { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("amount")]
        public string AmountString { get; set; }

        [JsonProperty("created_at")]
        public long CreatedAt { get; set; }

        [JsonIgnore]
        public double Amount
        {
            get
            {
                double amount = 0;
                string amountString = this.AmountString;
                if (!double.TryParse(amountString, out amount))
                {
                    amountString = amountString.Replace(".", ",");
                    double.TryParse(amountString, out amount);
                }
                return amount;
            }
        }

        public StreamlabsDonation()
        {
            this.CreatedAt = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public UserDonationModel ToGenericDonation()
        {
            return new UserDonationModel()
            {
                Source = UserDonationSourceEnum.Streamlabs,

                ID = this.ID.ToString(),
                UserName = this.UserName,
                Message = this.Message,

                Amount = Math.Round(this.Amount, 2),

                DateTime = DateTimeOffsetExtensions.FromUTCUnixTimeSeconds(this.CreatedAt),
            };
        }
    }

    public interface IStreamlabsService
    {
        Task<bool> Connect();

        Task Disconnect();

        OAuthTokenModel GetOAuthTokenCopy();

        Task<IEnumerable<StreamlabsDonation>> GetDonations(int maxAmount = 1);

        Task SpinWheel();
        
        Task EmptyJar();

        Task RollCredits();
    }
}
