using MixItUp.Base.Model.Trovo.Category;
using MixItUp.Base.Model.Trovo.Channels;
using MixItUp.Base.Model.Trovo.Chat;
using MixItUp.Base.Model.Trovo.Users;
using MixItUp.Base.Model.Web;
using MixItUp.Base.Services.External;
using MixItUp.Base.Services.Trovo.API;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Trovo
{
    [Obsolete]
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
            OAuthClientScopeEnum.send_to_my_channel,
            OAuthClientScopeEnum.manage_messages,

            OAuthClientScopeEnum.user_details_self,
        };

        public static DateTimeOffset GetTrovoDateTime(string dateTime)
        {
            try
            {
                if (!string.IsNullOrEmpty(dateTime) && long.TryParse(dateTime, out long seconds))
                {
                    DateTimeOffset result = DateTimeOffsetExtensions.FromUTCUnixTimeSeconds(seconds);
                    if (result > DateTimeOffset.MinValue)
                    {
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"{dateTime} - {ex}");
            }
            return DateTimeOffset.MinValue;
        }

        public static async Task<Result<TrovoPlatformService>> Connect(OAuthTokenModel token)
        {
            try
            {
                TrovoConnection connection = await TrovoConnection.ConnectViaOAuthToken(token);
                if (connection != null)
                {
                    var service = new TrovoPlatformService(connection);

                    // Attempt to load the user data, this will 401 if the token has been revoked
                    // Then we'll re-prompt to log in
                    var currentUser = await service.GetCurrentUser();
                    if (currentUser != null)
                    {
                        return new Result<TrovoPlatformService>(service);
                    }
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
                TrovoConnection connection = await TrovoConnection.ConnectViaLocalhostOAuthBrowser(TrovoPlatformService.ClientID, ServiceManager.Get<SecretsService>().GetSecret("TrovoSecret"), scopes, forceApprovalPrompt: true);
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
            return new Result<TrovoPlatformService>(MixItUp.Base.Resources.TrovoFailedToConnect);
        }

        public TrovoConnection Connection { get; private set; }

        public override string Name { get { return MixItUp.Base.Resources.TrovoConnection; } }

        public TrovoPlatformService(TrovoConnection connection)
        {
            this.Connection = connection;
        }

        public async Task<PrivateUserModel> GetCurrentUser() { return await AsyncRunner.RunAsync(this.Connection.Users.GetCurrentUser()); }

        public async Task<UserModel> GetUserByName(string username) { return await AsyncRunner.RunAsync(this.Connection.Users.GetUser(username)); }

        public async Task<IEnumerable<ChannelFollowerModel>> GetFollowers(string channelID, int maxResults = 1) { return await AsyncRunner.RunAsync(this.Connection.Channels.GetFollowers(channelID, maxResults)); }

        public async Task<IEnumerable<ChannelSubscriberModel>> GetSubscribers(string channelID, int maxResults = 1) { return await AsyncRunner.RunAsync(this.Connection.Channels.GetSubscribers(channelID, maxResults)); }

        //public async Task<PrivateChannelModel> GetCurrentChannel() { return await AsyncRunner.RunAsync(this.Connection.Channels.GetCurrentChannel()); }

        public async Task<ChannelModel> GetChannelByID(string channelID) { return await AsyncRunner.RunAsync(this.Connection.Channels.GetChannelByID(channelID)); }

        public async Task<bool> UpdateChannel(string id, string title = null, string categoryID = null, string langaugeCode = null, ChannelAudienceTypeEnum? audience = null) { return await AsyncRunner.RunAsync(this.Connection.Channels.UpdateChannel(id, title, categoryID, langaugeCode, audience)); }

        public async Task<IEnumerable<CategoryModel>> SearchCategories(string query, int maxResults = 1) { return await AsyncRunner.RunAsync(this.Connection.Categories.SearchCategories(query, maxResults)); }

        public async Task<string> GetChatToken() { return await AsyncRunner.RunAsync(this.Connection.Chat.GetToken()); }

        public async Task<string> GetChatToken(string channelID) { return await AsyncRunner.RunAsync(this.Connection.Chat.GetToken(channelID)); }

        public async Task<ChatEmotePackageModel> GetPlatformEmotes() { return await AsyncRunner.RunAsync(this.Connection.Chat.GetEmotes()); }

        public async Task<ChatEmotePackageModel> GetPlatformAndChannelEmotes(string channelID) { return await AsyncRunner.RunAsync(this.Connection.Chat.GetEmotes(new List<string>() { channelID })); }

        public async Task<ChatViewersModel> GetViewers(string channelID) { return await AsyncRunner.RunAsync(this.Connection.Chat.GetViewers(channelID, maxResults: 1000)); }

        public async Task<IEnumerable<TopChannelModel>> GetTopChannels(int maxResults = 1, string category = null) { return await AsyncRunner.RunAsync(this.Connection.Channels.GetTopChannels(maxResults, category)); }
    }
}
