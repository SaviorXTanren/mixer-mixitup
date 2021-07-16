using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services.Twitch;
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
    public interface IChatService
    {
        ITwitchChatService TwitchChatService { get; }

        Task Initialize(ITwitchChatService twitchChatService);

        bool DisableChat { get; set; }

        ThreadSafeObservableCollection<ChatMessageViewModel> Messages { get; }

        LockedDictionary<Guid, UserViewModel> AllUsers { get; }
        IEnumerable<UserViewModel> DisplayUsers { get; }
        event EventHandler DisplayUsersUpdated;

        event EventHandler ChatCommandsReprocessed;
        IEnumerable<CommandModelBase> ChatMenuCommands { get; }

        event EventHandler<Dictionary<string, uint>> OnPollEndOccurred;

        Task SendMessage(string message, bool sendAsStreamer = false, StreamingPlatformTypeEnum platform = StreamingPlatformTypeEnum.All, string replyMessageID = null);
        Task Whisper(UserViewModel user, string message, bool sendAsStreamer = false);
        Task Whisper(StreamingPlatformTypeEnum platform, string username, string message, bool sendAsStreamer = false);

        Task DeleteMessage(ChatMessageViewModel message);
        Task ClearMessages();

        Task TimeoutUser(UserViewModel user, uint durationInSeconds);
        Task PurgeUser(UserViewModel user);

        Task ModUser(UserViewModel user);
        Task UnmodUser(UserViewModel user);

        Task BanUser(UserViewModel user);
        Task UnbanUser(UserViewModel user);

        void RebuildCommandTriggers();

        Task AddMessage(ChatMessageViewModel message);
        Task RemoveMessage(string messageID);
        Task RemoveMessage(ChatMessageViewModel message);

        Task WriteToChatEventLog(ChatMessageViewModel message);
    }

    public class ChatService : IChatService
    {
        private const string ChatEventLogDirectoryName = "ChatEventLogs";
        private const string ChatEventLogFileNameFormat = "ChatEventLog-{0}.txt";

        public ITwitchChatService TwitchChatService { get; private set; }

        public bool DisableChat { get; set; }

        public ThreadSafeObservableCollection<ChatMessageViewModel> Messages { get; private set; } = new ThreadSafeObservableCollection<ChatMessageViewModel>();
        private LockedDictionary<string, ChatMessageViewModel> messagesLookup = new LockedDictionary<string, ChatMessageViewModel>();

        public LockedDictionary<Guid, UserViewModel> AllUsers { get; private set; } = new LockedDictionary<Guid, UserViewModel>();
        public IEnumerable<UserViewModel> DisplayUsers
        {
            get
            {
                lock (displayUsersLock)
                {
                    return this.displayUsers.Values.ToList().Take(ChannelSession.Settings.MaxUsersShownInChat);
                }
            }
        }
        public event EventHandler DisplayUsersUpdated = delegate { };
        private SortedList<string, UserViewModel> displayUsers = new SortedList<string, UserViewModel>();
        private object displayUsersLock = new object();

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

        public async Task Initialize(ITwitchChatService twitchChatService)
        {
            this.RebuildCommandTriggers();

            await ChannelSession.Services.FileService.CreateDirectory(ChatEventLogDirectoryName);
            this.currentChatEventLogFilePath = Path.Combine(ChatEventLogDirectoryName, string.Format(ChatEventLogFileNameFormat, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture)));

            if (twitchChatService != null)
            {
                this.TwitchChatService = twitchChatService;

                this.TwitchChatService.OnMessageOccurred += TwitchChatService_OnMessageOccurred;
                this.TwitchChatService.OnUsersJoinOccurred += TwitchChatService_OnUsersJoinOccurred;
                this.TwitchChatService.OnUsersLeaveOccurred += TwitchChatService_OnUsersLeaveOccurred;

                await this.TwitchChatService.Initialize();
            }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            AsyncRunner.RunAsyncBackground(this.ProcessHoursCurrency, this.cancellationTokenSource.Token, 60000);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public async Task SendMessage(string message, bool sendAsStreamer = false, StreamingPlatformTypeEnum platform = StreamingPlatformTypeEnum.All, string replyMessageID = null)
        {
            if (!string.IsNullOrEmpty(message))
            {
                if (platform.HasFlag(StreamingPlatformTypeEnum.Twitch))
                {
                    await this.TwitchChatService.SendMessage(message, sendAsStreamer, replyMessageID);

                    if (sendAsStreamer || ChannelSession.TwitchBotConnection == null)
                    {
                        UserViewModel user = ChannelSession.GetCurrentUser();
                        await this.AddMessage(new TwitchChatMessageViewModel(user, message, replyMessageID));
                    }
                }
            }
        }

        public async Task Whisper(UserViewModel user, string message, bool sendAsStreamer = false)
        {
            if (user != null && !string.IsNullOrEmpty(message))
            {
                if (user.Platform.HasFlag(StreamingPlatformTypeEnum.Twitch) && this.TwitchChatService != null)
                {
                    await this.TwitchChatService.SendWhisperMessage(user, message, sendAsStreamer);
                }
            }
        }

        public async Task Whisper(StreamingPlatformTypeEnum platform, string username, string message, bool sendAsStreamer = false)
        {
            UserViewModel user = ChannelSession.Services.User.GetActiveUserByUsername(username, platform);
            if (user != null)
            {
                await this.Whisper(user, message, sendAsStreamer);
            }
        }

        public async Task DeleteMessage(ChatMessageViewModel message)
        {
            if (message.Platform == StreamingPlatformTypeEnum.Twitch)
            {
                if (!string.IsNullOrEmpty(message.ID))
                {
                    await this.TwitchChatService.DeleteMessage(message);
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
        }

        public async Task ClearMessages()
        {
            this.messagesLookup.Clear();
            this.Messages.Clear();

            await this.TwitchChatService.ClearMessages();
        }

        public async Task PurgeUser(UserViewModel user)
        {
            if (user.Platform == StreamingPlatformTypeEnum.Twitch)
            {
                await this.TwitchChatService.TimeoutUser(user, 1);
            }
        }

        public async Task TimeoutUser(UserViewModel user, uint durationInSeconds)
        {
            if (user.Platform == StreamingPlatformTypeEnum.Twitch)
            {
                await this.TwitchChatService.TimeoutUser(user, (int)durationInSeconds);
            }
        }

        public async Task ModUser(UserViewModel user)
        {
            if (user.Platform == StreamingPlatformTypeEnum.Twitch)
            {
                await this.TwitchChatService.ModUser(user);
            }
        }

        public async Task UnmodUser(UserViewModel user)
        {
            if (user.Platform == StreamingPlatformTypeEnum.Twitch)
            {
                await this.TwitchChatService.UnmodUser(user);
            }
        }

        public async Task BanUser(UserViewModel user)
        {
            if (user.Platform == StreamingPlatformTypeEnum.Twitch)
            {
                await this.TwitchChatService.BanUser(user);
            }
        }

        public async Task UnbanUser(UserViewModel user)
        {
            if (user.Platform == StreamingPlatformTypeEnum.Twitch)
            {
                await this.TwitchChatService.UnbanUser(user);
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
                foreach (ChatCommandModel command in ChannelSession.Services.Command.AllEnabledChatAccessibleCommands)
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
                                return Task.FromResult(0);
                            });
                        }
                    }
                }

                if (message.User != null)
                {
                    await ChannelSession.Services.User.AddOrUpdateActiveUser(message.User);
                }

                // Add message to chat list
                bool showMessage = true;
                if (ChannelSession.Settings.HideBotMessages && message.User != null && ChannelSession.TwitchBotNewAPI != null && message.User.TwitchID.Equals(ChannelSession.TwitchBotNewAPI.id))
                {
                    showMessage = false;
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

                // Post message processing

                if (message is UserChatMessageViewModel && message.User != null)
                {
                    if (message.IsWhisper)
                    {
                        if (!message.IsStreamerOrBot)
                        {
                            if (!string.IsNullOrEmpty(ChannelSession.Settings.NotificationChatWhisperSoundFilePath))
                            {
                                await ChannelSession.Services.AudioService.Play(ChannelSession.Settings.NotificationChatWhisperSoundFilePath, ChannelSession.Settings.NotificationChatWhisperSoundVolume, ChannelSession.Settings.NotificationsAudioOutput);
                            }

                            if (!string.IsNullOrEmpty(message.PlainTextMessage))
                            {
                                await ChannelSession.Services.Events.PerformEvent(EventTypeEnum.ChatWhisperReceived, new CommandParametersModel(message));
                            }

                            // Don't send this if it's in response to another "You are whisperer #" message
                            if (ChannelSession.Settings.TrackWhispererNumber && message.User.WhispererNumber > 0 && !message.PlainTextMessage.StartsWith("You are whisperer #", StringComparison.InvariantCultureIgnoreCase))
                            {
                                await ChannelSession.Services.Chat.Whisper(message.User, $"You are whisperer #{message.User.WhispererNumber}.", sendAsStreamer: false);
                            }
                        }
                    }
                    else
                    {
                        if (this.DisableChat)
                        {
                            Logger.Log(LogLevel.Debug, string.Format("Deleting Message As Chat Disabled - {0} - {1}", message.ID, message));
                            await this.DeleteMessage(message);
                            return;
                        }

                        if (!string.IsNullOrEmpty(ChannelSession.Settings.NotificationChatTaggedSoundFilePath) && message.IsStreamerTagged)
                        {
                            await ChannelSession.Services.AudioService.Play(ChannelSession.Settings.NotificationChatTaggedSoundFilePath, ChannelSession.Settings.NotificationChatTaggedSoundVolume, ChannelSession.Settings.NotificationsAudioOutput);
                        }
                        else if (!string.IsNullOrEmpty(ChannelSession.Settings.NotificationChatMessageSoundFilePath))
                        {
                            await ChannelSession.Services.AudioService.Play(ChannelSession.Settings.NotificationChatMessageSoundFilePath, ChannelSession.Settings.NotificationChatMessageSoundVolume, ChannelSession.Settings.NotificationsAudioOutput);
                        }

                        if (message.User != null && !this.userEntranceCommands.Contains(message.User.ID))
                        {
                            this.userEntranceCommands.Add(message.User.ID);
                            if (ChannelSession.Settings.GetCommand(message.User.Data.EntranceCommandID) != null)
                            {
                                await ChannelSession.Services.Command.Queue(message.User.Data.EntranceCommandID, new CommandParametersModel(message.User, message.Platform, message.ToArguments()));
                            }
                        }

                        if (!string.IsNullOrEmpty(message.PlainTextMessage))
                        {
                            await ChannelSession.Services.Events.PerformEvent(EventTypeEnum.ChatMessageReceived, new CommandParametersModel(message));
                        }

                        message.User.Data.TotalChatMessageSent++;

                        string primaryTaggedUsername = message.PrimaryTaggedUsername;
                        if (!string.IsNullOrEmpty(primaryTaggedUsername))
                        {
                            UserViewModel primaryTaggedUser = ChannelSession.Services.User.GetActiveUserByUsername(primaryTaggedUsername, message.Platform);
                            if (primaryTaggedUser != null)
                            {
                                primaryTaggedUser.Data.TotalTimesTagged++;
                            }
                        }
                    }

                    await message.User.RefreshDetails();

                    if (!message.IsWhisper && await message.CheckForModeration())
                    {
                        await this.DeleteMessage(message);
                        return;
                    }

                    IEnumerable<string> arguments = null;
                    if (!string.IsNullOrEmpty(message.PlainTextMessage) && message.User != null && !message.User.UserRoles.Contains(UserRoleEnum.Banned))
                    {
                        if (!ChannelSession.Settings.AllowCommandWhispering && message.IsWhisper)
                        {
                            return;
                        }

                        if (ChannelSession.Settings.IgnoreBotAccountCommands)
                        {
                            if (ChannelSession.TwitchBotNewAPI != null && message.User.TwitchID.Equals(ChannelSession.TwitchBotNewAPI.id))
                            {
                                return;
                            }
                        }

                        Logger.Log(LogLevel.Debug, string.Format("Checking Message For Command - {0} - {1}", message.ID, message));

                        bool commandTriggered = false;
                        if (message.User.Data.CustomCommandIDs.Count > 0)
                        {
                            Dictionary<string, CommandModelBase> userOnlyTriggersToCommands = new Dictionary<string, CommandModelBase>();
                            List<ChatCommandModel> userOnlyWildcardCommands = new List<ChatCommandModel>();
                            foreach (Guid commandID in message.User.Data.CustomCommandIDs)
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

                            if (!commandTriggered && userOnlyTriggersToCommands.Count > 0)
                            {
                                commandTriggered = await this.CheckForChatCommandAndRun(message, userOnlyTriggersToCommands);
                            }
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

                        if (!commandTriggered)
                        {
                            commandTriggered = await this.CheckForChatCommandAndRun(message, this.triggersToCommands);
                        }
                    }

                    foreach (InventoryModel inventory in ChannelSession.Settings.Inventory.Values.ToList())
                    {
                        if (inventory.ShopEnabled && ChatCommandModel.DoesMessageMatchTriggers(message, new List<string>() { inventory.ShopCommand }, out arguments))
                        {
                            await inventory.PerformShopCommand(message.User, arguments, message.Platform);
                        }
                        else if (inventory.TradeEnabled && ChatCommandModel.DoesMessageMatchTriggers(message, new List<string>() { inventory.TradeCommand }, out arguments))
                        {
                            await inventory.PerformTradeCommand(message.User, arguments, message.Platform);
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

                    GlobalEvents.ChatMessageReceived(message);

                    await this.WriteToChatEventLog(message);
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
            return Task.FromResult(0);
        }

        public async Task WriteToChatEventLog(ChatMessageViewModel message)
        {
            if (ChannelSession.Settings.SaveChatEventLogs)
            {
                try
                {
                    await ChannelSession.Services.FileService.AppendFile(this.currentChatEventLogFilePath, string.Format($"{message} ({DateTime.Now.ToString("HH:mm", CultureInfo.InvariantCulture)})" + Environment.NewLine));
                }
                catch (Exception) { }
            }
        }

        private async Task UsersJoined(IEnumerable<UserViewModel> users)
        {
            List<AlertChatMessageViewModel> alerts = new List<AlertChatMessageViewModel>();

            foreach (UserViewModel user in users)
            {
                this.AllUsers[user.ID] = user;
                lock (displayUsersLock)
                {
                    this.displayUsers[user.SortableID] = user;
                }

                if (users.Count() < 5)
                {
                    alerts.Add(new AlertChatMessageViewModel(user.Platform, user, string.Format(MixItUp.Base.Resources.UserJoinedChat, user.FullDisplayName), ChannelSession.Settings.AlertUserJoinLeaveColor));
                }
            }
            this.DisplayUsersUpdated(this, new EventArgs());

            foreach (AlertChatMessageViewModel alert in alerts)
            {
                await ChannelSession.Services.Alerts.AddAlert(alert);
            }
        }

        private async Task UsersUpdated(IEnumerable<UserViewModel> users)
        {
            await this.UsersLeft(users);
            await this.UsersJoined(users);
        }

        private async Task UsersLeft(IEnumerable<UserViewModel> users)
        {
            List<AlertChatMessageViewModel> alerts = new List<AlertChatMessageViewModel>();

            foreach (UserViewModel user in users)
            {
                if (this.AllUsers.Remove(user.ID))
                {
                    lock (displayUsersLock)
                    {
                        if (!this.displayUsers.Remove(user.SortableID))
                        {
                            int index = this.displayUsers.IndexOfValue(user);
                            if (index >= 0)
                            {
                                this.displayUsers.RemoveAt(index);
                            }
                        }
                    }

                    if (users.Count() < 5)
                    {
                        alerts.Add(new AlertChatMessageViewModel(user.Platform, user, string.Format(MixItUp.Base.Resources.UserLeftChat, user.FullDisplayName), ChannelSession.Settings.AlertUserJoinLeaveColor));
                    }
                }
            }
            this.DisplayUsersUpdated(this, new EventArgs());

            foreach (AlertChatMessageViewModel alert in alerts)
            {
                await ChannelSession.Services.Alerts.AddAlert(alert);
            }
        }

        private async Task<bool> CheckForChatCommandAndRun(ChatMessageViewModel message, Dictionary<string, CommandModelBase> commands)
        {
            string[] messageParts = message.PlainTextMessage.Split(new char[] { ' ' });
            for (int i = 0; i < messageParts.Length; i++)
            {
                string commandCheck = string.Join(" ", messageParts.Take(i + 1)).ToLower();
                if (commandCheck.Length > this.longestTrigger)
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
            parameters.SpecialIdentifiers["message"] = message.PlainTextMessage;
            await ChannelSession.Services.Command.Queue(command, parameters);

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
            foreach (UserViewModel user in ChannelSession.Services.User.GetAllWorkableUsers())
            {
                user.UpdateMinuteData();
            }

            foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
            {
                currency.UpdateUserData();
            }

            foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
            {
                streamPass.UpdateUserData();
            }

            return Task.FromResult(0);
        }

        #region Twitch Event

        private async void TwitchChatService_OnMessageOccurred(object sender, ChatMessageViewModel message)
        {
            await this.AddMessage(message);
        }

        private async void TwitchChatService_OnUsersJoinOccurred(object sender, IEnumerable<UserViewModel> users)
        {
            await this.UsersJoined(users);
        }

        private async void TwitchChatService_OnUsersLeaveOccurred(object sender, IEnumerable<UserViewModel> users)
        {
            await this.UsersLeft(users);
        }

        #endregion Twitch Events
    }
}
