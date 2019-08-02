using Mixer.Base.Model.Channel;
using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class DesktopSettingsService : ISettingsService
    {
        public const string SettingsDirectoryName = "Settings";
        public const string SettingsTemplateDatabaseFileName = "SettingsTemplateDatabase.sqlite";

        private const string BackupFileExtension = ".backup";

        private static SemaphoreSlim semaphore = new SemaphoreSlim(1);

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
                    IChannelSettings setting = null;
                    try
                    {
                        setting = await this.LoadSettings(filePath);
                        if (setting != null)
                        {
                            settings.Add(setting);
                            continue;
                        }
                    }
                    catch (Exception ex) { Logger.Log(ex); }

                    string backupFilePath = filePath + DesktopSettingsService.BackupFileExtension;
                    setting = await this.LoadSettings(backupFilePath);
                    if (setting != null)
                    {
                        settings.Add(setting);
                        GlobalEvents.ShowMessageBox("We were unable to load your settings file due to file corruption and will instead load your backup. This means that your most recent changes from the last time you ran Mix It Up will not be present." + Environment.NewLine + Environment.NewLine + "We apologize for this inconvenience and have already recorded this issue to help prevent this from happening in the future.");
                    }
                }
            }
            return settings;
        }

        public void Initialize() { Directory.CreateDirectory(SettingsDirectoryName); }

        public async Task<IChannelSettings> Create(ExpandedChannelModel channel, bool isStreamer)
        {
            IChannelSettings settings = new DesktopChannelSettings(channel, isStreamer);
            if (File.Exists(this.GetFilePath(settings)))
            {
                settings = await this.LoadSettings(this.GetFilePath(settings));
            }

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
            await this.SaveSettings(settings, filePath + DesktopSettingsService.BackupFileExtension);

            DesktopChannelSettings desktopSettings = (DesktopChannelSettings)settings;
            File.Copy(desktopSettings.DatabasePath, desktopSettings.DatabasePath + DesktopSettingsService.BackupFileExtension, overwrite: true);
        }

        public async Task SavePackagedBackup(IChannelSettings settings, string filePath)
        {
            await this.Save(ChannelSession.Settings);

            string settingsFilePath = this.GetFilePath(settings);
            DesktopChannelSettings desktopSettings = (DesktopChannelSettings)settings;

            if (Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                using (ZipArchive zipFile = ZipFile.Open(filePath, ZipArchiveMode.Create))
                {
                    zipFile.CreateEntryFromFile(settingsFilePath, Path.GetFileName(settingsFilePath));
                    zipFile.CreateEntryFromFile(desktopSettings.DatabasePath, Path.GetFileName(desktopSettings.DatabasePath));
                }
            }
        }

        public async Task PerformBackupIfApplicable(IChannelSettings settings)
        {
            if (settings.SettingsBackupRate != SettingsBackupRateEnum.None && !string.IsNullOrEmpty(settings.SettingsBackupLocation))
            {
                DateTimeOffset newResetDate = settings.SettingsLastBackup;

                if (settings.SettingsBackupRate == SettingsBackupRateEnum.Daily) { newResetDate = newResetDate.AddDays(1); }
                else if (settings.SettingsBackupRate == SettingsBackupRateEnum.Weekly) { newResetDate = newResetDate.AddDays(7); }
                else if (settings.SettingsBackupRate == SettingsBackupRateEnum.Monthly) { newResetDate = newResetDate.AddMonths(1); }

                if (newResetDate < DateTimeOffset.Now)
                {
                    string filePath = Path.Combine(settings.SettingsBackupLocation, settings.Channel.id + "-Backup-" + DateTimeOffset.Now.ToString("MM-dd-yyyy") + ".mixitup");

                    await this.SavePackagedBackup(settings, filePath);

                    settings.SettingsLastBackup = DateTimeOffset.Now;
                }
            }
        }

        public string GetFilePath(IChannelSettings settings)
        {
            return Path.Combine(SettingsDirectoryName, string.Format("{0}.{1}.xml", settings.Channel.id.ToString(), (settings.IsStreamer) ? "Streamer" : "Moderator"));
        }

        public async Task ClearAllUserData(IChannelSettings settings)
        {
            DesktopChannelSettings desktopSettings = (DesktopChannelSettings)settings;
            await desktopSettings.DatabaseWrapper.RunWriteCommand("DELETE FROM Users");
        }

        public async Task SaveSettings(IChannelSettings settings, string filePath)
        {
            await semaphore.WaitAndRelease(async () =>
            {
                DesktopChannelSettings desktopSettings = (DesktopChannelSettings)settings;
                await desktopSettings.CopyLatestValues();
                await SerializerHelper.SerializeToFile(filePath, desktopSettings);
            });
        }

        public string GetDatabaseFilePath(IChannelSettings settings)
        {
            return Path.Combine(SettingsDirectoryName, string.Format("{0}.{1}.sqlite", settings.Channel.id.ToString(), (settings.IsStreamer) ? "Streamer" : "Moderator"));
        }

        public async Task<int> GetSettingsVersion(string filePath)
        {
            string fileData = await ChannelSession.Services.FileService.ReadFile(filePath);
            if (string.IsNullOrEmpty(fileData))
            {
                return -1;
            }

            JObject settingsJObj = JObject.Parse(fileData);
            return (int)settingsJObj["Version"];
        }

        public int GetLatestVersion()
        {
            return DesktopChannelSettings.LatestVersion;
        }

        private async Task<IChannelSettings> LoadSettings(string filePath)
        {
            int currentVersion = await GetSettingsVersion(filePath);
            if (currentVersion < DesktopChannelSettings.LatestVersion)
            {
                await DesktopSettingsUpgrader.UpgradeSettingsToLatest(currentVersion, filePath);
            }
            else if (currentVersion > DesktopChannelSettings.LatestVersion)
            {
                // Future build, like a preview build, we can't load this
                return null;
            }

            return await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
        }
    }
}
