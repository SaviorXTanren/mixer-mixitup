using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Web
{
    /// <summary>
    /// An Http listening server for processing OAuth requests.
    /// </summary>
    public class LocalOAuthHttpListenerServer : LocalHttpListenerServer
    {
        public const string REDIRECT_URL = "http://localhost:8919/";

        public const string AUTHORIZATION_CODE_URL_PARAMETER = "code";

        public const string LOGIN_REDIRECT_HTML = @"<!DOCTYPE html>
            <html>
            <head>
                <meta charset=""utf-8"" />
                <title>Mix It Up - Logged In</title>
                <link rel=""shortcut icon"" type=""image/x-icon"" href=""https://github.com/SaviorXTanren/mixer-mixitup/raw/master/Branding/MixItUp-Logo-Base-WhiteXS.png"" />
                <style>
                    body {
                        background: #0e162a
                    }
                </style>
            </head>
            <body>
                <img src=""https://static.mixitupapp.com/desktop/Mix-It-Up_Logo_Auth-Callback.png"" width=""150"" height=""150"" style=""position: absolute; left: 50%; top: 25%; transform: translate(-50%, -50%);"" />
                <div style='background-color:#232841; position: absolute; left: 50%; top: 50%; transform: translate(-50%, -50%); padding: 20px'>
                    <h1 style=""text-align:center;color:white;margin-top:10px"">Mix It Up</h1>
                    <h3 style=""text-align:center;color:white;"">Logged In Successfully</h3>
                    <p style=""text-align:center;color:white;"">You have been logged in, you may now close this webpage</p>
                </div>
            </body>
            </html>";

        private string authorizationCode;

        public LocalOAuthHttpListenerServer() { }

        public async Task<string> GetAuthorizationCode(string authorizationURL, int secondsToWait)
        {
            try
            {
                if (this.Start(REDIRECT_URL))
                {
                    ServiceManager.Get<IProcessService>().LaunchLink(authorizationURL);

                    for (int i = 0; i < secondsToWait; i++)
                    {
                        if (!string.IsNullOrEmpty(this.authorizationCode))
                        {
                            break;
                        }
                        await Task.Delay(1000);
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            this.Stop();

            return this.authorizationCode;
        }

        public async Task<Result<string>> GetAuthorizationCode(string authorizationURL, CancellationToken cancellationToken)
        {
            try
            {
                if (this.Start(REDIRECT_URL))
                {
                    ServiceManager.Get<IProcessService>().LaunchLink(authorizationURL);

                    while (!cancellationToken.IsCancellationRequested && string.IsNullOrWhiteSpace(this.authorizationCode))
                    {
                        await Task.Delay(1000);
                    }
                }
                else
                {
                    return new Result<string>(success: false, message: Resources.UnableToStartAuthenticationSession);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            this.Stop();

            return new Result<string>(success: !string.IsNullOrWhiteSpace(this.authorizationCode), message: null)
            {
                Value = this.authorizationCode
            };
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
                result = LOGIN_REDIRECT_HTML;

                this.authorizationCode = token;
            }

            await this.CloseConnection(listenerContext, statusCode, result);
        }
    }
}