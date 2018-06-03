using Mixer.Base.Model.Channel;
using Mixer.Base.Model.OAuth;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Favorites;
using MixItUp.Base.Model.Remote;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Interactive;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;

namespace MixItUp.Base
{
    public interface ISavableChannelSettings
    {
        int Version { get; set; }

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

        ExpandedChannelModel Channel { get; set; }

        bool FeatureMe { get; set; }
        StreamingSoftwareTypeEnum DefaultStreamingSoftware { get; set; }
        string DefaultAudioOutput { get; set; }

        bool WhisperAllAlerts { get; set; }
        bool LatestChatAtTop { get; set; }
        bool HideViewerAndChatterNumbers { get; set; }
        bool TrackWhispererNumber { get; set; }
        bool AllowCommandWhispering { get; set; }
        bool IgnoreBotAccountCommands { get; set; }
        bool CommandsOnlyInYourStream { get; set; }
        bool DeleteChatCommandsWhenRun { get; set; }

        uint DefaultInteractiveGame { get; set; }

        bool GameQueueSubPriority { get; set; }
        RequirementViewModel GameQueueRequirements { get; set; }

        bool QuotesEnabled { get; set; }
        
        int TimerCommandsInterval { get; set; }
        int TimerCommandsMinimumMessages { get; set; }

        string GiveawayCommand { get; set; }
        bool GiveawayGawkBoxTrigger { get; set; }
        bool GiveawayStreamlabsTrigger { get; set; }
        bool GiveawayTiltifyTrigger { get; set; }
        bool GiveawayDonationRequiredAmount { get; set; }
        double GiveawayDonationAmount { get; set; }
        int GiveawayTimer { get; set; }
        RequirementViewModel GiveawayRequirements { get; set; }

        bool ModerationUseCommunityFilteredWords { get; set; }
        MixerRoleEnum ModerationFilteredWordsExcempt { get; set; }

        int ModerationCapsBlockCount { get; set; }
        bool ModerationCapsBlockIsPercentage { get; set; }
        int ModerationPunctuationBlockCount { get; set; }
        bool ModerationPunctuationBlockIsPercentage { get; set; }
        int ModerationEmoteBlockCount { get; set; }
        bool ModerationEmoteBlockIsPercentage { get; set; }
        MixerRoleEnum ModerationChatTextExcempt { get; set; }

        bool ModerationBlockLinks { get; set; }
        MixerRoleEnum ModerationBlockLinksExcempt { get; set; }

        int ModerationTimeout1MinuteOffenseCount { get; set; }
        int ModerationTimeout5MinuteOffenseCount { get; set; }
        MixerRoleEnum ModerationTimeoutExempt { get; set; }

        bool EnableOverlay { get; set; }
        string OverlaySourceName { get; set; }

        string OBSStudioServerIP { get; set; }
        string OBSStudioServerPassword { get; set; }

        bool EnableStreamlabsOBSConnection { get; set; }

        bool EnableXSplitConnection { get; set; }

        bool EnableDeveloperAPI { get; set; }

        int TiltifyCampaign { get; set; }

        bool UnlockAllCommands { get; set; }

        int ChatFontSize { get; set; }
        bool ChatShowUserJoinLeave { get; set; }
        string ChatUserJoinLeaveColorScheme { get; set; }
        bool ChatShowEventAlerts { get; set; }
        string ChatEventAlertsColorScheme { get; set; }
        bool ChatShowInteractiveAlerts { get; set; }
        string ChatInteractiveAlertsColorScheme { get; set; }

        string NotificationChatTaggedSoundFilePath { get; set; }
        string NotificationChatWhisperSoundFilePath { get; set; }
        string NotificationServiceConnectSoundFilePath { get; set; }
        string NotificationServiceDisconnectSoundFilePath { get; set; }

        int MaxMessagesInChat { get; set; }

        bool AutoExportStatistics { get; set; }

        List<RemoteBoardModel> RemoteBoards { get; set; }
        List<RemoteDeviceModel> RemoteSavedDevices { get; set; }

        List<FavoriteGroupModel> FavoriteGroups { get; set; }

        List<SongRequestServiceTypeEnum> SongRequestServiceTypes { get; set; }
        bool SpotifyAllowExplicit { get; set; }
    }

    public interface IChannelSettings : ISavableChannelSettings
    {
        DatabaseDictionary<uint, UserDataViewModel> UserData { get; }

        LockedDictionary<Guid, UserCurrencyViewModel> Currencies { get; }

        LockedDictionary<string, int> CooldownGroups { get; }

        LockedList<PreMadeChatCommandSettings> PreMadeChatCommandSettings { get; }
        LockedList<ChatCommand> ChatCommands { get; }
        LockedList<EventCommand> EventCommands { get; }
        LockedList<InteractiveCommand> InteractiveCommands { get; }
        LockedList<TimerCommand> TimerCommands { get; }
        LockedList<ActionGroupCommand> ActionGroupCommands { get; }
        LockedList<GameCommandBase> GameCommands { get; }
        LockedList<RemoteCommand> RemoteCommands { get; }

        LockedList<UserQuoteViewModel> UserQuotes { get; }

        LockedList<string> FilteredWords { get; }
        LockedList<string> BannedWords { get; }
        LockedList<string> CommunityFilteredWords { get; }

        LockedDictionary<uint, List<InteractiveUserGroupViewModel>> InteractiveUserGroups { get; }
    }
}

