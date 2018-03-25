using Mixer.Base.Model.Channel;
using Mixer.Base.Model.OAuth;
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
        OAuthTokenModel TwitterOAuthToken { get; set; }
        OAuthTokenModel SpotifyOAuthToken { get; set; }

        ExpandedChannelModel Channel { get; set; }

        bool GameQueueSubPriority { get; set; }
        RequirementViewModel GameQueueRequirements { get; set; }

        bool QuotesEnabled { get; set; }
        
        int TimerCommandsInterval { get; set; }
        int TimerCommandsMinimumMessages { get; set; }

        string GiveawayCommand { get; set; }
        int GiveawayTimer { get; set; }
        RequirementViewModel GiveawayRequirements { get; set; }

        bool ModerationUseCommunityFilteredWords { get; set; }
        UserRole ModerationFilteredWordsExcempt { get; set; }

        int ModerationCapsBlockCount { get; set; }
        bool ModerationCapsBlockIsPercentage { get; set; }
        int ModerationPunctuationBlockCount { get; set; }
        bool ModerationPunctuationBlockIsPercentage { get; set; }
        int ModerationEmoteBlockCount { get; set; }
        bool ModerationEmoteBlockIsPercentage { get; set; }
        UserRole ModerationChatTextExcempt { get; set; }

        bool ModerationBlockLinks { get; set; }
        UserRole ModerationBlockLinksExcempt { get; set; }

        int ModerationTimeout1MinuteOffenseCount { get; set; }
        int ModerationTimeout5MinuteOffenseCount { get; set; }

        bool EnableOverlay { get; set; }
        string OverlaySourceName { get; set; }

        string OBSStudioServerIP { get; set; }
        string OBSStudioServerPassword { get; set; }

        bool EnableXSplitConnection { get; set; }

        bool EnableDeveloperAPI { get; set; }

        int ChatFontSize { get; set; }
        bool ChatShowUserJoinLeave { get; set; }
        bool ChatShowEventAlerts { get; set; }
        bool ChatShowInteractiveAlerts { get; set; }
        int MaxMessagesInChat { get; set; }

        bool AutoExportStatistics { get; set; }

        List<RemoteBoardModel> RemoteBoards { get; set; }
        List<RemoteDeviceModel> RemoteSavedDevices { get; set; }

        List<FavoriteGroupModel> FavoriteGroups { get; set; }

        SongRequestServiceTypeEnum SongRequestServiceType { get; set; }
        bool SpotifyAllowExplicit { get; set; }
    }

    public interface IChannelSettings : ISavableChannelSettings
    {
        DatabaseDictionary<uint, UserDataViewModel> UserData { get; }

        LockedDictionary<Guid, UserCurrencyViewModel> Currencies { get; }

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
        LockedDictionary<string, int> InteractiveCooldownGroups { get; }
    }
}

