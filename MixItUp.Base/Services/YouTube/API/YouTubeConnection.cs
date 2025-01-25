using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTubePartner.v1;
using MixItUp.Base.Model.Web;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.YouTube.API
{
    /// <summary>
    /// Authentication scopes for a user: https://developers.google.com/identity/protocols/googlescopes#youtubev3
    /// </summary>
    public enum OAuthClientScopeEnum
    {
        /// <summary>
        /// https://www.googleapis.com/auth/youtube.channel-memberships.creator
        /// </summary>
        [Name("https://www.googleapis.com/auth/youtube.channel-memberships.creator")]
        ChannelMemberships,
        /// <summary>
        /// https://www.googleapis.com/auth/youtube
        /// </summary>
        [Name("https://www.googleapis.com/auth/youtube")]
        ManageAccount,
        /// <summary>
        /// https://www.googleapis.com/auth/youtube.force-ssl
        /// </summary>
        [Name("https://www.googleapis.com/auth/youtube.force-ssl")]
        ManageData,
        /// <summary>
        /// https://www.googleapis.com/auth/youtube.readonly
        /// </summary>
        [Name("https://www.googleapis.com/auth/youtube.readonly")]
        ReadOnlyAccount,
        /// <summary>
        /// https://www.googleapis.com/auth/youtube.upload
        /// </summary>
        [Name("https://www.googleapis.com/auth/youtube.upload")]
        ManageVideos,
        /// <summary>
        /// https://www.googleapis.com/auth/youtubepartner
        /// </summary>
        [Name("https://www.googleapis.com/auth/youtubepartner")]
        ManagePartner,
        /// <summary>
        /// https://www.googleapis.com/auth/youtubepartner-channel-audit
        /// </summary>
        [Name("https://www.googleapis.com/auth/youtubepartner-channel-audit")]
        ManagePartnerAudit,
        /// <summary>
        /// https://www.googleapis.com/auth/yt-analytics.readonly
        /// </summary>
        [Name("https://www.googleapis.com/auth/yt-analytics.readonly")]
        ViewAnalytics,
        /// <summary>
        /// https://www.googleapis.com/auth/yt-analytics-monetary.readonly
        /// </summary>
        [Name("https://www.googleapis.com/auth/yt-analytics-monetary.readonly")]
        ViewMonetaryAnalytics,
    }

    /// <summary>
    /// The main connection handler for YouTube.
    /// </summary>
    public class YouTubeConnection
    {
        /// <summary>
        /// The default OAuth redirect URL used for authentication.
        /// </summary>
        public const string DEFAULT_OAUTH_LOCALHOST_URL = "http://127.0.0.1:8919/";

        /// <summary>
        /// The default request parameter for the authorization code from the OAuth service.
        /// </summary>
        public const string DEFAULT_AUTHORIZATION_CODE_URL_PARAMETER = "code";

        /// <summary>
        /// APIs for OAuth interaction.
        /// </summary>
        public OAuthService OAuth { get; private set; }

        /// <summary>
        /// APIs for Channels interaction.
        /// </summary>
        public ChannelsService Channels { get; private set; }

        /// <summary>
        /// APIs for Comments interaction.
        /// </summary>
        public CommentsService Comments { get; private set; }

        /// <summary>
        /// APIs for Live Broadcasts interaction.
        /// </summary>
        public LiveBroadcastsService LiveBroadcasts { get; private set; }

        /// <summary>
        /// APIs for Live Chat interaction.
        /// </summary>
        public LiveChatService LiveChat { get; private set; }

        /// <summary>
        /// APIs for Membership interaction.
        /// </summary>
        public MembershipService Membership { get; private set; }

        /// <summary>
        /// APIs for Playlists interaction.
        /// </summary>
        public PlaylistsService Playlists { get; private set; }

        /// <summary>
        /// APIs for Subscriptions interaction.
        /// </summary>
        public SubscriptionsService Subscriptions { get; private set; }

        /// <summary>
        /// APIs for Videos interaction.
        /// </summary>
        public VideosService Videos { get; private set; }

        /// <summary>
        /// The underlying YouTube service from Google's .NET client library.
        /// </summary>
        public YouTubeService GoogleYouTubeService { get; private set; }

        /// <summary>
        /// The underlying YouTube Partner service from Google's .NET client library.
        /// </summary>
        public YouTubePartnerService GoogleYouTubePartnerService { get; private set; }

        private OAuthTokenModel token;

        /// <summary>
        /// Generates the OAuth authorization URL to use for authentication.
        /// </summary>
        /// <param name="clientID">The ID of the client application</param>
        /// <param name="scopes">The authorization scopes to request</param>
        /// <param name="redirectUri">The redirect URL for the client application</param>
        /// <param name="forceApprovalPrompt">Whether to force an approval from the user</param>
        /// <returns>The authorization URL</returns>
        public static async Task<string> GetAuthorizationCodeURLForOAuthBrowser(string clientID, IEnumerable<OAuthClientScopeEnum> scopes, string redirectUri, bool forceApprovalPrompt = false)
        {
            Validator.ValidateString(clientID, "clientID");
            Validator.ValidateList(scopes, "scopes");
            Validator.ValidateString(redirectUri, "redirectUri");

            string url = "https://accounts.google.com/o/oauth2/v2/auth";

            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { "client_id", clientID },
                { "scope", YouTubeConnection.ConvertClientScopesToString(scopes) },
                { "response_type", "code" },
                { "redirect_uri", redirectUri },
            };

            if (forceApprovalPrompt)
            {
                parameters.Add("force_verify", "force");
            }

            FormUrlEncodedContent content = new FormUrlEncodedContent(parameters.AsEnumerable());

            return url + "?" + await content.ReadAsStringAsync();
        }

        /// <summary>
        /// Creates a YouTubeLiveConnection object from an OAuth authentication locally.
        /// </summary>
        /// <param name="clientID">The ID of the client application</param>
        /// <param name="clientSecret">The secret of the client application</param>
        /// <param name="scopes">The authorization scopes to request</param>
        /// <param name="forceApprovalPrompt">Whether to force an approval from the user</param>
        /// <param name="oauthListenerURL">The URL to listen for the OAuth successful authentication</param>
        /// <param name="successResponse">The response to send back upon successful authentication</param>
        /// <returns>The YouTubeLiveConnection object</returns>
        public static async Task<YouTubeConnection> ConnectViaLocalhostOAuthBrowser(string clientID, string clientSecret, IEnumerable<OAuthClientScopeEnum> scopes, bool forceApprovalPrompt = false, string oauthListenerURL = DEFAULT_OAUTH_LOCALHOST_URL, string successResponse = null)
        {
            Validator.ValidateString(clientID, "clientID");
            Validator.ValidateList(scopes, "scopes");

            LocalOAuthHttpListenerServer oauthServer = new LocalOAuthHttpListenerServer();
            string authorizationCode = await oauthServer.GetAuthorizationCode(await YouTubeConnection.GetAuthorizationCodeURLForOAuthBrowser(clientID, scopes, oauthListenerURL, forceApprovalPrompt), 30);

            if (authorizationCode != null)
            {
                return await YouTubeConnection.ConnectViaAuthorizationCode(clientID, clientSecret, authorizationCode, scopes, redirectUrl: oauthListenerURL);
            }
            return null;
        }

        /// <summary>
        /// Creates a YouTubeLiveConnection object from an authorization code.
        /// </summary>
        /// <param name="clientID">The ID of the client application</param>
        /// <param name="clientSecret">The secret of the client application</param>
        /// <param name="authorizationCode">The authorization code for the authenticated user</param>
        /// <param name="scopes">The list of scopes that were requested</param>
        /// <param name="redirectUrl">The redirect URL of the client application</param>
        /// <returns>The YouTubeLiveConnection object</returns>
        public static async Task<YouTubeConnection> ConnectViaAuthorizationCode(string clientID, string clientSecret, string authorizationCode, IEnumerable<OAuthClientScopeEnum> scopes = null, string redirectUrl = null)
        {
            Validator.ValidateString(clientID, "clientID");
            Validator.ValidateString(authorizationCode, "authorizationCode");

            OAuthService oauthService = new OAuthService();
            OAuthTokenModel token = await oauthService.GetOAuthTokenModel(clientID, clientSecret, authorizationCode, scopes, redirectUrl);
            if (token == null)
            {
                throw new InvalidOperationException("OAuth token was not acquired");
            }
            return new YouTubeConnection(token);
        }

        /// <summary>
        /// Creates a YouTubeLiveConnection object from an OAuth token.
        /// </summary>
        /// <param name="token">The OAuth token to use</param>
        /// <param name="refreshToken">Whether to refresh the token</param>
        /// <returns>The YouTubeLiveConnection object</returns>
        public static async Task<YouTubeConnection> ConnectViaOAuthToken(OAuthTokenModel token, bool refreshToken = true)
        {
            Validator.ValidateVariable(token, "token");

            YouTubeConnection connection = new YouTubeConnection(token);
            if (refreshToken)
            {
                await connection.RefreshOAuthToken();
            }

            return connection;
        }

        internal static string ConvertClientScopesToString(IEnumerable<OAuthClientScopeEnum> scopes)
        {
            List<string> scopeStrings = new List<string>() { "email", "profile", "openid" };
            scopeStrings.AddRange(EnumHelper.GetEnumNames(scopes));
            return string.Join(" ", scopeStrings);
        }

        private YouTubeConnection(OAuthTokenModel token)
        {
            Validator.ValidateVariable(token, "token");

            this.token = token;

            this.BuildYouTubeService();

            this.OAuth = new OAuthService(this);
            this.Channels = new ChannelsService(this);
            this.Comments = new CommentsService(this);
            this.LiveBroadcasts = new LiveBroadcastsService(this);
            this.LiveChat = new LiveChatService(this);
            this.Membership = new MembershipService(this);
            this.Playlists = new PlaylistsService(this);
            this.Subscriptions = new SubscriptionsService(this);
            this.Videos = new VideosService(this);
        }

        /// <summary>
        /// Refreshs the current OAuth token.
        /// </summary>
        /// <returns>An awaitable Task</returns>
        public async Task RefreshOAuthToken()
        {
            this.token = await this.OAuth.RefreshToken(this.token);
            this.BuildYouTubeService();
        }

        /// <summary>
        /// Gets a copy of the current OAuth token.
        /// </summary>
        /// <returns>The OAuth token copy</returns>
        public OAuthTokenModel GetOAuthTokenCopy() { return JSONSerializerHelper.Clone<OAuthTokenModel>(this.token); }

        internal async Task<OAuthTokenModel> GetOAuthToken(bool autoRefreshToken = true)
        {
            if (autoRefreshToken && this.token.ExpirationDateTime < DateTimeOffset.Now)
            {
                await this.RefreshOAuthToken();
            }
            return this.token;
        }

        private void BuildYouTubeService()
        {
            UserCredential credential = new UserCredential(new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets() { ClientId = this.token.clientID, ClientSecret = ServiceManager.Get<SecretsService>().GetSecret("YouTubeSecret") },
            }),
            "user",
            new TokenResponse()
            {
                AccessToken = this.token.accessToken,
                ExpiresInSeconds = this.token.expiresIn,
                RefreshToken = this.token.refreshToken,
            });

            this.GoogleYouTubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = this.token.clientID
            });

            this.GoogleYouTubePartnerService = new YouTubePartnerService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = this.token.clientID
            });
        }
    }
}
