using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services.Glimesh;
using MixItUp.Base.Services.Mock;
using MixItUp.Base.Services.Trovo;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Services.YouTube;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Twitch;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public class ChatService
    {
        private const string ChatEventLogDirectoryName = "ChatEventLogs";
        private const string ChatEventLogFileNameFormat = "ChatEventLog-{0}.txt";

        public static string SplitLargeMessage(string message, int maxLength, out string subMessage)
        {
            subMessage = null;
            if (message.Length >= maxLength)
            {
                string tempMessage = message.Substring(0, maxLength - 1);
                int splitIndex = tempMessage.LastIndexOf(' ');
                if (splitIndex <= 0)
                {
                    splitIndex = maxLength;
                }

                if (splitIndex + 1 < message.Length)
                {
                    subMessage = message.Substring(splitIndex + 1);
                    message = message.Substring(0, splitIndex);
                }
            }
            return message;
        }

        public bool DisableChat { get; set; }

        public ThreadSafeObservableCollection<ChatMessageViewModel> Messages { get; private set; } = new ThreadSafeObservableCollection<ChatMessageViewModel>();
        private LockedDictionary<string, ChatMessageViewModel> messagesLookup = new LockedDictionary<string, ChatMessageViewModel>();

        public event EventHandler ChatCommandsReprocessed = delegate { };
        public IEnumerable<CommandModelBase> ChatMenuCommands { get { return this.chatMenuCommands.ToList(); } }
        private List<CommandModelBase> chatMenuCommands = new List<CommandModelBase>();

        public event EventHandler<Dictionary<string, uint>> OnPollEndOccurred = delegate { };

        private Dictionary<string, CommandModelBase> triggersToCommands = new Dictionary<string, CommandModelBase>();
        private int longestTrigger = 0;
        private List<CommandModelBase> wildcardCommands = new List<CommandModelBase>();

        private HashSet<Guid> userEntranceCommands = new HashSet<Guid>();

        private SemaphoreSlim whisperNumberLock = new SemaphoreSlim(1);
        private Dictionary<Guid, int> whisperMap = new Dictionary<Guid, int>();

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private string currentChatEventLogFilePath;

        public ChatService() { }

        public async Task Initialize()
        {
            this.RebuildCommandTriggers();

            await ServiceManager.Get<IFileService>().CreateDirectory(ChatEventLogDirectoryName);
            this.currentChatEventLogFilePath = Path.Combine(ChatEventLogDirectoryName, string.Format(ChatEventLogFileNameFormat, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture)));

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            AsyncRunner.RunAsyncBackground(this.ProcessHoursCurrency, this.cancellationTokenSource.Token, 60000);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public async Task SendMessage(string message, bool sendAsStreamer = false, string replyMessageID = null)
        {
            await StreamingPlatforms.ForEachPlatform(async (p) =>
            {
                await this.SendMessage(message, p, sendAsStreamer, replyMessageID);
            });
        }

        public async Task SendMessage(string message, CommandParametersModel parameters, bool sendAsStreamer = false)
        {
            await this.SendMessage(message, parameters.Platform, sendAsStreamer, parameters.TriggeringChatMessageID);
        }

        public async Task SendMessage(string message, StreamingPlatformTypeEnum platform, bool sendAsStreamer = false, string replyMessageID = null)
        {
            if (!string.IsNullOrEmpty(message))
            {
                if (platform == StreamingPlatformTypeEnum.All)
                {
                    await this.SendMessage(message, sendAsStreamer, replyMessageID);
                }
                else if (!string.IsNullOrEmpty(message))
                {
                    if (platform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchChatService>().IsUserConnected)
                    {
                        if (!ChannelSession.Settings.TwitchReplyToCommandChatMessages)
                        {
                            replyMessageID = null;
                        }

                        await ServiceManager.Get<TwitchChatService>().SendMessage(message, sendAsStreamer, replyMessageID);

                        if (sendAsStreamer || !ServiceManager.Get<TwitchChatService>().IsBotConnected)
                        {
                            await this.AddMessage(new TwitchChatMessageViewModel(ChannelSession.User, message, replyMessageID));
                        }
                    }

                    if (platform == StreamingPlatformTypeEnum.YouTube && ServiceManager.Get<YouTubeChatService>().IsUserConnected)
                    {
                        await ServiceManager.Get<YouTubeChatService>().SendMessage(message, sendAsStreamer);
                    }

                    if (platform == StreamingPlatformTypeEnum.Glimesh && ServiceManager.Get<GlimeshChatEventService>().IsUserConnected)
                    {
                        await ServiceManager.Get<GlimeshChatEventService>().SendMessage(message, sendAsStreamer);
                    }

                    if (platform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoChatEventService>().IsUserConnected)
                    {
                        await ServiceManager.Get<TrovoChatEventService>().SendMessage(message, sendAsStreamer);
                    }

                    if (platform == StreamingPlatformTypeEnum.Mock)
                    {
                        await ServiceManager.Get<MockChatEventService>().SendMessage(message, sendAsStreamer);
                    }
                }
            }
        }

        public async Task Whisper(UserV2ViewModel user, string message, bool sendAsStreamer = false)
        {
            if (user != null && !string.IsNullOrEmpty(message))
            {
                if (user.Platform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchChatService>().IsUserConnected)
                {
                    await ServiceManager.Get<TwitchChatService>().SendWhisperMessage(user, message, sendAsStreamer);
                }
            }
        }

        public async Task Whisper(string username, StreamingPlatformTypeEnum platform, string message, bool sendAsStreamer = false)
        {
            UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByPlatformUsername(platform, username, performPlatformSearch: true);
            if (user != null)
            {
                await this.Whisper(user, message, sendAsStreamer);
            }
        }

        public async Task DeleteMessage(ChatMessageViewModel message, bool externalDeletion = false)
        {
            if (message == null)
            {
                return;
            }

            if (externalDeletion && !this.messagesLookup.TryGetValue(message.ID, out ChatMessageViewModel existingMessage) && existingMessage != null)
            {
                message = existingMessage;
            }

            if (!externalDeletion && !string.IsNullOrEmpty(message.ID))
            {
                if (message.Platform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchChatService>().IsUserConnected)
                {
                    await ServiceManager.Get<TwitchChatService>().DeleteMessage(message);
                }
                else if (message.Platform == StreamingPlatformTypeEnum.YouTube && ServiceManager.Get<YouTubeChatService>().IsUserConnected)
                {
                    await ServiceManager.Get<YouTubeChatService>().DeleteMessage(message);
                }
                else if (message.Platform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoChatEventService>().IsUserConnected)
                {
                    await ServiceManager.Get<TrovoChatEventService>().DeleteMessage(message);
                }
                else if (message.Platform == StreamingPlatformTypeEnum.Glimesh && ServiceManager.Get<GlimeshChatEventService>().IsUserConnected)
                {
                    await ServiceManager.Get<GlimeshChatEventService>().DeleteMessage(message);
                }
                else if (message.Platform == StreamingPlatformTypeEnum.Mock)
                {
                    await ServiceManager.Get<MockChatEventService>().DeleteMessage(message);
                }
            }

            if (!message.IsDeleted)
            {
                await message.Delete();
            }

            if (ChannelSession.Settings.HideDeletedMessages)
            {
                await this.RemoveMessage(message);
            }

            GlobalEvents.ChatMessageDeleted(message.ID);
        }

        public async Task ClearMessages(StreamingPlatformTypeEnum platform)
        {
            if (platform == StreamingPlatformTypeEnum.All)
            {
                await StreamingPlatforms.ForEachPlatform(async (p) =>
                {
                    await this.ClearMessages(p);
                });
            }
            else
            {
                if (platform == StreamingPlatformTypeEnum.Twitch)
                {
                    if (ServiceManager.Get<TwitchChatService>().IsUserConnected)
                    {
                        await ServiceManager.Get<TwitchChatService>().ClearMessages();
                    }
                }
                else if (platform == StreamingPlatformTypeEnum.Trovo)
                {
                    if (ServiceManager.Get<TrovoChatEventService>().IsUserConnected)
                    {
                        await ServiceManager.Get<TrovoChatEventService>().ClearChat();
                    }
                }

                this.messagesLookup.Clear();
                this.Messages.Clear();
            }
        }

        public async Task PurgeUser(UserV2ViewModel user)
        {
            if (user.Platform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchChatService>().IsUserConnected)
            {
                await ServiceManager.Get<TwitchChatService>().TimeoutUser(user, 1);
            }
        }

        public async Task TimeoutUser(UserV2ViewModel user, uint durationInSeconds)
        {
            if (user.Platform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchChatService>().IsUserConnected)
            {
                await ServiceManager.Get<TwitchChatService>().TimeoutUser(user, (int)durationInSeconds);
            }

            if (user.Platform == StreamingPlatformTypeEnum.YouTube && ServiceManager.Get<YouTubeChatService>().IsUserConnected)
            {
                await ServiceManager.Get<YouTubeChatService>().TimeoutUser(user, durationInSeconds);
            }

            if (user.Platform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoChatEventService>().IsUserConnected)
            {
                await ServiceManager.Get<TrovoChatEventService>().TimeoutUser(user.Username, (int)durationInSeconds);
            }

            if (user.Platform == StreamingPlatformTypeEnum.Glimesh && ServiceManager.Get<GlimeshChatEventService>().IsUserConnected)
            {
                if (durationInSeconds > 300)
                {
                    await ServiceManager.Get<GlimeshChatEventService>().LongTimeoutUser(user);
                }
                else
                {
                    await ServiceManager.Get<GlimeshChatEventService>().ShortTimeoutUser(user);
                }
            }
        }

        public async Task ModUser(UserV2ViewModel user)
        {
            if (user.Platform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchChatService>().IsUserConnected)
            {
                await ServiceManager.Get<TwitchChatService>().ModUser(user);
            }

            if (user.Platform == StreamingPlatformTypeEnum.YouTube && ServiceManager.Get<YouTubeChatService>().IsUserConnected)
            {
                await ServiceManager.Get<YouTubeChatService>().ModUser(user);
            }

            if (user.Platform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoChatEventService>().IsUserConnected)
            {
                await ServiceManager.Get<TrovoChatEventService>().ModUser(user.Username);
            }
        }

        public async Task UnmodUser(UserV2ViewModel user)
        {
            if (user.Platform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchChatService>().IsUserConnected)
            {
                await ServiceManager.Get<TwitchChatService>().UnmodUser(user);
            }

            if (user.Platform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoChatEventService>().IsUserConnected)
            {
                await ServiceManager.Get<TrovoChatEventService>().UnmodUser(user.Username);
            }
        }

        public async Task BanUser(UserV2ViewModel user)
        {
            if (user.Platform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchChatService>().IsUserConnected)
            {
                await ServiceManager.Get<TwitchChatService>().BanUser(user);
            }

            if (user.Platform == StreamingPlatformTypeEnum.YouTube && ServiceManager.Get<YouTubeChatService>().IsUserConnected)
            {
                await ServiceManager.Get<YouTubeChatService>().BanUser(user);
            }

            if (user.Platform == StreamingPlatformTypeEnum.Glimesh && ServiceManager.Get<GlimeshChatEventService>().IsUserConnected)
            {
                await ServiceManager.Get<GlimeshChatEventService>().BanUser(user);
            }

            if (user.Platform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoChatEventService>().IsUserConnected)
            {
                await ServiceManager.Get<TrovoChatEventService>().BanUser(user.Username);
            }
        }

        public async Task UnbanUser(UserV2ViewModel user)
        {
            if (user.Platform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchChatService>().IsUserConnected)
            {
                await ServiceManager.Get<TwitchChatService>().UnbanUser(user);
            }

            if (user.Platform == StreamingPlatformTypeEnum.Glimesh && ServiceManager.Get<GlimeshChatEventService>().IsUserConnected)
            {
                await ServiceManager.Get<GlimeshChatEventService>().UnbanUser(user);
            }

            if (user.Platform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoChatEventService>().IsUserConnected)
            {
                await ServiceManager.Get<TrovoChatEventService>().UnbanUser(user.Username);
            }
        }

        public void RebuildCommandTriggers()
        {
            try
            {
                this.triggersToCommands.Clear();
                this.longestTrigger = 0;
                this.wildcardCommands.Clear();
                this.chatMenuCommands.Clear();
                foreach (ChatCommandModel command in ServiceManager.Get<CommandService>().AllEnabledChatAccessibleCommands)
                {
                    if (command.Wildcards)
                    {
                        this.wildcardCommands.Add(command);
                    }
                    else
                    {
                        foreach (string trigger in command.GetFullTriggers())
                        {
                            string t = trigger.ToLower();
                            this.triggersToCommands[t] = command;
                            this.longestTrigger = Math.Max(this.longestTrigger, t.Length);
                        }
                    }

                    SettingsRequirementModel settings = command.Requirements.Settings;
                    if (settings != null && settings.ShowOnChatContextMenu)
                    {
                        this.chatMenuCommands.Add(command);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            this.ChatCommandsReprocessed(this, new EventArgs());
        }

        public async Task AddMessage(ChatMessageViewModel message)
        {
            try
            {
                message.ProcessingStartTime = DateTimeOffset.Now;
                Logger.Log(LogLevel.Debug, string.Format("Message Received - {0} - {1} - {2}", message.ID.ToString(), message.ProcessingStartTime, message));

                // Pre message processing

                if (message is UserChatMessageViewModel)
                {
                    if (message.User != null)
                    {
                        message.User.UpdateLastActivity();
                        if (message.IsWhisper && ChannelSession.Settings.TrackWhispererNumber && !message.IsStreamerOrBot && message.User.WhispererNumber == 0)
                        {
                            await this.whisperNumberLock.WaitAndRelease(() =>
                            {
                                if (!whisperMap.ContainsKey(message.User.ID))
                                {
                                    whisperMap[message.User.ID] = whisperMap.Count + 1;
                                }
                                message.User.WhispererNumber = whisperMap[message.User.ID];
                                return Task.CompletedTask;
                            });
                        }
                    }
                }

                if (message.User != null)
                {
                    await ServiceManager.Get<UserService>().AddOrUpdateActiveUser(message.User);
                }

                if (message.ProcessingTime > 500)
                {
                    Logger.Log(LogLevel.Error, string.Format("Long processing time detected for the following message (AFTER USER ADD/UPDATE): {0} - {1} ms - {2}", message.ID.ToString(), message.ProcessingTime, message));
                }

                // Add message to chat list
                bool showMessage = true;
                if (ChannelSession.Settings.HideBotMessages && message.User != null && message.Platform != StreamingPlatformTypeEnum.None)
                {
                    if (StreamingPlatforms.GetPlatformSessionService(message.Platform).IsBotConnected && string.Equals(message.User?.PlatformID, StreamingPlatforms.GetPlatformSessionService(message.Platform)?.BotID))
                    {
                        showMessage = false;
                    }
                }

                if (!(message is AlertChatMessageViewModel) || !ChannelSession.Settings.OnlyShowAlertsInDashboard)
                {
                    this.messagesLookup[message.ID] = message;
                    if (showMessage)
                    {
                        if (ChannelSession.Settings.LatestChatAtTop)
                        {
                            this.Messages.Insert(0, message);
                        }
                        else
                        {
                            this.Messages.Add(message);
                        }
                    }

                    if (this.Messages.Count > ChannelSession.Settings.MaxMessagesInChat)
                    {
                        ChatMessageViewModel removedMessage = (ChannelSession.Settings.LatestChatAtTop) ? this.Messages.Last() : this.Messages.First();
                        this.messagesLookup.Remove(removedMessage.ID);
                        this.Messages.Remove(removedMessage);
                    }
                }

                if (message.ProcessingTime > 500)
                {
                    Logger.Log(LogLevel.Error, string.Format("Long processing time detected for the following message (AFTER ALERTS): {0} - {1} ms - {2}", message.ID.ToString(), message.ProcessingTime, message));
                }

                // Post message processing

                if (message is UserChatMessageViewModel && message.User != null)
                {
                    await message.User.Refresh();

                    if (message.ProcessingTime > 500)
                    {
                        Logger.Log(LogLevel.Error, string.Format("Long processing time detected for the following message (AFTER USER REFRESH): {0} - {1} ms - {2}", message.ID.ToString(), message.ProcessingTime, message));
                    }

                    if (message.IsWhisper)
                    {
                        if (!message.IsStreamerOrBot)
                        {
                            if (!string.IsNullOrEmpty(ChannelSession.Settings.NotificationChatWhisperSoundFilePath))
                            {
                                await ServiceManager.Get<IAudioService>().Play(ChannelSession.Settings.NotificationChatWhisperSoundFilePath, ChannelSession.Settings.NotificationChatWhisperSoundVolume, ChannelSession.Settings.NotificationsAudioOutput);
                            }

                            if (!string.IsNullOrEmpty(message.PlainTextMessage))
                            {
                                await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.ChatWhisperReceived, new CommandParametersModel(message));
                            }

                            // Don't send this if it's in response to another "You are whisperer #" message
                            if (ChannelSession.Settings.TrackWhispererNumber && message.User.WhispererNumber > 0 && !message.PlainTextMessage.StartsWith("You are whisperer #", StringComparison.InvariantCultureIgnoreCase))
                            {
                                await ServiceManager.Get<ChatService>().Whisper(message.User, string.Format(MixItUp.Base.Resources.ChatWhisperNumberResponse, message.User.WhispererNumber), sendAsStreamer: false);
                            }
                        }
                    }
                    else
                    {
                        message.User.TotalChatMessageSent++;

                        if (this.DisableChat)
                        {
                            Logger.Log(LogLevel.Debug, string.Format("Deleting Message As Chat Disabled - {0} - {1}", message.ID, message));
                            await this.DeleteMessage(message);
                            return;
                        }

                        if (await message.CheckForModeration())
                        {
                            await this.DeleteMessage(message);
                            return;
                        }

                        if (!string.IsNullOrEmpty(ChannelSession.Settings.NotificationChatTaggedSoundFilePath) && message.IsStreamerTagged)
                        {
                            await ServiceManager.Get<IAudioService>().Play(ChannelSession.Settings.NotificationChatTaggedSoundFilePath, ChannelSession.Settings.NotificationChatTaggedSoundVolume, ChannelSession.Settings.NotificationsAudioOutput);
                        }
                        else if (!string.IsNullOrEmpty(ChannelSession.Settings.NotificationChatMessageSoundFilePath) && !message.User.IsSpecialtyExcluded)
                        {
                            await ServiceManager.Get<IAudioService>().Play(ChannelSession.Settings.NotificationChatMessageSoundFilePath, ChannelSession.Settings.NotificationChatMessageSoundVolume, ChannelSession.Settings.NotificationsAudioOutput);
                        }

                        if (message.ProcessingTime > 500)
                        {
                            Logger.Log(LogLevel.Error, string.Format("Long processing time detected for the following message (AFTER MODERATION/NOTIFICATIONS): {0} - {1} ms - {2}", message.ID.ToString(), message.ProcessingTime, message));
                        }

                        if (!this.userEntranceCommands.Contains(message.User.ID))
                        {
                            this.userEntranceCommands.Add(message.User.ID);
                            CommandModelBase customEntranceCommand = ChannelSession.Settings.GetCommand(message.User.EntranceCommandID);
                            if (customEntranceCommand != null && customEntranceCommand.IsEnabled)
                            {
                                await ServiceManager.Get<CommandService>().Queue(message.User.EntranceCommandID, new CommandParametersModel(message));
                            }
                            else if (!message.User.IsSpecialtyExcluded)
                            {
                                await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.ChatUserEntranceCommand, new CommandParametersModel(message));
                            }
                        }

                        if (!string.IsNullOrEmpty(message.PlainTextMessage))
                        {
                            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.ChatMessageReceived, new CommandParametersModel(message));
                        }

                        string primaryTaggedUsername = message.PrimaryTaggedUsername;
                        if (!string.IsNullOrEmpty(primaryTaggedUsername))
                        {
                            UserV2ViewModel primaryTaggedUser = ServiceManager.Get<UserService>().GetActiveUserByPlatformUsername(message.Platform, primaryTaggedUsername);
                            if (primaryTaggedUser != null)
                            {
                                primaryTaggedUser.TotalTimesTagged++;
                            }
                        }

                        if (message.ProcessingTime > 500)
                        {
                            Logger.Log(LogLevel.Error, string.Format("Long processing time detected for the following message (AFTER EVENT COMMANDS): {0} - {1} ms - {2}", message.ID.ToString(), message.ProcessingTime, message));
                        }
                    }

                    GlobalEvents.ChatMessageReceived(message);

                    await this.WriteToChatEventLog(message);

                    if (message.ProcessingTime > 500)
                    {
                        Logger.Log(LogLevel.Error, string.Format("Long processing time detected for the following message (AFTER CHAT MESSAGE GLOBAL EVENT): {0} - {1} ms - {2}", message.ID.ToString(), message.ProcessingTime, message));
                    }

                    IEnumerable<string> arguments = null;
#pragma warning disable CS0612 // Type or member is obsolete
                    if (!string.IsNullOrEmpty(message.PlainTextMessage) && !message.User.HasRole(UserRoleEnum.Banned))
#pragma warning restore CS0612 // Type or member is obsolete
                    {
                        if (!ChannelSession.Settings.AllowCommandWhispering && message.IsWhisper)
                        {
                            return;
                        }

                        if (ChannelSession.Settings.IgnoreBotAccountCommands && message.Platform != StreamingPlatformTypeEnum.None)
                        {
                            if (StreamingPlatforms.GetPlatformSessionService(message.Platform).IsBotConnected && string.Equals(message.User?.PlatformID, StreamingPlatforms.GetPlatformSessionService(message.Platform)?.BotID))
                            {
                                return;
                            }
                        }

                        Logger.Log(LogLevel.Debug, string.Format("Checking Message For Command - {0} - {1}", message.ID, message));

                        bool commandTriggered = false;
                        if (message.User.CustomCommandIDs.Count > 0)
                        {
                            Dictionary<string, CommandModelBase> userOnlyTriggersToCommands = new Dictionary<string, CommandModelBase>();
                            List<ChatCommandModel> userOnlyWildcardCommands = new List<ChatCommandModel>();
                            foreach (Guid commandID in message.User.CustomCommandIDs)
                            {
                                ChatCommandModel command = (ChatCommandModel)ChannelSession.Settings.GetCommand(commandID);
                                if (command != null && command.IsEnabled)
                                {
                                    if (command.Wildcards)
                                    {
                                        userOnlyWildcardCommands.Add(command);
                                    }
                                    else
                                    {
                                        foreach (string trigger in command.GetFullTriggers())
                                        {
                                            userOnlyTriggersToCommands[trigger.ToLower()] = command;
                                        }
                                    }
                                }
                            }

                            if (!commandTriggered && userOnlyTriggersToCommands.Count > 0)
                            {
                                commandTriggered = await this.CheckForChatCommandAndRun(message, userOnlyTriggersToCommands, ignoreTriggerLengthCheck: true);
                            }

                            if (!commandTriggered && userOnlyWildcardCommands.Count > 0)
                            {
                                foreach (ChatCommandModel command in userOnlyWildcardCommands)
                                {
                                    if (command.DoesMessageMatchWildcardTriggers(message, out arguments))
                                    {
                                        await this.RunChatCommand(message, command, arguments);
                                        commandTriggered = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (!commandTriggered)
                        {
                            commandTriggered = await this.CheckForChatCommandAndRun(message, this.triggersToCommands);
                        }

                        if (!commandTriggered)
                        {
                            foreach (ChatCommandModel command in this.wildcardCommands)
                            {
                                if (command.DoesMessageMatchWildcardTriggers(message, out arguments))
                                {
                                    await this.RunChatCommand(message, command, arguments);
                                    commandTriggered = true;
                                    break;
                                }
                            }
                        }

                        if (message.ProcessingTime > 500)
                        {
                            Logger.Log(LogLevel.Error, string.Format("Long processing time detected for the following message (AFTER CHAT COMMAND PROCESSING): {0} - {1} ms - {2}", message.ID.ToString(), message.ProcessingTime, message));
                        }
                    }

                    foreach (InventoryModel inventory in ChannelSession.Settings.Inventory.Values.ToList())
                    {
                        if (inventory.ShopEnabled && ChatCommandModel.DoesMessageMatchTriggers(message, new List<string>() { inventory.ShopCommand }, out arguments))
                        {
                            await inventory.PerformShopCommand(message.User, arguments);
                        }
                        else if (inventory.TradeEnabled && ChatCommandModel.DoesMessageMatchTriggers(message, new List<string>() { inventory.TradeCommand }, out arguments))
                        {
                            await inventory.PerformTradeCommand(message.User, arguments);
                        }
                    }

                    if (ChannelSession.Settings.RedemptionStoreEnabled)
                    {
                        if (ChatCommandModel.DoesMessageMatchTriggers(message, new List<string>() { ChannelSession.Settings.RedemptionStoreChatPurchaseCommand }, out arguments))
                        {
                            await RedemptionStorePurchaseModel.Purchase(message.User, arguments);
                        }
                        else if (ChatCommandModel.DoesMessageMatchTriggers(message, new List<string>() { ChannelSession.Settings.RedemptionStoreModRedeemCommand }, out arguments))
                        {
                            await RedemptionStorePurchaseModel.Redeem(message.User, arguments);
                        }
                    }
                }

                Logger.Log(LogLevel.Debug, string.Format("Message Processing Complete: {0} - {1} ms", message.ID, message.ProcessingTime));
                if (message.ProcessingTime > 500)
                {
                    Logger.Log(LogLevel.Error, string.Format("Long processing time detected for the following message: {0} - {1} ms - {2}", message.ID.ToString(), message.ProcessingTime, message));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public async Task RemoveMessage(string messageID)
        {
            if (!string.IsNullOrEmpty(messageID) && this.messagesLookup.ContainsKey(messageID))
            {
                await this.RemoveMessage(this.messagesLookup[messageID]);
            }
        }

        public Task RemoveMessage(ChatMessageViewModel message)
        {
            this.messagesLookup.Remove(message.ID);
            this.Messages.Remove(message);
            return Task.CompletedTask;
        }

        public async Task WriteToChatEventLog(ChatMessageViewModel message, string prepend = null)
        {
            if (ChannelSession.Settings.SaveChatEventLogs)
            {
                try
                {
                    prepend = prepend ?? string.Empty;
                    await ServiceManager.Get<IFileService>().AppendFile(this.currentChatEventLogFilePath, string.Format($"{prepend}{message} ({DateTime.Now.ToString("HH:mm", CultureInfo.InvariantCulture)})" + Environment.NewLine));
                }
                catch (Exception) { }
            }
        }

        private async Task<bool> CheckForChatCommandAndRun(ChatMessageViewModel message, Dictionary<string, CommandModelBase> commands, bool ignoreTriggerLengthCheck = false)
        {
            string[] messageParts = message.PlainTextMessage.Split(new char[] { ' ' });
            for (int i = 0; i < messageParts.Length; i++)
            {
                string commandCheck = string.Join(" ", messageParts.Take(i + 1)).ToLower();
                if (!ignoreTriggerLengthCheck && commandCheck.Length > this.longestTrigger)
                {
                    return false;
                }

                if (commands.ContainsKey(commandCheck))
                {
                    await this.RunChatCommand(message, commands[commandCheck], messageParts.Skip(i + 1));
                    return true;
                }
            }
            return false;
        }

        private async Task RunChatCommand(ChatMessageViewModel message, CommandModelBase command, IEnumerable<string> arguments)
        {
            Logger.Log(LogLevel.Debug, string.Format("Command Found For Message - {0} - {1} - {2}", message.ID, message, command));

            CommandParametersModel parameters = new CommandParametersModel(message);
            parameters.Arguments = new List<string>(arguments);   // Overwrite arguments to account for variable argument length for commands
            await ServiceManager.Get<CommandService>().Queue(command, parameters);

            SettingsRequirementModel settings = command.Requirements.Settings;
            if (settings != null)
            {
                if (settings != null && settings.ShouldChatMessageBeDeletedWhenRun)
                {
                    await this.DeleteMessage(message);
                }
            }
        }

        private Task ProcessHoursCurrency(CancellationToken cancellationToken)
        {
            Dictionary<StreamingPlatformTypeEnum, bool> liveStreams = new Dictionary<StreamingPlatformTypeEnum, bool>();

            StreamingPlatforms.ForEachPlatform(p =>
            {
                liveStreams[p] = StreamingPlatforms.GetPlatformSessionService(p).IsConnected && StreamingPlatforms.GetPlatformSessionService(p).IsLive;
            });

            if (liveStreams.Any(s => s.Value))
            {
                foreach (UserV2ViewModel user in ServiceManager.Get<UserService>().GetActiveUsers())
                {
                    if (liveStreams.TryGetValue(user.Platform, out bool active) && active)
                    {
                        user.UpdateViewingMinutes(liveStreams);
                    }
                }

                foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
                {
                    currency.UpdateUserData(liveStreams);
                }

                foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
                {
                    streamPass.UpdateUserData(liveStreams);
                }
            }

            return Task.CompletedTask;
        }
    }
}
