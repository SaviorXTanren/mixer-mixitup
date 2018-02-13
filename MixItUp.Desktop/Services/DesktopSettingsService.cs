using Mixer.Base.Model.Channel;
using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
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
            await DesktopSettingsUpgrader.Version5Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version6Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version7Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version8Upgrade(version, filePath);

            DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
            settings.InitializeDB = false;
            await ChannelSession.Services.Settings.Initialize(settings);
            settings.Version = DesktopChannelSettings.LatestVersion;

            await ChannelSession.Services.Settings.Save(settings);
        }

        private static async Task Version5Upgrade(int version, string filePath)
        {
            if (version < 5)
            {
                LegacyDesktopChannelSettings legacySettings = await SerializerHelper.DeserializeFromFile<LegacyDesktopChannelSettings>(filePath);

                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
                await ChannelSession.Services.Settings.Initialize(settings);
                foreach (string quote in legacySettings.quotesInternal)
                {
                    settings.UserQuotes.Add(new UserQuoteViewModel(quote));
                }

                List<CommandBase> commands = new List<CommandBase>();
                commands.AddRange(settings.ChatCommands);
                commands.AddRange(settings.InteractiveCommands);
                commands.AddRange(settings.EventCommands);
                commands.AddRange(settings.TimerCommands);

                UserCurrencyViewModel currency = settings.Currencies.Values.FirstOrDefault(c => !c.IsRank);
                if (currency == null)
                {
                    currency = settings.Currencies.Values.FirstOrDefault();
                }

                foreach (CommandBase command in commands)
                {
                    foreach (ActionBase action in command.Actions)
                    {
                        if (action is InteractiveAction)
                        {
                            InteractiveAction nAction = (InteractiveAction)action;
#pragma warning disable CS0612 // Type or member is obsolete
                            if (nAction.AddUserToGroup)
                            {
                                nAction.InteractiveType = InteractiveActionTypeEnum.MoveUserToGroup;
                            }
                            else
                            {
                                nAction.InteractiveType = InteractiveActionTypeEnum.MoveGroupToScene;
                            }
                            nAction.SceneID = nAction.MoveGroupToScene;
#pragma warning restore CS0612 // Type or member is obsolete
                        }

                        if (currency != null)
                        {
                            if (action is ChatAction)
                            {
                                ChatAction nAction = (ChatAction)action;
                                nAction.ChatText = nAction.ChatText.Replace("$usercurrencyname", currency.Name);
                            }
                            else if (action is CurrencyAction)
                            {
                                CurrencyAction nAction = (CurrencyAction)action;
                                nAction.ChatText = nAction.ChatText.Replace("$usercurrencyname", currency.Name);
                            }
                            else if (action is OBSStudioAction)
                            {
                                OBSStudioAction nAction = (OBSStudioAction)action;
                                if (!string.IsNullOrEmpty(nAction.SourceText))
                                {
                                    nAction.SourceText = nAction.SourceText.Replace("$usercurrencyname", currency.Name);
                                }
                            }
                            else if (action is OverlayAction)
                            {
                                OverlayAction nAction = (OverlayAction)action;
                                if (!string.IsNullOrEmpty(nAction.Text))
                                {
                                    nAction.Text = nAction.Text.Replace("$usercurrencyname", currency.Name);
                                }
                            }
                            else if (action is TextToSpeechAction)
                            {
                                TextToSpeechAction nAction = (TextToSpeechAction)action;
                                nAction.SpeechText = nAction.SpeechText.Replace("$usercurrencyname", currency.Name);
                            }
                            else if (action is XSplitAction)
                            {
                                XSplitAction nAction = (XSplitAction)action;
                                if (!string.IsNullOrEmpty(nAction.SourceText))
                                {
                                    nAction.SourceText = nAction.SourceText.Replace("$usercurrencyname", currency.Name);
                                }
                            }
                        }
                    }
                }

                foreach (GameCommandBase game in settings.GameCommands)
                {
                    if (game is IndividualProbabilityGameCommand)
                    {
                        IndividualProbabilityGameCommand individualGame = (IndividualProbabilityGameCommand)game;
                        DesktopSettingsUpgrader.SetAllGameChatActionsToWhispers(individualGame.UserJoinedCommand);
                        DesktopSettingsUpgrader.SetAllGameChatActionsToWhispers(individualGame.LoseLeftoverCommand);
                        foreach (GameOutcome outcome in individualGame.Outcomes)
                        {
                            DesktopSettingsUpgrader.SetAllGameChatActionsToWhispers(outcome.ResultCommand);
                        }
                    }
                    else if (game is OnlyOneWinnerGameCommand)
                    {
                        OnlyOneWinnerGameCommand oneWinnerGame = (OnlyOneWinnerGameCommand)game;
                        DesktopSettingsUpgrader.SetAllGameChatActionsToWhispers(oneWinnerGame.UserJoinedCommand);
                    }
                }

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version6Upgrade(int version, string filePath)
        {
            if (version < 6)
            {
                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<LegacyDesktopChannelSettings>(filePath);

                List<LegacyUserDataViewModel> legacyUsers = new List<LegacyUserDataViewModel>();
                if (settings.IsStreamer)
                {
                    string dbPath = ((DesktopSettingsService)ChannelSession.Services.Settings).GetDatabaseFilePath(settings);
                    SQLiteDatabaseWrapper databaseWrapper = new SQLiteDatabaseWrapper(dbPath);
                    await databaseWrapper.RunWriteCommand("DELETE FROM Users WHERE UserName IS NULL");
                }

                await ChannelSession.Services.Settings.Initialize(settings);

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version7Upgrade(int version, string filePath)
        {
            if (version < 7)
            {
                LegacyDesktopChannelSettings legacySettings = await SerializerHelper.DeserializeFromFile<LegacyDesktopChannelSettings>(filePath);

                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<LegacyDesktopChannelSettings>(filePath);
                await ChannelSession.Services.Settings.Initialize(settings);

                settings.ModerationUseCommunityFilteredWords = legacySettings.ModerationUseCommunityBannedWords;

                settings.ModerationFilteredWordsExcempt = legacySettings.ModerationIncludeModerators ? UserRole.Streamer : UserRole.Mod;
                settings.ModerationChatTextExcempt = legacySettings.ModerationIncludeModerators ? UserRole.Streamer : UserRole.Mod;
                settings.ModerationBlockLinksExcempt = legacySettings.ModerationIncludeModerators ? UserRole.Streamer : UserRole.Mod;

                foreach (string filteredWord in settings.BannedWords)
                {
                    settings.FilteredWords.Add(filteredWord);
                }
                settings.BannedWords.Clear();

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version8Upgrade(int version, string filePath)
        {
            if (version < 8)
            {
                LegacyDesktopChannelSettings legacySettings = await SerializerHelper.DeserializeFromFile<LegacyDesktopChannelSettings>(filePath);

                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<LegacyDesktopChannelSettings>(filePath);
                await ChannelSession.Services.Settings.Initialize(settings);

                PreMadeChatCommandSettings followCommand = settings.PreMadeChatCommandSettings.FirstOrDefault(c => c.Name.Equals("Follow Age"));
                if (followCommand != null)
                {
                    followCommand.Permissions = UserRole.User;
                }

                PreMadeChatCommandSettings subscribeCommand = settings.PreMadeChatCommandSettings.FirstOrDefault(c => c.Name.Equals("Subscribe Age"));
                if (subscribeCommand != null)
                {
                    subscribeCommand.Permissions = UserRole.User;
                }

                if (legacySettings.GameQueueMustFollow)
                {
                    settings.GameQueueRequirements.UserRole = UserRole.Follower;
                }
                settings.GameQueueRequirements.Currency = legacySettings.GameQueueCurrencyRequirement;
                settings.GameQueueRequirements.Rank = legacySettings.GameQueueRankRequirement;

                settings.GiveawayRequirements.UserRole = legacySettings.GiveawayUserRole;
                settings.GiveawayRequirements.Currency = legacySettings.GiveawayCurrencyRequirement;
                settings.GiveawayRequirements.Rank = legacySettings.GiveawayRankRequirement;

                List<PermissionsCommandBase> commands = new List<PermissionsCommandBase>();
                commands.AddRange(settings.ChatCommands);
                commands.AddRange(settings.GameCommands);
                foreach (PermissionsCommandBase command in commands)
                {
#pragma warning disable CS0612 // Type or member is obsolete
                    command.Requirements.UserRole = command.Permissions;
                    command.Requirements.Currency = command.CurrencyRequirement;
                    command.Requirements.Rank = command.RankRequirement;
#pragma warning restore CS0612 // Type or member is obsolete
                }

                foreach (GameCommandBase command in settings.GameCommands)
                {
#pragma warning disable CS0612 // Type or member is obsolete
                    command.Requirements.Currency.RequirementType = command.CurrencyRequirementType;
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

        private static string ReplaceSpecialIdentifiersVersion4(string text, UserCurrencyViewModel currency, UserCurrencyViewModel rank)
        {
            if (!string.IsNullOrEmpty(text))
            {
                text = text.Replace("$userrank ", "$userrankname - $userrankpoints ");

                if (currency != null)
                {
                    text = text.Replace("$usercurrency", "$" + currency.UserAmountSpecialIdentifier);
                }

                if (rank != null)
                {
                    text = text.Replace("$userrankpoints", "$" + rank.UserAmountSpecialIdentifier);
                    text = text.Replace("$userrankname", "$" + rank.UserRankNameSpecialIdentifier);
                }

                for (int i = 0; i < 10; i++)
                {
                    text = text.Replace("$arg" + i + "string", "$arg" + i + "text");

                    text = text.Replace("$arg" + i + "userrank", "$arg" + i + "userrankname - " + "$arg" + i + "userrankpoints");

                    if (currency != null)
                    {
                        text = text.Replace("$arg" + i + "usercurrency", "$arg" + i + currency.UserAmountSpecialIdentifier);
                    }

                    if (rank != null)
                    {
                        text = text.Replace("$arg" + i + "userrankpoints", "$arg" + i + rank.UserAmountSpecialIdentifier);
                        text = text.Replace("$arg" + i + "userrankname", "$arg" + i + rank.UserRankNameSpecialIdentifier);
                    }
                }
            }
            return text;
        }

        private static void SetAllGameChatActionsToWhispers(CustomCommand command)
        {
            if (command != null)
            {
                foreach (ActionBase action in command.Actions)
                {
                    if (action is ChatAction)
                    {
                        ChatAction cAction = (ChatAction)action;
                        cAction.IsWhisper = true;
                        cAction.SendAsStreamer = false;
                    }
                }
            }
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

            await this.CleanUpData(desktopSettings);
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

        public async Task CleanUpData(IChannelSettings settings)
        {
            if (ChannelSession.Connection != null)
            {
                var duplicateGroups = settings.UserData.Values.GroupBy(u => u.UserName).Where(g => g.Count() > 1);
                foreach (var duplicateGroup in duplicateGroups)
                {
                    if (!string.IsNullOrEmpty(duplicateGroup.Key))
                    {
                        UserModel realUser = await ChannelSession.Connection.GetUser(duplicateGroup.Key);
                        if (realUser != null)
                        {
                            UserDataViewModel correctUserData = duplicateGroup.FirstOrDefault(u => u.ID.Equals(realUser.id));
                            if (correctUserData != null)
                            {
                                foreach (var possibleDupeUser in duplicateGroup)
                                {
                                    if (realUser.id != possibleDupeUser.ID)
                                    {
                                        correctUserData.ViewingMinutes += possibleDupeUser.ViewingMinutes;
                                        foreach (var currencyData in possibleDupeUser.CurrencyAmounts)
                                        {
                                            correctUserData.AddCurrencyAmount(currencyData.Key, currencyData.Value.Amount);
                                        }
                                        ChannelSession.Settings.UserData.Remove(possibleDupeUser.ID);
                                    }
                                }
                            }
                        }
                    }
                }
            }
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
