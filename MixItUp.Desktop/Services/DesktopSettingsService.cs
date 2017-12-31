using Mixer.Base.Clients;
using Mixer.Base.Model.Channel;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Interactive;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    internal static class DesktopSettingsUpgrader
    {
        internal static async Task UpgradeSettingsToLatest(int version, string filePath)
        {
            await DesktopSettingsUpgrader.Version1Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version2Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version3Upgrade(version, filePath);

            DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
            await ChannelSession.Services.Settings.Initialize(settings);
            settings.Version = DesktopChannelSettings.LatestVersion;

            await ChannelSession.Services.Settings.Save(settings);
        }

        private static async Task Version1Upgrade(int version, string filePath)
        {
            if (version < 1)
            {
                string data = File.ReadAllText(filePath);
                data = data.Replace("MixItUp.Base.ChannelSettings, MixItUp.Base", "MixItUp.Desktop.DesktopChannelSettings, MixItUp.Desktop");
                data = data.Replace("MixItUp.Base.ViewModel.UserDataViewModel", "MixItUp.Base.ViewModel.User.UserDataViewModel");
                data = data.Replace("MixItUp.Base.ViewModel.User.UserViewModel", "MixItUp.Base.ViewModel.User.UserDataViewModel");
                File.WriteAllText(filePath, data);

                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
                await ChannelSession.Services.Settings.Initialize(settings);

                data = data.Replace("MixItUp.Desktop.DesktopChannelSettings, MixItUp.Desktop", "MixItUp.Desktop.LegacyDesktopChannelSettings, MixItUp.Desktop");
                LegacyDesktopChannelSettings legacySettings = SerializerHelper.DeserializeFromString<LegacyDesktopChannelSettings>(data);
                await ChannelSession.Services.Settings.Initialize(legacySettings);

                foreach (UserDataViewModel userData in legacySettings.userDataInternal)
                {
                    settings.UserData.Add(userData.ID, userData);
                }

                settings.CurrencyAcquisition.Name = legacySettings.CurrencyName;
                settings.CurrencyAcquisition.AcquireAmount = legacySettings.CurrencyAcquireAmount;
                settings.CurrencyAcquisition.AcquireInterval = legacySettings.CurrencyAcquireInterval;
                settings.CurrencyAcquisition.Enabled = legacySettings.CurrencyEnabled;
                settings.InteractiveUserGroups = new LockedDictionary<uint, List<InteractiveUserGroupViewModel>>(legacySettings.InteractiveUserGroups);
                settings.InteractiveCooldownGroups = new LockedDictionary<string, int>(legacySettings.InteractiveCooldownGroups);

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version2Upgrade(int version, string filePath)
        {
            if (version < 2)
            {
                string data = File.ReadAllText(filePath);
                data = data.Replace("interactiveControlsInternal", "interactiveCommandsInternal");
                data = data.Replace("CapsBlockCount", "ModerationCapsBlockCount");
                data = data.Replace("PunctuationBlockCount", "ModerationPunctuationBlockCount");
                data = data.Replace("EmoteBlockCount", "ModerationEmoteBlockCount");
                data = data.Replace("BlockLinks", "ModerationBlockLinks");
                data = data.Replace("Timeout1MinuteOffenseCount", "ModerationTimeout1MinuteOffenseCount");
                data = data.Replace("Timeout5MinuteOffenseCount", "ModerationTimeout5MinuteOffenseCount");
                File.WriteAllText(filePath, data);

                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
                await ChannelSession.Services.Settings.Initialize(settings);

                List<CommandBase> commands = new List<CommandBase>();
                commands.AddRange(settings.ChatCommands);
                commands.AddRange(settings.InteractiveCommands);
                commands.AddRange(settings.EventCommands);
                commands.AddRange(settings.TimerCommands);

                foreach (CommandBase command in commands)
                {
                    foreach (ActionBase action in command.Actions)
                    {
                        if (action is ChatAction)
                        {
                            ChatAction nAction = (ChatAction)action;
                            nAction.ChatText = DesktopSettingsUpgrader.ReplaceSpecialIdentifiers(nAction.ChatText);
                        }
                        else if (action is CurrencyAction)
                        {
                            CurrencyAction nAction = (CurrencyAction)action;
                            nAction.ChatText = DesktopSettingsUpgrader.ReplaceSpecialIdentifiers(nAction.ChatText);
                        }
                        else if (action is OBSStudioAction)
                        {
                            OBSStudioAction nAction = (OBSStudioAction)action;
                            nAction.SourceText = DesktopSettingsUpgrader.ReplaceSpecialIdentifiers(nAction.SourceText);
                        }
                        else if (action is OverlayAction)
                        {
                            OverlayAction nAction = (OverlayAction)action;
                            nAction.ImagePath = DesktopSettingsUpgrader.ReplaceSpecialIdentifiers(nAction.ImagePath);
                            nAction.Text = DesktopSettingsUpgrader.ReplaceSpecialIdentifiers(nAction.Text);
                        }
                        else if (action is TextToSpeechAction)
                        {
                            TextToSpeechAction nAction = (TextToSpeechAction)action;
                            nAction.SpeechText = DesktopSettingsUpgrader.ReplaceSpecialIdentifiers(nAction.SpeechText);
                        }
                        else if (action is XSplitAction)
                        {
                            XSplitAction nAction = (XSplitAction)action;
                            nAction.SourceText = DesktopSettingsUpgrader.ReplaceSpecialIdentifiers(nAction.SourceText);
                        }
                    }
                }

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version3Upgrade(int version, string filePath)
        {
            if (version < 3)
            {
                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
                await ChannelSession.Services.Settings.Initialize(settings);

                IEnumerable<EventCommand> commands = settings.EventCommands.Where(c => c.Actions.Count == 0);
                foreach (EventCommand command in commands)
                {
                    settings.EventCommands.Remove(command);
                }

                if (settings.RankChangedCommand != null && settings.RankChangedCommand.Actions.Count == 0)
                {
                    settings.RankChangedCommand = null;
                }

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static string ReplaceSpecialIdentifiers(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                text = Regex.Replace(text, "\\$user(?!(currency|rankname|rankpoints|rank|time|avatar|url))", "$username");
                text = Regex.Replace(text, "\\$arg1user(?!(currency|rankname|rankpoints|rank|time|avatar|url))", "$arg1username");
                text = Regex.Replace(text, "\\$arg1(?!user)", "$arg1string");
            }
            return text;
        }
    }

    public class DesktopSettingsService : ISettingsService
    {
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1);

        private const string SettingsDirectoryName = "Settings";
        private const string SettingsTemplateDatabaseFileName = "SettingsTemplateDatabase.sqlite";

        public async Task<IEnumerable<IChannelSettings>> GetAllSettings()
        {
            if (!Directory.Exists(SettingsDirectoryName))
            {
                Directory.CreateDirectory(SettingsDirectoryName);
            }

            List<IChannelSettings> settings = new List<IChannelSettings>();
            foreach (string filePath in Directory.GetFiles(SettingsDirectoryName))
            {
                if (filePath.EndsWith(".xml"))
                {
                    IChannelSettings setting = await this.LoadSettings(filePath);
                    if (setting != null)
                    {
                        settings.Add(setting);
                    }
                }
            }
            return settings;
        }

        public void Initialize() { Directory.CreateDirectory(SettingsDirectoryName); }

        public IChannelSettings Create(ExpandedChannelModel channel, bool isStreamer)
        {
            IChannelSettings settings = new DesktopChannelSettings(channel, isStreamer);
            string databaseFilePath = this.GetDatabaseFilePath(settings);
            if (!File.Exists(databaseFilePath))
            {
                File.Copy(SettingsTemplateDatabaseFileName, databaseFilePath);
            }
            return settings;
        }

        public async Task Initialize(IChannelSettings settings)
        {
            DesktopChannelSettings desktopSettings = (DesktopChannelSettings)settings;

            desktopSettings.DatabasePath = this.GetDatabaseFilePath(desktopSettings);
            if (!File.Exists(desktopSettings.DatabasePath))
            {
                File.Copy(SettingsTemplateDatabaseFileName, desktopSettings.DatabasePath, overwrite: true);
            }

            await desktopSettings.Initialize();
        }

        public async Task<bool> SaveAndValidate(IChannelSettings settings)
        {
            try
            {
                await this.Save(settings);
                IChannelSettings loadedSettings = await this.LoadSettings(this.GetFilePath(settings));
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return false;
        }

        public async Task Save(IChannelSettings settings) { await this.SaveSettings(settings, this.GetFilePath(settings)); }

        public async Task Save(IChannelSettings settings, string fileName) { await this.SaveSettings(settings, fileName); }

        public async Task SaveBackup(IChannelSettings settings)
        {
            string filePath = this.GetFilePath(settings);
            await this.SaveSettings(settings, filePath + ".backup");

            DesktopChannelSettings desktopSettings = (DesktopChannelSettings)settings;
            File.Copy(desktopSettings.DatabasePath, desktopSettings.DatabasePath + ".backup", overwrite: true);
        }

        public string GetFilePath(IChannelSettings settings)
        {
            return Path.Combine(SettingsDirectoryName, string.Format("{0}.{1}.xml", settings.Channel.id.ToString(), (settings.IsStreamer) ? "Streamer" : "Moderator"));
        }

        private async Task<IChannelSettings> LoadSettings(string filePath)
        {
            string data = File.ReadAllText(filePath);
            if (!data.Contains("\"Version\":"))
            {
                await DesktopSettingsUpgrader.UpgradeSettingsToLatest(0, filePath);
            }

            DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);

            if (settings.ShouldBeUpgraded())
            {
                await DesktopSettingsUpgrader.UpgradeSettingsToLatest(settings.Version, filePath);
                settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
            }
            return settings;
        }

        public async Task SaveSettings(IChannelSettings settings, string filePath)
        {
            try
            {
                await semaphore.WaitAsync();

                DesktopChannelSettings desktopSettings = (DesktopChannelSettings)settings;
                await desktopSettings.CopyLatestValues();
                await SerializerHelper.SerializeToFile(filePath, desktopSettings);
            }
            catch (Exception ex) { Logger.Log(ex); }
            finally { semaphore.Release(); }
        }

        public string GetDatabaseFilePath(IChannelSettings settings)
        {
            return Path.Combine(SettingsDirectoryName, string.Format("{0}.{1}.sqlite", settings.Channel.id.ToString(), (settings.IsStreamer) ? "Streamer" : "Moderator"));
        }
    }
}
