using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Interactive;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using MixItUp.Desktop.Database;
using Newtonsoft.Json.Linq;
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
            await DesktopSettingsUpgrader.Version11Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version12Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version13Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version14Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version15Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version16Upgrade(version, filePath);

            DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
            settings.InitializeDB = false;
            await ChannelSession.Services.Settings.Initialize(settings);
            settings.Version = DesktopChannelSettings.LatestVersion;

            await ChannelSession.Services.Settings.Save(settings);
        }

        private static async Task Version11Upgrade(int version, string filePath)
        {
            if (version < 11)
            {
                string data = File.ReadAllText(filePath);
                data = data.Replace("MixItUp.Base.Actions.OverlayAction, MixItUp.Base", "MixItUp.Desktop.Services.LegacyOverlayAction, MixItUp.Desktop");
                DesktopChannelSettings legacySettings = SerializerHelper.DeserializeFromString<DesktopChannelSettings>(data);
                await ChannelSession.Services.Settings.Initialize(legacySettings);

                Dictionary<Guid, LegacyOverlayAction> legacyOverlayActions = new Dictionary<Guid, LegacyOverlayAction>();

                List<CommandBase> commands = new List<CommandBase>();
                commands.AddRange(legacySettings.ChatCommands);
                commands.AddRange(legacySettings.EventCommands);
                commands.AddRange(legacySettings.InteractiveCommands);
                commands.AddRange(legacySettings.TimerCommands);
                commands.AddRange(legacySettings.ActionGroupCommands);
                commands.AddRange(legacySettings.GameCommands);
                commands.AddRange(legacySettings.RemoteCommands);
                foreach (CommandBase command in commands)
                {
                    foreach (ActionBase action in command.Actions)
                    {
                        if (action is LegacyOverlayAction)
                        {
                            legacyOverlayActions.Add(action.ID, (LegacyOverlayAction)action);
                        }
                    }
                }

                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<LegacyDesktopChannelSettings>(filePath);
                await ChannelSession.Services.Settings.Initialize(settings);

                commands.Clear();
                commands.AddRange(settings.ChatCommands);
                commands.AddRange(settings.EventCommands);
                commands.AddRange(settings.InteractiveCommands);
                commands.AddRange(settings.TimerCommands);
                commands.AddRange(settings.ActionGroupCommands);
                commands.AddRange(settings.GameCommands);
                commands.AddRange(settings.RemoteCommands);
                foreach (CommandBase command in commands)
                {
                    foreach (ActionBase action in command.Actions)
                    {
                        if (action is OverlayAction && legacyOverlayActions.ContainsKey(action.ID))
                        {
                            OverlayAction overlayAction = (OverlayAction)action;
                            LegacyOverlayAction legacyOverlayAction = legacyOverlayActions[action.ID];

                            OverlayEffectEntranceAnimationTypeEnum entrance = OverlayEffectEntranceAnimationTypeEnum.None;
                            OverlayEffectExitAnimationTypeEnum exit = OverlayEffectExitAnimationTypeEnum.None;
                            if (legacyOverlayAction.FadeDuration > 0)
                            {
                                entrance = OverlayEffectEntranceAnimationTypeEnum.FadeIn;
                                exit = OverlayEffectExitAnimationTypeEnum.FadeOut;
                            }

                            if (!string.IsNullOrEmpty(legacyOverlayAction.ImagePath))
                            {
                                overlayAction.Effect = new OverlayImageEffect(legacyOverlayAction.ImagePath, legacyOverlayAction.ImageWidth, legacyOverlayAction.ImageHeight,
                                    entrance, OverlayEffectVisibleAnimationTypeEnum.None, exit, legacyOverlayAction.Duration, legacyOverlayAction.Horizontal, legacyOverlayAction.Vertical);
                            }
                            else if (!string.IsNullOrEmpty(legacyOverlayAction.Text))
                            {
                                overlayAction.Effect = new OverlayTextEffect(legacyOverlayAction.Text, legacyOverlayAction.Color, legacyOverlayAction.FontSize,
                                    entrance, OverlayEffectVisibleAnimationTypeEnum.None, exit, legacyOverlayAction.Duration, legacyOverlayAction.Horizontal, legacyOverlayAction.Vertical);
                            }
                            else if (!string.IsNullOrEmpty(legacyOverlayAction.youtubeVideoID))
                            {
                                overlayAction.Effect = new OverlayYoutubeEffect(legacyOverlayAction.youtubeVideoID, legacyOverlayAction.youtubeStartTime, legacyOverlayAction.VideoWidth,
                                    legacyOverlayAction.VideoHeight, entrance, OverlayEffectVisibleAnimationTypeEnum.None, exit, legacyOverlayAction.Duration, legacyOverlayAction.Horizontal,
                                    legacyOverlayAction.Vertical);
                            }
                            else if (!string.IsNullOrEmpty(legacyOverlayAction.localVideoFilePath))
                            {
                                overlayAction.Effect = new OverlayVideoEffect(legacyOverlayAction.localVideoFilePath, legacyOverlayAction.VideoWidth, legacyOverlayAction.VideoHeight,
                                    entrance, OverlayEffectVisibleAnimationTypeEnum.None, exit, legacyOverlayAction.Duration, legacyOverlayAction.Horizontal, legacyOverlayAction.Vertical);
                            }
                            else if (!string.IsNullOrEmpty(legacyOverlayAction.HTMLText))
                            {
                                overlayAction.Effect = new OverlayHTMLEffect(legacyOverlayAction.HTMLText, entrance, OverlayEffectVisibleAnimationTypeEnum.None, exit, legacyOverlayAction.Duration,
                                    legacyOverlayAction.Horizontal, legacyOverlayAction.Vertical);
                            }
                        }
                        else if (action is CounterAction)
                        {
                            CounterAction counterAction = (CounterAction)action;
                            if (counterAction.SaveToFile)
                            {
                                counterAction.ResetOnLoad = !counterAction.ResetOnLoad;
                            }
                        }
                    }
                }

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version12Upgrade(int version, string filePath)
        {
            if (version < 12)
            {
                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
                SQLiteDatabaseWrapper databaseWrapper = new SQLiteDatabaseWrapper(((DesktopSettingsService)ChannelSession.Services.Settings).GetDatabaseFilePath(settings));

                await databaseWrapper.RunWriteCommand("ALTER TABLE Users ADD COLUMN CustomCommands TEXT");
                await databaseWrapper.RunWriteCommand("ALTER TABLE Users ADD COLUMN Options TEXT");
            }
        }

        private static async Task Version13Upgrade(int version, string filePath)
        {
            if (version < 13)
            {
                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<LegacyDesktopChannelSettings>(filePath);
                await ChannelSession.Services.Settings.Initialize(settings);

                List<PermissionsCommandBase> commands = new List<PermissionsCommandBase>();
                commands.AddRange(settings.ChatCommands);
                commands.AddRange(settings.GameCommands);
                foreach (PermissionsCommandBase command in commands)
                {
#pragma warning disable CS0612 // Type or member is obsolete
                    command.Requirements.Role.MixerRole = command.Requirements.UserRole;
#pragma warning restore CS0612 // Type or member is obsolete
                }

                foreach (InteractiveCommand command in settings.InteractiveCommands)
                {
                    if (command is InteractiveButtonCommand)
                    {
                        InteractiveButtonCommand bCommand = (InteractiveButtonCommand)command;
#pragma warning disable CS0612 // Type or member is obsolete
                        if (!string.IsNullOrEmpty(bCommand.CooldownGroup) && settings.interactiveCooldownGroupsInternal.ContainsKey(bCommand.CooldownGroup))
                        {
                            bCommand.Requirements.Cooldown = new CooldownRequirementViewModel(CooldownTypeEnum.Group, bCommand.CooldownGroup,
                                settings.interactiveCooldownGroupsInternal[bCommand.CooldownGroup]);
                            settings.CooldownGroups[bCommand.CooldownGroup] = settings.interactiveCooldownGroupsInternal[bCommand.CooldownGroup];
                        }
                        else
                        {
                            bCommand.Requirements.Cooldown = new CooldownRequirementViewModel(CooldownTypeEnum.Individual, bCommand.IndividualCooldown);
                        }
#pragma warning restore CS0612 // Type or member is obsolete
                    }
                }

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version14Upgrade(int version, string filePath)
        {
            if (version < 14)
            {
                string data = File.ReadAllText(filePath);
                JObject dataJObj = JObject.Parse(data);
                if (dataJObj.ContainsKey("interactiveCommandsInternal"))
                {
                    JArray interactiveCommands = (JArray)dataJObj["interactiveCommandsInternal"];
                    foreach (JToken interactiveCommand in interactiveCommands)
                    {
                        interactiveCommand["$type"] = "MixItUp.Base.Commands.InteractiveButtonCommand, MixItUp.Base";
                    }
                }
                data = SerializerHelper.SerializeToString(dataJObj);

                DesktopChannelSettings settings = SerializerHelper.DeserializeFromString<DesktopChannelSettings>(data);
                await ChannelSession.Services.Settings.Initialize(settings);

                List<PermissionsCommandBase> permissionCommands = new List<PermissionsCommandBase>();
                permissionCommands.AddRange(settings.ChatCommands);
                permissionCommands.AddRange(settings.GameCommands);
                permissionCommands.AddRange(settings.InteractiveCommands);
                foreach (PermissionsCommandBase command in permissionCommands)
                {
                    command.Requirements.Role.MixerRole = ConvertLegacyRoles(command.Requirements.Role.MixerRole);
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
                    foreach (ActionBase action in command.Actions)
                    {
                        if (action is InteractiveAction)
                        {
                            InteractiveAction iAction = (InteractiveAction)action;
                            iAction.RoleRequirement = ConvertLegacyRoles(iAction.RoleRequirement);
                        }
                    }
                }

                foreach (PreMadeChatCommandSettings preMadeCommandSettings in settings.PreMadeChatCommandSettings)
                {
                    preMadeCommandSettings.Permissions = ConvertLegacyRoles(preMadeCommandSettings.Permissions);
                }

                foreach (InteractiveUserGroupViewModel userGroup in settings.InteractiveUserGroups.Values.SelectMany(ug => ug))
                {
                    userGroup.AssociatedUserRole = ConvertLegacyRoles(userGroup.AssociatedUserRole);
                }

                foreach (GameCommandBase command in settings.GameCommands)
                {
                    if (command is OutcomeGameCommandBase)
                    {
                        OutcomeGameCommandBase outcomeGame = (OutcomeGameCommandBase)command;
                        foreach (GameOutcomeGroup group in outcomeGame.Groups)
                        {
                            group.Role = ConvertLegacyRoles(group.Role);
                        }
                    }
                }

                settings.ModerationFilteredWordsExcempt = ConvertLegacyRoles(settings.ModerationFilteredWordsExcempt);
                settings.ModerationChatTextExcempt = ConvertLegacyRoles(settings.ModerationChatTextExcempt);
                settings.ModerationBlockLinksExcempt = ConvertLegacyRoles(settings.ModerationBlockLinksExcempt);

                IEnumerable<InteractiveCommand> oldInteractiveCommand = settings.InteractiveCommands.ToList();
                settings.InteractiveCommands.Clear();
                foreach (InteractiveCommand command in oldInteractiveCommand)
                {
                    settings.InteractiveCommands.Add(new InteractiveButtonCommand()
                    {
                        ID = command.ID,
                        Name = command.Name,
                        Type = command.Type,
                        Commands = command.Commands,
                        Actions = command.Actions,
                        IsEnabled = command.IsEnabled,
                        IsBasic = command.IsBasic,
                        Unlocked = command.Unlocked,

                        Requirements = command.Requirements,

                        GameID = command.GameID,
                        SceneID = command.SceneID,
                        Control = command.Control,
                        Trigger = EnumHelper.GetEnumValueFromString<InteractiveButtonCommandTriggerType>(command.CommandsString),
                    });
                }

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version15Upgrade(int version, string filePath)
        {
            if (version < 15)
            {
                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
                await ChannelSession.Services.Settings.Initialize(settings);

#pragma warning disable CS0612 // Type or member is obsolete
                EventCommand donationCommand = settings.EventCommands.FirstOrDefault(c => c.MatchesEvent(EnumHelper.GetEnumName(OtherEventTypeEnum.Donation)));
#pragma warning restore CS0612 // Type or member is obsolete
                if (donationCommand != null)
                {
                    string donationCommandJson = SerializerHelper.SerializeToString(donationCommand);

                    EventCommand streamlabsCommand = SerializerHelper.DeserializeFromString<EventCommand>(donationCommandJson);
                    streamlabsCommand.OtherEventType = OtherEventTypeEnum.StreamlabsDonation;
                    settings.EventCommands.Add(streamlabsCommand);

                    EventCommand gawkBoxCommand = SerializerHelper.DeserializeFromString<EventCommand>(donationCommandJson);
                    gawkBoxCommand.OtherEventType = OtherEventTypeEnum.GawkBoxDonation;
                    settings.EventCommands.Add(gawkBoxCommand);

                    settings.EventCommands.Remove(donationCommand);
                }

                await ChannelSession.Services.Settings.Save(settings);
            }
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
                                sAction = StreamingSoftwareAction.CreateSourceTextAction(type, source, visible, text, textPath);
                            }
                            else if (!string.IsNullOrEmpty(url))
                            {
                                sAction = StreamingSoftwareAction.CreateSourceURLAction(type, source, visible, url);
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
