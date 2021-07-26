using Glimesh.Base.Models.Clients.Chat;
using Glimesh.Base.Models.Users;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Glimesh;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.User.Platform
{
    [DataContract]
    public class GlimeshUserPlatformV2Model : UserPlatformV2ModelBase
    {
        public GlimeshUserPlatformV2Model(UserModel user)
        {
            this.Platform = StreamingPlatformTypeEnum.Glimesh;

            this.SetUserProperties(user);
        }

        public GlimeshUserPlatformV2Model(ChatMessagePacketModel message) : this(message.User) { }

        private GlimeshUserPlatformV2Model() { }

        public override async Task Refresh()
        {
            if (ServiceManager.Get<GlimeshSessionService>().IsConnected)
            {
                UserModel user = await ServiceManager.Get<GlimeshSessionService>().UserConnection.GetUserByID(this.ID);
                if (user != null)
                {
                    this.SetUserProperties(user);
                }
            }
        }

        private void SetUserProperties(UserModel user)
        {
            this.ID = user.id;
            this.Username = user.username;
            this.DisplayName = user.displayname;
            this.AvatarLink = user.avatarUrl;
            this.AccountDate = GlimeshPlatformService.GetGlimeshDateTime(user.confirmedAt);
        }
    }
}
