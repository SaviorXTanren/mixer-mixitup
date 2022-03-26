using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Services.Glimesh;
using MixItUp.Base.Services.Trovo;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Services.YouTube;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twitch.Base.Models.Clients.Chat;
using Twitch.Base.Models.Clients.PubSub.Messages;
using Twitch.Base.Models.NewAPI.Chat;
using Twitch.Base.Models.NewAPI.Subscriptions;
using Twitch.Base.Models.NewAPI.Users;
using TwitchNewAPI = Twitch.Base.Models.NewAPI;

namespace MixItUp.Base.ViewModel.User
{
    [Obsolete]
    public class UserViewModel : IEquatable<UserViewModel>, IComparable<UserViewModel>
    {
        public const string UserDefaultColor = "MaterialDesignBody";

        public UserDataModel Data { get; private set; }

        //public static UserViewModel Create(string username)
        //{
        //    UserViewModel user = new UserViewModel(new UserDataModel());
        //    user.UnassociatedUsername = username;
        //    return user;
        //}

        //public static async Task<UserViewModel> Create(TwitchNewAPI.Users.UserModel twitchUser)
        //{
        //    UserViewModel user = await UserViewModel.Create(StreamingPlatformTypeEnum.Twitch, twitchUser.id, twitchUser.login);

        //    user.TwitchDisplayName = (!string.IsNullOrEmpty(twitchUser.display_name)) ? twitchUser.display_name : user.TwitchUsername;
        //    user.TwitchAvatarLink = twitchUser.profile_image_url;

        //    user.SetTwitchRoles();

        //    return user;
        //}

        //public static async Task<UserViewModel> Create(ChatMessagePacketModel twitchMessage)
        //{
        //    UserViewModel user = await UserViewModel.Create(StreamingPlatformTypeEnum.Twitch, twitchMessage.UserID, twitchMessage.UserLogin);

        //    user.TwitchDisplayName = (!string.IsNullOrEmpty(twitchMessage.UserDisplayName)) ? twitchMessage.UserDisplayName : user.TwitchUsername;

        //    user.SetTwitchRoles();

        //    user.SetTwitchChatDetails(twitchMessage);

        //    return user;
        //}

        //public static async Task<UserViewModel> Create(PubSubWhisperEventModel whisper)
        //{
        //    UserViewModel user = await UserViewModel.Create(StreamingPlatformTypeEnum.Twitch, whisper.from_id.ToString(), whisper.tags.login);

        //    user.TwitchDisplayName = (!string.IsNullOrEmpty(whisper.tags.display_name)) ? whisper.tags.display_name : user.TwitchUsername;

        //    user.SetTwitchRoles();

        //    return user;
        //}

        //public static async Task<UserViewModel> Create(PubSubWhisperEventRecipientModel whisperRecipient)
        //{
        //    UserViewModel user = await UserViewModel.Create(StreamingPlatformTypeEnum.Twitch, whisperRecipient.id.ToString(), whisperRecipient.username);

        //    user.TwitchDisplayName = (!string.IsNullOrEmpty(whisperRecipient.display_name)) ? whisperRecipient.display_name : user.TwitchUsername;
        //    user.TwitchAvatarLink = whisperRecipient.profile_image;

        //    user.SetTwitchRoles();

        //    return user;
        //}

        //public static async Task<UserViewModel> Create(ChatUserNoticePacketModel packet)
        //{
        //    UserViewModel user = await UserViewModel.Create(StreamingPlatformTypeEnum.Twitch, packet.UserID.ToString(), !string.IsNullOrEmpty(packet.RaidUserLogin) ? packet.RaidUserLogin : packet.Login);

        //    user.TwitchDisplayName = !string.IsNullOrEmpty(packet.RaidUserDisplayName) ? packet.RaidUserDisplayName : packet.DisplayName;
        //    if (string.IsNullOrEmpty(user.TwitchDisplayName))
        //    {
        //        user.TwitchDisplayName = user.TwitchUsername;
        //    }

        //    user.SetTwitchRoles();

        //    user.SetTwitchChatDetails(packet);

        //    return user;
        //}

        //public static async Task<UserViewModel> Create(ChatClearChatPacketModel packet)
        //{
        //    UserViewModel user = await UserViewModel.Create(StreamingPlatformTypeEnum.Twitch, packet.UserID, packet.UserLogin);

        //    user.SetTwitchRoles();

        //    return user;
        //}

        //public static async Task<UserViewModel> Create(PubSubBitsEventV2Model packet)
        //{
        //    UserViewModel user = await UserViewModel.Create(StreamingPlatformTypeEnum.Twitch, packet.user_id, packet.user_name);

        //    user.TwitchDisplayName = packet.user_name;

        //    user.SetTwitchRoles();

        //    return user;
        //}

        //public static async Task<UserViewModel> Create(PubSubSubscriptionsEventModel packet)
        //{
        //    UserViewModel user = await UserViewModel.Create(StreamingPlatformTypeEnum.Twitch, packet.user_id, packet.user_name);

        //    user.TwitchDisplayName = (!string.IsNullOrEmpty(packet.display_name)) ? packet.display_name : user.TwitchUsername;

        //    user.SetTwitchRoles();

        //    return user;
        //}

        //public static async Task<UserViewModel> Create(UserFollowModel follow)
        //{
        //    UserViewModel user = await UserViewModel.Create(StreamingPlatformTypeEnum.Twitch, follow.from_id, follow.from_name);

        //    user.TwitchDisplayName = follow.from_name;

        //    user.SetTwitchRoles();

        //    return user;
        //}

        ////public static async Task<UserViewModel> Create(TwitchWebhookFollowModel follow)
        ////{
        ////    UserViewModel user = await UserViewModel.Create(StreamingPlatformTypeEnum.Twitch, follow.UserID, follow.Username);

