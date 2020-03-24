using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Chat;
using Mixer.Base.Model.MixPlay;
using Mixer.Base.Model.User;
using MixItUp.Base.Model;
using MixItUp.Base.Model.MixPlay;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Twitch.Base.Models.Clients.Chat;
using Twitch.Base.Models.Clients.PubSub.Messages;
using Twitch.Base.Models.NewAPI.Users;
using TwitchV5API = Twitch.Base.Models.V5;
using TwitchNewAPI = Twitch.Base.Models.NewAPI;

namespace MixItUp.Base.ViewModel.User
{
    public enum UserRoleEnum
    {
        Banned,
        User = 10,
        Pro = 20,

        Affiliate = 23,

        Partner = 25,
        Follower = 30,
        Regular = 35,
        Subscriber = 40,
        GlobalMod = 48,
        Mod = 50,
        ChannelEditor = 55,
        Staff = 60,
        Streamer = 70,

        Custom = 99,
    }

    public enum AgeRatingEnum
    {
        Family,
        Teen,
        Adult,
    }

    public static class UserWithGroupsModelExtensions
    {
        public static DateTimeOffset? GetSubscriberDate(this UserWithGroupsModel userGroups)
        {
            return userGroups.GetCreatedDateForGroupIfCurrent(UserRoleEnum.Subscriber.ToString());
        }
    }

    public static class NewAPITwitchUserModelExtensions
    {
        public static bool IsAffiliate(this TwitchNewAPI.Users.UserModel twitchUser)
        {
            return twitchUser.broadcaster_type.Equals("affiliate");
        }

        public static bool IsPartner(this TwitchNewAPI.Users.UserModel twitchUser)
        {
            return twitchUser.broadcaster_type.Equals("partner");
        }

        public static bool IsStaff(this TwitchNewAPI.Users.UserModel twitchUser)
        {
            return twitchUser.type.Equals("staff") || twitchUser.type.Equals("admin");
        }

        public static bool IsGlobalMod(this TwitchNewAPI.Users.UserModel twitchUser)
        {
            return twitchUser.type.Equals("global_mod");
        }
    }

    public class UserViewModel : IEquatable<UserViewModel>, IComparable<UserViewModel>
    {
        public const string MixerUserDefaultAvatarLink = "https://mixer.com/_latest/assets/images/main/avatars/default.png";
        public const string MixerUserAvatarLinkFormat = "https://mixer.com/api/v1/users/{0}/avatar?w=128&h=128";

        private const int MinimumRefreshInterval = 5;

        public static IEnumerable<UserRoleEnum> SelectableBasicUserRoles()
        {
            List<UserRoleEnum> roles = new List<UserRoleEnum>(EnumHelper.GetEnumList<UserRoleEnum>());
            roles.Remove(UserRoleEnum.GlobalMod);
            roles.Remove(UserRoleEnum.Banned);
            roles.Remove(UserRoleEnum.Custom);
            return roles;
        }

        public static IEnumerable<UserRoleEnum> SelectableAdvancedUserRoles()
        {
            return UserViewModel.SelectableBasicUserRoles();
        }

        public UserDataModel Data { get; private set; }

        public Guid ID { get { return this.Data.ID; } }

