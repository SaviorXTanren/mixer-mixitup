using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
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
            await DesktopSettingsUpgrader.Version9Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version10Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version11Upgrade(version, filePath);

            DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
            settings.InitializeDB = false;
            await ChannelSession.Services.Settings.Initialize(settings);
            settings.Version = DesktopChannelSettings.LatestVersion;

            await ChannelSession.Services.Settings.Save(settings);
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

        private static async Task Version11Upgrade(int version, string filePath)
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
}
