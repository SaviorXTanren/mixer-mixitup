using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Net;
using System.Threading.Tasks;

namespace MixItUp.Base.Web
{
    /// <summary>
    /// An Http listening server for processing OAuth requests.
    /// </summary>
    public class LocalOAuthHttpListenerServer : LocalHttpListenerServer
    {
        public const string OAUTH_LOCALHOST_URL = "http://localhost:8919/";

        private const string AUTHORIZATION_CODE_URL_PARAMETER = "code";

        private const string SUCCESS_RESPONSE = "<!DOCTYPE html><html><body><h1 style=\"text-align:center;\">Logged In Successfully</h1><p style=\"text-align:center;\">You have been logged in, you may now close this webpage</p></body></html>";

        private string authorizationCode;

        public LocalOAuthHttpListenerServer() { }

        public async Task<string> GetAuthorizationCode(string authorizationURL)
        {
            try
            {
                this.Start(OAUTH_LOCALHOST_URL);

                ServiceManager.Get<IProcessService>().LaunchLink(authorizationURL);

                while (string.IsNullOrWhiteSpace(this.authorizationCode))
                {
                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            this.Stop();

            return this.authorizationCode;
        }

        /// <summary>
        /// Processes an http request.
        /// </summary>
        /// <param name="listenerContext">The context of the request</param>
        /// <returns>An awaitable task to process the request</returns>
        protected override async Task ProcessConnection(HttpListenerContext listenerContext)
        {
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            string result = string.Empty;

            string token = this.GetRequestParameter(listenerContext, AUTHORIZATION_CODE_URL_PARAMETER);
            if (!string.IsNullOrEmpty(token))
            {
                statusCode = HttpStatusCode.OK;
                result = SUCCESS_RESPONSE;

                this.authorizationCode = token;
            }

            await this.CloseConnection(listenerContext, statusCode, result);
        }
    }
}