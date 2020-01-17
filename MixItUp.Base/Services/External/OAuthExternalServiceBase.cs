using Mixer.Base;
using MixItUp.Base.Util;
using StreamingClient.Base.Model.OAuth;
using StreamingClient.Base.Services;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public interface IOAuthExternalService : IExternalService
    {
        bool IsConnected { get; }

        Task<ExternalServiceResult> Connect(OAuthTokenModel token);

        OAuthTokenModel GetOAuthTokenCopy();
    }

    public static class AdvancedHttpClientExtensions
    {
        public static void SetBasicClientIDClientSecretAuthorizationHeader(this AdvancedHttpClient client, string clientID, string clientSecret)
        {
            string authorizationValue = string.Format("{0}:{1}", clientID, clientSecret);
            byte[] authorizationBytes = System.Text.Encoding.UTF8.GetBytes(authorizationValue);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authorizationBytes));
        }
    }

    public abstract class OAuthExternalServiceBase : OAuthRestServiceBase, IOAuthExternalService, IDisposable
    {
        public const string LoginRedirectPageHTML = @"<!DOCTYPE html>
            <html>
            <head>
                <meta charset=""utf-8"" />
                <title>Mix It Up - Logged In</title>
                <link rel=""shortcut icon"" type=""image/x-icon"" href=""https://github.com/SaviorXTanren/mixer-mixitup/raw/master/Branding/MixItUp-Logo-Base-TransparentSM.png"" />
                <style>
                    body {
                        background: #0e162a
                    }
                </style>
            </head>
            <body>
                <img src=""https://github.com/SaviorXTanren/mixer-mixitup/raw/master/Branding/MixItUp-Logo-Base-WhiteSM.png"" width=""150"" height=""150"" style=""position: absolute; left: 50%; top: 25%; transform: translate(-50%, -50%);"" />
                <div style='background-color:#232841; position: absolute; left: 50%; top: 50%; transform: translate(-50%, -50%); padding: 20px'>
                    <h1 style=""text-align:center;color:white;margin-top:10px"">Mix It Up</h1>
                    <h3 style=""text-align:center;color:white;"">Logged In Successfully</h3>
                    <p style=""text-align:center;color:white;"">You have been logged in, you may now close this webpage</p>
                </div>
            </body>
            </html>";

        protected OAuthTokenModel token;

        protected string baseAddress;

        protected OAuthExternalServiceBase(string baseAddress) { this.baseAddress = baseAddress; }

        public abstract string Name { get; }

        public virtual bool IsConnected { get { return this.token != null; } }

        public abstract Task<ExternalServiceResult> Connect();

        public virtual async Task<ExternalServiceResult> Connect(OAuthTokenModel token)
        {
            try
            {
                this.token = token;
                await this.RefreshOAuthToken();

                ExternalServiceResult result = await this.InitializeInternal();
                if (!result.Success)
                {
                    this.token = null;
                }
                return result;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new ExternalServiceResult(ex);
            }
        }

        public abstract Task Disconnect();

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

        protected abstract Task<ExternalServiceResult> InitializeInternal();

        protected async Task<string> ConnectViaOAuthRedirect(string oauthPageURL) { return await this.ConnectViaOAuthRedirect(oauthPageURL, MixerConnection.DEFAULT_OAUTH_LOCALHOST_URL); }

        protected virtual async Task<string> ConnectViaOAuthRedirect(string oauthPageURL, string listeningAddress)
        {
            LocalOAuthHttpListenerServer oauthServer = new LocalOAuthHttpListenerServer(listeningAddress, MixerConnection.DEFAULT_AUTHORIZATION_CODE_URL_PARAMETER, successResponse: OAuthServiceBase.LoginRedirectPageHTML);
            oauthServer.Start();

            ProcessHelper.LaunchLink(oauthPageURL);

            string authorizationCode = await oauthServer.WaitForAuthorizationCode();
            oauthServer.Stop();

            return authorizationCode;
        }

        protected override string GetBaseAddress() { return this.baseAddress; }

        protected override async Task<OAuthTokenModel> GetOAuthToken(bool autoRefreshToken = true)
        {
            if (autoRefreshToken && this.token != null && this.token.ExpirationDateTime < DateTimeOffset.Now)
            {
                await this.RefreshOAuthToken();
            }
            return this.token;
        }

        protected async Task<OAuthTokenModel> GetWWWFormUrlEncodedOAuthToken(string endpoint, string clientID, string clientSecret, List<KeyValuePair<string, string>> bodyContent)
        {
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient())
                {
                    client.SetBasicClientIDClientSecretAuthorizationHeader(clientID, clientSecret);
                    using (var content = new FormUrlEncodedContent(bodyContent))
                    {
                        content.Headers.Clear();
                        content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                        HttpResponseMessage response = await client.PostAsync(endpoint, content);
                        return await response.ProcessResponse<OAuthTokenModel>();
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        protected abstract Task RefreshOAuthToken();

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void DisposeInternal() { }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    this.DisposeInternal();
                }

                // Free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        #endregion
    }
}
