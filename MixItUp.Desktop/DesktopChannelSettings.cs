using Mixer.Base.Model.Channel;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Favorites;
using MixItUp.Base.Model.MixPlay;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Remote.Authentication;
using MixItUp.Base.Model.Serial;
using MixItUp.Base.Model.SongRequests;
using MixItUp.Base.Model.User;
using MixItUp.Base.Remote.Models;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModel.Window.Dashboard;
using MixItUp.Desktop.Database;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Model.OAuth;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Desktop
{
    [DataContract]
    public class DesktopSavableChannelSettings : ISavableChannelSettings
    {
        public const int LatestVersion = 38;

        [JsonProperty]
        public int Version { get; set; } = DesktopChannelSettings.LatestVersion;

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
        public OAuthTokenModel SpotifyOAuthToken { get; set; }
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
        public Dictionary<string, CommandGroupSettings> CommandGroups { get; set; }
        [JsonProperty]
        public Dictionary<string, HotKeyConfiguration> HotKeys { get; set; }

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
        public List<MixPlaySharedProjectModel> CustomMixPlayProjectIDs { get; set; }

        [JsonProperty]
        public int RegularUserMinimumHours { get; set; }
        [JsonProperty]
        public List<UserTitleModel> UserTitles { get; set; }

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
        public Dictionary<string, int> OverlayCustomNameAndPorts { get; set; }
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
        public List<SerialDeviceModel> SerialDevices { get; set; }

        [JsonProperty]
        public RemoteConnectionAuthenticationTokenModel RemoteHostConnection { get; set; }
        [JsonProperty]
        public List<RemoteConnectionModel> RemoteClientConnections { get; set; }

        [JsonProperty]
        public List<FavoriteGroupModel> FavoriteGroups { get; set; }

        [JsonProperty]
        public HashSet<SongRequestServiceTypeEnum> SongRequestServiceTypes { get; set; }
        [JsonProperty]
        public bool SpotifyAllowExplicit { get; set; }
        [JsonProperty]
        public string DefaultPlaylist { get; set; }
        [JsonProperty]
        public bool SongRequestSubPriority { get; set; }
        [JsonProperty]
        public int SongRequestsMaxRequests { get; set; }
        [JsonProperty]
        public bool SongRequestsSaveRequestQueue { get; set; }
        [JsonProperty]
        public List<SongRequestModel> SongRequestsSavedRequestQueue { get; set; } = new List<SongRequestModel>();
        [JsonProperty]
        public int SongRequestVolume { get; set; } = 100;
        [JsonProperty]
        public List<SongRequestModel> SongRequestsBannedSongs { get; set; } = new List<SongRequestModel>();
        [JsonProperty]
        public CustomCommand SongAddedCommand { get; set; }
        [JsonProperty]
        public CustomCommand SongRemovedCommand { get; set; }
        [JsonProperty]
        public CustomCommand SongPlayedCommand { get; set; }

        [JsonProperty]
        public Dictionary<uint, JObject> CustomMixPlaySettings { get; set; }

        [JsonProperty]
        public DashboardLayoutTypeEnum DashboardLayout { get; set; }
        [JsonProperty]
        public List<DashboardItemTypeEnum> DashboardItems { get; set; } = new List<DashboardItemTypeEnum>();
        [JsonProperty]
        public List<Guid> DashboardQuickCommands { get; set; } = new List<Guid>();

        [JsonProperty]
        public string TelemetryUserId { get; set; }

        [JsonProperty]
        public string SettingsBackupLocation { get; set; }
        [JsonProperty]
        public SettingsBackupRateEnum SettingsBackupRate { get; set; }
        [JsonProperty]
        public DateTimeOffset SettingsLastBackup { get; set; }

        [JsonProperty]
        protected Dictionary<Guid, UserCurrencyViewModel> currenciesInternal { get; set; }
        [JsonProperty]
        protected Dictionary<Guid, UserInventoryViewModel> inventoriesInternal { get; set; }

        [JsonProperty]
        protected Dictionary<string, int> cooldownGroupsInternal { get; set; }

        [JsonProperty]
        protected List<PreMadeChatCommandSettings> preMadeChatCommandSettingsInternal { get; set; }
        [JsonProperty]
        protected List<ChatCommand> chatCommandsInternal { get; set; }
        [JsonProperty]
        protected List<EventCommand> eventCommandsInternal { get; set; }
        [JsonProperty]
        protected List<MixPlayCommand> mixPlayCmmandsInternal { get; set; }
        [JsonProperty]
        protected List<TimerCommand> timerCommandsInternal { get; set; }
        [JsonProperty]
        protected List<ActionGroupCommand> actionGroupCommandsInternal { get; set; }
        [JsonProperty]
        protected List<GameCommandBase> gameCommandsInternal { get; set; }

        [JsonProperty]
        protected List<UserQuoteViewModel> userQuotesInternal { get; set; }

        [JsonProperty]
        [Obsolete]
        public List<OverlayWidget> overlayWidgetsInternal { get; set; }
        [JsonProperty]
        protected List<OverlayWidgetModel> overlayWidgetModelsInternal { get; set; }

        [JsonProperty]
        protected List<RemoteProfileModel> remoteProfilesInternal { get; set; }
        [JsonProperty]
        protected Dictionary<Guid, RemoteProfileBoardsModel> remoteProfileBoardsInternal { get; set; }

        [JsonProperty]
        protected List<string> filteredWordsInternal { get; set; }
        [JsonProperty]
        protected List<string> bannedWordsInternal { get; set; }

        [JsonProperty]
        protected Dictionary<uint, List<MixPlayUserGroupModel>> mixPlayUserGroupsInternal { get; set; }
        [JsonProperty]
        [Obsolete]
        internal Dictionary<string, int> interactiveCooldownGroupsInternal { get; set; }

        public DesktopSavableChannelSettings()
        {
            this.CommandGroups = new Dictionary<string, CommandGroupSettings>();
            this.HotKeys = new Dictionary<string, HotKeyConfiguration>();
            this.OverlayCustomNameAndPorts = new Dictionary<string, int>();
            this.CustomMixPlayProjectIDs = new List<MixPlaySharedProjectModel>();
            this.UserTitles = new List<UserTitleModel>();
            this.SerialDevices = new List<SerialDeviceModel>();
            this.RemoteClientConnections = new List<RemoteConnectionModel>();
            this.FavoriteGroups = new List<FavoriteGroupModel>();
            this.SongRequestServiceTypes = new HashSet<SongRequestServiceTypeEnum>();
            this.CustomMixPlaySettings = new Dictionary<uint, JObject>();

            this.currenciesInternal = new Dictionary<Guid, UserCurrencyViewModel>();
            this.inventoriesInternal = new Dictionary<Guid, UserInventoryViewModel>();
            this.preMadeChatCommandSettingsInternal = new List<PreMadeChatCommandSettings>();
            this.cooldownGroupsInternal = new Dictionary<string, int>();
            this.chatCommandsInternal = new List<ChatCommand>();
            this.eventCommandsInternal = new List<EventCommand>();
            this.mixPlayCmmandsInternal = new List<MixPlayCommand>();
            this.timerCommandsInternal = new List<TimerCommand>();
            this.actionGroupCommandsInternal = new List<ActionGroupCommand>();
            this.gameCommandsInternal = new List<GameCommandBase>();
            this.userQuotesInternal = new List<UserQuoteViewModel>();
            this.remoteProfilesInternal = new List<RemoteProfileModel>();
            this.remoteProfileBoardsInternal = new Dictionary<Guid, RemoteProfileBoardsModel>();
            this.overlayWidgetModelsInternal = new List<OverlayWidgetModel>();
            this.filteredWordsInternal = new List<string>();
            this.bannedWordsInternal = new List<string>();
            this.mixPlayUserGroupsInternal = new Dictionary<uint, List<MixPlayUserGroupModel>>();
#pragma warning disable CS0612 // Type or member is obsolete
            this.overlayWidgetsInternal = new List<OverlayWidget>();
            this.interactiveCooldownGroupsInternal = new Dictionary<string, int>();
#pragma warning restore CS0612 // Type or member is obsolete
        }
    }

    [DataContract]
    public class DesktopChannelSettings : DesktopSavableChannelSettings, IChannelSettings
    {
        private const string CommunityFilteredWordsFilePath = "Assets\\CommunityBannedWords.txt";

        [JsonIgnore]
        public DatabaseDictionary<uint, UserDataViewModel> UserData { get; set; }

        [JsonIgnore]
        public LockedDictionary<Guid, UserCurrencyViewModel> Currencies { get; set; }
        [JsonIgnore]
        public LockedDictionary<Guid, UserInventoryViewModel> Inventories { get; set; }

        [JsonIgnore]
        public LockedDictionary<string, int> CooldownGroups { get; set; }

        [JsonIgnore]
        public LockedList<PreMadeChatCommandSettings> PreMadeChatCommandSettings { get; set; }
        [JsonIgnore]
        public LockedList<ChatCommand> ChatCommands { get; set; }
        [JsonIgnore]
        public LockedList<EventCommand> EventCommands { get; set; }
        [JsonIgnore]
        public LockedList<MixPlayCommand> MixPlayCommands { get; set; }
        [JsonIgnore]
        public LockedList<TimerCommand> TimerCommands { get; set; }
        [JsonIgnore]
        public LockedList<ActionGroupCommand> ActionGroupCommands { get; set; }
        [JsonIgnore]
        public LockedList<GameCommandBase> GameCommands { get; set; }

        [JsonIgnore]
        public LockedList<UserQuoteViewModel> UserQuotes { get; set; }

        [JsonIgnore]
        public LockedList<OverlayWidgetModel> OverlayWidgets { get; set; }

        [JsonIgnore]
        public LockedList<RemoteProfileModel> RemoteProfiles { get; set; }
        [JsonIgnore]
        public LockedDictionary<Guid, RemoteProfileBoardsModel> RemoteProfileBoards { get; set; }

        [JsonIgnore]
        public LockedList<string> FilteredWords { get; set; }
        [JsonIgnore]
        public LockedList<string> BannedWords { get; set; }
        [JsonIgnore]
        public LockedList<string> CommunityFilteredWords { get; set; }

        [JsonIgnore]
        public LockedDictionary<uint, List<MixPlayUserGroupModel>> MixPlayUserGroups { get; set; }

        [JsonIgnore]
        public string DatabasePath { get; set; }

        [JsonIgnore]
        internal SQLiteDatabaseWrapper DatabaseWrapper;

        [JsonIgnore]
        internal bool InitializeDB = true;

        public DesktopChannelSettings(ExpandedChannelModel channel, bool isStreamer = true)
            : this()
        {
            this.Channel = channel;
            this.IsStreamer = isStreamer;

            this.GameQueueUserJoinedCommand = CustomCommand.BasicChatCommand("Game Queue Used Joined", "You are #$queueposition in the queue to play.", isWhisper: true);
            this.GameQueueUserSelectedCommand = CustomCommand.BasicChatCommand("Game Queue Used Selected", "It's time to play @$username! Listen carefully for instructions on how to join...");

            this.GiveawayStartedReminderCommand = CustomCommand.BasicChatCommand("Giveaway Started/Reminder", "A giveaway has started for $giveawayitem! Type $giveawaycommand in chat in the next $giveawaytimelimit minute(s) to enter!");
            this.GiveawayUserJoinedCommand = CustomCommand.BasicChatCommand("Giveaway User Joined", "You have been entered into the giveaway, stay tuned to see who wins!", isWhisper: true);
            this.GiveawayWinnerSelectedCommand = CustomCommand.BasicChatCommand("Giveaway Winner Selected", "Congratulations @$username, you won $giveawayitem!");

            this.SongAddedCommand = CustomCommand.BasicChatCommand("Song Request Added", "$songtitle has been added to the queue", isWhisper: true);
            this.SongRemovedCommand = CustomCommand.BasicChatCommand("Song Request Removed", "$songtitle has been removed from the queue", isWhisper: true);
            this.SongPlayedCommand = CustomCommand.BasicChatCommand("Song Request Played", "Now Playing: $songtitle");

            this.ModerationStrike1Command = CustomCommand.BasicChatCommand("Moderation Strike 1", "$moderationreason. You have received a moderation strike & currently have $usermoderationstrikes strike(s)", isWhisper: true);
            this.ModerationStrike2Command = CustomCommand.BasicChatCommand("Moderation Strike 2", "$moderationreason. You have received a moderation strike & currently have $usermoderationstrikes strike(s)", isWhisper: true);
            this.ModerationStrike3Command = CustomCommand.BasicChatCommand("Moderation Strike 3", "$moderationreason. You have received a moderation strike & currently have $usermoderationstrikes strike(s)", isWhisper: true);

            this.DashboardItems = new List<DashboardItemTypeEnum>() { DashboardItemTypeEnum.None, DashboardItemTypeEnum.None, DashboardItemTypeEnum.None, DashboardItemTypeEnum.None };
            this.DashboardQuickCommands = new List<Guid>() { Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty };
        }

        public DesktopChannelSettings()
            : base()
        {
            this.UserData = new DatabaseDictionary<uint, UserDataViewModel>();
            this.Currencies = new LockedDictionary<Guid, UserCurrencyViewModel>();
            this.Inventories = new LockedDictionary<Guid, UserInventoryViewModel>();
            this.CooldownGroups = new LockedDictionary<string, int>();
            this.PreMadeChatCommandSettings = new LockedList<PreMadeChatCommandSettings>();
            this.ChatCommands = new LockedList<ChatCommand>();
            this.EventCommands = new LockedList<EventCommand>();
            this.MixPlayCommands = new LockedList<MixPlayCommand>();
            this.TimerCommands = new LockedList<TimerCommand>();
            this.ActionGroupCommands = new LockedList<ActionGroupCommand>();
            this.GameCommands = new LockedList<GameCommandBase>();
            this.UserQuotes = new LockedList<UserQuoteViewModel>();
            this.RemoteProfiles = new LockedList<RemoteProfileModel>();
            this.OverlayWidgets = new LockedList<OverlayWidgetModel>();
            this.FilteredWords = new LockedList<string>();
            this.BannedWords = new LockedList<string>();
            this.CommunityFilteredWords = new LockedList<string>();
            this.MixPlayUserGroups = new LockedDictionary<uint, List<MixPlayUserGroupModel>>();
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

            if (File.Exists(DesktopChannelSettings.CommunityFilteredWordsFilePath))
            {
                this.CommunityFilteredWords = new LockedList<string>(File.ReadAllLines(DesktopChannelSettings.CommunityFilteredWordsFilePath));
            }

            if (this.IsStreamer)
            {
                this.DatabaseWrapper = new SQLiteDatabaseWrapper(this.DatabasePath);
                if (this.InitializeDB)
                {
                    Dictionary<uint, UserDataViewModel> initialUsers = new Dictionary<uint, UserDataViewModel>();
                    await this.DatabaseWrapper.RunReadCommand("SELECT * FROM Users", (SQLiteDataReader dataReader) =>
                    {
                        UserDataViewModel userData = new UserDataViewModel(dataReader, this);
                        initialUsers[userData.ID] = userData;
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
        }

        public async Task CopyLatestValues()
        {
            this.Version = DesktopChannelSettings.LatestVersion;

            if (ChannelSession.MixerStreamerConnection != null)
            {
                this.OAuthToken = ChannelSession.MixerStreamerConnection.Connection.GetOAuthTokenCopy();
            }
            if (ChannelSession.MixerBotConnection != null)
            {
                this.BotOAuthToken = ChannelSession.MixerBotConnection.Connection.GetOAuthTokenCopy();
            }

            if (ChannelSession.Services.Streamlabs != null)
            {
                this.StreamlabsOAuthToken = ChannelSession.Services.Streamlabs.GetOAuthTokenCopy();
            }
            if (ChannelSession.Services.Twitter != null)
            {
                this.TwitterOAuthToken = ChannelSession.Services.Twitter.GetOAuthTokenCopy();
            }
            if (ChannelSession.Services.Spotify != null)
            {
                this.SpotifyOAuthToken = ChannelSession.Services.Spotify.GetOAuthTokenCopy();
            }
            if (ChannelSession.Services.Discord != null)
            {
                this.DiscordOAuthToken = ChannelSession.Services.Discord.GetOAuthTokenCopy();
            }
            if (ChannelSession.Services.Tiltify != null)
            {
                this.TiltifyOAuthToken = ChannelSession.Services.Tiltify.GetOAuthTokenCopy();
            }
            if (ChannelSession.Services.TipeeeStream != null)
            {
                this.TipeeeStreamOAuthToken = ChannelSession.Services.TipeeeStream.GetOAuthTokenCopy();
            }
            if (ChannelSession.Services.TreatStream != null)
            {
                this.TreatStreamOAuthToken = ChannelSession.Services.TreatStream.GetOAuthTokenCopy();
            }
            if (ChannelSession.Services.StreamJar != null)
            {
                this.StreamJarOAuthToken = ChannelSession.Services.StreamJar.GetOAuthTokenCopy();
            }
            if (ChannelSession.Services.Patreon != null)
            {
                this.PatreonOAuthToken = ChannelSession.Services.Patreon.GetOAuthTokenCopy();
            }
            if (ChannelSession.Services.IFTTT != null)
            {
                this.IFTTTOAuthToken = ChannelSession.Services.IFTTT.Token;
            }
            if (ChannelSession.Services.Streamloots != null)
            {
                this.StreamlootsOAuthToken = ChannelSession.Services.Streamloots.GetOAuthTokenCopy();
            }
            if (ChannelSession.Services.JustGiving != null)
            {
                this.JustGivingOAuthToken = ChannelSession.Services.JustGiving.GetOAuthTokenCopy();
            }

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
                await this.DatabaseWrapper.RunBulkWriteCommand("DELETE FROM Users WHERE ID = @ID", removedUsers.Select(u => new List<SQLiteParameter>() { new SQLiteParameter("@ID", value: (int)u) }));

                IEnumerable<UserDataViewModel> addedUsers = this.UserData.GetAddedValues();
                addedUsers = addedUsers.Where(u => !string.IsNullOrEmpty(u.UserName));
                await this.DatabaseWrapper.RunBulkWriteCommand("INSERT INTO Users(ID, UserName, ViewingMinutes, CurrencyAmounts, InventoryAmounts, CustomCommands, Options) VALUES(?,?,?,?,?,?,?)",
                    addedUsers.Select(u => new List<SQLiteParameter>() { new SQLiteParameter(DbType.UInt32, u.ID), new SQLiteParameter(DbType.String, value: u.UserName),
                    new SQLiteParameter(DbType.Int32, value: u.ViewingMinutes), new SQLiteParameter(DbType.String, value: u.GetCurrencyAmountsString()),
                    new SQLiteParameter(DbType.String, value: u.GetInventoryAmountsString()), new SQLiteParameter(DbType.String, value: u.GetCustomCommandsString()),
                    new SQLiteParameter(DbType.String, value: u.GetOptionsString()) }));

                IEnumerable<UserDataViewModel> changedUsers = this.UserData.GetChangedValues();
                changedUsers = changedUsers.Where(u => !string.IsNullOrEmpty(u.UserName));
                await this.DatabaseWrapper.RunBulkWriteCommand("UPDATE Users SET UserName = @UserName, ViewingMinutes = @ViewingMinutes, CurrencyAmounts = @CurrencyAmounts," +
                    " InventoryAmounts = @InventoryAmounts, CustomCommands = @CustomCommands, Options = @Options WHERE ID = @ID",
                    changedUsers.Select(u => new List<SQLiteParameter>() { new SQLiteParameter("@UserName", value: u.UserName), new SQLiteParameter("@ViewingMinutes", value: u.ViewingMinutes),
                    new SQLiteParameter("@CurrencyAmounts", value: u.GetCurrencyAmountsString()), new SQLiteParameter("@InventoryAmounts", value: u.GetInventoryAmountsString()),
                    new SQLiteParameter("@CustomCommands", value: u.GetCustomCommandsString()), new SQLiteParameter("@Options", value: u.GetOptionsString()), new SQLiteParameter("@ID", value: (int)u.ID) }));
            }

            // Clear out unused Cooldown Groups and Command Groups
            var allUsedCooldownGroupNames = 
                this.MixPlayCommands.Select(c => c.Requirements?.Cooldown?.GroupName)
                .Union(this.ChatCommands.Select(c => c.Requirements?.Cooldown?.GroupName))
                .Union(this.GameCommands.Select(c => c.Requirements?.Cooldown?.GroupName))
                .Distinct();
            var allUnusedCooldownGroupNames = this.CooldownGroups.ToDictionary().Where(c => !allUsedCooldownGroupNames.Contains(c.Key, StringComparer.InvariantCultureIgnoreCase));
            foreach(var unused in allUnusedCooldownGroupNames)
            {
                this.CooldownGroups.Remove(unused.Key);
            }

            var allUsedCommandGroupNames = 
                this.ChatCommands.Select(c => c.GroupName)
                .Union(this.ActionGroupCommands.Select(a=>a.GroupName))
                .Union(this.TimerCommands.Select(a => a.GroupName))
                .Distinct();
            var allUnusedCommandGroupNames = this.CommandGroups.Where(c => !allUsedCommandGroupNames.Contains(c.Key, StringComparer.InvariantCultureIgnoreCase));
            foreach (var unused in allUnusedCommandGroupNames)
            {
                this.CommandGroups.Remove(unused.Key);
            }
        }

        public Version GetLatestVersion() { return Assembly.GetEntryAssembly().GetName().Version; }
    }
}
