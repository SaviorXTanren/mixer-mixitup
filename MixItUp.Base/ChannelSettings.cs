using Mixer.Base.Model.Channel;
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

        public static async Task<ChannelSettings> LoadSettings(ChannelModel channel)
        {
            ChannelSettings settings = null;
            string filePath = ChannelSettings.GetSettingsFilePath(channel);
            if (File.Exists(filePath))
            {
                settings = await SerializerHelper.DeserializeFromFile<ChannelSettings>(filePath);

                settings.Channel = channel;
                settings.ChatCommands = new LockedList<ChatCommand>(settings.chatCommandsInternal);
                settings.EventCommands = new LockedList<EventCommand>(settings.eventCommandsInternal);
                settings.InteractiveControls = new LockedList<InteractiveCommand>(settings.interactiveControlsInternal);
                settings.TimerCommands = new LockedList<TimerCommand>(settings.timerCommandsInternal);
            }
            else
            {
                settings = new ChannelSettings(channel);
            }

            settings.ChatCommands.Add(new UptimeChatCommand());
            settings.ChatCommands.Add(new GameChatCommand());
            settings.ChatCommands.Add(new TitleChatCommand());
            settings.ChatCommands.Add(new TimeoutChatCommand());
            settings.ChatCommands.Add(new PurgeChatCommand());
            settings.ChatCommands.Add(new MixerAgeChatCommand());
            settings.ChatCommands.Add(new FollowAgeChatCommand());

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
        public ChannelModel Channel { get; set; }

        [JsonProperty]
        public List<UserDataViewModel> UserData { get; set; }

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

        public ChannelSettings(ChannelModel channel)
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

            this.UserData = new List<UserDataViewModel>();

            this.ChatCommands = new LockedList<ChatCommand>();
            this.EventCommands = new LockedList<EventCommand>();
            this.InteractiveControls = new LockedList<InteractiveCommand>();
            this.TimerCommands = new LockedList<TimerCommand>();

            this.TimerCommandsInterval = 10;
            this.TimerCommandsMinimumMessages = 10;
        }

        public async Task SaveSettings()
        {
            Directory.CreateDirectory(SettingsDirectoryName);
            string filePath = ChannelSettings.GetSettingsFilePath(this.Channel);

            this.chatCommandsInternal = this.ChatCommands.ToList();
            this.eventCommandsInternal = this.EventCommands.ToList();
            this.interactiveControlsInternal = this.InteractiveControls.ToList();
            this.timerCommandsInternal = this.TimerCommands.ToList();

            this.chatCommandsInternal.RemoveAll(c => c.Actions.Any(a => a.Type == ActionTypeEnum.Custom));

            await SerializerHelper.SerializeToFile(filePath, this);
        }
    }
}
