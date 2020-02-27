using Mixer.Base;
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
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
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
                Username = this.Name,
                Message = this.Message,

                Amount = Math.Round(this.Amount, 2),

                DateTime = this.CreatedAt,
            };
        }
    }

    public interface IStreamJarService : IOAuthExternalService
    {
        Task<StreamJarChannel> GetChannel();

        Task<IEnumerable<StreamJarDonation>> GetDonations();
    }

    public class StreamJarService : OAuthExternalServiceBase, IStreamJarService
    {
        private const string BaseAddress = "https://jar.streamjar.tv/v2/";

        private const string ClientID = "0ff4b414d6ec2296b824cd8a11ff75ff";
        private const string AuthorizationUrl = "https://control.streamjar.tv/oauth/authorize?response_type=code&client_id={0}&redirect_uri={1}&scopes=channel:tips:view";
        private const string TokenUrl = "https://jar.streamjar.tv/v2/oauth/authorize";

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private StreamJarChannel channel;

        private Dictionary<int, StreamJarDonation> donationsReceived = new Dictionary<int, StreamJarDonation>();

        public StreamJarService() : base(StreamJarService.BaseAddress) { }

        public override string Name { get { return "StreamJar"; } }

        public override async Task<Result> Connect()
        {
            try
            { 
                string authorizationCode = await this.ConnectViaOAuthRedirect(string.Format(StreamJarService.AuthorizationUrl, StreamJarService.ClientID, MixerConnection.DEFAULT_OAUTH_LOCALHOST_URL));
                if (!string.IsNullOrEmpty(authorizationCode))
                {
                    JObject payload = new JObject();
                    payload["grant_type"] = "authorization_code";
                    payload["client_id"] = StreamJarService.ClientID;
                    payload["code"] = authorizationCode;
                    payload["redirect_uri"] = MixerConnection.DEFAULT_OAUTH_LOCALHOST_URL;

                    this.token = await this.PostAsync<OAuthTokenModel>(StreamJarService.TokenUrl, AdvancedHttpClient.CreateContentFromObject(payload), autoRefreshToken: false);
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

        public override Task Disconnect()
        {
            this.cancellationTokenSource.Cancel();
            this.token = null;
            return Task.FromResult(0);
        }

        public async Task<StreamJarChannel> GetChannel()
        {
            try
            {
                JArray jarray = await this.GetAsync<JArray>("channels");
                if (jarray != null && jarray.Count > 0)
                {
                    return jarray.First.ToObject<StreamJarChannel>();
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<IEnumerable<StreamJarDonation>> GetDonations()
        {
            try
            {
                return await this.GetAsync<IEnumerable<StreamJarDonation>>(string.Format("channels/{0}/tips", this.channel.ID.ToString()));
            }
            catch (Exception ex) { Logger.Log(ex); }
            return new List<StreamJarDonation>();
        }

        protected override async Task RefreshOAuthToken()
        {
            if (this.token != null)
            {
                JObject payload = new JObject();
                payload["grant_type"] = "refresh_token";
                payload["client_id"] = StreamJarService.ClientID;
                payload["refresh_token"] = this.token.refreshToken;
                payload["redirect_uri"] = MixerConnection.DEFAULT_OAUTH_LOCALHOST_URL;

                this.token = await this.PostAsync<OAuthTokenModel>(StreamJarService.TokenUrl, AdvancedHttpClient.CreateContentFromObject(payload), autoRefreshToken: false);
            }
        }

        protected override async Task<Result> InitializeInternal()
        {
            this.cancellationTokenSource = new CancellationTokenSource();

            this.channel = await this.GetChannel();
            if (this.channel != null)
            {
                foreach (StreamJarDonation donation in await this.GetDonations())
                {
                    donationsReceived[donation.ID] = donation;
                }

                AsyncRunner.RunBackgroundTask(this.cancellationTokenSource.Token, 30000, this.BackgroundDonationCheck);

                return new Result();
            }
            return new Result("Failed to get channel data");
        }

        private async Task BackgroundDonationCheck(CancellationToken token)
        {
            foreach (StreamJarDonation sjDonation in await this.GetDonations())
            {
                if (!donationsReceived.ContainsKey(sjDonation.ID))
                {
                    donationsReceived[sjDonation.ID] = sjDonation;
                    await ChannelSession.Services.Events.PerformEvent(await EventService.ProcessDonationEvent(EventTypeEnum.StreamJarDonation, sjDonation.ToGenericDonation()));
                }
            }
        }

        protected override void DisposeInternal()
        {
            this.cancellationTokenSource.Dispose();
        }
    }
}
