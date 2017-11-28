using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Interactive;
using Mixer.Base.Model.OAuth;
using Mixer.Base.Util;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Interactive;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base
{
    [DataContract]
    public class ChannelSettings
    {
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1);

        private const string SettingsDirectoryName = "Settings";

        public static async Task<IEnumerable<ChannelSettings>> GetAllAvailableSettings()
        {
            if (!Directory.Exists(SettingsDirectoryName))
            {
                Directory.CreateDirectory(SettingsDirectoryName);
            }

            List<ChannelSettings> settings = new List<ChannelSettings>();
            foreach (string filePath in Directory.GetFiles(SettingsDirectoryName))
            {
                if (filePath.EndsWith("xml"))
                {
                    ChannelSettings setting = await ChannelSettings.LoadSettings(filePath);
                    if (setting != null)
                    {
                        settings.Add(setting);
                    }
                }
            }
            return settings;
        }

        public static async Task<ChannelSettings> LoadSettings(string filePath)
        {
            try
            {
                return await SerializerHelper.DeserializeFromFile<ChannelSettings>(filePath);
            }
            catch (JsonSerializationException) { }

            // If we get a serialization error, try doing minor upgrade tweaks and then deserializing the file again

            return await SerializerHelper.DeserializeFromFile<ChannelSettings>(filePath);
        }

        [JsonProperty]
        public bool IsStreamer { get; set; }

        [JsonProperty]
        private List<PreMadeChatCommandSettings> preMadeChatCommandSettingsInternal { get; set; }
        [JsonProperty]
        private List<ChatCommand> chatCommandsInternal { get; set; }
        [JsonProperty]
        private List<EventCommand> eventCommandsInternal { get; set; }
        [JsonProperty]
        private List<InteractiveCommand> interactiveControlsInternal { get; set; }
        [JsonProperty]
        private List<TimerCommand> timerCommandsInternal { get; set; }
        [JsonProperty]
        private List<string> quotesInternal { get; set; }
        [JsonProperty]
        private List<UserDataViewModel> userDataInternal { get; set; }

        [JsonProperty]
        public OAuthTokenModel OAuthToken { get; set; }
        [JsonProperty]
        public OAuthTokenModel BotOAuthToken { get; set; }

        [JsonProperty]
        public ExpandedChannelModel Channel { get; set; }

        [JsonProperty]
        public bool CurrencyEnabled { get; set; }
        [JsonProperty]
        public string CurrencyName { get; set; }
        [JsonProperty]
        public int CurrencyAcquireAmount { get; set; }
        [JsonProperty]
        public int CurrencyAcquireInterval { get; set; }

        [JsonProperty]
        public bool QuotesEnabled { get; set; }

        [JsonProperty]
        public int TimerCommandsInterval { get; set; }
        [JsonProperty]
        public int TimerCommandsMinimumMessages { get; set; }

        [JsonProperty]
        public Dictionary<uint, List<InteractiveUserGroupViewModel>> InteractiveUserGroups { get; set; }
        [JsonProperty]
        public Dictionary<string, int> InteractiveCooldownGroups { get; set; }

        [JsonProperty]
        public UserRole GiveawayUserRole { get; set; }
        [JsonProperty]
        public string GiveawayCommand { get; set; }
        [JsonProperty]
        public int GiveawayTimer { get; set; }

        [JsonProperty]
        public List<string> bannedWordsInternal { get; set; }
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
        public LockedDictionary<uint, UserDataViewModel> UserData { get; set; }
        [JsonIgnore]
        public LockedList<string> BannedWords { get; set; }

        public ChannelSettings(ExpandedChannelModel channel, bool isStreamer = true)
            : this()
        {
            this.Channel = channel;
            this.IsStreamer = isStreamer;
        }

        public ChannelSettings()
        {
            this.preMadeChatCommandSettingsInternal = new List<PreMadeChatCommandSettings>();
            this.chatCommandsInternal = new List<ChatCommand>();
            this.eventCommandsInternal = new List<EventCommand>();
            this.interactiveControlsInternal = new List<InteractiveCommand>();
            this.timerCommandsInternal = new List<TimerCommand>();
            this.quotesInternal = new List<string>();
            this.userDataInternal = new List<UserDataViewModel>();
            this.bannedWordsInternal = new List<string>();

            this.InteractiveUserGroups = new Dictionary<uint, List<InteractiveUserGroupViewModel>>();
            this.InteractiveCooldownGroups = new Dictionary<string, int>();        

            this.PreMadeChatCommandSettings = new LockedList<PreMadeChatCommandSettings>();
            this.ChatCommands = new LockedList<ChatCommand>();
            this.EventCommands = new LockedList<EventCommand>();
            this.InteractiveControls = new LockedList<InteractiveCommand>();
            this.TimerCommands = new LockedList<TimerCommand>();
            this.Quotes = new LockedList<string>();
            this.UserData = new LockedDictionary<uint, UserDataViewModel>();
            this.BannedWords = new LockedList<string>();

            this.TimerCommandsInterval = 10;
            this.TimerCommandsMinimumMessages = 10;

            this.GiveawayCommand = "giveaway";
            this.GiveawayUserRole = UserRole.User;
            this.GiveawayTimer = 60;

            this.MaxMessagesInChat = 100;
        }

        public void Initialize()
        {
            this.PreMadeChatCommandSettings = new LockedList<PreMadeChatCommandSettings>(this.preMadeChatCommandSettingsInternal);
            this.ChatCommands = new LockedList<ChatCommand>(this.chatCommandsInternal);
            this.EventCommands = new LockedList<EventCommand>(this.eventCommandsInternal);
            this.InteractiveControls = new LockedList<InteractiveCommand>(this.interactiveControlsInternal);
            this.TimerCommands = new LockedList<TimerCommand>(this.timerCommandsInternal);
            this.Quotes = new LockedList<string>(this.quotesInternal);
            this.UserData = new LockedDictionary<uint, UserDataViewModel>(this.userDataInternal.ToDictionary(u => u.ID, u => u));
            this.BannedWords = new LockedList<string>(this.bannedWordsInternal);
        }

        public async Task<bool> SaveAndValidate()
        {
            try
            {
                await this.Save();
                ChannelSettings settings = await ChannelSettings.LoadSettings(this.GetSettingsFileName());
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return false;
        }

        public async Task Save()
        {
            Directory.CreateDirectory(SettingsDirectoryName);
            string filePath = this.GetSettingsFileName();
            await this.Save(filePath);
        }

        public async Task Save(string filePath)
        {
            await semaphore.WaitAsync();

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
            this.userDataInternal = this.UserData.Values.ToList();
            this.bannedWordsInternal = this.BannedWords.ToList();

            await SerializerHelper.SerializeToFile(filePath, this);

            semaphore.Release();
        }

        public async Task SaveBackup()
        {
            string filePath = this.GetSettingsFileName();
            await this.Save(filePath + ".backup");
        }

        public string GetSettingsFileName()
        {
            return Path.Combine(SettingsDirectoryName, string.Format("{0}.{1}.xml", this.Channel.id.ToString(), (this.IsStreamer) ? "Streamer" : "Moderator"));
        }
    }
}
