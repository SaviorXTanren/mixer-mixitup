using Mixer.Base;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Model.OAuth;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class StreamlabsDonation
    {
        [JsonProperty("_id")]
        public string ID { get; set; }

        [JsonProperty("name")]
        public string UserName { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("formatted_amount")]
        public string AmountString { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonIgnore]
        public DateTimeOffset CreatedAtDateTime { get { return DateTimeOffset.UtcNow; } }

        public StreamlabsDonation() { }

        public UserDonationModel ToGenericDonation()
        {
            return new UserDonationModel()
            {
                Source = UserDonationSourceEnum.Streamlabs,

                ID = this.ID,
                Username = this.UserName,
                Message = this.Message,

                Amount = Math.Round(this.Amount, 2),

                DateTime = this.CreatedAtDateTime,
            };
        }
    }

    public interface IStreamlabsService : IOAuthExternalService
    {
        Task SpinWheel();

        Task EmptyJar();

        Task RollCredits();
    }

    public class StreamlabsService : OAuthExternalServiceBase, IStreamlabsService
    {
        private const string BaseAddress = "https://streamlabs.com/api/v1.0/";

        private const string ClientID = "ioEmsqlMK8jj0NuJGvvQn4ijp8XkyJ552VJ7MiDX";
        private const string AuthorizationUrl = "https://www.streamlabs.com/api/v1.0/authorize?client_id={0}&redirect_uri=http://localhost:8919/&response_type=code&scope=donations.read+socket.token+points.read+alerts.create+jar.write+wheel.write+credits.write";

        private ISocketIOConnection socket;

        public StreamlabsService(ISocketIOConnection socket)
            : base(StreamlabsService.BaseAddress)
        {
            this.socket = socket;
        }

        public override string Name { get { return "Streamlabs"; } }

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
                    payload["client_secret"] = ChannelSession.Services.Secrets.GetSecret("StreamlabsSecret");
                    payload["code"] = authorizationCode;
                    payload["redirect_uri"] = MixerConnection.DEFAULT_OAUTH_LOCALHOST_URL;

                    this.token = await this.PostAsync<OAuthTokenModel>("token", AdvancedHttpClient.CreateContentFromObject(payload), autoRefreshToken: false);
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
                payload["client_secret"] = ChannelSession.Services.Secrets.GetSecret("StreamlabsSecret");
                payload["refresh_token"] = this.token.refreshToken;
                payload["redirect_uri"] = MixerConnection.DEFAULT_OAUTH_LOCALHOST_URL;

                this.token = await this.PostAsync<OAuthTokenModel>("token", AdvancedHttpClient.CreateContentFromObject(payload), autoRefreshToken: false);
            }
        }

        protected override async Task<Result> InitializeInternal()
        {
            JObject jobj = await this.GetJObjectAsync("socket/token?access_token=" + this.token.accessToken);
            if (jobj != null && jobj.ContainsKey("socket_token"))
            {
                string socketToken = jobj["socket_token"].ToString();

                await this.socket.Connect($"https://sockets.streamlabs.com", $"token={socketToken}");

                this.socket.Listen("event", async (data) =>
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
                });

                return new Result();
            }
            return new Result("Failed to get web socket token");
        }
    }
}
