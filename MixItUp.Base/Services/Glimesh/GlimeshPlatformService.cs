using Glimesh.Base;
using Glimesh.Base.Models.Channels;
using Glimesh.Base.Models.Users;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using StreamingClient.Base.Model.OAuth;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Glimesh
{
    public interface IGlimeshPlatformService
    {

    }

    public class GlimeshPlatformService : StreamingPlatformServiceBase, IGlimeshPlatformService
    {
        public const string ClientID = "7ca2da26850dcc9395a565d920783d2bc859173894e4c0592542b518dccca1c3";

        public static readonly List<OAuthClientScopeEnum> StreamerScopes = new List<OAuthClientScopeEnum>()
        {
            OAuthClientScopeEnum.publicinfo,
            OAuthClientScopeEnum.chat,
        };

        public static readonly List<OAuthClientScopeEnum> BotScopes = new List<OAuthClientScopeEnum>()
        {
            OAuthClientScopeEnum.publicinfo,
            OAuthClientScopeEnum.chat,
        };

        public static DateTimeOffset? GetGlimeshDateTime(string dateTime)
        {
            if (!string.IsNullOrEmpty(dateTime))
            {
                return StreamingClient.Base.Util.DateTimeOffsetExtensions.FromUTCISO8601String(dateTime);
            }
            return null;
        }

        public static async Task<Result<GlimeshPlatformService>> Connect(OAuthTokenModel token)
        {
            try
            {
                GlimeshConnection connection = await GlimeshConnection.ConnectViaOAuthToken(token);
                if (connection != null)
                {
                    return new Result<GlimeshPlatformService>(new GlimeshPlatformService(connection));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result<GlimeshPlatformService>(ex);
            }
            return new Result<GlimeshPlatformService>("Glimesh OAuth token could not be used");
        }

        public static async Task<Result<GlimeshPlatformService>> ConnectUser()
        {
            return await GlimeshPlatformService.Connect(GlimeshPlatformService.StreamerScopes);
        }

        public static async Task<Result<GlimeshPlatformService>> ConnectBot()
        {
            return await GlimeshPlatformService.Connect(GlimeshPlatformService.BotScopes);
        }

        public static async Task<Result<GlimeshPlatformService>> Connect(IEnumerable<OAuthClientScopeEnum> scopes)
        {
            try
            {
                GlimeshConnection connection = await GlimeshConnection.ConnectViaLocalhostOAuthBrowser(GlimeshPlatformService.ClientID, ServiceManager.Get<SecretsService>().GetSecret("GlimeshSecret"),
                    scopes, forceApprovalPrompt: true, successResponse: OAuthExternalServiceBase.LoginRedirectPageHTML);
                if (connection != null)
                {
                    return new Result<GlimeshPlatformService>(new GlimeshPlatformService(connection));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result<GlimeshPlatformService>(ex);
            }
            return new Result<GlimeshPlatformService>("Failed to connect to establish connection to Glimesh");
        }

        public GlimeshConnection Connection { get; private set; }

        public override string Name { get { return "Glimesh Connection"; } }

        public GlimeshPlatformService(GlimeshConnection connection)
        {
            this.Connection = connection;
        }

        public async Task<UserModel> GetCurrentUser() { return await AsyncRunner.RunAsync(this.Connection.Users.GetCurrentUser()); }

        public async Task<UserModel> GetUserByID(string userID) { return await AsyncRunner.RunAsync(this.Connection.Users.GetUserByID(userID)); }

        public async Task<UserModel> GetUserByName(string username) { return await AsyncRunner.RunAsync(this.Connection.Users.GetUserByName(username)); }

        public async Task<IEnumerable<UserFollowModel>> GetFollowingUsers(string username) { return await AsyncRunner.RunAsync(this.Connection.Users.GetFollowingUsers(username)); }

        public async Task<IEnumerable<UserFollowModel>> GetUsersFollowed(string username) { return await AsyncRunner.RunAsync(this.Connection.Users.GetUsersFollowed(username)); }

        public async Task<IEnumerable<UserSubscriptionModel>> GetSubscribedUsers(string username) { return await AsyncRunner.RunAsync(this.Connection.Users.GetSubscribedUsers(username)); }

        public async Task<IEnumerable<UserSubscriptionModel>> GetUsersSubscribedTo(string username) { return await AsyncRunner.RunAsync(this.Connection.Users.GetUsersSubscribedTo(username)); }

        public async Task<ChannelModel> GetChannelByID(string channelID) { return await AsyncRunner.RunAsync(this.Connection.Channel.GetChannelByID(channelID)); }

        public async Task<ChannelModel> GetChannelByName(string username) { return await AsyncRunner.RunAsync(this.Connection.Channel.GetChannelByName(username)); }
    }
}
