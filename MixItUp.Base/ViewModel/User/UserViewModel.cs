using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twitch.Base.Models.Clients.Chat;
using Twitch.Base.Models.NewAPI.Chat;
using Twitch.Base.Models.NewAPI.Users;
using TwitchNewAPI = Twitch.Base.Models.NewAPI;
using TwitchV5API = Twitch.Base.Models.V5;

namespace MixItUp.Base.ViewModel.User
{
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
        public const string UserDefaultColor = "MaterialDesignBody";

        public UserDataModel Data { get; private set; }

        public UserViewModel(string username)
        {
            this.SetUserData(StreamingPlatformTypeEnum.None, null);

            this.UnassociatedUsername = username;
        }

        public UserViewModel(TwitchNewAPI.Users.UserModel twitchUser)
        {
            this.SetUserData(StreamingPlatformTypeEnum.Twitch, twitchUser.id);

            this.TwitchID = twitchUser.id;
            this.TwitchUsername = twitchUser.login;
            this.TwitchDisplayName = (!string.IsNullOrEmpty(twitchUser.display_name)) ? twitchUser.display_name : this.TwitchUsername;
            this.TwitchAvatarLink = twitchUser.profile_image_url;

            this.SetTwitchRoles();
        }

        public UserViewModel(TwitchV5API.Users.UserModel twitchUser)
        {
            this.SetUserData(StreamingPlatformTypeEnum.Twitch, twitchUser.id);

            this.TwitchID = twitchUser.id;
            this.TwitchUsername = twitchUser.name;
            this.TwitchDisplayName = (!string.IsNullOrEmpty(twitchUser.display_name)) ? twitchUser.display_name : this.TwitchUsername;
            this.TwitchAvatarLink = twitchUser.logo;

            this.SetTwitchRoles();
        }

        public UserViewModel(Twitch.Base.Models.Clients.Chat.ChatMessagePacketModel twitchMessage)
        {
            this.SetUserData(StreamingPlatformTypeEnum.Twitch, twitchMessage.UserID);

            this.TwitchID = twitchMessage.UserID;
            this.TwitchUsername = twitchMessage.UserLogin;
            this.TwitchDisplayName = (!string.IsNullOrEmpty(twitchMessage.UserDisplayName)) ? twitchMessage.UserDisplayName : this.TwitchUsername;

            this.SetTwitchRoles();
        }

        public UserViewModel(Twitch.Base.Models.Clients.PubSub.Messages.PubSubWhisperEventModel whisper)
        {
            this.SetUserData(StreamingPlatformTypeEnum.Twitch, whisper.from_id.ToString());

            this.TwitchID = whisper.from_id.ToString();
            this.TwitchUsername = whisper.tags.login;
            this.TwitchDisplayName = (!string.IsNullOrEmpty(whisper.tags.display_name)) ? whisper.tags.display_name : this.TwitchUsername;

            this.SetTwitchRoles();
        }

        public UserViewModel(Twitch.Base.Models.Clients.PubSub.Messages.PubSubWhisperEventRecipientModel whisperRecipient)
        {
            this.SetUserData(StreamingPlatformTypeEnum.Twitch, whisperRecipient.id.ToString());

            this.TwitchID = whisperRecipient.id.ToString();
            this.TwitchUsername = whisperRecipient.username;
            this.TwitchDisplayName = (!string.IsNullOrEmpty(whisperRecipient.display_name)) ? whisperRecipient.display_name : this.TwitchUsername;
            this.TwitchAvatarLink = whisperRecipient.profile_image;

            this.SetTwitchRoles();
        }

        public UserViewModel(Twitch.Base.Models.Clients.Chat.ChatUserNoticePacketModel packet)
        {
            this.SetUserData(StreamingPlatformTypeEnum.Twitch, packet.UserID.ToString());

            this.TwitchID = packet.UserID.ToString();
            this.TwitchUsername = !string.IsNullOrEmpty(packet.RaidUserLogin) ? packet.RaidUserLogin : packet.Login;
            this.TwitchDisplayName = !string.IsNullOrEmpty(packet.RaidUserDisplayName) ? packet.RaidUserDisplayName : packet.DisplayName;
            if (string.IsNullOrEmpty(this.TwitchDisplayName))
            {
                this.TwitchDisplayName = this.TwitchUsername;
            }

            this.SetTwitchRoles();
        }

        public UserViewModel(Twitch.Base.Models.Clients.Chat.ChatClearChatPacketModel packet)
        {
            this.SetUserData(StreamingPlatformTypeEnum.Twitch, packet.UserID);

            this.TwitchID = packet.UserID;
            this.TwitchUsername = packet.UserLogin;

            this.SetTwitchRoles();
        }

