using Google.Apis.YouTube.v3.Data;
using Google.Apis.YouTubePartner.v1.Data;
using MixItUp.Base.Model.Web;
using MixItUp.Base.Model.YouTube;
using MixItUp.Base.Services.External;
using MixItUp.Base.Services.YouTube.API;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.YouTube
{
    [Obsolete]
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

        private SearchResult latestNonStreamVideo;

        public YouTubePlatformService(YouTubeConnection connection)
        {
            this.Connection = connection;
        }

        public async Task<Channel> GetCurrentChannel() { return await AsyncRunner.RunAsync(this.Connection.Channels.GetMyChannel()); }

        public async Task<Channel> GetChannelByID(string id) { return await AsyncRunner.RunAsync(this.Connection.Channels.GetChannelByID(id)); }

        public async Task<Channel> GetChannelByUsername(string username) { return await AsyncRunner.RunAsync(this.Connection.Channels.GetChannelByUsername(username)); }

        public async Task<LiveBroadcast> GetMyActiveBroadcast() { return await AsyncRunner.RunAsync(this.Connection.LiveBroadcasts.GetMyActiveBroadcast()); }

        public async Task<LiveBroadcast> GetBroadcastByID(string id) { return await AsyncRunner.RunAsync(this.Connection.LiveBroadcasts.GetBroadcastByID(id)); }

        public async Task<LiveCuepoint> StartAdBreak(LiveBroadcast broadcast, long duration) { return await AsyncRunner.RunAsync(this.Connection.LiveBroadcasts.StartAdBreak(broadcast, duration)); }

        public async Task<Video> GetVideoByID(string id) { return await AsyncRunner.RunAsync(this.Connection.Videos.GetVideoByID(id)); }

        public async Task<SearchResult> GetLatestNonStreamVideo(string channelID)
        {
            if (this.latestNonStreamVideo == null)
            {
                IEnumerable<SearchResult> searchResults = await AsyncRunner.RunAsync(this.Connection.Videos.SearchVideos(channelID: channelID, liveType: Google.Apis.YouTube.v3.SearchResource.ListRequest.EventTypeEnum.None, maxResults: 10));
                this.latestNonStreamVideo = searchResults.FirstOrDefault(s => string.Equals(s.Snippet.LiveBroadcastContent, "none"));
            }
            return this.latestNonStreamVideo;
        }

        public async Task<Video> UpdateVideo(Video video, string title = null, string description = null, string categoryId = null) { return await AsyncRunner.RunAsync(this.Connection.Videos.UpdateVideo(video, title, description, categoryId)); }

        public async Task<IEnumerable<LiveChatModerator>> GetModerators(LiveBroadcast broadcast, int maxResults = 1) { return await AsyncRunner.RunAsync(this.Connection.LiveChat.GetModerators(broadcast, maxResults)); }

        public async Task<IEnumerable<Subscription>> GetSubscribers(string channelID, int maxResults = 1) { return await AsyncRunner.RunAsync(this.Connection.Subscriptions.GetMyRecentSubscribers(maxResults)); }

        public async Task<Subscription> CheckIfSubscribed(string channelID, string userID) { return await AsyncRunner.RunAsync(this.Connection.Subscriptions.CheckIfSubscribed(channelID, userID)); }

        public async Task<IEnumerable<MembershipsLevel>> GetMembershipLevels() { return await AsyncRunner.RunAsync(this.Connection.Membership.GetMyMembershipLevels()); }

        public async Task<IEnumerable<Member>> GetChannelMemberships(int maxResults = 1) { return await AsyncRunner.RunAsync(this.Connection.LiveChat.GetChannelMemberships(maxResults)); }

        public async Task<IEnumerable<Member>> GetMembers(int maxResults = 1) { return await AsyncRunner.RunAsync(this.Connection.Membership.GetMembers(maxResults)); }

        public async Task<Member> CheckIfMember(string userID) { return await AsyncRunner.RunAsync(this.Connection.Membership.CheckIfMember(userID)); }

        // Chat

        public async Task<LiveChatMessagesResultModel> GetChatMessages(LiveBroadcast broadcast, string nextResultsToken = null) { return await AsyncRunner.RunAsync(this.Connection.LiveChat.GetMessages(broadcast, nextResultsToken: nextResultsToken)); }

        public async Task<LiveChatMessage> SendChatMessage(LiveBroadcast broadcast, string message) { return await AsyncRunner.RunAsync(this.Connection.LiveChat.SendMessage(broadcast, message)); }

        public async Task DeleteChatMessage(LiveChatMessage message) { await AsyncRunner.RunAsync(this.Connection.LiveChat.DeleteMessage(message)); }

        public async Task<LiveChatModerator> ModChatUser(LiveBroadcast broadcast, Channel user) { return await AsyncRunner.RunAsync(this.Connection.LiveChat.ModUser(broadcast, user)); }

        public async Task UnmodChatUser(LiveChatModerator moderator) { await AsyncRunner.RunAsync(this.Connection.LiveChat.UnmodUser(moderator)); }

        public async Task<LiveChatBan> TimeoutChatUser(LiveBroadcast broadcast, Channel user, ulong duration) { return await AsyncRunner.RunAsync(this.Connection.LiveChat.TimeoutUser(broadcast, user, duration)); }

        public async Task<LiveChatBan> BanChatUser(LiveBroadcast broadcast, Channel user) { return await AsyncRunner.RunAsync(this.Connection.LiveChat.BanUser(broadcast, user)); }

        public async Task UnbanChatUser(LiveChatBan ban) { await AsyncRunner.RunAsync(this.Connection.LiveChat.UnbanUser(ban)); }
    }
}
