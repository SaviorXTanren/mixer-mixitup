using Mixer.Base.Model.Channel;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Favorites;
using MixItUp.Base.Model.MixPlay;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Remote.Authentication;
using MixItUp.Base.Model.Serial;
using MixItUp.Base.Model.User;
using MixItUp.Base.Remote.Models;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModel.Window.Dashboard;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Model.OAuth;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Settings
{
    [DataContract]
    public class SettingsV2Model
    {
        public const int LatestVersion = 39;

        [JsonProperty]
        public int Version { get; set; } = SettingsV2Model.LatestVersion;

        [JsonProperty]
        public bool LicenseAccepted { get; set; }

        [JsonProperty]
        public bool OptOutTracking { get; set; }

        [JsonProperty]
        public bool ReRunWizard { get; set; }
        [JsonProperty]
        public bool DiagnosticLogging { get; set; }

        [JsonProperty]
        public bool IsStreamer { get; set; }

        [JsonProperty]
        public OAuthTokenModel OAuthToken { get; set; }
        [JsonProperty]
        public OAuthTokenModel BotOAuthToken { get; set; }

        [JsonProperty]
        public OAuthTokenModel StreamlabsOAuthToken { get; set; }
        [JsonProperty]
        public OAuthTokenModel TwitterOAuthToken { get; set; }
        [JsonProperty]
        public OAuthTokenModel DiscordOAuthToken { get; set; }
        [JsonProperty]
        public OAuthTokenModel TiltifyOAuthToken { get; set; }
        [JsonProperty]
        public OAuthTokenModel TipeeeStreamOAuthToken { get; set; }
        [JsonProperty]
        public OAuthTokenModel TreatStreamOAuthToken { get; set; }
        [JsonProperty]
        public OAuthTokenModel StreamJarOAuthToken { get; set; }
        [JsonProperty]
        public OAuthTokenModel PatreonOAuthToken { get; set; }
        [JsonProperty]
        public OAuthTokenModel IFTTTOAuthToken { get; set; }
        [JsonProperty]
        public OAuthTokenModel StreamlootsOAuthToken { get; set; }
        [JsonProperty]
        public OAuthTokenModel JustGivingOAuthToken { get; set; }

        [JsonProperty]
        public Dictionary<string, CommandGroupSettings> CommandGroups { get; set; } = new Dictionary<string, CommandGroupSettings>();
        [JsonProperty]
        public Dictionary<string, HotKeyConfiguration> HotKeys { get; set; } = new Dictionary<string, HotKeyConfiguration>();

        [JsonProperty]
        public ExpandedChannelModel Channel { get; set; }

        [JsonProperty]
        public bool FeatureMe { get; set; }
        [JsonProperty]
        public StreamingSoftwareTypeEnum DefaultStreamingSoftware { get; set; } = StreamingSoftwareTypeEnum.OBSStudio;
        [JsonProperty]
        public string DefaultAudioOutput { get; set; }
        [JsonProperty]
        public bool SaveChatEventLogs { get; set; }

        [JsonProperty]
        public bool WhisperAllAlerts { get; set; }
        [JsonProperty]
        public bool OnlyShowAlertsInDashboard { get; set; }
        [JsonProperty]
        public bool LatestChatAtTop { get; set; }
        [JsonProperty]
        public bool HideViewerAndChatterNumbers { get; set; }
        [JsonProperty]
        public bool HideChatUserList { get; set; }
        [JsonProperty]
        public bool HideDeletedMessages { get; set; }
        [JsonProperty]
        public bool TrackWhispererNumber { get; set; }
        [JsonProperty]
        public bool AllowCommandWhispering { get; set; }
        [JsonProperty]
        public bool IgnoreBotAccountCommands { get; set; }
        [JsonProperty]
        public bool CommandsOnlyInYourStream { get; set; }
        [JsonProperty]
        public bool DeleteChatCommandsWhenRun { get; set; }
        [JsonProperty]
        public bool ShowMixrElixrEmotes { get; set; }
        [JsonProperty]
        public bool ShowChatMessageTimestamps { get; set; }

        [JsonProperty]
        public uint DefaultMixPlayGame { get; set; }
        [JsonProperty]
        public bool PreventUnknownMixPlayUsers { get; set; }
        [JsonProperty]
        public bool PreventSmallerMixPlayCooldowns { get; set; }
        [JsonProperty]
        public List<MixPlaySharedProjectModel> CustomMixPlayProjectIDs { get; set; } = new List<MixPlaySharedProjectModel>();

        [JsonProperty]
        public int RegularUserMinimumHours { get; set; }
        [JsonProperty]
        public List<UserTitleModel> UserTitles { get; set; } = new List<UserTitleModel>();

        [JsonProperty]
        public bool GameQueueSubPriority { get; set; }
        [JsonProperty]
        public RequirementViewModel GameQueueRequirements { get; set; } = new RequirementViewModel();
        [JsonProperty]
        public CustomCommand GameQueueUserJoinedCommand { get; set; }
        [JsonProperty]
        public CustomCommand GameQueueUserSelectedCommand { get; set; }

        [JsonProperty]
        public bool QuotesEnabled { get; set; }
        [JsonProperty]
        public string QuotesFormat { get; set; }

        [JsonProperty]
        public int TimerCommandsInterval { get; set; } = 10;
        [JsonProperty]
        public int TimerCommandsMinimumMessages { get; set; } = 10;
        [JsonProperty]
        public bool DisableAllTimers { get; set; }

        [JsonProperty]
        public string GiveawayCommand { get; set; } = "giveaway";
        [JsonProperty]
        public int GiveawayTimer { get; set; } = 1;
        [JsonProperty]
        public int GiveawayMaximumEntries { get; set; } = 1;
        [JsonProperty]
        public RequirementViewModel GiveawayRequirements { get; set; } = new RequirementViewModel();
        [JsonProperty]
        public int GiveawayReminderInterval { get; set; } = 5;
        [JsonProperty]
        public bool GiveawayRequireClaim { get; set; } = true;
        [JsonProperty]
        public bool GiveawayAllowPastWinners { get; set; }
        [JsonProperty]
        public CustomCommand GiveawayStartedReminderCommand { get; set; }
        [JsonProperty]
        public CustomCommand GiveawayUserJoinedCommand { get; set; }
        [JsonProperty]
        public CustomCommand GiveawayWinnerSelectedCommand { get; set; }

        [JsonProperty]
        public bool ModerationUseCommunityFilteredWords { get; set; }

        [JsonProperty]
        public int ModerationFilteredWordsTimeout1MinuteOffenseCount { get; set; }
        [JsonProperty]
        public int ModerationFilteredWordsTimeout5MinuteOffenseCount { get; set; }
        [JsonProperty]
        public MixerRoleEnum ModerationFilteredWordsExcempt { get; set; } = MixerRoleEnum.Mod;
        [JsonProperty]
        public bool ModerationFilteredWordsApplyStrikes { get; set; } = true;

        [JsonProperty]
        public int ModerationCapsBlockCount { get; set; }
        [JsonProperty]
        public bool ModerationCapsBlockIsPercentage { get; set; } = true;
        [JsonProperty]
        public int ModerationPunctuationBlockCount { get; set; }
        [JsonProperty]
        public bool ModerationPunctuationBlockIsPercentage { get; set; } = true;
        [JsonProperty]
        public MixerRoleEnum ModerationChatTextExcempt { get; set; } = MixerRoleEnum.Mod;
        [JsonProperty]
        public bool ModerationChatTextApplyStrikes { get; set; } = true;

        [JsonProperty]
        public bool ModerationBlockLinks { get; set; }
        [JsonProperty]
        public MixerRoleEnum ModerationBlockLinksExcempt { get; set; } = MixerRoleEnum.Mod;
        [JsonProperty]
        public bool ModerationBlockLinksApplyStrikes { get; set; } = true;

        [JsonProperty]
        public ModerationChatInteractiveParticipationEnum ModerationChatInteractiveParticipation { get; set; } = ModerationChatInteractiveParticipationEnum.None;
        [JsonProperty]
        public MixerRoleEnum ModerationChatInteractiveParticipationExcempt { get; set; } = MixerRoleEnum.Mod;

        [JsonProperty]
        public bool ModerationResetStrikesOnLaunch { get; set; }
        [JsonProperty]
        public CustomCommand ModerationStrike1Command { get; set; }
        [JsonProperty]
        public CustomCommand ModerationStrike2Command { get; set; }
        [JsonProperty]
        public CustomCommand ModerationStrike3Command { get; set; }

        [JsonProperty]
        public bool EnableOverlay { get; set; }
        [JsonProperty]
        public Dictionary<string, int> OverlayCustomNameAndPorts { get; set; } = new Dictionary<string, int>();
        [JsonProperty]
        public string OverlaySourceName { get; set; }
        [JsonProperty]
        public int OverlayWidgetRefreshTime { get; set; } = 5;

        [JsonProperty]
        public string OvrStreamServerIP { get; set; }

        [JsonProperty]
        public string OBSStudioServerIP { get; set; }
        [JsonProperty]
        public string OBSStudioServerPassword { get; set; }

        [JsonProperty]
        public bool EnableStreamlabsOBSConnection { get; set; }

        [JsonProperty]
        public bool EnableXSplitConnection { get; set; }

        [JsonProperty]
        public bool EnableDeveloperAPI { get; set; }

        [JsonProperty]
        public int TiltifyCampaign { get; set; }

        [JsonProperty]
        public int ExtraLifeTeamID { get; set; }
        [JsonProperty]
        public int ExtraLifeParticipantID { get; set; }
        [JsonProperty]
        public bool ExtraLifeIncludeTeamDonations { get; set; }

        [JsonProperty]
        public string JustGivingPageShortName { get; set; }

        [JsonProperty]
        public string DiscordServer { get; set; }
        [JsonProperty]
        public string DiscordCustomClientID { get; set; }
        [JsonProperty]
        public string DiscordCustomClientSecret { get; set; }
        [JsonProperty]
        public string DiscordCustomBotToken { get; set; }

        [JsonProperty]
        public string PatreonTierMixerSubscriberEquivalent { get; set; }

        [JsonProperty]
        public bool UnlockAllCommands { get; set; }

        [JsonProperty]
        public int ChatFontSize { get; set; } = 13;
        [JsonProperty]
        public bool ChatShowUserJoinLeave { get; set; }
        [JsonProperty]
        public string ChatUserJoinLeaveColorScheme { get; set; } = ColorSchemes.DefaultColorScheme;
        [JsonProperty]
        public bool ChatShowEventAlerts { get; set; }
        [JsonProperty]
        public string ChatEventAlertsColorScheme { get; set; } = ColorSchemes.DefaultColorScheme;
        [JsonProperty]
        public bool ChatShowMixPlayAlerts { get; set; }
        [JsonProperty]
        public string ChatMixPlayAlertsColorScheme { get; set; } = ColorSchemes.DefaultColorScheme;

        [JsonProperty]
        public string NotificationChatMessageSoundFilePath { get; set; }
        [JsonProperty]
        public int NotificationChatMessageSoundVolume { get; set; } = 100;
        [JsonProperty]
        public string NotificationChatTaggedSoundFilePath { get; set; }
        [JsonProperty]
        public int NotificationChatTaggedSoundVolume { get; set; } = 100;
        [JsonProperty]
        public string NotificationChatWhisperSoundFilePath { get; set; }
        [JsonProperty]
        public int NotificationChatWhisperSoundVolume { get; set; } = 100;
        [JsonProperty]
        public string NotificationServiceConnectSoundFilePath { get; set; }
        [JsonProperty]
        public int NotificationServiceConnectSoundVolume { get; set; } = 100;
        [JsonProperty]
        public string NotificationServiceDisconnectSoundFilePath { get; set; }
        [JsonProperty]
        public int NotificationServiceDisconnectSoundVolume { get; set; } = 100;

        [JsonProperty]
        public int MaxMessagesInChat { get; set; } = 100;
        [JsonProperty]
        public int MaxUsersShownInChat { get; set; } = 100;

        [JsonProperty]
        public bool AutoExportStatistics { get; set; }

        [JsonProperty]
        public List<SerialDeviceModel> SerialDevices { get; set; } = new List<SerialDeviceModel>();

        [JsonProperty]
        public RemoteConnectionAuthenticationTokenModel RemoteHostConnection { get; set; }
        [JsonProperty]
        public List<RemoteConnectionModel> RemoteClientConnections { get; set; } = new List<RemoteConnectionModel>();

        [JsonProperty]
        public List<FavoriteGroupModel> FavoriteGroups { get; set; } = new List<FavoriteGroupModel>();

        [JsonProperty]
        public Dictionary<uint, JObject> CustomMixPlaySettings { get; set; } = new Dictionary<uint, JObject>();

        [JsonProperty]
        public DashboardLayoutTypeEnum DashboardLayout { get; set; }
        [JsonProperty]
        public List<DashboardItemTypeEnum> DashboardItems { get; set; } = new List<DashboardItemTypeEnum>();
        [JsonProperty]
        public List<Guid> DashboardQuickCommands { get; set; } = new List<Guid>();

        [JsonProperty]
        public List<string> RecentStreamTitles { get; set; } = new List<string>();

        [DataMember]
        public Dictionary<string, object> LatestSpecialIdentifiersData { get; set; } = new Dictionary<string, object>();

        [JsonProperty]
        public string TelemetryUserId { get; set; }

        [JsonProperty]
        public string SettingsBackupLocation { get; set; }
        [JsonProperty]
        public SettingsBackupRateEnum SettingsBackupRate { get; set; }
        [JsonProperty]
        public DateTimeOffset SettingsLastBackup { get; set; }

        [JsonIgnore]
        public DatabaseDictionary<uint, UserDataViewModel> UserData { get; set; } = new DatabaseDictionary<uint, UserDataViewModel>();

        [JsonIgnore]
        public LockedDictionary<Guid, UserCurrencyViewModel> Currencies { get; set; } = new LockedDictionary<Guid, UserCurrencyViewModel>();
        [JsonIgnore]
        public LockedDictionary<Guid, UserInventoryViewModel> Inventories { get; set; } = new LockedDictionary<Guid, UserInventoryViewModel>();

        [JsonIgnore]
        public LockedDictionary<string, int> CooldownGroups { get; set; } = new LockedDictionary<string, int>();

        [JsonIgnore]
        public LockedList<PreMadeChatCommandSettings> PreMadeChatCommandSettings { get; set; } = new LockedList<PreMadeChatCommandSettings>();
        [JsonIgnore]
        public LockedList<ChatCommand> ChatCommands { get; set; } = new LockedList<ChatCommand>();
        [JsonIgnore]
        public LockedList<EventCommand> EventCommands { get; set; } = new LockedList<EventCommand>();
        [JsonIgnore]
        public LockedList<MixPlayCommand> MixPlayCommands { get; set; } = new LockedList<MixPlayCommand>();
        [JsonIgnore]
        public LockedList<TimerCommand> TimerCommands { get; set; } = new LockedList<TimerCommand>();
        [JsonIgnore]
        public LockedList<ActionGroupCommand> ActionGroupCommands { get; set; } = new LockedList<ActionGroupCommand>();
        [JsonIgnore]
        public LockedList<GameCommandBase> GameCommands { get; set; } = new LockedList<GameCommandBase>();

        [JsonIgnore]
        public LockedList<UserQuoteViewModel> UserQuotes { get; set; } = new LockedList<UserQuoteViewModel>();

        [JsonIgnore]
        public LockedList<OverlayWidgetModel> OverlayWidgets { get; set; } = new LockedList<OverlayWidgetModel>();

        [JsonIgnore]
        public LockedList<RemoteProfileModel> RemoteProfiles { get; set; } = new LockedList<RemoteProfileModel>();
        [JsonIgnore]
        public LockedDictionary<Guid, RemoteProfileBoardsModel> RemoteProfileBoards { get; set; } = new LockedDictionary<Guid, RemoteProfileBoardsModel>();

        [JsonIgnore]
        public LockedList<string> FilteredWords { get; set; } = new LockedList<string>();
        [JsonIgnore]
        public LockedList<string> BannedWords { get; set; } = new LockedList<string>();

        [JsonIgnore]
        public LockedDictionary<uint, List<MixPlayUserGroupModel>> MixPlayUserGroups { get; set; } = new LockedDictionary<uint, List<MixPlayUserGroupModel>>();

        [JsonIgnore]
        public string DatabasePath { get; set; }

        [JsonIgnore]
        internal bool InitializeDB = true;

        [JsonProperty]
        protected Dictionary<Guid, UserCurrencyViewModel> currenciesInternal { get; set; } = new Dictionary<Guid, UserCurrencyViewModel>();
        [JsonProperty]
        protected Dictionary<Guid, UserInventoryViewModel> inventoriesInternal { get; set; } = new Dictionary<Guid, UserInventoryViewModel>();

        [JsonProperty]
        protected Dictionary<string, int> cooldownGroupsInternal { get; set; } = new Dictionary<string, int>();

        [JsonProperty]
        protected List<PreMadeChatCommandSettings> preMadeChatCommandSettingsInternal { get; set; } = new List<PreMadeChatCommandSettings>();
        [JsonProperty]
        protected List<ChatCommand> chatCommandsInternal { get; set; } = new List<ChatCommand>();
        [JsonProperty]
        protected List<EventCommand> eventCommandsInternal { get; set; } = new List<EventCommand>();
        [JsonProperty]
        protected List<MixPlayCommand> mixPlayCmmandsInternal { get; set; } = new List<MixPlayCommand>();
        [JsonProperty]
        protected List<TimerCommand> timerCommandsInternal { get; set; } = new List<TimerCommand>();
        [JsonProperty]
        protected List<ActionGroupCommand> actionGroupCommandsInternal { get; set; } = new List<ActionGroupCommand>();
        [JsonProperty]
        protected List<GameCommandBase> gameCommandsInternal { get; set; } = new List<GameCommandBase>();

        [JsonProperty]
        protected List<UserQuoteViewModel> userQuotesInternal { get; set; } = new List<UserQuoteViewModel>();

        [JsonProperty]
        [Obsolete]
        public List<OverlayWidget> overlayWidgetsInternal { get; set; } = new List<OverlayWidget>();
        [JsonProperty]
        protected List<OverlayWidgetModel> overlayWidgetModelsInternal { get; set; } = new List<OverlayWidgetModel>();

        [JsonProperty]
        protected List<RemoteProfileModel> remoteProfilesInternal { get; set; } = new List<RemoteProfileModel>();
        [JsonProperty]
        protected Dictionary<Guid, RemoteProfileBoardsModel> remoteProfileBoardsInternal { get; set; } = new Dictionary<Guid, RemoteProfileBoardsModel>();

        [JsonProperty]
        protected List<string> filteredWordsInternal { get; set; } = new List<string>();
        [JsonProperty]
        protected List<string> bannedWordsInternal { get; set; } = new List<string>();

        [JsonProperty]
        protected Dictionary<uint, List<MixPlayUserGroupModel>> mixPlayUserGroupsInternal { get; set; } = new Dictionary<uint, List<MixPlayUserGroupModel>>();
        [JsonProperty]
        [Obsolete]
        internal Dictionary<string, int> interactiveCooldownGroupsInternal { get; set; } = new Dictionary<string, int>();

        public SettingsV2Model() { }

        public SettingsV2Model(ExpandedChannelModel channel, bool isStreamer = true)
            : this()
        {
            this.Channel = channel;
            this.IsStreamer = isStreamer;

            BuildMissingCommands();

            this.DashboardItems = new List<DashboardItemTypeEnum>() { DashboardItemTypeEnum.None, DashboardItemTypeEnum.None, DashboardItemTypeEnum.None, DashboardItemTypeEnum.None };
            this.DashboardQuickCommands = new List<Guid>() { Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty };
        }

        public async Task Initialize()
        {
            this.Currencies = new LockedDictionary<Guid, UserCurrencyViewModel>(this.currenciesInternal);
            this.Inventories = new LockedDictionary<Guid, UserInventoryViewModel>(this.inventoriesInternal);
            this.PreMadeChatCommandSettings = new LockedList<PreMadeChatCommandSettings>(this.preMadeChatCommandSettingsInternal);
            this.CooldownGroups = new LockedDictionary<string, int>(this.cooldownGroupsInternal);
            this.ChatCommands = new LockedList<ChatCommand>(this.chatCommandsInternal);
            this.EventCommands = new LockedList<EventCommand>(this.eventCommandsInternal);
            this.MixPlayCommands = new LockedList<MixPlayCommand>(this.mixPlayCmmandsInternal);
            this.TimerCommands = new LockedList<TimerCommand>(this.timerCommandsInternal);
            this.ActionGroupCommands = new LockedList<ActionGroupCommand>(this.actionGroupCommandsInternal);
            this.GameCommands = new LockedList<GameCommandBase>(this.gameCommandsInternal);
            this.UserQuotes = new LockedList<UserQuoteViewModel>(this.userQuotesInternal);
            this.RemoteProfiles = new LockedList<RemoteProfileModel>(this.remoteProfilesInternal);
            this.RemoteProfileBoards = new LockedDictionary<Guid, RemoteProfileBoardsModel>(this.remoteProfileBoardsInternal);
            this.OverlayWidgets = new LockedList<OverlayWidgetModel>(this.overlayWidgetModelsInternal);
            this.FilteredWords = new LockedList<string>(this.filteredWordsInternal);
            this.BannedWords = new LockedList<string>(this.bannedWordsInternal);
            this.MixPlayUserGroups = new LockedDictionary<uint, List<MixPlayUserGroupModel>>(this.mixPlayUserGroupsInternal);

            if (this.DashboardItems.Count < 4)
            {
                this.DashboardItems = new List<DashboardItemTypeEnum>() { DashboardItemTypeEnum.None, DashboardItemTypeEnum.None, DashboardItemTypeEnum.None, DashboardItemTypeEnum.None };
            }
            if (this.DashboardQuickCommands.Count < 5)
            {
                this.DashboardQuickCommands = new List<Guid>() { Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty };
            }

            if (this.IsStreamer)
            {
                if (this.InitializeDB)
                {
                    Dictionary<uint, UserDataViewModel> initialUsers = new Dictionary<uint, UserDataViewModel>();
                    await ChannelSession.Services.Database.Read(this.DatabasePath, "SELECT * FROM Users", (DbDataReader dataReader) =>
                    {
                        UserDataViewModel userData = new UserDataViewModel(dataReader, this);
                        initialUsers[userData.MixerID] = userData;
                    });
                    this.UserData = new DatabaseDictionary<uint, UserDataViewModel>(initialUsers);
                }
            }

            if (string.IsNullOrEmpty(this.TelemetryUserId))
            {
                if (ChannelSession.IsDebug())
                {
                    this.TelemetryUserId = "MixItUpDebuggingUser";
                }
                else
                {
                    this.TelemetryUserId = Guid.NewGuid().ToString();
                }
            }

            BuildMissingCommands();
        }

        public async Task CopyLatestValues()
        {
            this.Version = SettingsV2Model.LatestVersion;

            if (ChannelSession.MixerUserConnection != null)
            {
                this.OAuthToken = ChannelSession.MixerUserConnection.Connection.GetOAuthTokenCopy();
            }
            if (ChannelSession.MixerBotConnection != null)
            {
                this.BotOAuthToken = ChannelSession.MixerBotConnection.Connection.GetOAuthTokenCopy();
            }

            this.StreamlabsOAuthToken = ChannelSession.Services.Streamlabs.GetOAuthTokenCopy();
            this.StreamJarOAuthToken = ChannelSession.Services.StreamJar.GetOAuthTokenCopy();
            this.TipeeeStreamOAuthToken = ChannelSession.Services.TipeeeStream.GetOAuthTokenCopy();
            this.TreatStreamOAuthToken = ChannelSession.Services.TreatStream.GetOAuthTokenCopy();
            this.StreamlootsOAuthToken = ChannelSession.Services.Streamloots.GetOAuthTokenCopy();
            this.TiltifyOAuthToken = ChannelSession.Services.Tiltify.GetOAuthTokenCopy();
            this.PatreonOAuthToken = ChannelSession.Services.Patreon.GetOAuthTokenCopy();
            this.IFTTTOAuthToken = ChannelSession.Services.IFTTT.GetOAuthTokenCopy();
            this.JustGivingOAuthToken = ChannelSession.Services.JustGiving.GetOAuthTokenCopy();
            this.DiscordOAuthToken = ChannelSession.Services.Discord.GetOAuthTokenCopy();
            this.TwitterOAuthToken = ChannelSession.Services.Twitter.GetOAuthTokenCopy();

            this.currenciesInternal = this.Currencies.ToDictionary();
            this.inventoriesInternal = this.Inventories.ToDictionary();
            this.preMadeChatCommandSettingsInternal = this.PreMadeChatCommandSettings.ToList();
            this.cooldownGroupsInternal = this.CooldownGroups.ToDictionary();
            this.chatCommandsInternal = this.ChatCommands.ToList();
            this.eventCommandsInternal = this.EventCommands.ToList();
            this.mixPlayCmmandsInternal = this.MixPlayCommands.ToList();
            this.timerCommandsInternal = this.TimerCommands.ToList();
            this.actionGroupCommandsInternal = this.ActionGroupCommands.ToList();
            this.gameCommandsInternal = this.GameCommands.ToList();
            this.userQuotesInternal = this.UserQuotes.ToList();
            this.remoteProfilesInternal = this.RemoteProfiles.ToList();
            this.remoteProfileBoardsInternal = this.RemoteProfileBoards.ToDictionary();
            this.overlayWidgetModelsInternal = this.OverlayWidgets.ToList();
            this.filteredWordsInternal = this.FilteredWords.ToList();
            this.bannedWordsInternal = this.BannedWords.ToList();
            this.mixPlayUserGroupsInternal = this.MixPlayUserGroups.ToDictionary();

            if (this.IsStreamer)
            {
                IEnumerable<uint> removedUsers = this.UserData.GetRemovedValues();
                await ChannelSession.Services.Database.BulkWrite(this.DatabasePath, "DELETE FROM Users WHERE ID = @ID", removedUsers.Select(u => new Dictionary<string, object>() { { "@ID", u } }));

                IEnumerable<UserDataViewModel> addedUsers = this.UserData.GetAddedValues();
                addedUsers = addedUsers.Where(u => !string.IsNullOrEmpty(u.MixerUsername));
                await ChannelSession.Services.Database.BulkWrite(this.DatabasePath, "INSERT INTO Users(ID, UserName, ViewingMinutes, CurrencyAmounts, InventoryAmounts, CustomCommands, Options)" +
                    " VALUES(@ID, @UserName, @ViewingMinutes, @CurrencyAmounts, @InventoryAmounts, @CustomCommands, @Options)",
                    addedUsers.Select(u => new Dictionary<string, object>() { { "@ID", u.MixerID }, { "@UserName", u.MixerUsername }, { "@ViewingMinutes", u.ViewingMinutes },
                        { "@CurrencyAmounts", u.GetCurrencyAmountsString() }, { "@InventoryAmounts", u.GetInventoryAmountsString() }, { "@CustomCommands", u.GetCustomCommandsString() },
                        { "@Options", u.GetOptionsString() } }));

                IEnumerable<UserDataViewModel> changedUsers = this.UserData.GetChangedValues();
                changedUsers = changedUsers.Where(u => !string.IsNullOrEmpty(u.MixerUsername));
                await ChannelSession.Services.Database.BulkWrite(this.DatabasePath, "UPDATE Users SET UserName = @UserName, ViewingMinutes = @ViewingMinutes, CurrencyAmounts = @CurrencyAmounts," +
                    " InventoryAmounts = @InventoryAmounts, CustomCommands = @CustomCommands, Options = @Options WHERE ID = @ID",
                    changedUsers.Select(u => new Dictionary<string, object>() { { "@ID", u.MixerID }, { "@UserName", u.MixerUsername }, { "@ViewingMinutes", u.ViewingMinutes },
                        { "@CurrencyAmounts", u.GetCurrencyAmountsString() }, { "@InventoryAmounts", u.GetInventoryAmountsString() }, { "@CustomCommands", u.GetCustomCommandsString() },
                        { "@Options", u.GetOptionsString() } }));
            }

            // Clear out unused Cooldown Groups and Command Groups
            var allUsedCooldownGroupNames =
                this.MixPlayCommands.Select(c => c.Requirements?.Cooldown?.GroupName)
                .Union(this.ChatCommands.Select(c => c.Requirements?.Cooldown?.GroupName))
                .Union(this.GameCommands.Select(c => c.Requirements?.Cooldown?.GroupName))
                .Distinct();
            var allUnusedCooldownGroupNames = this.CooldownGroups.ToDictionary().Where(c => !allUsedCooldownGroupNames.Contains(c.Key, StringComparer.InvariantCultureIgnoreCase));
            foreach (var unused in allUnusedCooldownGroupNames)
            {
                this.CooldownGroups.Remove(unused.Key);
            }

            var allUsedCommandGroupNames =
                this.ChatCommands.Select(c => c.GroupName)
                .Union(this.ActionGroupCommands.Select(a => a.GroupName))
                .Union(this.TimerCommands.Select(a => a.GroupName))
                .Distinct();
            var allUnusedCommandGroupNames = this.CommandGroups.Where(c => !allUsedCommandGroupNames.Contains(c.Key, StringComparer.InvariantCultureIgnoreCase));
            foreach (var unused in allUnusedCommandGroupNames)
            {
                this.CommandGroups.Remove(unused.Key);
            }
        }

        private void BuildMissingCommands()
        {
            this.GameQueueUserJoinedCommand = this.GameQueueUserJoinedCommand ?? CustomCommand.BasicChatCommand("Game Queue Used Joined", "You are #$queueposition in the queue to play.", isWhisper: true);
            this.GameQueueUserSelectedCommand = this.GameQueueUserSelectedCommand ?? CustomCommand.BasicChatCommand("Game Queue Used Selected", "It's time to play @$username! Listen carefully for instructions on how to join...");

            this.GiveawayStartedReminderCommand = this.GiveawayStartedReminderCommand ?? CustomCommand.BasicChatCommand("Giveaway Started/Reminder", "A giveaway has started for $giveawayitem! Type $giveawaycommand in chat in the next $giveawaytimelimit minute(s) to enter!");
            this.GiveawayUserJoinedCommand = this.GiveawayUserJoinedCommand ?? CustomCommand.BasicChatCommand("Giveaway User Joined", "You have been entered into the giveaway, stay tuned to see who wins!", isWhisper: true);
            this.GiveawayWinnerSelectedCommand = this.GiveawayWinnerSelectedCommand ?? CustomCommand.BasicChatCommand("Giveaway Winner Selected", "Congratulations @$username, you won $giveawayitem!");

            this.ModerationStrike1Command = this.ModerationStrike1Command ?? CustomCommand.BasicChatCommand("Moderation Strike 1", "$moderationreason. You have received a moderation strike & currently have $usermoderationstrikes strike(s)", isWhisper: true);
            this.ModerationStrike2Command = this.ModerationStrike2Command ?? CustomCommand.BasicChatCommand("Moderation Strike 2", "$moderationreason. You have received a moderation strike & currently have $usermoderationstrikes strike(s)", isWhisper: true);
            this.ModerationStrike3Command = this.ModerationStrike3Command ?? CustomCommand.BasicChatCommand("Moderation Strike 3", "$moderationreason. You have received a moderation strike & currently have $usermoderationstrikes strike(s)", isWhisper: true);
        }

        public Version GetLatestVersion() { return Assembly.GetEntryAssembly().GetName().Version; }
    }
}