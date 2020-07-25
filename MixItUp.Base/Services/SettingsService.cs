using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModel.Window.Currency;
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

        Task<IEnumerable<SettingsV2Model>> GetAllSettings();

        Task<SettingsV2Model> Create(string name, bool isStreamer);

        Task Initialize(SettingsV2Model settings);

        Task Save(SettingsV2Model settings);

        Task SaveLocalBackup(SettingsV2Model settings);

        Task SavePackagedBackup(SettingsV2Model settings, string filePath);

        Task<Result<SettingsV2Model>> RestorePackagedBackup(string filePath);

        Task PerformAutomaticBackupIfApplicable(SettingsV2Model settings);
    }

    public class SettingsService : ISettingsService
    {
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public void Initialize() { Directory.CreateDirectory(SettingsV2Model.SettingsDirectoryName); }

        public async Task<IEnumerable<SettingsV2Model>> GetAllSettings()
        {
            bool backupSettingsLoaded = false;
            bool settingsLoadFailure = false;

            List<SettingsV2Model> allSettings = new List<SettingsV2Model>();
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
                            allSettings.Add(setting);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                    }

                    if (setting == null)
                    {
                        string localBackupFilePath = string.Format($"{filePath}.{SettingsV2Model.SettingsLocalBackupFileExtension}");
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

                SettingsV2Model settings = allSettings.FirstOrDefault(s => s.ID.Equals(ChannelSession.AppSettings.BackupSettingsToReplace));
                if (settings != null)
                {
                    File.Delete(settings.SettingsFilePath);
                    File.Delete(settings.DatabaseFilePath);

                    using (ZipArchive zipFile = ZipFile.Open(ChannelSession.AppSettings.BackupSettingsFilePath, ZipArchiveMode.Read))
                    {
                        zipFile.ExtractToDirectory(SettingsV2Model.SettingsDirectoryName);
                    }

                    ChannelSession.AppSettings.BackupSettingsFilePath = null;
                    ChannelSession.AppSettings.BackupSettingsToReplace = Guid.Empty;

                    return await this.GetAllSettings();
                }
            }
            else if (ChannelSession.AppSettings.SettingsToDelete != Guid.Empty)
            {
                Logger.Log(LogLevel.Debug, "Settings deletion detected, starting deletion process");

                SettingsV2Model settings = allSettings.FirstOrDefault(s => s.ID.Equals(ChannelSession.AppSettings.SettingsToDelete));
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

        public Task<SettingsV2Model> Create(string name, bool isStreamer)
        {
            return Task.FromResult(new SettingsV2Model(name, isStreamer));
        }

        public async Task Initialize(SettingsV2Model settings)
        {
            await settings.Initialize();
        }

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

        public async Task SaveLocalBackup(SettingsV2Model settings)
        {
            Logger.Log(LogLevel.Debug, "Settings local backup save operation started");

            await semaphore.WaitAndRelease(async () =>
            {
                await FileSerializerHelper.SerializeToFile(settings.SettingsLocalBackupFilePath, settings);
            });

            Logger.Log(LogLevel.Debug, "Settings local backup save operation finished");
        }

        public async Task SavePackagedBackup(SettingsV2Model settings, string filePath)
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

        public async Task<Result<SettingsV2Model>> RestorePackagedBackup(string filePath)
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

                            if (extractedFilePath.EndsWith(SettingsV2Model.SettingsFileExtension, StringComparison.InvariantCultureIgnoreCase))
                            {
                                settingsFile = extractedFilePath;
                            }
                            else if (extractedFilePath.EndsWith(SettingsV2Model.DatabaseFileExtension, StringComparison.InvariantCultureIgnoreCase))
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
                    return new Result<SettingsV2Model>("The backup file selected does not appear to contain Mix It Up settings.");
                }

                if (currentVersion > SettingsV2Model.LatestVersion)
                {
                    return new Result<SettingsV2Model>("The backup file is valid, but is from a newer version of Mix It Up.  Be sure to upgrade to the latest version." +
                        Environment.NewLine + Environment.NewLine + "NOTE: This may require you to opt-in to the Preview build from the General tab in Settings if this was made in a Preview build.");
                }

                return new Result<SettingsV2Model>(await FileSerializerHelper.DeserializeFromFile<SettingsV2Model>(settingsFile));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result<SettingsV2Model>(ex);
            }
        }

        public async Task PerformAutomaticBackupIfApplicable(SettingsV2Model settings)
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
                    string backupPath = Path.Combine(SettingsV2Model.SettingsDirectoryName, SettingsV2Model.DefaultAutomaticBackupSettingsDirectoryName);
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

                    string filePath = Path.Combine(backupPath, settings.MixerChannelID + "-Backup-" + DateTimeOffset.Now.ToString("MM-dd-yyyy") + "." + SettingsV2Model.SettingsBackupFileExtension);

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
                // Perform upgrade of settings
                if (currentVersion < 41)
                {
                    await SettingsV2Upgrader.Version41Upgrade(filePath);
                }
                if (currentVersion < 42)
                {
                    await SettingsV2Upgrader.Version42Upgrade(filePath);
                }
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

