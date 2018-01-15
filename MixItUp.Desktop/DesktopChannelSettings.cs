using Mixer.Base.Model.Channel;
using Mixer.Base.Model.OAuth;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Interactive;
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
        public const int LatestVersion = 4;

        [JsonProperty]
        public int Version { get; set; }

        [JsonProperty]
        public bool DiagnosticLogging { get; set; }

        [JsonProperty]
        public bool IsStreamer { get; set; }

        [JsonProperty]
        public OAuthTokenModel OAuthToken { get; set; }
        [JsonProperty]
        public OAuthTokenModel BotOAuthToken { get; set; }

        [JsonProperty]
        public ExpandedChannelModel Channel { get; set; }

        [JsonProperty]
        public bool GameQueueMustFollow { get; set; }
        [JsonProperty]
        public bool GameQueueSubPriority { get; set; }
        [JsonProperty]
        public UserCurrencyRequirementViewModel GameQueueRankRequirement { get; set; }
        [JsonProperty]
        public UserCurrencyRequirementViewModel GameQueueCurrencyRequirement { get; set; }

        [JsonProperty]
        public bool QuotesEnabled { get; set; }

        [JsonProperty]
        public int TimerCommandsInterval { get; set; }
        [JsonProperty]
        public int TimerCommandsMinimumMessages { get; set; }

        [JsonProperty]
        public UserRole GiveawayUserRole { get; set; }
        [JsonProperty]
        public string GiveawayCommand { get; set; }
        [JsonProperty]
        public int GiveawayTimer { get; set; }
        [JsonProperty]
        public UserCurrencyRequirementViewModel GiveawayRankRequirement { get; set; }
        [JsonProperty]
        public UserCurrencyRequirementViewModel GiveawayCurrencyRequirement { get; set; }

        [JsonProperty]
        public bool ModerationUseCommunityBannedWords { get; set; }
        [JsonProperty]
        public int ModerationCapsBlockCount { get; set; }
        [JsonProperty]
        public int ModerationPunctuationBlockCount { get; set; }
        [JsonProperty]
        public int ModerationEmoteBlockCount { get; set; }
        [JsonProperty]
        public bool ModerationBlockLinks { get; set; }
        [JsonProperty]
        public bool ModerationIncludeModerators { get; set; }
        [JsonProperty]
        public int ModerationTimeout1MinuteOffenseCount { get; set; }
        [JsonProperty]
        public int ModerationTimeout5MinuteOffenseCount { get; set; }

        [JsonProperty]
        public bool EnableOverlay { get; set; }
        [JsonProperty]
        public string OBSStudioServerIP { get; set; }
        [JsonProperty]
        public string OBSStudioServerPassword { get; set; }
        [JsonProperty]
        public bool EnableXSplitConnection { get; set; }

        [JsonProperty]
        public int MaxMessagesInChat { get; set; }

        [JsonProperty]
        protected Dictionary<string, UserCurrencyViewModel> currenciesInternal { get; set; }

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
        protected List<string> quotesInternal { get; set; }
        [JsonProperty]
        protected List<string> bannedWordsInternal { get; set; }

        [JsonProperty]
        protected Dictionary<uint, List<InteractiveUserGroupViewModel>> interactiveUserGroupsInternal { get; set; }
        [JsonProperty]
        protected Dictionary<string, int> interactiveCooldownGroupsInternal { get; set; }


        public DesktopSavableChannelSettings()
        {
            this.currenciesInternal = new Dictionary<string, UserCurrencyViewModel>();
            this.preMadeChatCommandSettingsInternal = new List<PreMadeChatCommandSettings>();
            this.chatCommandsInternal = new List<ChatCommand>();
            this.eventCommandsInternal = new List<EventCommand>();
            this.interactiveCommandsInternal = new List<InteractiveCommand>();
            this.timerCommandsInternal = new List<TimerCommand>();
            this.actionGroupCommandsInternal = new List<ActionGroupCommand>();
            this.gameCommandsInternal = new List<GameCommandBase>();
            this.quotesInternal = new List<string>();
            this.bannedWordsInternal = new List<string>();
            this.interactiveUserGroupsInternal = new Dictionary<uint, List<InteractiveUserGroupViewModel>>();
            this.interactiveCooldownGroupsInternal = new Dictionary<string, int>();
        }
    }

    [DataContract]
    public class DesktopChannelSettings : DesktopSavableChannelSettings, IChannelSettings
    {
        private const string CommunityBannedWordsFilePath = "Assets\\CommunityBannedWords.txt";

        [JsonIgnore]
        public DatabaseDictionary<uint, UserDataViewModel> UserData { get; set; }

        [JsonIgnore]
        public LockedDictionary<string, UserCurrencyViewModel> Currencies { get; set; }

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
        public LockedList<string> Quotes { get; set; }
        [JsonIgnore]
        public LockedList<string> BannedWords { get; set; }
        [JsonIgnore]
        public LockedList<string> CommunityBannedWords { get; set; }

        [JsonIgnore]
        public LockedDictionary<uint, List<InteractiveUserGroupViewModel>> InteractiveUserGroups { get; set; }
        [JsonIgnore]
        public LockedDictionary<string, int> InteractiveCooldownGroups { get; set; }

        [JsonIgnore]
        public string DatabasePath { get; set; }
        [JsonIgnore]
        private SQLiteDatabaseWrapper databaseWrapper;

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
            this.TimerCommandsInterval = 10;
            this.TimerCommandsMinimumMessages = 10;

            this.GiveawayCommand = "giveaway";
            this.GiveawayUserRole = UserRole.User;
            this.GiveawayTimer = 60;

            this.MaxMessagesInChat = 100;

            this.UserData = new DatabaseDictionary<uint, UserDataViewModel>();
            this.Currencies = new LockedDictionary<string, UserCurrencyViewModel>();
            this.PreMadeChatCommandSettings = new LockedList<PreMadeChatCommandSettings>();
            this.ChatCommands = new LockedList<ChatCommand>();
            this.EventCommands = new LockedList<EventCommand>();
            this.InteractiveCommands = new LockedList<InteractiveCommand>();
            this.TimerCommands = new LockedList<TimerCommand>();
            this.ActionGroupCommands = new LockedList<ActionGroupCommand>();
            this.gameCommandsInternal = new List<GameCommandBase>();
            this.Quotes = new LockedList<string>();
            this.BannedWords = new LockedList<string>();
            this.CommunityBannedWords = new LockedList<string>();
            this.InteractiveUserGroups = new LockedDictionary<uint, List<InteractiveUserGroupViewModel>>();
            this.InteractiveCooldownGroups = new LockedDictionary<string, int>();
        }

        public async Task Initialize()
        {
            this.Currencies = new LockedDictionary<string, UserCurrencyViewModel>(this.currenciesInternal);
            this.PreMadeChatCommandSettings = new LockedList<PreMadeChatCommandSettings>(this.preMadeChatCommandSettingsInternal);
            this.ChatCommands = new LockedList<ChatCommand>(this.chatCommandsInternal);
            this.EventCommands = new LockedList<EventCommand>(this.eventCommandsInternal);
            this.InteractiveCommands = new LockedList<InteractiveCommand>(this.interactiveCommandsInternal);
            this.TimerCommands = new LockedList<TimerCommand>(this.timerCommandsInternal);
            this.ActionGroupCommands = new LockedList<ActionGroupCommand>(this.actionGroupCommandsInternal);
            this.GameCommands = new LockedList<GameCommandBase>(this.gameCommandsInternal);
            this.Quotes = new LockedList<string>(this.quotesInternal);
            this.BannedWords = new LockedList<string>(this.bannedWordsInternal);
            this.InteractiveUserGroups = new LockedDictionary<uint, List<InteractiveUserGroupViewModel>>(this.interactiveUserGroupsInternal);
            this.InteractiveCooldownGroups = new LockedDictionary<string, int>(this.interactiveCooldownGroupsInternal);

            if (File.Exists(DesktopChannelSettings.CommunityBannedWordsFilePath))
            {
                this.CommunityBannedWords = new LockedList<string>(File.ReadAllLines(DesktopChannelSettings.CommunityBannedWordsFilePath));
            }

            this.databaseWrapper = new SQLiteDatabaseWrapper(this.DatabasePath);
            if (this.InitializeDB)
            {
                await this.databaseWrapper.RunReadCommand("SELECT * FROM Users", (SQLiteDataReader dataReader) =>
                {
                    UserDataViewModel userData = new UserDataViewModel(dataReader);
                    this.UserData.Add(userData.ID, userData);
                });
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

            this.currenciesInternal = this.Currencies.ToDictionary();
            this.preMadeChatCommandSettingsInternal = this.PreMadeChatCommandSettings.ToList();
            this.chatCommandsInternal = this.ChatCommands.ToList();
            this.eventCommandsInternal = this.EventCommands.ToList();
            this.interactiveCommandsInternal = this.InteractiveCommands.ToList();
            this.timerCommandsInternal = this.TimerCommands.ToList();
            this.actionGroupCommandsInternal = this.ActionGroupCommands.ToList();
            this.gameCommandsInternal = this.GameCommands.ToList();
            this.quotesInternal = this.Quotes.ToList();
            this.bannedWordsInternal = this.BannedWords.ToList();
            this.interactiveUserGroupsInternal = this.InteractiveUserGroups.ToDictionary();
            this.interactiveCooldownGroupsInternal = this.InteractiveCooldownGroups.ToDictionary();

            IEnumerable<UserDataViewModel> addedUsers = this.UserData.GetAddedValues();
            await this.databaseWrapper.RunBulkWriteCommand("INSERT INTO Users(ID, UserName, ViewingMinutes, CurrencyAmounts) VALUES(?,?,?,?)",
                addedUsers.Select(u => new List<SQLiteParameter>() { new SQLiteParameter(DbType.UInt32, u.ID), new SQLiteParameter(DbType.String, value: u.UserName),
                    new SQLiteParameter(DbType.Int32, value: u.ViewingMinutes), new SQLiteParameter(DbType.String, value: u.GetCurrencyAmountsString()) }));

            IEnumerable<UserDataViewModel> changedUsers = this.UserData.GetChangedValues();
            await this.databaseWrapper.RunBulkWriteCommand("UPDATE Users SET UserName = @UserName, ViewingMinutes = @ViewingMinutes, CurrencyAmounts = @CurrencyAmounts WHERE ID = @ID",
                changedUsers.Select(u => new List<SQLiteParameter>() { new SQLiteParameter("@UserName", value: u.UserName), new SQLiteParameter("@ViewingMinutes", value: u.ViewingMinutes),
                    new SQLiteParameter("@CurrencyAmounts", value: u.GetCurrencyAmountsString()), new SQLiteParameter("@ID", value: (int)u.ID) }));

            IEnumerable<uint> removedUsers = this.UserData.GetRemovedValues();
            await this.databaseWrapper.RunBulkWriteCommand("DELETE FROM Users WHERE ID = @ID", removedUsers.Select(u => new List<SQLiteParameter>() { new SQLiteParameter("@ID", value: (int)u) }));
        }

        public Version GetLatestVersion() { return Assembly.GetEntryAssembly().GetName().Version; }

        public bool ShouldBeUpgraded() { return this.Version < DesktopChannelSettings.LatestVersion; }
    }

    [DataContract]
    public class LegacyDesktopChannelSettings : DesktopChannelSettings
    {
        [JsonProperty]
        public bool CurrencyEnabled { get; set; }
        [JsonProperty]
        public string CurrencyName { get; set; }
        [JsonProperty]
        public int CurrencyAcquireAmount { get; set; }
        [JsonProperty]
        public int CurrencyAcquireInterval { get; set; }

        [JsonProperty]
        public UserCurrencyViewModel CurrencyAcquisition { get; set; }

        [JsonProperty]
        public UserCurrencyViewModel RankAcquisition { get; set; }
        [JsonProperty]
        public List<UserRankViewModel> Ranks { get; set; }
        [JsonProperty]
        public CustomCommand RankChangedCommand { get; set; }

        [JsonProperty]
        public UserRankViewModel GameQueueMinimumRank { get; set; }
        [JsonProperty]
        public int GameQueueCurrencyCost { get; set; }

        [JsonProperty]
        public int GiveawayCurrencyCost { get; set; }
        [JsonProperty]
        public string GiveawayUserRank { get; set; }

        [JsonProperty]
        public List<UserDataViewModel> userDataInternal { get; set; }

        [JsonProperty]
        public new Dictionary<uint, List<InteractiveUserGroupViewModel>> InteractiveUserGroups { get; set; }
        [JsonProperty]
        public new Dictionary<string, int> InteractiveCooldownGroups { get; set; }
    }
}
