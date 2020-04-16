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
        [JsonProperty("donation_id")]
        public int ID { get; set; }

        [JsonProperty("name")]
        public string UserName { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("amount")]
        public string AmountString { get; set; }

        [JsonProperty("created_at")]
        public long CreatedAt { get; set; }

        [JsonIgnore]
        public double Amount
        {
            get
            {
                if (this.AmountString.ParseCurrency(out double result))
                {
                    return result;
                }
                return 0;
            }
        }

        public StreamlabsDonation()
        {
            this.CreatedAt = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public UserDonationModel ToGenericDonation()
        {
            return new UserDonationModel()
            {
                Source = UserDonationSourceEnum.Streamlabs,

                ID = this.ID.ToString(),
                Username = this.UserName,
                Message = this.Message,

                Amount = Math.Round(this.Amount, 2),

                DateTime = StreamingClient.Base.Util.DateTimeOffsetExtensions.FromUTCUnixTimeSeconds(this.CreatedAt),
            };
        }
    }

    public interface IStreamlabsService : IOAuthExternalService
    {
        Task<IEnumerable<StreamlabsDonation>> GetDonations(int maxAmount = 1);

        Task SpinWheel();

        Task EmptyJar();

        Task RollCredits();
    }

    public class StreamlabsService : OAuthExternalServiceBase, IStreamlabsService
    {
        private const string BaseAddress = "https://streamlabs.com/api/v1.0/";

        private const string ClientID = "ioEmsqlMK8jj0NuJGvvQn4ijp8XkyJ552VJ7MiDX";
        private const string AuthorizationUrl = "https://www.streamlabs.com/api/v1.0/authorize?client_id={0}&redirect_uri=http://localhost:8919/&response_type=code&scope=donations.read+socket.token+points.read+alerts.create+jar.write+wheel.write+credits.write";

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private DateTimeOffset startTime;
        private Dictionary<int, StreamlabsDonation> donationsReceived = new Dictionary<int, StreamlabsDonation>();

        public StreamlabsService() : base(StreamlabsService.BaseAddress) { }

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

        public override Task Disconnect()
        {
            this.cancellationTokenSource.Cancel();
            this.token = null;
            return Task.FromResult(0);
        }

        public async Task<IEnumerable<StreamlabsDonation>> GetDonations(int maxAmount = 1)
        {
            List<StreamlabsDonation> results = new List<StreamlabsDonation>();
            try
            {
                int lastID = 0;
                while (results.Count < maxAmount)
                {
                    string beforeFilter = string.Empty;
                    if (lastID > 0)
                    {
                        beforeFilter = "?before=" + lastID;
                    }

                    HttpResponseMessage response = await this.GetAsync("donations" + beforeFilter);
                    JObject jobj = await response.ProcessJObjectResponse();
                    JArray data = (JArray)jobj["data"];

                    if (data.Count == 0)
                    {
                        break;
                    }

                    foreach (var d in data)
                    {
                        StreamlabsDonation donation = d.ToObject<StreamlabsDonation>();
                        lastID = donation.ID;
                        results.Add(donation);
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return results;
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

        protected override Task<Result> InitializeInternal()
        {
            this.cancellationTokenSource = new CancellationTokenSource();

            this.startTime = DateTimeOffset.Now;

            AsyncRunner.RunBackgroundTask(this.cancellationTokenSource.Token, 60000, this.BackgroundDonationCheck);

            return Task.FromResult(new Result());
        }

        private async Task BackgroundDonationCheck(CancellationToken token)
        {
            foreach (StreamlabsDonation slDonation in await this.GetDonations())
            {
                if (!donationsReceived.ContainsKey(slDonation.ID))
                {
                    donationsReceived[slDonation.ID] = slDonation;
                    UserDonationModel donation = slDonation.ToGenericDonation();
                    if (donation.DateTime > this.startTime)
                    {
                        await EventService.ProcessDonationEvent(EventTypeEnum.StreamlabsDonation, donation);
                    }
                }
            }
        }

        protected override void DisposeInternal()
        {
            this.cancellationTokenSource.Dispose();
        }
    }
}
