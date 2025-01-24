using MixItUp.Base.Model.User;
using MixItUp.Base.Model.Web;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class StreamlabsDonation
    {
        [JsonProperty("_id")]
        public string ID { get; set; }

        [JsonProperty("name")]
        public string Username { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("formatted_amount")]
        public string AmountString { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("from")]
        public string FromUsername { get; set; }

        [JsonProperty("from_user_id")]
        public string FromUserID { get; set; }

        [JsonIgnore]
        public DateTimeOffset CreatedAtDateTime { get { return DateTimeOffset.UtcNow; } }

        public StreamlabsDonation() { }

        public UserDonationModel ToGenericDonation()
        {
            return new UserDonationModel()
            {
                Source = UserDonationSourceEnum.Streamlabs,

                ID = this.ID,
                Username = this.Username,
                Message = this.Message,

                Amount = Math.Round(this.Amount, 2),

                DateTime = this.CreatedAtDateTime,
            };
        }
    }

    public class StreamlabsService : OAuthExternalServiceBase
    {
        private const string BaseAddress = "https://streamlabs.com/api/v2.0/";

        private const string ClientID = "9b044090-8eb3-4293-80eb-5b2195bb881b";
        private const string AuthorizationUrl = "https://streamlabs.com/api/v2.0/authorize?client_id={0}&redirect_uri=http://localhost:8919/&response_type=code&scope=donations.read+socket.token+points.read+alerts.create+jar.write+wheel.write+credits.write&state=123456";

        private ISocketIOConnection socket;

        public StreamlabsService(ISocketIOConnection socket)
            : base(StreamlabsService.BaseAddress)
        {
            this.socket = socket;
        }

        public override string Name { get { return MixItUp.Base.Resources.Streamlabs; } }

        public override async Task<Result> Connect()
        {
            try
            {
                string authorizationCode = await this.ConnectViaOAuthRedirect(string.Format(StreamlabsService.AuthorizationUrl, StreamlabsService.ClientID));
                if (!string.IsNullOrEmpty(authorizationCode))
                {
                    JObject payload = new JObject();
                    payload["grant_type"] = "authorization_code";
                    payload["client_id"] = StreamlabsService.ClientID;
                    payload["client_secret"] = ServiceManager.Get<SecretsService>().GetSecret("StreamlabsV2Secret");
                    payload["code"] = authorizationCode;
                    payload["redirect_uri"] = OAuthExternalServiceBase.DEFAULT_OAUTH_LOCALHOST_URL;

                    this.token = await this.PostAsync<OAuthTokenModel>("token", AdvancedHttpClient.CreateContentFromObject(payload), autoRefreshToken: false);
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
            this.socket.OnDisconnected -= Socket_OnDisconnected;
            await this.socket.Disconnect();

            this.token = null;
        }

        public async Task SpinWheel()
        {
            await this.PostAsync("wheel/spin", new StringContent($"access_token={this.token.accessToken}"));
        }

        public async Task EmptyJar()
        {
            await this.PostAsync("jar/empty", new StringContent($"access_token={this.token.accessToken}"));
        }

        public async Task RollCredits()
        {
            await this.PostAsync("credits/roll", new StringContent($"access_token={this.token.accessToken}"));
        }

        protected override async Task RefreshOAuthToken()
        {
            if (this.token != null)
            {
                JObject payload = new JObject();
                payload["grant_type"] = "refresh_token";
                payload["client_id"] = StreamlabsService.ClientID;
                payload["client_secret"] = ServiceManager.Get<SecretsService>().GetSecret("StreamlabsV2Secret");
                payload["refresh_token"] = this.token.refreshToken;
                payload["redirect_uri"] = OAuthExternalServiceBase.DEFAULT_OAUTH_LOCALHOST_URL;

                this.token = await this.PostAsync<OAuthTokenModel>("token", AdvancedHttpClient.CreateContentFromObject(payload), autoRefreshToken: false);
            }
        }

        protected override async Task<Result> InitializeInternal()
        {
            if (await this.ConnectSocket())
            {
                this.TrackServiceTelemetry("Streamlabs");
                return new Result();
            }
            return new Result(Resources.StreamlabsWebSocketTokenFailed);
        }

        private async void Socket_OnDisconnected(object sender, EventArgs e)
        {
            ChannelSession.DisconnectionOccurred(MixItUp.Base.Resources.Streamlabs);

            do
            {
                await Task.Delay(5000);
            }
            while (!await this.ConnectSocket());

            ChannelSession.ReconnectionOccurred(MixItUp.Base.Resources.Streamlabs);
        }

        private async Task<bool> ConnectSocket()
        {
            try
            {
                this.socket.OnDisconnected -= Socket_OnDisconnected;
                await this.socket.Disconnect();

                JObject jobj = await this.GetJObjectAsync("socket/token?access_token=" + this.token.accessToken);
                if (jobj != null && jobj.ContainsKey("socket_token"))
                {
                    string socketToken = jobj["socket_token"].ToString();

                    this.socket.Listen("event", async (data) =>
                    {
                        try
                        {
                            if (data != null)
                            {
                                JObject eventJObj = JObject.Parse(data.ToString());
                                if (eventJObj.ContainsKey("type"))
                                {
                                    if (eventJObj["type"].Value<string>().Equals("donation", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        var messages = eventJObj["message"] as JArray;
                                        if (messages != null)
                                        {
                                            foreach (var message in messages)
                                            {
                                                StreamlabsDonation slDonation = message.ToObject<StreamlabsDonation>();
                                                await EventService.ProcessDonationEvent(EventTypeEnum.StreamlabsDonation, slDonation.ToGenericDonation());
                                            }
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

                    this.socket.OnDisconnected += Socket_OnDisconnected;
                    await this.socket.Connect($"https://sockets.streamlabs.com?token={socketToken}");

                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return false;
        }
    }
}
