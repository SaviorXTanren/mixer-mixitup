using MixItUp.Base.Model;
using MixItUp.Base.Model.User;
using MixItUp.Base.Model.Web;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
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
            StreamingPlatformTypeEnum platform = StreamingPlatformTypeEnum.None;
            StreamingPlatforms.ForEachPlatform(p =>
            {
                if (string.Equals(p.ToString(), this.SenderType, StringComparison.InvariantCultureIgnoreCase))
                {
                    platform = p;
                }
            });

            return new UserDonationModel()
            {
                Source = UserDonationSourceEnum.TreatStream,

                ID = Guid.NewGuid().ToString(),
                Platform = platform,
                Username = this.Sender,
                Type = this.Title,
                Message = this.Message,

                Amount = 0,

                DateTime = DateTimeOffset.Now,
            };
        }
    }

    public class TreatStreamService : OAuthExternalServiceBase
    {
        private const string BaseAddress = "https://treatstream.com/api/";

        public const string ClientID = "n3x0h4lmr0sayf23f9rxtaqghbikvlszv3psgzbt";

        public const string ListeningURL = "http://localhost:8919";

        public const string AuthorizationURL = "https://treatstream.com/Oauth2/Authorize?response_type=code&client_id={0}&redirect_uri={1}&state=xyz&scope=";
        public const string OAuthTokenURL = "https://treatstream.com/Oauth2/Authorize/token";
        public const string RefreshTokenURL = "https://treatstream.com/Oauth2/RefreshToken";

        public event EventHandler OnWebSocketConnectedOccurred = delegate { };
        public event EventHandler OnWebSocketDisconnectedOccurred = delegate { };

        public event EventHandler<TreatStreamEvent> OnDonationOccurred = delegate { };

        public bool WebSocketConnected { get; private set; }

        private string authorizationToken;

        private ISocketIOConnection socket;
        private string socketToken;

        public TreatStreamService(ISocketIOConnection socket)
            : base(TreatStreamService.BaseAddress)
        {
            this.socket = socket;
        }

        public override string Name { get { return MixItUp.Base.Resources.TreatStream; } }

        public override async Task<Result> Connect()
        {
            try
            {
                this.authorizationToken = await this.ConnectViaOAuthRedirect(string.Format(TreatStreamService.AuthorizationURL, TreatStreamService.ClientID, TreatStreamService.ListeningURL));
                if (!string.IsNullOrEmpty(this.authorizationToken))
                {
                    JObject payload = new JObject();
                    payload["grant_type"] = "authorization_code";
                    payload["client_id"] = TreatStreamService.ClientID;
                    payload["client_secret"] = ServiceManager.Get<SecretsService>().GetSecret("TreatStreamSecret");
                    payload["code"] = this.authorizationToken;
                    payload["redirect_uri"] = TreatStreamService.ListeningURL;
                    payload["scope"] = "userinfo";

                    this.token = await this.PostAsync<OAuthTokenModel>(TreatStreamService.OAuthTokenURL, AdvancedHttpClient.CreateContentFromObject(payload), autoRefreshToken: false);
                    if (this.token != null)
                    {
                        token.expiresIn = int.MaxValue;

                        return await this.InitializeInternal();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result(ex);
            }
            return new Result(false);
        }

        public override async Task Disconnect()
        {
            this.token = null;
            if (this.WebSocketConnected)
            {
                this.socket.Send("disconnect", null);
                await this.socket.Disconnect();
                this.WebSocketConnected = false;
            }
        }

        public async Task<string> GetSocketToken()
        {
            try
            {
                JObject payload = new JObject();
                payload["client_id"] = TreatStreamService.ClientID;
                payload["access_token"] = this.token.accessToken;

                HttpContent content = AdvancedHttpClient.CreateContentFromObject(payload);
                content.Headers.Clear();
                content.Headers.Add("Content-Type", "application/json");
                JObject jobj = await this.PostAsync<JObject>("https://treatstream.com/Oauth2/Authorize/socketToken", content);
                if (jobj != null && jobj.ContainsKey("socket_token"))
                {
                    return jobj["socket_token"].ToString();
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task GetTreats()
        {
            try
            {
                string result = await this.GetStringAsync(string.Format("getMonthTreats/{0}", this.token.accessToken));
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public void WebSocketConnectedOccurred()
        {
            ChannelSession.ReconnectionOccurred(MixItUp.Base.Resources.TreatStream);
            this.OnWebSocketConnectedOccurred(this, new EventArgs());
        }

        public void WebSocketDisconnectedOccurred()
        {
            ChannelSession.DisconnectionOccurred(MixItUp.Base.Resources.TreatStream);
            this.OnWebSocketDisconnectedOccurred(this, new EventArgs());
        }

        public void DonationOccurred(TreatStreamEvent eventData)
        {
            this.OnDonationOccurred(this, eventData);
        }

        protected override async Task RefreshOAuthToken()
        {
            if (this.token != null)
            {
                JObject payload = new JObject();
                payload["client_id"] = TreatStreamService.ClientID;
                payload["client_secret"] = ServiceManager.Get<SecretsService>().GetSecret("TreatStreamSecret");
                payload["refresh_token"] = this.token.refreshToken;
                payload["grant_type"] = "refresh_token";

                this.token = await this.PostAsync<OAuthTokenModel>(TreatStreamService.RefreshTokenURL, AdvancedHttpClient.CreateContentFromObject(payload), autoRefreshToken: false);
            }
        }

        protected override async Task<Result> InitializeInternal()
        {
            this.socketToken = await this.GetSocketToken();
            if (!string.IsNullOrEmpty(socketToken))
            {
                if (await this.ConnectSocket())
                {
                    this.TrackServiceTelemetry("TreatStream");
                    return new Result();
                }
                return new Result(Resources.TreatStreamSocketFailed);
            }
            return new Result(Resources.TreatStreamSocketTokenFailed);
        }

        private async Task<bool> ConnectSocket()
        {
            this.socket.Listen("realTimeTreat", (data) =>
            {
                if (data != null)
                {
                    TreatStreamEvent tsEvent = JSONSerializerHelper.DeserializeFromString<TreatStreamEvent>(data.ToString());
                    if (tsEvent != null)
                    {
                        this.DonationOccurred(tsEvent);
                        Task.Run(async () =>
                        {
                            await EventService.ProcessDonationEvent(EventTypeEnum.TreatStreamDonation, tsEvent.ToGenericDonation());
                        });
                    }
                }
            });

            this.socket.Listen("error", async (errorData) =>
            {
                if (errorData != null)
                {
                    Logger.Log(errorData.ToString());
                }

                this.WebSocketDisconnectedOccurred();
                await this.ConnectSocket();
            });

            this.socket.Listen("disconnect", async (errorData) =>
            {
                if (errorData != null)
                {
                    Logger.Log(errorData.ToString());
                }

                this.WebSocketDisconnectedOccurred();
                await this.ConnectSocket();
            });

            await this.socket.Connect("wss://nodeapi.treatstream.com/?token=" + this.socketToken);
            this.WebSocketConnected = true;

            for (int i = 0; i < 10 && !this.WebSocketConnected; i++)
            {
                await Task.Delay(1000);
            }

            if (this.WebSocketConnected)
            {
                this.WebSocketConnectedOccurred();
                return true;
            }
            return false;
        }
    }
}
