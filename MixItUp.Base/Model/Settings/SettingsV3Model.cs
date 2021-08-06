using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Model.Serial;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Dashboard;
using Newtonsoft.Json;
using StreamingClient.Base.Model.OAuth;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Settings
{
    [DataContract]
    public class SettingsV3Model
    {
        public const int LatestVersion = 3;

        public const string SettingsDirectoryName = "Settings";
        public const string DefaultAutomaticBackupSettingsDirectoryName = "AutomaticBackups";

        public const string SettingsTemplateDatabaseFileName = "SettingsTemplateDatabase.db";

        public const string SettingsFileExtension = "miu3";
        public const string DatabaseFileExtension = "db3";
        public const string SettingsLocalBackupFileExtension = "backup";

        public const string SettingsBackupFileExtension = "miubackup";

        [DataMember]
        public int Version { get; set; } = SettingsV3Model.LatestVersion;

        [DataMember]
        public Guid ID { get; set; } = Guid.NewGuid();
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string TelemetryUserID { get; set; }

        [DataMember]
        public string SettingsBackupLocation { get; set; }
        [DataMember]
        public SettingsBackupRateEnum SettingsBackupRate { get; set; }
        [DataMember]
        public DateTimeOffset SettingsLastBackup { get; set; }

        #region Authentication

        [DataMember]
        public Dictionary<StreamingPlatformTypeEnum, StreamingPlatformAuthenticationSettingsModel> StreamingPlatformAuthentications { get; set; } = new Dictionary<StreamingPlatformTypeEnum, StreamingPlatformAuthenticationSettingsModel>();

        [DataMember]
        public OAuthTokenModel StreamlabsOAuthToken { get; set; }
        [DataMember]
        public OAuthTokenModel StreamElementsOAuthToken { get; set; }
        [DataMember]
        public OAuthTokenModel TwitterOAuthToken { get; set; }
        [DataMember]
        public OAuthTokenModel DiscordOAuthToken { get; set; }
        [DataMember]
        public OAuthTokenModel TiltifyOAuthToken { get; set; }
        [DataMember]
        public OAuthTokenModel TipeeeStreamOAuthToken { get; set; }
        [DataMember]
        public OAuthTokenModel TreatStreamOAuthToken { get; set; }
        [DataMember]
        public OAuthTokenModel PatreonOAuthToken { get; set; }
        [DataMember]
        public OAuthTokenModel IFTTTOAuthToken { get; set; }
        [DataMember]
        public OAuthTokenModel StreamlootsOAuthToken { get; set; }
        [DataMember]
        public OAuthTokenModel JustGivingOAuthToken { get; set; }
        [DataMember]
        public OAuthTokenModel RainMakerOAuthToken { get; set; }
        [DataMember]
        public OAuthTokenModel PixelChatOAuthToken { get; set; }
        [DataMember]
        public OAuthTokenModel VTubeStudioOAuthToken { get; set; }

        #endregion Authentication

        #region General

        [DataMember]
        public bool OptOutTracking { get; set; }
        [DataMember]
        public bool FeatureMe { get; set; }
        [DataMember]
        public StreamingSoftwareTypeEnum DefaultStreamingSoftware { get; set; } = StreamingSoftwareTypeEnum.OBSStudio;
        [DataMember]
        public string DefaultAudioOutput { get; set; }

        #endregion General

        #region Chat

        [DataMember]
        public int MaxMessagesInChat { get; set; } = 100;
        [DataMember]
        public int MaxUsersShownInChat { get; set; } = 100;

        [DataMember]
        public bool SaveChatEventLogs { get; set; }

        [DataMember]
        public int ChatFontSize { get; set; } = 13;
        [DataMember]
        public bool AddSeparatorsBetweenMessages { get; set; }
        [DataMember]
        public bool UseAlternatingBackgroundColors { get; set; }

        [DataMember]
        public bool OnlyShowAlertsInDashboard { get; set; }
        [DataMember]
        public bool LatestChatAtTop { get; set; }
        [DataMember]
        public bool TrackWhispererNumber { get; set; }
        [DataMember]
        public bool ShowChatMessageTimestamps { get; set; }

        [DataMember]
        public bool HideViewerAndChatterNumbers { get; set; }
        [DataMember]
        public bool HideChatUserList { get; set; }
        [DataMember]
        public bool HideDeletedMessages { get; set; }
        [DataMember]
        public bool HideBotMessages { get; set; }

        [DataMember]
        public bool ShowBetterTTVEmotes { get; set; }
        [DataMember]
        public bool ShowFrankerFaceZEmotes { get; set; }

        [DataMember]
        public bool HideUserAvatar { get; set; }
        [DataMember]
        public bool HideUserRoleBadge { get; set; }
        [DataMember]
        public bool HideUserSubscriberBadge { get; set; }
        [DataMember]
        public bool HideUserSpecialtyBadge { get; set; }

        [DataMember]
        public bool UseCustomUsernameColors { get; set; }
        [DataMember]
        public Dictionary<UserRoleEnum, string> CustomUsernameColors { get; set; } = new Dictionary<UserRoleEnum, string>();

        #endregion Chat

        #region Commands

        [DataMember]
        public bool AllowCommandWhispering { get; set; }
        [DataMember]
        public bool IgnoreBotAccountCommands { get; set; }
        [DataMember]
        public bool DeleteChatCommandsWhenRun { get; set; }
        [DataMember]
        [Obsolete]
        public bool UnlockAllCommands { get; set; }
        [DataMember]
        public CommandServiceLockTypeEnum CommandServiceLockType { get; set; } = CommandServiceLockTypeEnum.PerCommandType;

        [DataMember]
        public RequirementErrorCooldownTypeEnum RequirementErrorsCooldownType { get; set; } = RequirementErrorCooldownTypeEnum.Default;
        [DataMember]
        public int RequirementErrorsCooldownAmount { get; set; } = 10;
        [DataMember]
        public bool IncludeUsernameWithRequirementErrors { get; set; }

        [DataMember]
        public int TwitchMassGiftedSubsFilterAmount { get; set; } = 1;
        [DataMember]
        public bool TwitchReplyToCommandChatMessages { get; set; }

        [DataMember]
        public HashSet<ActionTypeEnum> ActionsToHide { get; set; } = new HashSet<ActionTypeEnum>();

        [DataMember]
        public Dictionary<string, CommandGroupSettingsModel> CommandGroups { get; set; } = new Dictionary<string, CommandGroupSettingsModel>();
        [DataMember]
        public Dictionary<string, int> CooldownGroupAmounts { get; set; } = new Dictionary<string, int>();

        [DataMember]
        public List<PreMadeChatCommandSettingsModel> PreMadeChatCommandSettings { get; set; } = new List<PreMadeChatCommandSettingsModel>();

        #endregion Commands

        #region Alerts

        [DataMember]
        public string AlertUserJoinLeaveColor { get; set; }
        [DataMember]
        public string AlertFollowColor { get; set; }
        [DataMember]
        public string AlertHostColor { get; set; }
        [DataMember]
        public string AlertRaidColor { get; set; }
        [DataMember]
        public string AlertSubColor { get; set; }
        [DataMember]
        public string AlertGiftedSubColor { get; set; }
        [DataMember]
        public string AlertMassGiftedSubColor { get; set; }
        [DataMember]
        public string AlertBitsCheeredColor { get; set; }
        [DataMember]
        public string AlertHypeTrainColor { get; set; }
        [DataMember]
        public string AlertDonationColor { get; set; }
        [DataMember]
        public string AlertChannelPointsColor { get; set; }
        [DataMember]
        public string AlertModerationColor { get; set; }
        [DataMember]
        public string AlertStreamlootsColor { get; set; }

        #endregion Alerts

        #region Notifications

        [DataMember]
        public string NotificationsAudioOutput { get; set; }

        [DataMember]
        public string NotificationChatMessageSoundFilePath { get; set; }
        [DataMember]
        public int NotificationChatMessageSoundVolume { get; set; } = 100;
        [DataMember]
        public string NotificationChatTaggedSoundFilePath { get; set; }
        [DataMember]
        public int NotificationChatTaggedSoundVolume { get; set; } = 100;
        [DataMember]
        public string NotificationChatWhisperSoundFilePath { get; set; }
        [DataMember]
        public int NotificationChatWhisperSoundVolume { get; set; } = 100;
        [DataMember]
        public string NotificationServiceConnectSoundFilePath { get; set; }
        [DataMember]
        public int NotificationServiceConnectSoundVolume { get; set; } = 100;
        [DataMember]
        public string NotificationServiceDisconnectSoundFilePath { get; set; }
        [DataMember]
        public int NotificationServiceDisconnectSoundVolume { get; set; } = 100;

        #endregion Notifications

        #region Users

        [DataMember]
        public int RegularUserMinimumHours { get; set; }
        [DataMember]
        public bool ExplicitUserRoleRequirements { get; set; }
        [DataMember]
        public List<UserTitleModel> UserTitles { get; set; } = new List<UserTitleModel>();

        #endregion Users

        #region Game Queue

        [DataMember]
        public bool GameQueueSubPriority { get; set; }
        [DataMember]
        public Guid GameQueueUserJoinedCommandID { get; set; }
        [DataMember]
        public Guid GameQueueUserSelectedCommandID { get; set; }

        #endregion Game Queue

        #region Quotes

        [DataMember]
        public bool QuotesEnabled { get; set; }
        [DataMember]
        public string QuotesFormat { get; set; }

        #endregion Quotes

        #region Timers

        [DataMember]
        public int TimerCommandsInterval { get; set; } = 10;
        [DataMember]
        public int TimerCommandsMinimumMessages { get; set; } = 10;
        [DataMember]
        public bool RandomizeTimers { get; set; }
        [DataMember]
        public bool DisableAllTimers { get; set; }

        #endregion Timers

        #region Giveaway

        [DataMember]
        public string GiveawayCommand { get; set; } = "giveaway";
        [DataMember]
        public int GiveawayTimer { get; set; } = 1;
        [DataMember]
        public int GiveawayMaximumEntries { get; set; } = 1;
        [DataMember]
        public RequirementsSetModel GiveawayRequirementsSet { get; set; } = new RequirementsSetModel();
        [DataMember]
        public int GiveawayReminderInterval { get; set; } = 5;
        [DataMember]
        public bool GiveawayRequireClaim { get; set; } = true;
        [DataMember]
        public bool GiveawayAllowPastWinners { get; set; }

        [DataMember]
        public Guid GiveawayStartedReminderCommandID { get; set; }
        [DataMember]
        public Guid GiveawayUserJoinedCommandID { get; set; }
        [DataMember]
        public Guid GiveawayWinnerSelectedCommandID { get; set; }

        #endregion Giveaway

        #region Moderation

        [DataMember]
        public bool ModerationUseCommunityFilteredWords { get; set; }
        [DataMember]
        public List<string> FilteredWords { get; set; } = new List<string>();
        [DataMember]
        public List<string> BannedWords { get; set; } = new List<string>();

        [DataMember]
        public int ModerationFilteredWordsTimeout1MinuteOffenseCount { get; set; }
        [DataMember]
        public int ModerationFilteredWordsTimeout5MinuteOffenseCount { get; set; }
        [DataMember]
        public UserRoleEnum ModerationFilteredWordsExcempt { get; set; } = UserRoleEnum.Mod;
        [DataMember]
        public bool ModerationFilteredWordsApplyStrikes { get; set; } = true;

        [DataMember]
        public int ModerationCapsBlockCount { get; set; }
        [DataMember]
        public bool ModerationCapsBlockIsPercentage { get; set; } = true;
        [DataMember]
        public int ModerationPunctuationBlockCount { get; set; }
        [DataMember]
        public bool ModerationPunctuationBlockIsPercentage { get; set; } = true;
        [DataMember]
        public UserRoleEnum ModerationChatTextExcempt { get; set; } = UserRoleEnum.Mod;
        [DataMember]
        public bool ModerationChatTextApplyStrikes { get; set; } = true;

        [DataMember]
        public bool ModerationBlockLinks { get; set; }
        [DataMember]
        public UserRoleEnum ModerationBlockLinksExcempt { get; set; } = UserRoleEnum.Mod;
        [DataMember]
        public bool ModerationBlockLinksApplyStrikes { get; set; } = true;

        [DataMember]
        public ModerationChatInteractiveParticipationEnum ModerationChatInteractiveParticipation { get; set; } = ModerationChatInteractiveParticipationEnum.None;
        [DataMember]
        public UserRoleEnum ModerationChatInteractiveParticipationExcempt { get; set; } = UserRoleEnum.Mod;

        [DataMember]
        public bool ModerationFollowEvent { get; set; }
        [DataMember]
        public int ModerationFollowEventMaxInQueue { get; set; } = 10;

        [DataMember]
        public bool ModerationResetStrikesOnLaunch { get; set; }

        [DataMember]
        public Guid ModerationStrike1CommandID { get; set; }
        [DataMember]
        public Guid ModerationStrike2CommandID { get; set; }
        [DataMember]
        public Guid ModerationStrike3CommandID { get; set; }

        #endregion Moderation

        #region Overlay

        [DataMember]
        public bool EnableOverlay { get; set; }
        [DataMember]
        public Dictionary<string, int> OverlayCustomNameAndPorts { get; set; } = new Dictionary<string, int>();
        [DataMember]
        public string OverlaySourceName { get; set; }

        [DataMember]
        public List<OverlayWidgetModel> OverlayWidgets { get; set; } = new List<OverlayWidgetModel>();
        [DataMember]
        public int OverlayWidgetRefreshTime { get; set; } = 5;

        #endregion Overlay

        #region Services

        [DataMember]
        public string OvrStreamServerIP { get; set; }

        [DataMember]
        public string OBSStudioServerIP { get; set; }
        [DataMember]
        public string OBSStudioServerPassword { get; set; }

        [DataMember]
        public bool EnableStreamlabsOBSConnection { get; set; }

        [DataMember]
        public bool EnableXSplitConnection { get; set; }

        [DataMember]
        public bool EnableDeveloperAPI { get; set; }
        [DataMember]
        public bool EnableDeveloperAPIAdvancedMode { get; set; }

        [DataMember]
        public int TiltifyCampaign { get; set; }

        [DataMember]
        public int ExtraLifeTeamID { get; set; }
        [DataMember]
        public int ExtraLifeParticipantID { get; set; }
        [DataMember]
        public bool ExtraLifeIncludeTeamDonations { get; set; }

        [DataMember]
        public string JustGivingPageShortName { get; set; }

        [DataMember]
        public string DiscordServer { get; set; }
        [DataMember]
        public string DiscordCustomClientID { get; set; }
        [DataMember]
        public string DiscordCustomClientSecret { get; set; }
        [DataMember]
        public string DiscordCustomBotToken { get; set; }

        [DataMember]
        public string PatreonTierSubscriberEquivalent { get; set; }

        [DataMember]
        public List<SerialDeviceModel> SerialDevices { get; set; } = new List<SerialDeviceModel>();

        #endregion Services

        #region Dashboard

        [DataMember]
        public DashboardLayoutTypeEnum DashboardLayout { get; set; }
        [DataMember]
        public List<DashboardItemTypeEnum> DashboardItems { get; set; } = new List<DashboardItemTypeEnum>();
        [DataMember]
        public List<Guid> DashboardQuickCommands { get; set; } = new List<Guid>();

        #endregion Dashboard

        #region Advanced

        [DataMember]
        public bool ReRunWizard { get; set; }

        #endregion Advanced

        #region Currency

        [DataMember]
        public Dictionary<Guid, CurrencyModel> Currency { get; set; } = new Dictionary<Guid, CurrencyModel>();

        [DataMember]
        public Dictionary<Guid, InventoryModel> Inventory { get; set; } = new Dictionary<Guid, InventoryModel>();

        [DataMember]
        public Dictionary<Guid, StreamPassModel> StreamPass { get; set; } = new Dictionary<Guid, StreamPassModel>();

        [DataMember]
        public bool RedemptionStoreEnabled { get; set; }
        [DataMember]
        public Dictionary<Guid, RedemptionStoreProductModel> RedemptionStoreProducts { get; set; } = new Dictionary<Guid, RedemptionStoreProductModel>();
        [DataMember]
        public string RedemptionStoreChatPurchaseCommand { get; set; } = "!purchase";
        [DataMember]
        public string RedemptionStoreModRedeemCommand { get; set; } = "!redeem";
        [DataMember]
        public Guid RedemptionStoreManualRedeemNeededCommandID { get; set; }
        [DataMember]
        public Guid RedemptionStoreDefaultRedemptionCommandID { get; set; }
        [DataMember]
        public List<RedemptionStorePurchaseModel> RedemptionStorePurchases { get; set; } = new List<RedemptionStorePurchaseModel>();

        #endregion Currency

        [DataMember]
        public List<string> RecentStreamTitles { get; set; } = new List<string>();
        [DataMember]
        public List<string> RecentStreamGames { get; set; } = new List<string>();

        [DataMember]
        public Dictionary<string, object> LatestSpecialIdentifiersData { get; set; } = new Dictionary<string, object>();

        [DataMember]
        public Dictionary<string, HotKeyConfiguration> HotKeys { get; set; } = new Dictionary<string, HotKeyConfiguration>();
        [DataMember]
        public Dictionary<string, CounterModel> Counters { get; set; } = new Dictionary<string, CounterModel>();

        #region Database Data

        [JsonIgnore]
        public DatabaseDictionary<Guid, CommandModelBase> Commands { get; set; } = new DatabaseDictionary<Guid, CommandModelBase>();

        [JsonIgnore]
        public DatabaseList<UserQuoteModel> Quotes { get; set; } = new DatabaseList<UserQuoteModel>();

        [JsonIgnore]
        public DatabaseDictionary<Guid, UserDataModel> UserData { get; set; } = new DatabaseDictionary<Guid, UserDataModel>();

        [JsonIgnore]
        private Dictionary<StreamingPlatformTypeEnum, LockedDictionary<string, Guid>> platformUserIDLookups { get; set; } = new Dictionary<StreamingPlatformTypeEnum, LockedDictionary<string, Guid>>();
        [JsonIgnore]
        private Dictionary<StreamingPlatformTypeEnum, LockedDictionary<string, Guid>> platformUsernameLookups { get; set; } = new Dictionary<StreamingPlatformTypeEnum, LockedDictionary<string, Guid>>();

        #endregion Database Data

        [JsonIgnore]
        public string SettingsFileName { get { return string.Format("{0}.{1}", this.ID, SettingsV3Model.SettingsFileExtension); } }
        [JsonIgnore]
        public string SettingsFilePath { get { return Path.Combine(SettingsV3Model.SettingsDirectoryName, this.SettingsFileName); } }

        [JsonIgnore]
        public string DatabaseFileName { get { return string.Format("{0}.{1}", this.ID, SettingsV3Model.DatabaseFileExtension); } }
        [JsonIgnore]
        public string DatabaseFilePath { get { return Path.Combine(SettingsV3Model.SettingsDirectoryName, this.DatabaseFileName); } }

        [JsonIgnore]
        public string SettingsLocalBackupFileName { get { return string.Format("{0}.{1}.{2}", this.ID, SettingsV3Model.SettingsFileExtension, SettingsV3Model.SettingsLocalBackupFileExtension); } }
        [JsonIgnore]
        public string SettingsLocalBackupFilePath { get { return Path.Combine(SettingsV3Model.SettingsDirectoryName, this.SettingsLocalBackupFileName); } }

        private bool fullUserDataLoadOccurred = false;

        public SettingsV3Model()
        {
            foreach (StreamingPlatformTypeEnum platform in StreamingPlatforms.SupportedPlatforms)
            {
                this.platformUserIDLookups[platform] = new LockedDictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
                this.platformUsernameLookups[platform] = new LockedDictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
            }
        }

        public SettingsV3Model(string name)
            : this()
        {
            this.Name = name;

            this.InitializeMissingData();
        }

        public async Task Initialize()
        {
            if (!ChannelSession.Services.FileService.FileExists(this.DatabaseFilePath))
            {
                await ChannelSession.Services.FileService.CopyFile(SettingsV3Model.SettingsTemplateDatabaseFileName, this.DatabaseFilePath);
            }

            await ChannelSession.Services.Database.Read(this.DatabaseFilePath, "SELECT * FROM Quotes", (Dictionary<string, object> data) =>
            {
                DateTimeOffset.TryParse((string)data["DateTime"], out DateTimeOffset dateTime);
                this.Quotes.Add(new UserQuoteModel(Convert.ToInt32(data["ID"]), data["Quote"].ToString(), dateTime, data["GameName"].ToString()));
            });
            this.Quotes.ClearTracking();

            await ChannelSession.Services.Database.Read(this.DatabaseFilePath, "SELECT * FROM Commands", (Dictionary<string, object> data) =>
            {
                CommandModelBase command = null;
                CommandTypeEnum type = (CommandTypeEnum)Convert.ToInt32(data["TypeID"]);

                string commandData = data["Data"].ToString();
                if (type == CommandTypeEnum.Chat)
                {
                    command = JSONSerializerHelper.DeserializeFromString<ChatCommandModel>(commandData);
                }
                else if (type == CommandTypeEnum.Event)
                {
                    command = JSONSerializerHelper.DeserializeFromString<EventCommandModel>(commandData);
                }
                else if (type == CommandTypeEnum.Timer)
                {
                    command = JSONSerializerHelper.DeserializeFromString<TimerCommandModel>(commandData);
                }
                else if (type == CommandTypeEnum.ActionGroup)
                {
                    command = JSONSerializerHelper.DeserializeFromString<ActionGroupCommandModel>(commandData);
                }
                else if (type == CommandTypeEnum.Game)
                {
                    commandData = commandData.Replace("MixItUp.Base.ViewModel.User.UserRoleEnum", "MixItUp.Base.Model.User.UserRoleEnum");
                    command = JSONSerializerHelper.DeserializeFromString<GameCommandModelBase>(commandData);
                }
                else if (type == CommandTypeEnum.TwitchChannelPoints)
                {
                    command = JSONSerializerHelper.DeserializeFromString<TwitchChannelPointsCommandModel>(commandData);
                }
                else if (type == CommandTypeEnum.StreamlootsCard)
                {
                    command = JSONSerializerHelper.DeserializeFromString<StreamlootsCardCommandModel>(commandData);
                }
                else if (type == CommandTypeEnum.Custom)
                {
                    command = JSONSerializerHelper.DeserializeFromString<CustomCommandModel>(commandData);
                }
                else if (type == CommandTypeEnum.UserOnlyChat)
                {
                    command = JSONSerializerHelper.DeserializeFromString<UserOnlyChatCommandModel>(commandData);
                }

                if (command != null)
                {
                    this.Commands[command.ID] = command;
                }
            });
            this.Commands.ClearTracking();

            foreach (CounterModel counter in this.Counters.Values.ToList())
            {
                if (counter.ResetOnLoad)
                {
                    await counter.ResetAmount();
                }
            }

            if (string.IsNullOrEmpty(this.TelemetryUserID))
            {
                if (ChannelSession.IsDebug())
                {
                    this.TelemetryUserID = "MixItUpDebuggingUser";
                }
                else
                {
                    this.TelemetryUserID = Guid.NewGuid().ToString();
                }
            }

            // Clear out unused Cooldown Groups and Command Groups
            var allUsedCooldownGroupNames = this.Commands.Values.ToList().Select(c => c.Requirements?.Cooldown?.GroupName).Distinct();
            var allUnusedCooldownGroupNames = this.CooldownGroupAmounts.ToList().Where(c => !allUsedCooldownGroupNames.Contains(c.Key, StringComparer.InvariantCultureIgnoreCase));
            foreach (var unused in allUnusedCooldownGroupNames)
            {
                this.CooldownGroupAmounts.Remove(unused.Key);
            }

            var allUsedCommandGroupNames = this.Commands.Values.ToList().Select(c => c.GroupName).Distinct();
            var allUnusedCommandGroupNames = this.CommandGroups.ToList().Where(c => !allUsedCommandGroupNames.Contains(c.Key, StringComparer.InvariantCultureIgnoreCase));
            foreach (var unused in allUnusedCommandGroupNames)
            {
                this.CommandGroups.Remove(unused.Key);
            }

            this.InitializeMissingData();
        }

        public void ClearMixerUserData()
        {
#pragma warning disable CS0612 // Type or member is obsolete
            foreach (UserDataModel userData in this.UserData.Values.ToList())
            {
                if (userData.MixerID > 0)
                {
                    this.UserData.Remove(userData.ID);
                }
            }
#pragma warning restore CS0612 // Type or member is obsolete
        }

        public async Task ClearAllUserData()
        {
            this.UserData.Clear();
            await ChannelSession.Services.Database.Write(this.DatabaseFilePath, "DELETE FROM Users");
        }

        public void CopyLatestValues()
        {
            Logger.Log(LogLevel.Debug, "Copying over latest values into Settings object");

            this.Version = SettingsV3Model.LatestVersion;

            if (ChannelSession.TwitchUserConnection != null)
            {
                this.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].UserOAuthToken = ChannelSession.TwitchUserConnection.Connection.GetOAuthTokenCopy();
            }
            if (ChannelSession.TwitchBotConnection != null)
            {
                this.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].BotOAuthToken = ChannelSession.TwitchBotConnection.Connection.GetOAuthTokenCopy();
            }

            if (ChannelSession.Services.Streamlabs.IsConnected)
            {
                this.StreamlabsOAuthToken = ChannelSession.Services.Streamlabs.GetOAuthTokenCopy();
            }
            if (ChannelSession.Services.StreamElements.IsConnected)
            {
                this.StreamElementsOAuthToken = ChannelSession.Services.StreamElements.GetOAuthTokenCopy();
            }
            if (ChannelSession.Services.Rainmaker.IsConnected)
            {
                this.RainMakerOAuthToken = ChannelSession.Services.Rainmaker.GetOAuthTokenCopy();
            }
            if (ChannelSession.Services.TipeeeStream.IsConnected)
            {
                this.TipeeeStreamOAuthToken = ChannelSession.Services.TipeeeStream.GetOAuthTokenCopy();
            }
            if (ChannelSession.Services.TreatStream.IsConnected)
            {
                this.TreatStreamOAuthToken = ChannelSession.Services.TreatStream.GetOAuthTokenCopy();
            }
            if (ChannelSession.Services.Streamloots.IsConnected)
            {
                this.StreamlootsOAuthToken = ChannelSession.Services.Streamloots.GetOAuthTokenCopy();
            }
            if (ChannelSession.Services.Tiltify.IsConnected)
            {
                this.TiltifyOAuthToken = ChannelSession.Services.Tiltify.GetOAuthTokenCopy();
            }
            if (ChannelSession.Services.Patreon.IsConnected)
            {
                this.PatreonOAuthToken = ChannelSession.Services.Patreon.GetOAuthTokenCopy();
            }
            if (ChannelSession.Services.IFTTT.IsConnected)
            {
                this.IFTTTOAuthToken = ChannelSession.Services.IFTTT.GetOAuthTokenCopy();
            }
            if (ChannelSession.Services.JustGiving.IsConnected)
            {
                this.JustGivingOAuthToken = ChannelSession.Services.JustGiving.GetOAuthTokenCopy();
            }
            if (ChannelSession.Services.Discord.IsConnected)
            {
                this.DiscordOAuthToken = ChannelSession.Services.Discord.GetOAuthTokenCopy();
            }
            if (ChannelSession.Services.Twitter.IsConnected)
            {
                this.TwitterOAuthToken = ChannelSession.Services.Twitter.GetOAuthTokenCopy();
            }
            if (ChannelSession.Services.PixelChat.IsConnected)
            {
                this.PixelChatOAuthToken = ChannelSession.Services.PixelChat.GetOAuthTokenCopy();
            }
            if (ChannelSession.Services.VTubeStudio.IsConnected)
            {
                this.VTubeStudioOAuthToken = ChannelSession.Services.VTubeStudio.GetOAuthTokenCopy();
            }
        }

        public async Task SaveDatabaseData()
        {
            IEnumerable<Guid> removedUsers = this.UserData.GetRemovedValues();
            await ChannelSession.Services.Database.BulkWrite(this.DatabaseFilePath, "DELETE FROM Users WHERE ID = $ID", removedUsers.Select(u => new Dictionary<string, object>() { { "$ID", u.ToString() } }));

            IEnumerable<UserDataModel> changedUsers = this.UserData.GetAddedChangedValues();
            await ChannelSession.Services.Database.BulkWrite(this.DatabaseFilePath,
                "REPLACE INTO Users(ID, TwitchID, TwitchUsername, YouTubeID, YouTubeUsername, FacebookID, FacebookUsername, TrovoID, TrovoUsername, GlimeshID, GlimeshUsername, Data) " +
                "VALUES($ID, $TwitchID, $TwitchUsername, $YouTubeID, $YouTubeUsername, $FacebookID, $FacebookUsername, $TrovoID, $TrovoUsername, $GlimeshID, $GlimeshUsername, $Data)",
                changedUsers.Select(u => new Dictionary<string, object>()
                {
                    { "$ID", u.ID.ToString() }, { "$TwitchID", u.TwitchID }, { "$TwitchUsername", u.TwitchUsername }, { "$YouTubeID", null }, { "$YouTubeUsername", null },
                    { "$FacebookID", null }, { "$FacebookUsername", null }, { "$TrovoID", null }, { "$TrovoUsername", null }, { "$GlimeshID", null }, { "$GlimeshUsername", null },
                    { "$Data", JSONSerializerHelper.SerializeToString(u) }
                }));

            List<Guid> removedCommands = new List<Guid>();
            await ChannelSession.Services.Database.BulkWrite(this.DatabaseFilePath, "DELETE FROM Commands WHERE ID = $ID",
                this.Commands.GetRemovedValues().Select(id => new Dictionary<string, object>() { { "$ID", id.ToString() } }));

            await ChannelSession.Services.Database.BulkWrite(this.DatabaseFilePath, "REPLACE INTO Commands(ID, TypeID, Data) VALUES($ID, $TypeID, $Data)",
                this.Commands.GetAddedChangedValues().Select(c => new Dictionary<string, object>() { { "$ID", c.ID.ToString() }, { "$TypeID", (int)c.Type }, { "$Data", JSONSerializerHelper.SerializeToString(c) } }));

            await ChannelSession.Services.Database.BulkWrite(this.DatabaseFilePath, "DELETE FROM Quotes WHERE ID = $ID",
                this.Quotes.GetRemovedValues().Select(q => new Dictionary<string, object>() { { "$ID", q.ID.ToString() } }));

            await ChannelSession.Services.Database.BulkWrite(this.DatabaseFilePath, "REPLACE INTO Quotes(ID, Quote, GameName, DateTime) VALUES($ID, $Quote, $GameName, $DateTime)",
                this.Quotes.GetAddedChangedValues().Select(q => new Dictionary<string, object>() { { "$ID", q.ID.ToString() }, { "$Quote", q.Quote }, { "$GameName", q.GameName }, { "$DateTime", q.DateTime.ToString() } }));
        }

        public async Task<UserDataModel> GetUserDataByID(Guid id)
        {
            if (id == Guid.Empty)
            {
                return null;
            }

            if (this.UserData.TryGetValue(id, out UserDataModel userData))
            {
                return userData;
            }

            await ChannelSession.Services.Database.Read(this.DatabaseFilePath, "SELECT * FROM Users WHERE ID = $ID",
                new Dictionary<string, object>() { { "$ID", id.ToString() } },
                (Dictionary<string, object> data) =>
                {
                    userData = JSONSerializerHelper.DeserializeFromString<UserDataModel>(data["Data"].ToString());
                });

            this.SetUserData(userData);
            return userData;
        }

        public async Task<UserDataModel> GetUserDataByPlatformID(StreamingPlatformTypeEnum platform, string platformID)
        {
            if (string.IsNullOrEmpty(platformID) || platform == StreamingPlatformTypeEnum.None || platform == StreamingPlatformTypeEnum.All)
            {
                return null;
            }

            if (this.platformUserIDLookups[platform].TryGetValue(platformID, out Guid id) && this.UserData.TryGetValue(id, out UserDataModel userData))
            {
                return userData;
            }

            string columnName = null;
            switch (platform)
            {
                case StreamingPlatformTypeEnum.Twitch: columnName = "TwitchID"; break;
            }

            userData = null;
            if (!string.IsNullOrEmpty(columnName))
            {
                await ChannelSession.Services.Database.Read(this.DatabaseFilePath, "SELECT * FROM Users WHERE " + columnName + " = $PlatformID",
                    new Dictionary<string, object>() { { "$PlatformID", platformID } },
                    (Dictionary<string, object> data) =>
                    {
                        userData = JSONSerializerHelper.DeserializeFromString<UserDataModel>(data["Data"].ToString());
                    });
            }

            this.SetUserData(userData);
            return userData;
        }

        public async Task<UserDataModel> GetUserDataByPlatformUsername(StreamingPlatformTypeEnum platform, string platformUsername)
        {
            if (string.IsNullOrEmpty(platformUsername) || platform == StreamingPlatformTypeEnum.None || platform == StreamingPlatformTypeEnum.All)
            {
                return null;
            }

            if (this.platformUsernameLookups[platform].TryGetValue(platformUsername, out Guid id) && this.UserData.TryGetValue(id, out UserDataModel userData))
            {
                return userData;
            }

            string columnName = null;
            switch (platform)
            {
                case StreamingPlatformTypeEnum.Twitch: columnName = "TwitchUsername"; break;
            }

            userData = null;
            if (!string.IsNullOrEmpty(columnName))
            {
                await ChannelSession.Services.Database.Read(this.DatabaseFilePath, "SELECT * FROM Users WHERE " + columnName + " LIKE $PlatformUsername",
                    new Dictionary<string, object>() { { "$PlatformUsername", platformUsername } },
                    (Dictionary<string, object> data) =>
                    {
                        userData = JSONSerializerHelper.DeserializeFromString<UserDataModel>(data["Data"].ToString());
                    });
            }

            this.SetUserData(userData);
            return userData;
        }

        public async Task LoadAllUserData()
        {
            if (!this.fullUserDataLoadOccurred)
            {
                this.fullUserDataLoadOccurred = true;

                await ChannelSession.Services.Database.Read(this.DatabaseFilePath, "SELECT * FROM Users", (Action<Dictionary<string, object>>)((Dictionary<string, object> data) =>
                {
                    this.SetUserData((UserDataModel)JSONSerializerHelper.DeserializeFromString<UserDataModel>((string)data[(string)"Data"].ToString()));
                }));
            }
        }

        public void SetUserData(UserDataModel userData, bool newData = false)
        {
            if (userData != null && userData.Platform != StreamingPlatformTypeEnum.None)
            {
                lock (this.UserData)
                {
                    if (!this.UserData.ContainsKey(userData.ID))
                    {
                        this.UserData[userData.ID] = userData;
                        if (newData)
                        {
                            this.UserData.ManualValueChanged(userData.ID);
                        }
                        else
                        {
                            this.UserData.ClearTracking(userData.ID);
                        }
                    }

                    if (!string.IsNullOrEmpty(userData.TwitchID))
                    {
                        this.platformUserIDLookups[StreamingPlatformTypeEnum.Twitch][userData.TwitchID] = userData.ID;
                    }
                    if (!string.IsNullOrEmpty(userData.TwitchUsername))
                    {
                        this.platformUsernameLookups[StreamingPlatformTypeEnum.Twitch][userData.TwitchUsername] = userData.ID;
                    }
                }
            }
        }

        public CommandModelBase GetCommand(Guid id) { return this.Commands.ContainsKey(id) ? this.Commands[id] : null; }

        public T GetCommand<T>(Guid id) where T : CommandModelBase { return (T)this.GetCommand(id); }

        public void SetCommand(CommandModelBase command) { if (command != null) { this.Commands[command.ID] = command; } }

        public void RemoveCommand(CommandModelBase command) { if (command != null) { this.Commands.Remove(command.ID); } }

        public void RemoveCommand(Guid id) { this.Commands.Remove(id); }

        private void InitializeMissingData()
        {
            foreach (StreamingPlatformTypeEnum platform in StreamingPlatforms.SupportedPlatforms)
            {
                if (!this.StreamingPlatformAuthentications.ContainsKey(platform))
                {
                    this.StreamingPlatformAuthentications[platform] = new StreamingPlatformAuthenticationSettingsModel(platform);
                }
            }

            if (this.DashboardItems.Count < 4)
            {
                this.DashboardItems = new List<DashboardItemTypeEnum>() { DashboardItemTypeEnum.None, DashboardItemTypeEnum.None, DashboardItemTypeEnum.None, DashboardItemTypeEnum.None };
            }
            if (this.DashboardQuickCommands.Count < 5)
            {
                this.DashboardQuickCommands = new List<Guid>() { Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty };
            }

            if (this.GetCommand(this.GameQueueUserJoinedCommandID) == null)
            {
                this.GameQueueUserJoinedCommandID = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameQueueUserJoinedCommandName, MixItUp.Base.Resources.GameQueueUserJoinedExample);
            }
            if (this.GetCommand(this.GameQueueUserSelectedCommandID) == null)
            {
                this.GameQueueUserSelectedCommandID = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameQueueUserSelectedCommandName, MixItUp.Base.Resources.GameQueueUserSelectedExample);
            }

            if (this.GetCommand(this.GiveawayStartedReminderCommandID) == null)
            {
                this.GiveawayStartedReminderCommandID = this.CreateBasicChatCommand(MixItUp.Base.Resources.GiveawayStartedReminderCommandName, MixItUp.Base.Resources.GiveawayStartedReminderExample);
            }
            if (this.GetCommand(this.GiveawayUserJoinedCommandID) == null)
            {
                this.GiveawayUserJoinedCommandID = this.CreateBasicCommand(MixItUp.Base.Resources.GiveawayUserJoinedCommandName);
            }
            if (this.GetCommand(this.GiveawayWinnerSelectedCommandID) == null)
            {
                this.GiveawayWinnerSelectedCommandID = this.CreateBasicChatCommand(MixItUp.Base.Resources.GiveawayWinnerSelectedCommandName, MixItUp.Base.Resources.GiveawayWinnerSelectedExample);
            }

            if (this.GetCommand(this.ModerationStrike1CommandID) == null)
            {
                this.ModerationStrike1CommandID = this.CreateBasicChatCommand(MixItUp.Base.Resources.ModerationStrike1CommandName, MixItUp.Base.Resources.ModerationStrikeExample);
            }
            if (this.GetCommand(this.ModerationStrike2CommandID) == null)
            {
                this.ModerationStrike2CommandID = this.CreateBasicChatCommand(MixItUp.Base.Resources.ModerationStrike2CommandName, MixItUp.Base.Resources.ModerationStrikeExample);
            }
            if (this.GetCommand(this.ModerationStrike3CommandID) == null)
            {
                this.ModerationStrike3CommandID = this.CreateBasicChatCommand(MixItUp.Base.Resources.ModerationStrike3CommandName, MixItUp.Base.Resources.ModerationStrikeExample);
            }

            if (this.GetCommand(this.RedemptionStoreManualRedeemNeededCommandID) == null)
            {
                this.RedemptionStoreManualRedeemNeededCommandID = this.CreateBasicChatCommand(MixItUp.Base.Resources.RedemptionStoreManualRedeemNeededCommandName, MixItUp.Base.Resources.RedemptionStoreManualRedeemNeededExample);
            }
            if (this.GetCommand(this.RedemptionStoreDefaultRedemptionCommandID) == null)
            {
                this.RedemptionStoreDefaultRedemptionCommandID = this.CreateBasicChatCommand(MixItUp.Base.Resources.RedemptionStoreDefaultRedemptionCommandName, MixItUp.Base.Resources.RedemptionStoreDefaultRedemptionExample);
            }
        }

        public Version GetLatestVersion() { return Assembly.GetEntryAssembly().GetName().Version; }

        private Guid CreateBasicCommand(string name)
        {
            CustomCommandModel command = new CustomCommandModel(name);
            this.SetCommand(command);
            return command.ID;
        }

        private Guid CreateBasicChatCommand(string name, string chatText)
        {
            CustomCommandModel command = new CustomCommandModel(name);
            command.Actions.Add(new ChatActionModel(chatText));
            this.SetCommand(command);
            return command.ID;
        }
    }
}