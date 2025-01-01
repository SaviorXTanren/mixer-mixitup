using MixItUp.Base.Model.Web;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Trovo.API
{
    /// <summary>
    /// The validation object for an OAuth token.
    /// </summary>
    public class OAuthTokenValidationModel
    {
        /// <summary>
        /// The ID of the user.
        /// </summary>
        public string uid { get; set; }
        /// <summary>
        /// The ID of the client application.
        /// </summary>
        public string client_id { get; set; }
        /// <summary>
        /// The display name of the user.
        /// </summary>
        public string nick_name { get; set; }
        /// <summary>
        /// The scopes requested.
        /// </summary>
        public List<string> scopes { get; set; } = new List<string>();
        /// <summary>
        /// The expiration time in Unix seconds.
        /// </summary>
        public long expire_ts { get; set; }

        /// <summary>
        /// The expiration date time.
        /// </summary>
        public DateTimeOffset Expiration { get { return DateTimeOffset.FromUnixTimeSeconds(this.expire_ts); } }
    }

    /// <summary>
    /// The OAuth service.
    /// </summary>
    public class OAuthService : TrovoServiceBase
    {
        /// <summary>
        /// The base OAuth address.
        /// </summary>
        public const string OAuthBaseAddress = "https://open.trovo.live/page/login.html";

        /// <summary>
        /// Creates an instance of the OAuthService.
        /// </summary>
        /// <param name="connection">The YouTube connection to use</param>
        public OAuthService(TrovoConnection connection) : base(connection) { }

        internal OAuthService(string clientID) : base(clientID) { }

        /// <summary>
        /// Creates an OAuth token for authenticating with the Twitch services.
        /// </summary>
        /// <param name="clientID">The id of the client application</param>
        /// <param name="clientSecret">The secret key of the client application</param>
        /// <param name="authorizationCode">The authorization code</param>
        /// <param name="scopes">The list of scopes that were requested</param>
        /// <param name="redirectUrl">The URL to redirect to after authorization is complete</param>
        /// <returns>The OAuth token</returns>
        public async Task<OAuthTokenModel> GetOAuthTokenModel(string clientID, string clientSecret, string authorizationCode, IEnumerable<OAuthClientScopeEnum> scopes, string redirectUrl = null)
        {
            Validator.ValidateString(clientID, "clientID");
            Validator.ValidateString(authorizationCode, "authorizationCode");

            JObject content = new JObject()
            {
                { "client_id", clientID },
                { "client_secret", clientSecret },
                { "code", authorizationCode },
                { "grant_type", "authorization_code" },
                { "redirect_uri", redirectUrl },
            };

            OAuthTokenModel token = await this.PostAsync<OAuthTokenModel>("exchangetoken", AdvancedHttpClient.CreateContentFromObject(content), autoRefreshToken: false);
            token.clientID = clientID;
            token.ScopeList = string.Join(",", scopes ?? new List<OAuthClientScopeEnum>());
            return token;
        }

        /// <summary>
        /// Refreshes the specified OAuth token.
        /// </summary>
        /// <param name="token">The token to refresh</param>
        /// <returns>The refreshed token</returns>
        public async Task<OAuthTokenModel> RefreshToken(OAuthTokenModel token)
        {
            Validator.ValidateVariable(token, "token");

            JObject content = new JObject()
            {
                //{ "client_secret", token.clientSecret },
                { "grant_type", "refresh_token" },
                { "refresh_token", token.refreshToken }
            };

            OAuthTokenModel newToken = await this.PostAsync<OAuthTokenModel>("refreshtoken", AdvancedHttpClient.CreateContentFromObject(content), autoRefreshToken: false);
            newToken.clientID = token.clientID;
            newToken.ScopeList = token.ScopeList;
            return newToken;
        }

        /// <summary>
        /// Refreshes the specified OAuth token.
        /// </summary>
        /// <param name="token">The token to refresh</param>
        /// <returns>The refreshed token</returns>
        public async Task<OAuthTokenValidationModel> ValidateToken(OAuthTokenModel token)
        {
            Validator.ValidateVariable(token, "token");
            return await this.GetAsync<OAuthTokenValidationModel>("validate");
        }
    }
}
