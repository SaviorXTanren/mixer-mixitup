using Mixer.Base.Model.Channel;
using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
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
            await DesktopSettingsUpgrader.Version7Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version8Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version9Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version10Upgrade(version, filePath);

            DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
            settings.InitializeDB = false;
            await ChannelSession.Services.Settings.Initialize(settings);
            settings.Version = DesktopChannelSettings.LatestVersion;

            await ChannelSession.Services.Settings.Save(settings);
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

        private static async Task Version9Upgrade(int version, string filePath)
        {
            if (version < 9)
            {
                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<LegacyDesktopChannelSettings>(filePath);
                await ChannelSession.Services.Settings.Initialize(settings);

                List<PermissionsCommandBase> commands = new List<PermissionsCommandBase>();
                commands.AddRange(settings.ChatCommands);
                commands.AddRange(settings.GameCommands);
                foreach (PermissionsCommandBase command in commands)
                {
#pragma warning disable CS0612 // Type or member is obsolete
                    command.Requirements.Cooldown.Amount = command.Cooldown;
#pragma warning restore CS0612 // Type or member is obsolete
                }

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version10Upgrade(int version, string filePath)
        {
            if (version < 10)
            {
                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<LegacyDesktopChannelSettings>(filePath);
                await ChannelSession.Services.Settings.Initialize(settings);

                UserCurrencyViewModel currency = settings.Currencies.Values.FirstOrDefault(c => !c.IsRank);
                if (currency == null)
                {
                    currency = settings.Currencies.Values.FirstOrDefault();
                }

                List<PermissionsCommandBase> commands = new List<PermissionsCommandBase>();
                commands.AddRange(settings.GameCommands);
                foreach (PermissionsCommandBase command in commands)
                {
#pragma warning disable CS0612 // Type or member is obsolete
                    if (command.Requirements.Currency == null)
                    {
                        command.Requirements.Currency = command.CurrencyRequirement;
                        if (command.Requirements.Currency == null)
                        {
                            command.Requirements.Currency = new CurrencyRequirementViewModel(currency, 1, 1);
                        }
                    }
                    if (command.Requirements.Rank == null)
                    {
                        command.Requirements.Rank = command.RankRequirement;
                    }
#pragma warning restore CS0612 // Type or member is obsolete
                }

                await ChannelSession.Services.Settings.Save(settings);
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
                        UserModel onlineUser = await ChannelSession.Connection.GetUser(duplicateGroup.Key);
                        if (onlineUser != null)
                        {
                            List<UserDataViewModel> dupeUsers = new List<UserDataViewModel>(duplicateGroup);
                            if (dupeUsers.Count > 0)
                            {
                                UserDataViewModel solidUser = dupeUsers.FirstOrDefault(u => u.ID == onlineUser.id);
                                if (solidUser != null)
                                {
                                    dupeUsers.Remove(solidUser);
                                    foreach (UserDataViewModel dupeUser in dupeUsers)
                                    {
                                        solidUser.ViewingMinutes += dupeUser.ViewingMinutes;
                                        foreach (var kvp in dupeUser.CurrencyAmounts)
                                        {
                                            solidUser.AddCurrencyAmount(kvp.Key, kvp.Value.Amount);
                                        }
                                    }

                                    foreach (UserDataViewModel dupeUser in dupeUsers)
                                    {
                                        ChannelSession.Settings.UserData.Remove(dupeUser.ID);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var dupeUser in duplicateGroup)
                        {
                            ChannelSession.Settings.UserData.Remove(dupeUser.ID);
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