        public UserViewModel(Twitch.Base.Models.Clients.PubSub.Messages.PubSubBitsEventV2Model packet)
        {
            this.SetUserData(StreamingPlatformTypeEnum.Twitch, packet.user_id);

            this.TwitchID = packet.user_id;
            this.TwitchDisplayName = this.TwitchUsername = packet.user_name;

            this.SetTwitchRoles();
        }

        public UserViewModel(Twitch.Base.Models.Clients.PubSub.Messages.PubSubSubscriptionsEventModel packet)
        {
            this.SetUserData(StreamingPlatformTypeEnum.Twitch, packet.user_id);

            this.TwitchID = packet.user_id;
            this.TwitchUsername = packet.user_name;
            this.TwitchDisplayName = (!string.IsNullOrEmpty(packet.display_name)) ? packet.display_name : this.TwitchUsername;

            this.SetTwitchRoles();
        }

        public UserViewModel(Twitch.Base.Models.NewAPI.Users.UserFollowModel follow)
        {
            this.SetUserData(StreamingPlatformTypeEnum.Twitch, follow.from_id);

            this.TwitchID = follow.from_id;
            this.TwitchDisplayName = this.TwitchUsername = follow.from_name;

            this.SetTwitchRoles();
        }

        public UserViewModel(Glimesh.Base.Models.Users.UserModel user)
        {
            this.SetUserData(StreamingPlatformTypeEnum.Glimesh, user.id);

            this.GlimeshID = user.id;
            this.GlimeshUsername = user.username;
            this.GlimeshDisplayName = user.displayname;
            this.GlimeshAvatarLink = user.FullAvatarURL;
            this.AccountDate = StreamingClient.Base.Util.DateTimeOffsetExtensions.FromUTCISO8601String(user.confirmedAt);
        }

        public UserViewModel(Glimesh.Base.Models.Clients.Chat.ChatMessagePacketModel message) : this(message.User) { }

        public UserViewModel(UserDataModel userData)
        {
            this.Data = userData;
        }

        [Obsolete]
        public UserViewModel() { }

        private void SetUserData(StreamingPlatformTypeEnum platform, string userID)
        {
            if (platform != StreamingPlatformTypeEnum.None && !string.IsNullOrEmpty(userID))
            {
                this.Data = ChannelSession.Settings.GetUserDataByPlatformID(platform, userID);
                if (this.Data == null)
                {
                    if (platform == StreamingPlatformTypeEnum.Twitch)
                    {
                        this.Data = new UserDataModel() { TwitchID = userID };
                    }
                    else if (platform == StreamingPlatformTypeEnum.Glimesh)
                    {
                        this.Data = new UserDataModel() { GlimeshID = userID };
                    }
                    ChannelSession.Settings.AddUserData(this.Data);
                }
                return;
            }
            this.Data = new UserDataModel();
        }

        [JsonIgnore]
        public Guid ID { get { return this.Data.ID; } }

        [JsonIgnore]
        public StreamingPlatformTypeEnum Platform
        {
            get
            {
                if (!string.IsNullOrEmpty(this.TwitchID)) { return StreamingPlatformTypeEnum.Twitch; }
                else if (!string.IsNullOrEmpty(this.GlimeshID)) { return StreamingPlatformTypeEnum.Glimesh; }
                return StreamingPlatformTypeEnum.None;
            }
        }

