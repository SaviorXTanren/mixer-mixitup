using Mixer.Base;
using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Model.OAuth;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using static MixItUp.Base.Services.External.DiscordWebSocketPacket;

namespace MixItUp.Base.Services.External
{
    public class DiscordUser : IEquatable<DiscordUser>
    {
        [JsonProperty("id")]
        public string ID { get; set; }
        [JsonProperty("username")]
        public string UserName { get; set; }
        [JsonProperty("discriminator")]
        public string Discriminator { get; set; }
        [JsonProperty("avatar")]
        public string AvatarID { get; set; }

        public DiscordUser() { }

        public DiscordUser(JObject data)
        {
            this.ID = data["id"].ToString();
            this.UserName = data["username"].ToString();
            this.Discriminator = data["discriminator"].ToString();
            this.AvatarID = data["avatar"].ToString();
        }

        public override bool Equals(object other)
        {
            if (other is DiscordUser)
            {
                return this.Equals((DiscordUser)other);
            }
            return false;
        }

        public bool Equals(DiscordUser other) { return this.ID.Equals(other.ID); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }
    }

    public class DiscordServer : IEquatable<DiscordServer>
    {
        private const string MixItUpServerRoleName = "Mix It Up";

        [JsonProperty("id")]
        public string ID { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("icon")]
        public string IconID { get; set; }
        [JsonProperty("owner")]
        public bool? Owner { get; set; }

        [JsonProperty("roles")]
        public List<DiscordServerRole> Roles { get; set; }

        public DiscordServer()
        {
            this.Roles = new List<DiscordServerRole>();
        }

        public uint MixItUpPermissions
        {
            get
            {
                DiscordServerRole role = this.Roles.FirstOrDefault(r => r.Name.Equals(DiscordServer.MixItUpServerRoleName));
                if (role != null)
                {
                    return role.Permissions;
                }
                return 0;
            }
        }

        public override bool Equals(object other)
        {
            if (other is DiscordServer)
            {
                return this.Equals((DiscordServer)other);
            }
            return false;
        }

        public bool Equals(DiscordServer other) { return this.ID.Equals(other.ID); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }
    }

    public class DiscordServerRole
    {
        [JsonProperty("id")]
        public string ID { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("permissions")]
        public uint Permissions { get; set; }

        public DiscordServerRole() { }
    }

    public class DiscordServerUser
    {
        [JsonProperty("user")]
        public DiscordUser User { get; set; }
        [JsonProperty("nick")]
        public string Nickname { get; set; }
        [JsonProperty("roles")]
        public List<string> Roles { get; set; }
        [JsonProperty("mute")]
        public bool Mute { get; set; }
        [JsonProperty("deaf")]
        public bool Deaf { get; set; }

        public DiscordServerUser()
        {
            this.Roles = new List<string>();
        }
    }

    public class DiscordChannel : IEquatable<DiscordChannel>
    {
        public enum DiscordChannelTypeEnum
        {
            Text = 0,
            DirectMessage = 1,
            Voice = 2,
            GroupDirectMessage = 3,
            Category = 4,
        }

        [JsonProperty("id")]
        public string ID { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("type")]
        public DiscordChannelTypeEnum Type { get; set; }

        [JsonProperty("recipients")]
        public List<DiscordUser> Users { get; set; }

        public DiscordChannel()
        {
            this.Users = new List<DiscordUser>();
        }

        public override bool Equals(object other)
        {
            if (other is DiscordChannel)
            {
                return this.Equals((DiscordChannel)other);
            }
            return false;
        }

        public bool Equals(DiscordChannel other) { return this.ID.Equals(other.ID); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }
    }

    public class DiscordMessage
    {
        [JsonProperty("id")]
        public string ID { get; set; }
        [JsonProperty("channel_id")]
        public string ChannelID { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("file")]
        public string File { get; set; }

        [JsonProperty("payload_json")]
        public string PayloadJSON { get; set; }

        public DiscordMessage() { }
    }

    public class DiscordChannelInvite
    {
        private const string InviteLinkTemplate = "https://discord.gg/{0}";

        [JsonProperty("code")]
        public string Code { get; set; }
        [JsonProperty("guild")]
        public DiscordServer Server { get; set; }
        [JsonProperty("channel")]
        public DiscordChannel Channel { get; set; }

        [JsonIgnore]
        public string InviteLink { get { return string.Format(InviteLinkTemplate, this.Code); } }

        public DiscordChannelInvite() { }
    }

