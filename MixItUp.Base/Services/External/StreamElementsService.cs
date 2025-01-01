using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
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
        [DataMember]
        public string tipId { get; set; }

        [DataMember]
        public string username { get; set; }

        [DataMember]
        public double? amount { get; set; }

        [DataMember]
        public string currency { get; set; }

        [DataMember]
        public double? count { get; set; }

        [DataMember]
        public string message { get; set; }

        [DataMember]
        public List<StreamElementsTipEventItemModel> items { get; set; } = new List<StreamElementsTipEventItemModel>();

        public UserDonationModel ToGenericDonation()
        {
            if (this.items.Count > 0)
            {
                double amount = this.items.Sum(i => i.price.GetValueOrDefault() * i.quantity.GetValueOrDefault());
                if (amount == 0 && this.amount != null && this.amount > 0)
                {
                    amount = this.amount.GetValueOrDefault();
                }

                return new UserDonationModel()
                {
                    Source = UserDonationSourceEnum.StreamElements,

                    ID = Guid.NewGuid().ToString(),
                    Username = this.username,
                    Message = this.message ?? string.Empty,

                    Amount = Math.Round(amount, 2),

                    DateTime = DateTimeOffset.Now,
                };
            }
            else
            {
                return new UserDonationModel()
                {
                    Source = UserDonationSourceEnum.StreamElements,

                    ID = this.tipId.ToString(),
                    Username = this.username,
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
        public const string TipEvent = "tip";
        public const string MerchEvent = "merch";

        [DataMember]
        public string _id { get; set; }

        [DataMember]
        public string channel { get; set; }

        [DataMember]
        public string type { get; set; }

        [DataMember]
        public string provider { get; set; }

        [DataMember]
        public string createdAt { get; set; }

        [DataMember]
        public string updatedAt { get; set; }

        [DataMember]
        public JToken data { get; set; }
    }

    public class StreamElementsService : OAuthExternalServiceBase
    {
        public const string TotalSpentSpecialIdentifier = "totalspent";

        private const string BaseAddress = "https://api.streamelements.com/kappa/v2/";

        private const string ClientID = "460928647d5469dd";
        private const string AuthorizationUrl = "https://api.streamelements.com/oauth2/authorize?client_id={0}&redirect_uri=http://localhost:8919/&response_type=code&state={1}&scope=tips:read";
        private const string TokenUrl = "https://api.streamelements.com/oauth2/token";

        private const string MerchAllItemsSpecialIdentifier = "allitems";
        private const string MerchTotalItemsSpecialIdentifier = "totalitems";

        public bool WebSocketConnected { get; private set; }

        private HashSet<string> donationsProcessed = new HashSet<string>();

        private StreamElementsChannel channel;

        private ISocketIOConnection socket;

        public StreamElementsService(ISocketIOConnection socket)
            : base(StreamElementsService.BaseAddress)
        {
            this.socket = socket;
        }

        public override string Name { get { return MixItUp.Base.Resources.StreamElements; } }

        public override async Task<Result> Connect()
        {
            try
            {
                string authorizationCode = await this.ConnectViaOAuthRedirect(string.Format(StreamElementsService.AuthorizationUrl, StreamElementsService.ClientID, Guid.NewGuid().ToString()));
                if (!string.IsNullOrEmpty(authorizationCode))
                {
                    string clientSecret = ServiceManager.Get<SecretsService>().GetSecret("StreamElementsSecret");

                    List<KeyValuePair<string, string>> bodyContent = new List<KeyValuePair<string, string>>();
                    bodyContent.Add(new KeyValuePair<string, string>("grant_type", "authorization_code"));
                    bodyContent.Add(new KeyValuePair<string, string>("client_id", StreamElementsService.ClientID));
                    bodyContent.Add(new KeyValuePair<string, string>("client_secret", clientSecret));
                    bodyContent.Add(new KeyValuePair<string, string>("code", authorizationCode));
                    bodyContent.Add(new KeyValuePair<string, string>("redirect_uri", OAuthExternalServiceBase.DEFAULT_OAUTH_LOCALHOST_URL));

                    this.token = await this.GetWWWFormUrlEncodedOAuthToken(StreamElementsService.TokenUrl, StreamElementsService.ClientID, clientSecret, bodyContent);
                    if (this.token != null)
                    {
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

        protected override async Task RefreshOAuthToken()
        {
            if (this.token != null)
            {
                string clientSecret = ServiceManager.Get<SecretsService>().GetSecret("StreamElementsSecret");

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
                if (await this.ConnectSocket())
                {
                    this.TrackServiceTelemetry("StreamElements");
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

        private async void Socket_OnDisconnected(object sender, EventArgs e)
        {
            ChannelSession.DisconnectionOccurred(MixItUp.Base.Resources.StreamElements);

            do
            {
                await Task.Delay(5000);
            } while (!await this.ConnectSocket());

            ChannelSession.ReconnectionOccurred(MixItUp.Base.Resources.StreamElements);
        }

        private async Task<bool> ConnectSocket()
        {
            try
            {
                this.WebSocketConnected = false;
                this.socket.OnDisconnected -= Socket_OnDisconnected;
                await this.socket.Disconnect();

                this.socket.Listen("disconnect", (data) =>
                {
                    this.Socket_OnDisconnected(null, new EventArgs());
                });

                this.socket.Listen("authenticated", (data) =>
                {
                    try
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
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                    }
                });

                this.socket.Listen("event", async (data) =>
                {
                    try
                    {
                        if (data != null)
                        {
                            Logger.Log(LogLevel.Debug, "StreamElements event: " + data.ToString());

                            StreamElementsWebSocketEventModel e = JSONSerializerHelper.DeserializeFromString<StreamElementsWebSocketEventModel>(data.ToString());
                            if (e.type != null && e.data != null)
                            {
                                if (string.Equals(e.type, StreamElementsWebSocketEventModel.TipEvent, StringComparison.OrdinalIgnoreCase))
                                {
                                    StreamElementsTipEventModel tipEvent = e.data.ToObject<StreamElementsTipEventModel>();

                                    if (!this.donationsProcessed.Contains(tipEvent.tipId))
                                    {
                                        this.donationsProcessed.Add(tipEvent.tipId);
                                        if (tipEvent.amount.GetValueOrDefault() > 0)
                                        {
                                            await EventService.ProcessDonationEvent(EventTypeEnum.StreamElementsDonation, tipEvent.ToGenericDonation());
                                        }
                                    }
                                }
                                else if (string.Equals(e.type, StreamElementsWebSocketEventModel.MerchEvent, StringComparison.OrdinalIgnoreCase))
                                {
                                    StreamElementsTipEventModel tipEvent = e.data.ToObject<StreamElementsTipEventModel>();
                                    if (!this.donationsProcessed.Contains(tipEvent.tipId) && tipEvent.items.Count > 0)
                                    {
                                        this.donationsProcessed.Add(tipEvent.tipId);

                                        List<string> arguments = new List<string>(tipEvent.items.Select(i => $"{i.name} x{i.quantity.GetValueOrDefault()}"));
                                        Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>();
                                        specialIdentifiers[StreamElementsService.MerchAllItemsSpecialIdentifier] = string.Join(", ", arguments);
                                        specialIdentifiers[StreamElementsService.MerchTotalItemsSpecialIdentifier] = tipEvent.items.Sum(i => i.quantity.GetValueOrDefault()).ToString();
                                        await EventService.ProcessDonationEvent(EventTypeEnum.StreamElementsMerchPurchase, tipEvent.ToGenericDonation(), arguments: arguments, additionalSpecialIdentifiers: specialIdentifiers);
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

                JObject packet = new JObject();
                packet["method"] = "oauth2";
                packet["token"] = this.token.accessToken;
                this.socket.Send("authenticate", packet);

                for (int i = 0; i < 10 && !this.WebSocketConnected; i++)
                {
                    await Task.Delay(1000);
                }

                if (this.WebSocketConnected)
                {
                    this.socket.OnDisconnected += Socket_OnDisconnected;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            return this.WebSocketConnected;
        }
    }
}
