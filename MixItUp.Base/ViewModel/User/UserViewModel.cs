using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Chat;
using Mixer.Base.Model.MixPlay;
using Mixer.Base.Model.User;
using MixItUp.Base.Model;
using MixItUp.Base.Model.MixPlay;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.User
{
    public enum UserRoleEnum
    {
        Banned,
        User = 10,
        Pro = 20,
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

    public class UserViewModel : IEquatable<UserViewModel>, IComparable<UserViewModel>
    {
        public const string MixerUserDefaultAvatarLink = "https://mixer.com/_latest/assets/images/main/avatars/default.png";
        public const string MixerUserAvatarLinkFormat = "https://mixer.com/api/v1/users/{0}/avatar?w=128&h=128";

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

        public UserViewModel(string username)
            : this(mixerID: 0)
        {
            this.MixerUsername = this.UnassociatedUsername = username;
        }

        public UserViewModel(UserModel user)
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
            this.SetMixerUserRoles(user.userRoles);

            this.IsInChat = true;
        }

        public UserViewModel(ChatMessageEventModel messageEvent)
            : this(mixerID: messageEvent.user_id)
        {
            this.MixerID = messageEvent.user_id;
            this.MixerUsername = messageEvent.user_name;
            this.SetMixerUserRoles(messageEvent.user_roles);

            this.IsInChat = true;
        }

        public UserViewModel(ChatMessageUserModel chatUser)
            : this(mixerID: chatUser.user_id)
        {
            this.MixerID = chatUser.user_id;
            this.MixerUsername = chatUser.user_name;
            this.SetMixerUserRoles(chatUser.user_roles);

            this.IsInChat = true;
        }

        public UserViewModel(MixPlayParticipantModel participant)
            : this(mixerID: participant.userID)
        {
            this.MixerID = participant.userID;
            this.MixerUsername = participant.username;

            this.SetMixerMixPlayDetails(participant);
        }

        public UserViewModel(UserDataModel userData)
        {
            this.Data = userData;
        }

        private UserViewModel(uint mixerID = 0)
        {
            if (mixerID > 0)
            {
                this.Data = ChannelSession.Settings.GetUserDataByMixerID(mixerID);
            }
            else
            {
                this.Data = new UserDataModel();
            }
        }

        public Guid ID { get { return this.Data.ID; } }

        public StreamingPlatformTypeEnum Platform
        {
            get
            {
                if (this.MixerID > 0 || this.InteractiveIDs.Count > 0) { return StreamingPlatformTypeEnum.Mixer; }
                return StreamingPlatformTypeEnum.None;
            }
        }

        public string PlatformID
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Mixer) { return this.MixerID.ToString(); }
                return null;
            }
        }

        public string Username
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Mixer) { return this.Data.MixerUsername; }
                return this.UnassociatedUsername;
            }
        }

        public HashSet<UserRoleEnum> UserRoles
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Mixer) { return this.Data.MixerUserRoles; }
                return new HashSet<UserRoleEnum>() { UserRoleEnum.User };
            }
        }

        public string AvatarLink
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Mixer) { return this.MixerAvatarLink; }
                return string.Empty;
            }
        }

        public string ChannelLink
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Mixer) { return $"https://www.mixer.com/{this.Username}"; }
                return string.Empty;
            }
        }

        public DateTimeOffset? AccountDate
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Mixer) { return this.Data.MixerAccountDate; }
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
                return null;
            }
            set
            {
                if (this.Platform == StreamingPlatformTypeEnum.Mixer) { this.Data.MixerFollowDate = value; }

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
                if (this.Platform == StreamingPlatformTypeEnum.Mixer) { return this.Data.MixerSubscribeDate; }
                return null;
            }
            set
            {
                if (this.Platform == StreamingPlatformTypeEnum.Mixer) { this.Data.MixerSubscribeDate = value; }
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

        public string UnassociatedUsername { get { return this.Data.UnassociatedUsername; } private set { this.Data.UnassociatedUsername = value; } }

        #region Mixer

        public uint MixerID { get { return this.Data.MixerID; } private set { if (value > 0) { this.Data.MixerID = value; } } }
        public string MixerUsername { get { return this.Data.MixerUsername; } private set { if (!string.IsNullOrEmpty(value)) { this.Data.MixerUsername = value; } } }
        public uint MixerChannelID { get { return this.Data.MixerChannelID; } private set { if (value > 0) { this.Data.MixerChannelID = value; } } }

        public UserFanProgressionModel MixerFanProgression { get { return this.Data.MixerFanProgression; } set { this.Data.MixerFanProgression = value; } }
        public int Sparks { get { return this.Data.Sparks; } set { this.Data.Sparks = value; } }

        public uint CurrentViewerCount { get { return this.Data.CurrentViewerCount; } set { this.Data.CurrentViewerCount = value; } }

        public LockedDictionary<string, MixPlayParticipantModel> InteractiveIDs { get { return this.Data.InteractiveIDs; } set { this.Data.InteractiveIDs = value; } }

        public string InteractiveGroupID { get { return this.Data.InteractiveGroupID; } set { this.Data.InteractiveGroupID = value; } }

        public bool IsInInteractiveTimeout { get { return this.Data.IsInInteractiveTimeout; } set { this.Data.IsInInteractiveTimeout = value; } }

        public bool IsAnonymous { get { return this.MixerID == 0 || this.InteractiveIDs.Values.Any(i => i.anonymous.GetValueOrDefault()); } }

        public string MixerAvatarLink { get { return string.Format(MixerUserAvatarLinkFormat, this.MixerID); } }

        public string MixerChannelBadgeLink { get { return this.MixerFanProgression?.level?.SmallAssetURL?.ToString(); } }

        public bool HasMixerChannelBadgeLink { get { return !string.IsNullOrEmpty(this.MixerChannelBadgeLink); } }

        #endregion Mixer

        public DateTimeOffset LastUpdated { get { return this.Data.LastUpdated; } set { this.Data.LastUpdated = value; } }

        public DateTimeOffset LastActivity { get { return this.Data.LastActivity; } set { this.Data.LastActivity = value; } }

        public HashSet<string> CustomRoles { get { return this.Data.CustomRoles; } set { this.Data.CustomRoles = value; } }

        public bool IgnoreForQueries { get { return this.Data.IgnoreForQueries; } set { this.Data.IgnoreForQueries = value; } }

        public bool IsInChat { get { return this.Data.IsInChat; } set { this.Data.IsInChat = value; } }

        public string TwitterURL { get { return this.Data.TwitterURL; } set { this.Data.TwitterURL = value; } }

        public PatreonCampaignMember PatreonUser { get { return this.Data.PatreonUser; } set { this.Data.PatreonUser = value; } }

        public UserRoleEnum PrimaryRole { get { return (this.UserRoles.Count() > 0) ? this.UserRoles.Max() : UserRoleEnum.User; } }

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
                        if (this.Data.MixerUserRoles.Contains(UserRoleEnum.Banned))
                        {
                            userRoles.Clear();
                            userRoles.Add(UserRoleEnum.Banned);
                        }
                        else
                        {
                            if (this.Data.MixerUserRoles.Count() > 1)
                            {
                                userRoles.Remove(UserRoleEnum.User);
                            }

                            if (userRoles.Contains(UserRoleEnum.ChannelEditor))
                            {
                                userRoles.Remove(UserRoleEnum.Mod);
                            }

                            if (this.Data.MixerUserRoles.Contains(UserRoleEnum.Subscriber) || this.Data.MixerUserRoles.Contains(UserRoleEnum.Streamer))
                            {
                                userRoles.Remove(UserRoleEnum.Follower);
                            }

                            if (this.Data.MixerUserRoles.Contains(UserRoleEnum.Streamer))
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
        }
        private object rolesDisplayStringLock = new object();

        public bool IsFollower { get { return this.UserRoles.Contains(UserRoleEnum.Follower) || this.HasPermissionsTo(UserRoleEnum.Subscriber); } }
        public bool IsRegular { get { return this.UserRoles.Contains(UserRoleEnum.Regular); } }
        public bool IsPlatformSubscriber { get { return this.UserRoles.Contains(UserRoleEnum.Subscriber); } }
        public bool ShowSubscriberBadge { get { return this.IsPlatformSubscriber && !string.IsNullOrEmpty(this.SubscriberBadgeLink); } }

        public string AccountAgeString { get { return (this.AccountDate != null) ? this.AccountDate.GetValueOrDefault().GetAge() : "Unknown"; } }
        public string FollowAgeString { get { return (this.FollowDate != null) ? this.FollowDate.GetValueOrDefault().GetAge() : "Not Following"; } }
        public string SubscribeAgeString { get { return (this.SubscribeDate != null) ? this.SubscribeDate.GetValueOrDefault().GetAge() : "Not Subscribed"; } }
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

        public int WhispererNumber { get { return this.Data.WhispererNumber; } set { this.Data.WhispererNumber = value; } }
        public bool HasWhisperNumber { get { return this.WhispererNumber > 0; } }

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

        public bool IsMixerMixPlayParticipant { get { return this.InteractiveIDs.Count > 0; } }

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
            Logger.Log($"Checking role permission for user: {this.PrimaryRole} - {checkRole}");

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
            if (!this.IsAnonymous)
            {
                if (!this.Data.UpdatedThisSession || force)
                {
                    DateTimeOffset refreshStart = DateTimeOffset.Now;

                    this.Data.UpdatedThisSession = true;
                    this.LastUpdated = DateTimeOffset.Now;

                    if (this.Platform.HasFlag(StreamingPlatformTypeEnum.Mixer))
                    {
                        await this.RefreshMixerUserDetails();
                        await this.RefreshMixerUserFanProgression();
                        await this.RefreshMixerUserFollowDate();

                        if (!this.IsInChat)
                        {
                            await this.RefreshMixerChatDetails();
                        }

                        if (this.IsPlatformSubscriber)
                        {
                            await this.RefreshMixerSubscriberDetails();
                        }
                        else
                        {
                            this.SubscribeDate = null;
                        }
                    }

                    this.SetCommonUserRoles();

                    await this.RefreshExternalServiceDetails();

                    double refreshTime = (DateTimeOffset.Now - refreshStart).TotalMilliseconds;
                    Logger.Log($"User refresh time: {refreshTime} ms");
                    if (refreshTime > 500)
                    {
                        Logger.Log(LogLevel.Error, string.Format("Long user rfresh time detected for the following user: {0} - {1} - {2} ms", this.ID, this.Username, refreshTime));
                    }
                }
            }
        }

        public void SetMixerChatDetails(ChatUserModel chatUser)
        {
            if (chatUser != null)
            {
                this.SetMixerUserRoles(chatUser.userRoles);
                this.IsInChat = true;
            }
        }

        public void RemoveMixerChatDetails(ChatUserModel chatUser)
        {
            this.IsInChat = false;
        }

        public void SetMixerMixPlayDetails(MixPlayParticipantModel participant)
        {
            this.InteractiveIDs[participant.sessionID] = participant;
            this.InteractiveGroupID = participant.groupID;
        }

        public void RemoveMixerMixPlayDetails(MixPlayParticipantModel participant)
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
            extraSpecialIdentifiers.Add(ModerationService.ModerationReasonSpecialIdentifier, moderationReason);

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
            else
            {
                this.UserRoles.Remove(UserRoleEnum.Regular);
            }
        }

        public UserModel GetMixerUserModel()
        {
            return new UserModel()
            {
                id = this.MixerID,
                username = this.MixerUsername,
            };
        }

        public ChatUserModel GetMixerUserChatModel()
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

        #region Mixer Refresh

        private async Task RefreshMixerUserDetails()
        {
            UserWithChannelModel user = await ChannelSession.MixerUserConnection.GetUser(this.MixerID);
            if (user != null)
            {
                this.SetMixerUserDetails(user);
            }
        }

        private async Task RefreshMixerUserFanProgression() { this.MixerFanProgression = await ChannelSession.MixerUserConnection.GetUserFanProgression(ChannelSession.MixerChannel, this.GetMixerUserModel()); }

        private async Task RefreshMixerUserFollowDate() { this.FollowDate = await ChannelSession.MixerUserConnection.CheckIfFollows(ChannelSession.MixerChannel, this.GetMixerUserModel()); }

        private async Task RefreshMixerChatDetails()
        {
            ChatUserModel chatUser = await ChannelSession.MixerUserConnection.GetChatUser(ChannelSession.MixerChannel, this.MixerID);
            if (chatUser != null)
            {
                this.SetMixerChatDetails(chatUser);
            }
        }

        private async Task RefreshMixerSubscriberDetails()
        {
            DateTimeOffset subDate = DateTimeOffset.MinValue;
            UserWithGroupsModel userGroups = await ChannelSession.MixerUserConnection.GetUserInChannel(ChannelSession.MixerChannel, this.MixerID);
            if (userGroups != null)
            {
                subDate = userGroups.GetSubscriberDate().GetValueOrDefault();
                if (subDate > DateTimeOffset.MinValue)
                {
                    this.SubscribeDate = subDate;
                    int totalMonths = this.SubscribeDate.GetValueOrDefault().TotalMonthsFromNow();
                    if (this.Data.TotalMonthsSubbed < totalMonths)
                    {
                        this.Data.TotalMonthsSubbed = (uint)totalMonths;
                    }
                }
            }

            if (subDate == DateTimeOffset.MinValue)
            {
                this.SubscribeDate = null;
            }
        }

        private void SetMixerUserDetails(UserModel user)
        {
            if (user.createdAt.GetValueOrDefault() > DateTimeOffset.MinValue)
            {
                this.AccountDate = user.createdAt;
            }
            this.MixerID = user.id;
            this.MixerUsername = user.username;
            this.Sparks = (int)user.sparks;
            this.TwitterURL = user.social?.twitter;
            if (user is UserWithChannelModel)
            {
                UserWithChannelModel userChannel = (UserWithChannelModel)user;
                this.MixerChannelID = userChannel.channel.id;
                this.CurrentViewerCount = userChannel.channel.viewersCurrent;
            }
        }

        private void SetMixerUserRoles(string[] userRoles)
        {
            HashSet<string> roles = new HashSet<string>(userRoles);
            if (userRoles != null && userRoles.Length > 0)
            {
                if (roles.Contains("Owner")) { this.UserRoles.Add(UserRoleEnum.Streamer); } else { this.UserRoles.Remove(UserRoleEnum.Streamer); }
                if (roles.Contains("Staff")) { this.UserRoles.Add(UserRoleEnum.Staff); } else { this.UserRoles.Remove(UserRoleEnum.Staff); }
                if (roles.Contains("ChannelEditor")) { this.UserRoles.Add(UserRoleEnum.ChannelEditor); } else { this.UserRoles.Remove(UserRoleEnum.ChannelEditor); }
                if (roles.Contains("Mod")) { this.UserRoles.Add(UserRoleEnum.Mod); } else { this.UserRoles.Remove(UserRoleEnum.Mod); }
                if (roles.Contains("GlobalMod")) { this.UserRoles.Add(UserRoleEnum.GlobalMod); } else { this.UserRoles.Remove(UserRoleEnum.GlobalMod); }
                if (roles.Contains("Subscriber")) { this.UserRoles.Add(UserRoleEnum.Subscriber); } else { this.UserRoles.Remove(UserRoleEnum.Subscriber); }
                if (roles.Contains("Partner") || roles.Contains("VerifiedPartner")) { this.UserRoles.Add(UserRoleEnum.Partner); } else { this.UserRoles.Remove(UserRoleEnum.Partner); }
                if (roles.Contains("Pro")) { this.UserRoles.Add(UserRoleEnum.Pro); } else { this.UserRoles.Remove(UserRoleEnum.Pro); }
                if (roles.Contains("Banned")) { this.UserRoles.Add(UserRoleEnum.Banned); } else { this.UserRoles.Remove(UserRoleEnum.Banned); }
            }

            if (ChannelSession.MixerChannel != null && ChannelSession.MixerChannel.user.id.Equals(this.MixerID))
            {
                this.UserRoles.Add(UserRoleEnum.Streamer);
            }
        }

        #endregion Mixer Refresh

        private void SetCommonUserRoles()
        {
            if (this.UserRoles.Contains(UserRoleEnum.Streamer))
            {
                this.UserRoles.Add(UserRoleEnum.ChannelEditor);
                this.UserRoles.Add(UserRoleEnum.Mod);
                this.UserRoles.Add(UserRoleEnum.Subscriber);
                this.UserRoles.Add(UserRoleEnum.Follower);
            }

            if (this.UserRoles.Contains(UserRoleEnum.ChannelEditor))
            {
                this.UserRoles.Add(UserRoleEnum.Mod);
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
            this.Data.RolesDisplayString = null;
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
            return Task.FromResult(0);
        }
    }
}
