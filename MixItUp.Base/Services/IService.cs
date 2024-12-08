using MixItUp.Base.Model;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Model.Web;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using System;
using System.Collections.Generic;
using System.Net.Http;
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
                    authorizationCode = this.OAuthToken.authorizationCode,
                    refreshToken = this.OAuthToken.refreshToken,
                    accessToken = this.OAuthToken.accessToken,
                    expiresIn = this.OAuthToken.expiresIn
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
        public class StreamingPlatformAccountModel
        {
            public string ID { get; set; }

            public string Username { get; set; }

            public string AvatarURL { get; set; }
        }

        public StreamingPlatformTypeEnum Platform { get; }

        public abstract string UserID { get; }
        public abstract string Username { get; }
        public abstract string ChannelID { get; }
        public abstract string ChannelLink { get; }

        public abstract StreamingPlatformAccountModel Account { get; }

        public abstract IEnumerable<string> Scopes { get; protected set; }

        public StreamingPlatformServiceBaseNew(string baseAddress) : base(baseAddress) { }

        public async override Task<Result> Connect()
        {
            StreamingPlatformAuthenticationSettingsModel authenticationSettings = this.GetAuthenticationSettings();
            if (authenticationSettings?.IsEnabled ?? false)
            {
                this.OAuthToken = authenticationSettings.UserOAuthToken;
                await this.RefreshOAuthToken();
            }
            else
            {
                Result result = await this.ConnectWithAuthorization(this.Scopes);
                if (!result.Success)
                {
                    return result;
                }
            }

            return await Initialize();
        }

        public abstract Task<Result> Initialize();

        public override async Task Disable()
        {
            await this.Disconnect();

            StreamingPlatformAuthenticationSettingsModel authenticationSettings = this.GetAuthenticationSettings();
            if (authenticationSettings != null)
            {
                authenticationSettings.ClearUserData();
            }
        }

        //public async Task<Result> ConnectBot()
        //{
        //    StreamingPlatformAuthenticationSettingsModel authenticationSettings = this.GetAuthenticationSettings();
        //    if (authenticationSettings?.BotOAuthToken != null)
        //    {
        //        this.SetOAuthToken(authenticationSettings.UserOAuthToken);
        //        await this.RefreshOAuthToken();
        //    }
        //    else
        //    {
        //        Result result = await this.ConnectWithAuthorization(this.BotScopes);
        //        if (!result.Success)
        //        {
        //            return result;
        //        }
        //    }

        //    return new Result();
        //}

        //public abstract Task DisconnectBot();

        //public async Task DisableBot()
        //{
        //    await this.Disconnect();

        //    StreamingPlatformAuthenticationSettingsModel authenticationSettings = this.GetAuthenticationSettings();
        //    if (authenticationSettings != null)
        //    {
        //        authenticationSettings.ClearBotData();
        //    }
        //}

        public StreamingPlatformAuthenticationSettingsModel GetAuthenticationSettings()
        {
            ChannelSession.Settings.StreamingPlatformAuthentications.TryGetValue(this.Platform, out StreamingPlatformAuthenticationSettingsModel authentication);
            return authentication;
        }
    }
}
