using Mixer.Base.Model.Channel;
using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Interactive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class WindowsSettingsService : ISettingsService
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

        public void Initialize(IChannelSettings settings)
        {
            DesktopChannelSettings desktopSettings = (DesktopChannelSettings)settings;
            desktopSettings.Initialize();
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
                await this.UpgradeSettingsToLatest(0, filePath);
            }

            DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);

            if (settings.ShouldBeUpgraded())
            {
                await this.UpgradeSettingsToLatest(settings.Version, filePath);
                settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
            }
            return settings;
        }

        public async Task SaveSettings(IChannelSettings settings, string filePath)
        {
            await semaphore.WaitAsync();

            DesktopChannelSettings desktopSettings = (DesktopChannelSettings)settings;

            desktopSettings.CopyLatestValues();

            await SerializerHelper.SerializeToFile(filePath, desktopSettings);

            semaphore.Release();
        }

        private async Task UpgradeSettingsToLatest(int version, string filePath)
        {
            DesktopChannelSettings settings = null;

            if (version < 1)
            {
                string data = File.ReadAllText(filePath);
                data = data.Replace("MixItUp.Base.ChannelSettings, MixItUp.Base", "MixItUp.Desktop.DesktopChannelSettings, MixItUp.Desktop");
                data = data.Replace("MixItUp.Base.ViewModel.UserDataViewModel", "MixItUp.Base.ViewModel.User.UserDataViewModel");
                data = data.Replace("MixItUp.Base.ViewModel.User.UserDataViewModel", "MixItUp.Base.ViewModel.User.UserViewModel");
                File.WriteAllText(filePath, data);

                settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
                this.Initialize(settings);

                data = data.Replace("MixItUp.Desktop.DesktopChannelSettings, MixItUp.Desktop", "MixItUp.Desktop.LegacyDesktopChannelSettings, MixItUp.Desktop");
                LegacyDesktopChannelSettings legacySettings = SerializerHelper.DeserializeFromString<LegacyDesktopChannelSettings>(data);
                this.Initialize(legacySettings);

                settings.CurrencyAcquisition.Name = legacySettings.CurrencyName;
                settings.CurrencyAcquisition.AcquireAmount = legacySettings.CurrencyAcquireAmount;
                settings.CurrencyAcquisition.AcquireInterval = legacySettings.CurrencyAcquireInterval;
                settings.CurrencyAcquisition.Enabled = legacySettings.CurrencyEnabled;
                settings.InteractiveUserGroups = new LockedDictionary<uint, List<InteractiveUserGroupViewModel>>(legacySettings.InteractiveUserGroups);
                settings.InteractiveCooldownGroups = new LockedDictionary<string, int>(legacySettings.InteractiveCooldownGroups);

                await this.Save(settings);

                string databaseFilePath = this.GetDatabaseFilePath(settings);
                if (!File.Exists(databaseFilePath))
                {
                    File.Copy(SettingsTemplateDatabaseFileName, databaseFilePath);
                }
            }

            settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
            this.Initialize(settings);
            settings.Version = DesktopChannelSettings.LatestVersion;

            await this.Save(settings);
        }

        public string GetDatabaseFilePath(IChannelSettings settings)
        {
            return Path.Combine(SettingsDirectoryName, string.Format("{0}.{1}.sqlite", settings.Channel.id.ToString(), (settings.IsStreamer) ? "Streamer" : "Moderator"));
        }
    }
}
