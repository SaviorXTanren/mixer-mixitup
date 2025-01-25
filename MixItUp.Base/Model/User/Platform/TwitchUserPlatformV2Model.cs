using MixItUp.Base.Model.Twitch.Channels;
using MixItUp.Base.Model.Twitch.Chat;
using MixItUp.Base.Model.Twitch.Clients.Chat;
using MixItUp.Base.Model.Twitch.Clients.PubSub.Messages;
using MixItUp.Base.Model.Twitch.EventSub;
using MixItUp.Base.Model.Twitch.Subscriptions;
using MixItUp.Base.Model.Twitch.User;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Services.Twitch.New;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.User.Platform
{
    [DataContract]
    public class TwitchUserPlatformV2Model : UserPlatformV2ModelBase
    {
        private const int FollowDelayLeniencyMinutes = 10;

        private static readonly HashSet<string> NonApplicableSpecialtyBadges = new HashSet<string>
        {
            "admin", "artist-badge", "broadcaster", "extension", "founder", "global_mod", "moderator", "staff", "subscriber", "twitchbot", "vip"
        };

        [DataMember]
        public string Color { get; set; }

        [Obsolete]
        [DataMember]
        public Dictionary<string, int> Badges { get; set; } = new Dictionary<string, int>();
        [DataMember]
        public Dictionary<string, int> BadgeInfo { get; set; } = new Dictionary<string, int>();

        [DataMember]
        public Dictionary<string, ChatMessageNotificationBadge> NewBadges { get; set; } = new Dictionary<string, ChatMessageNotificationBadge>();

        [DataMember]
        public ChatBadgeModel NewSubscriberBadge { get; set; }
        [DataMember]
        public ChatBadgeModel NewRoleBadge { get; set; }
        [DataMember]
        public ChatBadgeModel NewSpecialtyBadge { get; set; }

        [DataMember]
        public long TotalBitsCheered { get; set; }

        public TwitchUserPlatformV2Model(UserModel user)
        {
            this.Platform = StreamingPlatformTypeEnum.Twitch;

            this.SetUserProperties(user);
        }

        [Obsolete]
        public TwitchUserPlatformV2Model(ChatMessagePacketModel message) : this(message.UserID, message.UserLogin, message.UserDisplayName)
        {
            this.SetUserProperties(message);
        }

        public TwitchUserPlatformV2Model(PubSubWhisperEventModel whisper) : this(whisper.from_id.ToString(), whisper.tags.login, whisper.tags.display_name) { }

        public TwitchUserPlatformV2Model(PubSubWhisperEventRecipientModel whisper)
            : this(whisper.id.ToString(), whisper.username, whisper.display_name)
        {
            this.AvatarLink = whisper.profile_image;
        }

        [Obsolete]
        public TwitchUserPlatformV2Model(ChatUserNoticePacketModel notice)
             : this(notice.UserID.ToString(), !string.IsNullOrEmpty(notice.RaidUserLogin) ? notice.RaidUserLogin : notice.Login, !string.IsNullOrEmpty(notice.RaidUserDisplayName) ? notice.RaidUserDisplayName : notice.DisplayName)
        {
            this.SetUserProperties(notice);
        }

        public TwitchUserPlatformV2Model(ChatClearChatPacketModel packet) : this(packet.UserID, packet.UserLogin, null) { }

        public TwitchUserPlatformV2Model(PubSubBitsEventV2Model packet) : this(packet.user_id, packet.user_name, null) { }

        public TwitchUserPlatformV2Model(PubSubSubscriptionsEventModel packet) : this(packet.user_id, packet.user_name, null) { }

        public TwitchUserPlatformV2Model(ChannelFollowerModel follow) : this(follow.user_id, follow.user_login, follow.user_name) { }

        public TwitchUserPlatformV2Model(UserSubscriptionNotification subscription) : this(subscription.user_id, subscription.user_login, subscription.user_name) { }

        public TwitchUserPlatformV2Model(UserSubscriptionGiftNotification subscriptionGift) : this(subscriptionGift.user_id, subscriptionGift.user_login, subscriptionGift.user_name) { }

        public TwitchUserPlatformV2Model(ChannelPointAutomaticRewardRedemptionNotification redemption) : this(redemption.user_id, redemption.user_login, redemption.user_name) { }

        public TwitchUserPlatformV2Model(ChannelPointRewardCustomRedemptionNotification redemption) : this(redemption.user_id, redemption.user_login, redemption.user_name) { }

        public TwitchUserPlatformV2Model(ChatUserClearNotification userClear) : this(userClear.target_user_id, userClear.target_user_login, userClear.target_user_name) { }

        public TwitchUserPlatformV2Model(CheerNotification cheer) : this(cheer.user_id, cheer.user_login, cheer.user_name) { }

        public TwitchUserPlatformV2Model(ChatMessageDeletedNotification messageDeleted) : this(messageDeleted.target_user_id, messageDeleted.target_user_login, messageDeleted.target_user_name) { }

        public TwitchUserPlatformV2Model(ChatNotification notification) : this(notification.chatter_user_id, notification.chatter_user_login, notification.chatter_user_name) { this.SetUserProperties(notification); }

        public TwitchUserPlatformV2Model(ChatMessageNotification message) : this(message.chatter_user_id, message.chatter_user_login, message.chatter_user_name) { this.SetUserProperties(message); }

        public TwitchUserPlatformV2Model(ChatNotificationResub resub) : this(resub.gifter_user_id, resub.gifter_user_login, resub.gifter_user_name) { }

        public TwitchUserPlatformV2Model(ChatNotificationSubGift subGift) : this(subGift.recipient_user_id, subGift.recipient_user_login, subGift.recipient_user_name) { }

        public TwitchUserPlatformV2Model(ModerationNotification moderation) : this(moderation.moderator_user_id, moderation.moderator_user_login, moderation.moderator_user_name) { }

        public TwitchUserPlatformV2Model(ModerationNotificationBasicUser moderation) : this(moderation.user_id, moderation.user_login, moderation.user_name) { }

        public TwitchUserPlatformV2Model(string id, string username, string displayName)
        {
            this.Platform = StreamingPlatformTypeEnum.Twitch;
            this.ID = id;
            this.Username = username;
            this.DisplayName = displayName;
        }

        [Obsolete]
        public TwitchUserPlatformV2Model() : base() { }

        public bool HasTwitchSubscriberBadge { get { return this.HasNewTwitchBadge("subscriber"); } }

        public bool HasTwitchSubscriberFounderBadge { get { return this.HasNewTwitchBadge("founder"); } }

        public bool IsTwitchSubscriber { get { return this.HasTwitchSubscriberBadge || this.HasTwitchSubscriberFounderBadge; } }

        public override async Task Refresh()
        {
            if (ServiceManager.Get<TwitchSession>().IsConnected)
            {
                UserModel user = await ServiceManager.Get<TwitchSession>().StreamerService.GetNewAPIUserByID(this.ID);
                if (user != null)
                {
                    this.SetUserProperties(user);
                }

                ChannelFollowerModel follow = await ServiceManager.Get<TwitchSession>().StreamerService.CheckIfFollowsNewAPI(ServiceManager.Get<TwitchSession>().StreamerModel, this.GetTwitchNewAPIUserModel());
                if (follow != null && !string.IsNullOrEmpty(follow.followed_at))
                {
                    this.Roles.Add(UserRoleEnum.Follower);
                    this.FollowDate = TwitchService.GetTwitchDateTime(follow.followed_at);
                }
                else
                {
                    if (!this.Roles.Contains(UserRoleEnum.Follower) || (DateTimeOffset.Now - this.FollowDate.GetValueOrDefault()).TotalMinutes > FollowDelayLeniencyMinutes)
                    {
                        this.Roles.Remove(UserRoleEnum.Follower);
                        this.FollowDate = null;
                    }
                }

                if (ServiceManager.Get<TwitchSession>().StreamerModel.IsAffiliate() || ServiceManager.Get<TwitchSession>().StreamerModel.IsPartner())
                {
                    SubscriptionModel subscription = await ServiceManager.Get<TwitchSession>().StreamerService.GetBroadcasterSubscription(ServiceManager.Get<TwitchSession>().StreamerModel, this.ID);
                    if (subscription != null)
                    {
                        this.Roles.Add(UserRoleEnum.Subscriber);
                        this.SubscriberTier = TwitchClient.GetSubTierNumberFromText(subscription.tier);
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

        [Obsolete]
        public void SetUserProperties(ChatMessagePacketModel message)
        {
            this.SetUserProperties(message.UserDisplayName, message.BadgeDictionary, message.BadgeInfoDictionary, message.Color);
        }

        [Obsolete]
        public void SetUserProperties(ChatUserStatePacketModel userState)
        {
            this.SetUserProperties(userState.UserDisplayName, userState.BadgeDictionary, userState.BadgeInfoDictionary, userState.Color);
        }

        [Obsolete]
        public void SetUserProperties(ChatUserNoticePacketModel userNotice)
        {
            this.SetUserProperties(userNotice.UserDisplayName, userNotice.BadgeDictionary, userNotice.BadgeInfoDictionary, userNotice.Color);
        }

        [Obsolete]
        private void SetUserProperties(string displayName, Dictionary<string, int> badges, Dictionary<string, int> badgeInfo, string color)
        {
            this.DisplayName = displayName;
            this.Badges = badges;
            this.BadgeInfo = badgeInfo;
            if (!string.IsNullOrEmpty(color))
            {
                this.Color = color;
            }

            if (string.Equals(this.ID, ServiceManager.Get<TwitchSession>().Streamer)) { this.Roles.Add(UserRoleEnum.Streamer); } else { this.Roles.Remove(UserRoleEnum.Streamer); }
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
                    if (this.HasTwitchBadge("broadcaster")) { this.NewRoleBadge = this.GetTwitchBadgeURL("broadcaster"); }
                    else if (this.HasTwitchBadge("staff")) { this.NewRoleBadge = this.GetTwitchBadgeURL("staff"); }
                    else if (this.HasTwitchBadge("admin")) { this.NewRoleBadge = this.GetTwitchBadgeURL("admin"); }
                    else if (this.HasTwitchBadge("extension")) { this.NewRoleBadge = this.GetTwitchBadgeURL("extension"); }
                    else if (this.HasTwitchBadge("twitchbot")) { this.NewRoleBadge = this.GetTwitchBadgeURL("twitchbot"); }
                    else if (this.Roles.Contains(UserRoleEnum.Moderator)) { this.NewRoleBadge = this.GetTwitchBadgeURL("moderator"); }
                    else if (this.Roles.Contains(UserRoleEnum.TwitchVIP)) { this.NewRoleBadge = this.GetTwitchBadgeURL("vip"); }
                    else if (this.HasTwitchBadge("artist-badge")) { this.NewRoleBadge = this.GetTwitchBadgeURL("artist-badge"); }
                    else { this.NewRoleBadge = null; }

                    if (this.HasTwitchSubscriberFounderBadge) { this.NewSubscriberBadge = this.GetTwitchBadgeURL("founder"); }
                    else if (this.HasTwitchSubscriberBadge) { this.NewSubscriberBadge = this.GetTwitchBadgeURL("subscriber"); }
                    else { this.NewSubscriberBadge = null; }

                    this.NewSpecialtyBadge = null;
                    if (this.HasTwitchBadge("sub-gift-leader")) { this.NewSpecialtyBadge = this.GetTwitchBadgeURL("sub-gift-leader"); }
                    else if (this.HasTwitchBadge("bits-leader")) { this.NewSpecialtyBadge = this.GetTwitchBadgeURL("bits-leader"); }
                    else if (this.HasTwitchBadge("sub-gifter")) { this.NewSpecialtyBadge = this.GetTwitchBadgeURL("sub-gifter"); }
                    else if (this.HasTwitchBadge("bits")) { this.NewSpecialtyBadge = this.GetTwitchBadgeURL("bits"); }
                    else if (this.HasTwitchBadge("premium")) { this.NewSpecialtyBadge = this.GetTwitchBadgeURL("premium"); }
                    else if (this.Badges != null)
                    {
                        foreach (string name in this.Badges.Keys.ToList())
                        {
                            if (!TwitchUserPlatformV2Model.NonApplicableSpecialtyBadges.Contains(name))
                            {
                                this.NewSpecialtyBadge = this.GetTwitchBadgeURL(name);
                                break;
                            }
                        }
                    }
                }
            }

            this.SubscriberBadgeLink = this.NewSubscriberBadge?.image_url_1x;
            this.RoleBadgeLink = this.NewRoleBadge?.image_url_1x;
            this.SpecialtyBadgeLink = this.NewSpecialtyBadge?.image_url_1x;
        }

        private void SetUserProperties(UserModel user)
        {
            this.ID = user.id;
            this.Username = user.login;
            this.DisplayName = user.display_name;
            this.AvatarLink = user.profile_image_url;
            this.AccountDate = TwitchService.GetTwitchDateTime(user.created_at);

            if (string.Equals(this.ID, ServiceManager.Get<TwitchSession>().StreamerID)) { this.Roles.Add(UserRoleEnum.Streamer); } else { this.Roles.Remove(UserRoleEnum.Streamer); }
            if (user.IsAffiliate()) { this.Roles.Add(UserRoleEnum.TwitchAffiliate); } else { this.Roles.Remove(UserRoleEnum.TwitchAffiliate); }
            if (user.IsPartner()) { this.Roles.Add(UserRoleEnum.TwitchPartner); } else { this.Roles.Remove(UserRoleEnum.TwitchPartner); }
            if (user.IsGlobalMod()) { this.Roles.Add(UserRoleEnum.TwitchGlobalMod); } else { this.Roles.Remove(UserRoleEnum.TwitchGlobalMod); }
            if (user.IsStaff()) { this.Roles.Add(UserRoleEnum.TwitchStaff); } else { this.Roles.Remove(UserRoleEnum.TwitchStaff); }

            if (this.HasTwitchSubscriberFounderBadge) { this.NewSubscriberBadge = this.GetNewTwitchBadgeURL("founder"); }
            else if (this.HasTwitchSubscriberBadge) { this.NewSubscriberBadge = this.GetNewTwitchBadgeURL("subscriber"); }
            else { this.NewSubscriberBadge = null; }

            if (ServiceManager.Get<TwitchSession>().ChannelEditors.Contains(this.ID)) { this.Roles.Add(UserRoleEnum.TwitchChannelEditor); } else { this.Roles.Remove(UserRoleEnum.TwitchChannelEditor); }

            this.SubscriberBadgeLink = this.NewSubscriberBadge?.image_url_1x;
            this.RoleBadgeLink = this.NewRoleBadge?.image_url_1x;
            this.SpecialtyBadgeLink = this.NewSpecialtyBadge?.image_url_1x;
        }

        public void SetUserProperties(ChatMessageNotification notification)
        {
            this.SetUserProperties(notification.chatter_user_name, notification.badges, notification.color);
        }

        public void SetUserProperties(ChatNotification notification)
        {
            this.SetUserProperties(notification.chatter_user_name, notification.badges, notification.color);
        }

        private void SetUserProperties(string displayName, List<ChatMessageNotificationBadge> badges, string color)
        {
            this.DisplayName = displayName;

            this.NewBadges.Clear();
            if (badges != null)
            {
                foreach (ChatMessageNotificationBadge badge in badges)
                {
                    this.NewBadges[badge.set_id] = badge;
                }
            }

            if (!string.IsNullOrEmpty(color))
            {
                this.Color = color;
            }

            if (string.Equals(this.ID, ServiceManager.Get<TwitchSession>().StreamerID)) { this.Roles.Add(UserRoleEnum.Streamer); } else { this.Roles.Remove(UserRoleEnum.Streamer); }

            if (this.HasNewTwitchBadge("admin") || this.HasNewTwitchBadge("staff")) { this.Roles.Add(UserRoleEnum.TwitchStaff); } else { this.Roles.Remove(UserRoleEnum.TwitchStaff); }
            if (this.HasNewTwitchBadge("global_mod")) { this.Roles.Add(UserRoleEnum.TwitchGlobalMod); } else { this.Roles.Remove(UserRoleEnum.TwitchGlobalMod); }
            if (this.HasNewTwitchBadge("moderator")) { this.Roles.Add(UserRoleEnum.Moderator); } else { this.Roles.Remove(UserRoleEnum.Moderator); }
            if (this.IsTwitchSubscriber)
            {
                this.Roles.Add(UserRoleEnum.Subscriber);
            }
            else
            {
                this.Roles.Remove(UserRoleEnum.Subscriber);
            }
            if (this.HasNewTwitchBadge("vip")) { this.Roles.Add(UserRoleEnum.TwitchVIP); } else { this.Roles.Remove(UserRoleEnum.TwitchVIP); }

            if (this.HasNewTwitchBadge("broadcaster")) { this.NewRoleBadge = this.GetNewTwitchBadgeURL("broadcaster"); }
            else if (this.HasNewTwitchBadge("staff")) { this.NewRoleBadge = this.GetNewTwitchBadgeURL("staff"); }
            else if (this.HasNewTwitchBadge("admin")) { this.NewRoleBadge = this.GetNewTwitchBadgeURL("admin"); }
            else if (this.HasNewTwitchBadge("extension")) { this.NewRoleBadge = this.GetNewTwitchBadgeURL("extension"); }
            else if (this.HasNewTwitchBadge("twitchbot")) { this.NewRoleBadge = this.GetNewTwitchBadgeURL("twitchbot"); }
            else if (this.Roles.Contains(UserRoleEnum.Moderator)) { this.NewRoleBadge = this.GetNewTwitchBadgeURL("moderator"); }
            else if (this.Roles.Contains(UserRoleEnum.TwitchVIP)) { this.NewRoleBadge = this.GetNewTwitchBadgeURL("vip"); }
            else if (this.HasNewTwitchBadge("artist-badge")) { this.NewRoleBadge = this.GetNewTwitchBadgeURL("artist-badge"); }
            else { this.NewRoleBadge = null; }

            if (this.HasTwitchSubscriberFounderBadge) { this.NewSubscriberBadge = this.GetNewTwitchBadgeURL("founder"); }
            else if (this.HasTwitchSubscriberBadge) { this.NewSubscriberBadge = this.GetNewTwitchBadgeURL("subscriber"); }
            else { this.NewSubscriberBadge = null; }

            this.NewSpecialtyBadge = null;
            if (this.HasNewTwitchBadge("sub-gift-leader")) { this.NewSpecialtyBadge = this.GetNewTwitchBadgeURL("sub-gift-leader"); }
            else if (this.HasNewTwitchBadge("bits-leader")) { this.NewSpecialtyBadge = this.GetNewTwitchBadgeURL("bits-leader"); }
            else if (this.HasNewTwitchBadge("sub-gifter")) { this.NewSpecialtyBadge = this.GetNewTwitchBadgeURL("sub-gifter"); }
            else if (this.HasNewTwitchBadge("bits")) { this.NewSpecialtyBadge = this.GetNewTwitchBadgeURL("bits"); }
            else if (this.HasNewTwitchBadge("premium")) { this.NewSpecialtyBadge = this.GetNewTwitchBadgeURL("premium"); }
            else if (this.NewBadges.Count > 0)
            {
                foreach (string name in this.NewBadges.Keys.ToList())
                {
                    if (!TwitchUserPlatformV2Model.NonApplicableSpecialtyBadges.Contains(name))
                    {
                        this.NewSpecialtyBadge = this.GetNewTwitchBadgeURL(name);
                        break;
                    }
                }
            }

            this.SubscriberBadgeLink = this.NewSubscriberBadge?.image_url_1x;
            this.RoleBadgeLink = this.NewRoleBadge?.image_url_1x;
            this.SpecialtyBadgeLink = this.NewSpecialtyBadge?.image_url_1x;
        }

        [Obsolete]
        private int GetTwitchBadgeID(string name)
        {
            if (this.Badges != null && this.Badges.TryGetValue(name, out int version))
            {
                return version;
            }
            return -1;
        }

        [Obsolete]
        private bool HasTwitchBadge(string name) { return this.GetTwitchBadgeID(name) >= 0; }

        [Obsolete]
        private ChatBadgeModel GetTwitchBadgeURL(string name)
        {
            if (ServiceManager.Get<TwitchChatService>().ChatBadges.ContainsKey(name))
            {
                int id = this.GetTwitchBadgeID(name);
                if (ServiceManager.Get<TwitchChatService>().ChatBadges[name].ContainsKey(id.ToString()))
                {
                    return ServiceManager.Get<TwitchChatService>().ChatBadges[name][id.ToString()];
                }
            }
            return null;
        }

        private ChatMessageNotificationBadge GetUserNewTwitchBadgeID(string name)
        {
            if (this.NewBadges.TryGetValue(name, out ChatMessageNotificationBadge version))
            {
                return version;
            }
            return null;
        }

        private bool HasNewTwitchBadge(string name) { return this.GetUserNewTwitchBadgeID(name) != null; }

        private ChatBadgeModel GetNewTwitchBadgeURL(string name)
        {
            if (ServiceManager.Get<TwitchSession>().ChatBadges.ContainsKey(name))
            {
                ChatMessageNotificationBadge badge = this.GetUserNewTwitchBadgeID(name);
                if (ServiceManager.Get<TwitchSession>().ChatBadges.TryGetValue(name, out var badgeDictionary) && badgeDictionary.TryGetValue(badge.ID, out ChatBadgeModel b))
                {
                    return b;
                }
            }
            return null;
        }
    }
}
