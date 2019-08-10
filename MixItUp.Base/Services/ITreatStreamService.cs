using MixItUp.Base.Model.User;
using Newtonsoft.Json;
using StreamingClient.Base.Model.OAuth;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    [DataContract]
    public class TreatStreamEvent
    {
        [JsonProperty("sender")]
        public string Sender { get; set; }

        [JsonProperty("sender_type")]
        public string SenderType { get; set; }

        [JsonProperty("receiver")]
        public string Receiver { get; set; }

        [JsonProperty("receiver_type")]
        public string ReceiverType { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("date_created")]
        public string DateCreated { get; set; }

        public TreatStreamEvent() { }

        public UserDonationModel ToGenericDonation()
        {
            return new UserDonationModel()
            {
                Source = UserDonationSourceEnum.TreatStream,

                ID = Guid.NewGuid().ToString(),
                UserName = this.Sender,
                Type = this.Title,
                Message = this.Message,

                Amount = 0,

                DateTime = DateTimeOffset.Now,
            };
        }
    }

    public interface ITreatStreamService
    {
        bool WebSocketConnectedAndAuthenticated { get; }

        event EventHandler OnWebSocketConnectedOccurred;
        event EventHandler OnWebSocketDisconnectedOccurred;

        event EventHandler<TreatStreamEvent> OnDonationOccurred;

        Task<bool> Connect();
        Task Disconnect();

        Task<string> GetSocketToken();

        Task GetTreats();

        OAuthTokenModel GetOAuthTokenCopy();
    }
}
