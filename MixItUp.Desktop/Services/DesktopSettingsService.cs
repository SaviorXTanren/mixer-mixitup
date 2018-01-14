using Mixer.Base.Model.Channel;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.Desktop.Database;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    internal static class DesktopSettingsUpgrader
    {
        private class LegacyUserDataViewModel : UserDataViewModel
        {
            [DataMember]
            public new int RankPoints { get; set; }

            [DataMember]
            public int CurrencyAmount { get; set; }

            public LegacyUserDataViewModel(DbDataReader dataReader)
                : base(uint.Parse(dataReader["ID"].ToString()), dataReader["UserName"].ToString())
            {
                this.ViewingMinutes = int.Parse(dataReader["ViewingMinutes"].ToString());
                this.RankPoints = int.Parse(dataReader["RankPoints"].ToString());
                this.CurrencyAmount = int.Parse(dataReader["CurrencyAmount"].ToString());
            }
        }

        internal static async Task UpgradeSettingsToLatest(int version, string filePath)
        {
            await DesktopSettingsUpgrader.Version2Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version3Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version4Upgrade(version, filePath);

            DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
            settings.InitializeDB = false;
            await ChannelSession.Services.Settings.Initialize(settings);
            settings.Version = DesktopChannelSettings.LatestVersion;

            await ChannelSession.Services.Settings.Save(settings);
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
                            nAction.ChatText = DesktopSettingsUpgrader.ReplaceSpecialIdentifiersVersion2(nAction.ChatText);
                        }
                        else if (action is CurrencyAction)
                        {
                            CurrencyAction nAction = (CurrencyAction)action;
                            nAction.ChatText = DesktopSettingsUpgrader.ReplaceSpecialIdentifiersVersion2(nAction.ChatText);
                        }
                        else if (action is OBSStudioAction)
                        {
                            OBSStudioAction nAction = (OBSStudioAction)action;
                            nAction.SourceText = DesktopSettingsUpgrader.ReplaceSpecialIdentifiersVersion2(nAction.SourceText);
                        }
                        else if (action is OverlayAction)
                        {
                            OverlayAction nAction = (OverlayAction)action;
                            nAction.ImagePath = DesktopSettingsUpgrader.ReplaceSpecialIdentifiersVersion2(nAction.ImagePath);
                            nAction.Text = DesktopSettingsUpgrader.ReplaceSpecialIdentifiersVersion2(nAction.Text);
                        }
                        else if (action is TextToSpeechAction)
                        {
                            TextToSpeechAction nAction = (TextToSpeechAction)action;
                            nAction.SpeechText = DesktopSettingsUpgrader.ReplaceSpecialIdentifiersVersion2(nAction.SpeechText);
                        }
                        else if (action is XSplitAction)
                        {
                            XSplitAction nAction = (XSplitAction)action;
                            nAction.SourceText = DesktopSettingsUpgrader.ReplaceSpecialIdentifiersVersion2(nAction.SourceText);
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
                LegacyDesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<LegacyDesktopChannelSettings>(filePath);
                settings.InitializeDB = false;
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

        private static async Task Version4Upgrade(int version, string filePath)
        {
            if (version < 4)
            {
                string data = File.ReadAllText(filePath);
                data = data.Replace("MixItUp.Base.Actions.RankAction", "MixItUp.Base.Actions.CurrencyAction");
                data = data.Replace("\"Type\": 13\n", "\"Type\": 1\n");
                File.WriteAllText(filePath, data);

                LegacyDesktopChannelSettings legacySettings = await SerializerHelper.DeserializeFromFile<LegacyDesktopChannelSettings>(filePath);
                legacySettings.InitializeDB = false;

                string dbPath = ((DesktopSettingsService)ChannelSession.Services.Settings).GetDatabaseFilePath(legacySettings);
                List<LegacyUserDataViewModel> legacyUsers = new List<LegacyUserDataViewModel>();
                SQLiteDatabaseWrapper databaseWrapper = new SQLiteDatabaseWrapper(dbPath);
                await databaseWrapper.RunReadCommand("SELECT * FROM Users", (SQLiteDataReader dataReader) =>
                {
                    LegacyUserDataViewModel userData = new LegacyUserDataViewModel(dataReader);
                    legacyUsers.Add(userData);
                });
                File.Copy(DesktopSettingsService.SettingsTemplateDatabaseFileName, dbPath, overwrite: true);

                await ChannelSession.Services.Settings.Initialize(legacySettings);

                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
                settings.InitializeDB = false;
                await ChannelSession.Services.Settings.Initialize(settings);

                UserCurrencyViewModel currency = null;
                if (!string.IsNullOrEmpty(legacySettings.CurrencyAcquisition.Name))
                {
                    currency = legacySettings.CurrencyAcquisition;
                    currency.SpecialIdentifier = "usercurrency";
                    settings.Currencies.Add(legacySettings.CurrencyAcquisition.Name, legacySettings.CurrencyAcquisition);
                }

                UserCurrencyViewModel rank = null;
                if (!string.IsNullOrEmpty(legacySettings.RankAcquisition.Name))
                {
                    rank = legacySettings.RankAcquisition;
                    rank.SpecialIdentifier = "userrank";
                    rank.Ranks = legacySettings.Ranks;
                    rank.RankChangedCommand = legacySettings.RankChangedCommand;
                    settings.Currencies.Add(legacySettings.RankAcquisition.Name, legacySettings.RankAcquisition);
                }

                foreach (LegacyUserDataViewModel user in legacyUsers)
                {
                    settings.UserData[user.ID] = user;
                    if (rank != null) { settings.UserData[user.ID].SetCurrencyAmount(rank, user.RankPoints); }
                    if (currency != null) { settings.UserData[user.ID].SetCurrencyAmount(currency, user.CurrencyAmount); }
                }

                if (currency != null)
                {
                    if (legacySettings.GiveawayCurrencyCost > 0)
                    {
                        settings.GiveawayCurrencyRequirement = new UserCurrencyRequirementViewModel(currency, legacySettings.GiveawayCurrencyCost);
                    }
                    if (legacySettings.GameQueueCurrencyCost > 0)
                    {
                        settings.GameQueueCurrencyRequirement = new UserCurrencyRequirementViewModel(currency, legacySettings.GameQueueCurrencyCost);
                    }
                }

                if (rank != null)
                {
                    if (legacySettings.GiveawayUserRank != null && rank.Ranks.Any(r => r.Name.Equals(legacySettings.GiveawayUserRank)))
                    {
                        settings.GiveawayRankRequirement = new UserCurrencyRequirementViewModel(rank, rank.Ranks.FirstOrDefault(r => r.Name.Equals(legacySettings.GiveawayUserRank)));
                    }
                    if (legacySettings.GameQueueMinimumRank != null)
                    {
                        settings.GameQueueRankRequirement = new UserCurrencyRequirementViewModel(rank, legacySettings.GameQueueMinimumRank);
                    }
                }

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
                            nAction.ChatText = DesktopSettingsUpgrader.ReplaceSpecialIdentifiersVersion4(nAction.ChatText);
                        }
                        else if (action is CurrencyAction)
                        {
                            CurrencyAction nAction = (CurrencyAction)action;
                            nAction.ChatText = DesktopSettingsUpgrader.ReplaceSpecialIdentifiersVersion4(nAction.ChatText);
                        }
                        else if (action is OBSStudioAction)
                        {
                            OBSStudioAction nAction = (OBSStudioAction)action;
                            nAction.SourceText = DesktopSettingsUpgrader.ReplaceSpecialIdentifiersVersion4(nAction.SourceText);
                        }
                        else if (action is OverlayAction)
                        {
                            OverlayAction nAction = (OverlayAction)action;
                            nAction.ImagePath = DesktopSettingsUpgrader.ReplaceSpecialIdentifiersVersion4(nAction.ImagePath);
                            nAction.Text = DesktopSettingsUpgrader.ReplaceSpecialIdentifiersVersion4(nAction.Text);
                        }
                        else if (action is TextToSpeechAction)
                        {
                            TextToSpeechAction nAction = (TextToSpeechAction)action;
                            nAction.SpeechText = DesktopSettingsUpgrader.ReplaceSpecialIdentifiersVersion4(nAction.SpeechText);
                        }
                        else if (action is XSplitAction)
                        {
                            XSplitAction nAction = (XSplitAction)action;
                            nAction.SourceText = DesktopSettingsUpgrader.ReplaceSpecialIdentifiersVersion4(nAction.SourceText);
                        }
                    }
                }

                foreach (ChatCommand command in settings.ChatCommands)
                {
#pragma warning disable CS0612 // Type or member is obsolete
                    if (command.CurrencyCost > 0)
                    {
                        command.CurrencyRequirement = new UserCurrencyRequirementViewModel(currency, command.CurrencyCost);
                    }
#pragma warning restore CS0612 // Type or member is obsolete
                }

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static string ReplaceSpecialIdentifiersVersion2(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                text = Regex.Replace(text, "\\$user(?!(currency|rankname|rankpoints|rank|time|avatar|url))", "$username");
                text = Regex.Replace(text, "\\$arg1user(?!(currency|rankname|rankpoints|rank|time|avatar|url))", "$arg1username");
                text = Regex.Replace(text, "\\$arg1(?!user)", "$arg1string");
            }
            return text;
        }

        private static string ReplaceSpecialIdentifiersVersion4(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                text = Regex.Replace(text, "$userrank", "$userrankname - $userrank");
                text = Regex.Replace(text, "$userrankpoints", "$userrank");
                for (int i = 0; i < 10; i++)
                {
                    text = Regex.Replace(text, "$arg" + i + "usercurrency", "$arg" + i + "usercurrency");
                    text = Regex.Replace(text, "$arg" + i + "string", "$arg" + i + "text");
                }
            }
            return text;
        }
    }

    public class DesktopSettingsService : ISettingsService
    {
        public const string SettingsDirectoryName = "Settings";
        public const string SettingsTemplateDatabaseFileName = "SettingsTemplateDatabase.sqlite";

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
