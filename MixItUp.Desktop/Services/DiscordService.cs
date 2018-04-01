using Mixer.Base;
using Mixer.Base.Model.OAuth;
using Mixer.Base.Util;
using MixItUp.Base.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class DiscordService : OAuthServiceBase, IDiscordService
    {
        private const string BaseAddress = "https://discordapp.com/api/";

        private const string ClientID = "422657136510631936";
        private const string ClientSecret = "3N6V7bHyXQEffnzxlbsTggMtosRcA7XG";
        private const string AuthorizationUrl = "https://discordapp.com/api/oauth2/authorize?client_id={0}&permissions=281463809&redirect_uri=http%3A%2F%2Flocalhost%3A8919%2F&response_type=code&scope=bot%20guilds%20identify%20connections%20messages.read%20guilds.join%20email%20gdm.join";

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private DiscordUser user;

        public DiscordService() : base(DiscordService.BaseAddress) { }

        public DiscordService(OAuthTokenModel token) : base(DiscordService.BaseAddress, token) { }

        public async Task<bool> Connect()
        {
            if (this.token != null)
            {
                try
                {
                    await this.RefreshOAuthToken();

                    await this.InitializeInternal();

                    return true;
                }
                catch (Exception ex) { Logger.Log(ex); }
            }

            string authorizationCode = await this.ConnectViaOAuthRedirect(string.Format(DiscordService.AuthorizationUrl, DiscordService.ClientID));
            if (!string.IsNullOrEmpty(authorizationCode))
            {
                var body = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("redirect_uri", MixerConnection.DEFAULT_OAUTH_LOCALHOST_URL),
                    new KeyValuePair<string, string>("code", authorizationCode),
                };
                this.token = await this.GetWWWFormUrlEncodedOAuthToken("https://discordapp.com/api/oauth2/token", DiscordService.ClientID, DiscordService.ClientSecret, body);

                if (this.token != null)
                {
                    token.authorizationCode = authorizationCode;

                    await this.InitializeInternal();

                    return true;
                }
            }

            return false;
        }

        public Task Disconnect()
        {
            this.token = null;
            this.cancellationTokenSource.Cancel();
            return Task.FromResult(0);
        }

        public async Task<DiscordUser> GetCurrentUser()
        {
            try
            {
                JObject result = await this.GetJObjectAsync("users/@me");
                return new DiscordUser(result);
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<DiscordUser> GetUser(string userID)
        {
            try
            {
                JObject result = await this.GetJObjectAsync("users/" + userID);
                return new DiscordUser(result);
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<IEnumerable<DiscordServer>> GetCurrentUserServer()
        {
            List<DiscordServer> results = new List<DiscordServer>();
            try
            {
                HttpResponseMessage response = await this.GetAsync("users/@me/guilds");
                string responseString = await response.Content.ReadAsStringAsync();
                foreach (JToken token in JArray.Parse(responseString))
                {
                    results.Add(new DiscordServer((JObject)token));
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return results;
        }

        public async Task<DiscordServer> GetServer(string serverID)
        {
            try
            {
                JObject result = await this.GetJObjectAsync("guilds/" + serverID);
                return new DiscordServer(result);
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<IEnumerable<DiscordChannel>> GetServerChannel(DiscordServer server)
        {
            List<DiscordChannel> results = new List<DiscordChannel>();
            try
            {
                HttpResponseMessage response = await this.GetAsync("guilds/" + server.ID + "/channels");
                string responseString = await response.Content.ReadAsStringAsync();
                foreach (JToken token in JArray.Parse(responseString))
                {
                    results.Add(new DiscordChannel((JObject)token));
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return results;
        }

        public async Task<DiscordChannel> GetChannel(string channelID)
        {
            try
            {
                JObject result = await this.GetJObjectAsync("channels/" + channelID);
                return new DiscordChannel(result);
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<DiscordChannelInvite> CreateChannelInvite(DiscordChannel channel)
        {
            try
            {
                JObject result = await this.GetJObjectAsync("channels/" + channel.ID + "/invites");
                return new DiscordChannelInvite(result);
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        protected override async Task RefreshOAuthToken()
        {
            if (this.token != null)
            {
                var body = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("redirect_uri", MixerConnection.DEFAULT_OAUTH_LOCALHOST_URL),
                    new KeyValuePair<string, string>("refresh_token", this.token.refreshToken),
                };
                this.token = await this.GetWWWFormUrlEncodedOAuthToken("https://discordapp.com/api/oauth2/token", DiscordService.ClientID, DiscordService.ClientSecret, body);
            }
        }

        private async Task InitializeInternal()
        {
            this.user = await this.GetCurrentUser();
            var servers = await this.GetCurrentUserServer();
            var server = await this.GetServer(servers.First(s => s.Owner).ID);

            await this.GetServerChannel(server);

            //HttpResponseMessage result = await this.GetAsync("socket/token");
            //string resultJson = await result.Content.ReadAsStringAsync();
            //JObject jobj = JObject.Parse(resultJson);

            //this.websocketService = new StreamlabsWebSocketService(jobj["socket_token"].ToString());
        }
    }
}