        [JsonIgnore]
        public string PlatformID
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return this.TwitchID; }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return this.GlimeshID; }
                return null;
            }
        }

        [JsonIgnore]
        public string Username
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return this.TwitchUsername; }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return this.GlimeshUsername; }
                return this.UnassociatedUsername;
            }
        }

        [JsonIgnore]
        public string DisplayName
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return this.TwitchDisplayName ?? this.TwitchUsername; }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return this.GlimeshDisplayName ?? this.GlimeshUsername; }
                return this.UnassociatedUsername;
            }
        }

        [JsonIgnore]
        public HashSet<UserRoleEnum> UserRoles { get { return this.Data.UserRoles; } }

        [JsonIgnore]
        public string AvatarLink
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return this.TwitchAvatarLink; }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return this.GlimeshAvatarLink; }
                return string.Empty;
            }
        }
        [JsonIgnore]
        public bool ShowUserAvatar { get { return !ChannelSession.Settings.HideUserAvatar; } }

        public string Color
        {
            get
            {
                lock (colorLock)
                {
                    if (!string.IsNullOrEmpty(this.Data.Color))
                    {
                        return this.Data.Color;
                    }

                    if (ChannelSession.Settings.UseCustomUsernameColors)
                    {
                        foreach (UserRoleEnum role in this.UserRoles.OrderByDescending(r => r))
                        {
                            if (ChannelSession.Settings.CustomUsernameColors.ContainsKey(role))
                            {
                                string name = ChannelSession.Settings.CustomUsernameColors[role];
                                if (ColorSchemes.HTMLColorSchemeDictionary.ContainsKey(name))
                                {
                                    this.Data.Color = ColorSchemes.HTMLColorSchemeDictionary[name];
                                    break;
                                }
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(this.Data.Color))
                    {
                        if (this.Platform == StreamingPlatformTypeEnum.Twitch)
                        {
                            this.Data.Color = this.Data.TwitchColor;
                        }
                    }

                    if (string.IsNullOrEmpty(this.Data.Color))
                    {
                        this.Data.Color = UserViewModel.UserDefaultColor;
                    }

                    return this.Data.Color;
                }
            }
            private set
            {
                lock (colorLock)
                {
                    this.Data.Color = string.Empty;
                }
            }
        }
        private object colorLock = new object();

        [JsonIgnore]
        public string ChannelLink
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return $"https://www.twitch.tv/{this.Username}"; }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return $"https://www.glimesh.tv/{this.Username}"; }
                return string.Empty;
            }
        }

        public DateTimeOffset? AccountDate
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return this.Data.TwitchAccountDate; }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return this.Data.GlimeshAccountDate; }
                return null;
            }
            set
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { this.Data.TwitchAccountDate = value; }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { this.Data.GlimeshAccountDate = value; }
            }
        }

        public DateTimeOffset? FollowDate
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return this.Data.TwitchFollowDate; }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return this.Data.GlimeshFollowDate; }
                return null;
            }
            set
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { this.Data.TwitchFollowDate = value; }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { this.Data.GlimeshFollowDate = value; }

                if (this.FollowDate == null || this.FollowDate.GetValueOrDefault() == DateTimeOffset.MinValue)
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
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return this.Data.TwitchSubscribeDate; }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return this.Data.GlimeshSubscribeDate; }
                return null;
            }
            set
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { this.Data.TwitchSubscribeDate = value; }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { this.Data.GlimeshSubscribeDate = value; }

                if (this.SubscribeDate == null || this.SubscribeDate.GetValueOrDefault() == DateTimeOffset.MinValue)
                {
                    this.UserRoles.Remove(UserRoleEnum.Subscriber);
                }
                else
                {
                    this.UserRoles.Add(UserRoleEnum.Subscriber);
                }
            }
        }

        [JsonIgnore]
        public int SubscribeTier
        {
            get
            {
                if (this.IsPlatformSubscriber)
                {
                    if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return this.Data.TwitchSubscriberTier; }
                    else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return 1; }
                }
                return 0;
            }
        }

        [JsonIgnore]
        public string SubscribeTierString
        {
            get
            {
                int tier = this.SubscribeTier;
                return (tier > 0) ? $"{MixItUp.Base.Resources.Tier} {tier}" : MixItUp.Base.Resources.NotSubscribed;
            }
        }

        [JsonIgnore]
        public string PlatformBadgeLink
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return "/Assets/Images/Twitch-Small.png"; }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return "/Assets/Images/Glimesh.png"; }
                return null;
            }
        }

        [JsonIgnore]
        public string SubscriberBadgeLink
        {
            get
            {
                if (this.IsPlatformSubscriber)
                {
                    if (this.Platform == StreamingPlatformTypeEnum.Twitch && this.TwitchSubscriberBadge != null) { return this.TwitchSubscriberBadge.image_url_1x; }
                }
                return null;
            }
        }

        [JsonIgnore]
        public bool IsAnonymous
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return string.IsNullOrEmpty(this.TwitchID); }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return string.IsNullOrEmpty(this.GlimeshID); }
                return true;
            }
        }

        public DateTimeOffset LastSeen { get { return this.Data.LastSeen; } set { this.Data.LastSeen = value; } }

        public string UnassociatedUsername { get { return this.Data.UnassociatedUsername; } private set { this.Data.UnassociatedUsername = value; } }

        #region Twitch

        public string TwitchID { get { return this.Data.TwitchID; } private set { this.Data.TwitchID = value; } }
        public string TwitchUsername { get { return this.Data.TwitchUsername; } private set { this.Data.TwitchUsername = value; } }
        public string TwitchDisplayName { get { return this.Data.TwitchDisplayName; } private set { this.Data.TwitchDisplayName = value; } }
        public string TwitchAvatarLink { get { return this.Data.TwitchAvatarLink; } private set { this.Data.TwitchAvatarLink = value; } }

        public HashSet<UserRoleEnum> TwitchUserRoles { get { return this.Data.TwitchUserRoles; } private set { this.Data.TwitchUserRoles = value; } }

        public int TwitchSubMonths
        {
            get
            {
                if (this.Data.TwitchBadgeInfo != null && this.Data.TwitchBadgeInfo.TryGetValue("subscriber", out int months))
                {
                    return months;
                }
                return 0;
            }
        }

        public bool IsTwitchSubscriber { get { return this.HasTwitchSubscriberBadge || this.HasTwitchSubscriberFounderBadge; } }

        public bool HasTwitchSubscriberBadge { get { return this.HasTwitchBadge("subscriber"); } }

        public bool HasTwitchSubscriberFounderBadge { get { return this.HasTwitchBadge("founder"); } }

        public ChatBadgeModel TwitchSubscriberBadge { get; private set; }

        public bool HasTwitchRoleBadge { get { return this.TwitchRoleBadge != null && !ChannelSession.Settings.HideUserRoleBadge; } }

        public string TwitchRoleBadgeLink { get { return (this.HasTwitchRoleBadge) ? this.TwitchRoleBadge.image_url_1x : string.Empty; } }

        public ChatBadgeModel TwitchRoleBadge { get; private set; }

        public bool HasTwitchSpecialtyBadge { get { return this.TwitchSpecialtyBadge != null && !ChannelSession.Settings.HideUserSpecialtyBadge; } }

        public string TwitchSpecialtyBadgeLink { get { return (this.HasTwitchSpecialtyBadge) ? this.TwitchSpecialtyBadge.image_url_1x : string.Empty; } }

        public ChatBadgeModel TwitchSpecialtyBadge { get; private set; }

        #endregion Twitch

        #region Glimesh

        public string GlimeshID { get { return this.Data.GlimeshID; } private set { this.Data.GlimeshID = value; } }
        public string GlimeshUsername { get { return this.Data.GlimeshUsername; } private set { this.Data.GlimeshUsername = value; } }
        public string GlimeshDisplayName { get { return this.Data.GlimeshDisplayName; } private set { this.Data.GlimeshDisplayName = value; } }
        public string GlimeshAvatarLink { get { return this.Data.GlimeshAvatarLink; } private set { this.Data.GlimeshAvatarLink = value; } }

        public HashSet<UserRoleEnum> GlimeshUserRoles { get { return this.Data.GlimeshUserRoles; } private set { this.Data.GlimeshUserRoles = value; } }

        #endregion Glimesh

        public DateTimeOffset LastUpdated { get { return this.Data.LastUpdated; } set { this.Data.LastUpdated = value; } }

        public DateTimeOffset LastActivity { get { return this.Data.LastActivity; } set { this.Data.LastActivity = value; } }

        public HashSet<string> CustomRoles { get { return this.Data.CustomRoles; } set { this.Data.CustomRoles = value; } }

        public bool IgnoreForQueries { get { return this.Data.IgnoreForQueries; } set { this.Data.IgnoreForQueries = value; } }

        public bool IsInChat { get { return this.Data.IsInChat; } set { this.Data.IsInChat = value; } }

        public string TwitterURL { get { return this.Data.TwitterURL; } set { this.Data.TwitterURL = value; } }

        public PatreonCampaignMember PatreonUser { get { return this.Data.PatreonUser; } set { this.Data.PatreonUser = value; } }

        public UserRoleEnum PrimaryRole { get { return (this.UserRoles.Count() > 0) ? this.UserRoles.ToList().Max() : UserRoleEnum.User; } }

        public string PrimaryRoleString { get { return EnumLocalizationHelper.GetLocalizedName(this.PrimaryRole); } }

        [JsonIgnore]
        public string SortableID
        {
            get
            {
                UserRoleEnum role = this.PrimaryRole;
                if (role < UserRoleEnum.Subscriber)
                {
                    role = UserRoleEnum.User;
                }
                return (999 - role) + "-" + this.Username + "-" + this.Platform.ToString();
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

        public string RolesDisplayString
        {
            get
            {
                lock (this.rolesDisplayStringLock)
                {
                    if (this.Data.RolesDisplayString == null)
                    {
                        List<UserRoleEnum> userRoles = this.UserRoles.ToList();
                        if (this.Data.UserRoles.Contains(UserRoleEnum.Banned))
                        {
                            userRoles.Clear();
                            userRoles.Add(UserRoleEnum.Banned);
                        }
                        else
                        {
                            if (this.Data.UserRoles.Count() > 1)
                            {
                                userRoles.Remove(UserRoleEnum.User);
                            }

                            if (userRoles.Contains(UserRoleEnum.ChannelEditor))
                            {
                                userRoles.Remove(UserRoleEnum.Mod);
                            }

                            if (this.Data.UserRoles.Contains(UserRoleEnum.Subscriber) || this.Data.UserRoles.Contains(UserRoleEnum.Streamer))
                            {
                                userRoles.Remove(UserRoleEnum.Follower);
                            }

                            if (this.Data.UserRoles.Contains(UserRoleEnum.Streamer))
                            {
                                userRoles.Remove(UserRoleEnum.ChannelEditor);
                                userRoles.Remove(UserRoleEnum.Subscriber);
                            }
                        }

                        List<string> displayRoles = userRoles.Select(r => EnumLocalizationHelper.GetLocalizedName(r)).ToList();
                        displayRoles.AddRange(this.CustomRoles);

                        this.Data.RolesDisplayString = string.Join(", ", userRoles.OrderByDescending(r => r));
                    }
                    return this.Data.RolesDisplayString;
                }
            }
            private set
            {
                lock (this.rolesDisplayStringLock)
                {
                    this.Data.RolesDisplayString = value;
                }
            }
        }
        private object rolesDisplayStringLock = new object();

        [JsonIgnore]
        public bool IsFollower { get { return this.UserRoles.Contains(UserRoleEnum.Follower) || this.HasPermissionsTo(UserRoleEnum.Subscriber); } }
        [JsonIgnore]
        public bool IsRegular { get { return this.UserRoles.Contains(UserRoleEnum.Regular); } }
        [JsonIgnore]
        public bool IsPlatformSubscriber { get { return this.UserRoles.Contains(UserRoleEnum.Subscriber); } }
        [JsonIgnore]
        public bool ShowSubscriberBadge { get { return !ChannelSession.Settings.HideUserSubscriberBadge && this.IsPlatformSubscriber && !string.IsNullOrEmpty(this.SubscriberBadgeLink); } }

        [JsonIgnore]
        public string AccountAgeString { get { return (this.AccountDate != null) ? this.AccountDate.GetValueOrDefault().GetAge() : "Unknown"; } }
        [JsonIgnore]
        public string FollowAgeString { get { return (this.FollowDate != null) ? this.FollowDate.GetValueOrDefault().GetAge() : "Not Following"; } }
        [JsonIgnore]
        public string SubscribeAgeString { get { return (this.SubscribeDate != null) ? this.SubscribeDate.GetValueOrDefault().GetAge() : "Not Subscribed"; } }
        [JsonIgnore]
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
        [JsonIgnore]
        public string LastSeenString { get { return (this.LastSeen != DateTimeOffset.MinValue) ? this.LastSeen.ToFriendlyDateTimeString() : "Unknown"; } }

        public int WhispererNumber { get { return this.Data.WhispererNumber; } set { this.Data.WhispererNumber = value; } }
        [JsonIgnore]
        public bool HasWhisperNumber { get { return this.WhispererNumber > 0; } }

        [JsonIgnore]
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

        [JsonIgnore]
        public bool IsSubscriber { get { return this.UserRoles.Contains(UserRoleEnum.Subscriber) || this.IsEquivalentToSubscriber(); } }

        public bool HasPermissionsTo(UserRoleEnum checkRole)
        {
            Logger.Log($"Checking role permission for user: {this.PrimaryRole} - {checkRole}");

            if (checkRole == UserRoleEnum.Subscriber && this.IsEquivalentToSubscriber())
            {
                return true;
            }

            if (ChannelSession.Settings.ExplicitUserRoleRequirements)
            {
                return this.UserRoles.Contains(checkRole);
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
            if (!this.IsAnonymous)
            {
                if (!this.Data.UpdatedThisSession || force)
                {
                    DateTimeOffset refreshStart = DateTimeOffset.Now;

                    this.Data.UpdatedThisSession = true;
                    this.LastUpdated = DateTimeOffset.Now;

                    if (this.Platform.HasFlag(StreamingPlatformTypeEnum.Twitch))
                    {
                        await this.RefreshTwitchUserDetails();
                        await this.RefreshTwitchUserAccountDate();
                        await this.RefreshTwitchUserFollowDate();
                        await this.RefreshTwitchUserSubscribeDate();
                    }
                    
                    if (this.Platform.HasFlag(StreamingPlatformTypeEnum.Glimesh))
                    {
                        await this.RefreshGlimeshUserDetails();
                    }

                    this.SetCommonUserRoles();

                    await this.RefreshExternalServiceDetails();

                    double refreshTime = (DateTimeOffset.Now - refreshStart).TotalMilliseconds;
                    Logger.Log($"User refresh time: {refreshTime} ms");
                    if (refreshTime > 500)
                    {
                        Logger.Log(LogLevel.Error, string.Format("Long user refresh time detected for the following user: {0} - {1} - {2} ms", this.ID, this.Username, refreshTime));
                    }
                }
            }
        }

        #region Twitch Data Setter Functions

        public void SetTwitchChatDetails(ChatMessagePacketModel message)
        {
            this.SetTwitchChatDetails(message.UserDisplayName, message.BadgeDictionary, message.BadgeInfoDictionary, message.Color);
        }

        public void SetTwitchChatDetails(ChatUserStatePacketModel userState)
        {
            this.SetTwitchChatDetails(userState.UserDisplayName, userState.BadgeDictionary, userState.BadgeInfoDictionary, userState.Color);
        }

        public void SetTwitchChatDetails(ChatUserNoticePacketModel userNotice)
        {
            this.SetTwitchChatDetails(userNotice.UserDisplayName, userNotice.BadgeDictionary, userNotice.BadgeInfoDictionary, userNotice.Color);
        }

        private void SetTwitchChatDetails(string displayName, Dictionary<string, int> badges, Dictionary<string, int> badgeInfo, string color)
        {
            this.TwitchDisplayName = displayName;
            this.Data.TwitchBadges = badges;
            this.Data.TwitchBadgeInfo = badgeInfo;
            if (!string.IsNullOrEmpty(color))
            {
                this.Data.TwitchColor = color;
            }

            if (this.Data.TwitchBadges != null)
            {
                if (this.HasTwitchBadge("admin") || this.HasTwitchBadge("staff")) { this.TwitchUserRoles.Add(UserRoleEnum.Staff); } else { this.TwitchUserRoles.Remove(UserRoleEnum.Staff); }
                if (this.HasTwitchBadge("global_mod")) { this.TwitchUserRoles.Add(UserRoleEnum.GlobalMod); } else { this.TwitchUserRoles.Remove(UserRoleEnum.GlobalMod); }
                if (this.HasTwitchBadge("moderator")) { this.TwitchUserRoles.Add(UserRoleEnum.Mod); } else { this.TwitchUserRoles.Remove(UserRoleEnum.Mod); }
                if (this.IsTwitchSubscriber) { this.TwitchUserRoles.Add(UserRoleEnum.Subscriber); } else { this.TwitchUserRoles.Remove(UserRoleEnum.Subscriber); }
                if (this.HasTwitchBadge("turbo") || this.HasTwitchBadge("premium")) { this.TwitchUserRoles.Add(UserRoleEnum.Premium); } else { this.TwitchUserRoles.Remove(UserRoleEnum.Premium); }
                if (this.HasTwitchBadge("vip")) { this.TwitchUserRoles.Add(UserRoleEnum.VIP); } else { this.TwitchUserRoles.Remove(UserRoleEnum.VIP); }

                if (ChannelSession.Services.Chat.TwitchChatService != null)
                {
                    if (this.HasTwitchBadge("staff")) { this.TwitchRoleBadge = this.GetTwitchBadgeURL("staff"); }
                    else if (this.HasTwitchBadge("admin")) { this.TwitchRoleBadge = this.GetTwitchBadgeURL("admin"); }
                    else if (this.HasTwitchBadge("extension")) { this.TwitchRoleBadge = this.GetTwitchBadgeURL("extension"); }
                    else if (this.HasTwitchBadge("twitchbot")) { this.TwitchRoleBadge = this.GetTwitchBadgeURL("twitchbot"); }
                    else if (this.TwitchUserRoles.Contains(UserRoleEnum.Mod)) { this.TwitchRoleBadge = this.GetTwitchBadgeURL("moderator"); }
                    else if (this.TwitchUserRoles.Contains(UserRoleEnum.VIP)) { this.TwitchRoleBadge = this.GetTwitchBadgeURL("vip"); }

                    if (this.HasTwitchSubscriberFounderBadge) { this.TwitchSubscriberBadge = this.GetTwitchBadgeURL("founder"); }
                    else if (this.HasTwitchSubscriberBadge) { this.TwitchSubscriberBadge = this.GetTwitchBadgeURL("subscriber"); }                   

                    if (this.HasTwitchBadge("sub-gift-leader")) { this.TwitchSpecialtyBadge = this.GetTwitchBadgeURL("sub-gift-leader"); }
                    else if (this.HasTwitchBadge("bits-leader")) { this.TwitchSpecialtyBadge = this.GetTwitchBadgeURL("bits-leader"); }
                    else if (this.HasTwitchBadge("sub-gifter")) { this.TwitchSpecialtyBadge = this.GetTwitchBadgeURL("sub-gifter"); }
                    else if (this.HasTwitchBadge("bits")) { this.TwitchSpecialtyBadge = this.GetTwitchBadgeURL("bits"); }
                    else if (this.HasTwitchBadge("premium")) { this.TwitchSpecialtyBadge = this.GetTwitchBadgeURL("premium"); }
                }
            }

            this.SetCommonUserRoles();

            this.Color = null;
            this.RolesDisplayString = null;
        }

        private int GetTwitchBadgeVersion(string name)
        {
            if (this.Data.TwitchBadges != null && this.Data.TwitchBadges.TryGetValue(name, out int version))
            {
                return version;
            }
            return -1;
        }

        private bool HasTwitchBadge(string name) { return this.GetTwitchBadgeVersion(name) >= 0; }

        private ChatBadgeModel GetTwitchBadgeURL(string name)
        {
            if (ChannelSession.Services.Chat.TwitchChatService.ChatBadges.ContainsKey(name))
            {
                int versionID = this.GetTwitchBadgeVersion(name);
                if (ChannelSession.Services.Chat.TwitchChatService.ChatBadges[name].versions.ContainsKey(versionID.ToString()))
                {
                    return ChannelSession.Services.Chat.TwitchChatService.ChatBadges[name].versions[versionID.ToString()];
                }
            }
            return null;
        }

        #endregion Twitch Data Setter Functions

        public async Task AddModerationStrike(string moderationReason = null)
        {
            Dictionary<string, string> extraSpecialIdentifiers = new Dictionary<string, string>();
            extraSpecialIdentifiers.Add(ModerationService.ModerationReasonSpecialIdentifier, moderationReason);

            this.Data.ModerationStrikes++;
            if (this.Data.ModerationStrikes == 1)
            {
                await ChannelSession.Settings.GetCommand(ChannelSession.Settings.ModerationStrike1CommandID).Perform(new CommandParametersModel(this, extraSpecialIdentifiers));
            }
            else if (this.Data.ModerationStrikes == 2)
            {
                await ChannelSession.Settings.GetCommand(ChannelSession.Settings.ModerationStrike2CommandID).Perform(new CommandParametersModel(this, extraSpecialIdentifiers));
            }
            else if (this.Data.ModerationStrikes >= 3)
            {
                await ChannelSession.Settings.GetCommand(ChannelSession.Settings.ModerationStrike3CommandID).Perform(new CommandParametersModel(this, extraSpecialIdentifiers));
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
            if (ServiceContainer.Get<TwitchSessionService>().StreamIsLive)
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
            else
            {
                this.UserRoles.Remove(UserRoleEnum.Regular);
            }
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

        #region Twitch Refresh Functions

        private async Task RefreshTwitchUserDetails()
        {
            TwitchNewAPI.Users.UserModel twitchUser = (!string.IsNullOrEmpty(this.TwitchID)) ? await ServiceContainer.Get<TwitchSessionService>().UserConnection.GetNewAPIUserByID(this.TwitchID)
                : await ServiceContainer.Get<TwitchSessionService>().UserConnection.GetNewAPIUserByLogin(this.TwitchUsername);
            if (twitchUser != null)
            {
                this.TwitchID = twitchUser.id;
                this.TwitchUsername = twitchUser.login;
                this.TwitchDisplayName = (!string.IsNullOrEmpty(twitchUser.display_name)) ? twitchUser.display_name : this.TwitchDisplayName;
                this.TwitchAvatarLink = twitchUser.profile_image_url;

                if (twitchUser.IsPartner()) { this.UserRoles.Add(UserRoleEnum.Partner); } else { this.UserRoles.Remove(UserRoleEnum.Partner); }
                if (twitchUser.IsAffiliate()) { this.UserRoles.Add(UserRoleEnum.Affiliate); } else { this.UserRoles.Remove(UserRoleEnum.Affiliate); }
                if (twitchUser.IsStaff()) { this.UserRoles.Add(UserRoleEnum.Staff); } else { this.UserRoles.Remove(UserRoleEnum.Staff); }
                if (twitchUser.IsGlobalMod()) { this.UserRoles.Add(UserRoleEnum.GlobalMod); } else { this.UserRoles.Remove(UserRoleEnum.GlobalMod); }

                this.SetTwitchRoles();

                this.Color = null;
                this.RolesDisplayString = null;
            }
        }

        private void SetTwitchRoles()
        {
            this.TwitchUserRoles.Add(UserRoleEnum.User);
            if (ServiceContainer.Get<TwitchSessionService>().UserNewAPI != null && ServiceContainer.Get<TwitchSessionService>().UserNewAPI.id.Equals(this.TwitchID))
            {
                this.TwitchUserRoles.Add(UserRoleEnum.Streamer);
            }

            if (ServiceContainer.Get<TwitchSessionService>().ChannelEditorsV5.Contains(this.TwitchID))
            {
                this.TwitchUserRoles.Add(UserRoleEnum.ChannelEditor);
            }
            else
            {
                this.TwitchUserRoles.Remove(UserRoleEnum.ChannelEditor);
            }

            this.IsInChat = true;
        }

        private async Task RefreshTwitchUserAccountDate()
        {
            TwitchV5API.Users.UserModel twitchV5User = await ServiceContainer.Get<TwitchSessionService>().UserConnection.GetV5APIUserByLogin(this.TwitchUsername);
            if (twitchV5User != null && !string.IsNullOrEmpty(twitchV5User.created_at))
            {
                this.AccountDate = TwitchPlatformService.GetTwitchDateTime(twitchV5User.created_at);
            }
        }

        private async Task RefreshTwitchUserFollowDate()
        {
            UserFollowModel follow = await ServiceContainer.Get<TwitchSessionService>().UserConnection.CheckIfFollowsNewAPI(ServiceContainer.Get<TwitchSessionService>().UserNewAPI, this.GetTwitchNewAPIUserModel());
            if (follow != null && !string.IsNullOrEmpty(follow.followed_at))
            {
                this.FollowDate = TwitchPlatformService.GetTwitchDateTime(follow.followed_at);
            }
            else
            {
                this.FollowDate = null;
            }
        }

        private async Task RefreshTwitchUserSubscribeDate()
        {
            if (ServiceContainer.Get<TwitchSessionService>().UserNewAPI.IsAffiliate() || ServiceContainer.Get<TwitchSessionService>().UserNewAPI.IsPartner())
            {
                TwitchV5API.Users.UserSubscriptionModel subscription = await ServiceContainer.Get<TwitchSessionService>().UserConnection.CheckIfSubscribedV5(ServiceContainer.Get<TwitchSessionService>().ChannelV5, this.GetTwitchV5APIUserModel());
                if (subscription != null && !string.IsNullOrEmpty(subscription.created_at))
                {
                    this.SubscribeDate = TwitchPlatformService.GetTwitchDateTime(subscription.created_at);
                    this.Data.TwitchSubscriberTier = TwitchEventService.GetSubTierNumberFromText(subscription.sub_plan);
                }
                else
                {
                    this.SubscribeDate = null;
                    this.Data.TwitchSubscriberTier = 0;
                }
            }
        }

        #endregion Twitch Refresh Functions

        #region Glimesh Refresh Functions

        private async Task RefreshGlimeshUserDetails()
        {
            Glimesh.Base.Models.Users.UserModel glimeshUser = await ChannelSession.GlimeshUserConnection.GetUserByID(this.GlimeshID);
            if (glimeshUser != null)
            {
                this.GlimeshID = glimeshUser.id;
                this.GlimeshUsername = glimeshUser.username;
                this.GlimeshDisplayName = glimeshUser.displayname ?? glimeshUser.username;
                this.GlimeshAvatarLink = glimeshUser.FullAvatarURL;

                this.Color = null;
                this.RolesDisplayString = null;
            }
        }

        #endregion Glimesh Refresh Functions

        private void SetCommonUserRoles()
        {
            if (this.UserRoles.Contains(UserRoleEnum.Streamer))
            {
                this.UserRoles.Add(UserRoleEnum.ChannelEditor);
                this.UserRoles.Add(UserRoleEnum.Mod);
                this.UserRoles.Add(UserRoleEnum.Subscriber);
                this.UserRoles.Add(UserRoleEnum.Follower);
            }

            if (ChannelSession.Settings.RegularUserMinimumHours > 0 && this.Data.ViewingHoursPart >= ChannelSession.Settings.RegularUserMinimumHours)
            {
                this.UserRoles.Add(UserRoleEnum.Regular);
            }
            else
            {
                this.UserRoles.Remove(UserRoleEnum.Regular);
            }

            // Force re-build of roles display string
            this.Color = null;
            this.RolesDisplayString = null;
        }

        private Task RefreshExternalServiceDetails()
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
                    patreonUser = campaignMembers.FirstOrDefault(u => this.Platform.HasFlag(u.User.Platform) && string.Equals(u.User.PlatformUserID, this.PlatformID, StringComparison.InvariantCultureIgnoreCase));
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
            return Task.FromResult(0);
        }
    }
}
