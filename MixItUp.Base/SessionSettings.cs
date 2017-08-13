using Mixer.Base.Model.Channel;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base
{
    [DataContract]
    public class SessionSettings
    {
        private const string SettingsDirectoryName = "Settings";

        public static async Task<SessionSettings> LoadSettings(ChannelModel channel)
        {
            string filePath = SessionSettings.GetSettingsFilePath(channel);
            if (File.Exists(filePath))
            {
                SessionSettings settings = await SerializerHelper.DeserializeFromFile<SessionSettings>(filePath);

                settings.Channel = channel;

                foreach (CommandBase command in settings.ChatCommands) { command.DeserializeActions(); }
                foreach (CommandBase command in settings.InteractiveCommands) { command.DeserializeActions(); }
                foreach (CommandBase command in settings.EventCommands) { command.DeserializeActions(); }
                foreach (CommandBase command in settings.TimerCommands) { command.DeserializeActions(); }

                return settings;
            }
            else
            {
                return new SessionSettings(channel);
            }
        }

        private static string GetSettingsFilePath(ChannelModel channel) { return Path.Combine(SettingsDirectoryName, string.Format("{0}.xml", channel.id.ToString())); }

        public SessionSettings(ChannelModel channel) : this() { this.Channel = channel; }

        public SessionSettings()
        {
            this.SubscribedEvents = new List<SubscribedEventViewModel>();
            this.UserData = new List<UserDataViewModel>();
            this.ChatCommands = new List<ChatCommand>();
            this.InteractiveCommands = new List<InteractiveCommand>();
            this.EventCommands = new List<EventCommand>();
            this.TimerCommands = new List<TimerCommand>();
        }

        [DataMember]
        public ChannelModel Channel { get; set; }

        [DataMember]
        public List<SubscribedEventViewModel> SubscribedEvents { get; set; }

        [DataMember]
        public List<UserDataViewModel> UserData { get; set; }

        [DataMember]
        public List<ChatCommand> ChatCommands { get; set; }

        [DataMember]
        public List<InteractiveCommand> InteractiveCommands { get; set; }

        [DataMember]
        public List<EventCommand> EventCommands { get; set; }

        [DataMember]
        public List<TimerCommand> TimerCommands { get; set; }

        public async Task SaveSettings()
        {
            Directory.CreateDirectory(SettingsDirectoryName);

            foreach (CommandBase command in this.ChatCommands) { command.SerializeActions(); }
            foreach (CommandBase command in this.InteractiveCommands) { command.SerializeActions(); }
            foreach (CommandBase command in this.EventCommands) { command.SerializeActions(); }
            foreach (CommandBase command in this.TimerCommands) { command.SerializeActions(); }

            string filePath = SessionSettings.GetSettingsFilePath(this.Channel);
            await SerializerHelper.SerializeToFile(filePath, this);
        }
    }
}
