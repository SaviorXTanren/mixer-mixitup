using MixItUp.Base.Model.Web;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Twitch.API
{
    /// <summary>
    /// The APIs for OAuth-based services.
    /// </summary>
    public class OAuthService : TwitchServiceBase
    {
        private const string OAuthBaseAddress = "https://id.twitch.tv/";

        /// <summary>
        /// Creates an instance of the OAuthService.
        /// </summary>
        /// <param name="connection">The Twitch connection to use</param>
        public OAuthService(TwitchConnection connection) : base(connection, OAuthBaseAddress) { }

        internal OAuthService() : base(OAuthBaseAddress) { }

        /// <summary>
        /// Creates an OAuth token for authenticating with the Twitch services.
        /// </summary>
        /// <param name="clientID">The id of the client application</param>
        /// <param name="authorizationCode">The authorization code</param>
        /// <param name="scopes">The list of scopes that were requested</param>
        /// <param name="redirectUrl">The URL to redirect to after authorization is complete</param>
        /// <returns>The OAuth token</returns>
        public async Task<OAuthTokenModel> GetOAuthTokenModel(string clientID, string authorizationCode, IEnumerable<OAuthClientScopeEnum> scopes)
        {
            return await this.GetOAuthTokenModel(clientID, null, authorizationCode, scopes);
        }

        /// <summary>
        /// Creates an OAuth token for authenticating with the Twitch services.
        /// </summary>
        /// <param name="clientID">The id of the client application</param>
        /// <param name="clientSecret">The secret key of the client application</param>
        /// <param name="authorizationCode">The authorization code</param>
        /// <param name="scopes">The list of scopes that were requested</param>
        /// <param name="redirectUrl">The URL to redirect to after authorization is complete</param>
        /// <returns>The OAuth token</returns>
        public async Task<OAuthTokenModel> GetOAuthTokenModel(string clientID, string clientSecret, string authorizationCode, IEnumerable<OAuthClientScopeEnum> scopes)
        {
            Validator.ValidateString(clientID, "clientID");
            Validator.ValidateString(authorizationCode, "authorizationCode");

            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { "client_id", clientID },
                { "client_secret", clientSecret },
                { "code", authorizationCode },
                { "grant_type", "authorization_code" },
                { "redirect_uri", LocalOAuthHttpListenerServer.REDIRECT_URL },
            };
            FormUrlEncodedContent content = new FormUrlEncodedContent(parameters.AsEnumerable());

            OAuthTokenModel token = await this.PostAsync<OAuthTokenModel>("oauth2/token?" + await content.ReadAsStringAsync(), AdvancedHttpClient.CreateContentFromObject(string.Empty), autoRefreshToken: false);
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

            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { "client_id", token.clientID },
                //{ "client_secret", token.clientSecret },
                { "refresh_token", token.refreshToken },
                { "grant_type", "refresh_token" },
            };
            FormUrlEncodedContent content = new FormUrlEncodedContent(parameters.AsEnumerable());

            OAuthTokenModel newToken = await this.PostAsync<OAuthTokenModel>("oauth2/token?" + await content.ReadAsStringAsync(), AdvancedHttpClient.CreateContentFromObject(string.Empty), autoRefreshToken: false);
            newToken.clientID = token.clientID;
            newToken.ScopeList = token.ScopeList;
            return newToken;
        }
    }
}
