using Mixer.Base.Model.Channel;
using Mixer.Base.Model.OAuth;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base
{
    [DataContract]
    public class ChannelSettings
    {
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

        private static string GetSettingsFilePath(ChannelModel channel) { return Path.Combine(SettingsDirectoryName, string.Format("{0}.xml", channel.id.ToString())); }

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
        public List<UserDataViewModel> UserData { get; set; }

        [JsonProperty]
        public bool QuotesEnabled { get; set; }

        [JsonProperty]
        public int TimerCommandsInterval { get; set; }

        [JsonProperty]
        public int TimerCommandsMinimumMessages { get; set; }

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

        public ChannelSettings(ExpandedChannelModel channel)
            : this()
        {
            this.Channel = channel;
        }

        public ChannelSettings()
        {
            this.chatCommandsInternal = new List<ChatCommand>();
            this.eventCommandsInternal = new List<EventCommand>();
            this.interactiveControlsInternal = new List<InteractiveCommand>();
            this.timerCommandsInternal = new List<TimerCommand>();
            this.quotesInternal = new List<string>();

            this.UserData = new List<UserDataViewModel>();

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
            this.ChatCommands = new LockedList<ChatCommand>(this.chatCommandsInternal);
            this.EventCommands = new LockedList<EventCommand>(this.eventCommandsInternal);
            this.InteractiveControls = new LockedList<InteractiveCommand>(this.interactiveControlsInternal);
            this.TimerCommands = new LockedList<TimerCommand>(this.timerCommandsInternal);
            this.Quotes = new LockedList<string>(this.quotesInternal);

            this.ChatCommands.Add(new UptimeChatCommand());
            this.ChatCommands.Add(new GameChatCommand());
            this.ChatCommands.Add(new TitleChatCommand());
            this.ChatCommands.Add(new TimeoutChatCommand());
            this.ChatCommands.Add(new PurgeChatCommand());
            this.ChatCommands.Add(new StreamerAgeChatCommand());
            this.ChatCommands.Add(new MixerAgeChatCommand());
            this.ChatCommands.Add(new FollowAgeChatCommand());
            this.ChatCommands.Add(new SparksChatCommand());
            this.ChatCommands.Add(new QuoteChatCommand());
            this.ChatCommands.Add(new GiveawayChatCommand());
        }

        public async Task Save()
        {
            Directory.CreateDirectory(SettingsDirectoryName);
            string filePath = ChannelSettings.GetSettingsFilePath(this.Channel);

            this.OAuthToken = ChannelSession.MixerConnection.GetOAuthTokenCopy();
            this.BotOAuthToken = (ChannelSession.BotConnection != null) ? ChannelSession.BotConnection.GetOAuthTokenCopy() : null;

            this.chatCommandsInternal = this.ChatCommands.ToList();
            this.eventCommandsInternal = this.EventCommands.ToList();
            this.interactiveControlsInternal = this.InteractiveControls.ToList();
            this.timerCommandsInternal = this.TimerCommands.ToList();
            this.quotesInternal = this.Quotes.ToList();

            this.chatCommandsInternal.RemoveAll(c => c.Actions.Any(a => a.Type == ActionTypeEnum.Custom));

            await SerializerHelper.SerializeToFile(filePath, this);
        }
    }
}
