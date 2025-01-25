using Google.Apis.YouTube.v3.Data;
using MixItUp.Base.Services;
using MixItUp.Base.Services.YouTube.New;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.User.Platform
{
    [DataContract]
    public class YouTubeUserPlatformV2Model : UserPlatformV2ModelBase
    {
        [DataMember]
        public string YouTubeURL { get; set; }

        [DataMember]
        public HashSet<string> MemberLevels { get; set; } = new HashSet<string>();

        [DataMember]
        public string ModeratorID { get; set; }

        [DataMember]
        public string BanID { get; set; }

        private bool initialRefreshCompleted = false;

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

        public YouTubeUserPlatformV2Model(string id, string username, string displayName)
        {
            this.Platform = StreamingPlatformTypeEnum.YouTube;
            this.ID = id;
            this.Username = username;
            this.DisplayName = displayName;
        }

        [Obsolete]
        public YouTubeUserPlatformV2Model() : base() { }

        public override TimeSpan RefreshTimeSpan { get { return TimeSpan.FromMinutes(10); } }

        public override async Task Refresh()
        {
            if (ServiceManager.Get<YouTubeSession>().IsConnected)
            {
                Channel channel = await ServiceManager.Get<YouTubeSession>().StreamerService.GetChannelByID(this.ID);
                if (channel != null)
                {
                    this.SetChannelProperties(channel);

                    Subscription subscription = await ServiceManager.Get<YouTubeSession>().StreamerService.CheckIfSubscribed(ServiceManager.Get<YouTubeSession>().ChannelID, this.ID);
                    if (subscription != null)
                    {
                        this.Roles.Add(UserRoleEnum.YouTubeSubscriber);
                        this.Roles.Add(UserRoleEnum.Follower);
                        this.FollowDate = YouTubeSession.GetYouTubeDateTime(subscription.Snippet.PublishedAtRaw);
                    }
                    else
                    {
                        this.Roles.Remove(UserRoleEnum.YouTubeSubscriber);
                        this.Roles.Remove(UserRoleEnum.Follower);
                    }

                    if (!this.initialRefreshCompleted)
                    {
                        await this.RefreshMembershipDetails();
                    }

                    this.initialRefreshCompleted = true;
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
            if (message.AuthorDetails.IsChatSponsor.GetValueOrDefault())
            {
                this.Roles.Add(UserRoleEnum.YouTubeMember);
                this.Roles.Add(UserRoleEnum.Subscriber);
            }
            else
            {
                this.Roles.Remove(UserRoleEnum.YouTubeMember);
                this.Roles.Remove(UserRoleEnum.Subscriber);
            }
        }

        public async Task RefreshMembershipDetails()
        {
            if (ServiceManager.Get<YouTubeSession>().HasMembershipCapabilities)
            {
                Member membership = await ServiceManager.Get<YouTubeSession>().StreamerService.CheckIfMember(this.ID);
                if (membership != null)
                {
                    this.Roles.Add(UserRoleEnum.YouTubeMember);
                    this.Roles.Add(UserRoleEnum.Subscriber);
                    this.SubscribeDate = YouTubeSession.GetYouTubeDateTime(membership.Snippet.MembershipsDetails.MembershipsDuration.MemberSince);
                    this.MemberLevels.Clear();
                    this.MemberLevels.AddRange(membership.Snippet.MembershipsDetails.AccessibleLevels);
                }
                else
                {
                    this.Roles.Remove(UserRoleEnum.YouTubeMember);
                    this.Roles.Remove(UserRoleEnum.Subscriber);
                    this.SubscribeDate = null;
                    this.MemberLevels.Clear();
                }
            }
        }

        private void SetChannelProperties(Channel channel)
        {
            this.ID = channel.Id;
            this.Username = channel.Snippet.Title;
            this.DisplayName = channel.Snippet.Title;
            this.AvatarLink = channel.Snippet.Thumbnails.Default__.Url;
            this.YouTubeURL = "https://www.youtube.com/channel/" + channel.Id;
            this.AccountDate = YouTubeSession.GetYouTubeDateTime(channel.Snippet.PublishedAtRaw);
        }
    }
}
