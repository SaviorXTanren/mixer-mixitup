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
    public class GlimeshPlatformService : StreamingPlatformServiceBase
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

        public static DateTimeOffset GetGlimeshDateTime(string dateTime)
        {
            try
            {
                if (!string.IsNullOrEmpty(dateTime))
                {
                    DateTimeOffset result = StreamingClient.Base.Util.DateTimeOffsetExtensions.FromUTCISO8601String(dateTime);
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
            return new Result<GlimeshPlatformService>(MixItUp.Base.Resources.GlimeshFailedToConnect);
        }

        public GlimeshConnection Connection { get; private set; }

        public override string Name { get { return MixItUp.Base.Resources.GlimeshConnection; } }

        public GlimeshPlatformService(GlimeshConnection connection)
        {
            this.Connection = connection;
        }

        public async Task<UserModel> GetCurrentUser() { return await AsyncRunner.RunAsync(this.Connection.Users.GetCurrentUser()); }

        public async Task<UserModel> GetUserByID(string userID) { return await AsyncRunner.RunAsync(this.Connection.Users.GetUserByID(userID)); }

        public async Task<UserModel> GetUserByName(string username) { return await AsyncRunner.RunAsync(this.Connection.Users.GetUserByName(username)); }

        public async Task<IEnumerable<UserFollowModel>> GetFollowingUsers(UserModel streamer, int maxResults = 1) { return await AsyncRunner.RunAsync(this.Connection.Users.GetFollowingUsers(streamer, maxResults)); }

        public async Task<UserFollowModel> GetFollowingUser(UserModel streamer, UserModel user) { return await AsyncRunner.RunAsync(this.Connection.Users.GetFollowingUser(streamer, user)); }

        public async Task<IEnumerable<UserFollowModel>> GetUsersFollowed(UserModel user, int maxResults = 1) { return await AsyncRunner.RunAsync(this.Connection.Users.GetUsersFollowed(user, maxResults)); }

        public async Task<ChannelModel> GetChannelByID(string channelID) { return await AsyncRunner.RunAsync(this.Connection.Channel.GetChannelByID(channelID)); }

        public async Task<ChannelModel> GetChannelByName(string username) { return await AsyncRunner.RunAsync(this.Connection.Channel.GetChannelByName(username)); }
    }
}
