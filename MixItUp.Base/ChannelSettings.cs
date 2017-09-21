using Mixer.Base.Model.Channel;
using Mixer.Base.Model.OAuth;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
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
            List<ChannelSettings> settings = new List<ChannelSettings>();
            foreach (string filePath in Directory.GetFiles(SettingsDirectoryName))
            {
                ChannelSettings setting = await SerializerHelper.DeserializeFromFile<ChannelSettings>(filePath);
                if (setting != null)
                {
                    settings.Add(setting);
                }
            }
            return settings;
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
        public OAuthTokenModel OAuthToken { get; set; }

        [JsonProperty]
        public OAuthTokenModel BotOAuthToken { get; set; }

        [JsonProperty]
        public ExpandedChannelModel Channel { get; set; }

        [JsonProperty]
        public Dictionary<uint, UserDataViewModel> UserData { get; set; }

        [JsonProperty]
        public bool QuotesEnabled { get; set; }

        [JsonProperty]
        public int TimerCommandsInterval { get; set; }

        [JsonProperty]
        public int TimerCommandsMinimumMessages { get; set; }

        [JsonProperty]
        public bool EnableOverlay { get; set; }

        [JsonProperty]
        public string OBSStudioServerIP { get; set; }
        
        [JsonProperty]
        public string OBSStudioServerPassword { get; set; }

        [JsonProperty]
        public bool EnableXSplitConnection { get; set; }


        
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

            this.UserData = new Dictionary<uint, UserDataViewModel>();

            this.PreMadeChatCommandSettings = new LockedList<PreMadeChatCommandSettings>();
            this.ChatCommands = new LockedList<ChatCommand>();
            this.EventCommands = new LockedList<EventCommand>();
            this.InteractiveControls = new LockedList<InteractiveCommand>();
            this.TimerCommands = new LockedList<TimerCommand>();
            this.Quotes = new LockedList<string>();

            this.TimerCommandsInterval = 10;
            this.TimerCommandsMinimumMessages = 10;
        }

        public void Initialize()
        {
            this.PreMadeChatCommandSettings = new LockedList<PreMadeChatCommandSettings>(this.preMadeChatCommandSettingsInternal);
            this.ChatCommands = new LockedList<ChatCommand>(this.chatCommandsInternal);
            this.EventCommands = new LockedList<EventCommand>(this.eventCommandsInternal);
            this.InteractiveControls = new LockedList<InteractiveCommand>(this.interactiveControlsInternal);
            this.TimerCommands = new LockedList<TimerCommand>(this.timerCommandsInternal);
            this.Quotes = new LockedList<string>(this.quotesInternal);
        }

        public async Task Save()
        {
            await semaphore.WaitAsync();

            Directory.CreateDirectory(SettingsDirectoryName);
            string filePath = Path.Combine(SettingsDirectoryName, string.Format("{0}.{1}.xml", this.Channel.id.ToString(), (this.IsStreamer) ? "Streamer" : "Moderator"));

            this.OAuthToken = ChannelSession.MixerConnection.GetOAuthTokenCopy();
            this.BotOAuthToken = (ChannelSession.BotConnection != null) ? ChannelSession.BotConnection.GetOAuthTokenCopy() : null;

            this.preMadeChatCommandSettingsInternal = this.PreMadeChatCommandSettings.ToList();
            this.chatCommandsInternal = this.ChatCommands.ToList();
            this.eventCommandsInternal = this.EventCommands.ToList();
            this.interactiveControlsInternal = this.InteractiveControls.ToList();
            this.timerCommandsInternal = this.TimerCommands.ToList();
            this.quotesInternal = this.Quotes.ToList();

            await SerializerHelper.SerializeToFile(filePath, this);

            semaphore.Release();
        }
    }
}
