using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.Model.User.Platform;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.User
{
    public class UserV2ViewModel : UIViewModelBase, IEquatable<UserV2ViewModel>, IComparable<UserV2ViewModel>
    {
        public const string UserDefaultColor = "MaterialDesignBody";

        public static UserV2ViewModel CreateUnassociated(string username = null) { return new UserV2ViewModel(StreamingPlatformTypeEnum.None, UserV2Model.CreateUnassociated(username)); }

        public static void MergeUserData(UserV2ViewModel primary, UserV2ViewModel secondary)
        {
            primary.model.AddPlatformData(secondary.platformModel);
            primary.model.MergeUserData(secondary.Model);

            UserV2Model oldData = secondary.model;
            secondary.UpdateModel(primary.model);

            ServiceManager.Get<UserService>().DeleteUserData(oldData.ID);
            ServiceManager.Get<UserService>().SetUserData(primary.model);
        }

        private UserV2Model model;
        private UserPlatformV2ModelBase platformModel;

        public UserV2ViewModel(UserV2Model model) : this(StreamingPlatformTypeEnum.None, model) { }

        public UserV2ViewModel(StreamingPlatformTypeEnum platform, UserV2Model model)
        {
            this.model = model;

            if (platform != StreamingPlatformTypeEnum.None)
            {
                this.platformModel = this.GetPlatformData<UserPlatformV2ModelBase>(platform);
            }

            if (this.platformModel == null && this.HasPlatformData(ChannelSession.Settings.DefaultStreamingPlatform))
            {
                this.platformModel = this.GetPlatformData<UserPlatformV2ModelBase>(ChannelSession.Settings.DefaultStreamingPlatform);
            }

            if (this.platformModel == null && this.Model.GetPlatforms().Count > 0)
            {
                this.platformModel = this.GetPlatformData<UserPlatformV2ModelBase>(this.Model.GetPlatforms().First());
            }

            if (this.platformModel == null)
            {
                throw new InvalidOperationException($"User data does not contain any platform data - {model.ID} - {platform}");
            }

            this.ClearCachedProperties();
        }

        public UserV2Model Model { get { return this.model; } }

        public UserPlatformV2ModelBase PlatformModel { get { return this.platformModel; } }

        public Guid ID { get { return this.Model.ID; } }

        public StreamingPlatformTypeEnum Platform { get { return this.PlatformModel.Platform; } }

        public HashSet<StreamingPlatformTypeEnum> AllPlatforms { get { return this.Model.GetPlatforms(); } }

        public bool IsUnassociated { get { return this.Platform == StreamingPlatformTypeEnum.None; } }

        public string PlatformID { get { return this.PlatformModel.ID; } }

        public string Username { get { return this.PlatformModel.Username; } }

        public string DisplayName { get { return !string.IsNullOrEmpty(this.PlatformModel.DisplayName) ? this.PlatformModel.DisplayName : this.Username; } }

        public string FullDisplayName
        {
            get { return this.fullDisplayName; }
            set
            {
                this.fullDisplayName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string fullDisplayName;

        public string AvatarLink { get { return this.PlatformModel.AvatarLink; } }

        public bool ShowUserAvatar { get { return !ChannelSession.Settings.HideUserAvatar; } }

        public string LastSeenString { get { return (this.LastActivity != DateTimeOffset.MinValue) ? this.LastActivity.ToFriendlyDateTimeString() : "Unknown"; } }

        public HashSet<UserRoleEnum> Roles { get { return this.PlatformModel.Roles; } }

        public HashSet<UserRoleEnum> DisplayRoles { get; private set; } = new HashSet<UserRoleEnum>();

        public string Color
        {
            get { return this.color; }
            set
            {
                this.color = value;
                this.NotifyPropertyChanged();
            }
        }
        private string color;

        public string ColorInApp { get { return string.IsNullOrEmpty(this.Color) ? UserV2ViewModel.UserDefaultColor : this.Color; } }

        public string RolesString
        {
            get { return this.rolesString; }
            set
            {
                this.rolesString = value;
                this.NotifyPropertyChanged();
            }
        }
        private string rolesString;

        public string DisplayRolesString
        {
            get { return this.displayRolesString; }
            set
            {
                this.displayRolesString = value;
                this.NotifyPropertyChanged();
            }
        }
        private string displayRolesString;

        public UserRoleEnum PrimaryRole
        {
            get { return this.primaryRole; }
            set
            {
                this.primaryRole = value;
                this.NotifyPropertyChanged();
            }
        }
        private UserRoleEnum primaryRole;

        public string PrimaryRoleString { get { return EnumLocalizationHelper.GetLocalizedName(this.PrimaryRole); } }

        public bool IsPlatformSubscriber { get { return this.Roles.Contains(UserRoleEnum.Subscriber) || this.Roles.Contains(UserRoleEnum.YouTubeMember); } }

        public bool IsExternalSubscriber
        {
            get
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
        }

        public string AlejoPronoun { get { return ServiceManager.Get<AlejoPronounsService>().GetPronoun(this.Model.AlejoPronounID, this.Model.AlejoAltPronounID); } }

        public bool IsFollower { get { return this.HasRole(UserRoleEnum.Follower) || this.HasRole(UserRoleEnum.YouTubeSubscriber); } }
        public bool IsRegular { get { return this.HasRole(UserRoleEnum.Regular); } }
        public bool IsSubscriber { get { return this.IsPlatformSubscriber || this.IsExternalSubscriber; } }

        public string ChannelLink
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return $"https://www.twitch.tv/{this.Username}"; }
                else if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return ((YouTubeUserPlatformV2Model)this.PlatformModel).YouTubeURL; }
                else if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return $"https://trovo.live/{this.Username}"; }
                return string.Empty;
            }
        }

        public string PlatformImageURL { get { return StreamingPlatforms.GetPlatformImage(this.Platform); } }

        public bool ShowPlatformImage { get { return StreamingPlatforms.GetConnectedPlatformSessions().Count() > 1; } }

        public string PlatformBadgeLink
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return "/Assets/Images/Twitch-Small.png"; }
                else if (this.Platform == StreamingPlatformTypeEnum.YouTube) { return "/Assets/Images/YouTube.png"; }
                else if (this.Platform == StreamingPlatformTypeEnum.Trovo) { return "/Assets/Images/Trovo.png"; }
                return null;
            }
        }
        public string PlatformBadgeFullLink { get { return $"https://github.com/SaviorXTanren/mixer-mixitup/raw/master/MixItUp.WPF{this.PlatformBadgeLink}"; } }
        public bool ShowPlatformBadge { get { return true; } }

        public DateTimeOffset? AccountDate { get { return this.PlatformModel.AccountDate; } set { this.PlatformModel.AccountDate = value; } }
        public string AccountDateString { get { return (this.AccountDate != null) ? this.AccountDate.GetValueOrDefault().ToFriendlyDateString() : MixItUp.Base.Resources.Unknown; } }
        public string AccountAgeString { get { return (this.AccountDate != null) ? this.AccountDate.GetValueOrDefault().GetAge() : MixItUp.Base.Resources.Unknown; } }
        public int AccountDays { get { return (this.AccountDate != null) ? this.AccountDate.GetValueOrDefault().TotalDaysFromNow() : 0; } }

        public DateTimeOffset? FollowDate { get { return this.PlatformModel.FollowDate; } set { this.PlatformModel.FollowDate = value; } }
        public string FollowDateString { get { return (this.FollowDate != null) ? this.FollowDate.GetValueOrDefault().ToFriendlyDateString() : MixItUp.Base.Resources.NotFollowing; } }
        public string FollowAgeString { get { return (this.FollowDate != null) ? this.FollowDate.GetValueOrDefault().GetAge() : MixItUp.Base.Resources.NotFollowing; } }
        public int FollowDays { get { return (this.FollowDate != null) ? this.FollowDate.GetValueOrDefault().TotalDaysFromNow() : 0; } }
        public int FollowMonths { get { return (this.FollowDate != null) ? this.FollowDate.GetValueOrDefault().TotalMonthsFromNow() : 0; } }

        public DateTimeOffset? SubscribeDate { get { return this.PlatformModel.SubscribeDate; } set { this.PlatformModel.SubscribeDate = value; } }
        public string SubscribeDateString { get { return (this.SubscribeDate != null) ? this.SubscribeDate.GetValueOrDefault().ToFriendlyDateString() : MixItUp.Base.Resources.NotSubscribed; } }
        public string SubscribeAgeString { get { return (this.SubscribeDate != null) ? this.SubscribeDate.GetValueOrDefault().GetAge() : MixItUp.Base.Resources.NotSubscribed; } }
        public int SubscribeDays { get { return (this.SubscribeDate != null) ? this.SubscribeDate.GetValueOrDefault().TotalDaysFromNow() : 0; } }
        public int SubscribeMonths { get { return (this.SubscribeDate != null) ? this.SubscribeDate.GetValueOrDefault().TotalMonthsFromNow() : 0; } }

        public int SubscriberTier { get { return this.PlatformModel.SubscriberTier; } set { this.PlatformModel.SubscriberTier = value; } }
        public string SubscriberTierString
        {
            get
            {
                return (this.IsPlatformSubscriber) ? $"{MixItUp.Base.Resources.Tier} {this.SubscriberTier}" : MixItUp.Base.Resources.NotSubscribed;
            }
        }
        public string PlatformSubscriberBadgeLink { get { return this.PlatformModel.SubscriberBadgeLink; } }
        public bool ShowPlatformSubscriberBadge { get { return !ChannelSession.Settings.HideUserSubscriberBadge && this.IsPlatformSubscriber && !string.IsNullOrEmpty(this.PlatformSubscriberBadgeLink); } }

        public string PlatformRoleBadgeLink { get { return this.PlatformModel.RoleBadgeLink; } }
        public bool ShowPlatformRoleBadge { get { return !ChannelSession.Settings.HideUserRoleBadge && !string.IsNullOrEmpty(this.PlatformRoleBadgeLink); } }

        public string PlatformSpecialtyBadgeLink { get { return this.PlatformModel.SpecialtyBadgeLink; } }
        public bool ShowPlatformSpecialtyBadge { get { return !ChannelSession.Settings.HideUserRoleBadge && !string.IsNullOrEmpty(this.PlatformSpecialtyBadgeLink); } }

        public Dictionary<Guid, int> CurrencyAmounts { get { return this.Model.CurrencyAmounts; } }

        public Dictionary<Guid, Dictionary<Guid, int>> InventoryAmounts { get { return this.Model.InventoryAmounts; } }

        public Dictionary<Guid, int> StreamPassAmounts { get { return this.Model.StreamPassAmounts; } }

        public int OnlineViewingMinutes
        {
            get { return this.Model.OnlineViewingMinutes; }
            set
            {
                this.Model.OnlineViewingMinutes = value;
                this.NotifyPropertyChanged("OnlineViewingMinutes");
                this.NotifyPropertyChanged("OnlineViewingMinutesOnly");
                this.NotifyPropertyChanged("OnlineViewingHoursOnly");
            }
        }

        public int OnlineViewingMinutesOnly
        {
            get { return this.OnlineViewingMinutes % 60; }
            set
            {
                this.OnlineViewingMinutes = (this.OnlineViewingHoursOnly * 60) + value;
                this.NotifyPropertyChanged("OnlineViewingMinutes");
                this.NotifyPropertyChanged("OnlineViewingMinutesOnly");
                this.NotifyPropertyChanged("OnlineViewingHoursOnly");
            }
        }

        public int OnlineViewingHoursOnly
        {
            get { return this.OnlineViewingMinutes / 60; }
            set
            {
                this.OnlineViewingMinutes = value * 60 + this.OnlineViewingMinutesOnly;
                this.NotifyPropertyChanged("OnlineViewingMinutes");
                this.NotifyPropertyChanged("OnlineViewingMinutesOnly");
                this.NotifyPropertyChanged("OnlineViewingHoursOnly");
            }
        }

        public string OnlineViewingTimeString { get { return string.Format("{0} Hours & {1} Mins", this.OnlineViewingHoursOnly, this.OnlineViewingMinutesOnly); } }

        public int PrimaryCurrency
        {
            get
            {
                CurrencyModel currency = ChannelSession.Settings.Currency.Values.FirstOrDefault(c => !c.IsRank && c.IsPrimary);
                if (currency != null)
                {
                    return currency.GetAmount(this);
                }
                return 0;
            }
        }

        public int PrimaryRankPoints
        {
            get
            {

                CurrencyModel rank = ChannelSession.Settings.Currency.Values.FirstOrDefault(c => c.IsRank && c.IsPrimary);
                if (rank != null)
                {
                    return rank.GetAmount(this);
                }
                return 0;
            }
        }

        public string PrimaryRankNameAndPoints
        {
            get
            {
                CurrencyModel rank = ChannelSession.Settings.Currency.Values.FirstOrDefault(c => c.IsRank && c.IsPrimary);
                if (rank != null)
                {
                    return string.Format("{0} - {1}", rank.Name, rank.GetAmount(this));
                }

                return string.Empty;
            }
        }

        public long TotalStreamsWatched
        {
            get { return this.Model.TotalStreamsWatched; }
            set { this.Model.TotalStreamsWatched = value; }
        }

        public double TotalAmountDonated
        {
            get { return this.Model.TotalAmountDonated; }
            set { this.Model.TotalAmountDonated = value; }
        }

        public long TotalSubsGifted
        {
            get { return this.Model.TotalSubsGifted; }
            set { this.Model.TotalSubsGifted = value; }
        }

        public long TotalSubsReceived
        {
            get { return this.Model.TotalSubsReceived; }
            set { this.Model.TotalSubsReceived = value; }
        }

        public long TotalChatMessageSent
        {
            get { return this.Model.TotalChatMessageSent; }
            set { this.Model.TotalChatMessageSent = value; }
        }

        public long TotalTimesTagged
        {
            get { return this.Model.TotalTimesTagged; }
            set { this.Model.TotalTimesTagged = value; }
        }

        public long TotalCommandsRun
        {
            get { return this.Model.TotalCommandsRun; }
            set { this.Model.TotalCommandsRun = value; }
        }

        public long TotalMonthsSubbed
        {
            get { return this.Model.TotalMonthsSubbed; }
            set { this.Model.TotalMonthsSubbed = value; }
        }

        public uint ModerationStrikes
        {
            get { return this.Model.ModerationStrikes; }
            set { this.Model.ModerationStrikes = value; }
        }

        public bool IsSpecialtyExcluded
        {
            get { return this.Model.IsSpecialtyExcluded; }
            set { this.Model.IsSpecialtyExcluded = value; }
        }

        public CommandModelBase EntranceCommand
        {
            get { return ChannelSession.Settings.GetCommand(this.Model.EntranceCommandID); }
            set { this.Model.EntranceCommandID = (value != null) ? value.ID : Guid.Empty; }
        }

        public string Title
        {
            get
            {
                if (!string.IsNullOrEmpty(this.CustomTitle))
                {
                    return this.CustomTitle;
                }

                UserTitleModel title = ChannelSession.Settings.UserTitles.OrderByDescending(t => t.UserRole).ThenByDescending(t => t.Months).FirstOrDefault(t => t.MeetsTitle(this));
                if (title != null)
                {
                    return title.Name;
                }

                return MixItUp.Base.Resources.NoTitle;
            }
        }

        public string CustomTitle
        {
            get { return this.Model.CustomTitle; }
            set { this.Model.CustomTitle = value; }
        }

        public Guid EntranceCommandID
        {
            get { return this.Model.EntranceCommandID; }
            set { this.Model.EntranceCommandID = value; }
        }

        public List<Guid> CustomCommandIDs { get { return this.Model.CustomCommandIDs; } }

        public string Notes
        {
            get { return this.Model.Notes; }
            set { this.Model.Notes = value; }
        }

        public DateTimeOffset LastActivity { get { return this.Model.LastActivity; } }
        public string LastActivityDateString { get { return this.LastActivity.ToFriendlyDateString(); } }
        public string LastActivityAgeString { get { return this.LastActivity.GetAge(); } }
        public int LastActivityDays { get { return this.LastActivity.TotalDaysFromNow(); } }

        public DateTimeOffset LastUpdated { get; private set; }

        public bool IsInChat { get; set; }

        public string SortableID { get; private set; }

        public int WhispererNumber { get; set; }

        public bool HasWhisperNumber { get { return this.WhispererNumber > 0; } }

        public string PatreonID { get { return this.Model.PatreonUserID; } }

        public PatreonCampaignMember PatreonUser
        {
            get
            {
                if (this.patreonUser != null)
                {
                    return this.patreonUser;
                }

                if (!string.IsNullOrEmpty(this.Model.PatreonUserID) && ServiceManager.Get<PatreonService>().IsConnected)
                {
                    this.patreonUser = ServiceManager.Get<PatreonService>().CampaignMembers.FirstOrDefault(m => this.Model.PatreonUserID.Equals(m.ID));
                }

                return this.patreonUser;
            }
            set
            {
                this.patreonUser = value;
                if (this.patreonUser != null)
                {
                    this.Model.PatreonUserID = this.patreonUser.ID;
                }
                else
                {
                    this.model.PatreonUserID = null;
                }
            }
        }
        private PatreonCampaignMember patreonUser;

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

        public void UpdateLastActivity() { this.Model.LastActivity = DateTimeOffset.Now; }

        public void UpdateViewingMinutes(Dictionary<StreamingPlatformTypeEnum, bool> liveStreams)
        {
            this.OnlineViewingMinutes++;
            ChannelSession.Settings.Users.ManualValueChanged(this.ID);

            if (ChannelSession.Settings.RegularUserMinimumHours > 0 && this.OnlineViewingHoursOnly >= ChannelSession.Settings.RegularUserMinimumHours)
            {
                this.Roles.Add(UserRoleEnum.Regular);
            }
            else
            {
                this.Roles.Remove(UserRoleEnum.Regular);
            }
        }

        public bool HasRole(UserRoleEnum role) { return this.Roles.Contains(role); }

        public bool MeetsRole(UserRoleEnum role)
        {
            if ((role == UserRoleEnum.Subscriber || role == UserRoleEnum.YouTubeMember) && this.IsSubscriber)
            {
                return true;
            }

            if (ChannelSession.Settings.ExplicitUserRoleRequirements)
            {
                Logger.Log($"Perform explicit user role check: {role}");
                return this.HasRole(role);
            }

            Logger.Log($"Perform regular user role check: {this.PrimaryRole} >= {role}");
            return this.PrimaryRole >= role;
        }

        public bool ExceedRole(UserRoleEnum role) { return this.PrimaryRole > role; }

        public async Task AddModerationStrike(string moderationReason = null)
        {
            Dictionary<string, string> extraSpecialIdentifiers = new Dictionary<string, string>();
            extraSpecialIdentifiers.Add(ModerationService.ModerationReasonSpecialIdentifier, moderationReason);

            this.ModerationStrikes++;
            if (this.ModerationStrikes == 1)
            {
                await ServiceManager.Get<CommandService>().Queue(ChannelSession.Settings.ModerationStrike1CommandID, new CommandParametersModel(this, extraSpecialIdentifiers));
            }
            else if (this.ModerationStrikes == 2)
            {
                await ServiceManager.Get<CommandService>().Queue(ChannelSession.Settings.ModerationStrike2CommandID, new CommandParametersModel(this, extraSpecialIdentifiers));
            }
            else if (this.ModerationStrikes >= 3)
            {
                await ServiceManager.Get<CommandService>().Queue(ChannelSession.Settings.ModerationStrike3CommandID, new CommandParametersModel(this, extraSpecialIdentifiers));
            }
        }

        public void RemoveModerationStrike()
        {
            if (this.ModerationStrikes > 0)
            {
                this.ModerationStrikes--;
            }
        }

        public bool HasPlatformData(StreamingPlatformTypeEnum platform) { return this.Model.HasPlatformData(platform); }

        public T GetPlatformData<T>(StreamingPlatformTypeEnum platform) where T : UserPlatformV2ModelBase { return this.Model.GetPlatformData<T>(platform); }

        public async Task Refresh(bool force = false)
        {
            if (!this.IsUnassociated)
            {
                TimeSpan lastUpdatedTimeSpan = DateTimeOffset.Now - this.LastUpdated;
                if (force || lastUpdatedTimeSpan > this.PlatformModel.RefreshTimeSpan)
                {
                    this.LastUpdated = DateTimeOffset.Now;

                    DateTimeOffset refreshStart = DateTimeOffset.Now;
                    Logger.Log($"User refresh started: {this.ID}");

                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                    Task refreshTask = Task.Run(async () =>
                    {
                        try
                        {
                            await this.platformModel.Refresh();

                            double platformRefreshTime = (DateTimeOffset.Now - refreshStart).TotalMilliseconds;
                            if (platformRefreshTime > 1000)
                            {
                                Logger.Log(LogLevel.Error, string.Format("Long user refresh time detected for the following user (Platform refresh): {0} - {1} - {2} ms", this.ID, this.Username, platformRefreshTime));
                            }

                            if (ChannelSession.Settings.ShowAlejoPronouns && this.Platform == StreamingPlatformTypeEnum.Twitch)
                            {
                                AlejoUserPronoun pronouns = await ServiceManager.Get<AlejoPronounsService>().GetPronounData(this.Username);
                                if (pronouns != null)
                                {
                                    this.Model.AlejoPronounID = pronouns.pronoun_id;
                                    this.Model.AlejoAltPronounID = pronouns.alt_pronoun_id;
                                }
                            }

                            this.RefreshPatreonProperties();

                            double externalRefreshTime = (DateTimeOffset.Now - refreshStart).TotalMilliseconds;
                            if (externalRefreshTime > 1000)
                            {
                                Logger.Log(LogLevel.Error, string.Format("Long user refresh time detected for the following user (External refresh): {0} - {1} - {2} ms", this.ID, this.Username, externalRefreshTime));
                            }

                            this.ClearCachedProperties();
                            double cacheRefreshTime = (DateTimeOffset.Now - refreshStart).TotalMilliseconds;
                            if (cacheRefreshTime > 1000)
                            {
                                Logger.Log(LogLevel.Error, string.Format("Long user refresh time detected for the following user (Cache refresh): {0} - {1} - {2} ms", this.ID, this.Username, cacheRefreshTime));
                            }
                        }
                        catch (TaskCanceledException)
                        {
                            Logger.Log(LogLevel.Error, "Refresh task cancelled due to taking too long to process, resetting last updated state and trying again later");
                            this.LastUpdated = DateTimeOffset.MinValue;
                        }
                    }, cancellationTokenSource.Token);

                    await Task.WhenAny(new Task[] { refreshTask, Task.Delay(2000) });
                    if (!refreshTask.IsCompleted)
                    {
                        cancellationTokenSource.Cancel();
                    }

                    double refreshTime = (DateTimeOffset.Now - refreshStart).TotalMilliseconds;
                    Logger.Log($"User refresh time: {this.ID} - {refreshTime} ms");
                    if (refreshTime > 1000)
                    {
                        Logger.Log(LogLevel.Error, string.Format("Long user refresh time detected for the following user: {0} - {1} - {2} ms", this.ID, this.Username, refreshTime));
                    }
                }
            }
        }

        public void RefreshPatreonProperties()
        {
            if (ServiceManager.Get<PatreonService>().IsConnected && this.PatreonUser == null)
            {
                IEnumerable<PatreonCampaignMember> campaignMembers = ServiceManager.Get<PatreonService>().CampaignMembers;

                if (!string.IsNullOrEmpty(this.model.PatreonUserID))
                {
                    this.PatreonUser = campaignMembers.FirstOrDefault(u => u.UserID.Equals(this.model.PatreonUserID));
                }
                else
                {
                    this.PatreonUser = campaignMembers.FirstOrDefault(u => this.Platform == u.User.Platform && string.Equals(u.User.PlatformUserID, this.PlatformID, StringComparison.InvariantCultureIgnoreCase));
                }

                if (this.PatreonUser != null)
                {
                    this.model.PatreonUserID = this.PatreonUser.UserID;
                }
                else
                {
                    this.model.PatreonUserID = null;
                }
            }
        }

        public void MergeUserData(UserImportModel import)
        {
            this.OnlineViewingMinutes += import.OnlineViewingMinutes;
            foreach (var kvp in import.CurrencyAmounts)
            {
                if (!this.CurrencyAmounts.ContainsKey(kvp.Key))
                {
                    this.CurrencyAmounts[kvp.Key] = 0;
                }
                this.CurrencyAmounts[kvp.Key] += kvp.Value;
            }
        }

        private void ClearCachedProperties()
        {
            if (!string.IsNullOrEmpty(this.PlatformModel.DisplayName))
            {
                if (!string.Equals(this.DisplayName, this.Username, StringComparison.OrdinalIgnoreCase))
                {
                    this.FullDisplayName = $"{this.DisplayName} ({this.Username})";
                }
                else
                {
                    this.FullDisplayName = this.DisplayName;
                }
            }
            else
            {
                this.FullDisplayName = this.Username;
            }

            this.PrimaryRole = this.Roles.Max();

            this.RolesString = string.Join(", ", this.Roles.OrderBy(r => r).Select(r => r.ToString()));

            var displayRoles = new HashSet<UserRoleEnum>(this.Roles);
            if (displayRoles.Count > 1)
            {
                displayRoles.Remove(UserRoleEnum.User);
            }
            if (displayRoles.Contains(UserRoleEnum.Subscriber) || displayRoles.Contains(UserRoleEnum.YouTubeSubscriber))
            {
                displayRoles.Remove(UserRoleEnum.Follower);
            }
            if (displayRoles.Contains(UserRoleEnum.YouTubeMember))
            {
                displayRoles.Remove(UserRoleEnum.Subscriber);
            }
            if (displayRoles.Contains(UserRoleEnum.TrovoSuperMod))
            {
                displayRoles.Remove(UserRoleEnum.Moderator);
            }
            if (displayRoles.Contains(UserRoleEnum.Streamer))
            {
                displayRoles.Remove(UserRoleEnum.Subscriber);
            }
            displayRoles.Remove(UserRoleEnum.TwitchAffiliate);
            displayRoles.Remove(UserRoleEnum.TwitchPartner);
            this.DisplayRoles = displayRoles;

            var sortedRoles = this.DisplayRoles.OrderByDescending(r => r);
            this.DisplayRolesString = string.Join(", ", sortedRoles.Select(r => EnumLocalizationHelper.GetLocalizedName(r)));

            if (ChannelSession.Settings.UseCustomUsernameColors)
            {
                foreach (UserRoleEnum role in this.Roles.OrderByDescending(r => r))
                {
                    if (ChannelSession.Settings.CustomUsernameRoleColors.ContainsKey(role))
                    {
                        string name = ChannelSession.Settings.CustomUsernameRoleColors[role];
                        if (ColorSchemes.MaterialDesignColors.ContainsKey(name))
                        {
                            this.Color = ColorSchemes.MaterialDesignColors[name];
                            break;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(this.Color))
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch)
                {
                    this.Color = ((TwitchUserPlatformV2Model)this.PlatformModel).Color;
                }
            }

            var sortRole = this.PrimaryRole;
            if (sortRole < UserRoleEnum.Subscriber)
            {
                sortRole = UserRoleEnum.User;
            }
            this.SortableID = (99999 - sortRole) + "-" + this.Username + "-" + this.Platform.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is UserV2ViewModel)
            {
                return this.Equals((UserV2ViewModel)obj);
            }
            return false;
        }

        public bool Equals(UserV2ViewModel other)
        {
            return this.ID.Equals(other.ID);
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }

        public int CompareTo(UserV2ViewModel other) { return this.SortableID.CompareTo(other.SortableID); }

        public override string ToString() { return this.Username; }

        private void UpdateModel(UserV2Model model)
        {
            this.model = model;
        }
    }
}