        ////    user.TwitchDisplayName = follow.UserDisplayName;

        ////    user.SetTwitchRoles();

        ////    return user;
        ////}

        //public static async Task<UserViewModel> Create(Glimesh.Base.Models.Users.UserModel glimeshUser)
        //{
        //    UserViewModel user = await UserViewModel.Create(StreamingPlatformTypeEnum.Glimesh, glimeshUser.id, glimeshUser.username);

        //    user.GlimeshDisplayName = glimeshUser.displayname;
        //    user.GlimeshAvatarLink = glimeshUser.avatarUrl;
        //    user.AccountDate = GlimeshPlatformService.GetGlimeshDateTime(glimeshUser.confirmedAt);

        //    user.SetGlimeshRoles();

        //    return user;
        //}

        //public static async Task<UserViewModel> Create(Glimesh.Base.Models.Clients.Chat.ChatMessagePacketModel message) { return await UserViewModel.Create(message.User); }

        //public static async Task<UserViewModel> Create(Trovo.Base.Models.Users.UserModel trovoUser)
        //{
        //    UserViewModel user = await UserViewModel.Create(StreamingPlatformTypeEnum.Trovo, trovoUser.user_id, trovoUser.username);

        //    user.TrovoDisplayName = trovoUser.nickname;

        //    user.SetTrovoRoles();

        //    return user;
        //}

        //public static async Task<UserViewModel> Create(Trovo.Base.Models.Users.PrivateUserModel trovoUser)
        //{
        //    UserViewModel user = await UserViewModel.Create(StreamingPlatformTypeEnum.Trovo, trovoUser.userId, trovoUser.userName);

        //    user.TrovoDisplayName = trovoUser.nickName;
        //    user.TrovoAvatarLink = trovoUser.profilePic;

        //    user.SetTrovoRoles();

        //    return user;
        //}

        //public static async Task<UserViewModel> Create(Trovo.Base.Models.Chat.ChatMessageModel message)
        //{
        //    UserViewModel user = await UserViewModel.Create(StreamingPlatformTypeEnum.Trovo, message.sender_id.ToString(), message.user_name);

        //    user.TrovoDisplayName = message.nick_name;

        //    user.SetTrovoChatDetails(message);

        //    return user;
        //}

        //public static async Task<UserViewModel> Create(Google.Apis.YouTube.v3.Data.Channel channel)
        //{
        //    UserViewModel user = await UserViewModel.Create(StreamingPlatformTypeEnum.YouTube, channel.Id, channel.Snippet.Title);

        //    user.YouTubeDisplayName = channel.Snippet.Title;
        //    user.YouTubeAvatarLink = channel.Snippet.Thumbnails.Default__.Url;
        //    user.YouTubeURL = "https://www.youtube.com/channel/" + channel.Id;

        //    user.SetYouTubeRoles();

        //    return user;
        //}

        //public static async Task<UserViewModel> Create(Google.Apis.YouTube.v3.Data.LiveChatMessage message)
        //{
        //    UserViewModel user = await UserViewModel.Create(StreamingPlatformTypeEnum.YouTube, message.AuthorDetails?.ChannelId, message.AuthorDetails.DisplayName);

        //    user.SetYouTubeChatDetails(message);

        //    return user;
        //}

        //public UserViewModel(UserDataModel userData)
        //{
        //    this.Data = userData;
        //}

        [Obsolete]
        public UserViewModel() { }

        //private static async Task<UserViewModel> Create(StreamingPlatformTypeEnum platform, string platformID, string platformUsername)
        //{
        //    UserDataModel data = new UserDataModel();
        //    if (!string.IsNullOrEmpty(platformID) && !string.IsNullOrEmpty(platformUsername))
        //    {
        //        //data = await ServiceManager.Get<UserService>().GetUserDataByPlatformID(platform, platformID);
        //        //if (data == null)
        //        //{
        //        //    data = await ServiceManager.Get<UserService>().GetUserDataByPlatformUsername(platform, platformUsername);
        //        //    if (data == null)
        //        //    {
        //        //        data = new UserDataModel();
        //        //    }
        //        //}

        //        //switch (platform)
        //        //{
        //        //    case StreamingPlatformTypeEnum.Twitch:
        //        //        data.TwitchID = platformID;
        //        //        data.TwitchUsername = platformUsername;
        //        //        break;
        //        //    case StreamingPlatformTypeEnum.YouTube:
        //        //        data.YouTubeID = platformID;
        //        //        data.YouTubeUsername = platformUsername;
        //        //        break;
        //        //    case StreamingPlatformTypeEnum.Trovo:
        //        //        data.TrovoID = platformID;
        //        //        data.TrovoUsername = platformUsername;
        //        //        break;
        //        //    case StreamingPlatformTypeEnum.Glimesh:
        //        //        data.GlimeshID = platformID;
        //        //        data.GlimeshUsername = platformUsername;
        //        //        break;
        //        //}

        //        //ServiceManager.Get<UserService>().SetUserData(data, newData: true);
        //    }
        //    return new UserViewModel(data);
        //}

        [JsonIgnore]
        public Guid ID { get { return this.Data.ID; } }

        [JsonIgnore]
        public StreamingPlatformTypeEnum Platform { get { return this.Data.Platforms.FirstOrDefault(); } }

