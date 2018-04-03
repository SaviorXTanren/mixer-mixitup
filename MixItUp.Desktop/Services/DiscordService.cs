using Mixer.Base;
using Mixer.Base.Clients;
using Mixer.Base.Model.OAuth;
using Mixer.Base.Web;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using static MixItUp.Base.Services.DiscordWebSocketPacket;

namespace MixItUp.Desktop.Services
{
    public class DiscordOAuthServer : OAuthHttpListenerServer
    {
        private const string ServerIDIdentifier = "guild_id=";
        private const string BotPermissionsIdentifier = "permissions=";

        public string ServerID { get; private set; }
        public string BotPermissions { get; private set; }

        public DiscordOAuthServer() : base(MixerConnection.DEFAULT_OAUTH_LOCALHOST_URL, loginSuccessHtmlPageFilePath: "LoginRedirectPage.html") { }

        protected override async Task ProcessConnection(HttpListenerContext listenerContext)
        {
            if (this.ServerID == null)
            {
                this.ServerID = this.GetIdentifierValue(listenerContext, ServerIDIdentifier);
            }
            if (this.BotPermissions == null)
            {
                this.BotPermissions = this.GetIdentifierValue(listenerContext, BotPermissionsIdentifier);
            }

            await base.ProcessConnection(listenerContext);
        }
    }

    public class DiscordWebSocket : WebSocketClientBase
    {
        public DiscordUser BotUser { get; private set; }

        public bool IsReady { get; private set; }

        private int shardCount;
        private int? lastSequenceNumber = null;
        private int heartbeatTime = 0;

        private string sessionID;

        public async Task<bool> Connect(string endpoint, int shardCount)
        {
            this.shardCount = shardCount;

            endpoint += "?v=6&encoding=json";
            if (await base.Connect(endpoint))
            {
                this.HeartbeatPing().Wait(1);
                return true;
            }
            return false;
        }

        public override Task<bool> Connect(string endpoint)
        {
            throw new InvalidOperationException("Please use other constructor");
        }

        public override async Task Disconnect(WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure)
        {
            this.IsReady = false;
            await base.Disconnect(closeStatus);
        }

        public async Task Send(DiscordWebSocketPacket packet)
        {
            packet.Sequence = this.lastSequenceNumber;
            await this.Send(SerializerHelper.SerializeToString(packet));
        }

