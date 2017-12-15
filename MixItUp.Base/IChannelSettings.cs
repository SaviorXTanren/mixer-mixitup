using Mixer.Base.Model.Channel;
using Mixer.Base.Model.OAuth;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Interactive;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;

namespace MixItUp.Base
{
    public interface ISavableChannelSettings
    {
        int Version { get; set; }

        bool DiagnosticLogging { get; set; }

        bool IsStreamer { get; set; }

        OAuthTokenModel OAuthToken { get; set; }    
        OAuthTokenModel BotOAuthToken { get; set; }

        ExpandedChannelModel Channel { get; set; }

        UserItemAcquisitonViewModel CurrencyAcquisition { get; set; }

        UserItemAcquisitonViewModel RankAcquisition { get; set; }
        List<UserRankViewModel> Ranks { get; set; }
        CustomCommand RankChangedCommand { get; set; }

        bool GameQueueMustFollow { get; set; }
        bool GameQueueSubPriority { get; set; }

        bool QuotesEnabled { get; set; }
        
        int TimerCommandsInterval { get; set; }
        int TimerCommandsMinimumMessages { get; set; }

        UserRole GiveawayUserRole { get; set; }
        string GiveawayCommand { get; set; }
        int GiveawayCurrencyCost { get; set; }
        int GiveawayTimer { get; set; }

        int ModerationCapsBlockCount { get; set; }
        int ModerationPunctuationBlockCount { get; set; }
        int ModerationEmoteBlockCount { get; set; }
        bool ModerationBlockLinks { get; set; }
        bool ModerationIncludeModerators { get; set; }
        int ModerationTimeout1MinuteOffenseCount { get; set; }
        int ModerationTimeout5MinuteOffenseCount { get; set; }

        bool EnableOverlay { get; set; }
        string OBSStudioServerIP { get; set; }
        string OBSStudioServerPassword { get; set; }
        bool EnableXSplitConnection { get; set; }

        int MaxMessagesInChat { get; set; }
    }

    public interface IChannelSettings : ISavableChannelSettings
    {
        DatabaseDictionary<uint, UserDataViewModel> UserData { get; }

        LockedList<PreMadeChatCommandSettings> PreMadeChatCommandSettings { get; }
        LockedList<ChatCommand> ChatCommands { get; }
        LockedList<EventCommand> EventCommands { get; }
        LockedList<InteractiveCommand> InteractiveCommands { get; }
        LockedList<TimerCommand> TimerCommands { get; }

        LockedList<string> Quotes { get; }
        LockedList<string> BannedWords { get; }

        LockedDictionary<uint, List<InteractiveUserGroupViewModel>> InteractiveUserGroups { get; }
        LockedDictionary<string, int> InteractiveCooldownGroups { get; }
    }
}

