using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class PulsoidHeartRate
    {
        public long measured_at { get; set; }
        public JObject data { get; set; }

        public int heart_rate
        {
            get
            {
                if (this.data != null && this.data.TryGetValue("heart_rate", out JToken value) && int.TryParse(value.ToString(), out int result))
                {
                    return result;
                }
                return 0;
            }
        }
    }

    /// <summary>
    /// https://docs.pulsoid.net
    /// </summary>
    public class PulsoidService : OAuthExternalServiceBase
    {
        public const string ClientID = "21c57a62-d890-4f3d-8879-4b1171d51858";

        public const string BaseAddress = "https://dev.pulsoid.net/api/v1/";

        public const string TokenUrl = "https://pulsoid.net/oauth2/token";

        public override string Name { get { return Resources.Pulsoid; } }

        public string AuthorizationURL { get { return $"https://pulsoid.net/oauth2/authorize?client_id={PulsoidService.ClientID}&redirect_uri={OAuthExternalServiceBase.DEFAULT_OAUTH_LOCALHOST_URL}&response_type=code&scope=data:heart_rate:read&state={Guid.NewGuid()}"; } }

        public PulsoidService() : base(PulsoidService.BaseAddress) { }

        public override async Task<Result> Connect()
        {
            try
            {
                string authorizationCode = await this.ConnectViaOAuthRedirect(this.AuthorizationURL);
                if (!string.IsNullOrEmpty(authorizationCode))
                {
                    var body = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("grant_type", "authorization_code"),
                        new KeyValuePair<string, string>("client_id", PulsoidService.ClientID),
                        new KeyValuePair<string, string>("client_secret", ServiceManager.Get<SecretsService>().GetSecret("PulsoidSecret")),
                        new KeyValuePair<string, string>("redirect_uri", OAuthExternalServiceBase.DEFAULT_OAUTH_LOCALHOST_URL),
                        new KeyValuePair<string, string>("code", authorizationCode),
                    };

                    this.token = await this.GetWWWFormUrlEncodedOAuthToken(PulsoidService.TokenUrl, body);
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
            //this.socket.OnDisconnected -= Socket_OnDisconnected;
            //await this.socket.Disconnect();

            this.token = null;
        }

        public async Task<PulsoidHeartRate> GetHeartRate()
        {
            HttpResponseMessage response = await this.GetAsync("data/heart_rate/latest");
            if (response.IsSuccessStatusCode)
            {
                return await response.ProcessResponse<PulsoidHeartRate>();
            }
            else if (response.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                return new PulsoidHeartRate();
            }
            return null;
        }

        protected override async Task<Result> InitializeInternal()
        {
            PulsoidHeartRate heartRate = await this.GetHeartRate();
            if (heartRate != null)
            {
                return new Result();
            }
            return new Result(Resources.PulsoidUnableToGetHeartRateData);
        }

        protected override async Task RefreshOAuthToken()
        {
            if (this.token != null)
            {
                var body = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("client_id", PulsoidService.ClientID),
                    new KeyValuePair<string, string>("client_secret", ServiceManager.Get<SecretsService>().GetSecret("PulsoidSecret")),
                    new KeyValuePair<string, string>("refresh_token", this.token.refreshToken),
                };

                this.token = await this.GetWWWFormUrlEncodedOAuthToken(PulsoidService.TokenUrl, body);
            }
        }
    }
}