        public string Username
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Mixer) { return this.Data.MixerUsername; }
                else if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return this.TwitchVisualName; }
                return this.unassociatedUsername;
            }
        }
        private string unassociatedUsername;

        public StreamingPlatformTypeEnum Platform
        {
            get
            {
                if (this.MixerID > 0 || this.InteractiveIDs.Count > 0) { return StreamingPlatformTypeEnum.Mixer; }
                else if (!string.IsNullOrEmpty(this.TwitchID)) { return StreamingPlatformTypeEnum.Twitch; }
                return StreamingPlatformTypeEnum.None;
            }
        }

        public HashSet<UserRoleEnum> UserRoles
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Mixer) { return this.MixerUserRoles; }
                else if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return this.TwitchUserRoles; }
                return new HashSet<UserRoleEnum>() { UserRoleEnum.User };
            }
        }

        public string AvatarLink
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Mixer) { return this.MixerAvatarLink; }
                else if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return this.TwitchAvatarLink; }
                return string.Empty;
            }
        }

        public DateTimeOffset? AccountDate
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Mixer) { return this.Data.MixerAccountDate; }
                else if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return this.Data.TwitchAccountDate; }
                return null;
            }
            set
            {
                if (this.Platform == StreamingPlatformTypeEnum.Mixer) { this.Data.MixerAccountDate = value; }
            }
        }

        public DateTimeOffset? FollowDate
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Mixer) { return this.Data.MixerFollowDate; }
                else if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return this.Data.TwitchFollowDate; }
                return null;
            }
            set
            {
                if (this.Platform == StreamingPlatformTypeEnum.Mixer) { this.Data.MixerFollowDate = value; }

                if (value == null || value.GetValueOrDefault() == DateTimeOffset.MinValue)
                {
                    this.UserRoles.Remove(UserRoleEnum.Follower);
                }
                else
                {
                    this.UserRoles.Add(UserRoleEnum.Follower);
                }
            }
        }

        public DateTimeOffset? SubscribeDate
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Mixer) { return this.Data.MixerSubscribeDate; }
                else if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return this.Data.TwitchSubscribeDate; }
                return null;
            }
            set
            {
                if (this.Platform == StreamingPlatformTypeEnum.Mixer) { this.Data.MixerSubscribeDate = value; }

                if (value == null || value.GetValueOrDefault() == DateTimeOffset.MinValue)
                {
                    this.UserRoles.Remove(UserRoleEnum.Subscriber);
                }
                else
                {
                    this.UserRoles.Add(UserRoleEnum.Subscriber);
                }
            }
        }

        public string SubscriberBadgeLink
        {
            get
            {
                if (this.IsPlatformSubscriber)
                {
                    if (this.Platform == StreamingPlatformTypeEnum.Mixer) { return (ChannelSession.MixerChannel.badge != null) ? ChannelSession.MixerChannel.badge.url : string.Empty; }
                }
                return null;
            }
        }

        [JsonIgnore]
        public string UserLink
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Mixer) { return string.Format("https://www.mixer.com/{0}", this.Username); }
                else if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return string.Format("https://www.twitch.tv/{0}", this.Username); }
                return null;
            }
        }

        [JsonIgnore]
        public bool IsAnonymous
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Mixer) { return this.MixerID == 0 || this.InteractiveIDs.Values.Any(i => i.anonymous.GetValueOrDefault()); }
                else if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return string.IsNullOrEmpty(this.TwitchID); }
                return false;
            }
        }

        #region Mixer

        public uint MixerID { get { return this.Data.MixerID; } private set { if (value > 0) { this.Data.MixerID = value; } } }
        public string MixerUsername { get { return this.Data.MixerUsername; } private set { if (!string.IsNullOrEmpty(value)) { this.Data.MixerUsername = value; } } }
        public uint MixerChannelID { get { return this.Data.MixerChannelID; } private set { if (value > 0) { this.Data.MixerChannelID = value; } } }

        public HashSet<UserRoleEnum> MixerUserRoles { get; set; } = new HashSet<UserRoleEnum>() { UserRoleEnum.User };

        public UserFanProgressionModel MixerFanProgression { get; set; }
        public int Sparks { get; set; }

        public uint CurrentViewerCount { get; set; }

        public LockedDictionary<string, MixPlayParticipantModel> InteractiveIDs { get; set; } = new LockedDictionary<string, MixPlayParticipantModel>();

        public string InteractiveGroupID { get; set; }

        public bool IsInInteractiveTimeout { get; set; }

        public string MixerAvatarLink { get { return string.Format(MixerUserAvatarLinkFormat, this.Data.MixerID); } }

        public string MixerChannelBadgeLink { get { return this.MixerFanProgression?.level?.SmallAssetURL?.ToString(); } }

        public bool HasMixerChannelBadgeLink { get { return !string.IsNullOrEmpty(this.MixerChannelBadgeLink); } }

        #endregion Mixer

        #region Twitch

        [DataMember]
        public string TwitchID { get { return this.Data.TwitchID; } private set { this.Data.TwitchID = value; } }
        [DataMember]
        public string TwitchUsername { get { return this.Data.TwitchUsername; } private set { this.Data.TwitchUsername = value; } }
        [DataMember]
        public string TwitchDisplayName { get { return this.Data.TwitchDisplayName; } private set { this.Data.TwitchDisplayName = value; } }
        [DataMember]
        public string TwitchAvatarLink { get { return this.Data.TwitchAvatarLink; } private set { this.Data.TwitchAvatarLink = value; } }

        [DataMember]
        public HashSet<UserRoleEnum> TwitchUserRoles { get; set; } = new HashSet<UserRoleEnum>();

        [JsonIgnore]
        public string TwitchVisualName { get { return (!string.IsNullOrEmpty(this.TwitchDisplayName)) ? this.TwitchDisplayName : this.TwitchUsername; } }

        #endregion Twitch

        public HashSet<string> CustomRoles { get; set; } = new HashSet<string>();

        public bool IgnoreForQueries { get; set; }

        public bool IsInChat { get; set; }

        public string TwitterURL { get; set; }

        public PatreonCampaignMember PatreonUser { get; set; }

        private bool hasBeenRefreshed { get; set; }

        public UserViewModel(string username)
            : this(mixerID: 0)
        {
            this.InteractiveIDs = new LockedDictionary<string, MixPlayParticipantModel>();
            this.MixerUsername = this.unassociatedUsername = username;
        }

        public UserViewModel(Mixer.Base.Model.User.UserModel user)
            : this(mixerID: user.id)
        {
            this.MixerID = user.id;
            this.MixerUsername = user.username;
            this.SetMixerUserDetails(user);
        }

        public UserViewModel(ChannelModel channel)
            : this(mixerID: channel.userId)
        {
            this.MixerID = channel.userId;
            this.MixerUsername = channel.token;
            this.MixerChannelID = channel.id;
        }

        public UserViewModel(ChatUserModel user)
            : this(mixerID: user.userId.GetValueOrDefault())
        {
            this.MixerID = user.userId.GetValueOrDefault();
            this.MixerUsername = user.userName;
            this.SetMixerRoles(user.userRoles);

            this.IsInChat = true;
        }

        public UserViewModel(ChatMessageEventModel messageEvent)
            : this(mixerID: messageEvent.user_id)
        {
            this.MixerID = messageEvent.user_id;
            this.MixerUsername = messageEvent.user_name;
            this.SetMixerRoles(messageEvent.user_roles);

            this.IsInChat = true;
        }

        public UserViewModel(ChatMessageUserModel chatUser)
            : this(mixerID: chatUser.user_id)
        {
            this.MixerID = chatUser.user_id;
            this.MixerUsername = chatUser.user_name;
            this.SetMixerRoles(chatUser.user_roles);

            this.IsInChat = true;
        }

        public UserViewModel(MixPlayParticipantModel participant)
            : this(mixerID: participant.userID)
        {
            this.MixerID = participant.userID;
            this.MixerUsername = participant.username;

            this.SetInteractiveDetails(participant);
        }

        public UserViewModel(TwitchNewAPI.Users.UserModel twitchUser)
            : this(twitchID: twitchUser.id)
        {
            this.TwitchID = twitchUser.id;
            this.TwitchUsername = twitchUser.login;
            this.TwitchDisplayName = (!string.IsNullOrEmpty(twitchUser.display_name)) ? twitchUser.display_name : this.TwitchUsername;
            this.TwitchAvatarLink = twitchUser.profile_image_url;

            this.SetTwitchRoles();
        }

        public UserViewModel(ChatMessagePacketModel twitchMessage)
            : this(twitchID: twitchMessage.UserID)
        {
            this.TwitchID = twitchMessage.UserID;
            this.TwitchUsername = twitchMessage.UserLogin;
            this.TwitchDisplayName = (!string.IsNullOrEmpty(twitchMessage.UserDisplayName)) ? twitchMessage.UserDisplayName : this.TwitchUsername;

            this.SetTwitchRoles();
        }

        public UserViewModel(PubSubWhisperEventModel whisper)
            : this(twitchID: whisper.from_id.ToString())
        {
            this.TwitchID = whisper.from_id.ToString();
            this.TwitchUsername = whisper.tags.login;
            this.TwitchDisplayName = (!string.IsNullOrEmpty(whisper.tags.display_name)) ? whisper.tags.display_name : this.TwitchUsername;

            this.SetTwitchRoles();
        }

        public UserViewModel(PubSubWhisperEventRecipientModel whisperRecipient)
            : this(twitchID: whisperRecipient.id.ToString())
        {
            this.TwitchID = whisperRecipient.id.ToString();
            this.TwitchUsername = whisperRecipient.username;
            this.TwitchDisplayName = (!string.IsNullOrEmpty(whisperRecipient.display_name)) ? whisperRecipient.display_name : this.TwitchUsername;
            this.TwitchAvatarLink = whisperRecipient.profile_image;

            this.SetTwitchRoles();
        }

        public UserViewModel(ChatRawPacketModel packet)
            : this(twitchID: packet.Tags["user-id"])
        {
            this.TwitchID = packet.Tags["user-id"];
            this.TwitchUsername = packet.Tags["login"];
            this.TwitchDisplayName = (packet.Tags.ContainsKey("display-name") && !string.IsNullOrEmpty(packet.Tags["display-name"])) ? packet.Tags["display-name"] : this.TwitchUsername;

            this.SetTwitchRoles();
        }

        public UserViewModel(PubSubBitsEventV2Model packet)
            : this(twitchID: packet.user_id)
        {
            this.TwitchID = packet.user_id;
            this.TwitchDisplayName = this.TwitchUsername = packet.user_name;

            this.SetTwitchRoles();
        }

        public UserViewModel(PubSubSubscriptionsEventModel packet)
            : this(twitchID: packet.user_id)
        {
            this.TwitchID = packet.user_id;
            this.TwitchUsername = packet.user_name;
            this.TwitchDisplayName = (!string.IsNullOrEmpty(packet.display_name)) ? packet.display_name : this.TwitchUsername;

            this.SetTwitchRoles();
        }

        public UserViewModel(UserFollowModel follow)
            : this(twitchID: follow.from_id)
        {
            this.TwitchID = follow.from_id;
            this.TwitchDisplayName = this.TwitchUsername = follow.from_name;

            this.SetTwitchRoles();
        }

        public UserViewModel(UserDataModel userData)
        {
            this.Data = userData;
        }

        private UserViewModel(uint mixerID = 0, string twitchID = null)
        {
            if (mixerID > 0)
            {
                this.Data = ChannelSession.Settings.GetUserDataByMixerID(mixerID);
            }
            else if (!string.IsNullOrEmpty(twitchID))
            {
                this.Data = ChannelSession.Settings.GetUserDataByTwitchID(twitchID);
            }
            else
            {
                this.Data = new UserDataModel();
            }
        }

        public DateTimeOffset LastActivity { get; set; }

        public string RolesDisplayString { get; private set; }

        public UserRoleEnum PrimaryRole { get { return this.UserRoles.Max(); } }

        public string PrimaryRoleString { get { return EnumLocalizationHelper.GetLocalizedName(this.PrimaryRole); } }

        public string SortableID
        {
            get
            {
                UserRoleEnum role = this.PrimaryRole;
                if (role < UserRoleEnum.Subscriber)
                {
                    role = UserRoleEnum.User;
                }
                return (999 - role) + "-" + this.Platform.ToString() + "-" + this.Username;
            }
        }

        public string Title
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Data.CustomTitle))
                {
                    return this.Data.CustomTitle;
                }

                UserTitleModel title = ChannelSession.Settings.UserTitles.OrderByDescending(t => t.Role).ThenByDescending(t => t.Months).FirstOrDefault(t => t.MeetsTitle(this));
                if (title != null)
                {
                    return title.Name;
                }

                return "No Title";
            }
            set
            {
                this.Data.CustomTitle = value;
            }
        }

        public string AccountAgeString { get { return (this.AccountDate != null) ? this.AccountDate.GetValueOrDefault().GetAge() : "Unknown"; } }

        public bool IsFollower { get { return this.UserRoles.Contains(UserRoleEnum.Follower) || this.HasPermissionsTo(UserRoleEnum.Subscriber); } }

        public string FollowAgeString { get { return (this.FollowDate != null) ? this.FollowDate.GetValueOrDefault().GetAge() : "Not Following"; } }

        public bool IsPlatformSubscriber { get { return this.UserRoles.Contains(UserRoleEnum.Subscriber); } }

        public bool ShowSubscriberBadge { get { return this.IsPlatformSubscriber && !string.IsNullOrEmpty(this.SubscriberBadgeLink); } }

        public string SubscribeAgeString { get { return (this.SubscribeDate != null) ? this.SubscribeDate.GetValueOrDefault().GetAge() : "Not Subscribed"; } }

        public int WhispererNumber { get; set; }

        public bool HasWhisperNumber { get { return this.WhispererNumber > 0; } }

        public int SubscribeMonths
        {
            get
            {
                if (this.SubscribeDate != null)
                {
                    return this.SubscribeDate.GetValueOrDefault().TotalMonthsFromNow();
                }
                return 0;
            }
        }

        public string PrimaryRoleColorName
        {
            get
            {
                switch (this.PrimaryRole)
                {
                    case UserRoleEnum.Streamer:
                        return "UserStreamerRoleColor";
                    case UserRoleEnum.Staff:
                        return "UserStaffRoleColor";
                    case UserRoleEnum.ChannelEditor:
                    case UserRoleEnum.Mod:
                        return "UserModRoleColor";
                    case UserRoleEnum.GlobalMod:
                        return "UserGlobalModRoleColor";
                }

                if (this.UserRoles.Contains(UserRoleEnum.Pro))
                {
                    return "UserProRoleColor";
                }
                else
                {
                    return "UserDefaultRoleColor";
                }
            }
        }

        public bool IsInteractiveParticipant { get { return this.InteractiveIDs.Count > 0; } }

        public PatreonTier PatreonTier
        {
            get
            {
                if (ChannelSession.Services.Patreon.IsConnected && this.PatreonUser != null)
                {
                    return ChannelSession.Services.Patreon.Campaign.GetTier(this.PatreonUser.TierID);
                }
                return null;
            }
        }

        public bool HasPermissionsTo(UserRoleEnum checkRole)
        {
            if (checkRole == UserRoleEnum.Subscriber && this.IsEquivalentToSubscriber())
            {
                return true;
            }
            return this.PrimaryRole >= checkRole;
        }

        public bool ExceedsPermissions(UserRoleEnum checkRole) { return this.PrimaryRole > checkRole; }

        public bool IsEquivalentToSubscriber()
        {
            if (this.PatreonUser != null && ChannelSession.Services.Patreon.IsConnected && !string.IsNullOrEmpty(ChannelSession.Settings.PatreonTierMixerSubscriberEquivalent))
            {
                PatreonTier userTier = this.PatreonTier;
                PatreonTier equivalentTier = ChannelSession.Services.Patreon.Campaign.GetTier(ChannelSession.Settings.PatreonTierMixerSubscriberEquivalent);
                if (userTier != null && equivalentTier != null && userTier.Amount >= equivalentTier.Amount)
                {
                    return true;
                }
            }
            return false;
        }

        public void UpdateLastActivity() { this.LastActivity = DateTimeOffset.Now; }

        public async Task RefreshDetails(bool force = false)
        {
            if (!this.IsAnonymous && (!this.hasBeenRefreshed || force))
            {
                this.hasBeenRefreshed = true;

                if (this.Platform == StreamingPlatformTypeEnum.Mixer)
                {
                    UserWithChannelModel user = await ChannelSession.MixerUserConnection.GetUser(this.MixerID);
                    if (user != null)
                    {
                        this.SetMixerUserDetails(user);

                        if (!this.IsInChat && !this.IsAnonymous)
                        {
                            ChatUserModel chatUser = await ChannelSession.MixerUserConnection.GetChatUser(ChannelSession.MixerChannel, this.MixerID);
                            if (chatUser != null)
                            {
                                this.SetChatDetails(chatUser);
                            }
                        }

                        this.FollowDate = await ChannelSession.MixerUserConnection.CheckIfFollows(ChannelSession.MixerChannel, this.GetMixerUserModel());

                        if (this.IsPlatformSubscriber)
                        {
                            UserWithGroupsModel userGroups = await ChannelSession.MixerUserConnection.GetUserInChannel(ChannelSession.MixerChannel, this.MixerID);
                            if (userGroups != null)
                            {
                                DateTimeOffset subDate = userGroups.GetSubscriberDate().GetValueOrDefault();
                                if (subDate > DateTimeOffset.MinValue)
                                {
                                    this.Data.MixerSubscribeDate = subDate;

                                    int totalMonths = this.Data.MixerSubscribeDate.GetValueOrDefault().TotalMonthsFromNow();
                                    if (this.Data.TotalMonthsSubbed < totalMonths)
                                    {
                                        this.Data.TotalMonthsSubbed = (uint)totalMonths;
                                    }
                                }
                            }
                        }

                        this.MixerFanProgression = await ChannelSession.MixerUserConnection.GetUserFanProgression(ChannelSession.MixerChannel, user);
                    }
                }
                else if (this.Platform == StreamingPlatformTypeEnum.Twitch)
                {
                    TwitchNewAPI.Users.UserModel twitchUser = (!string.IsNullOrEmpty(this.TwitchID)) ? await ChannelSession.TwitchUserConnection.GetNewAPIUserByID(this.TwitchID)
                        : await ChannelSession.TwitchUserConnection.GetNewAPIUserByLogin(this.TwitchUsername);
                    if (twitchUser != null)
                    {
                        this.TwitchID = twitchUser.id;
                        this.TwitchUsername = twitchUser.login;
                        this.TwitchDisplayName = (!string.IsNullOrEmpty(twitchUser.display_name)) ? twitchUser.display_name : this.TwitchDisplayName;
                        this.TwitchAvatarLink = twitchUser.profile_image_url;

                        TwitchV5API.Users.UserModel twitchV5User = await ChannelSession.TwitchUserConnection.GetV5APIUserByLogin(this.TwitchUsername);
                        if (twitchV5User != null && !string.IsNullOrEmpty(twitchV5User.created_at) && DateTimeOffset.TryParse(twitchV5User.created_at, out DateTimeOffset createdDate))
                        {
                            this.Data.TwitchAccountDate = createdDate;
                        }

                        if (twitchUser.IsPartner())
                        {
                            this.TwitchUserRoles.Add(UserRoleEnum.Partner);
                        }
                        else if (twitchUser.IsAffiliate())
                        {
                            this.TwitchUserRoles.Add(UserRoleEnum.Affiliate);
                        }

                        if (twitchUser.IsStaff())
                        {
                            this.TwitchUserRoles.Add(UserRoleEnum.Staff);
                        }
                        if (twitchUser.IsGlobalMod())
                        {
                            this.TwitchUserRoles.Add(UserRoleEnum.GlobalMod);
                        }

                        UserFollowModel follow = await ChannelSession.TwitchUserConnection.CheckIfFollowsNewAPI(ChannelSession.TwitchChannelNewAPI, twitchUser);
                        if (follow != null && !string.IsNullOrEmpty(follow.followed_at) && DateTimeOffset.TryParse(follow.followed_at, out DateTimeOffset followDate))
                        {
                            this.Data.TwitchFollowDate = followDate;
                            this.TwitchUserRoles.Add(UserRoleEnum.Follower);
                        }

                        if (ChannelSession.TwitchUserNewAPI.IsAffiliate() || ChannelSession.TwitchUserNewAPI.IsPartner())
                        {
                            if (twitchV5User != null)
                            {
                                TwitchV5API.Users.UserSubscriptionModel subscription = await ChannelSession.TwitchUserConnection.CheckIfSubscribedV5(ChannelSession.TwitchChannelV5, twitchV5User);
                                if (subscription != null && !string.IsNullOrEmpty(subscription.created_at) && DateTimeOffset.TryParse(subscription.created_at, out DateTimeOffset subDate))
                                {
                                    this.Data.TwitchSubscribeDate = subDate;
                                    this.TwitchUserRoles.Add(UserRoleEnum.Subscriber);
                                }
                            }
                        }

                        this.SetTwitchRoles();
                    }
                }

                await this.SetCustomRoles();

                this.Data.UpdateData(this);
            }
        }

        public Task SetCustomRoles()
        {
            if (!this.IsAnonymous)
            {
                this.CustomRoles.Clear();

                if (ChannelSession.Services.Patreon.IsConnected && this.PatreonUser == null)
                {
                    IEnumerable<PatreonCampaignMember> campaignMembers = ChannelSession.Services.Patreon.CampaignMembers;

                    PatreonCampaignMember patreonUser = null;
                    if (!string.IsNullOrEmpty(this.Data.PatreonUserID))
                    {
                        patreonUser = campaignMembers.FirstOrDefault(u => u.UserID.Equals(this.Data.PatreonUserID));
                    }
                    else
                    {
                        patreonUser = campaignMembers.FirstOrDefault(u => u.User.LookupName.Equals(this.Username, StringComparison.CurrentCultureIgnoreCase));
                    }

                    this.PatreonUser = patreonUser;
                    if (patreonUser != null)
                    {
                        this.Data.PatreonUserID = patreonUser.UserID;
                    }
                    else
                    {
                        this.Data.PatreonUserID = null;
                    }
                }
            }
            return Task.FromResult(0);
        }

        public void SetChatDetails(ChatUserModel chatUser)
        {
            if (chatUser != null)
            {
                this.SetMixerRoles(chatUser.userRoles);
                this.IsInChat = true;
            }
        }

        public void RemoveChatDetails(ChatUserModel chatUser)
        {
            this.IsInChat = false;
        }

        public void SetInteractiveDetails(MixPlayParticipantModel participant)
        {
            this.InteractiveIDs[participant.sessionID] = participant;
            this.InteractiveGroupID = participant.groupID;
        }

        public void RemoveInteractiveDetails(MixPlayParticipantModel participant)
        {
            this.InteractiveIDs.Remove(participant.sessionID);
            if (this.InteractiveIDs.Count == 0)
            {
                this.InteractiveGroupID = MixPlayUserGroupModel.DefaultName;
            }
        }

        public async Task AddModerationStrike(string moderationReason = null)
        {
            Dictionary<string, string> extraSpecialIdentifiers = new Dictionary<string, string>();
            extraSpecialIdentifiers.Add(ModerationHelper.ModerationReasonSpecialIdentifier, moderationReason);

            this.Data.ModerationStrikes++;
            if (this.Data.ModerationStrikes == 1)
            {
                if (ChannelSession.Settings.ModerationStrike1Command != null)
                {
                    await ChannelSession.Settings.ModerationStrike1Command.Perform(this, extraSpecialIdentifiers: extraSpecialIdentifiers);
                }
            }
            else if (this.Data.ModerationStrikes == 2)
            {
                if (ChannelSession.Settings.ModerationStrike2Command != null)
                {
                    await ChannelSession.Settings.ModerationStrike2Command.Perform(this, extraSpecialIdentifiers: extraSpecialIdentifiers);
                }
            }
            else if (this.Data.ModerationStrikes >= 3)
            {
                if (ChannelSession.Settings.ModerationStrike3Command != null)
                {
                    await ChannelSession.Settings.ModerationStrike3Command.Perform(this, extraSpecialIdentifiers: extraSpecialIdentifiers);
                }
            }
        }

        public Task RemoveModerationStrike()
        {
            if (this.Data.ModerationStrikes > 0)
            {
                this.Data.ModerationStrikes--;
            }
            return Task.FromResult(0);
        }

        public void UpdateMinuteData()
        {
            if (ChannelSession.MixerChannel.online)
            {
                this.Data.ViewingMinutes++;
            }
            else
            {
                this.Data.OfflineViewingMinutes++;
            }
            ChannelSession.Settings.UserData.ManualValueChanged(this.ID);

            if (ChannelSession.Settings.RegularUserMinimumHours > 0 && this.Data.ViewingHoursPart >= ChannelSession.Settings.RegularUserMinimumHours)
            {
                this.UserRoles.Add(UserRoleEnum.Regular);
            }
        }

        public Mixer.Base.Model.User.UserModel GetMixerUserModel()
        {
            return new Mixer.Base.Model.User.UserModel()
            {
                id = this.MixerID,
                username = this.MixerUsername,
            };
        }

        public ChatUserModel GetMixerChatModel()
        {
            return new ChatUserModel()
            {
                userId = this.MixerID,
                userName = this.MixerUsername,
                userRoles = this.UserRoles.Select(r => r.ToString()).ToArray(),
            };
        }

        public IEnumerable<MixPlayParticipantModel> GetMixerMixPlayParticipantModels()
        {
            List<MixPlayParticipantModel> participants = new List<MixPlayParticipantModel>();
            foreach (string interactiveID in this.InteractiveIDs.Keys)
            {
                participants.Add(new MixPlayParticipantModel()
                {
                    userID = this.MixerID,
                    username = this.MixerUsername,
                    sessionID = interactiveID,
                    groupID = this.InteractiveGroupID,
                    disabled = this.IsInInteractiveTimeout,
                });
            }
            return participants;
        }

        public TwitchV5API.Users.UserModel GetTwitchV5APIUserModel()
        {
            return new TwitchV5API.Users.UserModel()
            {
                id = this.TwitchID,
                name = this.TwitchUsername,
                display_name = this.TwitchDisplayName,
            };
        }

        public TwitchNewAPI.Users.UserModel GetTwitchNewAPIUserModel()
        {
            return new TwitchNewAPI.Users.UserModel()
            {
                id = this.TwitchID,
                login = this.TwitchUsername,
                display_name = this.TwitchDisplayName,
            };
        }

        public override bool Equals(object obj)
        {
            if (obj is UserViewModel)
            {
                return this.Equals((UserViewModel)obj);
            }
            return false;
        }

        public bool Equals(UserViewModel other) { return this.ID.Equals(other.ID); }

        public int CompareTo(UserViewModel other) { return this.Username.CompareTo(other.Username); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }

        public override string ToString() { return this.Username; }

        private void SetMixerUserDetails(Mixer.Base.Model.User.UserModel user)
        {
            if (user.createdAt.GetValueOrDefault() > DateTimeOffset.MinValue)
            {
                this.AccountDate = user.createdAt;
            }
            this.Sparks = (int)user.sparks;
            this.TwitterURL = user.social?.twitter;
            if (user is UserWithChannelModel)
            {
                UserWithChannelModel userChannel = (UserWithChannelModel)user;
                this.MixerChannelID = userChannel.channel.id;
                this.CurrentViewerCount = userChannel.channel.viewersCurrent;
            }
        }

        private void SetMixerRoles(string[] userRoles)
        {
            this.MixerUserRoles.Clear();
            this.MixerUserRoles.Add(UserRoleEnum.User);

            if (userRoles != null && userRoles.Length > 0)
            {
                if (userRoles.Any(r => r.Equals("Owner"))) { this.MixerUserRoles.Add(UserRoleEnum.Streamer); }
                if (userRoles.Any(r => r.Equals("Staff"))) { this.MixerUserRoles.Add(UserRoleEnum.Staff); }
                if (userRoles.Any(r => r.Equals("ChannelEditor"))) { this.MixerUserRoles.Add(UserRoleEnum.ChannelEditor); }
                if (userRoles.Any(r => r.Equals("Mod"))) { this.MixerUserRoles.Add(UserRoleEnum.Mod); }
                if (userRoles.Any(r => r.Equals("GlobalMod"))) { this.MixerUserRoles.Add(UserRoleEnum.GlobalMod); }
                if (userRoles.Any(r => r.Equals("Subscriber"))) { this.MixerUserRoles.Add(UserRoleEnum.Subscriber); }
                if (userRoles.Any(r => r.Equals("Partner"))) { this.MixerUserRoles.Add(UserRoleEnum.Partner); }
                if (userRoles.Any(r => r.Equals("Pro"))) { this.MixerUserRoles.Add(UserRoleEnum.Pro); }
                if (userRoles.Any(r => r.Equals("Banned"))) { this.MixerUserRoles.Add(UserRoleEnum.Banned); }
            }

            if (ChannelSession.MixerChannel != null && ChannelSession.MixerChannel.user.id.Equals(this.MixerID))
            {
                this.MixerUserRoles.Add(UserRoleEnum.Streamer);
            }

            this.SetGeneralRoles();
        }

        private void SetTwitchRoles()
        {
            this.TwitchUserRoles.Add(UserRoleEnum.User);
            if (ChannelSession.TwitchChannelNewAPI != null && ChannelSession.TwitchChannelNewAPI.id.Equals(this.TwitchID))
            {
                this.TwitchUserRoles.Add(UserRoleEnum.Streamer);
            }

            this.SetGeneralRoles();
        }

        private void SetGeneralRoles()
        {
            if (this.FollowDate != null && this.FollowDate.GetValueOrDefault() > DateTimeOffset.MinValue)
            {
                this.UserRoles.Add(UserRoleEnum.Follower);
            }

            if (this.UserRoles.Contains(UserRoleEnum.Streamer))
            {
                this.MixerUserRoles.Add(UserRoleEnum.ChannelEditor);
                this.MixerUserRoles.Add(UserRoleEnum.Mod);
                this.MixerUserRoles.Add(UserRoleEnum.Subscriber);
                this.MixerUserRoles.Add(UserRoleEnum.Follower);
            }

            if (this.UserRoles.Contains(UserRoleEnum.ChannelEditor))
            {
                this.UserRoles.Add(UserRoleEnum.Mod);
            }

            if (ChannelSession.Settings.RegularUserMinimumHours > 0 && this.Data.ViewingHoursPart >= ChannelSession.Settings.RegularUserMinimumHours)
            {
                this.UserRoles.Add(UserRoleEnum.Regular);
            }

            HashSet<UserRoleEnum> displayRoles = new HashSet<UserRoleEnum>(this.UserRoles.ToList());
            if (this.UserRoles.Contains(UserRoleEnum.Banned))
            {
                displayRoles.Clear();
                displayRoles.Add(UserRoleEnum.Banned);
            }
            else
            {
                if (displayRoles.Count() > 1)
                {
                    displayRoles.Remove(UserRoleEnum.User);
                }

                if (displayRoles.Contains(UserRoleEnum.ChannelEditor))
                {
                    displayRoles.Remove(UserRoleEnum.Mod);
                }

                if (displayRoles.Contains(UserRoleEnum.Subscriber) || displayRoles.Contains(UserRoleEnum.Streamer))
                {
                    displayRoles.Remove(UserRoleEnum.Follower);
                }

                if (displayRoles.Contains(UserRoleEnum.Streamer))
                {
                    displayRoles.Remove(UserRoleEnum.ChannelEditor);
                    displayRoles.Remove(UserRoleEnum.Subscriber);
                }
            }

            List<string> displayRolesList = new List<string>(displayRoles.Select(r => EnumLocalizationHelper.GetLocalizedName(r)));
            displayRolesList.AddRange(this.CustomRoles);

            this.RolesDisplayString = string.Join(", ", displayRolesList.OrderByDescending(r => r));
        }
    }

    [DataContract]
    public class UserCurrencyDataViewModel : ViewModelBase, IEquatable<UserCurrencyDataViewModel>
    {
        [JsonIgnore]
        public UserDataModel User { get; set; }

        [JsonIgnore]
        public UserCurrencyModel Currency { get; set; }

        [DataMember]
        public int Amount
        {
            get { return this.Currency.GetAmount(this.User); }
            set
            {
                this.Currency.SetAmount(this.User, value);
                this.NotifyPropertyChanged();
            }
        }

        public UserCurrencyDataViewModel() { }

        public UserCurrencyDataViewModel(UserDataModel user, UserCurrencyModel currency)
        {
            this.User = user;
            this.Currency = currency;
        }

        public UserRankViewModel GetRank() { return this.Currency.GetRankForPoints(this.Amount); }

        public UserRankViewModel GetNextRank() { return this.Currency.GetNextRankForPoints(this.Amount); }

        public override bool Equals(object obj)
        {
            if (obj is UserCurrencyDataViewModel)
            {
                return this.Equals((UserCurrencyDataViewModel)obj);
            }
            return false;
        }

        public bool Equals(UserCurrencyDataViewModel other)
        {
            return this.User.Equals(other.User) && this.Currency.Equals(other.Currency);
        }

        public override int GetHashCode()
        {
            return this.User.GetHashCode() + this.Currency.GetHashCode();
        }

        public override string ToString()
        {
            UserRankViewModel rank = this.Currency.GetRankForPoints(this.Amount);
            return string.Format("{0} - {1}", rank.Name, this.Amount);
        }
    }

    [DataContract]
    public class UserInventoryDataViewModel : ViewModelBase, IEquatable<UserInventoryDataViewModel>
    {
        [JsonIgnore]
        public UserDataModel User { get; set; }

        [JsonIgnore]
        public UserInventoryModel Inventory { get; set; }

        public UserInventoryDataViewModel() { }

        public UserInventoryDataViewModel(UserDataModel user, UserInventoryModel inventory)
        {
            this.User = user;
            this.Inventory = inventory;
        }

        public int GetAmount(UserInventoryItemModel item) { return this.GetAmount(item.Name); }

        public int GetAmount(string itemName) { return this.Inventory.GetAmount(this.User, itemName); }

        public Dictionary<Guid, int> GetAmounts() { return this.Inventory.GetAmounts(this.User); }

        public void SetAmount(UserInventoryItemModel item, int amount) { this.SetAmount(item.Name, amount); }

        public void SetAmount(string itemName, int amount) { this.Inventory.SetAmount(this.User, itemName, amount); }

        public override bool Equals(object obj)
        {
            if (obj is UserInventoryDataViewModel)
            {
                return this.Equals((UserInventoryDataViewModel)obj);
            }
            return false;
        }

        public bool Equals(UserInventoryDataViewModel other)
        {
            return this.User.Equals(other.User) && this.Inventory.Equals(other.Inventory);
        }

        public override int GetHashCode()
        {
            return this.Inventory.GetHashCode();
        }
    }
}