using System;
using System.Threading.Tasks;
using Mixer.Base.Model.OAuth;
using MixItUp.Base;
using MixItUp.Base.Services;
using Newtonsoft.Json.Linq;

namespace MixItUp.Desktop.Services
{
    public class TreatStreamService : OAuthServiceBase, ITreatStreamService
    {
        private const string BaseAddress = "https://treatstream.com/api/";

        public const string ClientID = "xr7qxfpymxsdabivcit0sjv64qv2u5x34058pzvw";

        public const string ListeningURL = "http://localhost:8919";

        public const string AuthorizationURL = "https://treatstream.com/Oauth2/Authorize?response_type=code&client_id={0}&redirect_uri={1}&state=xyz&scope=";
        public const string OAuthTokenURL = "https://treatstream.com/Oauth2/Authorize/token";
        public const string RefreshTokenURL = "https://treatstream.com/Oauth2/RefreshToken";

        public event EventHandler OnWebSocketConnectedOccurred = delegate { };
        public event EventHandler OnWebSocketDisconnectedOccurred = delegate { };

        public event EventHandler<TreatStreamEvent> OnDonationOccurred = delegate { };

        public bool WebSocketConnectedAndAuthenticated { get; private set; }

        private string authorizationToken;

        public TreatStreamService() : base(TreatStreamService.BaseAddress) { }

        public TreatStreamService(OAuthTokenModel token) : base(TreatStreamService.BaseAddress, token) { }

        public async Task<bool> Connect()
        {
            if (this.token != null)
            {
                try
                {
                    await this.InitializeInternal();

                    return true;
                }
                catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }
            }

            this.authorizationToken = await this.ConnectViaOAuthRedirect(string.Format(TreatStreamService.AuthorizationURL, TreatStreamService.ClientID, TreatStreamService.ListeningURL));
            if (!string.IsNullOrEmpty(this.authorizationToken))
            {
                try
                {
                    JObject payload = new JObject();
                    payload["grant_type"] = "authorization_code";
                    payload["client_id"] = TreatStreamService.ClientID;
                    payload["client_secret"] = ChannelSession.SecretManager.GetSecret("TreatStreamSecret");
                    payload["code"] = this.authorizationToken;
                    payload["redirect_uri"] = TreatStreamService.ListeningURL;
                    payload["scope"] = "userinfo";

                    this.token = await this.PostAsync<OAuthTokenModel>(TreatStreamService.OAuthTokenURL, this.CreateContentFromObject(payload), autoRefreshToken: false);
                    if (this.token != null)
                    {
                        token.authorizationCode = this.authorizationToken;
                        token.AcquiredDateTime = DateTimeOffset.Now;
                        token.expiresIn = int.MaxValue;

                        await this.InitializeInternal();

                        return true;
                    }
                }
                catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }
            }
            return false;
        }

        public async Task Disconnect()
        {
            this.token = null;
        }

        public async Task GetTreats()
        {
            try
            {
                string result = await this.GetStringAsync(string.Format("getMonthTreats/{0}", this.token.accessToken));
            }
            catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }
        }

        protected override async Task RefreshOAuthToken()
        {
            if (this.token != null)
            {
                JObject payload = new JObject();
                payload["client_id"] = TipeeeStreamService.ClientID;
                payload["client_secret"] = ChannelSession.SecretManager.GetSecret("TipeeeStreamSecret");
                payload["refresh_token"] = this.token.refreshToken;
                payload["grant_type"] = "refresh_token";

                this.token = await this.PostAsync<OAuthTokenModel>(TreatStreamService.RefreshTokenURL, this.CreateContentFromObject(payload), autoRefreshToken: false);
            }
        }

        protected async Task InitializeInternal()
        {
            await this.GetTreats();
        }

        public void WebSocketConnectedOccurred()
        {
            this.OnWebSocketConnectedOccurred(this, new EventArgs());
        }

        public void WebSocketDisconnectedOccurred()
        {
            this.OnWebSocketDisconnectedOccurred(this, new EventArgs());
        }

        public void DonationOccurred(TreatStreamEvent eventData)
        {
            this.OnDonationOccurred(this, eventData);
        }
    }
}
