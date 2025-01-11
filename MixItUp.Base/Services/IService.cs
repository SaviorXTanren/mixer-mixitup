using MixItUp.Base.Model;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Model.Web;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.Web;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IService
    {
        string Name { get; }

        bool IsEnabled { get; }

        bool IsConnected { get; }

        Task<Result> Connect();

        Task Disconnect();

        Task Disable();
    }

    public abstract class ServiceBase : IService
    {
        public abstract string Name { get; }

        public abstract bool IsEnabled { get; }

        public abstract bool IsConnected { get; protected set; }

        public abstract Task<Result> Connect();

        public abstract Task Disconnect();

        public abstract Task Disable();
    }

    public abstract class OAuthServiceBase : ServiceBase
    {
        public const string HTTPS_OAUTH_REDIRECT_URL = "https://mixitupapp.com/oauthredirect/";

        public abstract string ClientID { get; }
        public abstract string ClientSecret { get; }

        protected AdvancedHttpClient HttpClient { get; }

        protected OAuthTokenModel OAuthToken
        {
            get { return this.token; }
            set
            {
                this.token = value;
                this.HttpClient.SetBearerAuthorization(this.token);
            }
        }
        private OAuthTokenModel token;

        public OAuthServiceBase(string baseAddress)
        {
            this.HttpClient = new AdvancedHttpClient(baseAddress);
        }

        public virtual OAuthTokenModel GetOAuthTokenCopy()
        {
            if (this.OAuthToken != null)
            {
                return new OAuthTokenModel()
                {
                    clientID = this.OAuthToken.clientID,
                    ScopeList = this.OAuthToken.ScopeList,
                    refreshToken = this.OAuthToken.refreshToken,
                    accessToken = this.OAuthToken.accessToken,
                    expiresIn = this.OAuthToken.expiresIn,
                    AcquiredDateTime = this.OAuthToken.AcquiredDateTime,
                };
            }
            return null;
        }

        protected abstract Task<string> GetAuthorizationCodeURL(IEnumerable<string> scopes, string state, bool forceApprovalPrompt = false);

        protected abstract Task<OAuthTokenModel> RequestOAuthToken(string authorizationCode, IEnumerable<string> scopes, string state);

        protected abstract Task RefreshOAuthToken();

        protected async Task<Result> ConnectWithAuthorization(IEnumerable<string> scopes)
        {
            try
            {
                string state = Guid.NewGuid().ToString();
                string authorizationCode = await this.GetAuthorizationCode(scopes, state, forceApprovalPrompt: true);
                if (!string.IsNullOrEmpty(authorizationCode))
                {
                    OAuthTokenModel token = await this.RequestOAuthToken(authorizationCode, scopes, state);
                    if (token != null)
                    {
                        this.OAuthToken = token;
                        return new Result();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result(ex);
            }
            return new Result(success: false);
        }

        protected async Task<string> GetAuthorizationCode(IEnumerable<string> scopes, string state, bool forceApprovalPrompt = false)
        {
            LocalOAuthHttpListenerServer oauthServer = new LocalOAuthHttpListenerServer();
            return await oauthServer.GetAuthorizationCode(await this.GetAuthorizationCodeURL(scopes, state, forceApprovalPrompt));
        }

        protected async Task<OAuthTokenModel> RequestWWWFormUrlEncodedOAuthToken(string endpoint, List<KeyValuePair<string, string>> bodyContent)
        {
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient())
                {
                    if (!string.IsNullOrEmpty(this.ClientID) && !string.IsNullOrEmpty(this.ClientSecret))
                    {
                        client.SetEncodedBasicAuthorization(this.ClientID, this.ClientSecret);
                    }

                    using (var content = new FormUrlEncodedContent(bodyContent))
                    {
                        content.Headers.Clear();
                        content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                        HttpResponseMessage response = await client.PostAsync(endpoint, content);
                        return await response.ProcessResponse<OAuthTokenModel>();
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }
    }

    public abstract class StreamingPlatformServiceBaseNew : OAuthServiceBase
    {
        public abstract StreamingPlatformTypeEnum Platform { get; }

        private IEnumerable<string> scopes;

        public override bool IsEnabled { get { return true; } }

        public StreamingPlatformServiceBaseNew(string baseAddress, IEnumerable<string> scopes)
            : base(baseAddress)
        {
            this.scopes = scopes;
        }

        public async override Task<Result> Connect()
        {
            StreamingPlatformAuthenticationSettingsModel authenticationSettings = this.GetAuthenticationSettings();
            if (authenticationSettings?.IsEnabled ?? false)
            {
                this.OAuthToken = authenticationSettings.UserOAuthToken;
                try
                {
                    await this.RefreshOAuthToken();
                    return new Result();
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            }

            return await this.ConnectWithAuthorization(this.scopes);
        }

        public override Task Disconnect()
        {
            return Task.CompletedTask;
        }

        public override Task Disable()
        {
            return Task.CompletedTask;
        }

        public StreamingPlatformAuthenticationSettingsModel GetAuthenticationSettings()
        {
            ChannelSession.Settings.StreamingPlatformAuthentications.TryGetValue(this.Platform, out StreamingPlatformAuthenticationSettingsModel authentication);
            return authentication;
        }
    }

    public abstract class StreamingPlatformSessionBase
    {
        public class StreamingPlatformAccountModel
        {
            public string ID { get; set; }

            public string Username { get; set; }

            public string AvatarURL { get; set; }
        }

        public abstract int MaxMessageLength { get; }

        public abstract StreamingPlatformTypeEnum Platform { get; }

        public UserV2ViewModel Streamer { get; protected set; }
        public string StreamerID { get; protected set; }
        public string StreamerUsername { get; protected set; }
        public string StreamerAvatarURL { get; protected set; }

        public UserV2ViewModel Bot { get; protected set; }
        public string BotID { get; protected set; }
        public string BotUsername { get; protected set; }
        public string BotAvatarURL { get; protected set; }

        public string ChannelID { get; protected set; }
        public string ChannelLink { get; protected set; }
        public virtual string StreamLink { get { return this.ChannelLink; } }

        public bool IsLive { get; protected set; }

        public string StreamTitle { get; protected set; }
        public string StreamCategoryID { get; protected set; }
        public string StreamCategoryName { get; protected set; }
        public string StreamCategoryImageURL { get; protected set; }
        public DateTimeOffset StreamStart { get; protected set; }
        public int StreamViewerCount { get; protected set; }

        public bool IsEnabled
        {
            get
            {
                StreamingPlatformAuthenticationSettingsModel authenticationSettings = this.GetAuthenticationSettings();
                return authenticationSettings?.IsEnabled ?? false;
            }
        }
        public bool IsBotEnabled
        {
            get
            {
                StreamingPlatformAuthenticationSettingsModel authenticationSettings = this.GetAuthenticationSettings();
                return authenticationSettings?.IsBotEnabled ?? false;
            }
        }

        public bool IsConnected { get; private set; }
        public bool IsBotConnected { get; private set; }

        protected abstract OAuthTokenModel StreamerOAuthToken { get; }
        protected abstract OAuthTokenModel BotOAuthToken { get; }

        public StreamingPlatformSessionBase() { }

        public async Task<Result> ConnectStreamer()
        {
            Result result = await this.ConnectStreamerInternal();

            this.IsConnected = result.Success;
            if (result.Success)
            {
                result = await this.RefreshDetails();
            }

            if (result.Success)
            {
                this.SaveAuthenticationSettings();
            }
            else
            {
                await this.DisconnectStreamer();
            }

            return result;
        }

        public async Task DisconnectStreamer()
        {
            this.IsConnected = false;
            await this.DisconnectStreamerInternal();
        }

        public async Task DisableStreamer()
        {
            await this.DisconnectStreamer();

            if (ChannelSession.Settings.StreamingPlatformAuthentications.TryGetValue(this.Platform, out StreamingPlatformAuthenticationSettingsModel streamingPlatformAuth))
            {
                streamingPlatformAuth.ClearUserData();
            }
        }

        protected abstract Task<Result> ConnectStreamerInternal();
        protected abstract Task DisconnectStreamerInternal();

        public async Task<Result> ConnectBot()
        {
            Result result = await this.ConnectBot();
            this.IsBotConnected = result.Success;

            if (result.Success)
            {
                this.SaveAuthenticationSettings();
            }
            else
            {
                await this.DisconnectBot();
            }

            return result;
        }

        public async Task DisconnectBot()
        {
            this.IsBotConnected = false;
            await this.DisconnectBotInternal();
        }

        public async Task DisableBot()
        {
            await this.DisconnectBot();

            if (ChannelSession.Settings.StreamingPlatformAuthentications.TryGetValue(this.Platform, out StreamingPlatformAuthenticationSettingsModel streamingPlatformAuth))
            {
                streamingPlatformAuth.ClearBotData();
            }
        }

        protected abstract Task<Result> ConnectBotInternal();
        protected abstract Task DisconnectBotInternal();

        public abstract Task<Result> RefreshDetails();

        public abstract Task<Result> SetStreamTitle(string title);

        public abstract Task<Result> SetStreamCategory(string category);

        public abstract Task SendMessage(string message, bool sendAsStreamer = false);

        public abstract Task DeleteMessage(ChatMessageViewModel message);

        public abstract Task ClearMessages();

        public abstract Task TimeoutUser(UserV2ViewModel user, int durationInSeconds, string reason = null);

        public abstract Task ModUser(UserV2ViewModel user);

        public abstract Task UnmodUser(UserV2ViewModel user);

        public abstract Task BanUser(UserV2ViewModel user, string reason = null);

        public abstract Task UnbanUser(UserV2ViewModel user);

        protected IEnumerable<string> SplitLargeMessage(string message)
        {
            List<string> messages = new List<string>();

            do
            {
                message = ChatService.SplitLargeMessage(message, MaxMessageLength, out string subMessage);
                messages.Add(message);
                message = subMessage;
            }
            while (!string.IsNullOrEmpty(message));

            return messages;
        }

        public StreamingPlatformAuthenticationSettingsModel GetAuthenticationSettings()
        {
            ChannelSession.Settings.StreamingPlatformAuthentications.TryGetValue(this.Platform, out StreamingPlatformAuthenticationSettingsModel authentication);
            return authentication;
        }

        public void SaveAuthenticationSettings()
        {
            ChannelSession.Settings.StreamingPlatformAuthentications[this.Platform] = new StreamingPlatformAuthenticationSettingsModel(this.Platform)
            {
                UserID = this.StreamerID,
                UserOAuthToken = this.StreamerOAuthToken,

                ChannelID = this.ChannelID,

                BotID = this.BotID,
                BotOAuthToken = this.BotOAuthToken,
            };
        }
    }

    public abstract class ServiceClientBase
    {
        public abstract bool IsConnected { get; }

        protected SemaphoreSlim reconnectSemaphore = new SemaphoreSlim(1);

        public abstract Task<Result> Connect();

        public abstract Task Disconnect();

        public virtual async Task Reconnect()
        {
            await this.reconnectSemaphore.WaitAsync();

            if (!this.IsConnected)
            {
                await this.Disconnect();

                do
                {
                    await Task.Delay(2000);

                    Result result = await this.Connect();
                    if (!result.Success)
                    {
                        await this.Disconnect();
                    }
                } while (!this.IsConnected);
            }

            this.reconnectSemaphore.Release();
        }
    }
}
