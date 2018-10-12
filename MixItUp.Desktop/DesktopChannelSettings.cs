using Mixer.Base.Model.Channel;
using Mixer.Base.Model.OAuth;
using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Favorites;
using MixItUp.Base.Model.Interactive;
using MixItUp.Base.Model.Remote;
using MixItUp.Base.Model.Serial;
using MixItUp.Base.Services;
using MixItUp.Base.Themes;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Interactive;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using MixItUp.Desktop.Database;
using Newtonsoft.Json;
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
        public const int LatestVersion = 22;

        [JsonProperty]
        public int Version { get; set; }

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
        public OAuthTokenModel GameWispOAuthToken { get; set; }
        [JsonProperty]
        public OAuthTokenModel GawkBoxOAuthToken { get; set; }
        [JsonProperty]
        public OAuthTokenModel TwitterOAuthToken { get; set; }
        [JsonProperty]
        public OAuthTokenModel SpotifyOAuthToken { get; set; }
        [JsonProperty]
        public OAuthTokenModel DiscordOAuthToken { get; set; }
        [JsonProperty]
        public OAuthTokenModel TiltifyOAuthToken { get; set; }

        [JsonProperty]
        public string StreamDeckDeviceName { get; set; }

        [JsonProperty]
        public ExpandedChannelModel Channel { get; set; }

        [JsonProperty]
        public bool FeatureMe { get; set; }
        [JsonProperty]
        public StreamingSoftwareTypeEnum DefaultStreamingSoftware { get; set; }
        [JsonProperty]
        public string DefaultAudioOutput { get; set; }
        [JsonProperty]
        public bool SaveChatEventLogs { get; set; }

        [JsonProperty]
        public bool WhisperAllAlerts { get; set; }
        [JsonProperty]
        public bool LatestChatAtTop { get; set; }
        [JsonProperty]
        public bool HideViewerAndChatterNumbers { get; set; }
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
        public uint DefaultInteractiveGame { get; set; }
        [JsonProperty]
        public bool PreventUnknownInteractiveUsers { get; set; }
        [JsonProperty]
        public List<InteractiveSharedProjectModel> CustomInteractiveProjectIDs { get; set; }

        [JsonProperty]
        public bool GameQueueSubPriority { get; set; }
        [JsonProperty]
        public RequirementViewModel GameQueueRequirements { get; set; }

        [JsonProperty]
        public bool QuotesEnabled { get; set; }

        [JsonProperty]
        public int TimerCommandsInterval { get; set; }
        [JsonProperty]
        public int TimerCommandsMinimumMessages { get; set; }

        [JsonProperty]
        public string GiveawayCommand { get; set; }
        [JsonProperty]
        public bool GiveawayGawkBoxTrigger { get; set; }
        [JsonProperty]
        public bool GiveawayStreamlabsTrigger { get; set; }
        [JsonProperty]
        public bool GiveawayTiltifyTrigger { get; set; }
        [JsonProperty]
        public bool GiveawayDonationRequiredAmount { get; set; }
        [JsonProperty]
        public double GiveawayDonationAmount { get; set; }
        [JsonProperty]
        public int GiveawayTimer { get; set; }
        [JsonProperty]
        public RequirementViewModel GiveawayRequirements { get; set; }
        [JsonProperty]
        public int GiveawayReminderInterval { get; set; }
        [JsonProperty]
        public bool GiveawayRequireClaim { get; set; }

        [JsonProperty]
        public bool ModerationUseCommunityFilteredWords { get; set; }

        [JsonProperty]
        public int ModerationFilteredWordsTimeout1MinuteOffenseCount { get; set; }
        [JsonProperty]
        public int ModerationFilteredWordsTimeout5MinuteOffenseCount { get; set; }
        [JsonProperty]
        public MixerRoleEnum ModerationFilteredWordsExcempt { get; set; }

        [JsonProperty]
        public int ModerationCapsBlockCount { get; set; }
        [JsonProperty]
        public bool ModerationCapsBlockIsPercentage { get; set; }
        [JsonProperty]
        public int ModerationPunctuationBlockCount { get; set; }
        [JsonProperty]
        public bool ModerationPunctuationBlockIsPercentage { get; set; }
        [JsonProperty]
        public int ModerationEmoteBlockCount { get; set; }
        [JsonProperty]
        public bool ModerationEmoteBlockIsPercentage { get; set; }
        [JsonProperty]
        public int ModerationChatTextTimeout1MinuteOffenseCount { get; set; }
        [JsonProperty]
        public int ModerationChatTextTimeout5MinuteOffenseCount { get; set; }
        [JsonProperty]
        public MixerRoleEnum ModerationChatTextExcempt { get; set; }

        [JsonProperty]
        public bool ModerationBlockLinks { get; set; }
        [JsonProperty]
        public MixerRoleEnum ModerationBlockLinksExcempt { get; set; }

        [JsonProperty]
        public int ModerationTimeout1MinuteOffenseCount { get; set; }
        [JsonProperty]
        public int ModerationTimeout5MinuteOffenseCount { get; set; }
        [JsonProperty]
        public MixerRoleEnum ModerationTimeoutExempt { get; set; }

        [JsonProperty]
        public bool EnableOverlay { get; set; }
        [JsonProperty]
        public string OverlaySourceName { get; set; }

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
        public string DiscordServer { get; set; }

        [JsonProperty]
        public bool UnlockAllCommands { get; set; }

        [JsonProperty]
        public int ChatFontSize { get; set; }
        [JsonProperty]
        public bool ChatShowUserJoinLeave { get; set; }
        [JsonProperty]
        public string ChatUserJoinLeaveColorScheme { get; set; }
        [JsonProperty]
        public bool ChatShowEventAlerts { get; set; }
        [JsonProperty]
        public string ChatEventAlertsColorScheme { get; set; }
        [JsonProperty]
        public bool ChatShowInteractiveAlerts { get; set; }
        [JsonProperty]
        public string ChatInteractiveAlertsColorScheme { get; set; }

        [JsonProperty]
        public string NotificationChatMessageSoundFilePath { get; set; }
        [JsonProperty]
        public string NotificationChatTaggedSoundFilePath { get; set; }
        [JsonProperty]
        public string NotificationChatWhisperSoundFilePath { get; set; }
        [JsonProperty]
        public string NotificationServiceConnectSoundFilePath { get; set; }
        [JsonProperty]
        public string NotificationServiceDisconnectSoundFilePath { get; set; }

        [JsonProperty]
        public int MaxMessagesInChat { get; set; }

        [JsonProperty]
        public bool AutoExportStatistics { get; set; }

        [JsonProperty]
        public List<SerialDeviceModel> SerialDevices { get; set; }

        [JsonProperty]
        public List<RemoteBoardModel> RemoteBoards { get; set; }
        [JsonProperty]
        public List<RemoteDeviceModel> RemoteSavedDevices { get; set; }

        [JsonProperty]
        public List<FavoriteGroupModel> FavoriteGroups { get; set; }

        [JsonProperty]
        public HashSet<SongRequestServiceTypeEnum> SongRequestServiceTypes { get; set; }
        [JsonProperty]
        public bool SpotifyAllowExplicit { get; set; }

        [JsonProperty]
        public string DefaultPlaylist { get; set; }

        [JsonProperty]
        public int SongRequestVolume { get; set; } = 100;

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
        protected Dictionary<string, int> cooldownGroupsInternal { get; set; }

        [JsonProperty]
        protected List<PreMadeChatCommandSettings> preMadeChatCommandSettingsInternal { get; set; }
        [JsonProperty]
        protected List<ChatCommand> chatCommandsInternal { get; set; }
        [JsonProperty]
        protected List<EventCommand> eventCommandsInternal { get; set; }
        [JsonProperty]
        protected List<InteractiveCommand> interactiveCommandsInternal { get; set; }
        [JsonProperty]
        protected List<TimerCommand> timerCommandsInternal { get; set; }
        [JsonProperty]
        protected List<ActionGroupCommand> actionGroupCommandsInternal { get; set; }
        [JsonProperty]
        protected List<GameCommandBase> gameCommandsInternal { get; set; }
        [JsonProperty]
        protected List<RemoteCommand> remoteCommandsInternal { get; set; }

        [JsonProperty]
        protected List<UserQuoteViewModel> userQuotesInternal { get; set; }

        [JsonProperty]
        protected List<string> filteredWordsInternal { get; set; }
        [JsonProperty]
        protected List<string> bannedWordsInternal { get; set; }

        [JsonProperty]
        protected Dictionary<uint, List<InteractiveUserGroupViewModel>> interactiveUserGroupsInternal { get; set; }
        [JsonProperty]
        [Obsolete]
        internal Dictionary<string, int> interactiveCooldownGroupsInternal { get; set; }

        public DesktopSavableChannelSettings()
        {
            this.CustomInteractiveProjectIDs = new List<InteractiveSharedProjectModel>();
            this.SerialDevices = new List<SerialDeviceModel>();
            this.RemoteBoards = new List<RemoteBoardModel>();
            this.RemoteSavedDevices = new List<RemoteDeviceModel>();
            this.FavoriteGroups = new List<FavoriteGroupModel>();
            this.SongRequestServiceTypes = new HashSet<SongRequestServiceTypeEnum>();

            this.currenciesInternal = new Dictionary<Guid, UserCurrencyViewModel>();
            this.preMadeChatCommandSettingsInternal = new List<PreMadeChatCommandSettings>();
            this.cooldownGroupsInternal = new Dictionary<string, int>();
            this.chatCommandsInternal = new List<ChatCommand>();
            this.eventCommandsInternal = new List<EventCommand>();
            this.interactiveCommandsInternal = new List<InteractiveCommand>();
            this.timerCommandsInternal = new List<TimerCommand>();
            this.actionGroupCommandsInternal = new List<ActionGroupCommand>();
            this.gameCommandsInternal = new List<GameCommandBase>();
            this.remoteCommandsInternal = new List<RemoteCommand>();
            this.userQuotesInternal = new List<UserQuoteViewModel>();
            this.filteredWordsInternal = new List<string>();
            this.bannedWordsInternal = new List<string>();
            this.interactiveUserGroupsInternal = new Dictionary<uint, List<InteractiveUserGroupViewModel>>();
#pragma warning disable CS0612 // Type or member is obsolete
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
        public LockedDictionary<string, int> CooldownGroups { get; set; }

        [JsonIgnore]
        public LockedList<PreMadeChatCommandSettings> PreMadeChatCommandSettings { get; set; }
        [JsonIgnore]
        public LockedList<ChatCommand> ChatCommands { get; set; }
        [JsonIgnore]
        public LockedList<EventCommand> EventCommands { get; set; }
        [JsonIgnore]
        public LockedList<InteractiveCommand> InteractiveCommands { get; set; }
        [JsonIgnore]
        public LockedList<TimerCommand> TimerCommands { get; set; }
        [JsonIgnore]
        public LockedList<ActionGroupCommand> ActionGroupCommands { get; set; }
        [JsonIgnore]
        public LockedList<GameCommandBase> GameCommands { get; set; }
        [JsonIgnore]
        public LockedList<RemoteCommand> RemoteCommands { get; set; }

        [JsonIgnore]
        public LockedList<UserQuoteViewModel> UserQuotes { get; set; }

        [JsonIgnore]
        public LockedList<string> FilteredWords { get; set; }
        [JsonIgnore]
        public LockedList<string> BannedWords { get; set; }
        [JsonIgnore]
        public LockedList<string> CommunityFilteredWords { get; set; }

        [JsonIgnore]
        public LockedDictionary<uint, List<InteractiveUserGroupViewModel>> InteractiveUserGroups { get; set; }

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

            this.Version = DesktopChannelSettings.LatestVersion;
        }

        public DesktopChannelSettings()
            : base()
        {
            this.DefaultStreamingSoftware = StreamingSoftwareTypeEnum.OBSStudio;

            this.TimerCommandsInterval = 10;
            this.TimerCommandsMinimumMessages = 10;

            this.GameQueueRequirements = new RequirementViewModel();

            this.GiveawayCommand = "giveaway";
            this.GiveawayTimer = 1;
            this.GiveawayRequirements = new RequirementViewModel();
            this.GiveawayReminderInterval = 5;
            this.GiveawayRequireClaim = true;

            this.MaxMessagesInChat = 100;
            this.ChatFontSize = 13;
            this.ChatUserJoinLeaveColorScheme = this.ChatEventAlertsColorScheme = this.ChatInteractiveAlertsColorScheme = ColorSchemes.DefaultColorScheme;

            this.ModerationFilteredWordsExcempt = MixerRoleEnum.Mod;
            this.ModerationChatTextExcempt = MixerRoleEnum.Mod;
            this.ModerationBlockLinksExcempt = MixerRoleEnum.Mod;
            this.ModerationTimeoutExempt = MixerRoleEnum.Mod;

            this.ModerationCapsBlockIsPercentage = true;
            this.ModerationPunctuationBlockIsPercentage = true;
            this.ModerationEmoteBlockIsPercentage = true;

            this.UserData = new DatabaseDictionary<uint, UserDataViewModel>();
            this.Currencies = new LockedDictionary<Guid, UserCurrencyViewModel>();
            this.CooldownGroups = new LockedDictionary<string, int>();
            this.PreMadeChatCommandSettings = new LockedList<PreMadeChatCommandSettings>();
            this.ChatCommands = new LockedList<ChatCommand>();
            this.EventCommands = new LockedList<EventCommand>();
            this.InteractiveCommands = new LockedList<InteractiveCommand>();
            this.TimerCommands = new LockedList<TimerCommand>();
            this.ActionGroupCommands = new LockedList<ActionGroupCommand>();
            this.GameCommands = new LockedList<GameCommandBase>();
            this.RemoteCommands = new LockedList<RemoteCommand>();
            this.UserQuotes = new LockedList<UserQuoteViewModel>();
            this.FilteredWords = new LockedList<string>();
            this.BannedWords = new LockedList<string>();
            this.CommunityFilteredWords = new LockedList<string>();
            this.InteractiveUserGroups = new LockedDictionary<uint, List<InteractiveUserGroupViewModel>>();
        }

        public async Task Initialize()
        {
            this.Currencies = new LockedDictionary<Guid, UserCurrencyViewModel>(this.currenciesInternal);
            this.PreMadeChatCommandSettings = new LockedList<PreMadeChatCommandSettings>(this.preMadeChatCommandSettingsInternal);
            this.CooldownGroups = new LockedDictionary<string, int>(this.cooldownGroupsInternal);
            this.ChatCommands = new LockedList<ChatCommand>(this.chatCommandsInternal);
            this.EventCommands = new LockedList<EventCommand>(this.eventCommandsInternal);
            this.InteractiveCommands = new LockedList<InteractiveCommand>(this.interactiveCommandsInternal);
            this.TimerCommands = new LockedList<TimerCommand>(this.timerCommandsInternal);
            this.ActionGroupCommands = new LockedList<ActionGroupCommand>(this.actionGroupCommandsInternal);
            this.GameCommands = new LockedList<GameCommandBase>(this.gameCommandsInternal);
            this.RemoteCommands = new LockedList<RemoteCommand>(this.remoteCommandsInternal);
            this.UserQuotes = new LockedList<UserQuoteViewModel>(this.userQuotesInternal);
            this.FilteredWords = new LockedList<string>(this.filteredWordsInternal);
            this.BannedWords = new LockedList<string>(this.bannedWordsInternal);
            this.InteractiveUserGroups = new LockedDictionary<uint, List<InteractiveUserGroupViewModel>>(this.interactiveUserGroupsInternal);

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
                if (MixItUp.Base.Util.Logger.IsDebug)
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

            if (ChannelSession.Connection != null)
            {
                this.OAuthToken = ChannelSession.Connection.Connection.GetOAuthTokenCopy();
            }
            if (ChannelSession.BotConnection != null)
            {
                this.BotOAuthToken = ChannelSession.BotConnection.Connection.GetOAuthTokenCopy();
            }

            if (ChannelSession.Services.Streamlabs != null)
            {
                this.StreamlabsOAuthToken = ChannelSession.Services.Streamlabs.GetOAuthTokenCopy();
            }
            if (ChannelSession.Services.GameWisp != null)
            {
                this.GameWispOAuthToken = ChannelSession.Services.GameWisp.GetOAuthTokenCopy();
            }
            if (ChannelSession.Services.GawkBox != null)
            {
                this.GawkBoxOAuthToken = ChannelSession.Services.GawkBox.GetOAuthTokenCopy();
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

            this.currenciesInternal = this.Currencies.ToDictionary();
            this.preMadeChatCommandSettingsInternal = this.PreMadeChatCommandSettings.ToList();
            this.cooldownGroupsInternal = this.CooldownGroups.ToDictionary();
            this.chatCommandsInternal = this.ChatCommands.ToList();
            this.eventCommandsInternal = this.EventCommands.ToList();
            this.interactiveCommandsInternal = this.InteractiveCommands.ToList();
            this.timerCommandsInternal = this.TimerCommands.ToList();
            this.actionGroupCommandsInternal = this.ActionGroupCommands.ToList();
            this.gameCommandsInternal = this.GameCommands.ToList();
            this.remoteCommandsInternal = this.RemoteCommands.ToList();
            this.userQuotesInternal = this.UserQuotes.ToList();
            this.filteredWordsInternal = this.FilteredWords.ToList();
            this.bannedWordsInternal = this.BannedWords.ToList();
            this.interactiveUserGroupsInternal = this.InteractiveUserGroups.ToDictionary();

            if (this.IsStreamer)
            {
                await this.RemoveDuplicateUsers();

                IEnumerable<uint> removedUsers = this.UserData.GetRemovedValues();
                await this.DatabaseWrapper.RunBulkWriteCommand("DELETE FROM Users WHERE ID = @ID", removedUsers.Select(u => new List<SQLiteParameter>() { new SQLiteParameter("@ID", value: (int)u) }));

                IEnumerable<UserDataViewModel> addedUsers = this.UserData.GetAddedValues();
                addedUsers = addedUsers.Where(u => !string.IsNullOrEmpty(u.UserName));
                await this.DatabaseWrapper.RunBulkWriteCommand("INSERT INTO Users(ID, UserName, ViewingMinutes, CurrencyAmounts, CustomCommands, Options) VALUES(?,?,?,?,?,?)",
                    addedUsers.Select(u => new List<SQLiteParameter>() { new SQLiteParameter(DbType.UInt32, u.ID), new SQLiteParameter(DbType.String, value: u.UserName),
                    new SQLiteParameter(DbType.Int32, value: u.ViewingMinutes), new SQLiteParameter(DbType.String, value: u.GetCurrencyAmountsString()),
                    new SQLiteParameter(DbType.String, value: u.GetCustomCommandsString()), new SQLiteParameter(DbType.String, value: u.GetOptionsString()) }));

                IEnumerable<UserDataViewModel> changedUsers = this.UserData.GetChangedValues();
                changedUsers = changedUsers.Where(u => !string.IsNullOrEmpty(u.UserName));
                await this.DatabaseWrapper.RunBulkWriteCommand("UPDATE Users SET UserName = @UserName, ViewingMinutes = @ViewingMinutes, CurrencyAmounts = @CurrencyAmounts," +
                    " CustomCommands = @CustomCommands, Options = @Options WHERE ID = @ID",
                    changedUsers.Select(u => new List<SQLiteParameter>() { new SQLiteParameter("@UserName", value: u.UserName), new SQLiteParameter("@ViewingMinutes", value: u.ViewingMinutes),
                    new SQLiteParameter("@CurrencyAmounts", value: u.GetCurrencyAmountsString()), new SQLiteParameter("@CustomCommands", value: u.GetCustomCommandsString()),
                        new SQLiteParameter("@Options", value: u.GetOptionsString()), new SQLiteParameter("@ID", value: (int)u.ID) }));
            }
        }

        public Version GetLatestVersion() { return Assembly.GetEntryAssembly().GetName().Version; }

        public bool ShouldBeUpgraded() { return this.Version < DesktopChannelSettings.LatestVersion; }

        public async Task RemoveDuplicateUsers()
        {
            if (ChannelSession.Connection != null)
            {
                var duplicateGroups = this.UserData.Values.GroupBy(u => u.UserName).Where(g => g.Count() > 1);
                foreach (var duplicateGroup in duplicateGroups)
                {
                    UserModel onlineUser = null;
                    if (!string.IsNullOrEmpty(duplicateGroup.Key))
                    {
                        onlineUser = await ChannelSession.Connection.GetUser(duplicateGroup.Key);
                    }

                    if (onlineUser != null)
                    {
                        List<UserDataViewModel> dupeUsers = new List<UserDataViewModel>(duplicateGroup);
                        if (dupeUsers.Count > 0)
                        {
                            UserDataViewModel solidUser = dupeUsers.FirstOrDefault(u => u.ID == onlineUser.id);
                            if (solidUser != null)
                            {
                                dupeUsers.Remove(solidUser);
                                foreach (UserDataViewModel dupeUser in dupeUsers)
                                {
                                    solidUser.ViewingMinutes += dupeUser.ViewingMinutes;
                                    foreach (var kvp in dupeUser.CurrencyAmounts)
                                    {
                                        solidUser.AddCurrencyAmount(kvp.Key, kvp.Value.Amount);
                                    }
                                }

                                foreach (UserDataViewModel dupeUser in dupeUsers)
                                {
                                    this.UserData.Remove(dupeUser.ID);
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var dupeUser in duplicateGroup)
                        {
                            this.UserData.Remove(dupeUser.ID);
                        }
                    }
                }
            }
        }
    }
}
