using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    [DataContract]
    public class StreamElementsChannel
    {
        [DataMember]
        public string _id { get; set; }
        [DataMember]
        public string username { get; set; }
        [DataMember]
        public string alias { get; set; }
        [DataMember]
        public string displayName { get; set; }

        [DataMember]
        public string providerId { get; set; }
        [DataMember]
        public string provider { get; set; }

        [DataMember]
        public string createdAt { get; set; }
        [DataMember]
        public string updatedAt { get; set; }
    }

    [DataContract]
    public class StreamElementsTipEventItemModel
    {
        [DataMember]
        public string name { get; set; }

        [DataMember]
        public double? price { get; set; }

        [DataMember]
        public int? quantity { get; set; }
    }

    [DataContract]
    public class StreamElementsTipEventModel
    {
        public const string MerchType = "merch";

        [DataMember]
        public string type { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public double? amount { get; set; }

        [DataMember]
        public double? count { get; set; }

        [DataMember]
        public string message { get; set; }

        [DataMember]
        public bool isTest { get; set; }

        [DataMember]
        public List<StreamElementsTipEventItemModel> items { get; set; } = new List<StreamElementsTipEventItemModel>();

        public UserDonationModel ToGenericDonation()
        {
            if (string.Equals(this.type, MerchType, StringComparison.OrdinalIgnoreCase) && this.items.Count > 0)
            {
                return new UserDonationModel()
                {
                    Source = UserDonationSourceEnum.StreamElements,

                    ID = Guid.NewGuid().ToString(),
                    Username = this.name,
                    Message = this.message ?? string.Empty,

                    Amount = Math.Round(this.items.Sum(i => i.price.GetValueOrDefault() * i.quantity.GetValueOrDefault()), 2),

                    DateTime = DateTimeOffset.Now,
                };
            }
            else
            {
                return new UserDonationModel()
                {
                    Source = UserDonationSourceEnum.StreamElements,

                    ID = Guid.NewGuid().ToString(),
                    Username = this.name,
                    Message = this.message ?? string.Empty,

                    Amount = Math.Round(this.amount.GetValueOrDefault(), 2),

                    DateTime = DateTimeOffset.Now,
                };
            }
        }
    }

    [DataContract]
    public class StreamElementsWebSocketEventModel
    {
        [DataMember]
        public string listener { get; set; }

        [JsonProperty("event")]
        public JToken EventDetails { get; set; }
    }

    public interface IStreamElementsService : IOAuthExternalService
    {
        Task<StreamElementsChannel> GetCurrentChannel();
    }

    public class StreamElementsService : OAuthExternalServiceBase, IStreamElementsService
    {
        public const string TotalSpentSpecialIdentifier = "totalspent";

        private const string BaseAddress = "https://api.streamelements.com/kappa/v2/";

        private const string ClientID = "460928647d5469dd";
        private const string AuthorizationUrl = "https://api.streamelements.com/oauth2/authorize?client_id={0}&redirect_uri=http://localhost:8919/&response_type=code&state={1}&scope=tips:read";
        private const string TokenUrl = "https://api.streamelements.com/oauth2/token";

        private const string TipEvent = "tip-latest";
        private const string MerchEvent = "merch-latest";

        public bool WebSocketConnected { get; private set; }

        public event EventHandler OnWebSocketConnectedOccurred = delegate { };
        public event EventHandler OnWebSocketDisconnectedOccurred = delegate { };

        private StreamElementsChannel channel;

        private ISocketIOConnection socket;

        public StreamElementsService(ISocketIOConnection socket)
            : base(StreamElementsService.BaseAddress)
        {
            this.socket = socket;
        }

        public override string Name { get { return "StreamElements"; } }

        public override async Task<Result> Connect()
        {
            try
            {
                string authorizationCode = await this.ConnectViaOAuthRedirect(string.Format(StreamElementsService.AuthorizationUrl, StreamElementsService.ClientID, Guid.NewGuid().ToString()));
                if (!string.IsNullOrEmpty(authorizationCode))
                {
                    string clientSecret = ChannelSession.Services.Secrets.GetSecret("StreamElementsSecret");

                    List<KeyValuePair<string, string>> bodyContent = new List<KeyValuePair<string, string>>();
                    bodyContent.Add(new KeyValuePair<string, string>("grant_type", "authorization_code"));
                    bodyContent.Add(new KeyValuePair<string, string>("client_id", StreamElementsService.ClientID));
                    bodyContent.Add(new KeyValuePair<string, string>("client_secret", clientSecret));
                    bodyContent.Add(new KeyValuePair<string, string>("code", authorizationCode));
                    bodyContent.Add(new KeyValuePair<string, string>("redirect_uri", OAuthExternalServiceBase.DEFAULT_OAUTH_LOCALHOST_URL));

                    this.token = await this.GetWWWFormUrlEncodedOAuthToken(StreamElementsService.TokenUrl, StreamElementsService.ClientID, clientSecret, bodyContent);
                    if (this.token != null)
                    {
                        token.authorizationCode = authorizationCode;
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
            if (this.socket != null)
            {
                this.socket.OnDisconnected -= Socket_OnDisconnected;
                await this.socket.Disconnect();
            }
            this.WebSocketConnected = false;

            this.token = null;
        }

        public async Task<StreamElementsChannel> GetCurrentChannel()
        {
            return await this.GetAsync<StreamElementsChannel>("channels/me");
        }

        public void WebSocketConnectedOccurred()
        {
            ChannelSession.ReconnectionOccurred("StreamElements");
            this.OnWebSocketConnectedOccurred(this, new EventArgs());
        }

        public void WebSocketDisconnectedOccurred()
        {
            ChannelSession.DisconnectionOccurred("StreamElements");
            this.OnWebSocketDisconnectedOccurred(this, new EventArgs());
        }

        protected override async Task RefreshOAuthToken()
        {
            if (this.token != null)
            {
                string clientSecret = ChannelSession.Services.Secrets.GetSecret("StreamElementsSecret");

                List<KeyValuePair<string, string>> bodyContent = new List<KeyValuePair<string, string>>();
                bodyContent.Add(new KeyValuePair<string, string>("grant_type", "refresh_token"));
                bodyContent.Add(new KeyValuePair<string, string>("client_id", StreamElementsService.ClientID));
                bodyContent.Add(new KeyValuePair<string, string>("client_secret", clientSecret));
                bodyContent.Add(new KeyValuePair<string, string>("refresh_token", this.token.refreshToken));
                bodyContent.Add(new KeyValuePair<string, string>("redirect_uri", OAuthExternalServiceBase.DEFAULT_OAUTH_LOCALHOST_URL));

                this.token = await this.GetWWWFormUrlEncodedOAuthToken(StreamElementsService.TokenUrl, StreamElementsService.ClientID, clientSecret, bodyContent);
            }
        }

        protected override async Task<Result> InitializeInternal()
        {
            this.channel = await this.GetCurrentChannel();
            if (this.channel != null)
            {
                this.socket.Listen("disconnect", (data) =>
                {
                    this.WebSocketDisconnectedOccurred();
                });

                this.socket.Listen("authenticated", (data) =>
                {
                    if (data != null)
                    {
                        JObject eventJObj = JObject.Parse(data.ToString());
                        var channelId = eventJObj["channelId"]?.Value<string>();
                        if (this.channel._id.Equals(channelId))
                        {
                            this.WebSocketConnected = true;
                        }
                    }
                });

                this.socket.Listen("event", async (data) =>
                {
                    try
                    {
                        if (data != null)
                        {
                            StreamElementsWebSocketEventModel e = JSONSerializerHelper.DeserializeFromString<StreamElementsWebSocketEventModel>(data.ToString());
                            if (e.EventDetails != null && e.EventDetails != null)
                            {
                                if (string.Equals(e.listener, StreamElementsService.TipEvent, StringComparison.OrdinalIgnoreCase))
                                {
                                    StreamElementsTipEventModel tipEvent = e.EventDetails.ToObject<StreamElementsTipEventModel>();
                                    if (tipEvent.amount.GetValueOrDefault() > 0)
                                    {
                                        await EventService.ProcessDonationEvent(EventTypeEnum.StreamElementsDonation, tipEvent.ToGenericDonation());
                                    }
                                }
                                else if (string.Equals(e.listener, StreamElementsService.MerchEvent, StringComparison.OrdinalIgnoreCase))
                                {
                                    StreamElementsTipEventModel tipEvent = e.EventDetails.ToObject<StreamElementsTipEventModel>();
                                    if (tipEvent.items.Count > 0)
                                    {
                                        List<string> arguments = new List<string>(tipEvent.items.Select(i => $"{i.name} x{i.quantity.GetValueOrDefault()}"));
                                        await EventService.ProcessDonationEvent(EventTypeEnum.StreamElementsMerchPurchase, tipEvent.ToGenericDonation(), arguments: arguments);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                    }
                });

                await this.socket.Connect($"wss://realtime.streamelements.com");
                this.socket.OnDisconnected += Socket_OnDisconnected;

                JObject packet = new JObject();
                packet["method"] = "oauth2";
                packet["token"] = this.token.accessToken;
                this.socket.Send("authenticate", packet);

                for (int i = 0; i < 10 && !this.WebSocketConnected; i++)
                {
                    await Task.Delay(1000);
                }

                this.TrackServiceTelemetry("Streamlabs");

                if (this.WebSocketConnected)
                {
                    this.WebSocketConnectedOccurred();
                    return new Result();
                }
                return new Result(Resources.StreamElementsSocketFailed);
            }
            return new Result(Resources.StreamElementsUserDataFailed);
        }

        protected override async Task<AdvancedHttpClient> GetHttpClient(bool autoRefreshToken = true)
        {
            AdvancedHttpClient client = await base.GetHttpClient(autoRefreshToken);
            if (this.token != null)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", this.token.accessToken);
            }
            return client;
        }

        private void Socket_OnDisconnected(object sender, EventArgs e)
        {
            this.WebSocketDisconnectedOccurred();
        }
    }
}
