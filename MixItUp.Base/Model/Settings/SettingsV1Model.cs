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
using System.Data.Common;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Settings
{
    [DataContract]
    [Obsolete]
    public class SettingsV1Model
    {
        public const int LatestVersion = 38;

        public const string SettingsFileExtension = ".xml";

        [JsonProperty]
        public int Version { get; set; } = SettingsV1Model.LatestVersion;

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
        public UserRoleEnum ModerationFilteredWordsExcempt { get; set; } = UserRoleEnum.Mod;
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
        public UserRoleEnum ModerationChatTextExcempt { get; set; } = UserRoleEnum.Mod;
        [JsonProperty]
        public bool ModerationChatTextApplyStrikes { get; set; } = true;

        [JsonProperty]
        public bool ModerationBlockLinks { get; set; }
        [JsonProperty]
        public UserRoleEnum ModerationBlockLinksExcempt { get; set; } = UserRoleEnum.Mod;
        [JsonProperty]
        public bool ModerationBlockLinksApplyStrikes { get; set; } = true;

        [JsonProperty]
        public ModerationChatInteractiveParticipationEnum ModerationChatInteractiveParticipation { get; set; } = ModerationChatInteractiveParticipationEnum.None;
        [JsonProperty]
        public UserRoleEnum ModerationChatInteractiveParticipationExcempt { get; set; } = UserRoleEnum.Mod;

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
        public List<SerialDeviceModel> SerialDevices { get; set; }

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

        [JsonProperty]
        public string TelemetryUserId { get; set; }

        [JsonProperty]
        public string SettingsBackupLocation { get; set; }
        [JsonProperty]
        public SettingsBackupRateEnum SettingsBackupRate { get; set; }
        [JsonProperty]
        public DateTimeOffset SettingsLastBackup { get; set; }

        [JsonProperty]
        public Dictionary<string, int> cooldownGroupsInternal { get; set; } = new Dictionary<string, int>();

        [JsonProperty]
        public List<PreMadeChatCommandSettings> preMadeChatCommandSettingsInternal { get; set; } = new List<PreMadeChatCommandSettings>();
        [JsonProperty]
        public List<ChatCommand> chatCommandsInternal { get; set; } = new List<ChatCommand>();
        [JsonProperty]
        public List<EventCommand> eventCommandsInternal { get; set; } = new List<EventCommand>();
        [JsonProperty]
        public List<MixPlayCommand> mixPlayCmmandsInternal { get; set; } = new List<MixPlayCommand>();
        [JsonProperty]
        public List<TimerCommand> timerCommandsInternal { get; set; } = new List<TimerCommand>();
        [JsonProperty]
        public List<ActionGroupCommand> actionGroupCommandsInternal { get; set; } = new List<ActionGroupCommand>();
        [JsonProperty]
        public List<GameCommandBase> gameCommandsInternal { get; set; } = new List<GameCommandBase>();

        [JsonProperty]
        public List<UserQuoteViewModel> userQuotesInternal { get; set; } = new List<UserQuoteViewModel>();

        [JsonProperty]
        public List<OverlayWidgetModel> overlayWidgetModelsInternal { get; set; } = new List<OverlayWidgetModel>();

        [JsonProperty]
        public List<RemoteProfileModel> remoteProfilesInternal { get; set; } = new List<RemoteProfileModel>();
        [JsonProperty]
        public Dictionary<Guid, RemoteProfileBoardsModel> remoteProfileBoardsInternal { get; set; } = new Dictionary<Guid, RemoteProfileBoardsModel>();

        [JsonProperty]
        public List<string> filteredWordsInternal { get; set; } = new List<string>();
        [JsonProperty]
        public List<string> bannedWordsInternal { get; set; } = new List<string>();

        [JsonProperty]
        public Dictionary<uint, List<MixPlayUserGroupModel>> mixPlayUserGroupsInternal { get; set; } = new Dictionary<uint, List<MixPlayUserGroupModel>>();
        [JsonProperty]
        [Obsolete]
        public Dictionary<string, int> interactiveCooldownGroupsInternal { get; set; } = new Dictionary<string, int>();

        public SettingsV1Model() { }

        [JsonIgnore]
        public Dictionary<uint, UserDataModel> UserData { get; set; } = new Dictionary<uint, UserDataModel>();

        [JsonProperty]
        public Dictionary<Guid, UserCurrencyModel> Currencies { get; set; } = new Dictionary<Guid, UserCurrencyModel>();
        [JsonProperty]
        public Dictionary<Guid, UserInventoryModel> Inventories { get; set; } = new Dictionary<Guid, UserInventoryModel>();

        [JsonIgnore]
        public string DatabaseFileName { get { return string.Format("{0}.{1}.sqlite", this.Channel.id.ToString(), (this.IsStreamer) ? "Streamer" : "Moderator"); } }

        public async Task LoadUserData()
        {
            Dictionary<uint, UserDataModel> initialUsers = new Dictionary<uint, UserDataModel>();
            await ChannelSession.Services.Database.Read(this.DatabaseFileName, "SELECT * FROM Users", (Dictionary<string, object> data) =>
            {
                UserDataModel userData = new UserDataModel();

                userData.MixerID = uint.Parse(data["ID"].ToString());
                userData.MixerUsername = data["UserName"].ToString();

                userData.ViewingMinutes = int.Parse(data["ViewingMinutes"].ToString());

                if (data.ContainsKey("CurrencyAmounts"))
                {
                    Dictionary<Guid, int> currencyAmounts = JsonConvert.DeserializeObject<Dictionary<Guid, int>>(data["CurrencyAmounts"].ToString());
                    if (currencyAmounts != null)
                    {
                        foreach (var kvp in currencyAmounts)
                        {
                            if (this.Currencies.ContainsKey(kvp.Key))
                            {
                                this.Currencies[kvp.Key].SetAmount(userData, kvp.Value);
                            }
                        }
                    }
                }

                if (data.ContainsKey("InventoryAmounts"))
                {
                    Dictionary<Guid, Dictionary<string, int>> inventoryAmounts = JsonConvert.DeserializeObject<Dictionary<Guid, Dictionary<string, int>>>(data["InventoryAmounts"].ToString());
                    if (inventoryAmounts != null)
                    {
                        foreach (var kvp in inventoryAmounts)
                        {
                            if (this.Inventories.ContainsKey(kvp.Key))
                            {
                                UserInventoryModel inventory = this.Inventories[kvp.Key];
                                foreach (var itemKVP in kvp.Value)
                                {
                                    inventory.SetAmount(userData, itemKVP.Key, itemKVP.Value);
                                }
                            }
                        }
                    }
                }

                if (data.ContainsKey("CustomCommands") && !string.IsNullOrEmpty(data["CustomCommands"].ToString()))
                {
                    userData.CustomCommands.AddRange(JSONSerializerHelper.DeserializeFromString<List<ChatCommand>>(data["CustomCommands"].ToString()));
                }

                if (data.ContainsKey("Options") && !string.IsNullOrEmpty(data["Options"].ToString()))
                {
                    JObject optionsJObj = JObject.Parse(data["Options"].ToString());
                    if (optionsJObj.ContainsKey("EntranceCommand") && optionsJObj["EntranceCommand"] != null)
                    {
                        userData.EntranceCommand = JSONSerializerHelper.DeserializeFromString<CustomCommand>(optionsJObj["EntranceCommand"].ToString());
                    }
                    userData.IsSparkExempt = GetOptionValue<bool>(optionsJObj, "IsSparkExempt");
                    userData.IsCurrencyRankExempt = GetOptionValue<bool>(optionsJObj, "IsCurrencyRankExempt");
                    userData.PatreonUserID = GetOptionValue<string>(optionsJObj, "PatreonUserID");
                    userData.ModerationStrikes = GetOptionValue<uint>(optionsJObj, "ModerationStrikes");
                    userData.CustomTitle = GetOptionValue<string>(optionsJObj, "CustomTitle");
                    userData.TotalStreamsWatched = GetOptionValue<uint>(optionsJObj, "TotalStreamsWatched");
                    userData.TotalAmountDonated = GetOptionValue<double>(optionsJObj, "TotalAmountDonated");
                    userData.TotalSparksSpent = GetOptionValue<uint>(optionsJObj, "TotalSparksSpent");
                    userData.TotalEmbersSpent = GetOptionValue<uint>(optionsJObj, "TotalEmbersSpent");
                    userData.TotalSubsGifted = GetOptionValue<uint>(optionsJObj, "TotalSubsGifted");
                    userData.TotalSubsReceived = GetOptionValue<uint>(optionsJObj, "TotalSubsReceived");
                    userData.TotalChatMessageSent = GetOptionValue<uint>(optionsJObj, "TotalChatMessageSent");
                    userData.TotalTimesTagged = GetOptionValue<uint>(optionsJObj, "TotalTimesTagged");
                    userData.TotalSkillsUsed = GetOptionValue<uint>(optionsJObj, "TotalSkillsUsed");
                    userData.TotalCommandsRun = GetOptionValue<uint>(optionsJObj, "TotalCommandsRun");
                    userData.TotalMonthsSubbed = GetOptionValue<uint>(optionsJObj, "TotalMonthsSubbed");
                }
            });
            this.UserData = new Dictionary<uint, UserDataModel>(initialUsers);
        }

        private T GetOptionValue<T>(JObject jobj, string key)
        {
            if (jobj[key] != null)
            {
                return jobj[key].ToObject<T>();
            }
            return default(T);
        }
    }
}
