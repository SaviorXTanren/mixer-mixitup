using Mixer.Base.Model.OAuth;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public class DiscordUser
    {
        public string ID { get; set; }
        public string UserName { get; set; }
        public string Discriminator { get; set; }
        public string AvatarID { get; set; }

        public DiscordUser(JObject data)
        {
            this.ID = data["id"].ToString();
            this.UserName = data["username"].ToString();
            this.Discriminator = data["discriminator"].ToString();
            this.AvatarID = data["avatar"].ToString();
        }
    }

    public class DiscordServer
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string IconID { get; set; }
        public uint Permissions { get; set; }
        public bool Owner { get; set; }

        public DiscordServer(JObject data)
        {
            this.ID = data["id"].ToString();
            this.Name = data["name"].ToString();
            this.IconID = data["icon"].ToString();
            if (data["permissions"] != null)
            {
                this.Permissions = uint.Parse(data["permissions"].ToString());
            }
            if (data["owner"] != null)
            {
                this.Owner = bool.Parse(data["owner"].ToString());
            }
        }
    }

    public class DiscordChannel
    {
        public string ID { get; set; }
        public string Name { get; set; }

        public DiscordChannel(JObject data)
        {
            this.ID = data["id"].ToString();
            this.Name = data["name"].ToString();
        }
    }

    public class DiscordChannelInvite
    {
        public DiscordChannelInvite(JObject data)
        {

        }
    }

    public interface IDiscordService
    {
        Task<bool> Connect();

        Task Disconnect();

        Task<DiscordUser> GetCurrentUser();

        Task<DiscordUser> GetUser(string userID);

        Task<IEnumerable<DiscordServer>> GetCurrentUserServer();

        Task<DiscordServer> GetServer(string serverID);

        Task<IEnumerable<DiscordChannel>> GetServerChannel(DiscordServer server);

        Task<DiscordChannel> GetChannel(string channelID);

        Task<DiscordChannelInvite> CreateChannelInvite(DiscordChannel channel);

        OAuthTokenModel GetOAuthTokenCopy();
    }
}
