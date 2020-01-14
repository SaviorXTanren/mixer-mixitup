using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Model.OAuth;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
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

    public interface ITipeeeStreamService : IOAuthExternalService
    {
        bool WebSocketConnected { get; }

        event EventHandler OnWebSocketConnectedOccurred;
        event EventHandler OnWebSocketDisconnectedOccurred;

        event EventHandler<TipeeeStreamEvent> OnDonationOccurred;

        Task<TipeeeStreamUser> GetUser();
        Task<string> GetAPIKey();
        Task<string> GetSocketAddress();

        Task<IEnumerable<TipeeeStreamEvent>> GetDonationEvents();
    }

    public class TipeeeStreamService : OAuthExternalServiceBase, ITipeeeStreamService
    {
        private const string BaseAddress = "https://api.tipeeestream.com/";

        public const string ClientID = "9611_u5i668t3urk0wcksc84kcgsgckc04wk4ookw0so04kkwgw0cg";

        public const string ListeningURL = "http://localhost:8919";

        public const string AuthorizationURL = "https://api.tipeeestream.com/oauth/v2/auth?client_id={0}&response_type=code&redirect_uri={1}&state=abc123";
        public const string OAuthTokenURL = "https://api.tipeeestream.com/oauth/v2/token";

        public event EventHandler OnWebSocketConnectedOccurred = delegate { };
        public event EventHandler OnWebSocketDisconnectedOccurred = delegate { };

        public event EventHandler<TipeeeStreamEvent> OnDonationOccurred = delegate { };

        public bool WebSocketConnected { get; private set; }

        private string authorizationToken;

        private ISocketIOConnection socket;

        private TipeeeStreamUser user;
        private string apiKey;
        private string socketAddress;

        public TipeeeStreamService(ISocketIOConnection socket)
            : base(TipeeeStreamService.BaseAddress)
        {
            this.socket = socket;
        }

        public override string Name { get { return "TipeeeStream"; } }

        public override async Task<ExternalServiceResult> Connect()
        {
            try
            {
                this.authorizationToken = await this.ConnectViaOAuthRedirect(string.Format(TipeeeStreamService.AuthorizationURL, TipeeeStreamService.ClientID, TipeeeStreamService.ListeningURL));
                if (!string.IsNullOrEmpty(this.authorizationToken))
                {
                    JObject payload = new JObject();
                    payload["grant_type"] = "authorization_code";
                    payload["client_id"] = TipeeeStreamService.ClientID;
                    payload["client_secret"] = ChannelSession.SecretManager.GetSecret("TipeeeStreamSecret");
                    payload["code"] = this.authorizationToken;
                    payload["redirect_uri"] = TipeeeStreamService.ListeningURL;

                    this.token = await this.PostAsync<OAuthTokenModel>("https://api.tipeeestream.com/oauth/v2/token", AdvancedHttpClient.CreateContentFromObject(payload), autoRefreshToken: false);
                    if (this.token != null)
                    {
                        token.authorizationCode = this.authorizationToken;
                        token.AcquiredDateTime = DateTimeOffset.Now;
                        token.expiresIn = int.MaxValue;

                        return await this.InitializeInternal();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new ExternalServiceResult(ex);
            }
            return new ExternalServiceResult(false);
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

        public async Task<TipeeeStreamUser> GetUser()
        {
            try
            {
                return await this.GetAsync<TipeeeStreamUser>("v1.0/me");
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<string> GetAPIKey()
        {
            try
            {
                JObject jobj = await this.GetAsync<JObject>("v1.0/me/api");
                if (jobj != null)
                {
                    return jobj["apiKey"].ToString();
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<string> GetSocketAddress()
        {
            try
            {
                JObject jobj = await this.GetAsync<JObject>("v2.0/site/socket");
                if (jobj != null && jobj.ContainsKey("datas"))
                {
                    return string.Format("{0}:{1}", jobj["datas"]["host"], jobj["datas"]["port"]);
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<IEnumerable<TipeeeStreamEvent>> GetDonationEvents()
        {
            List<TipeeeStreamEvent> results = new List<TipeeeStreamEvent>();
            try
            {
                JObject jobj = await this.GetAsync<JObject>("v1.0/events?type[]=donation");
                if (jobj != null && jobj.ContainsKey("datas"))
                {
                    JObject data = (JObject)jobj["datas"];
                    if (data.ContainsKey("items"))
                    {
                        foreach (JObject donation in (JArray)data["items"])
                        {
                            results.Add(donation.ToObject<TipeeeStreamEvent>());
                        }
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return results;
        }

        protected override async Task RefreshOAuthToken()
        {
            if (this.token != null)
            {
                JObject payload = new JObject();
                payload["client_id"] = TipeeeStreamService.ClientID;
                payload["client_secret"] = ChannelSession.SecretManager.GetSecret("TipeeeStreamSecret");
                payload["refresh_token"] = this.token.refreshToken;

                this.token = await this.PostAsync<OAuthTokenModel>("https://api.tipeeestream.com/oauth/v2/refresh-token", AdvancedHttpClient.CreateContentFromObject(payload), autoRefreshToken: false);
            }
        }

        protected override async Task<ExternalServiceResult> InitializeInternal()
        {
            this.user = await this.GetUser();
            if (this.user != null)
            {
                this.apiKey = await this.GetAPIKey();
                if (!string.IsNullOrEmpty(this.apiKey))
                {
                    this.socketAddress = await this.GetSocketAddress();
                    if (!string.IsNullOrEmpty(this.socketAddress))
                    {
                        if (await this.ConnectSocket())
                        {
                            return new ExternalServiceResult();
                        }
                        return new ExternalServiceResult("Failed to connect to socket");
                    }
                    return new ExternalServiceResult("Unable to get Socket URL address");
                }
                return new ExternalServiceResult("Unable to get Socket API key");
            }
            return new ExternalServiceResult("Unable to get User information");
        }

        private new async Task<T> GetAsync<T>(string url)
        {
            HttpResponseMessage response = await this.GetAsync(url);
            Logger.Log(LogLevel.Debug, string.Format("TipeeeStream Log: {0} - {1} - {2}", response.RequestMessage.ToString(), response.StatusCode, await response.Content.ReadAsStringAsync()));
            return await response.ProcessResponse<T>();
        }

        public void WebSocketConnectedOccurred()
        {
            ChannelSession.ReconnectionOccurred("TipeeeStream");
            this.OnWebSocketConnectedOccurred(this, new EventArgs());
        }

        public void WebSocketDisconnectedOccurred()
        {
            ChannelSession.DisconnectionOccurred("TipeeeStream");
            this.OnWebSocketDisconnectedOccurred(this, new EventArgs());
        }

        public void DonationOccurred(TipeeeStreamEvent eventData)
        {
            this.OnDonationOccurred(this, eventData);
        }

        private async Task<bool> ConnectSocket()
        {
            this.socket.Listen("connect", (data) =>
            {
                JObject joinRoomJObj = new JObject();
                joinRoomJObj["room"] = this.apiKey;
                joinRoomJObj["username"] = this.user.Username;
                this.socket.Send("join-room", joinRoomJObj);

                this.WebSocketConnected = true;
            });

            this.socket.Listen("new-event", (data) =>
            {
                if (data != null)
                {
                    TipeeeStreamResponse response = SerializerHelper.DeserializeFromString<TipeeeStreamResponse>(data.ToString());
                    if (response.Event.Type.Equals("donation"))
                    {
                        this.DonationOccurred(response.Event);
                        Task.Run(async () =>
                        {
                            UserDonationModel donation = response.Event.ToGenericDonation();
                            await EventCommand.ProcessDonationEventCommand(donation, OtherEventTypeEnum.TipeeeStreamDonation);
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

                await this.Connect();
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

            await this.socket.Connect(this.socketAddress, "access_token=" + this.apiKey);

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
