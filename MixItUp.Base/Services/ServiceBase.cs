using MixItUp.Base.Model;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Model.Web;
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
    public abstract class ServiceBase
    {
        public abstract string Name { get; }

        public abstract bool IsEnabled { get; }

        public abstract bool IsConnected { get; protected set; }

        public virtual async Task<Result> AutomaticConnect() { return await this.ManualConnect(CancellationToken.None); }

        public abstract Task<Result> ManualConnect(CancellationToken cancellationToken);

        public async Task<Result> ManualConnectWithTimeout()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            Task<Result> result = AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
            {
                return await this.ManualConnect(cancellationTokenSource.Token);
            }, cancellationTokenSource.Token);

            await Task.WhenAny(result, Task.Delay(60000));

            if (!result.IsCompleted && !result.IsFaulted)
            {
                cancellationTokenSource.Cancel();
            }

            return result.Result;
        }

        public abstract Task Disconnect();

        public abstract Task Disable();
    }

    public abstract class OAuthServiceBase : ServiceBase
    {
        public const string HTTPS_OAUTH_REDIRECT_URL = "https://mixitupapp.com/oauthredirect/";

        public abstract string ClientID { get; }
        public abstract string ClientSecret { get; }

        protected AdvancedHttpClient HttpClient { get; }

        protected virtual OAuthTokenModel OAuthToken
        {
            get { return this.token; }
            set
            {
                this.token = value;
                if (value != null)
                {
                    this.HttpClient.SetBearerAuthorization(this.token);
                }
                else
                {
                    this.HttpClient.RemoveAuthorization();
                }
            }
        }
        private OAuthTokenModel token;

        public OAuthServiceBase(string baseAddress)
        {
            this.HttpClient = new AdvancedHttpClient(baseAddress);
        }

        public override Task Disconnect()
        {
            this.IsConnected = false;

            return Task.CompletedTask;
        }

        public override async Task Disable()
        {
            await this.Disconnect();

            this.OAuthToken = null;
        }

        public async Task RefreshOAuthTokenIfCloseToExpiring()
        {
            if (this.IsConnected && this.OAuthToken != null && this.OAuthToken.TimeUntilExpiration.TotalMinutes < 15)
            {
                Logger.Log(LogLevel.Debug, $"OAuth Token close to expiring for {this.Name}, refreshing now");

                await this.RefreshOAuthToken();
            }
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

        protected async Task<Result> ConnectWithAuthorization(IEnumerable<string> scopes, CancellationToken cancellationToken)
        {
            try
            {
                string state = Guid.NewGuid().ToString();
                Result<string> authorizationCode = await this.GetAuthorizationCode(scopes, state, cancellationToken, forceApprovalPrompt: true);
                if (!authorizationCode.Success || string.IsNullOrWhiteSpace(authorizationCode.Value))
                {
                    return new Result(authorizationCode.Message);
                }

                OAuthTokenModel token = await this.RequestOAuthToken(authorizationCode.Value, scopes, state);
                if (token != null)
                {
                    this.OAuthToken = token;
                    return new Result();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result(ex);
            }
            return new Result(success: false);
        }

        protected async Task<Result<string>> GetAuthorizationCode(IEnumerable<string> scopes, string state, CancellationToken cancellationToken, bool forceApprovalPrompt = false)
        {
            LocalOAuthHttpListenerServer oauthServer = new LocalOAuthHttpListenerServer();
            return await oauthServer.GetAuthorizationCode(await this.GetAuthorizationCodeURL(scopes, state, forceApprovalPrompt), cancellationToken);
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
        private bool isBotService;

        public override bool IsEnabled { get { return true; } }

        public StreamingPlatformServiceBaseNew(string baseAddress, IEnumerable<string> scopes, bool isBotService = false)
            : base(baseAddress)
        {
            this.scopes = scopes;
            this.isBotService = isBotService;
        }

        public async override Task<Result> AutomaticConnect()
        {
            StreamingPlatformAuthenticationSettingsModel authenticationSettings = this.GetAuthenticationSettings();
            if (authenticationSettings?.IsEnabled ?? false)
            {
                this.OAuthToken = this.isBotService ? authenticationSettings.BotOAuthToken : authenticationSettings.UserOAuthToken;
                try
                {
                    string requestedScopes = OAuthTokenModel.GenerateScopeList(this.scopes);
                    if (!string.Equals(requestedScopes, this.OAuthToken.ScopeList, StringComparison.Ordinal))
                    {
                        Logger.Log(LogLevel.Error, $"Scope list mis-match for {this.Name}, forcing manual connect");
                        return new Result(success: false, Resources.NewAuthorizationScopesRequired);
                    }

                    await this.RefreshOAuthToken();
                    this.IsConnected = true;
                    return new Result();
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    return new Result(ex);
                }
            }
            return new Result(success: false);
        }

        public async override Task<Result> ManualConnect(CancellationToken cancellationToken)
        {
            Result result = await this.ConnectWithAuthorization(this.scopes, cancellationToken);
            if (result.Success)
            {
                this.IsConnected = true;
            }
            return result;
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

        public abstract OAuthServiceBase StreamerOAuthService { get; }
        public abstract OAuthServiceBase BotOAuthService { get; }

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

        public StreamingPlatformSessionBase() { }

        public async Task<Result> AutomaticConnectStreamer()
        {
            try
            {
                Result result = await this.StreamerOAuthService.AutomaticConnect();
                if (result.Success)
                {
                    return await this.InitializeStreamer();
                }
                return result;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result(ex);
            }
        }

        public async Task<Result> ManualConnectStreamer(CancellationToken cancellationToken)
        {
            try
            {
                Result result = await this.StreamerOAuthService.ManualConnect(cancellationToken);
                if (!cancellationToken.IsCancellationRequested && result.Success)
                {
                    return await this.InitializeStreamer();
                }
                return result;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result(ex);
            }
        }

        public async Task<Result> ManualConnectStreamerWithTimeout()
        {
            try
            {
                Result result = await this.StreamerOAuthService.ManualConnectWithTimeout();
                if (result.Success)
                {
                    return await this.InitializeStreamer();
                }
                return result;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result(ex);
            }
        }

        protected async Task<Result> InitializeStreamer()
        {
            Result result = await this.InitializeStreamerInternal();

            if (result.Success)
            {
                result = await this.RefreshDetails();

                this.IsConnected = result.Success;
                if (result.Success)
                {
                    this.SaveAuthenticationSettings();
                }
                else
                {
                    await this.DisconnectStreamer();
                }
            }

            return result;
        }

        public async Task DisconnectStreamer()
        {
            this.IsConnected = false;

            await this.StreamerOAuthService.Disconnect();

            await this.DisconnectStreamerInternal();
        }

        public async Task DisableStreamer()
        {
            await this.DisconnectStreamer();

            await this.StreamerOAuthService.Disable();

            if (ChannelSession.Settings.StreamingPlatformAuthentications.TryGetValue(this.Platform, out StreamingPlatformAuthenticationSettingsModel streamingPlatformAuth))
            {
                streamingPlatformAuth.ClearUserData();
            }

            this.Streamer = null;
            this.StreamerID = null;
            this.StreamerUsername = null;
            this.StreamerAvatarURL = null;

            this.ChannelID = null;
            this.ChannelLink = null;

            this.IsLive = false;
            this.StreamTitle = null;
            this.StreamCategoryID = null;
            this.StreamCategoryName = null;
            this.StreamCategoryImageURL = null;
            this.StreamStart = DateTimeOffset.MinValue;
            this.StreamViewerCount = 0;
        }

        protected abstract Task<Result> InitializeStreamerInternal();
        protected abstract Task DisconnectStreamerInternal();

        public async Task<Result> AutomaticConnectBot()
        {
            try
            {
                Result result = await this.BotOAuthService.AutomaticConnect();
                if (result.Success)
                {
                    return await this.InitializeBot();
                }
                return result;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result(ex);
            }
        }

        public async Task<Result> ManualConnectBot(CancellationToken cancellationToken)
        {
            try
            {
                Result result = await this.BotOAuthService.ManualConnect(cancellationToken);
                if (!cancellationToken.IsCancellationRequested && result.Success)
                {
                    return await this.InitializeBot();
                }
                return result;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result(ex);
            }
        }

        protected async Task<Result> InitializeBot()
        {
            Result result = await this.InitializeBotInternal();

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

            await this.BotOAuthService.Disconnect();

            await this.DisconnectBotInternal();
        }

        public async Task DisableBot()
        {
            await this.DisconnectBot();

            await this.BotOAuthService.Disable();

            if (ChannelSession.Settings.StreamingPlatformAuthentications.TryGetValue(this.Platform, out StreamingPlatformAuthenticationSettingsModel streamingPlatformAuth))
            {
                streamingPlatformAuth.ClearBotData();
            }

            this.Bot = null;
            this.BotID = null;
            this.BotUsername = null;
            this.BotAvatarURL = null;
        }

        protected abstract Task<Result> InitializeBotInternal();
        protected abstract Task DisconnectBotInternal();

        public abstract Task RefreshOAuthTokenIfCloseToExpiring();

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
            if (ChannelSession.Settings != null)
            {
                ChannelSession.Settings.StreamingPlatformAuthentications.TryGetValue(this.Platform, out StreamingPlatformAuthenticationSettingsModel authentication);
                return authentication;
            }
            return null;
        }

        public void SaveAuthenticationSettings()
        {
            if (ChannelSession.Settings != null)
            {
                ChannelSession.Settings.StreamingPlatformAuthentications[this.Platform] = new StreamingPlatformAuthenticationSettingsModel(this.Platform)
                {
                    UserID = this.StreamerID,
                    UserOAuthToken = this.StreamerOAuthService.GetOAuthTokenCopy(),

                    ChannelID = this.ChannelID,

                    BotID = this.BotID,
                    BotOAuthToken = this.BotOAuthService.GetOAuthTokenCopy(),
                };
            }
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
