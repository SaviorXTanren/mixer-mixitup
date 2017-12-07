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
        CustomCommand RankChanged { get; set; }

        bool GameQueueMustFollow { get; set; }
        bool GameQueueSubPriority { get; set; }

        bool QuotesEnabled { get; set; }
        
        int TimerCommandsInterval { get; set; }
        int TimerCommandsMinimumMessages { get; set; }

        UserRole GiveawayUserRole { get; set; }
        string GiveawayCommand { get; set; }
        int GiveawayCurrencyCost { get; set; }
        int GiveawayTimer { get; set; }

        int CapsBlockCount { get; set; }
        int PunctuationBlockCount { get; set; }
        int EmoteBlockCount { get; set; }
        bool BlockLinks { get; set; }
        int Timeout1MinuteOffenseCount { get; set; }
        int Timeout5MinuteOffenseCount { get; set; }

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
        LockedList<InteractiveCommand> InteractiveControls { get; }
        LockedList<TimerCommand> TimerCommands { get; }

        LockedList<string> Quotes { get; }
        LockedList<string> BannedWords { get; }

        LockedDictionary<uint, List<InteractiveUserGroupViewModel>> InteractiveUserGroups { get; }
        LockedDictionary<string, int> InteractiveCooldownGroups { get; }
    }
}

