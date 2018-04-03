using Mixer.Base.Model.OAuth;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
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

    public interface IDiscordService
    {
        DiscordUser User { get; }
        DiscordServer Server { get; }

        string BotPermissions { get; }

        Task<bool> Connect();

        Task Disconnect();

        Task<DiscordGateway> GetBotGateway();

        Task<DiscordUser> GetCurrentUser();

        Task<DiscordUser> GetUser(string userID);

        Task<IEnumerable<DiscordServer>> GetCurrentUserServers();

        Task<DiscordServer> GetServer(string serverID);

        Task<IEnumerable<DiscordServerUser>> GetServerMembers(DiscordServer server, int maxNumbers = 1);

        Task<DiscordServerUser> GetServerMember(DiscordServer server, DiscordUser user);

        Task<IEnumerable<DiscordChannel>> GetServerChannels(DiscordServer server);

        Task<DiscordChannel> GetChannel(string channelID);

        Task<DiscordMessage> CreateMessage(DiscordChannel channel, string message);

        Task<DiscordChannelInvite> CreateChannelInvite(DiscordChannel channel, bool isTemporary = false);

        Task ChangeServerMemberRole(DiscordServer server, DiscordUser user, IEnumerable<string> roles);

        Task MuteServerMember(DiscordServer server, DiscordUser user, bool mute = true);

        Task DeafenServerMember(DiscordServer server, DiscordUser user, bool deaf = true);

        OAuthTokenModel GetOAuthTokenCopy();
    }
}
