using MixItUp.Base.Model.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Model.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    [DataContract]
    public class TipeeeStreamResponse
    {
        [JsonProperty("appKey")]
        public string AppKey { get; set; }

        [JsonProperty("event")]
        public TipeeeStreamEvent Event { get; set; }
    }

    [DataContract]
    public class TipeeeStreamUser
    {
        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("currency")]
        public JObject Currency { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("providers")]
        public JArray Providers { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedDate { get; set; }

        [JsonProperty("session_at")]
        public DateTimeOffset SessionDate { get; set; }
    }

    [DataContract]
    public class TipeeeStreamEvent
    {
        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("user")]
        public TipeeeStreamUser User { get; set; }

        [JsonProperty("parameters")]
        public TipeeeStreamParameters Parameters { get; set; }

        [JsonProperty("parameters.amount")]
        public string Amount { get; set; }

        [JsonProperty("formattedAmount")]
        public string FormattedAmount { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        public UserDonationModel ToGenericDonation()
        {
            if (!double.TryParse(this.Parameters.Amount, out double amount))
            {
                if (!double.TryParse(this.Amount, out amount))
                {
                    string textAmount = string.Concat(this.FormattedAmount.ToCharArray().Where(c => char.IsDigit(c) || c == '.'));
                    double.TryParse(textAmount, out amount);
                }
            }

            return new UserDonationModel()
            {
                Source = UserDonationSourceEnum.TipeeeStream,

                ID = this.ID.ToString(),
                UserName = this.Parameters.Username,
                Message = this.Parameters.Message,

                Amount = Math.Round(amount, 2),

                DateTime = DateTimeOffset.Now,
            };
        }
    }

    [DataContract]
    public class TipeeeStreamParameters
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("fees")]
        public string Fees { get; set; }
    }

    public interface ITipeeeStreamService
    {
        bool WebSocketConnectedAndAuthenticated { get; }

        event EventHandler OnWebSocketConnectedOccurred;
        event EventHandler OnWebSocketDisconnectedOccurred;

        event EventHandler<TipeeeStreamEvent> OnDonationOccurred;

        Task<bool> Connect();
        Task Disconnect();

        Task<TipeeeStreamUser> GetUser();
        Task<string> GetAPIKey();
        Task<string> GetSocketAddress();

        Task<IEnumerable<TipeeeStreamEvent>> GetDonationEvents();

        OAuthTokenModel GetOAuthTokenCopy();
    }
}
