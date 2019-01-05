using Mixer.Base.Model.OAuth;
using MixItUp.Base.Model.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    [DataContract]
    public class StreamJarChannel
    {
        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        [JsonProperty("Currency")]
        public string currency { get; set; }

        [JsonProperty("tipsEnabled")]
        public bool TipsEnabled { get; set; }

        [JsonProperty("tipsConfigured")]
        public bool TipsConfigured { get; set; }

        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }
    }

    [DataContract]
    public class StreamJarDonation
    {
        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        [JsonProperty("hidden")]
        public string Hidden { get; set; }

        [JsonProperty("tid")]
        public string TID { get; set; }

        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        public StreamJarDonation() { }

        public UserDonationModel ToGenericDonation()
        {
            return new UserDonationModel()
            {
                Source = UserDonationSourceEnum.StreamJar,

                ID = this.ID.ToString(),
                UserName = this.Name,
                Message = this.Message,

                Amount = Math.Round(this.Amount, 2),

                DateTime = this.CreatedAt,
            };
        }
    }

    public interface IStreamJarService
    {
        Task<bool> Connect();

        Task Disconnect();

        Task<StreamJarChannel> GetChannel();

        Task<IEnumerable<StreamJarDonation>> GetDonations();

        OAuthTokenModel GetOAuthTokenCopy();
    }
}