        protected override async Task ProcessReceivedPacket(string packetJSON)
        {
            try
            {
                DiscordWebSocketPacket packet = SerializerHelper.DeserializeFromString<DiscordWebSocketPacket>(packetJSON);
                this.lastSequenceNumber = packet.Sequence;

                switch (packet.OPCodeType)
                {
                    case DiscordWebSocketPacketTypeEnum.Other:
                        if (packet.IsReadyPacket)
                        {
                            this.BotUser = new DiscordUser((JObject)packet.Data["user"]);
                            this.sessionID = packet.Data["session_id"].ToString();
                            this.IsReady = true;
                        }
                        break;

                    case DiscordWebSocketPacketTypeEnum.Heartbeat:
                        break;

                    case DiscordWebSocketPacketTypeEnum.Hello:
                        this.heartbeatTime = (int)packet.Data["heartbeat_interval"];

                        JObject data = new JObject();
                        data["token"] = "NDIyNjU3MTM2NTEwNjMxOTM2.DaLZSw.b1mdE04J205RnZcVTZ0G3RuSbG0";
                        data["large_threshold"] = 100;

                        JObject propertiesObj = new JObject();
                        propertiesObj["$device"] = "Mix It Up";
                        data["properties"] = propertiesObj;

                        DiscordWebSocketPacket identifyPacket = new DiscordWebSocketPacket() { OPCodeType = DiscordWebSocketPacketTypeEnum.Identify, Data = data };
                        await this.Send(identifyPacket);

                        break;

                    case DiscordWebSocketPacketTypeEnum.HeartbeatAck:
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private async Task HeartbeatPing()
        {
            try
            {
                while (this.IsOpen())
                {
                    try
                    {
                        if (this.IsReady && this.heartbeatTime > 0)
                        {
                            await Task.Delay(this.heartbeatTime / 2);
                            await this.Send(new DiscordWebSocketPacket() { OPCodeType = DiscordWebSocketPacketTypeEnum.Heartbeat, Sequence = this.lastSequenceNumber });
                        }
                        else
                        {
                            await Task.Delay(1000);
                        }
                    }
                    catch (Exception ex) { Logger.Log(ex); }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }
    }

    public class DiscordBotService : OAuthServiceBase
    {
        private string botToken;

        public DiscordBotService(string baseAddress, string botToken)
            : base(baseAddress)
        {
            this.botToken = botToken;
        }

        public async Task<DiscordGateway> GetBotGateway()
        {
            try
            {
                return await this.GetAsync<DiscordGateway>("gateway/bot");
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<DiscordServer> GetServer(string serverID)
        {
            try
            {
                return await this.GetAsync<DiscordServer>("guilds/" + serverID);
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<IEnumerable<DiscordChannel>> GetServerChannels(DiscordServer server)
        {
            try
            {
                string result = await this.GetStringAsync("guilds/" + server.ID + "/channels");

                return await this.GetAsync<IEnumerable<DiscordChannel>>("guilds/" + server.ID + "/channels");
            }
            catch (Exception ex) { Logger.Log(ex); }
            return new List<DiscordChannel>();
        }

        public async Task<IEnumerable<DiscordServerUser>> GetServerMembers(DiscordServer server, int maxNumbers = 1)
        {
            try
            {
                return await this.GetAsync<IEnumerable<DiscordServerUser>>("guilds/" + server.ID + "/members?limit=" + maxNumbers);
            }
            catch (Exception ex) { Logger.Log(ex); }
            return new List<DiscordServerUser>();
        }

        public async Task<DiscordServerUser> GetServerMember(DiscordServer server, DiscordUser user)
        {
            try
            {
                return await this.GetAsync<DiscordServerUser>("guilds/" + server.ID + "/members/" + user.ID);
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task ChangeServerMemberRole(DiscordServer server, DiscordUser user, IEnumerable<string> roles)
        {
            try
            {
                JArray rolesArray = new JArray();
                foreach (string role in roles)
                {
                    rolesArray.Add(role);
                }

                JObject jobj = new JObject();
                jobj["roles"] = rolesArray;

                await this.ModifyServerMember(server, user, jobj);
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public async Task MuteServerMember(DiscordServer server, DiscordUser user, bool mute = true)
        {
            try
            {
                JObject jobj = new JObject();
                jobj["mute"] = mute;
                await this.ModifyServerMember(server, user, jobj);
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public async Task DeafenServerMember(DiscordServer server, DiscordUser user, bool deaf = true)
        {
            try
            {
                JObject jobj = new JObject();
                jobj["deaf"] = deaf;
                await this.ModifyServerMember(server, user, jobj);
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public async Task<DiscordChannel> GetChannel(string channelID)
        {
            try
            {
                return await this.GetAsync<DiscordChannel>("channels/" + channelID);
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<DiscordMessage> CreateMessage(DiscordChannel channel, string message)
        {
            try
            {
                DiscordMessage messageObj = new DiscordMessage() { Content = message };
                return await this.PostAsync<DiscordMessage>("channels/" + channel.ID + "/messages", this.CreateContentFromObject(messageObj));
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<DiscordChannelInvite> CreateChannelInvite(DiscordChannel channel, bool isTemporary = false)
        {
            try
            {
                JObject obj = new JObject();
                obj["temporary"] = isTemporary;
                return await this.PostAsync<DiscordChannelInvite>("channels/" + channel.ID + "/invites", this.CreateContentFromObject(obj));
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        protected override async Task<HttpClientWrapper> GetHttpClient(bool autoRefreshToken = true)
        {
            HttpClientWrapper client = await base.GetHttpClient(autoRefreshToken);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", this.botToken);
            return client;
        }

        protected override Task RefreshOAuthToken() { return Task.FromResult(0); }

        private async Task<HttpResponseMessage> ModifyServerMember(DiscordServer server, DiscordUser user, JObject content)
        {
            try
            {
                return await this.PatchAsync("guilds/" + server.ID + "/members/" + user.ID, this.CreateContentFromObject(content));
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }
    }

    public class DiscordService : OAuthServiceBase, IDiscordService
    {
        public const string ClientBotPermissions = "8";

        private const string BaseAddress = "https://discordapp.com/api/";

        private const string ClientID = "422657136510631936";
        private const string ClientSecret = "3N6V7bHyXQEffnzxlbsTggMtosRcA7XG";

        private const string BotTokenSecret = "";

        private const string AuthorizationUrl = "https://discordapp.com/api/oauth2/authorize?client_id={0}&permissions={1}&redirect_uri=http%3A%2F%2Flocalhost%3A8919%2F&response_type=code&scope=bot%20guilds%20identify%20connections%20messages.read%20guilds.join%20email%20gdm.join";

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private DiscordBotService botService;

        public DiscordUser User { get; private set; }
        public DiscordServer Server { get; private set; }

        public string BotPermissions { get; private set; }

        private string serverID;

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

            string authorizationCode = await this.ConnectViaOAuthRedirect(string.Format(DiscordService.AuthorizationUrl, DiscordService.ClientID, DiscordService.ClientBotPermissions));
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

        public async Task<DiscordGateway> GetBotGateway() { return await this.botService.GetBotGateway(); }

        public async Task<DiscordUser> GetCurrentUser()
        {
            try
            {
                return await this.GetAsync<DiscordUser>("users/@me");
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<DiscordUser> GetUser(string userID)
        {
            try
            {
                return await this.GetAsync<DiscordUser>("users/" + userID);
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<IEnumerable<DiscordServer>> GetCurrentUserServers()
        {
            List<DiscordServer> results = new List<DiscordServer>();
            try
            {
                return await this.GetAsync<IEnumerable<DiscordServer>>("users/@me/guilds");
            }
            catch (Exception ex) { Logger.Log(ex); }
            return results;
        }

        public async Task<DiscordServer> GetServer(string serverID) { return await this.botService.GetServer(serverID); }

        public async Task<IEnumerable<DiscordServerUser>> GetServerMembers(DiscordServer server, int maxNumbers = 1) { return await this.botService.GetServerMembers(server, maxNumbers); }

        public async Task<DiscordServerUser> GetServerMember(DiscordServer server, DiscordUser user) { return await this.botService.GetServerMember(server, user); }

        public async Task<IEnumerable<DiscordChannel>> GetServerChannels(DiscordServer server) { return await this.botService.GetServerChannels(server); }

        public async Task<DiscordChannel> GetChannel(string channelID) { return await this.botService.GetChannel(channelID); }

        public async Task<DiscordMessage> CreateMessage(DiscordChannel channel, string message) { return await this.botService.CreateMessage(channel, message); }

        public async Task<DiscordChannelInvite> CreateChannelInvite(DiscordChannel channel, bool isTemporary = false) { return await this.botService.CreateChannelInvite(channel, isTemporary); }

        public async Task ChangeServerMemberRole(DiscordServer server, DiscordUser user, IEnumerable<string> roles) { await this.botService.ChangeServerMemberRole(server, user, roles); }

        public async Task MuteServerMember(DiscordServer server, DiscordUser user, bool mute = true) { await this.botService.MuteServerMember(server, user, mute); }

        public async Task DeafenServerMember(DiscordServer server, DiscordUser user, bool deaf = true) { await this.botService.DeafenServerMember(server, user, deaf); }

        protected override async Task<string> ConnectViaOAuthRedirect(string oauthPageURL)
        {
            DiscordOAuthServer oauthServer = new DiscordOAuthServer();
            oauthServer.Start();

            ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = oauthPageURL, UseShellExecute = true };
            Process.Start(startInfo);

            string authorizationCode = await oauthServer.WaitForAuthorizationCode();
            oauthServer.End();

            this.serverID = oauthServer.ServerID;
            this.BotPermissions = oauthServer.BotPermissions;

            return authorizationCode;
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
            this.botService = new DiscordBotService(this.baseAddress, DiscordService.BotTokenSecret);

            this.User = await this.GetCurrentUser();
            if (!string.IsNullOrEmpty(this.serverID))
            {
                this.Server = await this.GetServer(this.serverID);
            }

            //DiscordGateway gateway = await this.GetBotGateway();
            //if (gateway != null)
            //{
            //    DiscordWebSocket webSocket = new DiscordWebSocket();
            //    await webSocket.Connect(gateway.WebSocketURL, gateway.Shards);
            //}
        }
    }
}
