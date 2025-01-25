using MixItUp.Base.Model.Web;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Twitch.API
{
    /// <summary>
    /// Authentication scopes for a user: https://dev.twitch.tv/docs/authentication/#scopes
    /// </summary>
    public enum OAuthClientScopeEnum
    {
        // v5 API

        /// <summary>
        /// Read whether a user is subscribed to your channel.
        /// </summary>
        channel_check_subscription = 0,
        /// <summary>
        /// Trigger commercials on channel.
        /// </summary>
        channel_commercial,
        /// <summary>
        /// Write channel metadata (game, status, etc).
        /// </summary>
        channel_editor,
        /// <summary>
        /// Add posts and reactions to a channel feed.
        /// </summary>
        channel_feed_edit,
        /// <summary>
        /// View a channel feed.
        /// </summary>
        channel_feed_read,
        /// <summary>
        /// Read nonpublic channel information, including email address and stream key.
        /// </summary>
        channel_read,
        /// <summary>
        /// Reset a channel’s stream key.
        /// </summary>
        channel_stream,
        /// <summary>
        /// Read all subscribers to your channel.
        /// </summary>
        channel_subscriptions,
        /// <summary>
        /// (Deprecated — cannot be requested by new clients.)
        /// Log into chat and send messages.
        /// </summary>
        chat_login,
        /// <summary>
        /// Manage a user’s collections (of videos).
        /// </summary>
        collections_edit,
        /// <summary>
        /// Manage a user’s communities.
        /// </summary>
        communities_edit,
        /// <summary>
        /// Manage community moderators.
        /// </summary>
        communities_moderate,
        /// <summary>
        /// Use OpenID Connect authentication.
        /// </summary>
        openid,
        /// <summary>
        /// Turn on/off ignoring a user. Ignoring users means you cannot see them type, receive messages from them, etc.
        /// </summary>
        user_blocks_edit,
        /// <summary>
        /// Read a user’s list of ignored users.
        /// </summary>
        user_blocks_read,
        /// <summary>
        /// Read nonpublic user information, like email address.
        /// </summary>
        user_read,
        /// <summary>
        /// Read a user’s subscriptions.
        /// </summary>
        user_subscriptions,
        /// <summary>
        /// Turn on Viewer Heartbeat Service ability to record user data.
        /// </summary>
        viewing_activity_read,

        // Chat / PubSub

        /// <summary>
        /// Perform moderation actions in a channel. The user requesting the scope must be a moderator in the channel.
        /// </summary>
        channel__moderate = 100,
        /// <summary>
        /// Send live stream chat and rooms messages.
        /// </summary>
        chat__edit,
        /// <summary>
        /// View live stream chat and rooms messages.
        /// </summary>
        chat__read,
        /// <summary>
        /// View your whisper messages.
        /// </summary>
        whispers__read,
        /// <summary>
        /// Send whisper messages.
        /// </summary>
        whispers__edit,

        // New API

        /// <summary>
        /// View analytics data for your extensions.
        /// </summary>
        analytics__read__extensions = 200,
        /// <summary>
        /// View analytics data for your games.
        /// </summary>
        analytics__read__games,
        /// <summary>
        /// View Bits information for your channel.
        /// </summary>
        bits__read,
        /// <summary>
        /// Run commercials on a channel.
        /// </summary>
        channel__edit__commercial,
        /// <summary>
        /// Manage ads schedule on a channel.
        /// </summary>
        channel__manage__ads,
        /// <summary>
        /// Manage your channel’s broadcast configuration, including updating channel configuration and managing stream markers and stream tags.
        /// </summary>
        channel__manage__broadcast,
        /// <summary>
        /// Manage a channel’s Extension configuration, including activating Extensions.
        /// </summary>
        channel__manage__extensions,
        /// <summary>
        /// Manage a channel’s moderators.
        /// </summary>
        channel__manage__moderators,
        /// <summary>
        /// Manage a channel’s polls.
        /// </summary>
        channel__manage__polls,
        /// <summary>
        /// Manage of channel’s Channel Points Predictions
        /// </summary>
        channel__manage__predictions,
        /// <summary>
        /// Manage a channel raiding another channel.
        /// </summary>
        channel__manage__raids,
        /// <summary>
        /// Manage Channel Points custom rewards and their redemptions on a channel.
        /// </summary>
        channel__manage__redemptions,
        /// <summary>
        /// Manage a channel’s stream schedule.
        /// </summary>
        channel__manage__schedule,
        /// <summary>
        /// Manage a channel’s videos, including deleting videos.
        /// </summary>
        channel__manage__videos,
        /// <summary>
        /// Manage a channel’s VIPs.
        /// </summary>
        channel__manage__vips,
        /// <summary>
        /// Read the ads schedule and details on your channel.
        /// </summary>
        channel__read__ads,
        /// <summary>
        /// Read charity campaign details and user donations on your channel.
        /// </summary>
        channel__read__charity,
        /// <summary>
        /// View a list of users with the editor role for a channel.
        /// </summary>
        channel__read__editors,
        /// <summary>
        /// View Creator Goals for a channel.
        /// </summary>
        channel__read__goals,
        /// <summary>
        /// Get hype train information for your channel.
        /// </summary>
        channel__read__hype_train,
        /// <summary>
        /// View a channel’s polls.
        /// </summary>
        channel__read__polls,
        /// <summary>
        /// View a channel’s Channel Points Predictions.
        /// </summary>
        channel__read__predictions,
        /// <summary>
        /// Get channel point redemption events for your channel.
        /// </summary>
        channel__read__redemptions,
        /// <summary>
        /// View an authorized user’s stream key.
        /// </summary>
        channel__read__stream_key,
        /// <summary>
        /// Get a list of all subscribers to your channel and check if a user is subscribed to your channel
        /// </summary>
        channel__read__subscriptions,
        /// <summary>
        /// View a channel’s VIPs.
        /// </summary>
        channel__read__vips,
        /// <summary>
        /// Manage a clip object.
        /// </summary>
        clips__edit,
        /// <summary>
        /// Manage a broadcaster’s chat announcements.
        /// </summary>
        moderator__manage__announcements,
        /// <summary>
        /// Manage messages held for review by AutoMod in channels where you are a moderator.
        /// </summary>
        moderator__manage__automod,
        /// <summary>
        /// Manage a broadcaster’s AutoMod settings.
        /// </summary>
        moderator__manage__automod_settings,
        /// <summary>
        /// Ban and unban users.
        /// </summary>
        moderator__manage__banned_users,
        /// <summary>
        /// Manage a broadcaster’s list of blocked terms.
        /// </summary>
        moderator__manage__blocked_terms,
        /// <summary>
        /// Manage a broadcaster’s chat room messages.
        /// </summary>
        moderator__manage__chat_messages,
        /// <summary>
        /// Manage a broadcaster’s chat room settings.
        /// </summary>
        moderator__manage__chat_settings,
        /// <summary>
        /// Manage a broadcaster's shoutouts
        /// </summary>
        moderator__manage__shoutouts,
        /// <summary>
        /// Read moderation events.
        /// </summary>
        moderation__read,
        /// <summary>
        /// View a broadcaster’s list of blocked terms.
        /// </summary>
        moderator__read__blocked_terms,
        /// <summary>
        /// View a broadcaster’s AutoMod settings.
        /// </summary>
        moderator__read__automod_settings,
        /// <summary>
        /// View a broadcaster’s list of chatters.
        /// </summary>
        moderator__read__chatters,
        /// <summary>
        /// View a broadcaster’s chat room settings.
        /// </summary>
        moderator__read__chat_settings,
        /// <summary>
        /// View a broadcaster's followers.
        /// </summary>
        moderator__read__followers,
        /// <summary>
        /// Manage a user object.
        /// </summary>
        user__edit,
        /// <summary>
        /// Manage the block list of a user.
        /// </summary>
        user__manage__blocked_users,
        /// <summary>
        /// Manage a user's chat color.
        /// </summary>
        user__manage__chat_color,
        /// <summary>
        /// Manage the whispers of a user.
        /// </summary>
        user__manage__whispers,
        /// <summary>
        /// View the block list of a user.
        /// </summary>
        user__read__blocked_users,
        /// <summary>
        /// View your broadcasting configuration, including extension configurations.
        /// </summary>
        user__read__broadcast,
        /// <summary>
        /// Read authorized user’s email address.
        /// </summary>
        user__read__email,
        /// <summary>
        /// View the list of channels a user follows.
        /// </summary>
        user__read__follows,
        /// <summary>
        /// View if an authorized user is subscribed to specific channels.
        /// </summary>
        user__read__subscriptions,
        /// <summary>
        /// View the whispers of a user.
        /// </summary>
        user__read__whispers,
    }

    /// <summary>
    /// The main connection handler for Twitch.
    /// </summary>
    public class TwitchConnection
    {
        private OAuthTokenModel token;

        /// <summary>
        /// APIs for OAuth interaction.
        /// </summary>
        public OAuthService OAuth { get; private set; }

        /// <summary>
        /// APIs for the New Twitch API.
        /// </summary>
        public NewTwitchAPIServices NewAPI { get; private set; }

        /// <summary>
        /// The Client ID associated with the connection.
        /// </summary>
        public string ClientID { get { return (this.token != null) ? this.token.clientID : null; } }

        /// <summary>
        /// Generates the OAuth authorization URL to use for authentication.
        /// </summary>
        /// <param name="clientID">The ID of the client application</param>
        /// <param name="scopes">The authorization scopes to request</param>
        /// <param name="redirectUri">The redirect URL for the client application</param>
        /// <param name="forceApprovalPrompt">Whether to force an approval from the user</param>
        /// <returns>The authorization URL</returns>
        public static async Task<string> GetAuthorizationCodeURLForOAuthBrowser(string clientID, IEnumerable<OAuthClientScopeEnum> scopes, bool forceApprovalPrompt = false)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { "client_id", clientID },
                { "scope", TwitchConnection.ConvertClientScopesToString(scopes) },
                { "response_type", "code" },
                { "redirect_uri", LocalOAuthHttpListenerServer.REDIRECT_URL },
            };

            if (forceApprovalPrompt)
            {
                parameters.Add("force_verify", "true");
            }

            FormUrlEncodedContent content = new FormUrlEncodedContent(parameters.AsEnumerable());

            return "https://id.twitch.tv/oauth2/authorize?" + await content.ReadAsStringAsync();
        }

        /// <summary>
        /// Creates a TwitchConnection object from an OAuth authentication locally.
        /// </summary>
        /// <param name="clientID">The ID of the client application</param>
        /// <param name="clientSecret">The secret of the client application</param>
        /// <param name="scopes">The authorization scopes to request</param>
        /// <param name="forceApprovalPrompt">Whether to force an approval from the user</param>
        /// <param name="oauthListenerURL">The URL to listen for the OAuth successful authentication</param>
        /// <param name="successResponse">The response to send back upon successful authentication</param>
        /// <returns>The TwitchConnection object</returns>
        public static async Task<TwitchConnection> ConnectViaLocalhostOAuthBrowser(string clientID, string clientSecret, IEnumerable<OAuthClientScopeEnum> scopes, bool forceApprovalPrompt = false)
        {
            Validator.ValidateString(clientID, "clientID");
            Validator.ValidateList(scopes, "scopes");

            LocalOAuthHttpListenerServer oauthServer = new LocalOAuthHttpListenerServer();
            string authorizationCode = await oauthServer.GetAuthorizationCode(await TwitchConnection.GetAuthorizationCodeURLForOAuthBrowser(clientID, scopes, forceApprovalPrompt), 30);

            if (authorizationCode != null)
            {
                return await TwitchConnection.ConnectViaAuthorizationCode(clientID, clientSecret, authorizationCode, scopes);
            }
            return null;
        }

        /// <summary>
        /// Creates a TwitchConnection object from an authorization code.
        /// </summary>
        /// <param name="clientID">The ID of the client application</param>
        /// <param name="clientSecret">The secret of the client application</param>
        /// <param name="authorizationCode">The authorization code for the authenticated user</param>
        /// <param name="scopes">The list of scopes that were requested</param>
        /// <param name="redirectUrl">The redirect URL of the client application</param>
        /// <returns>The TwitchConnection object</returns>
        public static async Task<TwitchConnection> ConnectViaAuthorizationCode(string clientID, string clientSecret, string authorizationCode, IEnumerable<OAuthClientScopeEnum> scopes = null)
        {
            Validator.ValidateString(clientID, "clientID");
            Validator.ValidateString(authorizationCode, "authorizationCode");

            OAuthService oauthService = new OAuthService();
            OAuthTokenModel token = await oauthService.GetOAuthTokenModel(clientID, clientSecret, authorizationCode, scopes);
            if (token == null)
            {
                throw new InvalidOperationException("OAuth token was not acquired");
            }
            return new TwitchConnection(token);
        }

        /// <summary>
        /// Creates a TwitchConnection object from an OAuth token.
        /// </summary>
        /// <param name="token">The OAuth token to use</param>
        /// <param name="refreshToken">Whether to refresh the token</param>
        /// <returns>The TwitchConnection object</returns>
        public static async Task<TwitchConnection> ConnectViaOAuthToken(OAuthTokenModel token, bool refreshToken = true)
        {
            Validator.ValidateVariable(token, "token");

            TwitchConnection connection = new TwitchConnection(token);
            if (refreshToken)
            {
                await connection.RefreshOAuthToken();
            }

            return connection;
        }

        internal static string ConvertClientScopesToString(IEnumerable<OAuthClientScopeEnum> scopes)
        {
            string result = "";

            foreach (string scopeName in EnumHelper.GetEnumNames(scopes))
            {
                result += scopeName.Replace("__", ":") + " ";
            }

            if (result.Length > 0)
            {
                result = result.Substring(0, result.Length - 1);
            }

            return result;
        }

        private TwitchConnection(OAuthTokenModel token)
        {
            Validator.ValidateVariable(token, "token");

            this.token = token;

            this.OAuth = new OAuthService(this);
            this.NewAPI = new NewTwitchAPIServices(this);
        }

        /// <summary>
        /// Refreshs the current OAuth token.
        /// </summary>
        /// <returns>An awaitable Task</returns>
        public async Task RefreshOAuthToken() { this.token = await this.OAuth.RefreshToken(this.token); }

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
    }
}
