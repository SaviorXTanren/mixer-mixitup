using Mixer.Base.Model.Channel;
using MixItUp.Base;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
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
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public void Initialize() { Directory.CreateDirectory(SettingsV2Model.SettingsDirectoryName); }

        public async Task<IEnumerable<SettingsV2Model>> GetAllSettings()
        {
            // Check for old V1 settings
#pragma warning disable CS0612 // Type or member is obsolete
            List<SettingsV1Model> oldSettings = new List<SettingsV1Model>();
            foreach (string filePath in Directory.GetFiles(SettingsV2Model.SettingsDirectoryName))
            {
                if (filePath.EndsWith(SettingsV1Model.SettingsFileExtension))
                {
                    try
                    {
                        SettingsV1Model setting = await SettingsV1Upgrader.UpgradeSettingsToLatest(filePath);

                        string oldSettingsPath = Path.Combine(SettingsV2Model.SettingsDirectoryName, "Old");
                        Directory.CreateDirectory(oldSettingsPath);

                        await ChannelSession.Services.FileService.CopyFile(filePath, Path.Combine(oldSettingsPath, Path.GetFileName(filePath)));
                        await ChannelSession.Services.FileService.CopyFile(Path.Combine(SettingsV2Model.SettingsDirectoryName, setting.DatabaseFileName), Path.Combine(oldSettingsPath, setting.DatabaseFileName));

                        await ChannelSession.Services.FileService.DeleteFile(filePath);
                        await ChannelSession.Services.FileService.DeleteFile(Path.Combine(SettingsV2Model.SettingsDirectoryName, setting.DatabaseFileName));
                        await ChannelSession.Services.FileService.DeleteFile(filePath + ".backup");
                        await ChannelSession.Services.FileService.DeleteFile(Path.Combine(SettingsV2Model.SettingsDirectoryName, setting.DatabaseFileName) + ".backup");
                    }
                    catch (Exception ex) { Logger.Log(ex); }
                }
            }
#pragma warning restore CS0612 // Type or member is obsolete

            List<SettingsV2Model> settings = new List<SettingsV2Model>();
            foreach (string filePath in Directory.GetFiles(SettingsV2Model.SettingsDirectoryName))
            {
                if (filePath.EndsWith(SettingsV2Model.SettingsFileExtension))
                {
                    SettingsV2Model setting = null;
                    try
                    {
                        setting = await this.LoadSettings(filePath);
                        if (setting != null)
                        {
                            settings.Add(setting);
                        }
                    }
                    catch (Exception ex) { Logger.Log(ex); }
                }
            }
            return settings;
        }

        public Task<SettingsV2Model> Create(ExpandedChannelModel channel, bool isStreamer)
        {
            SettingsV2Model settings = new SettingsV2Model(channel, isStreamer);
            return Task.FromResult(settings);
        }

        public async Task Initialize(SettingsV2Model settings)
        {
            await settings.Initialize();
        }

        public async Task<bool> SaveAndValidate(SettingsV2Model settings)
        {
            try
            {
                await this.Save(settings);
                SettingsV2Model loadedSettings = await this.LoadSettings(settings.SettingsFilePath);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return false;
        }

        public async Task Save(SettingsV2Model settings)
        {
            await semaphore.WaitAndRelease(async () =>
            {
                settings.CopyLatestValues();
                await SerializerHelper.SerializeToFile(settings.SettingsFilePath, settings);
                await settings.SaveDatabaseData();
            });
        }

        public async Task SavePackagedBackup(SettingsV2Model settings, string filePath)
        {
            await this.Save(ChannelSession.Settings);

            if (Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                using (ZipArchive zipFile = ZipFile.Open(filePath, ZipArchiveMode.Create))
                {
                    zipFile.CreateEntryFromFile(settings.SettingsFilePath, Path.GetFileName(settings.SettingsFilePath));
                    zipFile.CreateEntryFromFile(settings.DatabaseFilePath, Path.GetFileName(settings.DatabaseFilePath));
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
                    string filePath = Path.Combine(settings.SettingsBackupLocation, settings.MixerChannelID + "-Backup-" + DateTimeOffset.Now.ToString("MM-dd-yyyy") + "." + SettingsV2Model.SettingsBackupFileExtension);

                    await this.SavePackagedBackup(settings, filePath);

                    settings.SettingsLastBackup = DateTimeOffset.Now;
                }
            }
        }

        private async Task<SettingsV2Model> LoadSettings(string filePath)
        {
            return await SettingsV2Upgrader.UpgradeSettingsToLatest(filePath);
        }
    }
}