#pragma warning disable CS0612 // Type or member is obsolete
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
#pragma warning restore CS0612 // Type or member is obsolete

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
#pragma warning disable CS0612 // Type or member is obsolete
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
#pragma warning restore CS0612 // Type or member is obsolete
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

        public static async Task Version42Upgrade(string filePath)
        {
            SettingsV2Model settings = await FileSerializerHelper.DeserializeFromFile<SettingsV2Model>(filePath, ignoreErrors: true);

            if (settings.IsStreamer)
            {
                List<UserQuoteModel> quotes = new List<UserQuoteModel>();
                await ChannelSession.Services.Database.Read(settings.DatabaseFilePath, "SELECT * FROM Quotes", (Dictionary<string, object> data) =>
                {
                    string json = (string)data["Data"];
                    json = json.Replace("MixItUp.Base.ViewModel.User.UserQuoteViewModel", "MixItUp.Base.Model.User.UserQuoteModel");
                    quotes.Add(JSONSerializerHelper.DeserializeFromString<UserQuoteModel>(json));
                });

                await ChannelSession.Services.Database.BulkWrite(settings.DatabaseFilePath, "REPLACE INTO Quotes(ID, Data) VALUES(@ID, @Data)",
                    quotes.Select(q => new Dictionary<string, object>() { { "@ID", q.ID.ToString() }, { "@Data", JSONSerializerHelper.SerializeToString(q) } }));
            }

            await settings.Initialize();

#pragma warning disable CS0612 // Type or member is obsolete
            if (settings.DiagnosticLogging)
            {
                ChannelSession.AppSettings.DiagnosticLogging = true;
            }
#pragma warning restore CS0612 // Type or member is obsolete


#pragma warning disable CS0612 // Type or member is obsolete
            foreach (UserCurrencyModel oldCurrency in settings.Currencies.Values)
            {
                CurrencyModel newCurrency = new CurrencyModel();

                newCurrency.ID = oldCurrency.ID;
                newCurrency.Name = oldCurrency.Name;
                newCurrency.AcquireAmount = oldCurrency.AcquireAmount;
                newCurrency.AcquireInterval = oldCurrency.AcquireInterval;
                newCurrency.MinimumActiveRate = oldCurrency.MinimumActiveRate;
                newCurrency.OfflineAcquireAmount = oldCurrency.OfflineAcquireAmount;
                newCurrency.OfflineAcquireInterval = oldCurrency.OfflineAcquireInterval;
                newCurrency.MaxAmount = oldCurrency.MaxAmount;
                newCurrency.SpecialIdentifier = oldCurrency.SpecialIdentifier;
                newCurrency.SubscriberBonus = oldCurrency.SubscriberBonus;
                newCurrency.ModeratorBonus = oldCurrency.ModeratorBonus;
                newCurrency.OnFollowBonus = oldCurrency.OnFollowBonus;
                newCurrency.OnHostBonus = oldCurrency.OnHostBonus;
                newCurrency.OnSubscribeBonus = oldCurrency.OnSubscribeBonus;
                newCurrency.ResetInterval = (Model.Currency.CurrencyResetRateEnum)((int)oldCurrency.ResetInterval);
                newCurrency.ResetStartCadence = oldCurrency.ResetStartCadence;
                newCurrency.LastReset = oldCurrency.LastReset;
                newCurrency.IsPrimary = oldCurrency.IsPrimary;

                if (oldCurrency.RankChangedCommand != null)
                {
                    settings.SetCustomCommand(oldCurrency.RankChangedCommand);
                    newCurrency.RankChangedCommandID = oldCurrency.RankChangedCommand.ID;
                }

                foreach (UserRankViewModel rank in oldCurrency.Ranks)
                {
                    newCurrency.Ranks.Add(new RankModel(rank.Name, rank.MinimumPoints));
                }

                settings.Currency[newCurrency.ID] = newCurrency;
            }

            foreach (UserInventoryModel oldInventory in settings.Inventories.Values)
            {
                InventoryModel newInventory = new InventoryModel();

                newInventory.ID = oldInventory.ID;
                newInventory.Name = oldInventory.Name;
                newInventory.DefaultMaxAmount = oldInventory.DefaultMaxAmount;
                newInventory.SpecialIdentifier = oldInventory.SpecialIdentifier;
                newInventory.ShopEnabled = oldInventory.ShopEnabled;
                newInventory.ShopCommand = oldInventory.ShopCommand;
                newInventory.ShopCurrencyID = oldInventory.ShopCurrencyID;
                newInventory.TradeEnabled = oldInventory.TradeEnabled;
                newInventory.TradeCommand = oldInventory.TradeCommand;

                if (oldInventory.ItemsBoughtCommand != null)
                {
                    settings.SetCustomCommand(oldInventory.ItemsBoughtCommand);
                    newInventory.ItemsBoughtCommandID = oldInventory.ItemsBoughtCommand.ID;
                }
                else
                {
                    CustomCommand buyCommand = new CustomCommand(InventoryWindowViewModel.ItemsBoughtCommandName);
                    buyCommand.Actions.Add(new ChatAction("You bought $itemtotal $itemname for $itemcost $currencyname", sendAsStreamer: false));
                    settings.SetCustomCommand(buyCommand);
                    newInventory.ItemsBoughtCommandID = buyCommand.ID;
                }

                if (oldInventory.ItemsSoldCommand != null)
                {
                    settings.SetCustomCommand(oldInventory.ItemsSoldCommand);
                    newInventory.ItemsSoldCommandID = oldInventory.ItemsSoldCommand.ID;
                }
                else
                {
                    CustomCommand sellCommand = new CustomCommand(InventoryWindowViewModel.ItemsSoldCommandName);
                    sellCommand.Actions.Add(new ChatAction("You sold $itemtotal $itemname for $itemcost $currencyname", sendAsStreamer: false));
                    settings.SetCustomCommand(sellCommand);
                    newInventory.ItemsSoldCommandID = sellCommand.ID;
                }

                if (oldInventory.ItemsTradedCommand != null)
                {
                    settings.SetCustomCommand(oldInventory.ItemsTradedCommand);
                    newInventory.ItemsTradedCommandID = oldInventory.ItemsTradedCommand.ID;
                }
                else
                {
                    CustomCommand tradeCommand = new CustomCommand(InventoryWindowViewModel.ItemsTradedCommandName);
                    tradeCommand.Actions.Add(new ChatAction("@$username traded $itemtotal $itemname to @$targetusername for $targetitemtotal $targetitemname", sendAsStreamer: false));
                    settings.SetCustomCommand(tradeCommand);
                    newInventory.ItemsTradedCommandID = tradeCommand.ID;
                }

                foreach (UserInventoryItemModel oldItem in oldInventory.Items.Values.ToList())
                {
                    InventoryItemModel newItem = new InventoryItemModel(oldItem.Name, oldItem.MaxAmount, oldItem.BuyAmount, oldItem.SellAmount);
                    newItem.ID = oldItem.ID;
                    newInventory.Items[newItem.ID] = newItem;
                }

                settings.Inventory[newInventory.ID] = newInventory;
            }
#pragma warning restore CS0612 // Type or member is obsolete

            if (settings.GiveawayRequirements != null && settings.GiveawayRequirements.Inventory != null && settings.Inventory.ContainsKey(settings.GiveawayRequirements.Inventory.InventoryID))
            {
                InventoryModel inventory = settings.Inventory[settings.GiveawayRequirements.Inventory.InventoryID];
#pragma warning disable CS0612 // Type or member is obsolete
                if (inventory != null && !string.IsNullOrEmpty(settings.GiveawayRequirements.Inventory.ItemName))
                {
                    InventoryItemModel item = inventory.GetItem(settings.GiveawayRequirements.Inventory.ItemName);
                    if (item != null)
                    {
                        settings.GiveawayRequirements.Inventory.ItemID = item.ID;
                    }
                    settings.GiveawayRequirements.Inventory.ItemName = null;
                }
#pragma warning restore CS0612 // Type or member is obsolete
            }

            foreach (CommandBase command in SettingsV2Upgrader.GetAllCommands(settings))
            {
                if (command is PermissionsCommandBase)
                {
                    PermissionsCommandBase pCommand = (PermissionsCommandBase)command;
                    if (pCommand.Requirements != null && pCommand.Requirements.Inventory != null && settings.Inventory.ContainsKey(pCommand.Requirements.Inventory.InventoryID))
                    {
                        InventoryModel inventory = settings.Inventory[pCommand.Requirements.Inventory.InventoryID];
#pragma warning disable CS0612 // Type or member is obsolete
                        if (inventory != null && !string.IsNullOrEmpty(pCommand.Requirements.Inventory.ItemName))
                        {
                            InventoryItemModel item = inventory.GetItem(pCommand.Requirements.Inventory.ItemName);
                            if (item != null)
                            {
                                pCommand.Requirements.Inventory.ItemID = item.ID;
                            }
                            pCommand.Requirements.Inventory.ItemName = null;
                        }
#pragma warning restore CS0612 // Type or member is obsolete
                    }
                }
            }

            List<UserDataModel> usersToRemove = new List<UserDataModel>();
            foreach (UserDataModel user in settings.UserData.Values.ToList())
            {
                if (user.MixerID <= 0)
                {
                    usersToRemove.Add(user);
                }
            }

            foreach (UserDataModel user in usersToRemove)
            {
                settings.UserData.Remove(user.ID);
            }

            await ChannelSession.Services.Settings.Save(settings);
        }

        public static async Task Version41Upgrade(string filePath)
        {
            SettingsV2Model settings = await FileSerializerHelper.DeserializeFromFile<SettingsV2Model>(filePath, ignoreErrors: true);

            // Check if the old CurrencyAmounts and InventoryAmounts tables exist
            bool tablesExist = false;
            await ChannelSession.Services.Database.Read(settings.DatabaseFilePath, "SELECT * FROM CurrencyAmounts LIMIT 1", (Dictionary<string, object> data) =>
            {
                if (data.Count > 0)
                {
                    tablesExist = true;
                }
            });

            if (tablesExist)
            {
                await settings.Initialize();

                await ChannelSession.Services.Database.Read(settings.DatabaseFilePath, "SELECT * FROM CurrencyAmounts", (Dictionary<string, object> data) =>
                {
                    Guid currencyID = Guid.Parse((string)data["CurrencyID"]);
                    Guid userID = Guid.Parse((string)data["UserID"]);
                    int amount = Convert.ToInt32(data["Amount"]);

                    if (amount > 0 && settings.UserData.ContainsKey(userID))
                    {
                        settings.UserData[userID].CurrencyAmounts[currencyID] = amount;
                        settings.UserData.ManualValueChanged(userID);
                    }
                });

                await ChannelSession.Services.Database.Read(settings.DatabaseFilePath, "SELECT * FROM InventoryAmounts", (Dictionary<string, object> data) =>
                {
                    Guid inventoryID = Guid.Parse((string)data["InventoryID"]);
                    Guid userID = Guid.Parse((string)data["UserID"]);
                    Guid itemID = Guid.Parse((string)data["ItemID"]);
                    int amount = Convert.ToInt32(data["Amount"]);

                    if (amount > 0 && settings.UserData.ContainsKey(userID))
                    {
                        if (!settings.UserData[userID].InventoryAmounts.ContainsKey(inventoryID))
                        {
                            settings.UserData[userID].InventoryAmounts[inventoryID] = new Dictionary<Guid, int>();
                        }
                        settings.UserData[userID].InventoryAmounts[inventoryID][itemID] = amount;
                        settings.UserData.ManualValueChanged(userID);
                    }
                });

                await ChannelSession.Services.Settings.Save(settings);
            }
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
}