    public class DiscordEmoji
    {
        [JsonProperty("id")]
        public string ID { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("require_colons")]
        public bool? RequireColons { get; set; }
        [JsonProperty("managed")]
        public bool? Managed { get; set; }
        [JsonProperty("animated")]
        public bool? Animated { get; set; }
    }

    public class DiscordGateway
    {
        [JsonProperty("url")]
        public string WebSocketURL { get; set; }
        [JsonProperty("shards")]
        public int Shards { get; set; }

        public DiscordGateway() { }
    }

    public class DiscordWebSocketPacket
    {
        private const string ReadyPacketName = "READY";

        public enum DiscordWebSocketPacketTypeEnum
        {
            Unknown = -1,

            Other = 0,
            Heartbeat = 1,
            Identify = 2,

            Hello = 10,
            HeartbeatAck = 11,
        }

        [JsonProperty("op")]
        public int OPCode;
        [JsonProperty("s")]
        public int? Sequence;
        [JsonProperty("t")]
        public string Name;
        [JsonProperty("d")]
        public JObject Data;

        [JsonIgnore]
        public DiscordWebSocketPacketTypeEnum OPCodeType { get { return (DiscordWebSocketPacketTypeEnum)this.OPCode; } set { this.OPCode = (int)value; } }

        [JsonIgnore]
        public bool IsReadyPacket { get { return ReadyPacketName.Equals(this.Name); } }
    }

    public interface IDiscordService : IOAuthExternalService
    {
        bool IsUsingCustomApplication { get; }

        DiscordUser User { get; }
        DiscordServer Server { get; }

        string BotPermissions { get; }

        Task<DiscordGateway> GetBotGateway();

        Task<DiscordUser> GetCurrentUser();

        Task<DiscordUser> GetUser(string userID);

        Task<IEnumerable<DiscordServer>> GetCurrentUserServers();

        Task<DiscordServer> GetServer(string serverID);

        Task<IEnumerable<DiscordServerUser>> GetServerMembers(DiscordServer server, int maxNumbers = 1);

        Task<DiscordServerUser> GetServerMember(DiscordServer server, DiscordUser user);

        Task<IEnumerable<DiscordChannel>> GetServerChannels(DiscordServer server);

        Task<DiscordChannel> GetChannel(string channelID);

        Task<IEnumerable<DiscordEmoji>> GetEmojis(DiscordServer server);

        Task<DiscordMessage> CreateMessage(DiscordChannel channel, string message, string filePath);

        Task<DiscordChannelInvite> CreateChannelInvite(DiscordChannel channel, bool isTemporary = false);

        Task ChangeServerMemberRole(DiscordServer server, DiscordUser user, IEnumerable<string> roles);

        Task MuteServerMember(DiscordServer server, DiscordUser user, bool mute = true);

        Task DeafenServerMember(DiscordServer server, DiscordUser user, bool deaf = true);
    }

    public class DiscordOAuthServer : LocalOAuthHttpListenerServer
    {
        private const string ServerIDIdentifier = "guild_id";
        private const string BotPermissionsIdentifier = "permissions";

        public string ServerID { get; private set; }
        public string BotPermissions { get; private set; }

        public DiscordOAuthServer() : base(MixerConnection.DEFAULT_OAUTH_LOCALHOST_URL, MixerConnection.DEFAULT_AUTHORIZATION_CODE_URL_PARAMETER, OAuthServiceBase.LoginRedirectPageHTML) { }

        protected override async Task ProcessConnection(HttpListenerContext listenerContext)
        {
            if (this.ServerID == null)
            {
                this.ServerID = this.GetRequestParameter(listenerContext, ServerIDIdentifier);
            }
            if (this.BotPermissions == null)
            {
                this.BotPermissions = this.GetRequestParameter(listenerContext, BotPermissionsIdentifier);
            }

            await base.ProcessConnection(listenerContext);
        }
    }

    public class DiscordWebSocket : ClientWebSocketBase
    {
        public DiscordUser BotUser { get; private set; }

        public bool IsReady { get; private set; }

        private int shardCount;
        private string botToken;

        private int? lastSequenceNumber = null;
        private int heartbeatTime = 0;

        private string sessionID;

        public async Task<bool> Connect(string endpoint, int shardCount, string botToken)
        {
            this.shardCount = shardCount;
            this.botToken = botToken;

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
            await this.Send(JSONSerializerHelper.SerializeToString(packet));
        }

