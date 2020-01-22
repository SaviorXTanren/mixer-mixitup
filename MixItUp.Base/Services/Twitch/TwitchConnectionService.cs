using MixItUp.Base.Util;
using StreamingClient.Base.Model.OAuth;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twitch.Base;
using NewAPI = Twitch.Base.Models.NewAPI.Users;

namespace MixItUp.Base.Services.Twitch
{
    public interface ITwitchConnectionService
    {

    }

    public class TwitchConnectionService : AsyncRequestServiceBase, ITwitchConnectionService
    {
        public const string ClientID = "50ipfqzuqbv61wujxcm80zyzqwoqp1";

        public static readonly List<OAuthClientScopeEnum> StreamerScopes = new List<OAuthClientScopeEnum>()
        {
            OAuthClientScopeEnum.channel_commercial,
            OAuthClientScopeEnum.channel_editor,
            OAuthClientScopeEnum.channel_read,
            OAuthClientScopeEnum.channel_subscriptions,

            OAuthClientScopeEnum.user_read,

            OAuthClientScopeEnum.bits__read,
            OAuthClientScopeEnum.channel__moderate,
            OAuthClientScopeEnum.chat__edit,
            OAuthClientScopeEnum.chat__read,
            OAuthClientScopeEnum.user__edit,
            OAuthClientScopeEnum.whispers__read,
            OAuthClientScopeEnum.whispers__edit,
        };

        public static readonly List<OAuthClientScopeEnum> ModeratorScopes = new List<OAuthClientScopeEnum>()
        {
            OAuthClientScopeEnum.channel_editor,
            OAuthClientScopeEnum.channel_read,

            OAuthClientScopeEnum.user_read,

            OAuthClientScopeEnum.bits__read,
            OAuthClientScopeEnum.channel__moderate,
            OAuthClientScopeEnum.chat__edit,
            OAuthClientScopeEnum.chat__read,
            OAuthClientScopeEnum.user__edit,
            OAuthClientScopeEnum.whispers__read,
            OAuthClientScopeEnum.whispers__edit,
        };

        public static readonly List<OAuthClientScopeEnum> BotScopes = new List<OAuthClientScopeEnum>()
        {
            OAuthClientScopeEnum.channel_editor,
            OAuthClientScopeEnum.channel_read,

            OAuthClientScopeEnum.user_read,

            OAuthClientScopeEnum.bits__read,
            OAuthClientScopeEnum.channel__moderate,
            OAuthClientScopeEnum.chat__edit,
            OAuthClientScopeEnum.chat__read,
            OAuthClientScopeEnum.user__edit,
            OAuthClientScopeEnum.whispers__read,
            OAuthClientScopeEnum.whispers__edit,
        };

        public TwitchConnection Connection { get; private set; }

        public TwitchConnectionService() { }

        public async Task<bool> ConnectAsStreamer() { return await this.Connect(TwitchConnectionService.StreamerScopes); }

        public async Task<bool> ConnectAsModerator() { return await this.Connect(TwitchConnectionService.ModeratorScopes); }

        public async Task<bool> ConnectAsBot() { return await this.Connect(TwitchConnectionService.BotScopes); }

        public async Task<bool> Connect(OAuthTokenModel token)
        {
            try
            {
                this.Connection = await TwitchConnection.ConnectViaOAuthToken(token);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return (this.Connection != null);
        }

        public async Task<NewAPI.UserModel> GetNewAPICurrentUser() { return await this.RunAsync(this.Connection.NewAPI.Users.GetCurrentUser()); }

        public async Task<NewAPI.UserModel> GetNewAPIUserByID(string userID) { return await this.RunAsync(this.Connection.NewAPI.Users.GetUserByID(userID)); }

        public async Task<NewAPI.UserModel> GetNewAPIUserByLogin(string login) { return await this.RunAsync(this.Connection.NewAPI.Users.GetUserByLogin(login)); }

        private async Task<bool> Connect(IEnumerable<OAuthClientScopeEnum> scopes)
        {
            try
            {
                this.Connection = await TwitchConnection.ConnectViaLocalhostOAuthBrowser(TwitchConnectionService.ClientID, ChannelSession.Services.Secrets.GetSecret("TwitchSecret"), scopes,
                    forceApprovalPrompt: true, successResponse: OAuthServiceBase.LoginRedirectPageHTML);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return (this.Connection != null);
        }
    }
}