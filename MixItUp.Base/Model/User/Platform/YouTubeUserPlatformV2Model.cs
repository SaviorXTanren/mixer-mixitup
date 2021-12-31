using Google.Apis.YouTube.v3.Data;
using MixItUp.Base.Services;
using MixItUp.Base.Services.YouTube;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.User.Platform
{
    [DataContract]
    public class YouTubeUserPlatformV2Model : UserPlatformV2ModelBase
    {
        [DataMember]
        public string YouTubeURL { get; set; }

        public YouTubeUserPlatformV2Model(Channel channel)
        {
            this.Platform = StreamingPlatformTypeEnum.YouTube;

            this.SetChannelProperties(channel);
        }

        public YouTubeUserPlatformV2Model(LiveChatMessage message)
        {
            this.Platform = StreamingPlatformTypeEnum.YouTube;

            this.SetMessageProperties(message);
        }

        [Obsolete]
        public YouTubeUserPlatformV2Model() : base() { }

        public override async Task Refresh()
        {
            if (ServiceManager.Get<YouTubeSessionService>().IsConnected)
            {
                Channel channel = await ServiceManager.Get<YouTubeSessionService>().UserConnection.GetChannelByID(this.ID);
                if (channel != null)
                {
                    this.SetChannelProperties(channel);
                }
            }
        }

        public void SetMessageProperties(LiveChatMessage message)
        {
            this.ID = message.AuthorDetails.ChannelId;
            this.Username = message.AuthorDetails.DisplayName;
            this.DisplayName = message.AuthorDetails.DisplayName;
            this.AvatarLink = message.AuthorDetails.ProfileImageUrl;
            this.YouTubeURL = message.AuthorDetails.ChannelUrl;

            if (message.AuthorDetails.IsChatOwner.GetValueOrDefault()) { this.Roles.Add(UserRoleEnum.Streamer); } else { this.Roles.Remove(UserRoleEnum.Streamer); }
            if (message.AuthorDetails.IsChatModerator.GetValueOrDefault()) { this.Roles.Add(UserRoleEnum.Moderator); } else { this.Roles.Remove(UserRoleEnum.Moderator); }
            if (message.AuthorDetails.IsChatSponsor.GetValueOrDefault()) { this.Roles.Add(UserRoleEnum.YouTubeMember); } else { this.Roles.Remove(UserRoleEnum.YouTubeMember); }
        }

        private void SetChannelProperties(Channel channel)
        {
            this.ID = channel.Id;
            this.Username = channel.Snippet.Title;
            this.DisplayName = channel.Snippet.Title;
            this.AvatarLink = channel.Snippet.Thumbnails.Default__.Url;
            this.YouTubeURL = "https://www.youtube.com/channel/" + channel.Id;
        }
    }
}
