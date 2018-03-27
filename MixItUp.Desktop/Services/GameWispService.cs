using Mixer.Base;
using Mixer.Base.Model.OAuth;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class GameWispService : OAuthServiceBase, IGameWispService
    {
        private const string BaseAddress = "https://api.gamewisp.com/pub/v1/";

        private const string ClientID = "0b23546e4f147c63509c29928f2bf87e73ce62f";
        private const string ClientSecret = "898fdf933a202478cce285cce8b29a5b97cbff9";
        private const string StateKey = "V21C2J2RWE51CYSM";
        private const string AuthorizationUrl = "https://api.gamewisp.com/pub/v1/oauth/authorize?client_id={0}&redirect_uri=http://localhost:8919/&response_type=code&scope=read_only,subscriber_read_full,user_read&state={1}";

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public GameWispService() : base(GameWispService.BaseAddress) { }

        public GameWispService(OAuthTokenModel token) : base(GameWispService.BaseAddress, token) { }

        public async Task<bool> Connect()
        {
            if (this.token != null)
            {
                try
                {
                    await this.RefreshOAuthToken();

                    await this.InitializeInternal();

                    return true;
                }
                catch (Exception ex) { Logger.Log(ex); }
            }

            string authorizationCode = await this.ConnectViaOAuthRedirect(string.Format(GameWispService.AuthorizationUrl, GameWispService.ClientID, GameWispService.StateKey));
            if (!string.IsNullOrEmpty(authorizationCode))
            {
                JObject payload = new JObject();
                payload["grant_type"] = "authorization_code";
                payload["client_id"] = GameWispService.ClientID;
                payload["client_secret"] = GameWispService.ClientSecret;
                payload["redirect_uri"] = MixerConnection.DEFAULT_OAUTH_LOCALHOST_URL;
                payload["code"] = authorizationCode;
                payload["state"] = GameWispService.StateKey;

                this.token = await this.PostAsync<OAuthTokenModel>("oauth/token", this.CreateContentFromObject(payload), autoRefreshToken: false);
                if (this.token != null)
                {
                    token.authorizationCode = authorizationCode;

                    await this.InitializeInternal();

                    return true;
                }
            }

            return false;
        }

        public Task Disconnect()
        {
            this.token = null;
            this.cancellationTokenSource.Cancel();
            return Task.FromResult(0);
        }

        protected override async Task RefreshOAuthToken()
        {
            if (this.token != null)
            {
                JObject payload = new JObject();
                payload["grant_type"] = "refresh_token";
                payload["client_id"] = GameWispService.ClientID;
                payload["client_secret"] = GameWispService.ClientSecret;
                payload["refresh_token"] = this.token.refreshToken;
                payload["redirect_uri"] = MixerConnection.DEFAULT_OAUTH_LOCALHOST_URL;

                this.token = await this.PostAsync<OAuthTokenModel>("oauth/token", this.CreateContentFromObject(payload), autoRefreshToken: false);
            }
        }

        private async Task InitializeInternal()
        {
            //HttpResponseMessage result = await this.GetAsync("socket/token");
            //string resultJson = await result.Content.ReadAsStringAsync();
            //JObject jobj = JObject.Parse(resultJson);

            //this.websocketService = new StreamlabsWebSocketService(jobj["socket_token"].ToString());
        }
    }
}
