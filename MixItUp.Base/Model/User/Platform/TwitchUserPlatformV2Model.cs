using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using System;
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
        public ChatBadgeModel RoleBadge { get; set; }
        [DataMember]
        public ChatBadgeModel SpecialtyBadge { get; set; }

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

        public TwitchUserPlatformV2Model(string id, string username, string displayName)
        {
            this.Platform = StreamingPlatformTypeEnum.Twitch;
            this.ID = id;
            this.Username = username;
            this.DisplayName = displayName;
        }

        [Obsolete]
        public TwitchUserPlatformV2Model() : base() { }

        public bool HasTwitchSubscriberBadge { get { return this.HasTwitchBadge("subscriber"); } }

        public bool HasTwitchSubscriberFounderBadge { get { return this.HasTwitchBadge("founder"); } }

        public bool IsTwitchSubscriber { get { return this.HasTwitchSubscriberBadge || this.HasTwitchSubscriberFounderBadge; } }

        public override async Task Refresh()
        {
            if (ServiceManager.Get<TwitchSessionService>().IsConnected)
            {
                UserModel user = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetNewAPIUserByID(this.ID);
                if (user != null)
                {
                    this.SetUserProperties(user);
                }

                UserFollowModel follow = await ServiceManager.Get<TwitchSessionService>().UserConnection.CheckIfFollowsNewAPI(ServiceManager.Get<TwitchSessionService>().User, this.GetTwitchNewAPIUserModel());
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

                if (ServiceManager.Get<TwitchSessionService>().User.IsAffiliate() || ServiceManager.Get<TwitchSessionService>().User.IsPartner())
                {
                    SubscriptionModel subscription = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetBroadcasterSubscription(ServiceManager.Get<TwitchSessionService>().User, this.GetTwitchNewAPIUserModel());
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
                        this.SubscriberTier = 1;
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

        public void SetUserProperties(ChatMessagePacketModel message)
        {
            this.SetUserProperties(message.UserDisplayName, message.BadgeDictionary, message.BadgeInfoDictionary, message.Color);
        }

        public void SetUserProperties(ChatUserStatePacketModel userState)
        {
            this.SetUserProperties(userState.UserDisplayName, userState.BadgeDictionary, userState.BadgeInfoDictionary, userState.Color);
        }

        public void SetUserProperties(ChatUserNoticePacketModel userNotice)
        {
            this.SetUserProperties(userNotice.UserDisplayName, userNotice.BadgeDictionary, userNotice.BadgeInfoDictionary, userNotice.Color);
        }

        private void SetUserProperties(string displayName, Dictionary<string, int> badges, Dictionary<string, int> badgeInfo, string color)
        {
            this.DisplayName = displayName;
            this.Badges = badges;
            this.BadgeInfo = badgeInfo;
            if (!string.IsNullOrEmpty(color))
            {
                this.Color = color;
            }

            if (string.Equals(this.ID, ServiceManager.Get<TwitchSessionService>().UserID)) { this.Roles.Add(UserRoleEnum.Streamer); } else { this.Roles.Remove(UserRoleEnum.Streamer); }
            if (this.Badges != null)
            {
                if (this.HasTwitchBadge("admin") || this.HasTwitchBadge("staff")) { this.Roles.Add(UserRoleEnum.TwitchStaff); } else { this.Roles.Remove(UserRoleEnum.TwitchStaff); }
                if (this.HasTwitchBadge("global_mod")) { this.Roles.Add(UserRoleEnum.TwitchGlobalMod); } else { this.Roles.Remove(UserRoleEnum.TwitchGlobalMod); }
                if (this.HasTwitchBadge("moderator")) { this.Roles.Add(UserRoleEnum.Moderator); } else { this.Roles.Remove(UserRoleEnum.Moderator); }
                if (this.IsTwitchSubscriber)
                {
                    this.Roles.Add(UserRoleEnum.Subscriber);
                }
                else
                {
                    this.Roles.Remove(UserRoleEnum.Subscriber);
                }
                if (this.HasTwitchBadge("vip")) { this.Roles.Add(UserRoleEnum.TwitchVIP); } else { this.Roles.Remove(UserRoleEnum.TwitchVIP); }

                if (ServiceManager.Get<TwitchChatService>() != null)
                {
                    if (this.HasTwitchBadge("staff")) { this.RoleBadge = this.GetTwitchBadgeURL("staff"); }
                    else if (this.HasTwitchBadge("admin")) { this.RoleBadge = this.GetTwitchBadgeURL("admin"); }
                    else if (this.HasTwitchBadge("extension")) { this.RoleBadge = this.GetTwitchBadgeURL("extension"); }
                    else if (this.HasTwitchBadge("twitchbot")) { this.RoleBadge = this.GetTwitchBadgeURL("twitchbot"); }
                    else if (this.Roles.Contains(UserRoleEnum.Moderator)) { this.RoleBadge = this.GetTwitchBadgeURL("moderator"); }
                    else if (this.Roles.Contains(UserRoleEnum.TwitchVIP)) { this.RoleBadge = this.GetTwitchBadgeURL("vip"); }

                    if (this.HasTwitchSubscriberFounderBadge) { this.SubscriberBadge = this.GetTwitchBadgeURL("founder"); }
                    else if (this.HasTwitchSubscriberBadge) { this.SubscriberBadge = this.GetTwitchBadgeURL("subscriber"); }

                    if (this.HasTwitchBadge("sub-gift-leader")) { this.SpecialtyBadge = this.GetTwitchBadgeURL("sub-gift-leader"); }
                    else if (this.HasTwitchBadge("bits-leader")) { this.SpecialtyBadge = this.GetTwitchBadgeURL("bits-leader"); }
                    else if (this.HasTwitchBadge("sub-gifter")) { this.SpecialtyBadge = this.GetTwitchBadgeURL("sub-gifter"); }
                    else if (this.HasTwitchBadge("bits")) { this.SpecialtyBadge = this.GetTwitchBadgeURL("bits"); }
                    else if (this.HasTwitchBadge("premium")) { this.SpecialtyBadge = this.GetTwitchBadgeURL("premium"); }
                }
            }

            this.SubscriberBadgeLink = this.SubscriberBadge?.image_url_1x;
            this.RoleBadgeLink = this.RoleBadge?.image_url_1x;
            this.SpecialtyBadgeLink = this.SpecialtyBadge?.image_url_1x;
        }

        private void SetUserProperties(UserModel user)
        {
            this.ID = user.id;
            this.Username = user.login;
            this.DisplayName = user.display_name;
            this.AvatarLink = user.profile_image_url;
            this.AccountDate = TwitchPlatformService.GetTwitchDateTime(user.created_at);

            if (string.Equals(this.ID, ServiceManager.Get<TwitchSessionService>().UserID)) { this.Roles.Add(UserRoleEnum.Streamer); } else { this.Roles.Remove(UserRoleEnum.Streamer); }
            if (user.IsAffiliate()) { this.Roles.Add(UserRoleEnum.TwitchAffiliate); } else { this.Roles.Remove(UserRoleEnum.TwitchAffiliate); }
            if (user.IsPartner()) { this.Roles.Add(UserRoleEnum.TwitchPartner); } else { this.Roles.Remove(UserRoleEnum.TwitchPartner); }
            if (user.IsGlobalMod()) { this.Roles.Add(UserRoleEnum.TwitchGlobalMod); } else { this.Roles.Remove(UserRoleEnum.TwitchGlobalMod); }
            if (user.IsStaff()) { this.Roles.Add(UserRoleEnum.TwitchStaff); } else { this.Roles.Remove(UserRoleEnum.TwitchStaff); }

            if (this.HasTwitchSubscriberFounderBadge) { this.SubscriberBadge = this.GetTwitchBadgeURL("founder"); }
            else if (this.HasTwitchSubscriberBadge) { this.SubscriberBadge = this.GetTwitchBadgeURL("subscriber"); }
            else { this.SubscriberBadge = null; }

            if (ServiceManager.Get<TwitchSessionService>().ChannelEditors.Contains(this.ID)) { this.Roles.Add(UserRoleEnum.TwitchChannelEditor); } else { this.Roles.Remove(UserRoleEnum.TwitchChannelEditor); }

            this.SubscriberBadgeLink = this.SubscriberBadge?.image_url_1x;
            this.RoleBadgeLink = this.RoleBadge?.image_url_1x;
            this.SpecialtyBadgeLink = this.SpecialtyBadge?.image_url_1x;
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
