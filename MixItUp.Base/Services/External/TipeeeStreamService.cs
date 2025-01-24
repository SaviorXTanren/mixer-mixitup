using MixItUp.Base.Model.User;
using MixItUp.Base.Model.Web;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
    public class TipeeeStreamUserProvider
    {
        [JsonProperty("code")]
        public string Platform { get; set; }

        [JsonProperty("id")]
        public string UserID { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }
    }

    [DataContract]
    public class TipeeeStreamUser
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("currency")]
        public JObject Currency { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("providers")]
        public List<TipeeeStreamUserProvider> Providers { get; set; } = new List<TipeeeStreamUserProvider>();

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
        public TipeeeStreamParameters Parameters { get; set; } = new TipeeeStreamParameters();

        [JsonProperty("formattedAmount")]
        public string DisplayAmount { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonIgnore]
        public double Amount
        {
            get
            {
                double result = 0;

                // Check for int value first
                if (this.Parameters.Amount != null)
                {
                    double? doubleValue = this.Parameters.Amount.Value<double?>();
                    if (doubleValue.HasValue)
                    {
                        return doubleValue.Value;
                    }

                    string stringValue = this.Parameters.Amount.Value<string>();
                    if (!string.IsNullOrEmpty(stringValue) && stringValue.ParseCurrency(out result))
                    {
                        return result;
                    }
                }

                if (!string.IsNullOrEmpty(this.DisplayAmount))
                {
                    this.DisplayAmount.ParseCurrency(out result);
                }
                return result;
            }
        }

        public UserDonationModel ToGenericDonation()
        {
            return new UserDonationModel()
            {
                Source = UserDonationSourceEnum.TipeeeStream,

                ID = this.ID.ToString(),
                Username = this.Parameters.Username,
                Message = this.Parameters.Message,

                Amount = Math.Round(this.Amount, 2),

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
        public JToken Amount { get; set; }

        [JsonProperty("fees")]
        public string Fees { get; set; }
    }

    public class TipeeeStreamService : OAuthExternalServiceBase
    {
        private const string BaseAddress = "https://api.tipeeestream.com/";

        public const string ClientID = "13420_3cfbu3ejsccgs4sg8s0sgos8cs0cwkg0k4wg80w4k08w4c08sk";

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

        public override string Name { get { return MixItUp.Base.Resources.TipeeeStream; } }

        public override async Task<Result> Connect()
        {
            try
            {
                this.authorizationToken = await this.ConnectViaOAuthRedirect(string.Format(TipeeeStreamService.AuthorizationURL, TipeeeStreamService.ClientID, TipeeeStreamService.ListeningURL));
                if (!string.IsNullOrEmpty(this.authorizationToken))
                {
                    JObject payload = new JObject();
                    payload["grant_type"] = "authorization_code";
                    payload["client_id"] = TipeeeStreamService.ClientID;
                    payload["client_secret"] = ServiceManager.Get<SecretsService>().GetSecret("TipeeeStreamSecret");
                    payload["code"] = this.authorizationToken;
                    payload["redirect_uri"] = TipeeeStreamService.ListeningURL;

                    this.token = await this.PostAsync<OAuthTokenModel>("https://api.tipeeestream.com/oauth/v2/token", AdvancedHttpClient.CreateContentFromObject(payload), autoRefreshToken: false);
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
                payload["client_secret"] = ServiceManager.Get<SecretsService>().GetSecret("TipeeeStreamSecret");
                payload["refresh_token"] = this.token.refreshToken;

                this.token = await this.PostAsync<OAuthTokenModel>("https://api.tipeeestream.com/oauth/v2/refresh-token", AdvancedHttpClient.CreateContentFromObject(payload), autoRefreshToken: false);
            }
        }

        protected override async Task<Result> InitializeInternal()
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
                            this.TrackServiceTelemetry("TipeeeStream");
                            return new Result();
                        }
                        return new Result(Resources.TipeeeStreamSocketFailed);
                    }
                    return new Result(Resources.TipeeeStreamSocketUrlFailed);
                }
                return new Result(Resources.TipeeStreamSocketKeyFailed);
            }
            return new Result(Resources.TipeeeStreamUserDataFailed);
        }

        private async Task<T> GetAsync<T>(string url)
        {
            HttpResponseMessage response = await this.GetAsync(url);
            return await response.ProcessResponse<T>();
        }

        public void WebSocketConnectedOccurred()
        {
            ChannelSession.ReconnectionOccurred(MixItUp.Base.Resources.TipeeeStream);
            this.OnWebSocketConnectedOccurred(this, new EventArgs());
        }

        public void WebSocketDisconnectedOccurred()
        {
            ChannelSession.DisconnectionOccurred(MixItUp.Base.Resources.TipeeeStream);
            this.OnWebSocketDisconnectedOccurred(this, new EventArgs());
        }

        public void DonationOccurred(TipeeeStreamEvent eventData)
        {
            this.OnDonationOccurred(this, eventData);
        }

        private async Task<bool> ConnectSocket()
        {
            this.socket.Listen("new-event", (data) =>
            {
                if (data != null)
                {
                    TipeeeStreamResponse response = JSONSerializerHelper.DeserializeFromString<TipeeeStreamResponse>(data.ToString());
                    if (response.Event.Type.Equals("donation"))
                    {
                        this.DonationOccurred(response.Event);
                        Task.Run(async () =>
                        {
                            await EventService.ProcessDonationEvent(EventTypeEnum.TipeeeStreamDonation, response.Event.ToGenericDonation());
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

            await this.socket.Connect(this.socketAddress + "?access_token=" + this.apiKey);

            JObject joinRoomJObj = new JObject();
            joinRoomJObj["room"] = this.apiKey;
            joinRoomJObj["username"] = this.user.Username;
            this.socket.Send("join-room", joinRoomJObj);

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
