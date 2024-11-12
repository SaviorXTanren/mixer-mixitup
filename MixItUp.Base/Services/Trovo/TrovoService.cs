using MixItUp.Base.Model;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Model.Web;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Trovo
{
    /// <summary>
    /// https://trovo.live/policy/apis-developer-doc.html
    /// </summary>
    public class TrovoService : StreamingPlatformServiceBaseNew
    {
        private const string OAuthBaseAddress = "https://open.trovo.live/page/login.html";

        private const string TrovoRestAPIBaseAddressFormat = "https://open-api.trovo.live/openplatform/";

        public static readonly List<string> StreamerScopes = new List<string>()
        {
            "chat_connect",
            "chat_send_self",
            "send_to_my_channel",
            "manage_messages",

            "channel_details_self",
            "channel_update_self",
            "channel_subscriptions",

            "user_details_self",
        };

        public static readonly List<string> BotScopes = new List<string>()
        {
            "chat_connect",
            "chat_send_self",
            "end_to_my_channel",
            "manage_messages",

            "user_details_self",
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

        internal static string ConvertClientScopesToString(IEnumerable<string> scopes)
        {
            return string.Join("+", scopes);
        }

        public override string Name { get { return Resources.Trovo; } }

        public override string ClientID { get { return "8FMjuk785AX4FMyrwPTU3B8vYvgHWN33"; } }
        public override string ClientSecret { get { return ServiceManager.Get<SecretsService>().GetSecret("TrovoSecret"); } }

        public override bool IsConnected { get; protected set; }

        public TrovoService() : base(TrovoRestAPIBaseAddressFormat) { }

        public override Task<Result> Disconnect()
        {
            throw new System.NotImplementedException();
        }

        protected override Task RefreshOAuthToken()
        {
            throw new System.NotImplementedException();
        }

        protected async override Task<string> GetAuthorizationCodeURL(IEnumerable<string> scopes, string state, bool forceApprovalPrompt = false)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { "client_id", this.ClientID },
                { "response_type", LocalOAuthHttpListenerServer.AUTHORIZATION_CODE_URL_PARAMETER },
                { "scope", TrovoService.ConvertClientScopesToString(scopes) },
                { "redirect_uri", LocalOAuthHttpListenerServer.REDIRECT_URL },
                { "state", state },
            };

            if (forceApprovalPrompt)
            {
                parameters.Add("force_verify", "force");
            }

            FormUrlEncodedContent content = new FormUrlEncodedContent(parameters.AsEnumerable());
            return OAuthBaseAddress + "?" + await content.ReadAsStringAsync();
        }

        protected async override Task<OAuthTokenModel> RequestOAuthToken(string authorizationCode, IEnumerable<string> scopes, string state)
        {
            JObject content = new JObject()
            {
                { "client_id", this.ClientID },
                { "client_secret", this.ClientSecret },
                { "code", authorizationCode },
                { "grant_type", "authorization_code" },
                { "redirect_uri", LocalOAuthHttpListenerServer.REDIRECT_URL },
            };

            OAuthTokenModel token = await this.HttpClient.PostAsync<OAuthTokenModel>("exchangetoken", AdvancedHttpClient.CreateContentFromObject(content));
            if (token != null)
            {
                token.clientID = this.ClientID;
                token.authorizationCode = authorizationCode;
                token.ScopeList = string.Join(",", scopes ?? new List<string>());
                return token;
            }
            return null;
        }
    }
}
