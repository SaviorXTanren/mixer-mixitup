using Mixer.Base.Clients;
using Mixer.Base.Model.MixPlay;
using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Interactive;
using MixItUp.Base.Model.MixPlay;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Model.User;
using MixItUp.Base.Remote.Models;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Interactive;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
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
            await DesktopSettingsUpgrader.Version36Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version37Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version38Upgrade(version, filePath);
            await DesktopSettingsUpgrader.Version39Upgrade(version, filePath);

            SettingsV2Model settings = await SerializerHelper.DeserializeFromFile<SettingsV2Model>(filePath, ignoreErrors: true);
            settings.InitializeDB = false;
            await ChannelSession.Services.Settings.Initialize(settings);
            settings.Version = SettingsV2Model.LatestVersion;

            await ChannelSession.Services.Settings.Save(settings);
        }

        private static async Task Version29Upgrade(int version, string filePath)
        {
            if (version < 29)
            {
                SettingsV2Model settings = await SerializerHelper.DeserializeFromFile<SettingsV2Model>(filePath, ignoreErrors: true);
                await ChannelSession.Services.Settings.Initialize(settings);

                // No longer needed, this used to upgrade song request actions

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version30Upgrade(int version, string filePath)
        {
            if (version < 30)
            {
                SettingsV2Model settings = await SerializerHelper.DeserializeFromFile<SettingsV2Model>(filePath, ignoreErrors: true);
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
                SettingsV2Model settings = await SerializerHelper.DeserializeFromFile<SettingsV2Model>(filePath, ignoreErrors: true);
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
                SettingsV2Model settings = await SerializerHelper.DeserializeFromFile<SettingsV2Model>(filePath, ignoreErrors: true);
                await ChannelSession.Services.Settings.Initialize(settings);

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
                SettingsV2Model settings = await SerializerHelper.DeserializeFromFile<SettingsV2Model>(filePath, ignoreErrors: true);
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
                SettingsV2Model settings = await SerializerHelper.DeserializeFromFile<SettingsV2Model>(filePath, ignoreErrors: true);
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
                if (settings.ChatMixPlayAlertsColorScheme.Equals(defaultColor))
                {
                    settings.ChatMixPlayAlertsColorScheme = ColorSchemes.DefaultColorScheme;
                }

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version35Upgrade(int version, string filePath)
        {
            if (version < 35)
            {
                SettingsV2Model settings = await SerializerHelper.DeserializeFromFile<SettingsV2Model>(filePath, ignoreErrors: true);
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

        private static async Task Version36Upgrade(int version, string filePath)
        {
            if (version < 36)
            {
                SettingsV2Model settings = await SerializerHelper.DeserializeFromFile<SettingsV2Model>(filePath, ignoreErrors: true);
                await ChannelSession.Services.Settings.Initialize(settings);

                foreach (CommandBase command in GetAllCommands(settings))
                {
                    StoreCommandUpgrader.ReplaceActionGroupAction(command.Actions);
                }

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version37Upgrade(int version, string filePath)
        {
            if (version < 37)
            {
                string fileData = await ChannelSession.Services.FileService.ReadFile(filePath);
                fileData = fileData.Replace("Mixer.Base.Model.Interactive.InteractiveConnectedButtonControlModel, Mixer.Base", "Mixer.Base.Model.MixPlay.MixPlayConnectedButtonControlModel, Mixer.Base");
                fileData = fileData.Replace("Mixer.Base.Model.Interactive.InteractiveConnectedJoystickControlModel, Mixer.Base", "Mixer.Base.Model.MixPlay.MixPlayConnectedJoystickControlModel, Mixer.Base");
                fileData = fileData.Replace("Mixer.Base.Model.Interactive.InteractiveConnectedTextBoxControlModel, Mixer.Base", "Mixer.Base.Model.MixPlay.MixPlayConnectedTextBoxControlModel, Mixer.Base");
                fileData = fileData.Replace("Mixer.Base.Model.Interactive.InteractiveButtonControlModel, Mixer.Base", "Mixer.Base.Model.MixPlay.MixPlayButtonControlModel, Mixer.Base");
                fileData = fileData.Replace("Mixer.Base.Model.Interactive.InteractiveJoystickControlModel, Mixer.Base", "Mixer.Base.Model.MixPlay.MixPlayJoystickControlModel, Mixer.Base");
                fileData = fileData.Replace("Mixer.Base.Model.Interactive.InteractiveTextBoxControlModel, Mixer.Base", "Mixer.Base.Model.MixPlay.MixPlayTextBoxControlModel, Mixer.Base");
                fileData = fileData.Replace("MixItUp.Base.Commands.InteractiveButtonCommandTriggerType, MixItUp.Base", "MixItUp.Base.Commands.InteractiveButtonCommandTriggerType, MixItUp.Desktop");
                fileData = fileData.Replace("MixItUp.Base.Commands.InteractiveJoystickSetupType, MixItUp.Base", "MixItUp.Base.Commands.InteractiveJoystickSetupType, MixItUp.Desktop");
                fileData = fileData.Replace("MixItUp.Base.Commands.InteractiveCommand, MixItUp.Base", "MixItUp.Base.Commands.InteractiveCommand, MixItUp.Desktop");
                fileData = fileData.Replace("MixItUp.Base.Commands.InteractiveButtonCommand, MixItUp.Base", "MixItUp.Base.Commands.InteractiveButtonCommand, MixItUp.Desktop");
                fileData = fileData.Replace("MixItUp.Base.Commands.InteractiveJoystickCommand, MixItUp.Base", "MixItUp.Base.Commands.InteractiveJoystickCommand, MixItUp.Desktop");
                fileData = fileData.Replace("MixItUp.Base.Commands.InteractiveJoystickCommand+InteractiveJoystickAction, MixItUp.Base", "MixItUp.Base.Commands.MixPlayJoystickCommand+MixPlayJoystickAction, MixItUp.Base");
                fileData = fileData.Replace("MixItUp.Base.Commands.InteractiveTextBoxCommand, MixItUp.Base", "MixItUp.Base.Commands.InteractiveTextBoxCommand, MixItUp.Desktop");
                fileData = fileData.Replace("MixItUp.Base.Model.Interactive.InteractiveSharedProjectModel, MixItUp.Base", "MixItUp.Base.Model.Interactive.InteractiveSharedProjectModel, MixItUp.Desktop");
                LegacyDesktopChannelSettings legacySettings = SerializerHelper.DeserializeFromString<LegacyDesktopChannelSettings>(fileData, ignoreErrors: true);

                SettingsV2Model settings = await SerializerHelper.DeserializeFromFile<SettingsV2Model>(filePath, ignoreErrors: true);
                await ChannelSession.Services.Settings.Initialize(settings);

                settings.ChatMixPlayAlertsColorScheme = legacySettings.ChatInteractiveAlertsColorScheme;
                settings.ChatShowMixPlayAlerts = legacySettings.ChatShowInteractiveAlerts;
                settings.CustomMixPlaySettings = legacySettings.CustomInteractiveSettings;
                settings.DefaultMixPlayGame = legacySettings.DefaultInteractiveGame;
                settings.PreventUnknownMixPlayUsers = legacySettings.PreventUnknownInteractiveUsers;
                settings.PreventSmallerMixPlayCooldowns = legacySettings.PreventSmallerCooldowns;

                settings.CustomMixPlaySettings = legacySettings.CustomInteractiveSettings;

                foreach (var command in legacySettings.interactiveCommandsInternal)
                {
                    if (command is InteractiveButtonCommand)
                    {
                        InteractiveButtonCommand iCommand = (InteractiveButtonCommand)command;
                        settings.MixPlayCommands.Add(new MixPlayButtonCommand()
                        {
                            Actions = iCommand.Actions,
                            Commands = iCommand.Commands,
                            Control = iCommand.Control,
                            GameID = iCommand.GameID,
                            GroupName = iCommand.GroupName,
                            ID = iCommand.ID,
                            IsBasic = iCommand.IsBasic,
                            IsEnabled = iCommand.IsEnabled,
                            IsRandomized = iCommand.IsRandomized,
                            Name = iCommand.Name,
                            Requirements = iCommand.Requirements,
                            SceneID = iCommand.SceneID,
                            StoreID = iCommand.StoreID,
                            Type = iCommand.Type,
                            Unlocked = iCommand.Unlocked,

                            HeldRate = iCommand.HeldRate,
                            IsBeingHeld = iCommand.IsBeingHeld,
                            Trigger = (MixPlayButtonCommandTriggerType)iCommand.Trigger,
                        });
                    }
                    else if (command is InteractiveJoystickCommand)
                    {
                        InteractiveJoystickCommand iCommand = (InteractiveJoystickCommand)command;
                        settings.MixPlayCommands.Add(new MixPlayJoystickCommand()
                        {
                            Actions = iCommand.Actions,
                            Commands = iCommand.Commands,
                            Control = iCommand.Control,
                            GameID = iCommand.GameID,
                            GroupName = iCommand.GroupName,
                            ID = iCommand.ID,
                            IsBasic = iCommand.IsBasic,
                            IsEnabled = iCommand.IsEnabled,
                            IsRandomized = iCommand.IsRandomized,
                            Name = iCommand.Name,
                            Requirements = iCommand.Requirements,
                            SceneID = iCommand.SceneID,
                            StoreID = iCommand.StoreID,
                            Type = iCommand.Type,
                            Unlocked = iCommand.Unlocked,

                            DeadZone = iCommand.DeadZone,
                            MappedKeys = iCommand.MappedKeys,
                            MouseMovementMultiplier = iCommand.MouseMovementMultiplier,
                            SetupType = (MixPlayJoystickSetupType)iCommand.SetupType
                        });
                    }
                    else if (command is InteractiveTextBoxCommand)
                    {
                        InteractiveTextBoxCommand iCommand = (InteractiveTextBoxCommand)command;
                        settings.MixPlayCommands.Add(new MixPlayTextBoxCommand()
                        {
                            Actions = iCommand.Actions,
                            Commands = iCommand.Commands,
                            Control = iCommand.Control,
                            GameID = iCommand.GameID,
                            GroupName = iCommand.GroupName,
                            ID = iCommand.ID,
                            IsBasic = iCommand.IsBasic,
                            IsEnabled = iCommand.IsEnabled,
                            IsRandomized = iCommand.IsRandomized,
                            Name = iCommand.Name,
                            Requirements = iCommand.Requirements,
                            SceneID = iCommand.SceneID,
                            StoreID = iCommand.StoreID,
                            Type = iCommand.Type,
                            Unlocked = iCommand.Unlocked,
                            
                            UseChatModeration = iCommand.UseChatModeration
                        });
                    }
                }

                foreach (var project in legacySettings.CustomInteractiveProjectIDs)
                {
                    settings.CustomMixPlayProjectIDs.Add(new MixPlaySharedProjectModel()
                    {
                        GameID = project.GameID,
                        ShareCode = project.ShareCode,
                        VersionID = project.VersionID
                    });
                }

                foreach (var userGroup in legacySettings.interactiveUserGroupsInternal)
                {
                    settings.MixPlayUserGroups[userGroup.Key] = new List<MixPlayUserGroupModel>();
                    foreach (var group in userGroup.Value)
                    {
                        settings.MixPlayUserGroups[userGroup.Key].Add(new MixPlayUserGroupModel()
                        {
                            AssociatedUserRole = group.AssociatedUserRole,
                            CurrentScene = group.CurrentScene,
                            DefaultScene = group.DefaultScene,
                            GroupName = group.GroupName
                        });
                    }
                }

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version38Upgrade(int version, string filePath)
        {
            if (version < 38)
            {
                SettingsV2Model settings = await SerializerHelper.DeserializeFromFile<SettingsV2Model>(filePath, ignoreErrors: true);
                await ChannelSession.Services.Settings.Initialize(settings);

                foreach (CommandBase command in GetAllCommands(settings))
                {
                    StoreCommandUpgrader.UpdateConditionalAction(command.Actions);
                }

                await ChannelSession.Services.Settings.Save(settings);
            }
        }

        private static async Task Version39Upgrade(int version, string filePath)
        {
            if (version < 39)
            {
                SettingsV2Model settings = await SerializerHelper.DeserializeFromFile<SettingsV2Model>(filePath, ignoreErrors: true);
#pragma warning disable CS0612 // Type or member is obsolete
                settings.MixerUserOAuthToken = settings.OAuthToken;
                settings.MixerBotOAuthToken = settings.BotOAuthToken;
                if (settings.Channel != null)
                {
                    settings.Name = settings.Channel.token;
                    settings.MixerChannelID = settings.Channel.id;
                }
                settings.TelemetryUserID = settings.TelemetryUserId;

                settings.RemoteProfileBoards = new Dictionary<Guid, RemoteProfileBoardsModel>(settings.remoteProfileBoardsInternal);
                settings.FilteredWords = new List<string>(settings.filteredWordsInternal);
                settings.BannedWords = new List<string>(settings.bannedWordsInternal);
                settings.MixPlayUserGroups = new Dictionary<uint, List<MixPlayUserGroupModel>>(settings.mixPlayUserGroupsInternal);
                settings.OverlayWidgets = new List<OverlayWidgetModel>(settings.overlayWidgetModelsInternal);
                settings.Currencies = new Dictionary<Guid, UserCurrencyModel>(settings.currenciesInternal);
                settings.Inventories = new Dictionary<Guid, UserInventoryModel>(settings.inventoriesInternal);
                settings.CooldownGroups = new Dictionary<string, int>(settings.cooldownGroupsInternal);
                settings.PreMadeChatCommandSettings = new List<PreMadeChatCommandSettings>(settings.preMadeChatCommandSettingsInternal);

                settings.ChatCommands = new LockedList<ChatCommand>(settings.chatCommandsInternal);
                settings.EventCommands = new LockedList<EventCommand>(settings.eventCommandsInternal);
                settings.MixPlayCommands = new LockedList<MixPlayCommand>(settings.mixPlayCmmandsInternal);
                settings.TimerCommands = new LockedList<TimerCommand>(settings.timerCommandsInternal);
                settings.ActionGroupCommands = new LockedList<ActionGroupCommand>(settings.actionGroupCommandsInternal);
                settings.GameCommands = new LockedList<GameCommandBase>(settings.gameCommandsInternal);
                settings.Quotes = new LockedList<UserQuoteViewModel>(settings.userQuotesInternal);

                int quoteID = 1;
                foreach (UserQuoteViewModel quote in settings.userQuotesInternal)
                {
                    quote.ID = quoteID;
                    quoteID++;
                }

                foreach (EventCommand command in settings.eventCommandsInternal)
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

                    string oldDatabaseFilePath = Path.Combine(Path.GetDirectoryName(filePath), settings.DatabasePath);
                    await ChannelSession.Services.Database.Read(oldDatabaseFilePath, "SELECT * FROM Users", (Dictionary<string, object> data) =>
                    {
                        UserDataViewModel userData = new UserDataViewModel();

                        userData.MixerID = uint.Parse(data["ID"].ToString());
                        userData.MixerUsername = data["UserName"].ToString();

                        userData.ViewingMinutes = int.Parse(data["ViewingMinutes"].ToString());

                        if (data.ContainsKey("CurrencyAmounts"))
                        {
                            Dictionary<Guid, int> currencyAmounts = JsonConvert.DeserializeObject<Dictionary<Guid, int>>(data["CurrencyAmounts"].ToString());
                            if (currencyAmounts != null)
                            {
                                foreach (var kvp in currencyAmounts)
                                {
                                    if (settings.Currencies.ContainsKey(kvp.Key))
                                    {
                                        settings.Currencies[kvp.Key].SetAmount(userData, kvp.Value);
                                    }
                                }
                            }
                        }

                        if (data.ContainsKey("InventoryAmounts"))
                        {
                            Dictionary<Guid, Dictionary<string, int>> inventoryAmounts = JsonConvert.DeserializeObject<Dictionary<Guid, Dictionary<string, int>>>(data["InventoryAmounts"].ToString());
                            if (inventoryAmounts != null)
                            {
                                foreach (var kvp in inventoryAmounts)
                                {
                                    if (settings.Inventories.ContainsKey(kvp.Key))
                                    {
                                        UserInventoryModel inventory = settings.Inventories[kvp.Key];
                                        foreach (var itemKVP in kvp.Value)
                                        {
                                            inventory.SetAmount(userData, itemKVP.Key, itemKVP.Value);
                                        }
                                    }
                                }
                            }
                        }

                        if (data.ContainsKey("CustomCommands") && !string.IsNullOrEmpty(data["CustomCommands"].ToString()))
                        {
                            userData.CustomCommands.AddRange(SerializerHelper.DeserializeFromString<List<ChatCommand>>(data["CustomCommands"].ToString()));
                        }

                        if (data.ContainsKey("Options") && !string.IsNullOrEmpty(data["Options"].ToString()))
                        {
                            JObject optionsJObj = JObject.Parse(data["Options"].ToString());
                            if (optionsJObj.ContainsKey("EntranceCommand") && optionsJObj["EntranceCommand"] != null)
                            {
                                userData.EntranceCommand = SerializerHelper.DeserializeFromString<CustomCommand>(optionsJObj["EntranceCommand"].ToString());
                            }
                            userData.IsSparkExempt = GetOptionValue<bool>(optionsJObj, "IsSparkExempt");
                            userData.IsCurrencyRankExempt = GetOptionValue<bool>(optionsJObj, "IsCurrencyRankExempt");
                            userData.GameWispUserID = GetOptionValue<uint>(optionsJObj, "GameWispUserID");
                            userData.PatreonUserID = GetOptionValue<string>(optionsJObj, "PatreonUserID");
                            userData.ModerationStrikes = GetOptionValue<uint>(optionsJObj, "ModerationStrikes");
                            userData.CustomTitle = GetOptionValue<string>(optionsJObj, "CustomTitle");
                            userData.TotalStreamsWatched = GetOptionValue<uint>(optionsJObj, "TotalStreamsWatched");
                            userData.TotalAmountDonated = GetOptionValue<double>(optionsJObj, "TotalAmountDonated");
                            userData.TotalSparksSpent = GetOptionValue<uint>(optionsJObj, "TotalSparksSpent");
                            userData.TotalEmbersSpent = GetOptionValue<uint>(optionsJObj, "TotalEmbersSpent");
                            userData.TotalSubsGifted = GetOptionValue<uint>(optionsJObj, "TotalSubsGifted");
                            userData.TotalSubsReceived = GetOptionValue<uint>(optionsJObj, "TotalSubsReceived");
                            userData.TotalChatMessageSent = GetOptionValue<uint>(optionsJObj, "TotalChatMessageSent");
                            userData.TotalTimesTagged = GetOptionValue<uint>(optionsJObj, "TotalTimesTagged");
                            userData.TotalSkillsUsed = GetOptionValue<uint>(optionsJObj, "TotalSkillsUsed");
                            userData.TotalCommandsRun = GetOptionValue<uint>(optionsJObj, "TotalCommandsRun");
                            userData.TotalMonthsSubbed = GetOptionValue<uint>(optionsJObj, "TotalMonthsSubbed");
                        }

                        settings.UserData[userData.MixerID] = userData;
                    });

#pragma warning restore CS0612 // Type or member is obsolete
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

        private static IEnumerable<CommandBase> GetAllCommands(SettingsV2Model settings)
        {
            List<CommandBase> commands = new List<CommandBase>();

            commands.AddRange(settings.ChatCommands);
            commands.AddRange(settings.EventCommands);
            commands.AddRange(settings.MixPlayCommands);
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

    [DataContract]
    public class LegacyDesktopChannelSettings : SettingsV2Model
    {
        [JsonProperty]
        public bool ChatShowInteractiveAlerts { get; set; }
        [JsonProperty]
        public string ChatInteractiveAlertsColorScheme { get; set; }
        
        [JsonProperty]
        public uint DefaultInteractiveGame { get; set; }
        [JsonProperty]
        public bool PreventUnknownInteractiveUsers { get; set; }
        [JsonProperty]
        public bool PreventSmallerCooldowns { get; set; }
        [JsonProperty]
        public List<InteractiveSharedProjectModel> CustomInteractiveProjectIDs { get; set; }

        [JsonProperty]
        public Dictionary<uint, JObject> CustomInteractiveSettings { get; set; }

        [JsonProperty]
        public Dictionary<uint, List<InteractiveUserGroupViewModel>> interactiveUserGroupsInternal { get; set; }

        [JsonProperty]
        public List<InteractiveCommand> interactiveCommandsInternal { get; set; }
    }
}

#region Legacy Interactive Classes

namespace MixItUp.Base.Commands
{
    public enum InteractiveButtonCommandTriggerType
    {
        [Name("Mouse/Key Down")]
        MouseKeyDown,
        [Name("Mouse/Key Up")]
        MouseKeyUp,
        [Name("Mouse/Key Held")]
        MouseKeyHeld,
    }

    public enum InteractiveJoystickSetupType
    {
        [Name("Directional Arrows")]
        DirectionalArrows,
        WASD,
        [Name("Mouse Movement")]
        MouseMovement,
        [Name("Map To Individual Keys")]
        MapToIndividualKeys,
    }

    public class InteractiveCommand : PermissionsCommandBase
    {
        private static SemaphoreSlim interactiveCommandPerformSemaphore = new SemaphoreSlim(1);

        [JsonProperty]
        public uint GameID { get; set; }

        [JsonProperty]
        public string SceneID { get; set; }

        [JsonProperty]
        public MixPlayControlModel Control { get; set; }

        [JsonProperty]
        [Obsolete]
        public int IndividualCooldown { get; set; }

        [JsonProperty]
        [Obsolete]
        public string CooldownGroup { get; set; }

        protected override SemaphoreSlim AsyncSemaphore { get { return InteractiveCommand.interactiveCommandPerformSemaphore; } }

        public InteractiveCommand() { }

        [JsonIgnore]
        public virtual string EventTypeString { get { return string.Empty; } }
    }

    public class InteractiveButtonCommand : InteractiveCommand
    {
        public const string BasicCommandCooldownGroup = "All Buttons";

        [JsonProperty]
        public InteractiveButtonCommandTriggerType Trigger { get; set; }

        [JsonProperty]
        public int HeldRate { get; set; }

        [JsonIgnore]
        public bool IsBeingHeld { get; set; }

        public InteractiveButtonCommand() { }
    }

    public class InteractiveJoystickCommand : InteractiveCommand
    {
        private class InteractiveJoystickAction : ActionBase
        {
            private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

            protected override SemaphoreSlim AsyncSemaphore { get { return InteractiveJoystickAction.asyncSemaphore; } }

            public InteractiveJoystickAction() { }

            protected override Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
            {
                throw new NotImplementedException();
            }
        }

        [JsonProperty]
        public InteractiveJoystickSetupType SetupType { get; set; }

        [JsonProperty]
        public double DeadZone { get; set; }

        [JsonProperty]
        public List<InputKeyEnum?> MappedKeys { get; set; }

        [JsonProperty]
        public double MouseMovementMultiplier { get; set; }

        public InteractiveJoystickCommand()
        {
            this.MappedKeys = new List<InputKeyEnum?>();
        }
    }

    public class InteractiveTextBoxCommand : InteractiveCommand
    {
        public InteractiveTextBoxCommand() { }

        [JsonProperty]
        public bool UseChatModeration { get; set; }
    }
}

namespace Mixer.Base.Model.Interactive
{
    public class InteractiveConnectedButtonControlModel : InteractiveButtonControlModel
    {
        public long cooldown { get; set; }
        public float progress { get; set; }
    }

    public class InteractiveButtonControlModel : InteractiveControlModel
    {
        public const string ButtonControlKind = "button";

        public InteractiveButtonControlModel() { this.kind = ButtonControlKind; }

        public string text { get; set; }
        public int? cost { get; set; }
        public int? keyCode { get; set; }

        public string textSize { get; set; }
        public string textColor { get; set; }
        public string focusColor { get; set; }
        public string accentColor { get; set; }
        public string borderColor { get; set; }
        public string backgroundColor { get; set; }
        public string backgroundImage { get; set; }

        public string tooltip { get; set; }
    }

    public class InteractiveControlModel
    {
        public string controlID { get; set; }
        public string kind { get; set; }
        public string etag { get; set; }
        public bool disabled { get; set; }
        public InteractiveControlPositionModel[] position { get; set; }
        public JObject meta { get; set; } = new JObject();
    }

    public class InteractiveControlPositionModel
    {
        public string size { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int x { get; set; }
        public int y { get; set; }
    }

    public class InteractiveGameListingModel : InteractiveGameModel
    {
        public InteractiveGameVersionModel[] versions { get; set; }
        public UserModel owner { get; set; }
    }

    public class InteractiveGameModel : TimeStampedModel
    {
        public uint id { get; set; }
        public uint ownerId { get; set; }
        public string name { get; set; }
        public string coverUrl { get; set; }
        public string description { get; set; }
        public bool hasPublishedVersions { get; set; }
        public string installation { get; set; }
        public string controlVersion { get; set; }
    }

    public class InteractiveGameVersionUpdateableModel : TimeStampedModel
    {
        public InteractiveGameVersionUpdateableModel()
        {
            this.controls = new InteractiveSceneCollectionModel();
        }

        public string state { get; set; }
        public string installation { get; set; }
        public string download { get; set; }
        public InteractiveSceneCollectionModel controls { get; set; }
        public string controlVersion { get; set; }
    }

    public class InteractiveGameVersionModel : InteractiveGameVersionUpdateableModel
    {
        public uint id { get; set; }
        public uint gameId { get; set; }
        public string version { get; set; }
        public uint versionOrder { get; set; }
        public string changelog { get; set; }
    }

    public class InteractiveGroupModel
    {
        public string groupID { get; set; }
        public string sceneID { get; set; }
    }

    public class InteractiveConnectedJoystickControlModel : InteractiveJoystickControlModel
    {
        public double angle { get; set; }
        public double intensity { get; set; }
    }

    public class InteractiveJoystickControlModel : InteractiveControlModel
    {
        public const string JoystickControlKind = "joystick";

        public InteractiveJoystickControlModel() { this.kind = JoystickControlKind; }

        public int? sampleRate { get; set; }
    }

    public class InteractiveConnectedLabelControlModel : InteractiveLabelControlModel
    {
    }

    public class InteractiveLabelControlModel : InteractiveControlModel
    {
        public const string LabelControlKind = "label";

        public InteractiveLabelControlModel() { this.kind = LabelControlKind; }

        public string text { get; set; }

        public bool bold { get; set; }
        public bool italic { get; set; }
        public bool underline { get; set; }
        public string textSize { get; set; }
        public string textColor { get; set; }
    }

    public class InteractiveSceneCollectionModel
    {
        public InteractiveSceneCollectionModel() { this.scenes = new List<InteractiveSceneModel>(); }

        public List<InteractiveSceneModel> scenes { get; set; }
    }

    public class InteractiveSceneModel : InteractiveControlCollectionModel
    {
        public InteractiveSceneModel() { }

        public string sceneID { get; set; }
        public string etag { get; set; }
    }

    public class InteractiveControlCollectionModel
    {
        public InteractiveControlCollectionModel()
        {
            this.buttons = new List<InteractiveButtonControlModel>();
            this.joysticks = new List<InteractiveJoystickControlModel>();
            this.labels = new List<InteractiveLabelControlModel>();
            this.textBoxes = new List<InteractiveTextBoxControlModel>();
        }

        [JsonProperty("controls")]
        public JArray controlsUnstructured
        {
            get
            {
                JArray array = new JArray();
                array.Merge(JArray.FromObject(this.buttons));
                array.Merge(JArray.FromObject(this.joysticks));
                array.Merge(JArray.FromObject(this.labels));
                array.Merge(JArray.FromObject(this.textBoxes));
                return array;
            }
            set
            {
                this.buttons = value.ToTypedArray<InteractiveButtonControlModel>().
                    Where(c => c.kind.Equals(InteractiveButtonControlModel.ButtonControlKind)).ToList();
                this.joysticks = value.ToTypedArray<InteractiveJoystickControlModel>().
                    Where(c => c.kind.Equals(InteractiveJoystickControlModel.JoystickControlKind)).ToList();
                this.labels = value.ToTypedArray<InteractiveLabelControlModel>().
                    Where(c => c.kind.Equals(InteractiveLabelControlModel.LabelControlKind)).ToList();
                this.textBoxes = value.ToTypedArray<InteractiveTextBoxControlModel>().
                    Where(c => c.kind.Equals(InteractiveTextBoxControlModel.TextBoxControlKind)).ToList();
            }
        }

        [JsonIgnore]
        public List<InteractiveButtonControlModel> buttons { get; set; }

        [JsonIgnore]
        public List<InteractiveJoystickControlModel> joysticks { get; set; }

        [JsonIgnore]
        public List<InteractiveLabelControlModel> labels { get; set; }

        [JsonIgnore]
        public List<InteractiveTextBoxControlModel> textBoxes { get; set; }

        [JsonIgnore]
        public IEnumerable<InteractiveControlModel> allControls
        {
            get
            {
                List<InteractiveControlModel> controls = new List<InteractiveControlModel>();
                controls.AddRange(this.buttons);
                controls.AddRange(this.joysticks);
                controls.AddRange(this.labels);
                controls.AddRange(this.textBoxes);
                return controls;
            }
        }
    }

    public class InteractiveConnectedTextBoxControlModel : InteractiveTextBoxControlModel
    {
    }

    public class InteractiveTextBoxControlModel : InteractiveControlModel
    {
        public const string TextBoxControlKind = "textbox";

        public InteractiveTextBoxControlModel() { this.kind = TextBoxControlKind; }

        public int? cost { get; set; }

        public string submitText { get; set; }
        public string placeholder { get; set; }

        public bool hasSubmit { get; set; }
        public bool multiline { get; set; }
    }
}

namespace MixItUp.Base.Model.Interactive
{
    public class InteractiveSharedProjectModel
    {
        public static readonly InteractiveSharedProjectModel FortniteDropMap = new InteractiveSharedProjectModel(271086, 277002, "dxr3qllr");
        public static readonly InteractiveSharedProjectModel PUBGDropMap = new InteractiveSharedProjectModel(271188, 277104, "58virtn9");
        public static readonly InteractiveSharedProjectModel RealmRoyaleDropMap = new InteractiveSharedProjectModel(271221, 277137, "4h0qt5ub");
        public static readonly InteractiveSharedProjectModel BlackOps4DropMap = new InteractiveSharedProjectModel(286365, 292278, "svdiqaq5");
        public static readonly InteractiveSharedProjectModel ApexLegendsDropMap = new InteractiveSharedProjectModel(320709, 326616, "yar067ds");
        public static readonly InteractiveSharedProjectModel SuperAnimalRoyaleDropMap = new InteractiveSharedProjectModel(332619, 338526, "nbb7xrcu");

        public static readonly InteractiveSharedProjectModel MixerPaint = new InteractiveSharedProjectModel(271176, 277092, "zu52jzv2");

        public static readonly InteractiveSharedProjectModel FlySwatter = new InteractiveSharedProjectModel(295410, 301323, "5b11a82j");

        public static readonly List<InteractiveSharedProjectModel> AllMixPlayProjects = new List<InteractiveSharedProjectModel>() { FortniteDropMap, PUBGDropMap, RealmRoyaleDropMap, BlackOps4DropMap, ApexLegendsDropMap, SuperAnimalRoyaleDropMap, MixerPaint, FlySwatter };

        public uint GameID { get; set; }
        public uint VersionID { get; set; }
        public string ShareCode { get; set; }

        public InteractiveSharedProjectModel() { }

        public InteractiveSharedProjectModel(uint versionID, string shareCode) : this(0, versionID, shareCode) { }

        public InteractiveSharedProjectModel(uint gameID, uint versionID, string shareCode)
        {
            this.GameID = gameID;
            this.VersionID = versionID;
            this.ShareCode = shareCode;
        }
    }
}

namespace MixItUp.Base.ViewModel.Interactive
{
    [DataContract]
    public class InteractiveUserGroupViewModel
    {
        public const string DefaultName = "default";

        public InteractiveUserGroupViewModel() { }

        public InteractiveUserGroupViewModel(MixerRoleEnum associatedUserRole)
            : this((associatedUserRole != MixerRoleEnum.User) ? EnumHelper.GetEnumName(associatedUserRole) : DefaultName)
        {
            this.AssociatedUserRole = associatedUserRole;
        }

        public InteractiveUserGroupViewModel(string groupName) : this(groupName, InteractiveUserGroupViewModel.DefaultName) { }

        public InteractiveUserGroupViewModel(string groupName, string defaultScene)
        {
            this.GroupName = groupName;
            this.DefaultScene = defaultScene;
            this.AssociatedUserRole = MixerRoleEnum.Custom;
        }

        public InteractiveUserGroupViewModel(MixPlayGroupModel group) : this(group.groupID, group.sceneID) { }

        [DataMember]
        public string GroupName { get; set; }
        [DataMember]
        public string DefaultScene { get; set; }
        [DataMember]
        public MixerRoleEnum AssociatedUserRole { get; set; }

        [JsonIgnore]
        public string CurrentScene { get; set; }
    }
}

#endregion Legacy/Obsolete Classes

