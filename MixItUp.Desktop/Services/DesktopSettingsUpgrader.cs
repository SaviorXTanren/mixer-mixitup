using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Interactive;
using MixItUp.Base.ViewModel.Requirement;
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
#pragma warning disable CS0612 // Type or member is obsolete
                    if (!string.IsNullOrEmpty(command.CooldownGroup) && settings.interactiveCooldownGroupsInternal.ContainsKey(command.CooldownGroup))
                    {
                        command.Requirements.Cooldown = new CooldownRequirementViewModel(CooldownTypeEnum.Group, command.CooldownGroup, settings.interactiveCooldownGroupsInternal[command.CooldownGroup]);
                        settings.CooldownGroups[command.CooldownGroup] = settings.interactiveCooldownGroupsInternal[command.CooldownGroup];
                    }
                    else
                    {
                        command.Requirements.Cooldown = new CooldownRequirementViewModel(CooldownTypeEnum.Individual, command.IndividualCooldown);
                    }
#pragma warning restore CS0612 // Type or member is obsolete
                }

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version14Upgrade(int version, string filePath)
        {
            if (version < 14)
            {
                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<LegacyDesktopChannelSettings>(filePath);
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
