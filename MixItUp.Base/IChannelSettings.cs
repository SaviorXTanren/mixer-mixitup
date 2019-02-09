using Mixer.Base.Model.Channel;
using Mixer.Base.Model.OAuth;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Favorites;
using MixItUp.Base.Model.Interactive;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Serial;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Interactive;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace MixItUp.Base
{
    public interface ISavableChannelSettings
    {
        int Version { get; set; }

        bool LicenseAccepted { get; set; }

        bool OptOutTracking { get; set; }

        bool ReRunWizard { get; set; }
        bool DiagnosticLogging { get; set; }

        bool IsStreamer { get; set; }

        OAuthTokenModel OAuthToken { get; set; }
        OAuthTokenModel BotOAuthToken { get; set; }

        OAuthTokenModel StreamlabsOAuthToken { get; set; }
        OAuthTokenModel GameWispOAuthToken { get; set; }
        OAuthTokenModel GawkBoxOAuthToken { get; set; }
        OAuthTokenModel TwitterOAuthToken { get; set; }
        OAuthTokenModel SpotifyOAuthToken { get; set; }
        OAuthTokenModel DiscordOAuthToken { get; set; }
        OAuthTokenModel TiltifyOAuthToken { get; set; }
        OAuthTokenModel TipeeeStreamOAuthToken { get; set; }
        OAuthTokenModel TreatStreamOAuthToken { get; set; }
        OAuthTokenModel StreamJarOAuthToken { get; set; }
        OAuthTokenModel PatreonOAuthToken { get; set; }

        Dictionary<string, CommandGroupSettings> CommandGroups { get; set; }
        Dictionary<string, HotKeyConfiguration> HotKeys { get; set; }

        ExpandedChannelModel Channel { get; set; }

        bool FeatureMe { get; set; }
        StreamingSoftwareTypeEnum DefaultStreamingSoftware { get; set; }
        string DefaultAudioOutput { get; set; }
        bool SaveChatEventLogs { get; set; }

        bool WhisperAllAlerts { get; set; }
        bool LatestChatAtTop { get; set; }
        bool HideViewerAndChatterNumbers { get; set; }
        bool HideChatUserList { get; set; }
        bool HideDeletedMessages { get; set; }
        bool TrackWhispererNumber { get; set; }
        bool AllowCommandWhispering { get; set; }
        bool IgnoreBotAccountCommands { get; set; }
        bool CommandsOnlyInYourStream { get; set; }
        bool DeleteChatCommandsWhenRun { get; set; }

        uint DefaultInteractiveGame { get; set; }
        bool PreventUnknownInteractiveUsers { get; set; }
        bool PreventSmallerCooldowns { get; set; }
        List<InteractiveSharedProjectModel> CustomInteractiveProjectIDs { get; set; }

        int RegularUserMinimumHours { get; set; }
        List<UserTitleModel> UserTitles { get; set; }

        bool GameQueueSubPriority { get; set; }
        RequirementViewModel GameQueueRequirements { get; set; }

        bool QuotesEnabled { get; set; }
        string QuotesFormat { get; set; }
        
        int TimerCommandsInterval { get; set; }
        int TimerCommandsMinimumMessages { get; set; }
        bool DisableAllTimers { get; set; }

        string GiveawayCommand { get; set; }
        bool GiveawayGawkBoxTrigger { get; set; }
        bool GiveawayStreamlabsTrigger { get; set; }
        bool GiveawayTiltifyTrigger { get; set; }
        bool GiveawayDonationRequiredAmount { get; set; }
        double GiveawayDonationAmount { get; set; }
        int GiveawayTimer { get; set; }
        int GiveawayMaximumEntries { get; set; }
        RequirementViewModel GiveawayRequirements { get; set; }
        int GiveawayReminderInterval { get; set; }
        bool GiveawayRequireClaim { get; set; }
        bool GiveawayAllowPastWinners { get; set; }
        CustomCommand GiveawayUserJoinedCommand { get; set; }
        CustomCommand GiveawayWinnerSelectedCommand { get; set; }

        bool ModerationUseCommunityFilteredWords { get; set; }
        MixerRoleEnum ModerationFilteredWordsExcempt { get; set; }
        bool ModerationFilteredWordsApplyStrikes { get; set; }

        int ModerationCapsBlockCount { get; set; }
        bool ModerationCapsBlockIsPercentage { get; set; }
        int ModerationPunctuationBlockCount { get; set; }
        bool ModerationPunctuationBlockIsPercentage { get; set; }
        MixerRoleEnum ModerationChatTextExcempt { get; set; }
        bool ModerationChatTextApplyStrikes { get; set; }

        bool ModerationBlockLinks { get; set; }
        MixerRoleEnum ModerationBlockLinksExcempt { get; set; }
        bool ModerationBlockLinksApplyStrikes { get; set; }

        ModerationChatInteractiveParticipationEnum ModerationChatInteractiveParticipation { get; set; }

        bool ModerationResetStrikesOnLaunch { get; set; }
        CustomCommand ModerationStrike1Command { get; set; }
        CustomCommand ModerationStrike2Command { get; set; }
        CustomCommand ModerationStrike3Command { get; set; }

        bool EnableOverlay { get; set; }
        Dictionary<string, int> OverlayCustomNameAndPorts { get; set; }
        string OverlaySourceName { get; set; }
        int OverlayWidgetRefreshTime { get; set; }

        string OBSStudioServerIP { get; set; }
        string OBSStudioServerPassword { get; set; }

        bool EnableStreamlabsOBSConnection { get; set; }

        bool EnableXSplitConnection { get; set; }

        bool EnableDeveloperAPI { get; set; }

        int TiltifyCampaign { get; set; }

        int ExtraLifeTeamID { get; set; }
        int ExtraLifeParticipantID { get; set; }
        bool ExtraLifeIncludeTeamDonations { get; set; }

        string DiscordServer { get; set; }

        string PatreonTierMixerSubscriberEquivalent { get; set; }

        bool UnlockAllCommands { get; set; }

        int ChatFontSize { get; set; }
        bool ChatShowUserJoinLeave { get; set; }
        string ChatUserJoinLeaveColorScheme { get; set; }
        bool ChatShowEventAlerts { get; set; }
        string ChatEventAlertsColorScheme { get; set; }
        bool ChatShowInteractiveAlerts { get; set; }
        string ChatInteractiveAlertsColorScheme { get; set; }

        string NotificationChatMessageSoundFilePath { get; set; }
        string NotificationChatTaggedSoundFilePath { get; set; }
        string NotificationChatWhisperSoundFilePath { get; set; }
        string NotificationServiceConnectSoundFilePath { get; set; }
        string NotificationServiceDisconnectSoundFilePath { get; set; }

        int MaxMessagesInChat { get; set; }

        bool AutoExportStatistics { get; set; }

        List<SerialDeviceModel> SerialDevices { get; set; }

        List<FavoriteGroupModel> FavoriteGroups { get; set; }

        HashSet<SongRequestServiceTypeEnum> SongRequestServiceTypes { get; set; }
        bool SpotifyAllowExplicit { get; set; }
        string DefaultPlaylist { get; set; }
        int SongRequestVolume { get; set; }

        Dictionary<uint, JObject> CustomInteractiveSettings { get; set; }

        string TelemetryUserId { get; set; }

        string SettingsBackupLocation { get; set; }
        SettingsBackupRateEnum SettingsBackupRate { get; set; }
        DateTimeOffset SettingsLastBackup { get; set; }
    }

    public interface IChannelSettings : ISavableChannelSettings
    {
        DatabaseDictionary<uint, UserDataViewModel> UserData { get; }

        LockedDictionary<Guid, UserCurrencyViewModel> Currencies { get; }
        LockedDictionary<Guid, UserInventoryViewModel> Inventories { get; }

        LockedDictionary<string, int> CooldownGroups { get; }

        LockedList<PreMadeChatCommandSettings> PreMadeChatCommandSettings { get; }
        LockedList<ChatCommand> ChatCommands { get; }
        LockedList<EventCommand> EventCommands { get; }
        LockedList<InteractiveCommand> InteractiveCommands { get; }
        LockedList<TimerCommand> TimerCommands { get; }
        LockedList<ActionGroupCommand> ActionGroupCommands { get; }
        LockedList<GameCommandBase> GameCommands { get; }

        LockedList<UserQuoteViewModel> UserQuotes { get; }

        LockedList<OverlayWidget> OverlayWidgets { get; set; }

        LockedList<string> FilteredWords { get; }
        LockedList<string> BannedWords { get; }
        LockedList<string> CommunityFilteredWords { get; }

        LockedDictionary<uint, List<InteractiveUserGroupViewModel>> InteractiveUserGroups { get; }
    }

    public static class DbDataReaderExtensions
    {
        public static bool ColumnExists(this DbDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }

}

