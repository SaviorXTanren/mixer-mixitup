using Google.Apis.YouTube.v3.Data;
using Google.Apis.YouTubePartner.v1.Data;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using StreamingClient.Base.Model.OAuth;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YouTube.Base;

namespace MixItUp.Base.Services.YouTube
{
    public class YouTubePlatformService : StreamingPlatformServiceBase
    {
        public const string ClientID = "284178717531-kago2rk85ip02qb0vmlo8898m17s6oo8.apps.googleusercontent.com";

        public static readonly List<OAuthClientScopeEnum> StreamerScopes = new List<OAuthClientScopeEnum>()
        {
            OAuthClientScopeEnum.ChannelMemberships,
            OAuthClientScopeEnum.ManageAccount,
            OAuthClientScopeEnum.ManageData,
            OAuthClientScopeEnum.ManagePartner,
            OAuthClientScopeEnum.ReadOnlyAccount,
            OAuthClientScopeEnum.ViewAnalytics
        };

        public static readonly List<OAuthClientScopeEnum> BotScopes = new List<OAuthClientScopeEnum>()
        {
            OAuthClientScopeEnum.ManageAccount,
            OAuthClientScopeEnum.ManageData,
            OAuthClientScopeEnum.ReadOnlyAccount,
        };

        public static async Task<Result<YouTubePlatformService>> Connect(OAuthTokenModel token)
        {
            try
            {
                YouTubeConnection connection = await YouTubeConnection.ConnectViaOAuthToken(token);
                if (connection != null)
                {
                    return new Result<YouTubePlatformService>(new YouTubePlatformService(connection));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result<YouTubePlatformService>(ex);
            }
            return new Result<YouTubePlatformService>("YouTube OAuth token could not be used");
        }

        public static async Task<Result<YouTubePlatformService>> ConnectUser()
        {
            return await YouTubePlatformService.Connect(YouTubePlatformService.StreamerScopes);
        }

        public static async Task<Result<YouTubePlatformService>> ConnectBot()
        {
            return await YouTubePlatformService.Connect(YouTubePlatformService.BotScopes);
        }

        public static async Task<Result<YouTubePlatformService>> Connect(IEnumerable<OAuthClientScopeEnum> scopes)
        {
            try
            {
                YouTubeConnection connection = await YouTubeConnection.ConnectViaLocalhostOAuthBrowser(YouTubePlatformService.ClientID, ServiceManager.Get<SecretsService>().GetSecret("YouTubeSecret"),
                    scopes, forceApprovalPrompt: true, successResponse: OAuthExternalServiceBase.LoginRedirectPageHTML);
                if (connection != null)
                {
                    return new Result<YouTubePlatformService>(new YouTubePlatformService(connection));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result<YouTubePlatformService>(ex);
            }
            return new Result<YouTubePlatformService>(MixItUp.Base.Resources.YouTubeFailedToConnect);
        }

        public YouTubeConnection Connection { get; private set; }

        public override string Name { get { return "YouTube Connection"; } }

        public YouTubePlatformService(YouTubeConnection connection)
        {
            this.Connection = connection;
        }

        public async Task<Channel> GetCurrentChannel() { return await AsyncRunner.RunAsync(this.Connection.Channels.GetMyChannel()); }

        public async Task<Channel> GetChannelByID(string id) { return await AsyncRunner.RunAsync(this.Connection.Channels.GetChannelByID(id)); }

        public async Task<Channel> GetChannelByUsername(string username) { return await AsyncRunner.RunAsync(this.Connection.Channels.GetChannelByUsername(username)); }

        public async Task<LiveBroadcast> GetMyActiveBroadcast() { return await AsyncRunner.RunAsync(this.Connection.LiveBroadcasts.GetMyActiveBroadcast()); }

        public async Task<LiveCuepoint> StartAdBreak(LiveBroadcast broadcast, long duration) { return await AsyncRunner.RunAsync(this.Connection.LiveBroadcasts.StartAdBreak(broadcast, duration)); }

        public async Task<IEnumerable<Member>> GetChannelMemberships(int maxResults = 1) { return await AsyncRunner.RunAsync(this.Connection.LiveChat.GetChannelMemberships(maxResults)); }

        public async Task<IEnumerable<LiveChatModerator>> GetModerators(LiveBroadcast broadcast, int maxResults = 1) { return await AsyncRunner.RunAsync(this.Connection.LiveChat.GetModerators(broadcast, maxResults)); }
    }
}
