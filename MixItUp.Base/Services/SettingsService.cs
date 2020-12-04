using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Currency;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public enum SettingsBackupRateEnum
    {
        None = 0,
        Daily,
        Weekly,
        Monthly,
    }

    public interface ISettingsService
    {
        void Initialize();

        Task<IEnumerable<SettingsV3Model>> GetAllSettings();

        Task<SettingsV3Model> Create(string name, bool isStreamer);

        Task Initialize(SettingsV3Model settings);

#pragma warning disable CS0612 // Type or member is obsolete
        Task Save(SettingsV2Model settings);
#pragma warning restore CS0612 // Type or member is obsolete

        Task Save(SettingsV3Model settings);

        Task SaveLocalBackup(SettingsV3Model settings);

        Task SavePackagedBackup(SettingsV3Model settings, string filePath);

        Task<Result<SettingsV3Model>> RestorePackagedBackup(string filePath);

        Task PerformAutomaticBackupIfApplicable(SettingsV3Model settings);
    }

    public class SettingsService : ISettingsService
    {
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public void Initialize() { Directory.CreateDirectory(SettingsV3Model.SettingsDirectoryName); }

        public async Task<IEnumerable<SettingsV3Model>> GetAllSettings()
        {
            bool backupSettingsLoaded = false;
            bool settingsLoadFailure = false;

#pragma warning disable CS0612 // Type or member is obsolete
            foreach (string filePath in Directory.GetFiles(SettingsV2Model.SettingsDirectoryName))
            {
                if (filePath.EndsWith(SettingsV2Model.SettingsFileExtension))
                {
                    SettingsV2Model setting = null;
                    try
                    {
                        setting = await SettingsV2Upgrader.UpgradeSettingsToLatest(filePath);
                        if (setting != null)
                        {
                            await SettingsV3Upgrader.UpgradeV2ToV3(filePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                    }
                }
            }
#pragma warning restore CS0612 // Type or member is obsolete

            List<SettingsV3Model> allSettings = new List<SettingsV3Model>();
            foreach (string filePath in Directory.GetFiles(SettingsV3Model.SettingsDirectoryName))
            {
                if (filePath.EndsWith(SettingsV3Model.SettingsFileExtension))
                {
                    SettingsV3Model setting = null;
                    try
                    {
                        setting = await this.LoadSettings(filePath);
                        if (setting != null)
                        {
                            allSettings.Add(setting);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                    }

                    if (setting == null)
                    {
                        string localBackupFilePath = string.Format($"{filePath}.{SettingsV3Model.SettingsLocalBackupFileExtension}");
                        if (File.Exists(localBackupFilePath))
                        {
                            try
                            {
                                setting = await this.LoadSettings(localBackupFilePath);
                                if (setting != null)
                                {
                                    allSettings.Add(setting);
                                    backupSettingsLoaded = true;
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Log(ex);
                            }
                        }
                    }

                    if (setting == null)
                    {
                        settingsLoadFailure = true;
                    }
                }
            }

            if (!string.IsNullOrEmpty(ChannelSession.AppSettings.BackupSettingsFilePath) && ChannelSession.AppSettings.BackupSettingsToReplace != Guid.Empty)
            {
                Logger.Log(LogLevel.Debug, "Restored settings file detected, starting restore process");

                SettingsV3Model settings = allSettings.FirstOrDefault(s => s.ID.Equals(ChannelSession.AppSettings.BackupSettingsToReplace));
                if (settings != null)
                {
                    File.Delete(settings.SettingsFilePath);
                    File.Delete(settings.DatabaseFilePath);

                    using (ZipArchive zipFile = ZipFile.Open(ChannelSession.AppSettings.BackupSettingsFilePath, ZipArchiveMode.Read))
                    {
                        zipFile.ExtractToDirectory(SettingsV3Model.SettingsDirectoryName);
                    }

                    ChannelSession.AppSettings.BackupSettingsFilePath = null;
                    ChannelSession.AppSettings.BackupSettingsToReplace = Guid.Empty;

                    return await this.GetAllSettings();
                }
            }
            else if (ChannelSession.AppSettings.SettingsToDelete != Guid.Empty)
            {
                Logger.Log(LogLevel.Debug, "Settings deletion detected, starting deletion process");

                SettingsV3Model settings = allSettings.FirstOrDefault(s => s.ID.Equals(ChannelSession.AppSettings.SettingsToDelete));
                if (settings != null)
                {
                    File.Delete(settings.SettingsFilePath);
                    File.Delete(settings.DatabaseFilePath);

                    ChannelSession.AppSettings.SettingsToDelete = Guid.Empty;

                    return await this.GetAllSettings();
                }
            }

            if (backupSettingsLoaded)
            {
                await DialogHelper.ShowMessage("One or more of the settings file could not be loaded due to file corruption and the most recent local backup was loaded instead.");
            }
            if (settingsLoadFailure)
            {
                await DialogHelper.ShowMessage("One or more settings files were unable to be loaded. Please visit the Mix It Up discord for assistance on this issue.");
            }

            return allSettings;
        }

        public Task<SettingsV3Model> Create(string name, bool isStreamer)
        {
            return Task.FromResult(new SettingsV3Model(name, isStreamer));
        }

        public async Task Initialize(SettingsV3Model settings)
        {
            await settings.Initialize();
        }

#pragma warning disable CS0612 // Type or member is obsolete
        public async Task Save(SettingsV2Model settings)
        {
            Logger.Log(LogLevel.Debug, "Settings save operation started");

            await semaphore.WaitAndRelease(async () =>
            {
                settings.CopyLatestValues();
                await FileSerializerHelper.SerializeToFile(settings.SettingsFilePath, settings);
                await settings.SaveDatabaseData();
            });

            Logger.Log(LogLevel.Debug, "Settings save operation finished");
        }
#pragma warning restore CS0612 // Type or member is obsolete

        public async Task Save(SettingsV3Model settings)
        {
            Logger.Log(LogLevel.Debug, "Settings save operation started");

            await semaphore.WaitAndRelease(async () =>
            {
                settings.CopyLatestValues();
                await FileSerializerHelper.SerializeToFile(settings.SettingsFilePath, settings);
                await settings.SaveDatabaseData();
            });

            Logger.Log(LogLevel.Debug, "Settings save operation finished");
        }

        public async Task SaveLocalBackup(SettingsV3Model settings)
        {
            Logger.Log(LogLevel.Debug, "Settings local backup save operation started");

            await semaphore.WaitAndRelease(async () =>
            {
                await FileSerializerHelper.SerializeToFile(settings.SettingsLocalBackupFilePath, settings);
            });

            Logger.Log(LogLevel.Debug, "Settings local backup save operation finished");
        }

        public async Task SavePackagedBackup(SettingsV3Model settings, string filePath)
        {
            await this.Save(ChannelSession.Settings);

            try
            {
                if (Directory.Exists(Path.GetDirectoryName(filePath)))
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    using (ZipArchive zipFile = ZipFile.Open(filePath, ZipArchiveMode.Create))
                    {
                        zipFile.CreateEntryFromFile(settings.SettingsFilePath, Path.GetFileName(settings.SettingsFilePath));
                        if (settings.IsStreamer)
                        {
                            zipFile.CreateEntryFromFile(settings.DatabaseFilePath, Path.GetFileName(settings.DatabaseFilePath));
                        }
                    }
                }
                else
                {
                    Logger.Log(LogLevel.Error, $"Directory does not exist for saving packaged backup: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public async Task<Result<SettingsV3Model>> RestorePackagedBackup(string filePath)
        {
            try
            {
                string tempFilePath = ChannelSession.Services.FileService.GetTempFolder();
                string tempFolder = Path.GetDirectoryName(tempFilePath);

                string settingsFile = null;
                string databaseFile = null;

                try
                {
                    using (ZipArchive zipFile = ZipFile.Open(filePath, ZipArchiveMode.Read))
                    {
                        foreach (ZipArchiveEntry entry in zipFile.Entries)
                        {
                            string extractedFilePath = Path.Combine(tempFolder, entry.Name);
                            if (File.Exists(extractedFilePath))
                            {
                                File.Delete(extractedFilePath);
                            }

                            if (extractedFilePath.EndsWith(SettingsV3Model.SettingsFileExtension, StringComparison.InvariantCultureIgnoreCase))
                            {
                                settingsFile = extractedFilePath;
                            }
                            else if (extractedFilePath.EndsWith(SettingsV3Model.DatabaseFileExtension, StringComparison.InvariantCultureIgnoreCase))
                            {
                                databaseFile = extractedFilePath;
                            }
                        }
                        zipFile.ExtractToDirectory(tempFolder);
                    }
                }
                catch (Exception ex) { Logger.Log(ex); }

                int currentVersion = -1;
                if (!string.IsNullOrEmpty(settingsFile))
                {
                    currentVersion = await SettingsV2Upgrader.GetSettingsVersion(settingsFile);
                }

                if (currentVersion == -1)
                {
                    return new Result<SettingsV3Model>("The backup file selected does not appear to contain Mix It Up settings.");
                }

                if (currentVersion > SettingsV3Model.LatestVersion)
                {
                    return new Result<SettingsV3Model>("The backup file is valid, but is from a newer version of Mix It Up.  Be sure to upgrade to the latest version." +
                        Environment.NewLine + Environment.NewLine + "NOTE: This may require you to opt-in to the Preview build from the General tab in Settings if this was made in a Preview build.");
                }

                return new Result<SettingsV3Model>(await FileSerializerHelper.DeserializeFromFile<SettingsV3Model>(settingsFile));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result<SettingsV3Model>(ex);
            }
        }

        public async Task PerformAutomaticBackupIfApplicable(SettingsV3Model settings)
        {
            if (settings.SettingsBackupRate != SettingsBackupRateEnum.None)
            {
                Logger.Log(LogLevel.Debug, "Checking whether to perform automatic backup");

                DateTimeOffset newResetDate = settings.SettingsLastBackup;

                if (settings.SettingsBackupRate == SettingsBackupRateEnum.Daily) { newResetDate = newResetDate.AddDays(1); }
                else if (settings.SettingsBackupRate == SettingsBackupRateEnum.Weekly) { newResetDate = newResetDate.AddDays(7); }
                else if (settings.SettingsBackupRate == SettingsBackupRateEnum.Monthly) { newResetDate = newResetDate.AddMonths(1); }

                if (newResetDate < DateTimeOffset.Now)
                {
                    string backupPath = Path.Combine(SettingsV3Model.SettingsDirectoryName, SettingsV3Model.DefaultAutomaticBackupSettingsDirectoryName);
                    if (!string.IsNullOrEmpty(settings.SettingsBackupLocation))
                    {
                        backupPath = settings.SettingsBackupLocation;
                    }

                    if (!Directory.Exists(backupPath))
                    {
                        try
                        {
                            Directory.CreateDirectory(backupPath);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(LogLevel.Error, "Failed to create automatic backup directory");
                            Logger.Log(ex);
                            return;
                        }
                    }

                    string filePath = Path.Combine(backupPath, settings.Name + "-Backup-" + DateTimeOffset.Now.ToString("MM-dd-yyyy") + "." + SettingsV3Model.SettingsBackupFileExtension);

                    await this.SavePackagedBackup(settings, filePath);

                    settings.SettingsLastBackup = DateTimeOffset.Now;
                }
            }
        }

        private async Task<SettingsV3Model> LoadSettings(string filePath)
        {
            return await SettingsV3Upgrader.UpgradeSettingsToLatest(filePath);
        }
    }

    public static class SettingsV3Upgrader
    {
#pragma warning disable CS0612 // Type or member is obsolete
        public static async Task UpgradeV2ToV3(string filePath)
        {
            SettingsV2Model oldSettings = await FileSerializerHelper.DeserializeFromFile<SettingsV2Model>(filePath, ignoreErrors: true);
            SettingsV3Model newSettings = new SettingsV3Model();

            await oldSettings.Initialize();








            foreach (var kvp in oldSettings.CooldownGroups)
            {
                newSettings.CooldownGroupAmounts[kvp.Key] = kvp.Value;
            }







            await ChannelSession.Services.Settings.Save(newSettings);

            await ChannelSession.Services.FileService.CopyFile(oldSettings.SettingsFilePath, Path.Combine(SettingsV2Model.SettingsDirectoryName, "Old", oldSettings.SettingsFileName));
            await ChannelSession.Services.FileService.DeleteFile(oldSettings.SettingsFilePath);
        }
#pragma warning restore CS0612 // Type or member is obsolete

        public static async Task<SettingsV3Model> UpgradeSettingsToLatest(string filePath)
        {
            int currentVersion = await GetSettingsVersion(filePath);
            if (currentVersion < 0)
            {
                // Settings file is invalid, we can't use this
                return null;
            }
            else if (currentVersion > SettingsV3Model.LatestVersion)
            {
                // Future build, like a preview build, we can't load this
                return null;
            }
            else if (currentVersion < SettingsV3Model.LatestVersion)
            {
                // Perform upgrade of settings
            }
            SettingsV3Model settings = await FileSerializerHelper.DeserializeFromFile<SettingsV3Model>(filePath, ignoreErrors: true);
            settings.Version = SettingsV3Model.LatestVersion;
            return settings;
        }

        public static async Task<int> GetSettingsVersion(string filePath)
        {
            string fileData = await ChannelSession.Services.FileService.ReadFile(filePath);
            if (string.IsNullOrEmpty(fileData))
            {
                return -1;
            }
            JObject settingsJObj = JObject.Parse(fileData);
            return (int)settingsJObj["Version"];
        }
    }

#pragma warning disable CS0612 // Type or member is obsolete
    public static class SettingsV2Upgrader
    {
        public static async Task<SettingsV2Model> UpgradeSettingsToLatest(string filePath)
        {
            int currentVersion = await GetSettingsVersion(filePath);
            if (currentVersion < 0)
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
                if (currentVersion < 43)
                {
                    await SettingsV2Upgrader.Version43Upgrade(filePath);
                }
                if (currentVersion < 44)
                {
                    await SettingsV2Upgrader.Version44Upgrade(filePath);
                }
            }
            SettingsV2Model settings = await FileSerializerHelper.DeserializeFromFile<SettingsV2Model>(filePath, ignoreErrors: true);
            settings.Version = SettingsV2Model.LatestVersion;
            return settings;
        }

        public static async Task<int> GetSettingsVersion(string filePath)
        {
            string fileData = await ChannelSession.Services.FileService.ReadFile(filePath);
            if (string.IsNullOrEmpty(fileData))
            {
                return -1;
            }
            JObject settingsJObj = JObject.Parse(fileData);
            return (int)settingsJObj["Version"];
        }

        public static async Task Version44Upgrade(string filePath)
        {
            SettingsV2Model settings = await FileSerializerHelper.DeserializeFromFile<SettingsV2Model>(filePath, ignoreErrors: true);
            await settings.Initialize();

            if (settings.ChatShowUserJoinLeave)
            {
                settings.AlertUserJoinLeaveColor = settings.ChatUserJoinLeaveColorScheme;
            }

            if (settings.ChatShowEventAlerts)
            {
                settings.AlertBitsCheeredColor = settings.ChatEventAlertsColorScheme;
                settings.AlertChannelPointsColor = settings.ChatEventAlertsColorScheme;
                settings.AlertFollowColor = settings.ChatEventAlertsColorScheme;
                settings.AlertGiftedSubColor = settings.ChatEventAlertsColorScheme;
                settings.AlertHostColor = settings.ChatEventAlertsColorScheme;
                settings.AlertMassGiftedSubColor = settings.ChatEventAlertsColorScheme;
                settings.AlertModerationColor = settings.ChatEventAlertsColorScheme;
                settings.AlertRaidColor = settings.ChatEventAlertsColorScheme;
                settings.AlertSubColor = settings.ChatEventAlertsColorScheme;
            }

            await ChannelSession.Services.Settings.Save(settings);
        }

        public static async Task Version43Upgrade(string filePath)
        {
            SettingsV2Model settings = await FileSerializerHelper.DeserializeFromFile<SettingsV2Model>(filePath, ignoreErrors: true);
            await settings.Initialize();

            if (settings.IsStreamer)
            {
                List<EventCommand> eventCommandsToAdd = new List<EventCommand>();
                List<EventCommand> eventCommandsToRemove = new List<EventCommand>();
                foreach (EventCommand command in settings.EventCommands)
                {
                    EventCommand newCommand = null;
                    switch (command.EventCommandType)
                    {
                        case EventTypeEnum.MixerChannelEmbersUsed:
                            newCommand = new EventCommand(EventTypeEnum.TwitchChannelBitsCheered);
                            break;
                        case EventTypeEnum.MixerChannelFollowed:
                            newCommand = new EventCommand(EventTypeEnum.TwitchChannelFollowed);
                            break;
                        case EventTypeEnum.MixerChannelHosted:
                            newCommand = new EventCommand(EventTypeEnum.TwitchChannelRaided);
                            break;
                        case EventTypeEnum.MixerChannelResubscribed:
                            newCommand = new EventCommand(EventTypeEnum.TwitchChannelResubscribed);
                            break;
                        case EventTypeEnum.MixerChannelStreamStart:
                            newCommand = new EventCommand(EventTypeEnum.TwitchChannelStreamStart);
                            break;
                        case EventTypeEnum.MixerChannelStreamStop:
                            newCommand = new EventCommand(EventTypeEnum.TwitchChannelStreamStop);
                            break;
                        case EventTypeEnum.MixerChannelSubscribed:
                            newCommand = new EventCommand(EventTypeEnum.TwitchChannelSubscribed);
                            break;
                        case EventTypeEnum.MixerChannelSubscriptionGifted:
                            newCommand = new EventCommand(EventTypeEnum.TwitchChannelSubscriptionGifted);
                            break;
                        case EventTypeEnum.MixerChannelUnfollowed:
                            newCommand = new EventCommand(EventTypeEnum.TwitchChannelUnfollowed);
                            break;
                    }

                    if (newCommand != null)
                    {
                        eventCommandsToAdd.Add(newCommand);
                        eventCommandsToRemove.Add(command);

                        newCommand.Actions.AddRange(command.Actions);
                        newCommand.Unlocked = command.Unlocked;
                        newCommand.IsEnabled = command.IsEnabled;
                    }
                }

                foreach (EventCommand command in eventCommandsToRemove)
                {
                    settings.EventCommands.Remove(command);
                }
                foreach (EventCommand command in eventCommandsToAdd)
                {
                    settings.EventCommands.Add(command);
                }

                settings.StreamElementsOAuthToken = null;
                settings.StreamJarOAuthToken = null;
                settings.StreamlabsOAuthToken = null;
                settings.TipeeeStreamOAuthToken = null;
                settings.TreatStreamOAuthToken = null;
            }

            await ChannelSession.Services.Settings.Save(settings);
        }

        private static IEnumerable<CommandBase> GetAllCommands(SettingsV2Model settings)
        {
            List<CommandBase> commands = new List<CommandBase>();

            commands.AddRange(settings.ChatCommands);
            commands.AddRange(settings.EventCommands);
            commands.AddRange(settings.TimerCommands);
            commands.AddRange(settings.ActionGroupCommands);
            commands.AddRange(settings.GameCommands);
            commands.AddRange(settings.TwitchChannelPointsCommands);
            commands.AddRange(settings.CustomCommands.Values);

            foreach (UserDataModel userData in settings.UserData.Values)
            {
                commands.AddRange(userData.CustomCommands);
                if (userData.EntranceCommand != null)
                {
                    commands.Add(userData.EntranceCommand);
                }
            }

            foreach (GameCommandBase gameCommand in settings.GameCommands)
            {
                commands.AddRange(gameCommand.GetAllInnerCommands());
            }

            foreach (OverlayWidgetModel widget in settings.OverlayWidgets)
            {
                if (widget.Item is OverlayStreamBossItemModel)
                {
                    OverlayStreamBossItemModel item = ((OverlayStreamBossItemModel)widget.Item);
                    if (item.NewStreamBossCommand != null)
                    {
                        commands.Add(item.NewStreamBossCommand);
                    }
                }
                else if (widget.Item is OverlayProgressBarItemModel)
                {
                    OverlayProgressBarItemModel item = ((OverlayProgressBarItemModel)widget.Item);
                    if (item.GoalReachedCommand != null)
                    {
                        commands.Add(item.GoalReachedCommand);
                    }
                }
                else if (widget.Item is OverlayTimerItemModel)
                {
                    OverlayTimerItemModel item = ((OverlayTimerItemModel)widget.Item);
                    if (item.TimerCompleteCommand != null)
                    {
                        commands.Add(item.TimerCompleteCommand);
                    }
                }
            }

            commands.Add(settings.GameQueueUserJoinedCommand);
            commands.Add(settings.GameQueueUserSelectedCommand);
            commands.Add(settings.GiveawayStartedReminderCommand);
            commands.Add(settings.GiveawayUserJoinedCommand);
            commands.Add(settings.GiveawayWinnerSelectedCommand);
            commands.Add(settings.ModerationStrike1Command);
            commands.Add(settings.ModerationStrike2Command);
            commands.Add(settings.ModerationStrike3Command);

            return commands.Where(c => c != null);
        }
    }
#pragma warning restore CS0612 // Type or member is obsolete
}
