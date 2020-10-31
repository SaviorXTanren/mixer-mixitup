using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
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
        public const int LatestVersion = 1;

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
        public bool IsStreamer { get; set; }

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
        public Dictionary<StreamingPlatformTypeEnum, PlatformAuthenticationSettingsModel> PlatformAuthentications { get; set; } = new Dictionary<StreamingPlatformTypeEnum, PlatformAuthenticationSettingsModel>();

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
        public OAuthTokenModel StreamJarOAuthToken { get; set; }
        [DataMember]
        public OAuthTokenModel PatreonOAuthToken { get; set; }
        [DataMember]
        public OAuthTokenModel IFTTTOAuthToken { get; set; }
        [DataMember]
        public OAuthTokenModel StreamlootsOAuthToken { get; set; }
        [DataMember]
        public OAuthTokenModel JustGivingOAuthToken { get; set; }

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
        [DataMember]
        public bool SaveChatEventLogs { get; set; }

        #endregion General

        #region Chat

        [DataMember]
        public int MaxMessagesInChat { get; set; } = 100;
        [DataMember]
        public int MaxUsersShownInChat { get; set; } = 100;

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
        public bool UnlockAllCommands { get; set; }

        [DataMember]
        public int TwitchMassGiftedSubsFilterAmount { get; set; } = 1;

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
        public string AlertChannelPointsColor { get; set; }
        [DataMember]
        public string AlertModerationColor { get; set; }

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
        public List<UserTitleModel> UserTitles { get; set; } = new List<UserTitleModel>();

        #endregion Users

        #region Game Queue

        [DataMember]
        public bool GameQueueSubPriority { get; set; }
        [DataMember]
        public RequirementViewModel GameQueueRequirements { get; set; } = new RequirementViewModel();
        [Obsolete]
        [DataMember]
        public Base.Commands.CustomCommand GameQueueUserJoinedCommand { get; set; }
        [Obsolete]
        [DataMember]
        public Base.Commands.CustomCommand GameQueueUserSelectedCommand { get; set; }

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
        public RequirementViewModel GiveawayRequirements { get; set; } = new RequirementViewModel();
        [DataMember]
        public int GiveawayReminderInterval { get; set; } = 5;
        [DataMember]
        public bool GiveawayRequireClaim { get; set; } = true;
        [DataMember]
        public bool GiveawayAllowPastWinners { get; set; }
        [Obsolete]
        [DataMember]
        public Base.Commands.CustomCommand GiveawayStartedReminderCommand { get; set; }
        [Obsolete]
        [DataMember]
        public Base.Commands.CustomCommand GiveawayUserJoinedCommand { get; set; }
        [Obsolete]
        [DataMember]
        public Base.Commands.CustomCommand GiveawayWinnerSelectedCommand { get; set; }

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
        public bool ModerationResetStrikesOnLaunch { get; set; }
        [Obsolete]
        [DataMember]
        public Base.Commands.CustomCommand ModerationStrike1Command { get; set; }
        [Obsolete]
        [DataMember]
        public Base.Commands.CustomCommand ModerationStrike2Command { get; set; }
        [Obsolete]
        [DataMember]
        public Base.Commands.CustomCommand ModerationStrike3Command { get; set; }

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

        #region Remote

        [DataMember]
        public RemoteConnectionAuthenticationTokenModel RemoteHostConnection { get; set; }
        [DataMember]
        public List<RemoteConnectionModel> RemoteClientConnections { get; set; } = new List<RemoteConnectionModel>();

        [DataMember]
        public List<RemoteProfileModel> RemoteProfiles { get; set; } = new List<RemoteProfileModel>();
        [DataMember]
        public Dictionary<Guid, RemoteProfileBoardsModel> RemoteProfileBoards { get; set; } = new Dictionary<Guid, RemoteProfileBoardsModel>();

        #endregion Remote

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
        public string PatreonTierMixerSubscriberEquivalent { get; set; }

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
        public DatabaseList<UserQuoteViewModel> Quotes { get; set; } = new DatabaseList<UserQuoteViewModel>();

        [JsonIgnore]
        public DatabaseDictionary<Guid, UserDataModel> UserData { get; set; } = new DatabaseDictionary<Guid, UserDataModel>();
        [JsonIgnore]
        private Dictionary<string, Guid> TwitchUserIDLookups { get; set; } = new Dictionary<string, Guid>();
        [JsonIgnore]
        private Dictionary<StreamingPlatformTypeEnum, Dictionary<string, Guid>> UsernameLookups { get; set; } = new Dictionary<StreamingPlatformTypeEnum, Dictionary<string, Guid>>();

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

        public SettingsV3Model() { }

        public SettingsV3Model(string name, bool isStreamer = true)
            : this()
        {
            this.Name = name;
            this.IsStreamer = isStreamer;

            this.InitializeMissingData();
        }

        public async Task Initialize()
        {
            if (this.IsStreamer)
            {
                if (!ChannelSession.Services.FileService.FileExists(this.DatabaseFilePath))
                {
                    await ChannelSession.Services.FileService.CopyFile(SettingsV3Model.SettingsTemplateDatabaseFileName, this.DatabaseFilePath);
                }

                foreach (StreamingPlatformTypeEnum platform in StreamingPlatforms.Platforms)
                {
                    this.UsernameLookups[platform] = new Dictionary<string, Guid>();
                }

                await ChannelSession.Services.Database.Read(this.DatabaseFilePath, "SELECT * FROM Users", (Dictionary<string, object> data) =>
                {
                    UserDataModel userData = JSONSerializerHelper.DeserializeFromString<UserDataModel>((string)data["Data"]);
                    this.UserData[userData.ID] = userData;
                    if (userData.Platform.HasFlag(StreamingPlatformTypeEnum.Twitch))
                    {
                        this.TwitchUserIDLookups[userData.TwitchID] = userData.ID;
                        if (!string.IsNullOrEmpty(userData.TwitchUsername))
                        {
                            this.UsernameLookups[StreamingPlatformTypeEnum.Twitch][userData.TwitchUsername.ToLowerInvariant()] = userData.ID;
                        }
                    }
#pragma warning disable CS0612 // Type or member is obsolete
                    else if (userData.Platform.HasFlag(StreamingPlatformTypeEnum.Mixer))
                    {
                        if (!string.IsNullOrEmpty(userData.MixerUsername))
                        {
                            this.UsernameLookups[StreamingPlatformTypeEnum.Mixer][userData.MixerUsername.ToLowerInvariant()] = userData.ID;
                        }
                    }
#pragma warning restore CS0612 // Type or member is obsolete
                });
                this.UserData.ClearTracking();

                await ChannelSession.Services.Database.Read(this.DatabaseFilePath, "SELECT * FROM Quotes", (Dictionary<string, object> data) =>
                {
                    string json = (string)data["Data"];
                    if (json.Contains("MixItUp.Base.ViewModel.User.UserQuoteViewModel"))
                    {
                        json = json.Replace("MixItUp.Base.ViewModel.User.UserQuoteViewModel", "MixItUp.Base.Model.User.UserQuoteModel");
                        this.Quotes.Add(new UserQuoteViewModel(JSONSerializerHelper.DeserializeFromString<UserQuoteModel>(json)));
                    }
                    else
                    {
                        this.Quotes.Add(new UserQuoteViewModel(JSONSerializerHelper.DeserializeFromString<UserQuoteModel>((string)data["Data"])));
                    }
                });
                this.Quotes.ClearTracking();

                await ChannelSession.Services.Database.Read(this.DatabaseFilePath, "SELECT * FROM Commands", (Dictionary<string, object> data) =>
                {
                    CommandModelBase command = null;
                    CommandTypeEnum type = (CommandTypeEnum)Convert.ToInt32(data["TypeID"]);
                    if (type == CommandTypeEnum.Chat)
                    {
                        command = JSONSerializerHelper.DeserializeFromString<ChatCommandModel>((string)data["Data"]);
                    }
                    else if (type == CommandTypeEnum.Event)
                    {
                        command = JSONSerializerHelper.DeserializeFromString<EventCommandModel>((string)data["Data"]);
                    }
                    else if (type == CommandTypeEnum.Timer)
                    {
                        command = JSONSerializerHelper.DeserializeFromString<TimerCommandModel>((string)data["Data"]);
                    }
                    else if (type == CommandTypeEnum.ActionGroup)
                    {
                        command = JSONSerializerHelper.DeserializeFromString<ActionGroupCommandModel>((string)data["Data"]);
                    }
                    else if (type == CommandTypeEnum.Game)
                    {
                        command = JSONSerializerHelper.DeserializeFromString<GameCommandModelBase>((string)data["Data"]);
                    }
                    else if (type == CommandTypeEnum.TwitchChannelPoints)
                    {
                        command = JSONSerializerHelper.DeserializeFromString<TwitchChannelPointsCommandModel>((string)data["Data"]);
                    }
                    else if (type == CommandTypeEnum.Custom)
                    {
                        command = JSONSerializerHelper.DeserializeFromString<CustomCommandModel>((string)data["Data"]);
                    }
                    
                    if (command != null)
                    {
                        this.Commands[command.ID] = command;
                    }
                });

                ChannelSession.ChatCommands.Clear();
                ChannelSession.EventCommands.Clear();
                ChannelSession.TimerCommands.Clear();
                ChannelSession.ActionGroupCommands.Clear();
                ChannelSession.GameCommands.Clear();
                ChannelSession.TwitchChannelPointsCommands.Clear();
                foreach (CommandModelBase command in this.Commands.Values.ToList())
                {
                    if (command is ChatCommandModel) { ChannelSession.ChatCommands.Add((ChatCommandModel)command); }
                    else if (command is EventCommandModel) { ChannelSession.EventCommands.Add((EventCommandModel)command); }
                    else if (command is TimerCommandModel) { ChannelSession.TimerCommands.Add((TimerCommandModel)command); }
                    else if (command is ActionGroupCommandModel) { ChannelSession.ActionGroupCommands.Add((ActionGroupCommandModel)command); }
                    else if (command is GameCommandModelBase) { ChannelSession.GameCommands.Add((GameCommandModelBase)command); }
                    else if (command is TwitchChannelPointsCommandModel) { ChannelSession.TwitchChannelPointsCommands.Add((TwitchChannelPointsCommandModel)command); }
                }

                foreach (CounterModel counter in this.Counters.Values.ToList())
                {
                    if (counter.ResetOnLoad)
                    {
                        await counter.ResetAmount();
                    }
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

            // Mod accounts cannot use this feature, forcefully disable on load
            if (!this.IsStreamer)
            {
                this.TrackWhispererNumber = false;
            }

            this.InitializeMissingData();
        }

        public void ClearMixerUserData()
        {
#pragma warning disable CS0612 // Type or member is obsolete
            foreach (Guid id in this.UsernameLookups[StreamingPlatformTypeEnum.Mixer].Values.ToList())
            {
                this.UserData.Remove(id);
            }

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
                this.PlatformAuthentications[StreamingPlatformTypeEnum.Twitch].UserOAuthToken = ChannelSession.TwitchUserConnection.Connection.GetOAuthTokenCopy();
            }
            if (ChannelSession.TwitchBotConnection != null)
            {
                this.PlatformAuthentications[StreamingPlatformTypeEnum.Twitch].BotOAuthToken = ChannelSession.TwitchBotConnection.Connection.GetOAuthTokenCopy();
            }

            if (ChannelSession.Services.Streamlabs.IsConnected)
            {
                this.StreamlabsOAuthToken = ChannelSession.Services.Streamlabs.GetOAuthTokenCopy();
            }
            if (ChannelSession.Services.StreamElements.IsConnected)
            {
                this.StreamElementsOAuthToken = ChannelSession.Services.StreamElements.GetOAuthTokenCopy();
            }
            if (ChannelSession.Services.StreamJar.IsConnected)
            {
                this.StreamJarOAuthToken = ChannelSession.Services.StreamJar.GetOAuthTokenCopy();
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

            // TODO
            // Clear out unused Cooldown Groups and Command Groups
            //var allUsedCooldownGroupNames =
            //    this.ChatCommands.Select(c => c.Requirements?.Cooldown?.GroupName)
            //    .Union(this.GameCommands.Select(c => c.Requirements?.Cooldown?.GroupName))
            //    .Distinct();
            //var allUnusedCooldownGroupNames = this.CooldownGroupAmounts.ToList().Where(c => !allUsedCooldownGroupNames.Contains(c.Key, StringComparer.InvariantCultureIgnoreCase));
            //foreach (var unused in allUnusedCooldownGroupNames)
            //{
            //    this.CooldownGroupAmounts.Remove(unused.Key);
            //}

            //var allUsedCommandGroupNames =
            //    this.ChatCommands.Select(c => c.GroupName)
            //    .Union(this.ActionGroupCommands.Select(a => a.GroupName))
            //    .Union(this.TimerCommands.Select(a => a.GroupName))
            //    .Distinct();
            //var allUnusedCommandGroupNames = this.CommandGroups.ToList().Where(c => !allUsedCommandGroupNames.Contains(c.Key, StringComparer.InvariantCultureIgnoreCase));
            //foreach (var unused in allUnusedCommandGroupNames)
            //{
            //    this.CommandGroups.Remove(unused.Key);
            //}
        }

        public async Task SaveDatabaseData()
        {
            if (this.IsStreamer)
            {
                IEnumerable<Guid> removedUsers = this.UserData.GetRemovedValues();
                await ChannelSession.Services.Database.BulkWrite(this.DatabaseFilePath, "DELETE FROM Users WHERE ID = @ID", removedUsers.Select(u => new Dictionary<string, object>() { { "@ID", u.ToString() } }));

                IEnumerable<UserDataModel> changedUsers = this.UserData.GetChangedValues();
                await ChannelSession.Services.Database.BulkWrite(this.DatabaseFilePath, "REPLACE INTO Users(ID, Data) VALUES(@ID, @Data)",
                    changedUsers.Select(u => new Dictionary<string, object>() { { "@ID", u.ID.ToString() }, { "@Data", JSONSerializerHelper.SerializeToString(u) } }));

                List<Guid> removedCommands = new List<Guid>();
                await ChannelSession.Services.Database.BulkWrite(this.DatabaseFilePath, "DELETE FROM Commands WHERE ID = @ID",
                    this.Commands.GetRemovedValues().Select(id => new Dictionary<string, object>() { { "@ID", id.ToString() } }));

                await ChannelSession.Services.Database.BulkWrite(this.DatabaseFilePath, "REPLACE INTO Commands(ID, TypeID, Data) VALUES(@ID, @TypeID, @Data)",
                    this.Commands.GetAddedChangedValues().Select(c => new Dictionary<string, object>() { { "@ID", c.ID.ToString() }, { "@TypeID", (int)c.Type }, { "@Data", JSONSerializerHelper.SerializeToString(c) } }));

                await ChannelSession.Services.Database.BulkWrite(this.DatabaseFilePath, "DELETE FROM Quotes WHERE ID = @ID",
                    this.Quotes.GetRemovedValues().Select(q => new Dictionary<string, object>() { { "@ID", q.ID.ToString() } }));

                await ChannelSession.Services.Database.BulkWrite(this.DatabaseFilePath, "REPLACE INTO Quotes(ID, Data) VALUES(@ID, @Data)",
                    this.Quotes.GetAddedChangedValues().Select(q => new Dictionary<string, object>() { { "@ID", q.ID.ToString() }, { "@Data", JSONSerializerHelper.SerializeToString(q.Model) } }));
            }
        }

        public UserDataModel GetUserData(Guid id)
        {
            lock (this.UserData)
            {
                if (this.UserData.ContainsKey(id))
                {
                    return this.UserData[id];
                }
                return null;
            }
        }

        public UserDataModel GetUserDataByTwitchID(string twitchID)
        {
            lock (this.UserData)
            {
                if (!string.IsNullOrEmpty(twitchID) && this.TwitchUserIDLookups.ContainsKey(twitchID))
                {
                    Guid id = this.TwitchUserIDLookups[twitchID];
                    if (this.UserData.ContainsKey(id))
                    {
                        return this.UserData[id];
                    }
                }
                return null;
            }
        }

        public UserDataModel GetUserDataByUsername(StreamingPlatformTypeEnum platform, string username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                if (platform == StreamingPlatformTypeEnum.All)
                {
                    foreach (StreamingPlatformTypeEnum p in StreamingPlatforms.Platforms)
                    {
                        UserDataModel userData = this.GetUserDataByUsername(p, username);
                        if (userData != null)
                        {
                            return userData;
                        }
                    }
                }
                else
                {
                    lock (this.UserData)
                    {
                        if (this.UsernameLookups.ContainsKey(platform) && this.UsernameLookups[platform].ContainsKey(username.ToLowerInvariant()))
                        {
                            Guid id = this.UsernameLookups[platform][username.ToLowerInvariant()];
                            if (this.UserData.ContainsKey(id))
                            {
                                return this.UserData[id];
                            }
                        }
                    }
                }
            }
            return null;
        }

        public void AddUserData(UserDataModel user)
        {
            this.UserData[user.ID] = user;
            if (!string.IsNullOrEmpty(user.TwitchID))
            {
                this.TwitchUserIDLookups[user.TwitchID] = user.ID;
            }
            this.UserData.ManualValueChanged(user.ID);
        }

        public CommandModelBase GetCommand(Guid id) { return this.Commands.ContainsKey(id) ? this.Commands[id] : null; }

        public void SetCommand(CommandModelBase command) { if (command != null) { this.Commands[command.ID] = command; } }

        public void RemoveCommand(CommandModelBase command) { if (command != null) { this.Commands.Remove(command.ID); } }

        public void RemoveCommand(Guid id) { this.Commands.Remove(id); }

        private void InitializeMissingData()
        {
            foreach (StreamingPlatformTypeEnum platform in EnumHelper.GetEnumList<StreamingPlatformTypeEnum>())
            {
                if (!this.PlatformAuthentications.ContainsKey(platform))
                {
                    this.PlatformAuthentications[platform] = new PlatformAuthenticationSettingsModel(platform);
                }
            }

            // TODO
            //this.GameQueueUserJoinedCommand = this.GameQueueUserJoinedCommand ?? CustomCommandModel.BasicChatCommand(MixItUp.Base.Resources.GameQueueUserJoinedCommandName, "You are #$queueposition in the queue to play.");
            //this.GameQueueUserSelectedCommand = this.GameQueueUserSelectedCommand ?? CustomCommandModel.BasicChatCommand(MixItUp.Base.Resources.GameQueueUserSelectedCommandName, "It's time to play @$username! Listen carefully for instructions on how to join...");

            //this.GiveawayStartedReminderCommand = this.GiveawayStartedReminderCommand ?? CustomCommandModel.BasicChatCommand("Giveaway Started/Reminder", "A giveaway has started for $giveawayitem! Type $giveawaycommand in chat in the next $giveawaytimelimit minute(s) to enter!");
            //this.GiveawayUserJoinedCommand = this.GiveawayUserJoinedCommand ?? CustomCommandModel.BasicChatCommand("Giveaway User Joined");
            //this.GiveawayWinnerSelectedCommand = this.GiveawayWinnerSelectedCommand ?? CustomCommandModel.BasicChatCommand("Giveaway Winner Selected", "Congratulations @$username, you won $giveawayitem!");

            //this.ModerationStrike1Command = this.ModerationStrike1Command ?? CustomCommandModel.BasicChatCommand("Moderation Strike 1", "$moderationreason. You have received a moderation strike & currently have $usermoderationstrikes strike(s)", isWhisper: true);
            //this.ModerationStrike2Command = this.ModerationStrike2Command ?? CustomCommandModel.BasicChatCommand("Moderation Strike 2", "$moderationreason. You have received a moderation strike & currently have $usermoderationstrikes strike(s)", isWhisper: true);
            //this.ModerationStrike3Command = this.ModerationStrike3Command ?? CustomCommandModel.BasicChatCommand("Moderation Strike 3", "$moderationreason. You have received a moderation strike & currently have $usermoderationstrikes strike(s)", isWhisper: true);

            if (this.DashboardItems.Count < 4)
            {
                this.DashboardItems = new List<DashboardItemTypeEnum>() { DashboardItemTypeEnum.None, DashboardItemTypeEnum.None, DashboardItemTypeEnum.None, DashboardItemTypeEnum.None };
            }
            if (this.DashboardQuickCommands.Count < 5)
            {
                this.DashboardQuickCommands = new List<Guid>() { Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty };
            }

            // TODO
            //if (this.GetCustomCommand(this.RedemptionStoreManualRedeemNeededCommandID) == null)
            //{
            //    CustomCommand command = CustomCommand.BasicChatCommand(RedemptionStorePurchaseModel.ManualRedemptionNeededCommandName, "@$username just purchased $productname and needs to be manually redeemed");
            //    this.RedemptionStoreManualRedeemNeededCommandID = command.ID;
            //    this.SetCustomCommand(command);
            //}
            //if (this.GetCustomCommand(this.RedemptionStoreDefaultRedemptionCommandID) == null)
            //{
            //    CustomCommand command = CustomCommand.BasicChatCommand(RedemptionStorePurchaseModel.DefaultRedemptionCommandName, "@$username just redeemed $productname");
            //    this.RedemptionStoreDefaultRedemptionCommandID = command.ID;
            //    this.SetCustomCommand(command);
            //}
        }

        public Version GetLatestVersion() { return Assembly.GetEntryAssembly().GetName().Version; }
    }
}