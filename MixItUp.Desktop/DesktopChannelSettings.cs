using Mixer.Base.Model.Channel;
using Mixer.Base.Model.OAuth;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Interactive;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace MixItUp.Desktop
{
    [DataContract]
    public class DesktopSavableChannelSettings : ISavableChannelSettings
    {
        [JsonProperty]
        public string Version { get; set; }

        [JsonProperty]
        public bool IsStreamer { get; set; }

        [JsonProperty]
        public OAuthTokenModel OAuthToken { get; set; }
        [JsonProperty]
        public OAuthTokenModel BotOAuthToken { get; set; }

        [JsonProperty]
        public ExpandedChannelModel Channel { get; set; }

        [JsonProperty]
        public UserItemAcquisitonViewModel CurrencyAcquisition { get; set; }

        [JsonProperty]
        public UserItemAcquisitonViewModel RankAcquisition { get; set; }
        [JsonProperty]
        public List<UserRankViewModel> Ranks { get; set; }

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
        public int CapsBlockCount { get; set; }
        [JsonProperty]
        public int PunctuationBlockCount { get; set; }
        [JsonProperty]
        public int EmoteBlockCount { get; set; }
        [JsonProperty]
        public bool BlockLinks { get; set; }
        [JsonProperty]
        public int Timeout1MinuteOffenseCount { get; set; }
        [JsonProperty]
        public int Timeout5MinuteOffenseCount { get; set; }

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
        protected List<UserDataViewModel> userDataInternal { get; set; }

        [JsonProperty]
        protected List<PreMadeChatCommandSettings> preMadeChatCommandSettingsInternal { get; set; }
        [JsonProperty]
        protected List<ChatCommand> chatCommandsInternal { get; set; }
        [JsonProperty]
        protected List<EventCommand> eventCommandsInternal { get; set; }
        [JsonProperty]
        protected List<InteractiveCommand> interactiveControlsInternal { get; set; }
        [JsonProperty]
        protected List<TimerCommand> timerCommandsInternal { get; set; }

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
            this.userDataInternal = new List<UserDataViewModel>();
            this.preMadeChatCommandSettingsInternal = new List<PreMadeChatCommandSettings>();
            this.chatCommandsInternal = new List<ChatCommand>();
            this.eventCommandsInternal = new List<EventCommand>();
            this.interactiveControlsInternal = new List<InteractiveCommand>();
            this.timerCommandsInternal = new List<TimerCommand>();
            this.quotesInternal = new List<string>();
            this.bannedWordsInternal = new List<string>();
            this.interactiveUserGroupsInternal = new Dictionary<uint, List<InteractiveUserGroupViewModel>>();
            this.interactiveCooldownGroupsInternal = new Dictionary<string, int>();
        }
    }

    [DataContract]
    public class DesktopChannelSettings : DesktopSavableChannelSettings, IChannelSettings
    {
        [JsonIgnore]
        public LockedDictionary<uint, UserDataViewModel> UserData { get; set; }

        [JsonIgnore]
        public LockedList<PreMadeChatCommandSettings> PreMadeChatCommandSettings { get; set; }
        [JsonIgnore]
        public LockedList<ChatCommand> ChatCommands { get; set; }
        [JsonIgnore]
        public LockedList<EventCommand> EventCommands { get; set; }
        [JsonIgnore]
        public LockedList<InteractiveCommand> InteractiveControls { get; set; }
        [JsonIgnore]
        public LockedList<TimerCommand> TimerCommands { get; set; }

        [JsonIgnore]
        public LockedList<string> Quotes { get; set; }
        [JsonIgnore]
        public LockedList<string> BannedWords { get; set; }

        [JsonIgnore]
        public LockedDictionary<uint, List<InteractiveUserGroupViewModel>> InteractiveUserGroups { get; set; }
        [JsonIgnore]
        public LockedDictionary<string, int> InteractiveCooldownGroups { get; set; }

        public DesktopChannelSettings(ExpandedChannelModel channel, bool isStreamer = true)
            : this()
        {
            this.Channel = channel;
            this.IsStreamer = isStreamer;

            this.Version = this.GetLatestVersion().ToString();
        }

        public DesktopChannelSettings()
            : base()
        {
            this.CurrencyAcquisition = new UserItemAcquisitonViewModel();
            this.RankAcquisition = new UserItemAcquisitonViewModel();
            this.Ranks = new List<UserRankViewModel>();

            this.TimerCommandsInterval = 10;
            this.TimerCommandsMinimumMessages = 10;

            this.GiveawayCommand = "giveaway";
            this.GiveawayUserRole = UserRole.User;
            this.GiveawayTimer = 60;

            this.MaxMessagesInChat = 100;

            this.UserData = new LockedDictionary<uint, UserDataViewModel>();
            this.PreMadeChatCommandSettings = new LockedList<PreMadeChatCommandSettings>();
            this.ChatCommands = new LockedList<ChatCommand>();
            this.EventCommands = new LockedList<EventCommand>();
            this.InteractiveControls = new LockedList<InteractiveCommand>();
            this.TimerCommands = new LockedList<TimerCommand>();
            this.Quotes = new LockedList<string>();
            this.BannedWords = new LockedList<string>();
            this.InteractiveUserGroups = new LockedDictionary<uint, List<InteractiveUserGroupViewModel>>();
            this.InteractiveCooldownGroups = new LockedDictionary<string, int>();
        }

        public void Initialize()
        {
            this.UserData = new LockedDictionary<uint, UserDataViewModel>(this.userDataInternal.ToDictionary(u => u.ID, u => u));
            this.PreMadeChatCommandSettings = new LockedList<PreMadeChatCommandSettings>(this.preMadeChatCommandSettingsInternal);
            this.ChatCommands = new LockedList<ChatCommand>(this.chatCommandsInternal);
            this.EventCommands = new LockedList<EventCommand>(this.eventCommandsInternal);
            this.InteractiveControls = new LockedList<InteractiveCommand>(this.interactiveControlsInternal);
            this.TimerCommands = new LockedList<TimerCommand>(this.timerCommandsInternal);
            this.Quotes = new LockedList<string>(this.quotesInternal);
            this.BannedWords = new LockedList<string>(this.bannedWordsInternal);
            this.InteractiveUserGroups = new LockedDictionary<uint, List<InteractiveUserGroupViewModel>>(this.interactiveUserGroupsInternal);
            this.InteractiveCooldownGroups = new LockedDictionary<string, int>(this.interactiveCooldownGroupsInternal);
        }

        public void CopyLatestValues()
        {
            this.OAuthToken = ChannelSession.Connection.Connection.GetOAuthTokenCopy();
            if (ChannelSession.BotConnection != null)
            {
                this.BotOAuthToken = ChannelSession.BotConnection.Connection.GetOAuthTokenCopy();
            }

            this.preMadeChatCommandSettingsInternal = this.PreMadeChatCommandSettings.ToList();
            this.chatCommandsInternal = this.ChatCommands.ToList();
            this.eventCommandsInternal = this.EventCommands.ToList();
            this.interactiveControlsInternal = this.InteractiveControls.ToList();
            this.timerCommandsInternal = this.TimerCommands.ToList();
            this.quotesInternal = this.Quotes.ToList();
            this.bannedWordsInternal = this.BannedWords.ToList();
            this.interactiveUserGroupsInternal = this.InteractiveUserGroups.ToDictionary();
            this.interactiveCooldownGroupsInternal = this.InteractiveCooldownGroups.ToDictionary();
        }

        public Version GetLatestVersion() { return Assembly.GetEntryAssembly().GetName().Version; }

        public bool ShouldBeUpgraded()
        {
            Version latest = this.GetLatestVersion();
            Version current = (!string.IsNullOrEmpty(this.Version)) ? new Version(this.Version) : new Version();
            return latest.CompareTo(current) > 0;
        }
    }

    [DataContract]
    public class LegacyDesktopChannelSettings : DesktopChannelSettings
    {
        [JsonProperty]
        public new Dictionary<uint, List<InteractiveUserGroupViewModel>> InteractiveUserGroups { get; set; }
        [JsonProperty]
        public new Dictionary<string, int> InteractiveCooldownGroups { get; set; }
    }
}
