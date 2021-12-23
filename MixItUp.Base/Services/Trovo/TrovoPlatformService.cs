using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using StreamingClient.Base.Model.OAuth;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trovo.Base;
using Trovo.Base.Models.Category;
using Trovo.Base.Models.Channels;
using Trovo.Base.Models.Chat;
using Trovo.Base.Models.Users;

namespace MixItUp.Base.Services.Trovo
{
    public class TrovoPlatformService : StreamingPlatformServiceBase
    {
        public const string ClientID = "8FMjuk785AX4FMyrwPTU3B8vYvgHWN33";

        public static readonly List<OAuthClientScopeEnum> StreamerScopes = new List<OAuthClientScopeEnum>()
        {
            OAuthClientScopeEnum.chat_connect,
            OAuthClientScopeEnum.chat_send_self,
            OAuthClientScopeEnum.send_to_my_channel,
            OAuthClientScopeEnum.manage_messages,

            OAuthClientScopeEnum.channel_details_self,
            OAuthClientScopeEnum.channel_update_self,
            OAuthClientScopeEnum.channel_subscriptions,

            OAuthClientScopeEnum.user_details_self,
        };

        public static readonly List<OAuthClientScopeEnum> BotScopes = new List<OAuthClientScopeEnum>()
        {
            OAuthClientScopeEnum.chat_connect,
            OAuthClientScopeEnum.chat_send_self,
            OAuthClientScopeEnum.manage_messages,

            OAuthClientScopeEnum.user_details_self,
        };

        public static async Task<Result<TrovoPlatformService>> Connect(OAuthTokenModel token)
        {
            try
            {
                TrovoConnection connection = await TrovoConnection.ConnectViaOAuthToken(token);
                if (connection != null)
                {
                    return new Result<TrovoPlatformService>(new TrovoPlatformService(connection));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result<TrovoPlatformService>(ex);
            }
            return new Result<TrovoPlatformService>("Trovo OAuth token could not be used");
        }

        public static async Task<Result<TrovoPlatformService>> ConnectUser()
        {
            return await TrovoPlatformService.Connect(TrovoPlatformService.StreamerScopes);
        }

        public static async Task<Result<TrovoPlatformService>> ConnectBot()
        {
            return await TrovoPlatformService.Connect(TrovoPlatformService.BotScopes);
        }

        public static async Task<Result<TrovoPlatformService>> Connect(IEnumerable<OAuthClientScopeEnum> scopes)
        {
            try
            {
                TrovoConnection connection = await TrovoConnection.ConnectViaLocalhostOAuthBrowser(TrovoPlatformService.ClientID, scopes, forceApprovalPrompt: true, successResponse: OAuthExternalServiceBase.LoginRedirectPageHTML);
                if (connection != null)
                {
                    return new Result<TrovoPlatformService>(new TrovoPlatformService(connection));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result<TrovoPlatformService>(ex);
            }
            return new Result<TrovoPlatformService>("Failed to connect to establish connection to Glimesh");
        }

        public TrovoConnection Connection { get; private set; }

        public override string Name { get { return "Trovo Connection"; } }

        public TrovoPlatformService(TrovoConnection connection)
        {
            this.Connection = connection;
        }

        public async Task<PrivateUserModel> GetCurrentUser() { return await AsyncRunner.RunAsync(this.Connection.Users.GetCurrentUser()); }

        public async Task<UserModel> GetUserByName(string username) { return await AsyncRunner.RunAsync(this.Connection.Users.GetUser(username)); }

        public async Task<IEnumerable<ChannelFollowerModel>> GetFollowers(string channelID, int maxResults = 1) { return await AsyncRunner.RunAsync(this.Connection.Channels.GetFollowers(channelID, maxResults)); }

        public async Task<IEnumerable<ChannelSubscriberModel>> GetSubscribers(string channelID, int maxResults = 1) { return await AsyncRunner.RunAsync(this.Connection.Channels.GetSubscribers(channelID, maxResults)); }

        public async Task<PrivateChannelModel> GetCurrentChannel() { return await AsyncRunner.RunAsync(this.Connection.Channels.GetCurrentChannel()); }

        public async Task<ChannelModel> GetChannelByID(string channelID) { return await AsyncRunner.RunAsync(this.Connection.Channels.GetChannelByID(channelID)); }

        public async Task<bool> UpdateChannel(string id, string title = null, string categoryID = null, string langaugeCode = null, ChannelAudienceTypeEnum? audience = null) { return await AsyncRunner.RunAsync(this.Connection.Channels.UpdateChannel(id, title, categoryID, langaugeCode, audience)); }

        public async Task<IEnumerable<CategoryModel>> SearchCategories(string query, int maxResults = 1) { return await AsyncRunner.RunAsync(this.Connection.Categories.SearchCategories(query, maxResults)); }

        public async Task<string> GetChatToken() { return await AsyncRunner.RunAsync(this.Connection.Chat.GetToken()); }

        public async Task<string> GetChatToken(string channelID) { return await AsyncRunner.RunAsync(this.Connection.Chat.GetToken(channelID)); }

        public async Task<ChatEmotePackageModel> GetEmotes(string channelID) { return await AsyncRunner.RunAsync(this.Connection.Chat.GetEmotes(new List<string>() { channelID })); }

        public async Task<ChatViewersModel> GetViewers(string channelID, int maxResults = 1) { return await AsyncRunner.RunAsync(this.Connection.Chat.GetViewers(channelID)); }
    }
}