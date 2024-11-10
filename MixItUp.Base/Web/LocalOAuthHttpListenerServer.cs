using System.Net;
using System.Threading.Tasks;

namespace MixItUp.Base.Web
{
    /// <summary>
    /// An Http listening server for processing OAuth requests.
    /// </summary>
    public class LocalOAuthHttpListenerServer : LocalHttpListenerServer
    {
        private const string defaultSuccessResponse = "<!DOCTYPE html><html><body><h1 style=\"text-align:center;\">Logged In Successfully</h1><p style=\"text-align:center;\">You have been logged in, you may now close this webpage</p></body></html>";

        private string authorizationCodeParameterName = null;
        private string successResponse = null;

        private string authorizationCode = null;

        /// <summary>
        /// Creates a new instance of the LocalOAuthHttpListenerService with the specified address.
        /// </summary>
        /// <param name="authorizationCodeParameterName">The name of the parameter for the authorization code</param>
        public LocalOAuthHttpListenerServer(string authorizationCodeParameterName)
        {
            this.authorizationCodeParameterName = authorizationCodeParameterName;
        }

        /// <summary>
        /// Creates a new instance of the LocalOAuthHttpListenerService with the specified address &amp; login response.
        /// </summary>
        /// <param name="authorizationCodeParameterName">The name of the parameter for the authorization code</param>
        /// <param name="successResponse">The response to send upon successfully obtaining an authorization token</param>
        public LocalOAuthHttpListenerServer(string authorizationCodeParameterName, string successResponse)
            : this(authorizationCodeParameterName)
        {
            this.successResponse = successResponse;
        }

        /// <summary>
        /// Waits for a successful authorization response from the OAuth service.
        /// </summary>
        /// <param name="secondsToWait">The total number of seconds to wait</param>
        /// <returns>The authorization token from the OAuth service</returns>
        public async Task<string> WaitForAuthorizationCode(int secondsToWait = 30)
        {
            for (int i = 0; i < secondsToWait; i++)
            {
                if (!string.IsNullOrEmpty(this.authorizationCode))
                {
                    break;
                }
                await Task.Delay(1000);
            }
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

            string token = this.GetRequestParameter(listenerContext, this.authorizationCodeParameterName);
            if (!string.IsNullOrEmpty(token))
            {
                statusCode = HttpStatusCode.OK;
                result = defaultSuccessResponse;
                if (!string.IsNullOrEmpty(this.successResponse))
                {
                    result = successResponse;
                }

                this.authorizationCode = token;
            }

            await this.CloseConnection(listenerContext, statusCode, result);
        }
    }
}