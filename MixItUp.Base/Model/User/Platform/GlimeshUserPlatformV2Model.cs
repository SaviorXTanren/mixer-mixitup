using Glimesh.Base.Models.Clients.Chat;
using Glimesh.Base.Models.Users;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Glimesh;
using System;
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

        [Obsolete]
        public GlimeshUserPlatformV2Model() : base() { }

        public override async Task Refresh()
        {
            if (ServiceManager.Get<GlimeshSessionService>().IsConnected)
            {
                UserModel user = await ServiceManager.Get<GlimeshSessionService>().UserConnection.GetUserByID(this.ID);
                if (user != null)
                {
                    this.SetUserProperties(user);
                }

                if (string.Equals(this.ID, ServiceManager.Get<GlimeshSessionService>().User?.id)) { this.Roles.Add(UserRoleEnum.Streamer); } else { this.Roles.Remove(UserRoleEnum.Streamer); }
                if (ServiceManager.Get<GlimeshSessionService>().Moderators.Contains(this.ID)) { this.Roles.Add(UserRoleEnum.Moderator); } else { this.Roles.Remove(UserRoleEnum.Moderator); }
                if (ServiceManager.Get<GlimeshSessionService>().Followers.Contains(this.ID)) { this.Roles.Add(UserRoleEnum.Follower); } else { this.Roles.Remove(UserRoleEnum.Follower); }
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

        public void SetUserProperties(ChatMessagePacketModel message) { this.SetUserProperties(message.User); }
    }
}
