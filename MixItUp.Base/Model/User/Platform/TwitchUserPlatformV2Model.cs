using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Twitch.Base.Models.Clients.Chat;
using Twitch.Base.Models.Clients.PubSub.Messages;
using Twitch.Base.Models.NewAPI.Chat;
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

        [DataMember]
        public ChatBadgeModel SubscriberBadge { get; set; }

        [DataMember]
        public long TotalBitsCheered { get; set; }

        public TwitchUserPlatformV2Model(UserModel user)
        {
            this.Platform = StreamingPlatformTypeEnum.Twitch;

            this.SetUserProperties(user);
        }

        public TwitchUserPlatformV2Model(ChatMessagePacketModel message) : this(message.UserID, message.UserLogin, message.UserDisplayName) { }

        public TwitchUserPlatformV2Model(PubSubWhisperEventModel whisper) : this(whisper.from_id.ToString(), whisper.tags.login, whisper.tags.display_name) { }

        public TwitchUserPlatformV2Model(PubSubWhisperEventRecipientModel whisper)
            : this(whisper.id.ToString(), whisper.username, whisper.display_name)
        {
            this.AvatarLink = whisper.profile_image;
        }

        public TwitchUserPlatformV2Model(ChatUserNoticePacketModel notice)
             : this(notice.UserID.ToString(), !string.IsNullOrEmpty(notice.RaidUserLogin) ? notice.RaidUserLogin : notice.Login, !string.IsNullOrEmpty(notice.RaidUserDisplayName) ? notice.RaidUserDisplayName : notice.DisplayName)
        { }

        public TwitchUserPlatformV2Model(ChatClearChatPacketModel packet) : this(packet.UserID, packet.UserLogin, null) { }

        public TwitchUserPlatformV2Model(PubSubBitsEventV2Model packet) : this(packet.user_id, packet.user_name, null) { }

        public TwitchUserPlatformV2Model(PubSubSubscriptionsEventModel packet) : this(packet.user_id, packet.user_name, null) { }

        public TwitchUserPlatformV2Model(UserFollowModel follow) : this(follow.from_id, follow.from_name, null) { }

        public TwitchUserPlatformV2Model(TwitchWebhookUserFollowModel follow) : this(follow.ID, follow.Username, follow.DisplayName) { }

        public TwitchUserPlatformV2Model(string id, string username, string displayName)
        {
            this.Platform = StreamingPlatformTypeEnum.Twitch;
            this.ID = id;
            this.Username = username;
            this.DisplayName = displayName;
        }

        private TwitchUserPlatformV2Model() { }

        public bool HasTwitchSubscriberBadge { get { return this.HasTwitchBadge("subscriber"); } }

        public bool HasTwitchSubscriberFounderBadge { get { return this.HasTwitchBadge("founder"); } }

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

        public UserModel GetTwitchNewAPIUserModel()
        {
            return new UserModel()
            {
                id = this.ID,
                login = this.Username
            };
        }

        private void SetUserProperties(UserModel user)
        {
            this.ID = user.id;
            this.Username = user.login;
            this.DisplayName = user.display_name;
            this.AvatarLink = user.profile_image_url;
            this.AccountDate = TwitchPlatformService.GetTwitchDateTime(user.created_at);

            if (user.IsAffiliate()) { this.Roles.Add(UserRoleEnum.TwitchAffiliate); } else { this.Roles.Remove(UserRoleEnum.TwitchAffiliate); }
            if (user.IsPartner()) { this.Roles.Add(UserRoleEnum.TwitchPartner); } else { this.Roles.Remove(UserRoleEnum.TwitchPartner); }
            if (user.IsGlobalMod()) { this.Roles.Add(UserRoleEnum.TwitchGlobalMod); } else { this.Roles.Remove(UserRoleEnum.TwitchGlobalMod); }
            if (user.IsStaff()) { this.Roles.Add(UserRoleEnum.TwitchStaff); } else { this.Roles.Remove(UserRoleEnum.TwitchStaff); }

            if (this.HasTwitchSubscriberFounderBadge) { this.SubscriberBadge = this.GetTwitchBadgeURL("founder"); }
            else if (this.HasTwitchSubscriberBadge) { this.SubscriberBadge = this.GetTwitchBadgeURL("subscriber"); }
            else { this.SubscriberBadge = null; }

            if (ServiceManager.Get<TwitchSessionService>().ChannelEditors.Contains(this.ID)) { this.Roles.Add(UserRoleEnum.TwitchChannelEditor); } else { this.Roles.Remove(UserRoleEnum.TwitchChannelEditor); }
        }

        private int GetTwitchBadgeVersion(string name)
        {
            if (this.Badges != null && this.Badges.TryGetValue(name, out int version))
            {
                return version;
            }
            return -1;
        }

        private bool HasTwitchBadge(string name) { return this.GetTwitchBadgeVersion(name) >= 0; }

        private ChatBadgeModel GetTwitchBadgeURL(string name)
        {
            if (ServiceManager.Get<TwitchChatService>().ChatBadges.ContainsKey(name))
            {
                int versionID = this.GetTwitchBadgeVersion(name);
                if (ServiceManager.Get<TwitchChatService>().ChatBadges[name].versions.ContainsKey(versionID.ToString()))
                {
                    return ServiceManager.Get<TwitchChatService>().ChatBadges[name].versions[versionID.ToString()];
                }
            }
            return null;
        }
    }
}
