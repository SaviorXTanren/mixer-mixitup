using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.Serialization;
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
}
