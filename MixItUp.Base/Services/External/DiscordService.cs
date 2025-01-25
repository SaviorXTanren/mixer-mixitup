using MixItUp.Base.Model;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        [JsonProperty("global_name")]
        public string GlobalName { get; set; }

        public DiscordUser() { }

        public DiscordUser(JObject data)
        {
            this.ID = data["id"].ToString();
            this.UserName = data["username"].ToString();
            this.Discriminator = data["discriminator"].ToString();
            this.AvatarID = data["avatar"].ToString();
            this.GlobalName = data["global_name"].ToString();
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
            Announcements = 5,
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

    public class DiscordVoiceConnection
    {
        public string ServerID { get; set; }
        public string UserID { get; set; }
        public string SessionID { get; set; }
        public string Endpoint { get; set; }
        public string Token { get; set; }
    }

    public class DiscordWebSocketPacket
    {
        private const string ReadyPacketName = "READY";
        private const string VoiceStateUpdatePacketName = "VOICE_STATE_UPDATE";
        private const string VoiceServerUpdatePacketName = "VOICE_SERVER_UPDATE";

        public enum DiscordWebSocketPacketTypeEnum
        {
            Unknown = -1,

            Other = 0,
            Heartbeat = 1,
            Identify = 2,

            VoiceStateUpdate = 4,

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

        [JsonIgnore]
        public bool IsVoiceStateUpdatePacket { get { return VoiceStateUpdatePacketName.Equals(this.Name); } }

        [JsonIgnore]
        public bool IsVoiceServerUpdatePacket { get { return VoiceServerUpdatePacketName.Equals(this.Name); } }
    }

    public class DiscordVoiceWebSocketPacket
    {
        public enum DiscordVoiceWebSocketPacketTypeEnum
        {
            Unknown = -1,

            Identify = 0,
            SelectProtocol = 1,
            Ready = 2,

            Heartbeat = 3,

            SessionDescription = 4,

            Speaking = 5,

            HeartbeatAck = 6,

            Resume = 7,
            Hello = 8,
            Resumed = 9,

            ClientConnect = 11,
            ClientDisconnect = 13
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
        public DiscordVoiceWebSocketPacketTypeEnum OPCodeType { get { return (DiscordVoiceWebSocketPacketTypeEnum)this.OPCode; } set { this.OPCode = (int)value; } }
    }

    public class DiscordOAuthServer : LocalOAuthHttpListenerServer
    {
        private const string ServerIDIdentifier = "guild_id";
        private const string BotPermissionsIdentifier = "permissions";

        public string ServerID { get; private set; }
        public string BotPermissions { get; private set; }

        public DiscordOAuthServer() { }

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

    public class DiscordVoiceWebSocket : ClientWebSocketBase
    {
        public event EventHandler<string> OnUserJoinedVoice = delegate { };
        public event EventHandler<string> OnUserLeftVoice = delegate { };

        public event EventHandler<string> OnUserStartedSpeaking = delegate { };
        public event EventHandler<string> OnUserStoppedSpeaking = delegate { };

        public bool IsReady { get; private set; }

        private DiscordVoiceConnection voiceConnection;

        private int? lastSequenceNumber = null;
        private int heartbeatTime = 0;

        public async Task<bool> Connect(DiscordVoiceConnection voiceConnection)
        {
            this.voiceConnection = voiceConnection;

            if (await base.Connect("wss://" + this.voiceConnection.Endpoint + "?v=4"))
            {
                await this.Send(new DiscordVoiceWebSocketPacket() { OPCodeType = DiscordVoiceWebSocketPacket.DiscordVoiceWebSocketPacketTypeEnum.Identify, Data = new JObject()
                {
                    { "server_id", this.voiceConnection.ServerID },
                    { "user_id", this.voiceConnection.UserID },
                    { "session_id", this.voiceConnection.SessionID },
                    { "token", this.voiceConnection.Token },
                }});

                for (int i = 0; i < 5 && !this.IsReady; i++)
                {
                    await Task.Delay(1000);
                }

                if (this.IsReady)
                {
                    this.HeartbeatPing().Wait(1);

                    return true;
                }
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

        public async Task Send(DiscordVoiceWebSocketPacket packet)
        {
            packet.Sequence = this.lastSequenceNumber;
            await this.Send(JSONSerializerHelper.SerializeToString(packet));
        }

        protected override Task ProcessReceivedPacket(string packetJSON)
        {
            try
            {
                DiscordVoiceWebSocketPacket packet = JSONSerializerHelper.DeserializeFromString<DiscordVoiceWebSocketPacket>(packetJSON);
                this.lastSequenceNumber = packet.Sequence;

                switch (packet.OPCodeType)
                {
                    case DiscordVoiceWebSocketPacket.DiscordVoiceWebSocketPacketTypeEnum.Ready:
                        this.IsReady = true;
                        break;

                    case DiscordVoiceWebSocketPacket.DiscordVoiceWebSocketPacketTypeEnum.Hello:
                        this.heartbeatTime = (int)packet.Data["heartbeat_interval"];
                        break;

                    case DiscordVoiceWebSocketPacket.DiscordVoiceWebSocketPacketTypeEnum.HeartbeatAck:
                        break;

                    case DiscordVoiceWebSocketPacket.DiscordVoiceWebSocketPacketTypeEnum.ClientConnect:
                        if (packet.Data.TryGetValue("user_ids", out JToken userIDs) && userIDs is JArray)
                        {
                            foreach (JToken user in (JArray)userIDs)
                            {
                                this.OnUserJoinedVoice(this, user.ToString());
                            }
                        }
                        break;

                    case DiscordVoiceWebSocketPacket.DiscordVoiceWebSocketPacketTypeEnum.Speaking:
                    case DiscordVoiceWebSocketPacket.DiscordVoiceWebSocketPacketTypeEnum.ClientDisconnect:
                        if (packet.Data.TryGetValue("user_id", out JToken userID))
                        {
                            if (packet.OPCodeType == DiscordVoiceWebSocketPacket.DiscordVoiceWebSocketPacketTypeEnum.ClientDisconnect)
                            {
                                this.OnUserLeftVoice(this, userID.ToString());
                            }
                            else if (packet.OPCodeType == DiscordVoiceWebSocketPacket.DiscordVoiceWebSocketPacketTypeEnum.Speaking)
                            {
                                if (packet.Data.TryGetValue("speaking", out JToken speakingToken) && int.TryParse(speakingToken?.ToString(), out int speaking))
                                {
                                    if (speaking > 0)
                                    {
                                        this.OnUserStartedSpeaking(this, userID.ToString());
                                    }
                                    else
                                    {
                                        this.OnUserStoppedSpeaking(this, userID.ToString());
                                    }
                                }
                            }
                        }
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return Task.CompletedTask;
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

                            JObject jobj = new JObject()
                            {
                                { "op", (int)DiscordVoiceWebSocketPacket.DiscordVoiceWebSocketPacketTypeEnum.Heartbeat },
                                { "d", this.lastSequenceNumber }
                            };

                            await this.Send(JSONSerializerHelper.SerializeToString(jobj));
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

    public class DiscordWebSocket : ClientWebSocketBase
    {
        public DiscordUser BotUser { get; private set; }

        public bool IsReady { get; private set; }

        private int shardCount;
        private string botToken;

        private int? lastSequenceNumber = null;
        private int heartbeatTime = 0;

        private string sessionID;

        private DiscordWebSocketPacket voiceStateUpdatePacket;
        private DiscordWebSocketPacket voiceServerUpdatePacket;

        public async Task<bool> Connect(string endpoint, int shardCount, string botToken)
        {
            this.shardCount = shardCount;
            this.botToken = botToken;

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

        public async Task<DiscordVoiceConnection> ConnectToVoice(DiscordServer server, string channelID)
        {
            try
            {
                if (this.IsOpen())
                {
                    this.voiceStateUpdatePacket = null;
                    this.voiceServerUpdatePacket = null;

                    await this.Send(new DiscordWebSocketPacket() { OPCodeType = DiscordWebSocketPacketTypeEnum.VoiceStateUpdate, Sequence = this.lastSequenceNumber, Data = new JObject()
                    {
                        { "guild_id", server.ID },
                        { "channel_id", channelID },
                        { "self_mute", true },
                        { "self_deaf", false },
                    }});

                    for (int i = 0; i < 5 && (this.voiceStateUpdatePacket == null || this.voiceServerUpdatePacket == null); i++)
                    {
                        await Task.Delay(1000);
                    }

                    if (this.voiceStateUpdatePacket != null && this.voiceServerUpdatePacket != null)
                    {
                        if (this.voiceStateUpdatePacket.Data.TryGetValue("guild_id", out JToken guildID) &&
                            this.voiceStateUpdatePacket.Data.TryGetValue("user_id", out JToken userID) &&
                            this.voiceStateUpdatePacket.Data.TryGetValue("session_id", out JToken sessionID) &&
                            this.voiceServerUpdatePacket.Data.TryGetValue("endpoint", out JToken endpoint) &&
                            this.voiceServerUpdatePacket.Data.TryGetValue("token", out JToken token))
                        {
                            return new DiscordVoiceConnection()
                            {
                                ServerID = guildID.ToString(),
                                UserID = userID.ToString(),
                                SessionID = sessionID.ToString(),
                                Endpoint = endpoint.ToString(),
                                Token = token.ToString()
                            };
                        }
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }

            return null;
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
                        else if (packet.IsVoiceStateUpdatePacket)
                        {
                            this.voiceStateUpdatePacket = packet;
                        }
                        else if (packet.IsVoiceServerUpdatePacket)
                        {
                            this.voiceServerUpdatePacket = packet;
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

    public class DiscordBotService : OAuthExternalServiceBase
    {
        private string botToken;

        public override string Name { get { return MixItUp.Base.Resources.DiscordBot; } }

        public DiscordBotService(string baseAddress, string botToken)
            : base(baseAddress)
        {
            this.botToken = botToken;
        }

        public override Task<Result> Connect() { throw new NotImplementedException(); }

        public override Task Disconnect() { throw new NotImplementedException(); }

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

        public async Task<IEnumerable<DiscordServerUser>> SearchServerMembers(DiscordServer server, string search, int maxNumbers = 1)
        {
            try
            {
                return await this.GetAsync<IEnumerable<DiscordServerUser>>("guilds/" + server.ID + "/members/search?query=" + search + "&limit=" + maxNumbers);
            }
            catch (Exception ex) { Logger.Log(ex); }
            return new List<DiscordServerUser>();
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

        public async Task<DiscordServerUser> GetServerMember(DiscordServer server, string userID)
        {
            try
            {
                return await this.GetAsync<DiscordServerUser>("guilds/" + server.ID + "/members/" + userID);
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
                    byte[] bytes = await ServiceManager.Get<IFileService>().ReadFileAsBytes(filePath);
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

        protected override Task<Result> InitializeInternal() { throw new NotImplementedException(); }

        protected override Task RefreshOAuthToken() { return Task.CompletedTask; }

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

    public class DiscordService : OAuthExternalServiceBase, IDisposable
    {
        /// <summary>
        /// View Channels, Send Messages, Send TTS Messages, Embed Links, Attach Files, Mention Everyone, Use External Emojis, Connect, Mute Members, Deafen Members
        /// </summary>
        public const string ClientBotPermissions = "14081024";

        public event EventHandler<string> OnUserJoinedVoice = delegate { };
        public event EventHandler<string> OnUserLeftVoice = delegate { };

        public event EventHandler<string> OnUserStartedSpeaking = delegate { };
        public event EventHandler<string> OnUserStoppedSpeaking = delegate { };

        private const string BaseAddress = "https://discord.com/api/v10";

        private const string DefaultClientID = "422657136510631936";

        private const string AuthorizationUrl = "https://discord.com/api/oauth2/authorize?client_id={0}&permissions={1}&redirect_uri=http%3A%2F%2Flocalhost%3A8919%2F&response_type=code&scope=bot%20guilds%20identify%20connections%20email";

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private DiscordBotService botService;

        private DiscordWebSocket webSocket;

        private DiscordVoiceWebSocket voiceWebSocket;

        public DiscordUser User { get; private set; }
        public DiscordServer Server { get; private set; }

        public string ConnectedVoiceChannelID { get; private set; }

        public IEnumerable<DiscordEmoji> Emojis { get; private set; }

        public string BotPermissions { get; private set; }

        private DateTimeOffset lastCommand = DateTimeOffset.MinValue;

        public DiscordService() : base(DiscordService.BaseAddress) { }

        public override string Name { get { return MixItUp.Base.Resources.Discord; } }

        public bool IsUsingCustomApplication { get { return !string.IsNullOrEmpty(ChannelSession.Settings.DiscordCustomClientID); } }
        public string ClientID { get { return (this.IsUsingCustomApplication) ? ChannelSession.Settings.DiscordCustomClientID : DiscordService.DefaultClientID; } }
        public string ClientSecret { get { return (this.IsUsingCustomApplication) ? ChannelSession.Settings.DiscordCustomClientSecret : ServiceManager.Get<SecretsService>().GetSecret("DiscordSecret"); } }
        public string BotToken { get { return (this.IsUsingCustomApplication) ? ChannelSession.Settings.DiscordCustomBotToken : ServiceManager.Get<SecretsService>().GetSecret("DiscordBotToken"); } }

        public override async Task<Result> Connect()
        {
            try
            {
                DiscordOAuthServer oauthServer = new DiscordOAuthServer();
                string authorizationCode = await oauthServer.GetAuthorizationCode(string.Format(DiscordService.AuthorizationUrl, this.ClientID, DiscordService.ClientBotPermissions), 60);

                if (!string.IsNullOrEmpty(authorizationCode))
                {
                    ChannelSession.Settings.DiscordServer = oauthServer.ServerID;
                    this.BotPermissions = oauthServer.BotPermissions;

                    var body = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("client_id", this.ClientID),
                        new KeyValuePair<string, string>("client_secret", this.ClientSecret),
                        new KeyValuePair<string, string>("grant_type", "authorization_code"),
                        new KeyValuePair<string, string>("redirect_uri", OAuthExternalServiceBase.DEFAULT_OAUTH_LOCALHOST_URL),
                        new KeyValuePair<string, string>("code", authorizationCode),
                    };
                    this.token = await this.GetWWWFormUrlEncodedOAuthToken("https://discord.com/api/v10/oauth2/token", this.ClientID, this.ClientSecret, body);

                    if (this.token != null)
                    {
                        return await this.InitializeInternal();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result(ex);
            }
            return new Result(false);
        }

        public override async Task Disconnect()
        {
            this.token = null;
            this.cancellationTokenSource.Cancel();

            if (this.webSocket != null)
            {
                await this.webSocket.Disconnect();
                this.webSocket = null;
            }

            if (this.voiceWebSocket != null)
            {
                this.voiceWebSocket.OnUserJoinedVoice -= VoiceWebSocket_OnUserJoinedVoice;
                this.voiceWebSocket.OnUserLeftVoice -= VoiceWebSocket_OnUserLeftVoice;
                this.voiceWebSocket.OnUserStartedSpeaking -= VoiceWebSocket_OnUserStartedSpeaking;
                this.voiceWebSocket.OnUserStoppedSpeaking -= VoiceWebSocket_OnUserStoppedSpeaking;

                await this.voiceWebSocket.Disconnect();
                this.voiceWebSocket = null;
            }
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

        public async Task<IEnumerable<DiscordServerUser>> SearchServerMembers(DiscordServer server, string search, int maxNumbers = 1) { return await this.botService.SearchServerMembers(server, search, maxNumbers); }

        public async Task<IEnumerable<DiscordServerUser>> GetServerMembers(DiscordServer server, int maxNumbers = 1) { return await this.botService.GetServerMembers(server, maxNumbers); }

        public async Task<DiscordServerUser> GetServerMember(DiscordServer server, string userID) { return await this.botService.GetServerMember(server, userID); }

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

        public async Task<bool> ConnectToVoice(DiscordServer server, string channelID)
        {
            this.voiceWebSocket.OnUserJoinedVoice -= VoiceWebSocket_OnUserJoinedVoice;
            this.voiceWebSocket.OnUserLeftVoice -= VoiceWebSocket_OnUserLeftVoice;
            this.voiceWebSocket.OnUserStartedSpeaking -= VoiceWebSocket_OnUserStartedSpeaking;
            this.voiceWebSocket.OnUserStoppedSpeaking -= VoiceWebSocket_OnUserStoppedSpeaking;

            DiscordVoiceConnection voiceConnection = await this.webSocket.ConnectToVoice(ServiceManager.Get<DiscordService>().Server, channelID);
            if (voiceConnection != null)
            {
                this.voiceWebSocket = new DiscordVoiceWebSocket();
                if (await this.voiceWebSocket.Connect(voiceConnection))
                {
                    this.voiceWebSocket.OnUserJoinedVoice += VoiceWebSocket_OnUserJoinedVoice;
                    this.voiceWebSocket.OnUserLeftVoice += VoiceWebSocket_OnUserLeftVoice;
                    this.voiceWebSocket.OnUserStartedSpeaking += VoiceWebSocket_OnUserStartedSpeaking;
                    this.voiceWebSocket.OnUserStoppedSpeaking += VoiceWebSocket_OnUserStoppedSpeaking;

                    this.ConnectedVoiceChannelID = channelID;

                    return true;
                }
            }
            return false;
        }

        public async Task DisconnectFromVoice()
        {
            this.ConnectedVoiceChannelID = null;

            if (this.voiceWebSocket != null)
            {
                this.voiceWebSocket.OnUserJoinedVoice -= VoiceWebSocket_OnUserJoinedVoice;
                this.voiceWebSocket.OnUserLeftVoice -= VoiceWebSocket_OnUserLeftVoice;
                this.voiceWebSocket.OnUserStartedSpeaking -= VoiceWebSocket_OnUserStartedSpeaking;
                this.voiceWebSocket.OnUserStoppedSpeaking -= VoiceWebSocket_OnUserStoppedSpeaking;

                await this.voiceWebSocket.Disconnect();

                this.voiceWebSocket = null;
            }
        }

        protected override async Task RefreshOAuthToken()
        {
            if (this.token != null)
            {
                var body = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("client_id", this.ClientID),
                    new KeyValuePair<string, string>("client_secret", this.ClientSecret),
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("redirect_uri", OAuthExternalServiceBase.DEFAULT_OAUTH_LOCALHOST_URL),
                    new KeyValuePair<string, string>("refresh_token", this.token.refreshToken),
                };
                this.token = await this.GetWWWFormUrlEncodedOAuthToken("https://discord.com/api/v10/oauth2/token", this.ClientID, this.ClientSecret, body);
            }
        }

        protected override async Task<Result> InitializeInternal()
        {
            this.botService = new DiscordBotService(this.baseAddress, this.BotToken);

            this.User = await this.GetCurrentUser();
            if (this.User != null)
            {
                if (!string.IsNullOrEmpty(ChannelSession.Settings.DiscordServer))
                {
                    this.Server = await this.GetServer(ChannelSession.Settings.DiscordServer);
                    if (this.Server != null)
                    {
                        this.Emojis = await this.GetEmojis(this.Server);

                        if (!this.IsUsingCustomApplication)
                        {
                            return new Result();
                        }

                        DiscordGateway gateway = await this.GetBotGateway();
                        if (gateway != null)
                        {
                            this.webSocket = new DiscordWebSocket();
                            if (await this.webSocket.Connect(gateway.WebSocketURL + "?v=6&encoding=json", gateway.Shards, this.BotToken))
                            {
                                this.TrackServiceTelemetry("Discord");
                                return new Result();
                            }
                            return new Result(Resources.DiscordBotWebSocketFailed);
                        }
                        return new Result(Resources.DiscoardBotGatewayFailed);
                    }
                }
                return new Result(Resources.DiscordServerDataFailed);
            }
            return new Result(Resources.DiscordUserDataFailed);
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
            await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.DiscordActionBlockedDueToRateLimiting, StreamingPlatformTypeEnum.All);
            return false;
        }

        private void VoiceWebSocket_OnUserJoinedVoice(object sender, string e) { this.OnUserJoinedVoice(sender, e); }

        private void VoiceWebSocket_OnUserLeftVoice(object sender, string e) { this.OnUserLeftVoice(sender, e); }

        private void VoiceWebSocket_OnUserStartedSpeaking(object sender, string e) { this.OnUserStartedSpeaking(sender, e); }

        private void VoiceWebSocket_OnUserStoppedSpeaking(object sender, string e) { this.OnUserStoppedSpeaking(sender, e); }
    }
}