        protected override async Task ProcessReceivedPacket(string packetJSON)
        {
            try
            {
                DiscordWebSocketPacket packet = JSONSerializerHelper.DeserializeFromString<DiscordWebSocketPacket>(packetJSON);
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
                        data["token"] = this.botToken;
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

        public async Task<IEnumerable<DiscordEmoji>> GetEmojis(DiscordServer server)
        {
            try
            {
                return await this.GetAsync<IEnumerable<DiscordEmoji>>(string.Format("guilds/{0}/emojis", server.ID));
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<DiscordMessage> CreateMessage(DiscordChannel channel, string message, string filePath)
        {
            try
            {
                DiscordMessage messageObj = new DiscordMessage() { Content = message };
                var messageContent = AdvancedHttpClient.CreateContentFromObject(messageObj);

                var multiPart = new MultipartFormDataContent();
                multiPart.Add(messageContent, "\"payload_json\"");

                if (!string.IsNullOrEmpty(filePath))
                {
                    byte[] bytes = await ChannelSession.Services.FileService.ReadFileAsBytes(filePath);
                    if (bytes != null && bytes.Length > 0)
                    {
                        var fileContent = new ByteArrayContent(bytes);
                        string fileName = System.IO.Path.GetFileName(filePath);
                        multiPart.Add(fileContent, "\"file\"", $"\"{fileName}\"");
                    }
                }

                return await this.PostAsync<DiscordMessage>("channels/" + channel.ID + "/messages", multiPart);
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
                return await this.PostAsync<DiscordChannelInvite>("channels/" + channel.ID + "/invites", AdvancedHttpClient.CreateContentFromObject(obj));
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        protected override async Task<AdvancedHttpClient> GetHttpClient(bool autoRefreshToken = true)
        {
            AdvancedHttpClient client = await base.GetHttpClient(autoRefreshToken);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", this.botToken);
            return client;
        }

        protected override Task RefreshOAuthToken() { return Task.FromResult(0); }

        private async Task<HttpResponseMessage> ModifyServerMember(DiscordServer server, DiscordUser user, JObject content)
        {
            try
            {
                return await this.PatchAsync("guilds/" + server.ID + "/members/" + user.ID, AdvancedHttpClient.CreateContentFromObject(content));
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }
    }

    public class DiscordService : OAuthExternalServiceBase, IDiscordService, IDisposable
    {
        /// <summary>
        /// View Channels, Send Messages, Send TTS Messages, Embed Links, Attach Files, Mention Everyone, Use External Emojis, Connect, Mute Members, Deafen Members
        /// </summary>
        public const string ClientBotPermissions = "14081024";

        private const string BaseAddress = "https://discordapp.com/api/";

        private const string DefaultClientID = "422657136510631936";

        private const string AuthorizationUrl = "https://discordapp.com/api/oauth2/authorize?client_id={0}&permissions={1}&redirect_uri=http%3A%2F%2Flocalhost%3A8919%2F&response_type=code&scope=bot%20guilds%20identify%20connections%20messages.read%20guilds.join%20email%20gdm.join";

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private DiscordBotService botService;

        public DiscordUser User { get; private set; }
        public DiscordServer Server { get; private set; }

        public IEnumerable<DiscordEmoji> Emojis { get; private set; }

        public string BotPermissions { get; private set; }

        private DateTimeOffset lastCommand = DateTimeOffset.MinValue;

        public DiscordService() : base(DiscordService.BaseAddress) { }

        public override string Name { get { return "Discord"; } }

        public bool IsUsingCustomApplication { get { return !string.IsNullOrEmpty(ChannelSession.Settings.DiscordCustomClientID); } }
        public string ClientID { get { return (this.IsUsingCustomApplication) ? ChannelSession.Settings.DiscordCustomClientID : DiscordService.DefaultClientID; } }
        public string ClientSecret { get { return (this.IsUsingCustomApplication) ? ChannelSession.Settings.DiscordCustomClientSecret : ChannelSession.Services.Secrets.GetSecret("DiscordSecret"); } }
        public string BotToken { get { return (this.IsUsingCustomApplication) ? ChannelSession.Settings.DiscordCustomBotToken : ChannelSession.Services.Secrets.GetSecret("DiscordBotToken"); } }

        public override async Task<ExternalServiceResult> Connect()
        {
            try
            {
                string authorizationCode = await this.ConnectViaOAuthRedirect(string.Format(DiscordService.AuthorizationUrl, this.ClientID, DiscordService.ClientBotPermissions), secondsToWait: 60);
                if (!string.IsNullOrEmpty(authorizationCode))
                {
                    var body = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("grant_type", "authorization_code"),
                        new KeyValuePair<string, string>("redirect_uri", MixerConnection.DEFAULT_OAUTH_LOCALHOST_URL),
                        new KeyValuePair<string, string>("code", authorizationCode),
                    };
                    this.token = await this.GetWWWFormUrlEncodedOAuthToken("https://discordapp.com/api/oauth2/token", this.ClientID, this.ClientSecret, body);

                    if (this.token != null)
                    {
                        token.authorizationCode = authorizationCode;
                        return await this.InitializeInternal();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new ExternalServiceResult(ex);
            }
            return new ExternalServiceResult(false);
        }

        public override Task Disconnect()
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

        public async Task<IEnumerable<DiscordEmoji>> GetEmojis(DiscordServer server) { return await this.botService.GetEmojis(server); }

        public async Task<DiscordMessage> CreateMessage(DiscordChannel channel, string message, string filePath)
        {
            if (await this.IsWithinRateLimiting())
            {
                if (this.Emojis != null)
                {
                    foreach (DiscordEmoji emoji in this.Emojis)
                    {
                        string findString = emoji.Name;
                        if (emoji.RequireColons.GetValueOrDefault())
                        {
                            findString = ":" + findString + ":";
                        }

                        string replacementString = ":" + emoji.Name + ":";
                        if (emoji.Animated.GetValueOrDefault())
                        {
                            replacementString = "a" + replacementString;
                        }
                        replacementString = "<" + replacementString + emoji.ID + ">";

                        message = message.Replace(findString, replacementString);
                    }
                }
                return await this.botService.CreateMessage(channel, message, filePath);
            }
            return null;
        }

        public async Task<DiscordChannelInvite> CreateChannelInvite(DiscordChannel channel, bool isTemporary = false) { return await this.botService.CreateChannelInvite(channel, isTemporary); }

        public async Task ChangeServerMemberRole(DiscordServer server, DiscordUser user, IEnumerable<string> roles) { await this.botService.ChangeServerMemberRole(server, user, roles); }

        public async Task MuteServerMember(DiscordServer server, DiscordUser user, bool mute = true) { await this.botService.MuteServerMember(server, user, mute); }

        public async Task DeafenServerMember(DiscordServer server, DiscordUser user, bool deaf = true) { await this.botService.DeafenServerMember(server, user, deaf); }

        protected override async Task<string> ConnectViaOAuthRedirect(string oauthPageURL, string listeningURL)
        {
            DiscordOAuthServer oauthServer = new DiscordOAuthServer();
            oauthServer.Start();

            ProcessHelper.LaunchLink(oauthPageURL);

            string authorizationCode = await oauthServer.WaitForAuthorizationCode();
            oauthServer.Stop();

            ChannelSession.Settings.DiscordServer = oauthServer.ServerID;
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
                this.token = await this.GetWWWFormUrlEncodedOAuthToken("https://discordapp.com/api/oauth2/token", this.ClientID, this.ClientSecret, body);
            }
        }

        protected override async Task<ExternalServiceResult> InitializeInternal()
        {
            this.botService = new DiscordBotService(this.baseAddress, this.BotToken);

            this.User = await this.GetCurrentUser();
            if (!string.IsNullOrEmpty(ChannelSession.Settings.DiscordServer))
            {
                this.Server = await this.GetServer(ChannelSession.Settings.DiscordServer);
                if (this.Server != null)
                {
                    this.Emojis = await this.GetEmojis(this.Server);

                    if (!this.IsUsingCustomApplication)
                    {
                        return new ExternalServiceResult();
                    }

                    DiscordGateway gateway = await this.GetBotGateway();
                    if (gateway != null)
                    {
                        DiscordWebSocket webSocket = new DiscordWebSocket();
                        if (await webSocket.Connect(gateway.WebSocketURL, gateway.Shards, this.BotToken))
                        {
                            return new ExternalServiceResult();
                        }
                        return new ExternalServiceResult("Could not connect Bot Application to web socket");
                    }
                    return new ExternalServiceResult("Could not get Bot Application Gateway data");
                }
                return new ExternalServiceResult("Could not get Server data");
            }
            return new ExternalServiceResult("Could not get User data");
        }

        protected override void DisposeInternal()
        {
            this.cancellationTokenSource.Dispose();
        }

        private async Task<bool> IsWithinRateLimiting()
        {
            if (this.IsUsingCustomApplication || this.lastCommand.TotalSecondsFromNow() > 30)
            {
                this.lastCommand = DateTimeOffset.Now;
                return true;
            }
            await ChannelSession.Services.Chat.Whisper(await ChannelSession.GetCurrentUser(), "The Discord action you were trying to perform was blocked due to too many requests. Please ensure you are only performing 1 Discord action every 30 seconds. You can add a custom Discord Bot under the Services page to circumvent this block.");
            return false;
        }
    }
}