        [JsonIgnore]
        public string PlatformID
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return this.TwitchID; }
                else if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return this.YouTubeID; }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return this.GlimeshID; }
                else if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return this.TrovoID; }
                return null;
            }
        }

        [JsonIgnore]
        public string Username
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return this.TwitchUsername; }
                else if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return this.YouTubeUsername; }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return this.GlimeshUsername; }
                else if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return this.TrovoUsername; }
                return this.UnassociatedUsername;
            }
        }

        [JsonIgnore]
        public string DisplayName
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return !string.IsNullOrEmpty(this.TwitchDisplayName) ? this.TwitchDisplayName : this.TwitchUsername; }
                return this.UnassociatedUsername;
            }
        }

        [JsonIgnore]
        public string FullDisplayName
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch)
                {
                    if (!string.IsNullOrEmpty(this.TwitchDisplayName) && !string.Equals(this.TwitchDisplayName, this.TwitchUsername, StringComparison.OrdinalIgnoreCase))
                    {
                        return $"{this.TwitchDisplayName} ({this.TwitchUsername})";
                    }
                    else
                    {
                        return this.TwitchDisplayName;
                    }
                }
                else if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return this.YouTubeDisplayName ?? this.YouTubeUsername; }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return this.GlimeshDisplayName ?? this.GlimeshUsername; }
                else if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return this.TrovoDisplayName ?? this.TrovoUsername; }
                return this.UnassociatedUsername;
            }
        }

        [JsonIgnore]
        public HashSet<OldUserRoleEnum> UserRoles { get { return this.Data.UserRoles; } }

        [JsonIgnore]
        public IEnumerable<OldUserRoleEnum> DisplayRoles
        {
            get
            {
                List<OldUserRoleEnum> userRoles = this.UserRoles.ToList();

                if (this.Data.UserRoles.Contains(OldUserRoleEnum.Banned))
                {
                    userRoles.Clear();
                    userRoles.Add(OldUserRoleEnum.Banned);
                }
                else
                {
                    if (this.Data.UserRoles.Count() > 1)
                    {
                        userRoles.Remove(OldUserRoleEnum.User);
                    }

                    if (this.Data.UserRoles.Contains(OldUserRoleEnum.Subscriber) || this.Data.UserRoles.Contains(OldUserRoleEnum.Streamer))
                    {
                        userRoles.Remove(OldUserRoleEnum.Follower);
                    }

                    if (this.Data.UserRoles.Contains(OldUserRoleEnum.Streamer))
                    {
                        userRoles.Remove(OldUserRoleEnum.ChannelEditor);
                        userRoles.Remove(OldUserRoleEnum.Subscriber);
                    }
                }

                return userRoles.OrderByDescending(r => r);
            }
        }

        [JsonIgnore]
        public string AvatarLink
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return this.TwitchAvatarLink; }
                else if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return this.YouTubeAvatarLink; }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return this.GlimeshAvatarLink; }
                else if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return this.TrovoAvatarLink; }
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
                        foreach (OldUserRoleEnum role in this.UserRoles.OrderByDescending(r => r))
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
                else if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return this.YouTubeURL; }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return $"https://www.glimesh.tv/{this.Username}"; }
                else if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return $"https://trovo.live/{this.Username}"; }
                return string.Empty;
            }
        }

        public DateTimeOffset? AccountDate
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return this.Data.TwitchAccountDate; }
                else if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return this.Data.YouTubeAccountDate; }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return this.Data.GlimeshAccountDate; }
                else if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return this.Data.TrovoAccountDate; }
                return null;
            }
            set
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { this.Data.TwitchAccountDate = value; }
                else if (this.Platform == StreamingPlatformTypeEnum.YouTube) { this.Data.YouTubeAccountDate = value; }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { this.Data.GlimeshAccountDate = value; }
                else if (this.Platform == StreamingPlatformTypeEnum.Trovo) { this.Data.TrovoAccountDate = value; }
            }
        }

        public DateTimeOffset? FollowDate
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return this.Data.TwitchFollowDate; }
                else if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return this.Data.YouTubeFollowDate; }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return this.Data.GlimeshFollowDate; }
                else if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return this.Data.TrovoFollowDate; }
                return null;
            }
            set
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { this.Data.TwitchFollowDate = value; }
                else if (this.Platform == StreamingPlatformTypeEnum.YouTube) { this.Data.YouTubeFollowDate = value; }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { this.Data.GlimeshFollowDate = value; }
                else if (this.Platform == StreamingPlatformTypeEnum.Trovo) { this.Data.TrovoFollowDate = value; }

                if (this.FollowDate == null || this.FollowDate.GetValueOrDefault() == DateTimeOffset.MinValue)
                {
                    this.UserRoles.Remove(OldUserRoleEnum.Follower);
                }
                else
                {
                    this.UserRoles.Add(OldUserRoleEnum.Follower);
                }
            }
        }

        public DateTimeOffset? SubscribeDate
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return this.Data.TwitchSubscribeDate; }
                else if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return this.Data.YouTubeSubscribeDate; }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return this.Data.GlimeshSubscribeDate; }
                else if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return this.Data.TrovoSubscribeDate; }
                return null;
            }
            set
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { this.Data.TwitchSubscribeDate = value; }
                else if (this.Platform == StreamingPlatformTypeEnum.YouTube) { this.Data.YouTubeSubscribeDate = value; }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { this.Data.GlimeshSubscribeDate = value; }
                else if (this.Platform == StreamingPlatformTypeEnum.Trovo) { this.Data.TrovoSubscribeDate = value; }

                if (this.SubscribeDate == null || this.SubscribeDate.GetValueOrDefault() == DateTimeOffset.MinValue)
                {
                    this.UserRoles.Remove(OldUserRoleEnum.Subscriber);
                }
                else
                {
                    this.UserRoles.Add(OldUserRoleEnum.Subscriber);
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
                    else if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return 1; }
                    else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return 1; }
                    else if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return this.Data.TrovoSubscriberLevel; }
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
                else if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return "/Assets/Images/YouTube.png"; }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return "/Assets/Images/Glimesh.png"; }
                else if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return "/Assets/Images/Trovo.png"; }
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
                else if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return string.IsNullOrEmpty(this.YouTubeID); }
                else if (this.Platform == StreamingPlatformTypeEnum.Glimesh) { return string.IsNullOrEmpty(this.GlimeshID); }
                else if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return string.IsNullOrEmpty(this.TrovoID); }
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

        public HashSet<OldUserRoleEnum> TwitchUserRoles { get { return this.Data.TwitchUserRoles; } private set { this.Data.TwitchUserRoles = value; } }

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

        #region YouTube

        public string YouTubeID { get { return this.Data.YouTubeID; } private set { this.Data.YouTubeID = value; } }
        public string YouTubeUsername { get { return this.Data.YouTubeUsername; } private set { this.Data.YouTubeUsername = value; } }
        public string YouTubeDisplayName { get { return this.Data.YouTubeDisplayName; } private set { this.Data.YouTubeDisplayName = value; } }
        public string YouTubeAvatarLink { get { return this.Data.YouTubeAvatarLink; } private set { this.Data.YouTubeAvatarLink = value; } }
        public string YouTubeURL { get { return this.Data.YouTubeURL; } private set { this.Data.YouTubeURL = value; } }

        public HashSet<OldUserRoleEnum> YouTubeUserRoles { get { return this.Data.YouTubeUserRoles; } private set { this.Data.YouTubeUserRoles = value; } }

        #endregion YouTube

        #region Glimesh

        public string GlimeshID { get { return this.Data.GlimeshID; } private set { this.Data.GlimeshID = value; } }
        public string GlimeshUsername { get { return this.Data.GlimeshUsername; } private set { this.Data.GlimeshUsername = value; } }
        public string GlimeshDisplayName { get { return this.Data.GlimeshDisplayName; } private set { this.Data.GlimeshDisplayName = value; } }
        public string GlimeshAvatarLink { get { return this.Data.GlimeshAvatarLink; } private set { this.Data.GlimeshAvatarLink = value; } }

        public HashSet<OldUserRoleEnum> GlimeshUserRoles { get { return this.Data.GlimeshUserRoles; } private set { this.Data.GlimeshUserRoles = value; } }

        #endregion Glimesh

        #region Trovo

        public string TrovoID { get { return this.Data.TrovoID; } private set { this.Data.TrovoID = value; } }
        public string TrovoUsername { get { return this.Data.TrovoUsername; } private set { this.Data.TrovoUsername = value; } }
        public string TrovoDisplayName { get { return this.Data.TrovoDisplayName; } private set { this.Data.TrovoDisplayName = value; } }
        public string TrovoAvatarLink { get { return this.Data.TrovoAvatarLink; } private set { this.Data.TrovoAvatarLink = value; } }

        public HashSet<OldUserRoleEnum> TrovoUserRoles { get { return this.Data.GlimeshUserRoles; } private set { this.Data.GlimeshUserRoles = value; } }

        #endregion Trovo

        public DateTimeOffset LastUpdated { get { return this.Data.LastUpdated; } set { this.Data.LastUpdated = value; } }

        public DateTimeOffset LastActivity { get { return this.Data.LastActivity; } set { this.Data.LastActivity = value; } }

        public HashSet<string> CustomRoles { get { return this.Data.CustomRoles; } set { this.Data.CustomRoles = value; } }

        public bool IgnoreForQueries { get { return this.Data.IgnoreForQueries; } set { this.Data.IgnoreForQueries = value; } }

        public bool IsInChat { get { return this.Data.IsInChat; } set { this.Data.IsInChat = value; } }

        public string TwitterURL { get { return this.Data.TwitterURL; } set { this.Data.TwitterURL = value; } }

        public PatreonCampaignMember PatreonUser { get { return this.Data.PatreonUser; } set { this.Data.PatreonUser = value; } }

        public OldUserRoleEnum PrimaryRole { get { return this.Data.PrimaryRole; } }

        public string PrimaryRoleString { get { return EnumLocalizationHelper.GetLocalizedName(this.PrimaryRole); } }

        [JsonIgnore]
        public string SortableID
        {
            get
            {
                OldUserRoleEnum role = this.PrimaryRole;
                if (role < OldUserRoleEnum.Subscriber)
                {
                    role = OldUserRoleEnum.User;
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

                //UserTitleModel title = ChannelSession.Settings.UserTitles.OrderByDescending(t => t.Role).ThenByDescending(t => t.Months).FirstOrDefault(t => t.MeetsTitle(this));
                //if (title != null)
                //{
                //    return title.Name;
                //}

                return "No Title";
            }
            set
            {
                this.Data.CustomTitle = value;
            }
        }

        public string RolesString
        {
            get
            {
                lock (this.rolesStringLock)
                {
                    if (this.Data.RolesString == null)
                    {
                        List<string> displayRoles = new List<string>(this.DisplayRoles.OrderByDescending(r => r).Select(r => r.ToString()));
                        displayRoles.AddRange(this.CustomRoles);
                        this.Data.RolesString = string.Join(", ", displayRoles);
                    }
                    return this.Data.RolesString;
                }
            }
            private set
            {
                lock (this.rolesStringLock)
                {
                    this.Data.RolesString = value;
                }
            }
        }
        private object rolesStringLock = new object();

        public string RolesLocalizedString
        {
            get
            {
                lock (this.rolesLocalizedStringLock)
                {
                    if (this.Data.RolesDisplayString == null)
                    {
                        List<string> displayRoles = new List<string>(this.DisplayRoles.OrderByDescending(r => r).Select(r => EnumLocalizationHelper.GetLocalizedName(r)));
                        displayRoles.AddRange(this.CustomRoles);
                        this.Data.RolesDisplayString = string.Join(", ", displayRoles);
                    }
                    return this.Data.RolesDisplayString;
                }
            }
            private set
            {
                lock (this.rolesLocalizedStringLock)
                {
                    this.Data.RolesDisplayString = value;
                }
            }
        }
        private object rolesLocalizedStringLock = new object();

        [JsonIgnore]
        public bool IsFollower { get { return this.UserRoles.Contains(OldUserRoleEnum.Follower) || this.HasPermissionsTo(OldUserRoleEnum.Subscriber); } }
        [JsonIgnore]
        public bool IsRegular { get { return this.UserRoles.Contains(OldUserRoleEnum.Regular); } }
        [JsonIgnore]
        public bool IsPlatformSubscriber { get { return this.UserRoles.Contains(OldUserRoleEnum.Subscriber); } }
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
                if (ServiceManager.Get<PatreonService>().IsConnected && this.PatreonUser != null)
                {
                    return ServiceManager.Get<PatreonService>().Campaign.GetTier(this.PatreonUser.TierID);
                }
                return null;
            }
        }

        [JsonIgnore]
        public bool IsSubscriber { get { return this.UserRoles.Contains(OldUserRoleEnum.Subscriber) || this.IsEquivalentToSubscriber(); } }

        public bool HasPermissionsTo(OldUserRoleEnum checkRole)
        {
            Logger.Log($"Checking role permission for user: {this.PrimaryRole} - {checkRole}");

            if (checkRole == OldUserRoleEnum.Subscriber && this.IsEquivalentToSubscriber())
            {
                return true;
            }

            if (checkRole == OldUserRoleEnum.VIPExclusive && this.UserRoles.Contains(OldUserRoleEnum.VIP))
            {
                return true;
            }

            if (ChannelSession.Settings.ExplicitUserRoleRequirements)
            {
                return this.UserRoles.Contains(checkRole);
            }

            return this.PrimaryRole >= checkRole;
        }

        public bool ExceedsPermissions(OldUserRoleEnum checkRole) { return this.PrimaryRole > checkRole; }

        public bool IsEquivalentToSubscriber()
        {
            if (this.PatreonUser != null && ServiceManager.Get<PatreonService>().IsConnected && !string.IsNullOrEmpty(ChannelSession.Settings.PatreonTierSubscriberEquivalent))
            {
                PatreonTier userTier = this.PatreonTier;
                PatreonTier equivalentTier = ServiceManager.Get<PatreonService>().Campaign.GetTier(ChannelSession.Settings.PatreonTierSubscriberEquivalent);
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

                    if (this.Platform == StreamingPlatformTypeEnum.Twitch)
                    {
                        await this.RefreshTwitchUserDetails();
                        await this.RefreshTwitchUserAccountDate();
                        await this.RefreshTwitchUserFollowDate();
                        await this.RefreshTwitchUserSubscribeDate();
                    }
                    else if (this.Platform == StreamingPlatformTypeEnum.YouTube)
                    {
                        await this.RefreshYouTubeUserDetails();
                    }
                    else if (this.Platform == StreamingPlatformTypeEnum.Glimesh)
                    {
                        await this.RefreshGlimeshUserDetails();
                    }
                    else if (this.Platform == StreamingPlatformTypeEnum.Trovo)
                    {
                        await this.RefreshTrovoUserDetails();
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

        public void SetTwitchChatDetails(Twitch.Base.Models.Clients.Chat.ChatMessagePacketModel message)
        {
            this.SetTwitchChatDetails(message.UserDisplayName, message.BadgeDictionary, message.BadgeInfoDictionary, message.Color);
        }

        public void SetTwitchChatDetails(Twitch.Base.Models.Clients.Chat.ChatUserStatePacketModel userState)
        {
            this.SetTwitchChatDetails(userState.UserDisplayName, userState.BadgeDictionary, userState.BadgeInfoDictionary, userState.Color);
        }

        public void SetTwitchChatDetails(Twitch.Base.Models.Clients.Chat.ChatUserNoticePacketModel userNotice)
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
                if (this.HasTwitchBadge("admin") || this.HasTwitchBadge("staff")) { this.TwitchUserRoles.Add(OldUserRoleEnum.Staff); } else { this.TwitchUserRoles.Remove(OldUserRoleEnum.Staff); }
                if (this.HasTwitchBadge("global_mod")) { this.TwitchUserRoles.Add(OldUserRoleEnum.GlobalMod); } else { this.TwitchUserRoles.Remove(OldUserRoleEnum.GlobalMod); }
                if (this.HasTwitchBadge("moderator")) { this.TwitchUserRoles.Add(OldUserRoleEnum.Mod); } else { this.TwitchUserRoles.Remove(OldUserRoleEnum.Mod); }
                if (this.IsTwitchSubscriber) { this.TwitchUserRoles.Add(OldUserRoleEnum.Subscriber); } else { this.TwitchUserRoles.Remove(OldUserRoleEnum.Subscriber); }
                if (this.HasTwitchBadge("turbo") || this.HasTwitchBadge("premium")) { this.TwitchUserRoles.Add(OldUserRoleEnum.Premium); } else { this.TwitchUserRoles.Remove(OldUserRoleEnum.Premium); }
                if (this.HasTwitchBadge("vip")) { this.TwitchUserRoles.Add(OldUserRoleEnum.VIP); } else { this.TwitchUserRoles.Remove(OldUserRoleEnum.VIP); }

                if (ServiceManager.Get<TwitchChatService>() != null)
                {
                    if (this.HasTwitchBadge("staff")) { this.TwitchRoleBadge = this.GetTwitchBadgeURL("staff"); }
                    else if (this.HasTwitchBadge("admin")) { this.TwitchRoleBadge = this.GetTwitchBadgeURL("admin"); }
                    else if (this.HasTwitchBadge("extension")) { this.TwitchRoleBadge = this.GetTwitchBadgeURL("extension"); }
                    else if (this.HasTwitchBadge("twitchbot")) { this.TwitchRoleBadge = this.GetTwitchBadgeURL("twitchbot"); }
                    else if (this.TwitchUserRoles.Contains(OldUserRoleEnum.Mod)) { this.TwitchRoleBadge = this.GetTwitchBadgeURL("moderator"); }
                    else if (this.TwitchUserRoles.Contains(OldUserRoleEnum.VIP)) { this.TwitchRoleBadge = this.GetTwitchBadgeURL("vip"); }

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

            this.ClearDisplayProperties();
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

        #endregion Twitch Data Setter Functions

        #region YouTube Data Setter Functions

        public void SetYouTubeChatDetails(Google.Apis.YouTube.v3.Data.LiveChatMessage message)
        {
            if (message.AuthorDetails != null)
            {
                this.YouTubeID = message.AuthorDetails.ChannelId;
                this.YouTubeUsername = this.YouTubeDisplayName = message.AuthorDetails.DisplayName;
                this.YouTubeAvatarLink = message.AuthorDetails.ProfileImageUrl;
                this.YouTubeURL = message.AuthorDetails.ChannelUrl;

                this.SetYouTubeRoles(message);
            }
        }

        #endregion YouTube Data Setter Functions

        #region Glimesh Data Setter Functions

        public void SetGlimeshChatDetails(Glimesh.Base.Models.Clients.Chat.ChatMessagePacketModel message)
        {
            if (message.User != null)
            {
                this.GlimeshUsername = message.User.username;
                this.GlimeshDisplayName = message.User.displayname;
                this.GlimeshAvatarLink = message.User.avatarUrl;
                this.AccountDate = GlimeshPlatformService.GetGlimeshDateTime(message.User.confirmedAt);
            }
        }

        #endregion Glimesh Data Setter Functions

        #region Trovo Data Setter Functions

        public void SetTrovoChatDetails(Trovo.Base.Models.Chat.ChatMessageModel message)
        {
            this.TrovoAvatarLink = message.FullAvatarURL;

            this.SetTrovoRoles(message.roles);
        }

        #endregion Trovo Data Setter Functions

        //public async Task AddModerationStrike(string moderationReason = null)
        //{
        //    Dictionary<string, string> extraSpecialIdentifiers = new Dictionary<string, string>();
        //    extraSpecialIdentifiers.Add(ModerationService.ModerationReasonSpecialIdentifier, moderationReason);

        //    this.Data.ModerationStrikes++;
        //    //if (this.Data.ModerationStrikes == 1)
        //    //{
        //    //    await ServiceManager.Get<CommandService>().Queue(ChannelSession.Settings.ModerationStrike1CommandID, new CommandParametersModel(this, extraSpecialIdentifiers));
        //    //}
        //    //else if (this.Data.ModerationStrikes == 2)
        //    //{
        //    //    await ServiceManager.Get<CommandService>().Queue(ChannelSession.Settings.ModerationStrike2CommandID, new CommandParametersModel(this, extraSpecialIdentifiers));
        //    //}
        //    //else if (this.Data.ModerationStrikes >= 3)
        //    //{
        //    //    await ServiceManager.Get<CommandService>().Queue(ChannelSession.Settings.ModerationStrike3CommandID, new CommandParametersModel(this, extraSpecialIdentifiers));
        //    //}
        //}

        public Task RemoveModerationStrike()
        {
            if (this.Data.ModerationStrikes > 0)
            {
                this.Data.ModerationStrikes--;
            }
            return Task.CompletedTask;
        }

        public void UpdateMinuteData()
        {
            this.Data.ViewingMinutes++;
            ChannelSession.Settings.Users.ManualValueChanged(this.ID);

            if (ChannelSession.Settings.RegularUserMinimumHours > 0 && this.Data.ViewingHoursPart >= ChannelSession.Settings.RegularUserMinimumHours)
            {
                this.UserRoles.Add(OldUserRoleEnum.Regular);
            }
            else
            {
                this.UserRoles.Remove(OldUserRoleEnum.Regular);
            }
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
            if (ServiceManager.Get<TwitchSessionService>().IsConnected)
            {
                TwitchNewAPI.Users.UserModel twitchUser = (!string.IsNullOrEmpty(this.TwitchID)) ? await ServiceManager.Get<TwitchSessionService>().UserConnection.GetNewAPIUserByID(this.TwitchID)
                    : await ServiceManager.Get<TwitchSessionService>().UserConnection.GetNewAPIUserByLogin(this.TwitchUsername);
                if (twitchUser != null)
                {
                    this.TwitchID = twitchUser.id;
                    this.TwitchUsername = twitchUser.login;
                    this.TwitchDisplayName = (!string.IsNullOrEmpty(twitchUser.display_name)) ? twitchUser.display_name : this.TwitchDisplayName;
                    this.TwitchAvatarLink = twitchUser.profile_image_url;

                    if (twitchUser.IsPartner()) { this.UserRoles.Add(OldUserRoleEnum.Partner); } else { this.UserRoles.Remove(OldUserRoleEnum.Partner); }
                    if (twitchUser.IsAffiliate()) { this.UserRoles.Add(OldUserRoleEnum.Affiliate); } else { this.UserRoles.Remove(OldUserRoleEnum.Affiliate); }
                    if (twitchUser.IsStaff()) { this.UserRoles.Add(OldUserRoleEnum.Staff); } else { this.UserRoles.Remove(OldUserRoleEnum.Staff); }
                    if (twitchUser.IsGlobalMod()) { this.UserRoles.Add(OldUserRoleEnum.GlobalMod); } else { this.UserRoles.Remove(OldUserRoleEnum.GlobalMod); }

                    this.SetTwitchRoles();

                    this.ClearDisplayProperties();
                }
            }
        }

        private void SetTwitchRoles()
        {
            this.TwitchUserRoles.Add(OldUserRoleEnum.User);
            if (string.Equals(ServiceManager.Get<TwitchSessionService>().UserID, this.TwitchID, StringComparison.OrdinalIgnoreCase))
            {
                this.TwitchUserRoles.Add(OldUserRoleEnum.Streamer);
            }

            if (ServiceManager.Get<TwitchSessionService>().ChannelEditors.Contains(this.TwitchID))
            {
                this.TwitchUserRoles.Add(OldUserRoleEnum.ChannelEditor);
            }
            else
            {
                this.TwitchUserRoles.Remove(OldUserRoleEnum.ChannelEditor);
            }

            this.IsInChat = true;
        }

        private async Task RefreshTwitchUserAccountDate()
        {
            TwitchNewAPI.Users.UserModel twitchUser = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetNewAPIUserByID(this.TwitchID);
            if (twitchUser != null && !string.IsNullOrEmpty(twitchUser.created_at))
            {
                this.AccountDate = TwitchPlatformService.GetTwitchDateTime(twitchUser.created_at);
            }
        }

        private async Task RefreshTwitchUserFollowDate()
        {
            UserFollowModel follow = await ServiceManager.Get<TwitchSessionService>().UserConnection.CheckIfFollowsNewAPI(ServiceManager.Get<TwitchSessionService>().User, this.GetTwitchNewAPIUserModel());
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
            if (ServiceManager.Get<TwitchSessionService>().User.IsAffiliate() || ServiceManager.Get<TwitchSessionService>().User.IsPartner())
            {
                SubscriptionModel subscription = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetBroadcasterSubscription(ServiceManager.Get<TwitchSessionService>().User, this.GetTwitchNewAPIUserModel());
                if (subscription != null)
                {
                    // TODO: No subscription data from this API. https://twitch.uservoice.com/forums/310213-developers/suggestions/43806120-add-subscription-date-to-subscription-apis
                    //this.SubscribeDate = TwitchPlatformService.GetTwitchDateTime(subscription.created_at);
                    this.Data.TwitchSubscriberTier = TwitchEventService.GetSubTierNumberFromText(subscription.tier);
                }
                else
                {
                    this.SubscribeDate = null;
                    this.Data.TwitchSubscriberTier = 0;
                }
            }
        }

        #endregion Twitch Refresh Functions

        #region YouTube Refresh Functions

        private async Task RefreshYouTubeUserDetails()
        {
            if (ServiceManager.Get<YouTubeSessionService>().IsConnected)
            {
                Google.Apis.YouTube.v3.Data.Channel youtubeUser = await ServiceManager.Get<YouTubeSessionService>().UserConnection.GetChannelByID(this.YouTubeUsername);
                if (youtubeUser != null)
                {
                    this.YouTubeID = youtubeUser.Id;
                    this.YouTubeUsername = this.YouTubeDisplayName = youtubeUser.Snippet.Title;
                    this.YouTubeAvatarLink = youtubeUser.Snippet.Thumbnails.Standard.Url;
                    this.YouTubeURL = youtubeUser.Snippet.CustomUrl;

                    this.ClearDisplayProperties();
                }

                this.SetYouTubeRoles();
            }
        }

        private void SetYouTubeRoles(Google.Apis.YouTube.v3.Data.LiveChatMessage message = null)
        {
            this.YouTubeUserRoles.Add(OldUserRoleEnum.User);
            if (string.Equals(ServiceManager.Get<YouTubeSessionService>().UserID, this.YouTubeID, StringComparison.OrdinalIgnoreCase))
            {
                this.YouTubeUserRoles.Add(OldUserRoleEnum.Streamer);
            }

            if (message != null)
            {
                if (message.AuthorDetails.IsChatOwner.GetValueOrDefault())
                {
                    this.YouTubeUserRoles.Add(OldUserRoleEnum.Streamer);
                }
                else
                {
                    this.YouTubeUserRoles.Remove(OldUserRoleEnum.Streamer);
                }

                if (message.AuthorDetails.IsChatModerator.GetValueOrDefault())
                {
                    this.YouTubeUserRoles.Add(OldUserRoleEnum.Mod);
                }
                else
                {
                    this.YouTubeUserRoles.Add(OldUserRoleEnum.Mod);
                }

                if (message.AuthorDetails.IsChatSponsor.GetValueOrDefault())
                {
                    this.YouTubeUserRoles.Add(OldUserRoleEnum.Subscriber);
                }
                else
                {
                    this.YouTubeUserRoles.Add(OldUserRoleEnum.Subscriber);
                }

                this.IsInChat = true;
            }
        }

        #endregion YouTube Refresh Functions

        #region Glimesh Refresh Functions

        private async Task RefreshGlimeshUserDetails()
        {
            if (ServiceManager.Get<GlimeshSessionService>().IsConnected)
            {
                Glimesh.Base.Models.Users.UserModel glimeshUser = await ServiceManager.Get<GlimeshSessionService>().UserConnection.GetUserByID(this.GlimeshID);
                if (glimeshUser != null)
                {
                    this.GlimeshID = glimeshUser.id;
                    this.GlimeshUsername = glimeshUser.username;
                    this.GlimeshDisplayName = glimeshUser.displayname ?? glimeshUser.username;
                    this.GlimeshAvatarLink = glimeshUser.avatarUrl;

                    this.ClearDisplayProperties();
                }

                this.SetGlimeshRoles();
            }
        }
        private void SetGlimeshRoles()
        {
            this.GlimeshUserRoles.Add(OldUserRoleEnum.User);
            if (string.Equals(ServiceManager.Get<GlimeshSessionService>().UserID, this.GlimeshID, StringComparison.OrdinalIgnoreCase))
            {
                this.GlimeshUserRoles.Add(OldUserRoleEnum.Streamer);
            }

            this.IsInChat = true;
        }

        #endregion Glimesh Refresh Functions

        #region Trovo Refresh Functions

        private async Task RefreshTrovoUserDetails()
        {
            if (ServiceManager.Get<TrovoSessionService>().IsConnected)
            {
                Trovo.Base.Models.Users.UserModel trovoUser = await ServiceManager.Get<TrovoSessionService>().UserConnection.GetUserByName(this.TrovoUsername);
                if (trovoUser != null)
                {
                    this.TrovoID = trovoUser.user_id;
                    this.TrovoUsername = trovoUser.username;
                    this.TrovoDisplayName = trovoUser.nickname ?? trovoUser.username;

                    this.ClearDisplayProperties();
                }

                this.SetTrovoRoles();
            }
        }

        private void SetTrovoRoles(IEnumerable<string> roles = null)
        {
            this.TrovoUserRoles.Add(OldUserRoleEnum.User);
            if (ServiceManager.Get<GlimeshSessionService>().User != null && ServiceManager.Get<GlimeshSessionService>().User.id.Equals(this.GlimeshID))
            {
                this.TrovoUserRoles.Add(OldUserRoleEnum.Streamer);
            }

            if (roles != null)
            {
                HashSet<string> rolesSet = new HashSet<string>(roles);

                if (rolesSet.Contains(Trovo.Base.Models.Chat.ChatMessageModel.StreamerRole))
                {
                    this.TrovoUserRoles.Add(OldUserRoleEnum.Streamer);
                }
                else
                {
                    this.TrovoUserRoles.Remove(OldUserRoleEnum.Streamer);
                }

                if (rolesSet.Contains(Trovo.Base.Models.Chat.ChatMessageModel.AdminRole))
                {
                    this.TrovoUserRoles.Add(OldUserRoleEnum.Staff);
                }
                else
                {
                    this.TrovoUserRoles.Remove(OldUserRoleEnum.Staff);
                }

                if (rolesSet.Contains(Trovo.Base.Models.Chat.ChatMessageModel.WardenRole))
                {
                    this.TrovoUserRoles.Add(OldUserRoleEnum.GlobalMod);
                }
                else
                {
                    this.TrovoUserRoles.Remove(OldUserRoleEnum.GlobalMod);
                }

                if (rolesSet.Contains(Trovo.Base.Models.Chat.ChatMessageModel.ModeratorRole))
                {
                    this.TrovoUserRoles.Add(OldUserRoleEnum.Mod);
                }
                else
                {
                    this.TrovoUserRoles.Remove(OldUserRoleEnum.Mod);
                }

                if (rolesSet.Contains(Trovo.Base.Models.Chat.ChatMessageModel.FollowerRole))
                {
                    this.TrovoUserRoles.Add(OldUserRoleEnum.Follower);
                }
                else
                {
                    this.TrovoUserRoles.Remove(OldUserRoleEnum.Follower);
                }

                if (rolesSet.Contains(Trovo.Base.Models.Chat.ChatMessageModel.SubscriberRole))
                {
                    this.TrovoUserRoles.Add(OldUserRoleEnum.Subscriber);
                    this.Data.TrovoSubscriberLevel = 1;
                }
                else
                {
                    this.TrovoUserRoles.Remove(OldUserRoleEnum.Subscriber);
                    this.Data.TrovoSubscriberLevel = 0;
                }

                this.IsInChat = true;
            }
        }

        #endregion Glimesh Refresh Functions

        private void SetCommonUserRoles()
        {
            if (this.UserRoles.Contains(OldUserRoleEnum.Streamer))
            {
                this.UserRoles.Add(OldUserRoleEnum.ChannelEditor);
                this.UserRoles.Add(OldUserRoleEnum.Mod);
                this.UserRoles.Add(OldUserRoleEnum.Subscriber);
                this.UserRoles.Add(OldUserRoleEnum.Follower);
            }

            if (ChannelSession.Settings.RegularUserMinimumHours > 0 && this.Data.ViewingHoursPart >= ChannelSession.Settings.RegularUserMinimumHours)
            {
                this.UserRoles.Add(OldUserRoleEnum.Regular);
            }
            else
            {
                this.UserRoles.Remove(OldUserRoleEnum.Regular);
            }

            this.ClearDisplayProperties();
        }

        private void ClearDisplayProperties()
        {
            this.Color = null;
            this.RolesString = null;
            this.RolesLocalizedString = null;
        }

        private Task RefreshExternalServiceDetails()
        {
            this.CustomRoles.Clear();
            if (ServiceManager.Get<PatreonService>().IsConnected && this.PatreonUser == null)
            {
                IEnumerable<PatreonCampaignMember> campaignMembers = ServiceManager.Get<PatreonService>().CampaignMembers;

                PatreonCampaignMember patreonUser = null;
                if (!string.IsNullOrEmpty(this.Data.PatreonUserID))
                {
                    patreonUser = campaignMembers.FirstOrDefault(u => u.UserID.Equals(this.Data.PatreonUserID));
                }
                else
                {
                    patreonUser = campaignMembers.FirstOrDefault(u => this.Platform == u.User.Platform && string.Equals(u.User.PlatformUserID, this.PlatformID, StringComparison.InvariantCultureIgnoreCase));
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
            return Task.CompletedTask;
        }
    }
}
