using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    internal static class DesktopSettingsUpgrader
    {
        private static string GetDefaultReferenceFilePath(string software, string source)
        {
            return Path.Combine(ChannelSession.Services.FileService.GetApplicationDirectory(), software, StreamingSoftwareAction.SourceTextFilesDirectoryName, source + ".txt");
        }

        internal static async Task UpgradeSettingsToLatest(int version, string filePath)
        {
            await DesktopSettingsUpgrader.Version29Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version30Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version31Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version32Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version33Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version34Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version35Upgrade(version, filePath);

            DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
            settings.InitializeDB = false;
            await ChannelSession.Services.Settings.Initialize(settings);
            settings.Version = DesktopChannelSettings.LatestVersion;

            await ChannelSession.Services.Settings.Save(settings);
        }

        private static async Task Version29Upgrade(int version, string filePath)
        {
            if (version < 29)
            {
                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
                await ChannelSession.Services.Settings.Initialize(settings);

                settings.SongAddedCommand = CustomCommand.BasicChatCommand("Song Request Added", "$songtitle has been added to the queue", isWhisper: true);
                settings.SongPlayedCommand = CustomCommand.BasicChatCommand("Song Request Played", "Now Playing: $songtitle");

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version30Upgrade(int version, string filePath)
        {
            if (version < 30)
            {
                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
                await ChannelSession.Services.Settings.Initialize(settings);

                settings.GameQueueUserJoinedCommand = CustomCommand.BasicChatCommand("Game Queue Used Joined", "You are #$queueposition in the queue to play.", isWhisper: true);
                settings.GameQueueUserSelectedCommand = CustomCommand.BasicChatCommand("Game Queue Used Selected", "It's time to play @$username! Listen carefully for instructions on how to join...");

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version31Upgrade(int version, string filePath)
        {
            if (version < 31)
            {
                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
                await ChannelSession.Services.Settings.Initialize(settings);

                settings.GiveawayStartedReminderCommand = CustomCommand.BasicChatCommand("Giveaway Started/Reminder", "A giveaway has started for $giveawayitem! Type $giveawaycommand in chat in the next $giveawaytimelimit minute(s) to enter!");

                foreach (GameCommandBase command in settings.GameCommands)
                {
                    if (command is GroupGameCommand)
                    {
                        GroupGameCommand groupCommand = (GroupGameCommand)command;
                        groupCommand.NotEnoughPlayersCommand = CustomCommand.BasicChatCommand("Game Sub-Command", "@$username couldn't get enough users to join in...");
                    }
                    else if (command is DuelGameCommand)
                    {
                        DuelGameCommand duelCommand = (DuelGameCommand)command;
                        duelCommand.NotAcceptedCommand = CustomCommand.BasicChatCommand("Game Sub-Command", "@$targetusername did not respond in time...");
                    }
                }

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version32Upgrade(int version, string filePath)
        {
            if (version < 32)
            {
                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
                await ChannelSession.Services.Settings.Initialize(settings);

                settings.SongRemovedCommand = CustomCommand.BasicChatCommand("Song Request Removed", "$songtitle has been removed from the queue", isWhisper: true);

                foreach (GameCommandBase command in settings.GameCommands.ToList())
                {
                    if (command is BetGameCommand)
                    {
                        settings.GameCommands.Remove(command);
                    }
                }

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version33Upgrade(int version, string filePath)
        {
            if (version < 33)
            {
                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
                await ChannelSession.Services.Settings.Initialize(settings);

#pragma warning disable CS0612 // Type or member is obsolete
                foreach (OverlayWidget widget in settings.overlayWidgetsInternal.ToList())
                {
                    if (widget.Item is OverlayGameStats)
                    {
                        settings.overlayWidgetsInternal.Remove(widget);
                    }
                }
#pragma warning restore CS0612 // Type or member is obsolete

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version34Upgrade(int version, string filePath)
        {
            if (version < 34)
            {
                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
                await ChannelSession.Services.Settings.Initialize(settings);

                string defaultColor = "Default Color";
                if (settings.ChatUserJoinLeaveColorScheme.Equals(defaultColor))
                {
                    settings.ChatUserJoinLeaveColorScheme = ColorSchemes.DefaultColorScheme;
                }
                if (settings.ChatEventAlertsColorScheme.Equals(defaultColor))
                {
                    settings.ChatEventAlertsColorScheme = ColorSchemes.DefaultColorScheme;
                }
                if (settings.ChatInteractiveAlertsColorScheme.Equals(defaultColor))
                {
                    settings.ChatInteractiveAlertsColorScheme = ColorSchemes.DefaultColorScheme;
                }

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version35Upgrade(int version, string filePath)
        {
            if (version < 35)
            {
                DesktopChannelSettings settings = await SerializerHelper.DeserializeFromFile<DesktopChannelSettings>(filePath);
                await ChannelSession.Services.Settings.Initialize(settings);

                foreach (CommandBase command in GetAllCommands(settings))
                {
                    StoreCommandUpgrader.RestructureNewerOverlayActions(command.Actions);
                }

#pragma warning disable CS0612 // Type or member is obsolete
                foreach (OverlayWidget widget in settings.overlayWidgetsInternal)
                {
                    OverlayItemModelBase newItem = StoreCommandUpgrader.ConvertOverlayItem(widget.Item);
                    newItem.ID = widget.Item.ID;
                    newItem.Position = new OverlayItemPositionModel((OverlayItemPositionType)widget.Position.PositionType, widget.Position.Horizontal, widget.Position.Vertical, 0);
                    OverlayWidgetModel newWidget = new OverlayWidgetModel(widget.Name, widget.OverlayName, newItem, 0);
                    settings.OverlayWidgets.Add(newWidget);
                    if (newWidget.SupportsRefreshUpdating && !widget.DontRefresh)
                    {
                        newWidget.RefreshTime = settings.OverlayWidgetRefreshTime;
                    }
                }
                settings.overlayWidgetsInternal.Clear();
#pragma warning restore CS0612 // Type or member is obsolete

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

        private static IEnumerable<CommandBase> GetAllCommands(IChannelSettings settings)
        {
            List<CommandBase> commands = new List<CommandBase>();

            commands.AddRange(settings.ChatCommands);
            commands.AddRange(settings.EventCommands);
            commands.AddRange(settings.InteractiveCommands);
            commands.AddRange(settings.TimerCommands);
            commands.AddRange(settings.ActionGroupCommands);
            commands.AddRange(settings.GameCommands);

            foreach (UserDataViewModel userData in settings.UserData.Values)
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

            foreach (UserCurrencyViewModel currency in settings.Currencies.Values)
            {
                if (currency.RankChangedCommand != null)
                {
                    commands.Add(currency.RankChangedCommand);
                }
            }

            foreach (UserInventoryViewModel inventory in settings.Inventories.Values)
            {
                commands.Add(inventory.ItemsBoughtCommand);
                commands.Add(inventory.ItemsSoldCommand);
            }

#pragma warning disable CS0612 // Type or member is obsolete
            foreach (OverlayWidget widget in settings.overlayWidgetsInternal)
            {
                if (widget.Item is OverlayStreamBoss)
                {
                    OverlayStreamBoss item = ((OverlayStreamBoss)widget.Item);
                    if (item.NewStreamBossCommand != null)
                    {
                        commands.Add(item.NewStreamBossCommand);
                    }
                }
                else if (widget.Item is OverlayProgressBar)
                {
                    OverlayProgressBar item = ((OverlayProgressBar)widget.Item);
                    if (item.GoalReachedCommand != null)
                    {
                        commands.Add(item.GoalReachedCommand);
                    }
                }
                else if (widget.Item is OverlayTimer)
                {
                    OverlayTimer item = ((OverlayTimer)widget.Item);
                    if (item.TimerCompleteCommand != null)
                    {
                        commands.Add(item.TimerCompleteCommand);
                    }
                }
            }
#pragma warning restore CS0612 // Type or member is obsolete

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
            commands.Add(settings.SongAddedCommand);
            commands.Add(settings.SongRemovedCommand);
            commands.Add(settings.SongPlayedCommand);

            return commands.Where(c => c != null);
        }
    }

    [DataContract]
    public class LegacyDesktopChannelSettings : DesktopChannelSettings
    {

    }
}
