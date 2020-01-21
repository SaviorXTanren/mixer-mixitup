using Mixer.Base.Model.Channel;
using MixItUp.Base;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
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

        public async Task<IEnumerable<SettingsV2Model>> GetAllSettings()
        {
            if (!Directory.Exists(SettingsDirectoryName))
            {
                Directory.CreateDirectory(SettingsDirectoryName);
            }

            List<SettingsV2Model> settings = new List<SettingsV2Model>();
            foreach (string filePath in Directory.GetFiles(SettingsDirectoryName))
            {
                if (filePath.EndsWith(".xml"))
                {
                    SettingsV2Model setting = null;
                    try
                    {
                        setting = await this.UpgradeSettings(filePath);
                        if (setting != null)
                        {
                            settings.Add(setting);
                            continue;
                        }
                    }
                    catch (Exception ex) { Logger.Log(ex); }

                    string backupFilePath = filePath + DesktopSettingsService.BackupFileExtension;
                    setting = await this.UpgradeSettings(backupFilePath);
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

        public async Task<SettingsV2Model> Create(ExpandedChannelModel channel, bool isStreamer)
        {
            SettingsV2Model settings = new SettingsV2Model(channel, isStreamer);
            if (File.Exists(this.GetFilePath(settings)))
            {
                var tempSettings = await this.UpgradeSettings(this.GetFilePath(settings));
                if (tempSettings == null)
                {
                    GlobalEvents.ShowMessageBox("We were unable to load your settings file due to file corruption. Unfortunately, we could not repair your settings."+ Environment.NewLine + Environment.NewLine + "We apologize for this inconvenience. If you have backups, you can restore them from the settings menu.");
                }
                else
                {
                    settings = tempSettings;
                }
            }

            string databaseFilePath = this.GetDatabaseFilePath(settings);
            if (!File.Exists(databaseFilePath))
            {
                File.Copy(SettingsTemplateDatabaseFileName, databaseFilePath);
            }
            return settings;
        }

        public async Task Initialize(SettingsV2Model settings)
        {
            settings.DatabasePath = this.GetDatabaseFilePath(settings);
            if (!File.Exists(settings.DatabasePath))
            {
                File.Copy(SettingsTemplateDatabaseFileName, settings.DatabasePath, overwrite: true);
            }

            await settings.Initialize();
        }

        public async Task<bool> SaveAndValidate(SettingsV2Model settings)
        {
            try
            {
                await this.Save(settings);
                SettingsV2Model loadedSettings = await this.UpgradeSettings(this.GetFilePath(settings));
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return false;
        }

        public async Task Save(SettingsV2Model settings) { await this.SaveSettings(settings, this.GetFilePath(settings)); }

        public async Task SaveBackup(SettingsV2Model settings)
        {
            string filePath = this.GetFilePath(settings);
            await this.SaveSettings(settings, filePath + DesktopSettingsService.BackupFileExtension);

            File.Copy(settings.DatabasePath, settings.DatabasePath + DesktopSettingsService.BackupFileExtension, overwrite: true);
        }

        public async Task SavePackagedBackup(SettingsV2Model settings, string filePath)
        {
            await this.Save(ChannelSession.Settings);

            string settingsFilePath = this.GetFilePath(settings);

            if (Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                using (ZipArchive zipFile = ZipFile.Open(filePath, ZipArchiveMode.Create))
                {
                    zipFile.CreateEntryFromFile(settingsFilePath, Path.GetFileName(settingsFilePath));
                    zipFile.CreateEntryFromFile(settings.DatabasePath, Path.GetFileName(settings.DatabasePath));
                }
            }
        }

        public async Task PerformBackupIfApplicable(SettingsV2Model settings)
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

        public string GetFilePath(SettingsV2Model settings)
        {
            return Path.Combine(SettingsDirectoryName, string.Format("{0}.{1}.xml", settings.Channel.id.ToString(), (settings.IsStreamer) ? "Streamer" : "Moderator"));
        }

        public async Task ClearAllUserData(SettingsV2Model settings)
        {
            await ChannelSession.Services.Database.Write(settings.DatabasePath, "DELETE FROM Users");
        }

        public async Task SaveSettings(SettingsV2Model settings, string filePath)
        {
            await semaphore.WaitAndRelease(async () =>
            {
                await settings.CopyLatestValues();
                await SerializerHelper.SerializeToFile(filePath, settings);
            });
        }

        public string GetDatabaseFilePath(SettingsV2Model settings)
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
            return SettingsV2Model.LatestVersion;
        }

        private async Task<SettingsV2Model> UpgradeSettings(string filePath)
        {
            int currentVersion = await GetSettingsVersion(filePath);
            if (currentVersion == -1)
            {
                // Settings file is invalid, we can't use this
                return null;
            }
            else if (currentVersion > SettingsV2Model.LatestVersion)
            {
                // Future build, like a preview build, we can't load this
                return null;
            }
            else if (currentVersion < SettingsV2Model.LatestVersion)
            {
                await DesktopSettingsUpgrader.UpgradeSettingsToLatest(currentVersion, filePath);
            }

            return await SerializerHelper.DeserializeFromFile<SettingsV2Model>(filePath);
        }
    }
}
