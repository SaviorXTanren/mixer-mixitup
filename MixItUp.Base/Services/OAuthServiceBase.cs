using Mixer.Base;
using Mixer.Base.Model.OAuth;
using Mixer.Base.Services;
using Mixer.Base.Web;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public abstract class OAuthServiceBase : RestServiceBase
    {
        protected OAuthTokenModel token;

        protected string baseAddress;

        protected OAuthServiceBase(string baseAddress) { this.baseAddress = baseAddress; }

        protected OAuthServiceBase(string baseAddress, OAuthTokenModel token) : this(baseAddress) { this.token = token; }

        public OAuthTokenModel GetOAuthTokenCopy()
        {
            if (this.token != null)
            {
                return new OAuthTokenModel()
                {
                    clientID = this.token.clientID,
                    authorizationCode = this.token.authorizationCode,
                    refreshToken = this.token.refreshToken,
                    accessToken = this.token.accessToken,
                    expiresIn = this.token.expiresIn
                };
            }
            return null;
        }

        protected async Task<string> ConnectViaOAuthRedirect(string oauthPageURL)
        {
            OAuthHttpListenerServer oauthServer = new OAuthHttpListenerServer(MixerConnection.DEFAULT_OAUTH_LOCALHOST_URL, loginSuccessHtmlPageFilePath: "LoginRedirectPage.html");
            oauthServer.Start();

            ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = oauthPageURL, UseShellExecute = true };
            Process.Start(startInfo);

            string authorizationCode = await oauthServer.WaitForAuthorizationCode();
            oauthServer.End();

            return authorizationCode;
        }

        protected override string GetBaseAddress() { return this.baseAddress; }

        protected override async Task<OAuthTokenModel> GetOAuthToken()
        {
            if (this.token != null && this.token.ExpirationDateTime < DateTimeOffset.Now)
            {
                await this.RefreshOAuthToken();
            }
            return this.token;
        }

        protected abstract Task RefreshOAuthToken();
    }
}
