using MixItUp.Base.Commands;
using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
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

        Task<SettingsV3Model> Create(string name);

        Task Initialize(SettingsV3Model settings);

#pragma warning disable CS0612 // Type or member is obsolete
        Task Save(SettingsV2Model settings);
#pragma warning restore CS0612 // Type or member is obsolete

        Task Save(SettingsV3Model settings);

        Task SaveLocalBackup(SettingsV3Model settings);

        Task SavePackagedBackup(SettingsV3Model settings, string filePath);

        Task<Result<SettingsV3Model>> RestorePackagedBackup(string filePath);

        Task<bool> PerformAutomaticBackupIfApplicable(SettingsV3Model settings);
    }

    public class SettingsService : ISettingsService
    {
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public void Initialize() { Directory.CreateDirectory(SettingsV3Model.SettingsDirectoryName); }

        public async Task<IEnumerable<SettingsV3Model>> GetAllSettings()
        {
            bool v2SettingsUpgradeNeeded = false;
            bool backupSettingsLoaded = false;
            bool settingsLoadFailure = false;

#pragma warning disable CS0612 // Type or member is obsolete
            foreach (string filePath in Directory.GetFiles(SettingsV2Model.SettingsDirectoryName))
            {
                if (filePath.EndsWith(SettingsV2Model.SettingsFileExtension))
                {
                    if (!v2SettingsUpgradeNeeded)
                    {
                        if (!await DialogHelper.ShowConfirmation(Resources.UpgradePrompt1 +
                            Environment.NewLine + Environment.NewLine +
                            Resources.UpgradePrompt2))
                        {
                            return new List<SettingsV3Model>();
                        }
                    }
                    v2SettingsUpgradeNeeded = true;

                    try
                    {
                        await SettingsV3Upgrader.UpgradeV2ToV3(filePath);
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

                    // Adding delay to ensure the above files are actually deleted
                    await Task.Delay(2000);

                    await ChannelSession.Services.FileService.UnzipFiles(ChannelSession.AppSettings.BackupSettingsFilePath, SettingsV3Model.SettingsDirectoryName);

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
                await DialogHelper.ShowMessage(Resources.BackupSettingsLoadedError);
            }
            if (settingsLoadFailure)
            {
                await DialogHelper.ShowMessage(Resources.SettingsLoadFailure);
            }

            return allSettings;
        }

        public Task<SettingsV3Model> Create(string name)
        {
            return Task.FromResult(new SettingsV3Model(name));
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
            if (settings != null)
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
        }

        public async Task SaveLocalBackup(SettingsV3Model settings)
        {
            if (settings != null)
            {
                Logger.Log(LogLevel.Debug, "Settings local backup save operation started");

                if (ChannelSession.Services.FileService.GetFileSize(settings.SettingsFilePath) == 0)
                {
                    Logger.Log(LogLevel.Debug, "Main settings file is empty, aborting local backup settings save operation");
                    return;
                }

                await semaphore.WaitAndRelease(async () =>
                {
                    await FileSerializerHelper.SerializeToFile(settings.SettingsLocalBackupFilePath, settings);
                });

                Logger.Log(LogLevel.Debug, "Settings local backup save operation finished");
            }
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
                        zipFile.CreateEntryFromFile(settings.DatabaseFilePath, Path.GetFileName(settings.DatabaseFilePath));
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
                    currentVersion = await SettingsV3Upgrader.GetSettingsVersion(settingsFile);
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

        public async Task<bool> PerformAutomaticBackupIfApplicable(SettingsV3Model settings)
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

                    try
                    {
                        if (!Directory.Exists(backupPath))
                        {
                            Directory.CreateDirectory(backupPath);
                        }

                        string filePath = Path.Combine(backupPath, settings.Name + "-Backup-" + DateTimeOffset.Now.ToString("MM-dd-yyyy") + "." + SettingsV3Model.SettingsBackupFileExtension);

                        await this.SavePackagedBackup(settings, filePath);

                        settings.SettingsLastBackup = DateTimeOffset.Now;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, "Failed to create automatic backup directory");
                        Logger.Log(ex);
                        return false;
                    }
                }
            }
            return true;
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
            await oldSettings.Initialize();

            if (oldSettings.IsStreamer)
            {
                string settingsText = await ChannelSession.Services.FileService.ReadFile(filePath);
                settingsText = settingsText.Replace("MixItUp.Base.Model.Settings.SettingsV2Model, MixItUp.Base", "MixItUp.Base.Model.Settings.SettingsV3Model, MixItUp.Base");
                settingsText = settingsText.Replace("MixItUp.Base.ViewModel.User.UserRoleEnum", "MixItUp.Base.Model.User.UserRoleEnum");
                SettingsV3Model newSettings = JSONSerializerHelper.DeserializeFromString<SettingsV3Model>(settingsText, ignoreErrors: true);
                await newSettings.Initialize();

                newSettings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].UserOAuthToken = oldSettings.TwitchUserOAuthToken;
                newSettings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].UserID = oldSettings.TwitchUserID;
                newSettings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].ChannelID = oldSettings.TwitchChannelID;
                newSettings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].BotID = (oldSettings.TwitchBotOAuthToken != null) ? string.Empty : null;
                newSettings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].BotOAuthToken = oldSettings.TwitchBotOAuthToken;

                newSettings.PatreonTierSubscriberEquivalent = oldSettings.PatreonTierMixerSubscriberEquivalent;

                foreach (var kvp in oldSettings.CooldownGroups)
                {
                    newSettings.CooldownGroupAmounts[kvp.Key] = kvp.Value;
                }

                foreach (var kvp in oldSettings.CommandGroups)
                {
                    newSettings.CommandGroups[kvp.Key] = new CommandGroupSettingsModel(kvp.Value);
                }

                foreach (ChatCommand command in oldSettings.ChatCommands)
                {
                    newSettings.SetCommand(new ChatCommandModel(command));
                }

                foreach (EventCommand command in oldSettings.EventCommands)
                {
                    newSettings.SetCommand(new EventCommandModel(command));
                }

                foreach (TimerCommand command in oldSettings.TimerCommands)
                {
                    newSettings.SetCommand(new TimerCommandModel(command));
                }

                foreach (ActionGroupCommand command in oldSettings.ActionGroupCommands)
                {
                    newSettings.SetCommand(new ActionGroupCommandModel(command));
                }

                foreach (TwitchChannelPointsCommand command in oldSettings.TwitchChannelPointsCommands)
                {
                    newSettings.SetCommand(new TwitchChannelPointsCommandModel(command));
                }

                foreach (CustomCommand command in oldSettings.CustomCommands.Values)
                {
                    newSettings.SetCommand(new CustomCommandModel(command));
                }

                foreach (GameCommandBase command in oldSettings.GameCommands)
                {
                    if (command.GetType() == typeof(BeachBallGameCommand)) { newSettings.SetCommand(new HotPotatoGameCommandModel((BeachBallGameCommand)command)); }
                    else if (command.GetType() == typeof(BetGameCommand)) { newSettings.SetCommand(new BetGameCommandModel((BetGameCommand)command)); }
                    else if (command.GetType() == typeof(BidGameCommand)) { newSettings.SetCommand(new BidGameCommandModel((BidGameCommand)command)); }
                    else if (command.GetType() == typeof(CoinPusherGameCommand)) { newSettings.SetCommand(new CoinPusherGameCommandModel((CoinPusherGameCommand)command)); }
                    else if (command.GetType() == typeof(DuelGameCommand)) { newSettings.SetCommand(new DuelGameCommandModel((DuelGameCommand)command)); }
                    else if (command.GetType() == typeof(HangmanGameCommand)) { newSettings.SetCommand(new HangmanGameCommandModel((HangmanGameCommand)command)); }
                    else if (command.GetType() == typeof(HeistGameCommand)) { newSettings.SetCommand(new HeistGameCommandModel((HeistGameCommand)command)); }
                    else if (command.GetType() == typeof(HitmanGameCommand)) { newSettings.SetCommand(new HitmanGameCommandModel((HitmanGameCommand)command)); }
                    else if (command.GetType() == typeof(HotPotatoGameCommand)) { newSettings.SetCommand(new HotPotatoGameCommandModel((HotPotatoGameCommand)command)); }
                    else if (command.GetType() == typeof(LockBoxGameCommand)) { newSettings.SetCommand(new LockBoxGameCommandModel((LockBoxGameCommand)command)); }
                    else if (command.GetType() == typeof(PickpocketGameCommand)) { newSettings.SetCommand(new StealGameCommandModel((PickpocketGameCommand)command)); }
                    else if (command.GetType() == typeof(RouletteGameCommand)) { newSettings.SetCommand(new RouletteGameCommandModel((RouletteGameCommand)command)); }
                    else if (command.GetType() == typeof(RussianRouletteGameCommand)) { newSettings.SetCommand(new RussianRouletteGameCommandModel((RussianRouletteGameCommand)command)); }
                    else if (command.GetType() == typeof(SlotMachineGameCommand)) { newSettings.SetCommand(new SlotMachineGameCommandModel((SlotMachineGameCommand)command)); }
                    else if (command.GetType() == typeof(SpinGameCommand)) { newSettings.SetCommand(new SpinGameCommandModel((SpinGameCommand)command)); }
                    else if (command.GetType() == typeof(StealGameCommand)) { newSettings.SetCommand(new StealGameCommandModel((StealGameCommand)command)); }
                    else if (command.GetType() == typeof(TreasureDefenseGameCommand)) { newSettings.SetCommand(new TreasureDefenseGameCommandModel((TreasureDefenseGameCommand)command)); }
                    else if (command.GetType() == typeof(TriviaGameCommand)) { newSettings.SetCommand(new TriviaGameCommandModel((TriviaGameCommand)command)); }
                    else if (command.GetType() == typeof(VendingMachineGameCommand)) { newSettings.SetCommand(new SpinGameCommandModel((VendingMachineGameCommand)command)); }
                    else if (command.GetType() == typeof(VolcanoGameCommand)) { newSettings.SetCommand(new VolcanoGameCommandModel((VolcanoGameCommand)command)); }
                    else if (command.GetType() == typeof(WordScrambleGameCommand)) { newSettings.SetCommand(new WordScrambleGameCommandModel((WordScrambleGameCommand)command)); }
                }

                newSettings.RemoveCommand(newSettings.GameQueueUserJoinedCommandID);
                newSettings.GameQueueUserJoinedCommandID = SettingsV3Upgrader.ImportCustomCommand(newSettings, oldSettings.GameQueueUserJoinedCommand);

                newSettings.RemoveCommand(newSettings.GameQueueUserSelectedCommandID);
                newSettings.GameQueueUserSelectedCommandID = SettingsV3Upgrader.ImportCustomCommand(newSettings, oldSettings.GameQueueUserSelectedCommand);

                newSettings.RemoveCommand(newSettings.GiveawayStartedReminderCommandID);
                newSettings.GiveawayStartedReminderCommandID = SettingsV3Upgrader.ImportCustomCommand(newSettings, oldSettings.GiveawayStartedReminderCommand);

                newSettings.RemoveCommand(newSettings.GiveawayUserJoinedCommandID);
                newSettings.GiveawayUserJoinedCommandID = SettingsV3Upgrader.ImportCustomCommand(newSettings, oldSettings.GiveawayUserJoinedCommand);

                newSettings.RemoveCommand(newSettings.GiveawayWinnerSelectedCommandID);
                newSettings.GiveawayWinnerSelectedCommandID = SettingsV3Upgrader.ImportCustomCommand(newSettings, oldSettings.GiveawayWinnerSelectedCommand);

                newSettings.RemoveCommand(newSettings.ModerationStrike1CommandID);
                newSettings.ModerationStrike1CommandID = SettingsV3Upgrader.ImportCustomCommand(newSettings, oldSettings.ModerationStrike1Command);

                newSettings.RemoveCommand(newSettings.ModerationStrike2CommandID);
                newSettings.ModerationStrike2CommandID = SettingsV3Upgrader.ImportCustomCommand(newSettings, oldSettings.ModerationStrike2Command);

                newSettings.RemoveCommand(newSettings.ModerationStrike3CommandID);
                newSettings.ModerationStrike3CommandID = SettingsV3Upgrader.ImportCustomCommand(newSettings, oldSettings.ModerationStrike3Command);

                foreach (UserQuoteViewModel quote in oldSettings.Quotes)
                {
                    newSettings.Quotes.Add(quote.Model);
                }

                foreach (var kvp in oldSettings.UserData)
                {
                    newSettings.UserData[kvp.Key] = kvp.Value;
                    if (kvp.Value.EntranceCommand != null)
                    {
                        CustomCommandModel entranceCommand = new CustomCommandModel(kvp.Value.EntranceCommand);
                        newSettings.SetCommand(entranceCommand);
                        kvp.Value.EntranceCommandID = entranceCommand.ID;
                        kvp.Value.EntranceCommand = null;
                    }

                    foreach (ChatCommand command in kvp.Value.CustomCommands)
                    {
                        UserOnlyChatCommandModel userCommand = new UserOnlyChatCommandModel(command, kvp.Key);
                        newSettings.SetCommand(userCommand);
                        kvp.Value.CustomCommandIDs.Add(userCommand.ID);
                    }
                    kvp.Value.CustomCommands.Clear();
                }

                newSettings.GiveawayRequirementsSet = new RequirementsSetModel(oldSettings.GiveawayRequirements);

                foreach (OverlayWidgetModel widget in newSettings.OverlayWidgets.ToList())
                {
                    if (widget.Item is OverlayClipPlaybackItemModel)
                    {
                        newSettings.OverlayWidgets.Remove(widget);
                    }
                    else if (widget.Item is OverlayLeaderboardListItemModel)
                    {
                        if (((OverlayLeaderboardListItemModel)widget.Item).NewLeaderCommand != null)
                        {
                            CustomCommandModel command = new CustomCommandModel(((OverlayLeaderboardListItemModel)widget.Item).NewLeaderCommand);
                            newSettings.SetCommand(command);
                            ((OverlayLeaderboardListItemModel)widget.Item).LeaderChangedCommandID = command.ID;
                            ((OverlayLeaderboardListItemModel)widget.Item).NewLeaderCommand = null;
                        }
                    }
                    else if (widget.Item is OverlayProgressBarItemModel)
                    {
                        if (((OverlayProgressBarItemModel)widget.Item).GoalReachedCommand != null)
                        {
                            CustomCommandModel command = new CustomCommandModel(((OverlayProgressBarItemModel)widget.Item).GoalReachedCommand);
                            newSettings.SetCommand(command);
                            ((OverlayProgressBarItemModel)widget.Item).ProgressGoalReachedCommandID = command.ID;
                            ((OverlayProgressBarItemModel)widget.Item).GoalReachedCommand = null;
                        }
                    }
                    else if (widget.Item is OverlayStreamBossItemModel)
                    {
                        if (((OverlayStreamBossItemModel)widget.Item).NewStreamBossCommand != null)
                        {
                            CustomCommandModel command = new CustomCommandModel(((OverlayStreamBossItemModel)widget.Item).NewStreamBossCommand);
                            newSettings.SetCommand(command);
                            ((OverlayStreamBossItemModel)widget.Item).StreamBossChangedCommandID = command.ID;
                            ((OverlayStreamBossItemModel)widget.Item).NewStreamBossCommand = null;
                        }
                    }
                    else if (widget.Item is OverlayTimerItemModel)
                    {
                        if (((OverlayTimerItemModel)widget.Item).TimerCompleteCommand != null)
                        {
                            CustomCommandModel command = new CustomCommandModel(((OverlayTimerItemModel)widget.Item).TimerCompleteCommand);
                            newSettings.SetCommand(command);
                            ((OverlayTimerItemModel)widget.Item).TimerFinishedCommandID = command.ID;
                            ((OverlayTimerItemModel)widget.Item).TimerCompleteCommand = null;
                        }
                    }
                }

                await ChannelSession.Services.Settings.Save(newSettings);
            }

            await ChannelSession.Services.FileService.CopyFile(oldSettings.SettingsFilePath, Path.Combine(SettingsV2Model.SettingsDirectoryName, "Old", oldSettings.SettingsFileName));
            await ChannelSession.Services.FileService.CopyFile(oldSettings.SettingsLocalBackupFilePath, Path.Combine(SettingsV2Model.SettingsDirectoryName, "Old", oldSettings.SettingsLocalBackupFileName));
            await ChannelSession.Services.FileService.CopyFile(oldSettings.DatabaseFilePath, Path.Combine(SettingsV2Model.SettingsDirectoryName, "Old", oldSettings.DatabaseFileName));

            await ChannelSession.Services.FileService.DeleteFile(oldSettings.SettingsFilePath);
            await ChannelSession.Services.FileService.DeleteFile(oldSettings.SettingsLocalBackupFilePath);
            await ChannelSession.Services.FileService.DeleteFile(oldSettings.DatabaseFilePath);
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
                await SettingsV3Upgrader.Version2Upgrade(currentVersion, filePath);
                await SettingsV3Upgrader.Version3Upgrade(currentVersion, filePath);
            }
            SettingsV3Model settings = await FileSerializerHelper.DeserializeFromFile<SettingsV3Model>(filePath, ignoreErrors: true);
            settings.Version = SettingsV3Model.LatestVersion;
            return settings;
        }

        public static async Task Version3Upgrade(int version, string filePath)
        {
            if (version < 3)
            {
                SettingsV3Model settings = await FileSerializerHelper.DeserializeFromFile<SettingsV3Model>(filePath, ignoreErrors: true);
                await settings.Initialize();

                if (settings.StreamingPlatformAuthentications.ContainsKey(StreamingPlatformTypeEnum.Twitch))
                {
                    settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].UserOAuthToken = null;
                }

                await settings.LoadAllUserData();

                await ChannelSession.Services.Database.Write(settings.DatabaseFilePath, "ALTER TABLE Users ADD COLUMN TwitchUsername TEXT DEFAULT NULL");
                await ChannelSession.Services.Database.Write(settings.DatabaseFilePath, "ALTER TABLE Users ADD COLUMN YouTubeUsername TEXT DEFAULT NULL");
                await ChannelSession.Services.Database.Write(settings.DatabaseFilePath, "ALTER TABLE Users ADD COLUMN FacebookUsername TEXT DEFAULT NULL");
                await ChannelSession.Services.Database.Write(settings.DatabaseFilePath, "ALTER TABLE Users ADD COLUMN TrovoUsername TEXT DEFAULT NULL");
                await ChannelSession.Services.Database.Write(settings.DatabaseFilePath, "ALTER TABLE Users ADD COLUMN GlimeshUsername TEXT DEFAULT NULL");

                Dictionary<Guid, string> userIDToUsername = new Dictionary<Guid, string>();
                foreach (var kvp in settings.UserData)
                {
                    if (kvp.Value.Platform == StreamingPlatformTypeEnum.Twitch && !string.IsNullOrEmpty(kvp.Value.TwitchUsername))
                    {
                        userIDToUsername[kvp.Key] = kvp.Value.TwitchUsername;
                    }
                }

                await ChannelSession.Services.Database.BulkWrite(settings.DatabaseFilePath,
                    "UPDATE Users SET TwitchUsername = $TwitchUsername WHERE ID = $ID",
                    userIDToUsername.Select(u => new Dictionary<string, object>()
                    {
                        { "$ID", u.Key.ToString() }, { "$TwitchUsername", u.Value.ToString() }
                    }));

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        public static async Task Version2Upgrade(int version, string filePath)
        {
            if (version < 2)
            {
                SettingsV3Model settings = await FileSerializerHelper.DeserializeFromFile<SettingsV3Model>(filePath, ignoreErrors: true);
                await settings.Initialize();

                if (settings.StreamingPlatformAuthentications.ContainsKey(StreamingPlatformTypeEnum.Twitch))
                {
                    settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].UserOAuthToken = null;
                }

#pragma warning disable CS0612 // Type or member is obsolete
                if (settings.UnlockAllCommands)
#pragma warning restore CS0612 // Type or member is obsolete
                {
                    settings.CommandServiceLockType = CommandServiceLockTypeEnum.None;
                }

                await ChannelSession.Services.Settings.Save(settings);
            }
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

#pragma warning disable CS0612 // Type or member is obsolete
        private static Guid ImportCustomCommand(SettingsV3Model settings, CustomCommand oldCommand)
        {
            CustomCommandModel newCommand = new CustomCommandModel(oldCommand);
            settings.SetCommand(newCommand);
            return newCommand.ID;
        }
#pragma warning restore CS0612 // Type or member is obsolete
    }
}
