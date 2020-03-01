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
    public class SettingsV2Model
    {
        public const int LatestVersion = 40;

        public const string SettingsDirectoryName = "Settings";

        public const string SettingsTemplateDatabaseFileName = "SettingsTemplateDatabase.db";

        public const string SettingsFileExtension = "miu";
        public const string DatabaseFileExtension = "db";
        public const string SettingsLocalBackupFileExtension = "backup";

        public const string SettingsBackupFileExtension = "miubackup";

        [DataMember]
        public int Version { get; set; } = SettingsV2Model.LatestVersion;

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
        public OAuthTokenModel MixerUserOAuthToken { get; set; }
        [DataMember]
        public OAuthTokenModel MixerBotOAuthToken { get; set; }
        [DataMember]
        public uint MixerChannelID { get; set; }

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
        public bool ChatShowUserJoinLeave { get; set; }
        [DataMember]
        public string ChatUserJoinLeaveColorScheme { get; set; } = ColorSchemes.DefaultColorScheme;
        [DataMember]
        public bool ChatShowEventAlerts { get; set; }
        [DataMember]
        public string ChatEventAlertsColorScheme { get; set; } = ColorSchemes.DefaultColorScheme;
        [DataMember]
        public bool ChatShowMixPlayAlerts { get; set; }
        [DataMember]
        public string ChatMixPlayAlertsColorScheme { get; set; } = ColorSchemes.DefaultColorScheme;

        [DataMember]
        public bool WhisperAllAlerts { get; set; }
        [DataMember]
        public bool OnlyShowAlertsInDashboard { get; set; }
        [DataMember]
        public bool LatestChatAtTop { get; set; }
        [DataMember]
        public bool HideViewerAndChatterNumbers { get; set; }
        [DataMember]
        public bool HideChatUserList { get; set; }
        [DataMember]
        public bool HideDeletedMessages { get; set; }
        [DataMember]
        public bool TrackWhispererNumber { get; set; }
        [DataMember]
        public bool AllowCommandWhispering { get; set; }
        [DataMember]
        public bool IgnoreBotAccountCommands { get; set; }
        [DataMember]
        public bool CommandsOnlyInYourStream { get; set; }
        [DataMember]
        public bool DeleteChatCommandsWhenRun { get; set; }
        [DataMember]
        public bool ShowMixrElixrEmotes { get; set; }
        [DataMember]
        public bool ShowChatMessageTimestamps { get; set; }

        #endregion Chat

        #region Notifications

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

        #region MixPlay

        [DataMember]
        public uint DefaultMixPlayGame { get; set; }
        [DataMember]
        public bool PreventUnknownMixPlayUsers { get; set; }
        [DataMember]
        public bool PreventSmallerMixPlayCooldowns { get; set; }
        [DataMember]
        public List<MixPlaySharedProjectModel> CustomMixPlayProjectIDs { get; set; } = new List<MixPlaySharedProjectModel>();

        [DataMember]
        public Dictionary<uint, List<MixPlayUserGroupModel>> MixPlayUserGroups { get; set; } = new Dictionary<uint, List<MixPlayUserGroupModel>>();

        [DataMember]
        public Dictionary<uint, JObject> CustomMixPlaySettings { get; set; } = new Dictionary<uint, JObject>();

        #endregion MixPlay

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
        [DataMember]
        public CustomCommand GameQueueUserJoinedCommand { get; set; }
        [DataMember]
        public CustomCommand GameQueueUserSelectedCommand { get; set; }

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
        [DataMember]
        public CustomCommand GiveawayStartedReminderCommand { get; set; }
        [DataMember]
        public CustomCommand GiveawayUserJoinedCommand { get; set; }
        [DataMember]
        public CustomCommand GiveawayWinnerSelectedCommand { get; set; }

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
        [DataMember]
        public CustomCommand ModerationStrike1Command { get; set; }
        [DataMember]
        public CustomCommand ModerationStrike2Command { get; set; }
        [DataMember]
        public CustomCommand ModerationStrike3Command { get; set; }

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
        [DataMember]
        public bool DiagnosticLogging { get; set; }
        [DataMember]
        public bool UnlockAllCommands { get; set; }

        #endregion Advanced

        [DataMember]
        public Dictionary<Guid, UserCurrencyModel> Currencies { get; set; } = new Dictionary<Guid, UserCurrencyModel>();
        [DataMember]
        public Dictionary<Guid, UserInventoryModel> Inventories { get; set; } = new Dictionary<Guid, UserInventoryModel>();

        [DataMember]
        public Dictionary<string, int> CooldownGroups { get; set; } = new Dictionary<string, int>();

        [DataMember]
        public List<PreMadeChatCommandSettings> PreMadeChatCommandSettings { get; set; } = new List<PreMadeChatCommandSettings>();

        [DataMember]
        public List<string> RecentStreamTitles { get; set; } = new List<string>();

        [DataMember]
        public Dictionary<string, object> LatestSpecialIdentifiersData { get; set; } = new Dictionary<string, object>();

        [DataMember]
        public List<FavoriteGroupModel> FavoriteGroups { get; set; } = new List<FavoriteGroupModel>();

        [DataMember]
        public Dictionary<string, CommandGroupSettings> CommandGroups { get; set; } = new Dictionary<string, CommandGroupSettings>();
        [DataMember]
        public Dictionary<string, HotKeyConfiguration> HotKeys { get; set; } = new Dictionary<string, HotKeyConfiguration>();
        [DataMember]
        public Dictionary<string, CounterModel> Counters { get; set; } = new Dictionary<string, CounterModel>();

        #region Database Data

        [JsonIgnore]
        public DatabaseList<ChatCommand> ChatCommands { get; set; } = new DatabaseList<ChatCommand>();
        [JsonIgnore]
        public DatabaseList<EventCommand> EventCommands { get; set; } = new DatabaseList<EventCommand>();
        [JsonIgnore]
        public DatabaseList<MixPlayCommand> MixPlayCommands { get; set; } = new DatabaseList<MixPlayCommand>();
        [JsonIgnore]
        public DatabaseList<TimerCommand> TimerCommands { get; set; } = new DatabaseList<TimerCommand>();
        [JsonIgnore]
        public DatabaseList<ActionGroupCommand> ActionGroupCommands { get; set; } = new DatabaseList<ActionGroupCommand>();
        [JsonIgnore]
        public DatabaseList<GameCommandBase> GameCommands { get; set; } = new DatabaseList<GameCommandBase>();

        [JsonIgnore]
        public DatabaseList<UserQuoteViewModel> Quotes { get; set; } = new DatabaseList<UserQuoteViewModel>();

        [JsonIgnore]
        public DatabaseDictionary<Guid, UserDataModel> UserData { get; set; } = new DatabaseDictionary<Guid, UserDataModel>();
        [JsonIgnore]
        private Dictionary<uint, Guid> MixerUserIDLookups { get; set; } = new Dictionary<uint, Guid>();

        #endregion Database Data

        [JsonIgnore]
        public string SettingsFileName { get { return string.Format("{0}.{1}", this.ID, SettingsV2Model.SettingsFileExtension); } }
        [JsonIgnore]
        public string SettingsFilePath { get { return Path.Combine(SettingsV2Model.SettingsDirectoryName, this.SettingsFileName); } }

        [JsonIgnore]
        public string DatabaseFileName { get { return string.Format("{0}.{1}", this.ID, SettingsV2Model.DatabaseFileExtension); } }
        [JsonIgnore]
        public string DatabaseFilePath { get { return Path.Combine(SettingsV2Model.SettingsDirectoryName, this.DatabaseFileName); } }

        [JsonIgnore]
        public string SettingsLocalBackupFileName { get { return string.Format("{0}.{1}.{2}", this.ID, SettingsV2Model.SettingsFileExtension, SettingsV2Model.SettingsLocalBackupFileExtension); } }
        [JsonIgnore]
        public string SettingsLocalBackupFilePath { get { return Path.Combine(SettingsV2Model.SettingsDirectoryName, this.SettingsLocalBackupFileName); } }

        public SettingsV2Model() { }

        public SettingsV2Model(ExpandedChannelModel channel, bool isStreamer = true)
            : this()
        {
            this.Name = channel.token;
            this.MixerChannelID = channel.id;
            this.IsStreamer = isStreamer;

            if (ChannelSession.IsDebug())
            {
                this.DiagnosticLogging = true;
            }

            this.InitializeMissingData();
        }

        public async Task Initialize()
        {
            if (this.IsStreamer)
            {
                if (!ChannelSession.Services.FileService.FileExists(this.DatabaseFilePath))
                {
                    await ChannelSession.Services.FileService.CopyFile(SettingsV2Model.SettingsTemplateDatabaseFileName, this.DatabaseFilePath);
                }

                await ChannelSession.Services.Database.Read(this.DatabaseFilePath, "SELECT * FROM Users", (Dictionary<string, object> data) =>
                {
                    UserDataModel userData = JSONSerializerHelper.DeserializeFromString<UserDataModel>((string)data["Data"]);
                    this.UserData[userData.ID] = userData;
                    this.MixerUserIDLookups[userData.MixerID] = userData.ID;
                });
                this.UserData.ClearTracking();

                await ChannelSession.Services.Database.Read(this.DatabaseFilePath, "SELECT * FROM CurrencyAmounts", (Dictionary<string, object> data) =>
                {
                    Guid currencyID = Guid.Parse((string)data["CurrencyID"]);
                    Guid userID = Guid.Parse((string)data["UserID"]);
                    int amount = Convert.ToInt32(data["Amount"]);

                    if (this.Currencies.ContainsKey(currencyID))
                    {
                        this.Currencies[currencyID].UserAmounts[userID] = amount;
                    }
                });
                foreach (var kvp in this.Currencies)
                {
                    kvp.Value.UserAmounts.ClearTracking();
                }

                await ChannelSession.Services.Database.Read(this.DatabaseFilePath, "SELECT * FROM InventoryAmounts", (Dictionary<string, object> data) =>
                {
                    Guid inventoryID = Guid.Parse((string)data["InventoryID"]);
                    Guid userID = Guid.Parse((string)data["UserID"]);
                    Guid itemID = Guid.Parse((string)data["ItemID"]);
                    int amount = Convert.ToInt32(data["Amount"]);

                    if (this.Inventories.ContainsKey(inventoryID))
                    {
                        if (!this.Inventories[inventoryID].UserAmounts.ContainsKey(userID))
                        {
                            this.Inventories[inventoryID].UserAmounts[userID] = new Dictionary<Guid, int>();
                        }
                        this.Inventories[inventoryID].UserAmounts[userID][itemID] = amount;
                    }
                });
                foreach (var kvp in this.Inventories)
                {
                    kvp.Value.UserAmounts.ClearTracking();
                }

                await ChannelSession.Services.Database.Read(this.DatabaseFilePath, "SELECT * FROM Quotes", (Dictionary<string, object> data) =>
                {
                    this.Quotes.Add(JSONSerializerHelper.DeserializeFromString<UserQuoteViewModel>((string)data["Data"]));
                });

                await ChannelSession.Services.Database.Read(this.DatabaseFilePath, "SELECT * FROM Commands", (Dictionary<string, object> data) =>
                {
                    CommandTypeEnum type = (CommandTypeEnum)Convert.ToInt32(data["TypeID"]);
                    if (type == CommandTypeEnum.Chat)
                    {
                        this.ChatCommands.Add(JSONSerializerHelper.DeserializeFromString<ChatCommand>((string)data["Data"]));
                    }
                    else if (type == CommandTypeEnum.Event)
                    {
                        this.EventCommands.Add(JSONSerializerHelper.DeserializeFromString<EventCommand>((string)data["Data"]));
                    }
                    else if (type == CommandTypeEnum.Interactive)
                    {
                        this.MixPlayCommands.Add(JSONSerializerHelper.DeserializeFromString<MixPlayCommand>((string)data["Data"]));
                    }
                    else if (type == CommandTypeEnum.Timer)
                    {
                        this.TimerCommands.Add(JSONSerializerHelper.DeserializeFromString<TimerCommand>((string)data["Data"]));
                    }
                    else if (type == CommandTypeEnum.ActionGroup)
                    {
                        this.ActionGroupCommands.Add(JSONSerializerHelper.DeserializeFromString<ActionGroupCommand>((string)data["Data"]));
                    }
                    else if (type == CommandTypeEnum.Game)
                    {
                        this.GameCommands.Add(JSONSerializerHelper.DeserializeFromString<GameCommandBase>((string)data["Data"]));
                    }
                });

                this.ChatCommands.ClearTracking();
                this.EventCommands.ClearTracking();
                this.MixPlayCommands.ClearTracking();
                this.TimerCommands.ClearTracking();
                this.ActionGroupCommands.ClearTracking();
                this.GameCommands.ClearTracking();
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

        public async Task ClearAllUserData()
        {
            this.UserData.Clear();
            await ChannelSession.Services.Database.Write(this.DatabaseFilePath, "DELETE FROM Users");
        }

        public void CopyLatestValues()
        {
            Logger.Log(LogLevel.Debug, "Copying over latest values into Settings object");

            this.Version = SettingsV2Model.LatestVersion;

            if (ChannelSession.MixerUserConnection != null)
            {
                this.MixerUserOAuthToken = ChannelSession.MixerUserConnection.Connection.GetOAuthTokenCopy();
            }
            if (ChannelSession.MixerBotConnection != null)
            {
                this.MixerBotOAuthToken = ChannelSession.MixerBotConnection.Connection.GetOAuthTokenCopy();
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

            // Clear out unused Cooldown Groups and Command Groups
            var allUsedCooldownGroupNames =
                this.MixPlayCommands.Select(c => c.Requirements?.Cooldown?.GroupName)
                .Union(this.ChatCommands.Select(c => c.Requirements?.Cooldown?.GroupName))
                .Union(this.GameCommands.Select(c => c.Requirements?.Cooldown?.GroupName))
                .Distinct();
            var allUnusedCooldownGroupNames = this.CooldownGroups.ToList().Where(c => !allUsedCooldownGroupNames.Contains(c.Key, StringComparer.InvariantCultureIgnoreCase));
            foreach (var unused in allUnusedCooldownGroupNames)
            {
                this.CooldownGroups.Remove(unused.Key);
            }

            var allUsedCommandGroupNames =
                this.ChatCommands.Select(c => c.GroupName)
                .Union(this.ActionGroupCommands.Select(a => a.GroupName))
                .Union(this.TimerCommands.Select(a => a.GroupName))
                .Distinct();
            var allUnusedCommandGroupNames = this.CommandGroups.ToList().Where(c => !allUsedCommandGroupNames.Contains(c.Key, StringComparer.InvariantCultureIgnoreCase));
            foreach (var unused in allUnusedCommandGroupNames)
            {
                this.CommandGroups.Remove(unused.Key);
            }
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

                foreach (var kvp in this.Currencies)
                {
                    IEnumerable<Guid> changedKeys = kvp.Value.UserAmounts.GetChangedKeys();
                    await ChannelSession.Services.Database.BulkWrite(this.DatabaseFilePath, "REPLACE INTO CurrencyAmounts(CurrencyID, UserID, Amount) VALUES(@CurrencyID, @UserID, @Amount)",
                        changedKeys.Select(u => new Dictionary<string, object>() { { "@CurrencyID", kvp.Value.ID.ToString() }, { "@UserID", u.ToString() }, { "@Amount", kvp.Value.GetAmount(u) } }));
                }

                foreach (var kvp in this.Inventories)
                {
                    List<Dictionary<string, object>> changedData = new List<Dictionary<string, object>>();

                    IEnumerable<Guid> changedKeys = kvp.Value.UserAmounts.GetChangedKeys();
                    foreach (Guid changedKey in changedKeys)
                    {
                        foreach (var item in kvp.Value.GetAmounts(changedKey))
                        {
                            changedData.Add(new Dictionary<string, object>()
                            {
                                { "@InventoryID", kvp.Value.ID.ToString() },
                                { "@UserID", changedKey.ToString() },
                                { "@ItemID", item.Key.ToString() },
                                { "@Amount", item.Value }
                            });
                        }
                    }

                    await ChannelSession.Services.Database.BulkWrite(this.DatabaseFilePath, "REPLACE INTO InventoryAmounts(InventoryID, UserID, ItemID, Amount) VALUES(@InventoryID, @UserID, @ItemID, @Amount)", changedData);
                }

                List<CommandBase> removedCommands = new List<CommandBase>();
                removedCommands.AddRange(this.ChatCommands.GetRemovedValues());
                removedCommands.AddRange(this.EventCommands.GetRemovedValues());
                removedCommands.AddRange(this.MixPlayCommands.GetRemovedValues());
                removedCommands.AddRange(this.TimerCommands.GetRemovedValues());
                removedCommands.AddRange(this.ActionGroupCommands.GetRemovedValues());
                removedCommands.AddRange(this.GameCommands.GetRemovedValues());
                await ChannelSession.Services.Database.BulkWrite(this.DatabaseFilePath, "DELETE FROM Commands WHERE ID = @ID",
                    removedCommands.Select(c => new Dictionary<string, object>() { { "@ID", c.ID.ToString() } }));

                List<CommandBase> addedChangedCommands = new List<CommandBase>();
                addedChangedCommands.AddRange(this.ChatCommands.GetAddedChangedValues());
                addedChangedCommands.AddRange(this.EventCommands.GetAddedChangedValues());
                addedChangedCommands.AddRange(this.MixPlayCommands.GetAddedChangedValues());
                addedChangedCommands.AddRange(this.TimerCommands.GetAddedChangedValues());
                addedChangedCommands.AddRange(this.ActionGroupCommands.GetAddedChangedValues());
                addedChangedCommands.AddRange(this.GameCommands.GetAddedChangedValues());
                await ChannelSession.Services.Database.BulkWrite(this.DatabaseFilePath, "REPLACE INTO Commands(ID, TypeID, Data) VALUES(@ID, @TypeID, @Data)",
                    addedChangedCommands.Select(c => new Dictionary<string, object>() { { "@ID", c.ID.ToString() }, { "@TypeID", (int)c.Type }, { "@Data", JSONSerializerHelper.SerializeToString(c) } }));

                await ChannelSession.Services.Database.BulkWrite(this.DatabaseFilePath, "DELETE FROM Quotes WHERE ID = @ID",
                    this.Quotes.GetRemovedValues().Select(q => new Dictionary<string, object>() { { "@ID", q.ID.ToString() } }));

                await ChannelSession.Services.Database.BulkWrite(this.DatabaseFilePath, "REPLACE INTO Quotes(ID, Data) VALUES(@ID, @Data)",
                    this.Quotes.GetAddedChangedValues().Select(q => new Dictionary<string, object>() { { "@ID", q.ID.ToString() }, { "@Data", JSONSerializerHelper.SerializeToString(q) } }));
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

        public UserDataModel GetUserDataByMixerID(uint mixerID)
        {
            lock (this.UserData)
            {
                if (mixerID > 0 && this.MixerUserIDLookups.ContainsKey(mixerID))
                {
                    Guid id = this.MixerUserIDLookups[mixerID];
                    if (this.UserData.ContainsKey(id))
                    {
                        return this.UserData[id];
                    }
                }
                return this.CreateUserData(mixerID);
            }
        }

        private UserDataModel CreateUserData(uint mixerID = 0)
        {
            UserDataModel userData = new UserDataModel();
            this.UserData[userData.ID] = userData;

            userData.MixerID = mixerID;
            if (userData.MixerID > 0)
            {
                this.MixerUserIDLookups[userData.MixerID] = userData.ID;
            }

            return userData;
        }

        private void InitializeMissingData()
        {
            this.GameQueueUserJoinedCommand = this.GameQueueUserJoinedCommand ?? CustomCommand.BasicChatCommand("Game Queue Used Joined", "You are #$queueposition in the queue to play.", isWhisper: true);
            this.GameQueueUserSelectedCommand = this.GameQueueUserSelectedCommand ?? CustomCommand.BasicChatCommand("Game Queue Used Selected", "It's time to play @$username! Listen carefully for instructions on how to join...");

            this.GiveawayStartedReminderCommand = this.GiveawayStartedReminderCommand ?? CustomCommand.BasicChatCommand("Giveaway Started/Reminder", "A giveaway has started for $giveawayitem! Type $giveawaycommand in chat in the next $giveawaytimelimit minute(s) to enter!");
            this.GiveawayUserJoinedCommand = this.GiveawayUserJoinedCommand ?? CustomCommand.BasicChatCommand("Giveaway User Joined", "You have been entered into the giveaway, stay tuned to see who wins!", isWhisper: true);
            this.GiveawayWinnerSelectedCommand = this.GiveawayWinnerSelectedCommand ?? CustomCommand.BasicChatCommand("Giveaway Winner Selected", "Congratulations @$username, you won $giveawayitem!");

            this.ModerationStrike1Command = this.ModerationStrike1Command ?? CustomCommand.BasicChatCommand("Moderation Strike 1", "$moderationreason. You have received a moderation strike & currently have $usermoderationstrikes strike(s)", isWhisper: true);
            this.ModerationStrike2Command = this.ModerationStrike2Command ?? CustomCommand.BasicChatCommand("Moderation Strike 2", "$moderationreason. You have received a moderation strike & currently have $usermoderationstrikes strike(s)", isWhisper: true);
            this.ModerationStrike3Command = this.ModerationStrike3Command ?? CustomCommand.BasicChatCommand("Moderation Strike 3", "$moderationreason. You have received a moderation strike & currently have $usermoderationstrikes strike(s)", isWhisper: true);

            if (this.DashboardItems.Count < 4)
            {
                this.DashboardItems = new List<DashboardItemTypeEnum>() { DashboardItemTypeEnum.None, DashboardItemTypeEnum.None, DashboardItemTypeEnum.None, DashboardItemTypeEnum.None };
            }
            if (this.DashboardQuickCommands.Count < 5)
            {
                this.DashboardQuickCommands = new List<Guid>() { Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty };
            }
        }

        public Version GetLatestVersion() { return Assembly.GetEntryAssembly().GetName().Version; }
    }
}