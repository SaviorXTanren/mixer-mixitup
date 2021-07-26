using MixItUp.Base.Model.User.Twitch;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Twitch.Base.Models.Clients.Chat;
using Twitch.Base.Models.Clients.PubSub.Messages;
using Twitch.Base.Models.NewAPI.Subscriptions;
using Twitch.Base.Models.NewAPI.Users;

namespace MixItUp.Base.Model.User.Platform
{
    [DataContract]
    public class TwitchUserPlatformV2Model : UserPlatformV2ModelBase
    {
        [DataMember]
        public string Color { get; set; }

        [DataMember]
        public Dictionary<string, int> Badges { get; set; } = new Dictionary<string, int>();
        [DataMember]
        public Dictionary<string, int> BadgeInfo { get; set; } = new Dictionary<string, int>();

        public TwitchUserPlatformV2Model(UserModel user)
        {
            this.Platform = StreamingPlatformTypeEnum.Twitch;

            this.SetUserProperties(user);
        }

        public TwitchUserPlatformV2Model(ChatMessagePacketModel message)
        {
            this.Platform = StreamingPlatformTypeEnum.Twitch;
            this.ID = message.UserID;
            this.Username = message.UserLogin;
            this.DisplayName = message.UserDisplayName;
        }

        public TwitchUserPlatformV2Model(PubSubWhisperEventModel whisper)
        {
            this.Platform = StreamingPlatformTypeEnum.Twitch;
            this.ID = whisper.from_id.ToString();
            this.Username = whisper.tags.login;
            this.DisplayName = whisper.tags.display_name;
        }

        public TwitchUserPlatformV2Model(PubSubWhisperEventRecipientModel whisper)
        {
            this.Platform = StreamingPlatformTypeEnum.Twitch;
            this.ID = whisper.id.ToString();
            this.Username = whisper.username;
            this.DisplayName = whisper.display_name;
            this.AvatarLink = whisper.profile_image;
        }

        public TwitchUserPlatformV2Model(ChatUserNoticePacketModel notice)
        {
            this.Platform = StreamingPlatformTypeEnum.Twitch;
            this.ID = notice.UserID.ToString();
            this.Username = !string.IsNullOrEmpty(notice.RaidUserLogin) ? notice.RaidUserLogin : notice.Login;
            this.DisplayName = !string.IsNullOrEmpty(notice.RaidUserDisplayName) ? notice.RaidUserDisplayName : notice.DisplayName;
        }

        public TwitchUserPlatformV2Model(ChatClearChatPacketModel packet)
        {
            this.Platform = StreamingPlatformTypeEnum.Twitch;
            this.ID = packet.UserID;
            this.Username = packet.UserLogin;
        }

        public TwitchUserPlatformV2Model(PubSubBitsEventV2Model packet)
        {
            this.Platform = StreamingPlatformTypeEnum.Twitch;
            this.ID = packet.user_id;
            this.Username = packet.user_name;
        }

        public TwitchUserPlatformV2Model(PubSubSubscriptionsEventModel packet)
        {
            this.Platform = StreamingPlatformTypeEnum.Twitch;
            this.ID = packet.user_id;
            this.Username = packet.user_name;
        }

        public TwitchUserPlatformV2Model(UserFollowModel follow)
        {
            this.Platform = StreamingPlatformTypeEnum.Twitch;
            this.ID = follow.from_id;
            this.Username = follow.from_name;
        }

        public TwitchUserPlatformV2Model(TwitchWebhookFollowModel follow)
        {
            this.Platform = StreamingPlatformTypeEnum.Twitch;
            this.ID = follow.UserID;
            this.Username = follow.Username;
            this.DisplayName = follow.UserDisplayName;
        }

        private TwitchUserPlatformV2Model() { }

        public override async Task Refresh()
        {
            if (ServiceManager.Get<TwitchSessionService>().IsConnected)
            {
                UserModel user = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetNewAPIUserByID(this.ID);
                if (user != null)
                {
                    this.SetUserProperties(user);
                }

                UserFollowModel follow = await ServiceManager.Get<TwitchSessionService>().UserConnection.CheckIfFollowsNewAPI(ServiceManager.Get<TwitchSessionService>().UserNewAPI, this.GetTwitchNewAPIUserModel());
                if (follow != null && !string.IsNullOrEmpty(follow.followed_at))
                {
                    this.Roles.Add(UserRoleEnum.Follower);
                    this.FollowDate = TwitchPlatformService.GetTwitchDateTime(follow.followed_at);
                }
                else
                {
                    this.Roles.Remove(UserRoleEnum.Follower);
                    this.FollowDate = null;
                }

                if (ServiceManager.Get<TwitchSessionService>().UserNewAPI.IsAffiliate() || ServiceManager.Get<TwitchSessionService>().UserNewAPI.IsPartner())
                {
                    SubscriptionModel subscription = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetUserSubscription(ServiceManager.Get<TwitchSessionService>().UserNewAPI, this.GetTwitchNewAPIUserModel());
                    if (subscription != null)
                    {
                        this.Roles.Add(UserRoleEnum.Subscriber);
                        this.SubscriberTier = TwitchEventService.GetSubTierNumberFromText(subscription.tier);
                        // TODO: No subscription data from this API. https://twitch.uservoice.com/forums/310213-developers/suggestions/43806120-add-subscription-date-to-subscription-apis
                        //this.SubscribeDate = TwitchPlatformService.GetTwitchDateTime(subscription.created_at);
                    }
                    else
                    {
                        this.Roles.Remove(UserRoleEnum.Subscriber);
                        this.SubscriberTier = 0;
                        this.SubscribeDate = null;
                    }
                }
            }
        }

        private void SetUserProperties(UserModel user)
        {
            this.ID = user.id;
            this.Username = user.login;
            this.DisplayName = user.display_name;
            this.AvatarLink = user.profile_image_url;
            this.AccountDate = TwitchPlatformService.GetTwitchDateTime(user.created_at);

            if (user.IsPartner()) { this.Roles.Add(UserRoleEnum.Partner); } else { this.Roles.Remove(UserRoleEnum.Partner); }
            if (user.IsAffiliate()) { this.Roles.Add(UserRoleEnum.Affiliate); } else { this.Roles.Remove(UserRoleEnum.Affiliate); }
            if (user.IsStaff()) { this.Roles.Add(UserRoleEnum.Staff); } else { this.Roles.Remove(UserRoleEnum.Staff); }
            if (user.IsGlobalMod()) { this.Roles.Add(UserRoleEnum.GlobalMod); } else { this.Roles.Remove(UserRoleEnum.GlobalMod); }

            if (ServiceManager.Get<TwitchSessionService>().ChannelEditors.Contains(this.ID))
            {
                this.Roles.Add(UserRoleEnum.ChannelEditor);
            }
            else
            {
                this.Roles.Remove(UserRoleEnum.ChannelEditor);
            }
        }

        private UserModel GetTwitchNewAPIUserModel()
        {
            return new UserModel()
            {
                id = this.ID,
                login = this.Username
            };
        }
    }
}
