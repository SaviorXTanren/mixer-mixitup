using Mixer.Base.Clients;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.MixPlay;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Model.User;
using MixItUp.Base.Remote.Models;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
#pragma warning disable CS0612 // Type or member is obsolete
    internal static class SettingsV1Upgrader
    {
        internal static async Task<SettingsV1Model> UpgradeSettingsToLatest(string filePath)
        {
            await SettingsV1Upgrader.Version39Upgrade(filePath);

            SettingsV1Model settings = await SerializerHelper.DeserializeFromFile<SettingsV1Model>(filePath, ignoreErrors: true);
            await settings.LoadUserData();
            settings.Version = SettingsV1Model.LatestVersion;

            await SerializerHelper.SerializeToFile<SettingsV1Model>(filePath, settings);

            return settings;
        }

        public static async Task Version39Upgrade(string filePath)
        {
            SettingsV1Model oldSettings = await SerializerHelper.DeserializeFromFile<SettingsV1Model>(filePath, ignoreErrors: true);

            SettingsV2Model newSettings = await SerializerHelper.DeserializeFromFile<SettingsV2Model>(filePath, ignoreErrors: true);
            await ChannelSession.Services.Settings.Initialize(newSettings);

            newSettings.MixerUserOAuthToken = oldSettings.OAuthToken;
            newSettings.MixerBotOAuthToken = oldSettings.BotOAuthToken;
            if (oldSettings.Channel != null)
            {
                newSettings.Name = oldSettings.Channel.token;
                newSettings.MixerChannelID = oldSettings.Channel.id;
            }
            newSettings.TelemetryUserID = oldSettings.TelemetryUserId;

            newSettings.RemoteProfileBoards = new Dictionary<Guid, RemoteProfileBoardsModel>(oldSettings.remoteProfileBoardsInternal);
            newSettings.FilteredWords = new List<string>(oldSettings.filteredWordsInternal);
            newSettings.BannedWords = new List<string>(oldSettings.bannedWordsInternal);
            newSettings.MixPlayUserGroups = new Dictionary<uint, List<MixPlayUserGroupModel>>(oldSettings.mixPlayUserGroupsInternal);
            newSettings.OverlayWidgets = new List<OverlayWidgetModel>(oldSettings.overlayWidgetModelsInternal);
            newSettings.Currencies = oldSettings.Currencies;
            newSettings.Inventories = oldSettings.Inventories;
            newSettings.CooldownGroups = new Dictionary<string, int>(oldSettings.cooldownGroupsInternal);
            newSettings.PreMadeChatCommandSettings = new List<PreMadeChatCommandSettings>(oldSettings.preMadeChatCommandSettingsInternal);

            newSettings.ChatCommands = new LockedList<ChatCommand>(oldSettings.chatCommandsInternal);
            newSettings.EventCommands = new LockedList<EventCommand>(oldSettings.eventCommandsInternal);
            newSettings.MixPlayCommands = new LockedList<MixPlayCommand>(oldSettings.mixPlayCmmandsInternal);
            newSettings.TimerCommands = new LockedList<TimerCommand>(oldSettings.timerCommandsInternal);
            newSettings.ActionGroupCommands = new LockedList<ActionGroupCommand>(oldSettings.actionGroupCommandsInternal);
            newSettings.GameCommands = new LockedList<GameCommandBase>(oldSettings.gameCommandsInternal);
            newSettings.Quotes = new LockedList<UserQuoteViewModel>(oldSettings.userQuotesInternal);

            int quoteID = 1;
            foreach (UserQuoteViewModel quote in oldSettings.userQuotesInternal)
            {
                newSettings.Quotes.Add(quote);
                quote.ID = quoteID;
                quoteID++;
            }

            foreach (EventCommand command in newSettings.EventCommands)
            {
                if (command.OtherEventType != OtherEventTypeEnum.None)
                {
                    switch (command.OtherEventType)
                    {
                        case OtherEventTypeEnum.MixerChannelStreamStart:
                            command.EventCommandType = EventTypeEnum.MixerChannelStreamStart;
                            break;
                        case OtherEventTypeEnum.MixerChannelStreamStop:
                            command.EventCommandType = EventTypeEnum.MixerChannelStreamStop;
                            break;
                        case OtherEventTypeEnum.ChatUserUnfollow:
                            command.EventCommandType = EventTypeEnum.MixerChannelUnfollowed;
                            break;
                        case OtherEventTypeEnum.MixerSparksUsed:
                            command.EventCommandType = EventTypeEnum.MixerSparksUsed;
                            break;
                        case OtherEventTypeEnum.MixerEmbersUsed:
                            command.EventCommandType = EventTypeEnum.MixerEmbersUsed;
                            break;
                        case OtherEventTypeEnum.MixerSkillUsed:
                            command.EventCommandType = EventTypeEnum.MixerSkillUsed;
                            break;
                        case OtherEventTypeEnum.ChatUserFirstJoin:
                            command.EventCommandType = EventTypeEnum.MixerChatUserFirstJoin;
                            break;
                        case OtherEventTypeEnum.ChatUserJoined:
                            command.EventCommandType = EventTypeEnum.MixerChatUserJoined;
                            break;
                        case OtherEventTypeEnum.ChatUserLeft:
                            command.EventCommandType = EventTypeEnum.MixerChatUserLeft;
                            break;
                        case OtherEventTypeEnum.ChatUserPurge:
                            command.EventCommandType = EventTypeEnum.MixerChatUserPurge;
                            break;
                        case OtherEventTypeEnum.ChatUserBan:
                            command.EventCommandType = EventTypeEnum.MixerChatUserBan;
                            break;
                        case OtherEventTypeEnum.ChatMessageReceived:
                            command.EventCommandType = EventTypeEnum.MixerChatMessageReceived;
                            break;
                        case OtherEventTypeEnum.ChatMessageDeleted:
                            command.EventCommandType = EventTypeEnum.MixerChatMessageDeleted;
                            break;
                        case OtherEventTypeEnum.StreamlabsDonation:
                            command.EventCommandType = EventTypeEnum.StreamlabsDonation;
                            break;
                        case OtherEventTypeEnum.TipeeeStreamDonation:
                            command.EventCommandType = EventTypeEnum.TipeeeStreamDonation;
                            break;
                        case OtherEventTypeEnum.TreatStreamDonation:
                            command.EventCommandType = EventTypeEnum.TreatStreamDonation;
                            break;
                        case OtherEventTypeEnum.StreamJarDonation:
                            command.EventCommandType = EventTypeEnum.StreamJarDonation;
                            break;
                        case OtherEventTypeEnum.TiltifyDonation:
                            command.EventCommandType = EventTypeEnum.TiltifyDonation;
                            break;
                        case OtherEventTypeEnum.ExtraLifeDonation:
                            command.EventCommandType = EventTypeEnum.ExtraLifeDonation;
                            break;
                        case OtherEventTypeEnum.JustGivingDonation:
                            command.EventCommandType = EventTypeEnum.JustGivingDonation;
                            break;
                        case OtherEventTypeEnum.PatreonSubscribed:
                            command.EventCommandType = EventTypeEnum.PatreonSubscribed;
                            break;
                        case OtherEventTypeEnum.StreamlootsCardRedeemed:
                            command.EventCommandType = EventTypeEnum.StreamlootsCardRedeemed;
                            break;
                        case OtherEventTypeEnum.StreamlootsPackPurchased:
                            command.EventCommandType = EventTypeEnum.StreamlootsPackPurchased;
                            break;
                        case OtherEventTypeEnum.StreamlootsPackGifted:
                            command.EventCommandType = EventTypeEnum.StreamlootsPackGifted;
                            break;
                    }
                }
                else
                {
                    switch (command.EventType)
                    {
                        case ConstellationEventTypeEnum.channel__id__followed:
                            command.EventCommandType = EventTypeEnum.MixerChannelFollowed;
                            break;
                        case ConstellationEventTypeEnum.channel__id__hosted:
                            command.EventCommandType = EventTypeEnum.MixerChannelHosted;
                            break;
                        case ConstellationEventTypeEnum.channel__id__subscribed:
                            command.EventCommandType = EventTypeEnum.MixerChannelSubscribed;
                            break;
                        case ConstellationEventTypeEnum.channel__id__resubscribed:
                            command.EventCommandType = EventTypeEnum.MixerChannelResubscribed;
                            break;
                        case ConstellationEventTypeEnum.channel__id__subscriptionGifted:
                            command.EventCommandType = EventTypeEnum.MixerChannelSubscriptionGifted;
                            break;
                        case ConstellationEventTypeEnum.progression__id__levelup:
                            command.EventCommandType = EventTypeEnum.MixerFanProgressionLevelUp;
                            break;
                    }
                }
            }

            await ChannelSession.Services.Settings.Save(newSettings);

            newSettings = await SerializerHelper.DeserializeFromFile<SettingsV2Model>(newSettings.SettingsFilePath, ignoreErrors: true);
            await ChannelSession.Services.Settings.Initialize(newSettings);

            foreach (CommandBase command in GetAllCommands(newSettings))
            {
                foreach (ActionBase action in command.Actions)
                {
                    if (action is InteractiveAction)
                    {
                        InteractiveAction iaction = (InteractiveAction)action;
                        iaction.CooldownAmountString = iaction.CooldownAmount.ToString();
                    }
                }
            }
        }

        private static UserRoleEnum ConvertLegacyRoles(UserRoleEnum legacyRole)
        {
            int legacyRoleID = (int)legacyRole;
            if ((int)UserRoleEnum.Custom == legacyRoleID)
            {
                return UserRoleEnum.Custom;
            }
            else
            {
                return (UserRoleEnum)(legacyRoleID * 10);
            }
        }

        private static IEnumerable<CommandBase> GetAllCommands(SettingsV2Model settings)
        {
            List<CommandBase> commands = new List<CommandBase>();

            commands.AddRange(settings.ChatCommands);
            commands.AddRange(settings.EventCommands);
            commands.AddRange(settings.MixPlayCommands);
            commands.AddRange(settings.TimerCommands);
            commands.AddRange(settings.ActionGroupCommands);
            commands.AddRange(settings.GameCommands);

            foreach (UserDataModel userData in settings.UserData.Values)
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

            foreach (UserCurrencyModel currency in settings.Currencies.Values)
            {
                if (currency.RankChangedCommand != null)
                {
                    commands.Add(currency.RankChangedCommand);
                }
            }

            foreach (UserInventoryModel inventory in settings.Inventories.Values)
            {
                commands.Add(inventory.ItemsBoughtCommand);
                commands.Add(inventory.ItemsSoldCommand);
            }

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

            return commands.Where(c => c != null);
        }

        private static T GetOptionValue<T>(JObject jobj, string key)
        {
            if (jobj[key] != null)
            {
                return jobj[key].ToObject<T>();
            }
            return default(T);
        }
    }
}

#pragma warning restore CS0612 // Type or member is obsolete