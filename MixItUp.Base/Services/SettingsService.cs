using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Model.Twitch.Bits;
using MixItUp.Base.Model.Web;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Overlay;
using Newtonsoft.Json.Linq;
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

    public static class OAuthTokenModelStaticMethods
    {
        public static void Reset(this OAuthTokenModel token)
        {
            if (token != null)
            {
                token.accessToken = String.Empty;
                token.refreshToken = String.Empty;
            }
        }
    }

    public class SettingsService
    {
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public void Initialize() { Directory.CreateDirectory(SettingsV3Model.SettingsDirectoryName); }

        public async Task<IEnumerable<SettingsV3Model>> GetAllSettings()
        {
            bool backupSettingsLoaded = false;
            bool settingsLoadFailure = false;

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

            if (!string.IsNullOrEmpty(ChannelSession.AppSettings.SettingsRestoreFilePath))
            {
                try
                {
                    Logger.Log(LogLevel.Debug, "Restored settings file detected, starting restore process");

                    if (ChannelSession.AppSettings.SettingsToReplaceDuringRestore != Guid.Empty)
                    {
                        SettingsV3Model settings = allSettings.FirstOrDefault(s => s.ID.Equals(ChannelSession.AppSettings.SettingsToReplaceDuringRestore));
                        if (settings != null)
                        {

                                File.Delete(settings.SettingsFilePath);
                                File.Delete(settings.DatabaseFilePath);

                                // Adding delay to ensure the above files are actually deleted
                                await Task.Delay(2000);

                        }
                    }

                    await ServiceManager.Get<IFileService>().UnzipFiles(ChannelSession.AppSettings.SettingsRestoreFilePath, SettingsV3Model.SettingsDirectoryName);

                    ChannelSession.AppSettings.SettingsRestoreFilePath = null;
                    ChannelSession.AppSettings.SettingsToReplaceDuringRestore = Guid.Empty;

                    return await this.GetAllSettings();
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    await DialogHelper.ShowMessage(Resources.FailedToRestoreSettings);
                }
            }
            else if (ChannelSession.AppSettings.SettingsToDelete != Guid.Empty)
            {
                try
                {
                    Logger.Log(LogLevel.Debug, "Settings deletion detected, starting deletion process");

                    SettingsV3Model settings = allSettings.FirstOrDefault(s => s.ID.Equals(ChannelSession.AppSettings.SettingsToDelete));
                    ChannelSession.AppSettings.SettingsToDelete = Guid.Empty;

                    if (settings != null)
                    {
                        File.Delete(settings.SettingsFilePath);
                        File.Delete(settings.DatabaseFilePath);

                        return await this.GetAllSettings();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    await DialogHelper.ShowMessage(Resources.FailedToDeleteSettings);
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

            // Empty out all restore and deleting values to ensure they are clear
            ChannelSession.AppSettings.SettingsRestoreFilePath = null;
            ChannelSession.AppSettings.SettingsToReplaceDuringRestore = Guid.Empty;
            ChannelSession.AppSettings.SettingsToDelete = Guid.Empty;

            return allSettings;
        }

        public async Task Initialize(SettingsV3Model settings)
        {
            await settings.Initialize();
        }

        public async Task Save(SettingsV3Model settings)
        {
            if (settings != null)
            {
                try
                {
                    await semaphore.WaitAsync();

                    settings.CopyLatestValues();
                    await FileSerializerHelper.SerializeToFile(settings.SettingsFilePath, settings);
                    await settings.SaveDatabaseData();
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        public async Task SaveLocalBackup(SettingsV3Model settings)
        {
            if (settings != null)
            {
                Logger.Log(LogLevel.Debug, "Settings local backup save operation started");

                if (ServiceManager.Get<IFileService>().GetFileSize(settings.SettingsFilePath) == 0)
                {
                    Logger.Log(LogLevel.Debug, "Main settings file is empty, aborting local backup settings save operation");
                    return;
                }

                try
                {
                    await semaphore.WaitAsync();

                    await FileSerializerHelper.SerializeToFile(settings.SettingsLocalBackupFilePath, settings);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
                finally
                {
                    semaphore.Release();
                }

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

                    ServiceManager.Get<IDatabaseService>().ClearAllPools();

                    using (ZipArchive zipFile = ZipFile.Open(filePath, ZipArchiveMode.Create))
                    {
                        zipFile.CreateEntryFromFile(settings.SettingsFilePath, Path.GetFileName(settings.SettingsFilePath));
                        zipFile.CreateEntryFromFile(settings.DatabaseFilePath, Path.GetFileName(settings.DatabaseFilePath));
                    }
                    return;
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

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            await DialogHelper.ShowMessage(MixItUp.Base.Resources.BackupGenerationFailed);
        }

        public async Task<Result<SettingsV3Model>> RestorePackagedBackup(string filePath)
        {
            try
            {
                string tempFilePath = ServiceManager.Get<IFileService>().GetTempFolder();
                string tempFolder = Path.GetDirectoryName(tempFilePath);

                string settingsFile = null;
                string databaseFile = null;

                ServiceManager.Get<IDatabaseService>().ClearAllPools();

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
                    return new Result<SettingsV3Model>(MixItUp.Base.Resources.SettingsBackupNotValid);
                }

                if (currentVersion > SettingsV3Model.LatestVersion)
                {
                    return new Result<SettingsV3Model>(MixItUp.Base.Resources.SettingsBackupTooNew);
                }

                return new Result<SettingsV3Model>(await FileSerializerHelper.DeserializeFromFile<SettingsV3Model>(settingsFile, ignoreErrors: true));
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
        public static bool OverlayV3UpgradeOccurred;

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
                await SettingsV3Upgrader.Version7Upgrade(currentVersion, filePath);
                await SettingsV3Upgrader.Version8Upgrade(currentVersion, filePath);
            }
            SettingsV3Model settings = await FileSerializerHelper.DeserializeFromFile<SettingsV3Model>(filePath, ignoreErrors: true);
            settings.Version = SettingsV3Model.LatestVersion;
            return settings;
        }

        public static async Task Version8Upgrade(int version, string filePath)
        {
            if (version < 8)
            {
                await ReplaceOAuthTokenModelClass(filePath);

                SettingsV3Model settings = await FileSerializerHelper.DeserializeFromFile<SettingsV3Model>(filePath, ignoreErrors: true);
                await settings.Initialize();

                if (settings.TTSMonsterOAuthToken != null)
                {
#pragma warning disable CS0612 // Type or member is obsolete
                    settings.TTSMonsterOAuthToken.accessToken = settings.TTSMonsterOAuthToken.clientSecret;
#pragma warning restore CS0612 // Type or member is obsolete
                }

                await ServiceManager.Get<SettingsService>().Save(settings);
            }
        }

        private static async Task ReplaceOAuthTokenModelClass(string filePath)
        {
            string settingsFileContents = await ServiceManager.Get<IFileService>().ReadFile(filePath);
            settingsFileContents = settingsFileContents.Replace("StreamingClient.Base.Model.OAuth.OAuthTokenModel, StreamingClient.Base", "MixItUp.Base.Model.Web.OAuthTokenModel, MixItUp.Base");
            await ServiceManager.Get<IFileService>().SaveFile(filePath, settingsFileContents);
        }

        public static async Task Version7Upgrade(int version, string filePath)
        {
            if (version < 7)
            {
                await ReplaceOAuthTokenModelClass(filePath);

                SettingsV3Model settings = await FileSerializerHelper.DeserializeFromFile<SettingsV3Model>(filePath, ignoreErrors: true);
                await settings.Initialize();

                ChannelSession.SetChannelSessionSettings(settings);

                settings.OverlayEndpointsV3.Add(new OverlayEndpointV3Model(OverlayEndpointV3Model.DefaultOverlayName)
                {
                    ID = Guid.Empty
                });

#pragma warning disable CS0612 // Type or member is obsolete
                foreach (var kvp in settings.OverlayCustomNameAndPorts)
                {
                    settings.OverlayEndpointsV3.Add(new OverlayEndpointV3Model(kvp.Key));
                }
                settings.OverlayCustomNameAndPorts.Clear();

                foreach (var kvp in settings.Commands)
                {
                    if (kvp.Value is ActionGroupCommandModel)
                    {
                        ActionGroupCommandModel command = (ActionGroupCommandModel)kvp.Value;
                        if (command.RunOneRandomly)
                        {
                            RandomActionModel randomAction = new RandomActionModel(amount: "1", noDuplicates: false, persistNoDuplicates: false, command.Actions.ToList());
                            command.Actions.Clear();
                            command.Actions.Add(randomAction);
                            settings.Commands.ManualValueChanged(command.ID);
                        }
                    }
                    else if (kvp.Value is GameCommandModelBase)
                    {
                        GameCommandModelBase command = (GameCommandModelBase)kvp.Value;
                        foreach (CommandModelBase subCommand in command.GetInnerCommands())
                        {
                            UpdateActionsV7(settings, subCommand.Actions);
                        }
                        settings.Commands.ManualValueChanged(kvp.Key);
                    }

                    if (UpdateActionsV7(settings, kvp.Value.Actions))
                    {
                        settings.Commands.ManualValueChanged(kvp.Key);
                    }
                }

                foreach (OverlayWidgetModel widget in settings.OverlayWidgets)
                {
                    OverlayWidgetV3Model newWidget = new OverlayWidgetV3Model();

                    OverlayEndpointV3Model endpoint = settings.OverlayEndpointsV3.FirstOrDefault(e => string.Equals(e.Name, widget.OverlayName));

                    newWidget.Name = widget.Name;
                    newWidget.IsEnabled = widget.IsEnabled;
                    if (widget.Item.ItemType == OverlayItemModelTypeEnum.HTML || widget.Item.ItemType == OverlayItemModelTypeEnum.Image ||
                        widget.Item.ItemType == OverlayItemModelTypeEnum.Text || widget.Item.ItemType == OverlayItemModelTypeEnum.Video ||
                        widget.Item.ItemType == OverlayItemModelTypeEnum.YouTube)
                    {
                        newWidget.RefreshTime = widget.RefreshTime;
                    }

                    OverlayItemV3ModelBase item = SettingsV3Upgrader.ConvertOldOverlayItem(widget.Item, settings);
                    if (item != null)
                    {
                        item.ID = widget.Item.ID;
                        newWidget.Item = item;
                        if (endpoint != null && endpoint.ID != Guid.Empty)
                        {
                            newWidget.Item.OverlayEndpointID = endpoint.ID;
                        }

                        if (newWidget.Type == OverlayItemV3Type.EndCredits)
                        {
                            newWidget.IsEnabled = true;
                        }

                        if (newWidget.Type == OverlayItemV3Type.Text)
                        {
                            newWidget.Item.Javascript = OverlayResources.OverlayTextWidgetDefaultJavascript;
                        }
                        else if (newWidget.Type == OverlayItemV3Type.Image)
                        {
                            newWidget.Item.Javascript = OverlayResources.OverlayImageWidgetDefaultJavascript;
                        }
                        else if (newWidget.Type == OverlayItemV3Type.Video)
                        {
                            newWidget.Item.Javascript = OverlayResources.OverlayVideoWidgetDefaultJavascript;
                        }
                        else if (newWidget.Type == OverlayItemV3Type.YouTube)
                        {
                            newWidget.Item.Javascript = OverlayResources.OverlayYouTubeWidgetDefaultJavascript;
                        }
                        else if (newWidget.Type == OverlayItemV3Type.HTML)
                        {
                            newWidget.Item.Javascript = OverlayResources.OverlayHTMLWidgetDefaultJavascript;
                        }

                        settings.OverlayWidgetsV3.Add(newWidget);
                    }
                }
                settings.OverlayWidgets.Clear();
#pragma warning restore CS0612 // Type or member is obsolete

                ChannelSession.SetChannelSessionSettings(null);

                await ServiceManager.Get<SettingsService>().Save(settings);

                SettingsV3Upgrader.OverlayV3UpgradeOccurred = true;
            }
        }

        public static bool UpdateActionsV7(SettingsV3Model settings, IEnumerable<ActionModelBase> actions)
        {
            bool updated = false;
            foreach (ActionModelBase actionModel in actions)
            {
                if (actionModel is OverlayActionModel)
                {
                    OverlayActionModel action = (OverlayActionModel)actionModel;

#pragma warning disable CS0612 // Type or member is obsolete
                    if (action.WidgetID == Guid.Empty && action.OverlayItem != null)
                    {
                        action.Duration = action.OverlayItem.Effects.Duration.ToString();
                        if (action.OverlayItem.Effects.EntranceAnimation != OverlayItemEffectEntranceAnimationTypeEnum.None)
                        {
                            action.EntranceAnimation = new OverlayAnimationV3Model()
                            {
                                AnimateCSSAnimation = EnumHelper.GetEnumValueFromString<OverlayAnimateCSSAnimationType>(action.OverlayItem.Effects.EntranceAnimation.ToString())
                            };
                        }
                        else
                        {
                            action.EntranceAnimation = new OverlayAnimationV3Model()
                            {
                                AnimateCSSAnimation = OverlayAnimateCSSAnimationType.None
                            };
                        }

                        if (action.OverlayItem.Effects.VisibleAnimation != OverlayItemEffectVisibleAnimationTypeEnum.None)
                        {
                            action.CustomAnimations.Add(new OverlayAnimationV3Model()
                            {
                                AnimateCSSAnimation = EnumHelper.GetEnumValueFromString<OverlayAnimateCSSAnimationType>(action.OverlayItem.Effects.VisibleAnimation.ToString()),
                                StartTime = action.OverlayItem.Effects.Duration / 2
                            });
                        }

                        if (action.OverlayItem.Effects.ExitAnimation != OverlayItemEffectExitAnimationTypeEnum.None)
                        {
                            action.ExitAnimation = new OverlayAnimationV3Model()
                            {
                                AnimateCSSAnimation = EnumHelper.GetEnumValueFromString<OverlayAnimateCSSAnimationType>(action.OverlayItem.Effects.ExitAnimation.ToString())
                            };
                        }
                        else
                        {
                            action.ExitAnimation = new OverlayAnimationV3Model()
                            {
                                AnimateCSSAnimation = OverlayAnimateCSSAnimationType.None
                            };
                        }

                        OverlayItemV3ModelBase item = SettingsV3Upgrader.ConvertOldOverlayItem(action.OverlayItem, settings);
                        if (item != null)
                        {
                            action.OverlayItemV3 = item;
                            OverlayEndpointV3Model endpoint = settings.OverlayEndpointsV3.FirstOrDefault(e => string.Equals(e.Name, action.OverlayName));
                            if (endpoint != null && endpoint.ID != Guid.Empty)
                            {
                                action.OverlayItemV3.OverlayEndpointID = endpoint.ID;
                            }
                        }
                    }
                    else if (action.WidgetID != Guid.Empty)
                    {
                        OverlayWidgetModel widget = settings.OverlayWidgets.FirstOrDefault(w => w.Item.ID == action.WidgetID);
                        if (widget != null)
                        {
                            if (widget.Item.ItemType == OverlayItemModelTypeEnum.EndCredits && action.ShowWidget)
                            {
                                action.EndCreditsID = action.WidgetID;
                                action.WidgetID = Guid.Empty;
                            }
                        }
                        else
                        {
                            OverlayWidgetV3Model widgetv3 = settings.OverlayWidgetsV3.FirstOrDefault(w => w.ID == action.WidgetID);
                            if (widget != null)
                            {
                                if (widgetv3.Type == OverlayItemV3Type.EndCredits && action.ShowWidget)
                                {
                                    action.EndCreditsID = action.WidgetID;
                                    action.WidgetID = Guid.Empty;
                                }
                            }
                        }
                    }
#pragma warning restore CS0612 // Type or member is obsolete
                    updated = true;
                }
                else if (actionModel is TextToSpeechActionModel)
                {
                    TextToSpeechActionModel action = (TextToSpeechActionModel)actionModel;
                    updated = true;
                }
                else if (actionModel is GroupActionModel)
                {
                    GroupActionModel action = (GroupActionModel)actionModel;
                    if (UpdateActionsV7(settings, action.Actions))
                    {
                        updated = true;
                    }

                    if (actionModel is ConditionalActionModel)
                    {
                        ConditionalActionModel conditionalAction = (ConditionalActionModel)actionModel;
                        if (conditionalAction.Clauses.Count == 0)
                        {
                            conditionalAction.Clauses.Add(new ConditionalClauseModel(ConditionalComparisionTypeEnum.Equals, "1", "1", null));
                            updated = true;
                        }
                    }
                }
            }
            return updated;
        }

        public static async Task<int> GetSettingsVersion(string filePath)
        {
            string fileData = await ServiceManager.Get<IFileService>().ReadFile(filePath);
            if (string.IsNullOrEmpty(fileData))
            {
                return -1;
            }
            JObject settingsJObj = JObject.Parse(fileData);
            return (int)settingsJObj["Version"];
        }

        private static readonly Dictionary<string, string> HTMLColorSchemeDictionary = new Dictionary<string, string>()
        {
            { "#ffb300", "Amber" },
            { "#000000", "Black" },
            { "#2196f3", "Blue" },
            { "#795548", "Brown" },
            { "#00bcd4", "Cyan" },
            { "#ff5722", "DeepOrange" },
            { "#673ab7", "DeepPurple" },
            { "#4caf50", "Green" },
            { "#9e9e9e", "Grey" },
            { "#3f51b5", "Indigo" },
            { "#03a9f4", "LightBlue" },
            { "#8bc34a", "LightGreen" },
            { "#cddc39", "Lime" },
            { "#ff9800", "Orange" },
            { "#e91e63", "Pink" },
            { "#9c27b0", "Purple" },
            { "#f44336", "Red" },
            { "#009688", "Teal" },
            { "#ffffff", "White" },
            { "#ffeb3b", "Yellow" },
        };

        private static string UpgradeOldOverlayColor(string oldFontColor)
        {
            if (!string.IsNullOrEmpty(oldFontColor) && HTMLColorSchemeDictionary.ContainsKey(oldFontColor))
            {
                return HTMLColorSchemeDictionary[oldFontColor];
            }
            return oldFontColor;
        }

#pragma warning disable CS0612 // Type or member is obsolete
        private static OverlayItemV3ModelBase ConvertOldOverlayItem(OverlayItemModelBase item, SettingsV3Model settings)
        {
            OverlayItemV3ViewModelBase vmResult = null;
            OverlayItemV3ModelBase result = null;
            if (item.ItemType == OverlayItemModelTypeEnum.ChatMessages)
            {
                OverlayChatMessagesListItemModel oldItem = (OverlayChatMessagesListItemModel)item;
                OverlayChatV3ViewModel newItem = new OverlayChatV3ViewModel();
                vmResult = newItem;

                newItem.BackgroundColor = UpgradeOldOverlayColor(oldItem.BackgroundColor);
                newItem.BorderColor = UpgradeOldOverlayColor(oldItem.BorderColor);
                newItem.FontColor = UpgradeOldOverlayColor(oldItem.TextColor);
                newItem.FontName = oldItem.TextFont;
                newItem.FontSize = oldItem.Height;
                newItem.MessageRemovalTime = oldItem.FadeOut;

                newItem.MessageAddedAnimation.SelectedAnimatedCSSAnimation = EnumHelper.GetEnumValueFromString<OverlayAnimateCSSAnimationType>(oldItem.Effects.EntranceAnimation.ToString());
                newItem.MessageRemovedAnimation.SelectedAnimatedCSSAnimation = EnumHelper.GetEnumValueFromString<OverlayAnimateCSSAnimationType>(oldItem.Effects.ExitAnimation.ToString());
                result = newItem.GetItem();
                result.OldCustomHTML = oldItem.HTML;
            }
            else if (item.ItemType == OverlayItemModelTypeEnum.EndCredits)
            {
                OverlayEndCreditsItemModel oldItem = (OverlayEndCreditsItemModel)item;
                OverlayEndCreditsV3ViewModel newItem = new OverlayEndCreditsV3ViewModel();
                vmResult = newItem;

                newItem.BackgroundColor = UpgradeOldOverlayColor(oldItem.BackgroundColor);
                newItem.Header.FontColor = UpgradeOldOverlayColor(oldItem.SectionTextColor);
                newItem.Header.FontName = oldItem.SectionTextFont;
                newItem.Header.FontSize = oldItem.SectionTextSize;
                newItem.FontColor = UpgradeOldOverlayColor(oldItem.ItemTextColor);
                newItem.FontName = oldItem.ItemTextFont;
                newItem.FontSize = oldItem.ItemTextSize;
                newItem.SelectedScrollSpeed = EnumHelper.GetEnumValueFromString<OverlayEndCreditsSpeedV3TypeEnum>(oldItem.Speed.ToString());

                foreach (var oldSection in oldItem.SectionTemplates)
                {
                    OverlayEndCreditsSectionV3ViewModel newSection = new OverlayEndCreditsSectionV3ViewModel(newItem);
                    switch (oldSection.Key)
                    {
                        case OverlayEndCreditsSectionTypeEnum.Followers:
                            newSection.SelectedType = OverlayEndCreditsSectionV3Type.Followers;
                            break;
                        case OverlayEndCreditsSectionTypeEnum.NewSubscribers:
                            newSection.SelectedType = OverlayEndCreditsSectionV3Type.NewSubscribers;
                            break;
                        case OverlayEndCreditsSectionTypeEnum.Resubscribers:
                            newSection.SelectedType = OverlayEndCreditsSectionV3Type.Resubscribers;
                            break;
                        case OverlayEndCreditsSectionTypeEnum.GiftedSubs:
                            newSection.SelectedType = OverlayEndCreditsSectionV3Type.GiftedSubscriptions;
                            break;
                        case OverlayEndCreditsSectionTypeEnum.Donations:
                            newSection.SelectedType = OverlayEndCreditsSectionV3Type.Donations;
                            break;
                        case OverlayEndCreditsSectionTypeEnum.Subscribers:
                            newSection.SelectedType = OverlayEndCreditsSectionV3Type.Subscribers;
                            break;
                        case OverlayEndCreditsSectionTypeEnum.Moderators:
                            newSection.SelectedType = OverlayEndCreditsSectionV3Type.Moderators;
                            break;
                        case OverlayEndCreditsSectionTypeEnum.FreeFormHTML:
                        case OverlayEndCreditsSectionTypeEnum.FreeFormHTML2:
                        case OverlayEndCreditsSectionTypeEnum.FreeFormHTML3:
                            newSection.SelectedType = OverlayEndCreditsSectionV3Type.HTML;
                            newSection.Name = oldSection.Value.SectionHTML;
                            newSection.ItemTemplate = oldSection.Value.UserHTML;
                            break;
                        case OverlayEndCreditsSectionTypeEnum.Bits:
                            newSection.SelectedType = OverlayEndCreditsSectionV3Type.TwitchBits;
                            break;
                        case OverlayEndCreditsSectionTypeEnum.Hosts:
                        case OverlayEndCreditsSectionTypeEnum.Raids:
                            newSection.SelectedType = OverlayEndCreditsSectionV3Type.Raids;
                            break;
                    }

                    if (newSection != null)
                    {
                        newSection.Name = EnumLocalizationHelper.GetLocalizedName(newSection.SelectedType);
                        newItem.Sections.Add(newSection);
                    }
                }
                result = newItem.GetItem();
                result.OldCustomHTML = string.Join("\n\n", oldItem.HTML, oldItem.TitleTemplate);
                foreach (var oldSection in oldItem.SectionTemplates)
                {
                    result.OldCustomHTML += "\n\n";
                    result.OldCustomHTML += string.Join("\n\n", oldSection.Value.SectionHTML, oldSection.Value.UserHTML);
                }
            }
            else if (item.ItemType == OverlayItemModelTypeEnum.EventList)
            {
                OverlayEventListItemModel oldItem = (OverlayEventListItemModel)item;
                OverlayEventListV3ViewModel newItem = new OverlayEventListV3ViewModel();
                vmResult = newItem;

                newItem.TotalToShow = oldItem.TotalToShow;
                newItem.BorderColor = UpgradeOldOverlayColor(oldItem.BorderColor);
                newItem.BackgroundColor = UpgradeOldOverlayColor(oldItem.BackgroundColor);
                newItem.FontColor = UpgradeOldOverlayColor(oldItem.TextColor);
                newItem.FontName = oldItem.TextFont;
                newItem.Header.FontColor = UpgradeOldOverlayColor(oldItem.TextColor);
                newItem.Header.FontName = oldItem.TextFont;
                newItem.Width = oldItem.Width.ToString();
                newItem.Height = oldItem.Height.ToString();

                newItem.AddToTop = true;
                newItem.ItemAddedAnimation.SelectedAnimatedCSSAnimation = EnumHelper.GetEnumValueFromString<OverlayAnimateCSSAnimationType>(oldItem.Effects.EntranceAnimation.ToString());
                newItem.ItemRemovedAnimation.SelectedAnimatedCSSAnimation = EnumHelper.GetEnumValueFromString<OverlayAnimateCSSAnimationType>(oldItem.Effects.ExitAnimation.ToString());
                if (oldItem.ItemTypes.Contains(OverlayEventListItemTypeEnum.Followers))
                {
                    newItem.Follows = true;
                }
                if (oldItem.ItemTypes.Contains(OverlayEventListItemTypeEnum.Hosts) || oldItem.ItemTypes.Contains(OverlayEventListItemTypeEnum.Raids))
                {
                    newItem.Raids = true;
                }
                if (oldItem.ItemTypes.Contains(OverlayEventListItemTypeEnum.Subscribers))
                {
                    newItem.TwitchSubscriptions = true;
                    newItem.YouTubeMemberships = true;
                    newItem.TrovoSubscriptions = true;
                }
                if (oldItem.ItemTypes.Contains(OverlayEventListItemTypeEnum.Donations))
                {
                    newItem.Donations = true;
                }
                if (oldItem.ItemTypes.Contains(OverlayEventListItemTypeEnum.Bits))
                {
                    newItem.TwitchBits = true;
                }
                result = newItem.GetItem();
                result.OldCustomHTML = oldItem.HTML;
            }
            else if (item.ItemType == OverlayItemModelTypeEnum.GameQueue)
            {
                OverlayGameQueueListItemModel oldItem = (OverlayGameQueueListItemModel)item;
                OverlayGameQueueV3ViewModel newItem = new OverlayGameQueueV3ViewModel();
                vmResult = newItem;

                newItem.TotalToShow = oldItem.TotalToShow;
                newItem.BorderColor = UpgradeOldOverlayColor(oldItem.BorderColor);
                newItem.BackgroundColor = UpgradeOldOverlayColor(oldItem.BackgroundColor);
                newItem.FontColor = UpgradeOldOverlayColor(oldItem.TextColor);
                newItem.FontName = oldItem.TextFont;
                newItem.Width = oldItem.Width.ToString();
                newItem.Height = oldItem.Height.ToString();

                newItem.ItemAddedAnimation.SelectedAnimatedCSSAnimation = EnumHelper.GetEnumValueFromString<OverlayAnimateCSSAnimationType>(oldItem.Effects.EntranceAnimation.ToString());
                newItem.ItemRemovedAnimation.SelectedAnimatedCSSAnimation = EnumHelper.GetEnumValueFromString<OverlayAnimateCSSAnimationType>(oldItem.Effects.ExitAnimation.ToString());
                result = newItem.GetItem();
                result.OldCustomHTML = oldItem.HTML;
            }
            else if (item.ItemType == OverlayItemModelTypeEnum.HTML)
            {
                OverlayHTMLItemModel oldItem = (OverlayHTMLItemModel)item;
                OverlayHTMLV3ViewModel newItem = new OverlayHTMLV3ViewModel();
                vmResult = newItem;
                result = newItem.GetItem();
                result.HTML = oldItem.HTML;
                result.OldCustomHTML = oldItem.HTML;
                result.HTML = oldItem.HTML;
            }
            else if (item.ItemType == OverlayItemModelTypeEnum.Image)
            {
                OverlayImageItemModel oldItem = (OverlayImageItemModel)item;
                OverlayImageV3ViewModel newItem = new OverlayImageV3ViewModel();
                vmResult = newItem;
                newItem.FilePath = oldItem.FilePath;
                newItem.Width = oldItem.Width.ToString();
                newItem.Height = oldItem.Height.ToString();
                result = newItem.GetItem();
            }
            else if (item.ItemType == OverlayItemModelTypeEnum.Leaderboard)
            {
                OverlayLeaderboardListItemModel oldItem = (OverlayLeaderboardListItemModel)item;
                OverlayLeaderboardV3ViewModel newItem = new OverlayLeaderboardV3ViewModel();
                vmResult = newItem;

                newItem.TotalToShow = oldItem.TotalToShow;
                newItem.BorderColor = UpgradeOldOverlayColor(oldItem.BorderColor);
                newItem.BackgroundColor = UpgradeOldOverlayColor(oldItem.BackgroundColor);
                newItem.FontColor = UpgradeOldOverlayColor(oldItem.TextColor);
                newItem.FontName = oldItem.TextFont;
                newItem.Header.FontColor = UpgradeOldOverlayColor(oldItem.TextColor);
                newItem.Header.FontName = oldItem.TextFont;
                newItem.Width = oldItem.Width.ToString();
                newItem.Height = oldItem.Height.ToString();

                if (oldItem.LeaderboardType == OverlayLeaderboardListItemTypeEnum.CurrencyRank && ChannelSession.Settings.Currency.TryGetValue(oldItem.CurrencyID, out CurrencyModel currency))
                {
                    newItem.SelectedConsumable = currency;
                    newItem.SelectedLeaderboardType = OverlayLeaderboardTypeV3Enum.Consumable;
                }
                else if (oldItem.LeaderboardType == OverlayLeaderboardListItemTypeEnum.Bits)
                {
                    newItem.SelectedLeaderboardType = OverlayLeaderboardTypeV3Enum.TwitchBits;
                    switch (oldItem.BitsLeaderboardDateRange)
                    {
                        case BitsLeaderboardPeriodEnum.Day: newItem.SelectedTwitchBitsDataRange = OverlayLeaderboardDateRangeV3Enum.Daily; break;
                        case BitsLeaderboardPeriodEnum.Week: newItem.SelectedTwitchBitsDataRange = OverlayLeaderboardDateRangeV3Enum.Weekly; break;
                        case BitsLeaderboardPeriodEnum.Month: newItem.SelectedTwitchBitsDataRange = OverlayLeaderboardDateRangeV3Enum.Monthly; break;
                        case BitsLeaderboardPeriodEnum.Year: newItem.SelectedTwitchBitsDataRange = OverlayLeaderboardDateRangeV3Enum.Yearly; break;
                        case BitsLeaderboardPeriodEnum.All: newItem.SelectedTwitchBitsDataRange = OverlayLeaderboardDateRangeV3Enum.AllTime; break;
                    }
                }
                else
                {
                    newItem.SelectedLeaderboardType = OverlayLeaderboardTypeV3Enum.ViewingTime;
                }

                newItem.ItemAddedAnimation.SelectedAnimatedCSSAnimation = EnumHelper.GetEnumValueFromString<OverlayAnimateCSSAnimationType>(oldItem.Effects.EntranceAnimation.ToString());
                newItem.ItemRemovedAnimation.SelectedAnimatedCSSAnimation = EnumHelper.GetEnumValueFromString<OverlayAnimateCSSAnimationType>(oldItem.Effects.ExitAnimation.ToString());
                result = newItem.GetItem();
                result.OldCustomHTML = oldItem.HTML;
            }
            else if (item.ItemType == OverlayItemModelTypeEnum.ProgressBar)
            {
                OverlayProgressBarItemModel oldItem = (OverlayProgressBarItemModel)item;
                OverlayGoalV3ViewModel newItem = new OverlayGoalV3ViewModel();
                vmResult = newItem;

                newItem.ProgressColor = UpgradeOldOverlayColor(oldItem.ProgressColor);
                newItem.GoalColor = UpgradeOldOverlayColor(oldItem.BackgroundColor);
                newItem.FontColor = UpgradeOldOverlayColor(oldItem.TextColor);
                newItem.FontName = oldItem.TextFont;
                newItem.Width = oldItem.Width.ToString();
                newItem.Height = oldItem.Height.ToString();

                if (oldItem.ProgressBarType == OverlayProgressBarItemTypeEnum.Followers)
                {
                    newItem.FollowAmount = 1;
                }
                else if (oldItem.ProgressBarType == OverlayProgressBarItemTypeEnum.Subscribers)
                {
                    newItem.TwitchSubscriptionTier1Amount = 1;
                    newItem.TwitchSubscriptionTier2Amount = 1;
                    newItem.TwitchSubscriptionTier3Amount = 1;
                    newItem.TrovoSubscriptionTier1Amount = 1;
                    newItem.TrovoSubscriptionTier2Amount = 1;
                    newItem.TrovoSubscriptionTier3Amount = 1;
                    foreach (var membership in newItem.YouTubeMemberships)
                    {
                        membership.Amount = 1;
                    }
                }
                else if (oldItem.ProgressBarType == OverlayProgressBarItemTypeEnum.Donations)
                {
                    newItem.DonationAmount = 1;
                }
                else if (oldItem.ProgressBarType == OverlayProgressBarItemTypeEnum.Bits)
                {
                    newItem.TwitchBitsAmount = 1;
                }
                else if (oldItem.ProgressBarType == OverlayProgressBarItemTypeEnum.Custom)
                {
                    string counterName = oldItem.CurrentAmountCustom.Replace("$", string.Empty).ToLowerInvariant();
                    if (settings.Counters.TryGetValue(counterName, out CounterModel counter))
                    {
                        newItem.SelectedGoalType = OverlayGoalV3Type.Counter;
                        newItem.SelectedCounter = counter;
                    }
                }

                newItem.Segments.Clear();
                newItem.Segments.Add(new OverlayGoalSegmentV3ViewModel(newItem)
                {
                    Amount = oldItem.GoalAmount
                });

                result = newItem.GetItem();
                result.OldCustomHTML = oldItem.HTML;
                ((OverlayGoalV3Model)result).TotalAmount = oldItem.CurrentAmount;
            }
            else if (item.ItemType == OverlayItemModelTypeEnum.StreamBoss)
            {
                OverlayStreamBossItemModel oldItem = (OverlayStreamBossItemModel)item;
                OverlayStreamBossV3ViewModel newItem = new OverlayStreamBossV3ViewModel();
                vmResult = newItem;
                newItem.Height = oldItem.Height.ToString();
                newItem.Width = oldItem.Width.ToString();
                newItem.FontColor = UpgradeOldOverlayColor(oldItem.TextColor);
                newItem.FontName = UpgradeOldOverlayColor(oldItem.TextFont);
                newItem.BorderColor = UpgradeOldOverlayColor(oldItem.BorderColor);
                newItem.HealthColor = UpgradeOldOverlayColor(oldItem.BackgroundColor);
                newItem.DamageColor = UpgradeOldOverlayColor(oldItem.ProgressColor);
                newItem.BaseHealth = oldItem.StartingHealth;
                newItem.DamageOcurredAnimation.SelectedAnimatedCSSAnimation = EnumHelper.GetEnumValueFromString<OverlayAnimateCSSAnimationType>(oldItem.DamageAnimation.ToString());
                newItem.NewBossAnimation.SelectedAnimatedCSSAnimation = EnumHelper.GetEnumValueFromString<OverlayAnimateCSSAnimationType>(oldItem.NewBossAnimation.ToString());
                newItem.FollowAmount = oldItem.FollowBonus;
                newItem.RaidAmount = oldItem.RaidBonus;
                newItem.TwitchSubscriptionTier1Amount = oldItem.SubscriberBonus;
                newItem.TwitchSubscriptionTier2Amount = oldItem.SubscriberBonus;
                newItem.TwitchSubscriptionTier3Amount = oldItem.SubscriberBonus;
                newItem.TrovoSubscriptionTier1Amount = oldItem.SubscriberBonus;
                newItem.TrovoSubscriptionTier2Amount = oldItem.SubscriberBonus;
                newItem.TrovoSubscriptionTier3Amount = oldItem.SubscriberBonus;
                foreach (var membership in newItem.YouTubeMemberships)
                {
                    membership.Amount = oldItem.SubscriberBonus;
                }
                newItem.DonationAmount = oldItem.DonationBonus;
                newItem.TwitchBitsAmount = oldItem.BitsBonus;
                newItem.SelfHealingMultiplier = oldItem.HealingBonus;
                newItem.OverkillBonusHealthMultiplier = oldItem.OverkillBonus;
                newItem.KillBonusHealth = 0;
                newItem.YouTubeSuperChatAmount = 0;
                newItem.TrovoElixirSpellAmount = 0;
                result = newItem.GetItem();
                result.OldCustomHTML = oldItem.HTML;

                if (oldItem.CurrentHealth > 0)
                {
                    ((OverlayStreamBossV3Model)result).CurrentHealth = oldItem.CurrentHealth;
                }
                ((OverlayStreamBossV3Model)result).CurrentBoss = oldItem.CurrentBossID;
                ((OverlayStreamBossV3Model)result).NewBossCommandID = oldItem.StreamBossChangedCommandID;
            }
            else if (item.ItemType == OverlayItemModelTypeEnum.Text)
            {
                OverlayTextItemModel oldItem = (OverlayTextItemModel)item;
                OverlayTextV3ViewModel newItem = new OverlayTextV3ViewModel();
                vmResult = newItem;
                newItem.Text = oldItem.Text;
                newItem.FontColor = UpgradeOldOverlayColor(oldItem.Color);
                newItem.FontName = oldItem.Font;
                newItem.FontSize = oldItem.Size;
                newItem.Bold = oldItem.Bold;
                newItem.Underline = oldItem.Underline;
                newItem.Italics = oldItem.Italic;
                newItem.ShadowColor = UpgradeOldOverlayColor(oldItem.ShadowColor);
                result = newItem.GetItem();
            }
            else if (item.ItemType == OverlayItemModelTypeEnum.Timer)
            {
                OverlayTimerItemModel oldItem = (OverlayTimerItemModel)item;
                OverlayPersistentTimerV3ViewModel newItem = new OverlayPersistentTimerV3ViewModel();
                vmResult = newItem;
                newItem.InitialAmount = oldItem.TotalLength;
                newItem.FontColor = UpgradeOldOverlayColor(oldItem.TextColor);
                newItem.FontName = oldItem.TextFont;
                newItem.FontSize = oldItem.TextSize;
                result = newItem.GetItem();
                ((OverlayPersistentTimerV3Model)result).TimerCompletedCommandID = oldItem.TimerFinishedCommandID;
            }
            else if (item.ItemType == OverlayItemModelTypeEnum.TimerTrain)
            {
                OverlayTimerTrainItemModel oldItem = (OverlayTimerTrainItemModel)item;
                OverlayPersistentTimerV3ViewModel newItem = new OverlayPersistentTimerV3ViewModel();
                vmResult = newItem;
                newItem.FontColor = UpgradeOldOverlayColor(oldItem.TextColor);
                newItem.FontName = oldItem.TextFont;
                newItem.FontSize = oldItem.TextSize;
                newItem.FollowAmount = oldItem.FollowBonus;
                newItem.RaidAmount = oldItem.RaidBonus;
                newItem.TwitchSubscriptionTier1Amount = oldItem.SubscriberBonus;
                newItem.TwitchSubscriptionTier2Amount = oldItem.SubscriberBonus;
                newItem.TwitchSubscriptionTier3Amount = oldItem.SubscriberBonus;
                newItem.TrovoSubscriptionTier1Amount = oldItem.SubscriberBonus;
                newItem.TrovoSubscriptionTier2Amount = oldItem.SubscriberBonus;
                newItem.TrovoSubscriptionTier3Amount = oldItem.SubscriberBonus;
                foreach (var membership in newItem.YouTubeMemberships)
                {
                    membership.Amount = oldItem.SubscriberBonus;
                }
                newItem.DonationAmount = oldItem.DonationBonus;
                newItem.TwitchBitsAmount = oldItem.BitsBonus;
                result = newItem.GetItem();
            }
            else if (item.ItemType == OverlayItemModelTypeEnum.Video)
            {
                OverlayVideoItemModel oldItem = (OverlayVideoItemModel)item;
                OverlayVideoV3ViewModel newItem = new OverlayVideoV3ViewModel();
                vmResult = newItem;
                newItem.FilePath = oldItem.FilePath;
                newItem.Volume = oldItem.Volume;
                newItem.Loop = oldItem.Loop;
                newItem.Width = oldItem.Width.ToString();
                newItem.Height = oldItem.Height.ToString();
                result = newItem.GetItem();
            }
            else if (item.ItemType == OverlayItemModelTypeEnum.WebPage)
            {
                OverlayWebPageItemModel oldItem = (OverlayWebPageItemModel)item;
                OverlayHTMLV3ViewModel newItem = new OverlayHTMLV3ViewModel();
                vmResult = newItem;

                result = newItem.GetItem();

                result.HTML = $"<iframe width={oldItem.Width} height={oldItem.Height} frameBorder=0 src=\"{oldItem.FilePath}\"></iframe>";
            }
            else if (item.ItemType == OverlayItemModelTypeEnum.YouTube)
            {
                OverlayYouTubeItemModel oldItem = (OverlayYouTubeItemModel)item;
                OverlayYouTubeV3ViewModel newItem = new OverlayYouTubeV3ViewModel();
                vmResult = newItem;
                newItem.VideoID = oldItem.FilePath;
                newItem.Volume = oldItem.Volume;
                newItem.StartTime = oldItem.StartTime.ToString();
                newItem.Width = oldItem.Width.ToString();
                newItem.Height = oldItem.Height.ToString();
                result = newItem.GetItem();
            }

            if (result != null)
            {
                if (string.IsNullOrEmpty(result.HTML))
                {
                    result.HTML = OverlayItemV3ModelBase.GetPositionWrappedHTML(vmResult.DefaultHTML);
                }
                else
                {
                    result.HTML = OverlayItemV3ModelBase.GetPositionWrappedHTML(result.HTML);
                }
                result.CSS = OverlayItemV3ModelBase.GetPositionWrappedCSS(vmResult.DefaultCSS);
                result.Javascript = vmResult.DefaultJavascript;

                result.Layer = item.Position.Layer;
                result.XPosition = item.Position.Horizontal;
                result.YPosition = item.Position.Vertical;

                if (item.Position.PositionType == OverlayItemPositionType.Pixel)
                {
                    result.PositionType = OverlayPositionV3Type.Pixel;
                }
                else if (item.Position.PositionType == OverlayItemPositionType.Percentage)
                {
                    result.PositionType = OverlayPositionV3Type.Percentage;
                }
            }

            return result;
        }
#pragma warning restore CS0612 // Type or member is obsolete
    }
}
