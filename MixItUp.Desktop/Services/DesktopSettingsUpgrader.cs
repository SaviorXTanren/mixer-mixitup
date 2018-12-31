using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.Desktop.Database;
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
        private static string OBSStudioReferenceTextFilesDirectory = "OBS";
        private static string XSplitReferenceTextFilesDirectory = "XSplit";
        private static string StreamlabsOBSStudioReferenceTextFilesDirectory = "StreamlabsOBS";

        private static string GetDefaultReferenceFilePath(string software, string source)
        {
            return Path.Combine(ChannelSession.Services.FileService.GetApplicationDirectory(), software, StreamingSoftwareAction.SourceTextFilesDirectoryName, source + ".txt");
        }

        private class LegacyUserDataViewModel : UserDataViewModel
        {
            [DataMember]
            public int RankPoints { get; set; }

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
            await DesktopSettingsUpgrader.Version16Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version17Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version18Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version19Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version20Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version21Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version22Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version23Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version24Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version25Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version26Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version27Upgrade(version, filePath);

            DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
            settings.InitializeDB = false;
            await ChannelSession.Services.Settings.Initialize(settings);
            settings.Version = DesktopChannelSettings.LatestVersion;

            await ChannelSession.Services.Settings.Save(settings);
        }

        private static async Task Version16Upgrade(int version, string filePath)
        {
            if (version < 16)
            {
                string data = File.ReadAllText(filePath);
                DesktopChannelSettings settings = SerializerHelper.DeserializeFromString<DesktopChannelSettings>(data);
                await ChannelSession.Services.Settings.Initialize(settings);

                List<CommandBase> commands = new List<CommandBase>();
                commands.AddRange(settings.ChatCommands);
                commands.AddRange(settings.EventCommands);
                commands.AddRange(settings.InteractiveCommands);
                commands.AddRange(settings.TimerCommands);
                commands.AddRange(settings.ActionGroupCommands);
                commands.AddRange(settings.GameCommands);
                commands.AddRange(settings.RemoteCommands);
                foreach (CommandBase command in commands)
                {
                    for (int i = 0; i < command.Actions.Count; i++)
                    {
                        ActionBase action = command.Actions[i];
#pragma warning disable CS0612 // Type or member is obsolete
                        if (action is OBSStudioAction || action is XSplitAction || action is StreamlabsOBSAction)
                        {
                            StreamingSoftwareTypeEnum type = StreamingSoftwareTypeEnum.OBSStudio;
                            string scene = null;
                            string source = null;
                            bool visible = false;
                            string text = null;
                            string textPath = null;
                            string url = null;
                            StreamingSourceDimensions dimensions = null;

                            if (action is OBSStudioAction)
                            {
                                type = StreamingSoftwareTypeEnum.OBSStudio;
                                OBSStudioAction obsAction = (OBSStudioAction)action;
                                scene = obsAction.SceneName;
                                source = obsAction.SourceName;
                                visible = obsAction.SourceVisible;
                                text = obsAction.SourceText;
                                url = obsAction.SourceURL;
                                dimensions = obsAction.SourceDimensions;
                                if (!string.IsNullOrEmpty(source)) { textPath = GetDefaultReferenceFilePath(OBSStudioReferenceTextFilesDirectory, source); }
                            }
                            else if (action is XSplitAction)
                            {
                                type = StreamingSoftwareTypeEnum.XSplit;
                                XSplitAction xsplitAction = (XSplitAction)action;
                                scene = xsplitAction.SceneName;
                                source = xsplitAction.SourceName;
                                visible = xsplitAction.SourceVisible;
                                text = xsplitAction.SourceText;
                                url = xsplitAction.SourceURL;
                                if (!string.IsNullOrEmpty(source)) { textPath = GetDefaultReferenceFilePath(XSplitReferenceTextFilesDirectory, source); }
                            }
                            else if (action is StreamlabsOBSAction)
                            {
                                type = StreamingSoftwareTypeEnum.StreamlabsOBS;
                                StreamlabsOBSAction slobsAction = (StreamlabsOBSAction)action;
                                scene = slobsAction.SceneName;
                                source = slobsAction.SourceName;
                                visible = slobsAction.SourceVisible;
                                text = slobsAction.SourceText;
                                if (!string.IsNullOrEmpty(source)) { textPath = GetDefaultReferenceFilePath(StreamlabsOBSStudioReferenceTextFilesDirectory, source); }
                            }
#pragma warning restore CS0612 // Type or member is obsolete

                            StreamingSoftwareAction sAction = null;
                            if (!string.IsNullOrEmpty(scene))
                            {
                                sAction = StreamingSoftwareAction.CreateSceneAction(type, scene);
                            }
                            else if (!string.IsNullOrEmpty(text))
                            {
                                sAction = StreamingSoftwareAction.CreateTextSourceAction(type, source, visible, text, textPath);
                            }
                            else if (!string.IsNullOrEmpty(url))
                            {
                                sAction = StreamingSoftwareAction.CreateWebBrowserSourceAction(type, source, visible, url);
                            }
                            else if (dimensions != null)
                            {
                                sAction = StreamingSoftwareAction.CreateSourceDimensionsAction(type, source, visible, dimensions);
                            }
                            else
                            {
                                sAction = StreamingSoftwareAction.CreateSourceVisibilityAction(type, source, visible);
                            }

                            command.Actions[i] = sAction;
                        }
                    }
                }

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version17Upgrade(int version, string filePath)
        {
            if (version < 17)
            {
                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
                await ChannelSession.Services.Settings.Initialize(settings);
                settings.GiveawayTimer = Math.Max(settings.GiveawayTimer / 60, 1);
                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version18Upgrade(int version, string filePath)
        {
            if (version < 18)
            {
                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
                await ChannelSession.Services.Settings.Initialize(settings);

                List<CommandBase> commands = new List<CommandBase>();
                commands.AddRange(settings.ChatCommands);
                commands.AddRange(settings.EventCommands);
                commands.AddRange(settings.InteractiveCommands);
                commands.AddRange(settings.TimerCommands);
                commands.AddRange(settings.ActionGroupCommands);
                commands.AddRange(settings.GameCommands);
                commands.AddRange(settings.RemoteCommands);
                foreach (CommandBase command in commands)
                {
                    StoreCommandUpgrader.SeperateChatFromCurrencyActions(command.Actions);
                }

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version19Upgrade(int version, string filePath)
        {
            if (version < 19)
            {
                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
                await ChannelSession.Services.Settings.Initialize(settings);

                PreMadeChatCommandSettings cSetting = settings.PreMadeChatCommandSettings.FirstOrDefault(c => c.Name.Equals("Ban"));
                if (cSetting != null)
                {
                    cSetting.IsEnabled = false;
                }

                List<CommandBase> commands = new List<CommandBase>();
                commands.AddRange(settings.ChatCommands);
                commands.AddRange(settings.EventCommands);
                commands.AddRange(settings.InteractiveCommands);
                commands.AddRange(settings.TimerCommands);
                commands.AddRange(settings.ActionGroupCommands);
                commands.AddRange(settings.GameCommands);
                commands.AddRange(settings.RemoteCommands);
                foreach (CommandBase command in commands)
                {
                    StoreCommandUpgrader.ChangeWaitActionsToUseSpecialIdentifiers(command.Actions);
                }

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version20Upgrade(int version, string filePath)
        {
            if (version < 20)
            {
                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
                await ChannelSession.Services.Settings.Initialize(settings);

                settings.ChatCommands.RemoveAll(c => c == null);
                settings.EventCommands.RemoveAll(c => c == null);
                settings.InteractiveCommands.RemoveAll(c => c == null);
                settings.TimerCommands.RemoveAll(c => c == null);
                settings.ActionGroupCommands.RemoveAll(c => c == null);
                settings.GameCommands.RemoveAll(c => c == null);
                settings.RemoteCommands.RemoveAll(c => c == null);

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version21Upgrade(int version, string filePath)
        {
            if (version < 21)
            {
                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
                await ChannelSession.Services.Settings.Initialize(settings);

                settings.GameCommands.Clear();

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version22Upgrade(int version, string filePath)
        {
            if (version < 22)
            {
                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
                await ChannelSession.Services.Settings.Initialize(settings);

                PreMadeChatCommandSettings cSetting = settings.PreMadeChatCommandSettings.FirstOrDefault(c => c.Name.Equals("Ban"));
                if (cSetting != null)
                {
                    cSetting.IsEnabled = false;
                }

                List<CommandBase> commands = new List<CommandBase>();
                commands.AddRange(settings.ChatCommands);
                commands.AddRange(settings.EventCommands);
                commands.AddRange(settings.InteractiveCommands);
                commands.AddRange(settings.TimerCommands);
                commands.AddRange(settings.ActionGroupCommands);
                commands.AddRange(settings.GameCommands);
                commands.AddRange(settings.RemoteCommands);
                foreach (CommandBase command in commands)
                {
                    StoreCommandUpgrader.ChangeCounterActionsToUseSpecialIdentifiers(command.Actions);
                }

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version23Upgrade(int version, string filePath)
        {
            if (version < 23)
            {
                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
                await ChannelSession.Services.Settings.Initialize(settings);

                settings.GiveawayMaximumEntries = 1;
                settings.GiveawayUserJoinedCommand = CustomCommand.BasicChatCommand("Giveaway User Joined", "You have been entered into the giveaway, stay tuned to see who wins!", isWhisper: true);
                settings.GiveawayWinnerSelectedCommand = CustomCommand.BasicChatCommand("Giveaway Winner Selected", "Congratulations @$username, you won! Type \"!claim\" in chat in the next 60 seconds to claim your prize!", isWhisper: true);

                settings.ModerationStrike1Command = CustomCommand.BasicChatCommand("Moderation Strike 1", "You have received a moderation strike, you currently have $usermoderationstrikes strike(s)", isWhisper: true);
                settings.ModerationStrike2Command = CustomCommand.BasicChatCommand("Moderation Strike 2", "You have received a moderation strike, you currently have $usermoderationstrikes strike(s)", isWhisper: true);
                settings.ModerationStrike3Command = CustomCommand.BasicChatCommand("Moderation Strike 3", "You have received a moderation strike, you currently have $usermoderationstrikes strike(s)", isWhisper: true);

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version24Upgrade(int version, string filePath)
        {
            if (version < 24)
            {
                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
                await ChannelSession.Services.Settings.Initialize(settings);

                List<CommandBase> commands = new List<CommandBase>();
                commands.AddRange(settings.ChatCommands);
                commands.AddRange(settings.EventCommands);
                commands.AddRange(settings.InteractiveCommands);
                commands.AddRange(settings.TimerCommands);
                commands.AddRange(settings.ActionGroupCommands);
                commands.AddRange(settings.GameCommands);
                commands.AddRange(settings.RemoteCommands);
                foreach (CommandBase command in commands)
                {
                    StoreCommandUpgrader.RestructureNewOverlayActions(command.Actions);
                }

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version25Upgrade(int version, string filePath)
        {
            if (version < 25)
            {
                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
                await ChannelSession.Services.Settings.Initialize(settings);

                settings.OverlayWidgetRefreshTime = 5;

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version26Upgrade(int version, string filePath)
        {
            if (version < 26)
            {
                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
                await ChannelSession.Services.Settings.Initialize(settings);

                settings.ModerationFilteredWordsApplyStrikes = true;
                settings.ModerationChatTextApplyStrikes = true;
                settings.ModerationBlockLinksApplyStrikes = true;

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version27Upgrade(int version, string filePath)
        {
            if (version < 27)
            {
                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
                await ChannelSession.Services.Settings.Initialize(settings);

                SQLiteDatabaseWrapper databaseWrapper = new SQLiteDatabaseWrapper(((DesktopSettingsService)ChannelSession.Services.Settings).GetDatabaseFilePath(settings));

                await databaseWrapper.RunWriteCommand("ALTER TABLE Users ADD COLUMN InventoryAmounts TEXT");
                await databaseWrapper.RunWriteCommand("UPDATE Users SET InventoryAmounts = '{ }'");

                foreach (UserCurrencyViewModel currency in settings.Currencies.Values)
                {
                    currency.ModeratorBonus = currency.SubscriberBonus;
                }

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static MixerRoleEnum ConvertLegacyRoles(MixerRoleEnum legacyRole)
        {
            int legacyRoleID = (int)legacyRole;
            if ((int)MixerRoleEnum.Custom == legacyRoleID)
            {
                return MixerRoleEnum.Custom;
            }
            else
            {
                return (MixerRoleEnum)(legacyRoleID * 10);
            }
        }
    }

    [DataContract]
    public class LegacyDesktopChannelSettings : DesktopChannelSettings
    {

    }

    [DataContract]
    public class LegacyOverlayAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return LegacyOverlayAction.asyncSemaphore; } }

        [DataMember]
        public string ImagePath;
        [DataMember]
        public int ImageWidth;
        [DataMember]
        public int ImageHeight;

        [DataMember]
        public string Text;
        [DataMember]
        public string Color;
        [DataMember]
        public int FontSize;

        [DataMember]
        public int VideoWidth;
        [DataMember]
        public int VideoHeight;

        [DataMember]
        public string youtubeVideoID;
        [DataMember]
        public int youtubeStartTime;

        [DataMember]
        public string localVideoFilePath;

        [DataMember]
        public string HTMLText;

        [DataMember]
        public double Duration;
        [DataMember]
        public int FadeDuration;
        [DataMember]
        public int Horizontal;
        [DataMember]
        public int Vertical;

        public LegacyOverlayAction() : base(ActionTypeEnum.Overlay)
        {
            this.VideoHeight = 315;
            this.VideoWidth = 560;
        }

        protected override Task PerformInternal(UserViewModel user, IEnumerable<string> arguments) { return Task.FromResult(0); }
    }

    public enum LegacyMixerRoleEnum
    {
        Banned = 0,
        User = 1,
        Pro = 2,
        Follower = 3,
        Subscriber = 4,
        Mod = 5,
        Staff = 6,
        Streamer = 7,

        Custom = 99,
    }
}
