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

        public Guid ID { get { return this.Data.ID; } }

        public string Username
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Mixer) { return this.Data.MixerUsername; }
                return this.unassociatedUsername;
            }
        }
        private string unassociatedUsername;

        public StreamingPlatformTypeEnum Platform
        {
            get
            {
                if (this.MixerID > 0 || this.InteractiveIDs.Count > 0) { return StreamingPlatformTypeEnum.Mixer; }
                return StreamingPlatformTypeEnum.None;
            }
        }

        public HashSet<UserRoleEnum> UserRoles
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Mixer) { return this.MixerUserRoles; }
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

        public bool IsAnonymous { get { return this.Data.MixerID == 0 || this.InteractiveIDs.Values.Any(i => i.anonymous.GetValueOrDefault()); } }

        public string MixerAvatarLink { get { return string.Format(MixerUserAvatarLinkFormat, this.Data.MixerID); } }

        public string MixerChannelBadgeLink { get { return this.MixerFanProgression?.level?.SmallAssetURL?.ToString(); } }

        public bool HasMixerChannelBadgeLink { get { return !string.IsNullOrEmpty(this.MixerChannelBadgeLink); } }

        #endregion Mixer

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

        public bool IsRegular { get { return this.UserRoles.Contains(UserRoleEnum.Regular); } }

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

                    this.FollowDate = await ChannelSession.MixerUserConnection.CheckIfFollows(ChannelSession.MixerChannel, this.GetModel());

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

                await this.SetCustomRoles();

                this.Data.LastUpdated = DateTimeOffset.Now;
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

        public UserModel GetModel()
        {
            return new UserModel()
            {
                id = this.MixerID,
                username = this.MixerUsername,
            };
        }

        public ChatUserModel GetChatModel()
        {
            return new ChatUserModel()
            {
                userId = this.MixerID,
                userName = this.MixerUsername,
                userRoles = this.UserRoles.Select(r => r.ToString()).ToArray(),
            };
        }

        public IEnumerable<MixPlayParticipantModel> GetParticipantModels()
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

        private void SetMixerUserDetails(UserModel user)
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

            if (this.FollowDate != null && this.FollowDate.GetValueOrDefault() > DateTimeOffset.MinValue)
            {
                this.MixerUserRoles.Add(UserRoleEnum.Follower);
            }

            if (this.MixerUserRoles.Contains(UserRoleEnum.Streamer))
            {
                this.MixerUserRoles.Add(UserRoleEnum.ChannelEditor);
                this.MixerUserRoles.Add(UserRoleEnum.Mod);
                this.MixerUserRoles.Add(UserRoleEnum.Subscriber);
                this.MixerUserRoles.Add(UserRoleEnum.Follower);
            }

            if (this.MixerUserRoles.Contains(UserRoleEnum.ChannelEditor))
            {
                this.MixerUserRoles.Add(UserRoleEnum.Mod);
            }

            if (ChannelSession.Settings.RegularUserMinimumHours > 0 && this.Data.ViewingHoursPart >= ChannelSession.Settings.RegularUserMinimumHours)
            {
                this.MixerUserRoles.Add(UserRoleEnum.Regular);
            }

            List<UserRoleEnum> mixerDisplayRoles = this.MixerUserRoles.ToList();
            if (this.MixerUserRoles.Contains(UserRoleEnum.Banned))
            {
                mixerDisplayRoles.Clear();
                mixerDisplayRoles.Add(UserRoleEnum.Banned);
            }
            else
            {
                if (this.MixerUserRoles.Count() > 1)
                {
                    mixerDisplayRoles.Remove(UserRoleEnum.User);
                }

                if (mixerDisplayRoles.Contains(UserRoleEnum.ChannelEditor))
                {
                    mixerDisplayRoles.Remove(UserRoleEnum.Mod);
                }

                if (this.MixerUserRoles.Contains(UserRoleEnum.Subscriber) || this.MixerUserRoles.Contains(UserRoleEnum.Streamer))
                {
                    mixerDisplayRoles.Remove(UserRoleEnum.Follower);
                }

                if (this.MixerUserRoles.Contains(UserRoleEnum.Streamer))
                {
                    mixerDisplayRoles.Remove(UserRoleEnum.ChannelEditor);
                    mixerDisplayRoles.Remove(UserRoleEnum.Subscriber);
                }
            }

            List<string> displayRoles = new List<string>(mixerDisplayRoles.Select(r => EnumLocalizationHelper.GetLocalizedName(r)));
            displayRoles.AddRange(this.CustomRoles);

            this.RolesDisplayString = string.Join(", ", displayRoles.OrderByDescending(r => r));
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
