using Mixer.Base.Clients;
using Mixer.Base.Model.Chat;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.MixerAPI
{
    public class ChatClientWrapper : MixerWebSocketWrapper
    {
        private SemaphoreSlim whisperNumberLock = new SemaphoreSlim(1);
        private Dictionary<uint, int> whisperMap = new Dictionary<uint, int>();

        public event EventHandler<ChatMessageViewModel> OnMessageOccurred = delegate { };
        public event EventHandler<ChatDeleteMessageEventModel> OnDeleteMessageOccurred = delegate { };
        public event EventHandler OnClearMessagesOccurred = delegate { };

        public event EventHandler<UserViewModel> OnUserJoinOccurred = delegate { };
        public event EventHandler<UserViewModel> OnUserUpdateOccurred = delegate { };
        public event EventHandler<UserViewModel> OnUserLeaveOccurred = delegate { };
        public event EventHandler<Tuple<UserViewModel, string>> OnUserPurgeOccurred = delegate { };

        public LockedDictionary<Guid, ChatMessageViewModel> Messages { get; private set; } = new LockedDictionary<Guid, ChatMessageViewModel>();

        public bool DisableChat { get; set; }

        public ChatClient Client { get; private set; }
        public ChatClient BotClient { get; private set; }

        private HashSet<uint> userEntranceCommands = new HashSet<uint>();

        public ChatClientWrapper() { }

        public async Task<bool> Connect()
        {
            return await this.AttemptConnect();
        }

        public async Task<bool> ConnectBot()
        {
            if (ChannelSession.BotConnection != null)
            {
                this.BotClient = await this.ConnectAndAuthenticateChatClient(ChannelSession.BotConnection);
                if (this.BotClient != null)
                {
                    this.BotClient.OnMessageOccurred += BotChatClient_OnMessageOccurred;
                    this.BotClient.OnDisconnectOccurred += BotClient_OnDisconnectOccurred;
                    if (ChannelSession.Settings.DiagnosticLogging)
                    {
                        this.BotClient.OnPacketSentOccurred += WebSocketClient_OnPacketSentOccurred;
                        this.BotClient.OnMethodOccurred += WebSocketClient_OnMethodOccurred;
                        this.BotClient.OnReplyOccurred += WebSocketClient_OnReplyOccurred;
                        this.BotClient.OnEventOccurred += WebSocketClient_OnEventOccurred;
                    }
                    return true;
                }
            }
            return false;
        }

        public async Task Disconnect()
        {
            if (this.Client != null)
            {
                this.Client.OnClearMessagesOccurred -= ChatClient_OnClearMessagesOccurred;
                this.Client.OnDeleteMessageOccurred -= ChatClient_OnDeleteMessageOccurred;
                this.Client.OnMessageOccurred -= ChatClient_OnMessageOccurred;
                this.Client.OnPollEndOccurred -= ChatClient_OnPollEndOccurred;
                this.Client.OnPollStartOccurred -= ChatClient_OnPollStartOccurred;
                this.Client.OnPurgeMessageOccurred -= ChatClient_OnPurgeMessageOccurred;
                this.Client.OnUserJoinOccurred -= ChatClient_OnUserJoinOccurred;
                this.Client.OnUserLeaveOccurred -= ChatClient_OnUserLeaveOccurred;
                this.Client.OnUserUpdateOccurred -= ChatClient_OnUserUpdateOccurred;
                this.Client.OnDisconnectOccurred -= StreamerClient_OnDisconnectOccurred;
                if (ChannelSession.Settings.DiagnosticLogging)
                {
                    this.Client.OnPacketSentOccurred -= WebSocketClient_OnPacketSentOccurred;
                    this.Client.OnMethodOccurred -= WebSocketClient_OnMethodOccurred;
                    this.Client.OnReplyOccurred -= WebSocketClient_OnReplyOccurred;
                    this.Client.OnEventOccurred -= WebSocketClient_OnEventOccurred;
                }

                await this.RunAsync(this.Client.Disconnect());

                this.backgroundThreadCancellationTokenSource.Cancel();
            }
            this.Client = null;
        }

        public async Task DisconnectBot()
        {
            if (this.BotClient != null)
            {
                this.BotClient.OnMessageOccurred -= BotChatClient_OnMessageOccurred;
                this.BotClient.OnDisconnectOccurred -= BotClient_OnDisconnectOccurred;
                if (ChannelSession.Settings.DiagnosticLogging)
                {
                    this.BotClient.OnPacketSentOccurred -= WebSocketClient_OnPacketSentOccurred;
                    this.BotClient.OnMethodOccurred -= WebSocketClient_OnMethodOccurred;
                    this.BotClient.OnReplyOccurred -= WebSocketClient_OnReplyOccurred;
                    this.BotClient.OnEventOccurred -= WebSocketClient_OnEventOccurred;
                }

                await this.RunAsync(this.BotClient.Disconnect());
            }
            this.BotClient = null;
        }

        public async Task SendMessage(string message, bool sendAsStreamer = false)
        {
            if (this.GetBotClient(sendAsStreamer) != null)
            {
                message = this.SplitLargeMessage(message, out string subMessage);

                await this.RunAsync(this.GetBotClient(sendAsStreamer).SendMessage(message));

                // Adding delay to prevent messages from arriving in wrong order
                await Task.Delay(250);

                if (!string.IsNullOrEmpty(subMessage))
                {
                    await this.SendMessage(subMessage, sendAsStreamer: sendAsStreamer);
                }
            }
        }

        public async Task Whisper(string username, string message, bool sendAsStreamer = false)
        {
            if (this.GetBotClient(sendAsStreamer) != null)
            {
                message = this.SplitLargeMessage(message, out string subMessage);

                await this.RunAsync(this.GetBotClient(sendAsStreamer).Whisper(username, message));

                // Adding delay to prevent messages from arriving in wrong order
                await Task.Delay(250);

                if (!string.IsNullOrEmpty(subMessage))
                {
                    await this.Whisper(username, subMessage, sendAsStreamer: sendAsStreamer);
                }
            }
        }

        public async Task DeleteMessage(ChatMessageViewModel message)
        {
            if (this.Client != null)
            {
                Util.Logger.LogDiagnostic(string.Format("Deleting Message - {0}", message.Message));

                await this.RunAsync(this.Client.DeleteMessage(message.ID));
            }
        }

        public async Task ClearMessages()
        {
            if (this.Client != null)
            {
                await this.RunAsync(this.Client.ClearMessages());
            }
        }

        public async Task PurgeUser(string username)
        {
            if (this.Client != null)
            {
                await this.RunAsync(this.Client.PurgeUser(username));
            }
        }

        public async Task TimeoutUser(string username, uint durationInSeconds)
        {
            if (this.Client != null)
            {
                await this.RunAsync(this.Client.TimeoutUser(username, durationInSeconds));
            }
        }

        public async Task BanUser(UserViewModel user)
        {
            if (this.Client != null)
            {
                await ChannelSession.Connection.AddUserRoles(ChannelSession.Channel, user.GetModel(), new List<MixerRoleEnum>() { MixerRoleEnum.Banned });
            }
        }

        public async Task UnBanUser(UserViewModel user)
        {
            if (this.Client != null)
            {
                await ChannelSession.Connection.RemoveUserRoles(ChannelSession.Channel, user.GetModel(), new List<MixerRoleEnum>() { MixerRoleEnum.Banned });
            }
        }

        protected override async Task<bool> ConnectInternal()
        {
            if (ChannelSession.Connection != null)
            {
                this.backgroundThreadCancellationTokenSource = new CancellationTokenSource();

                this.Client = await this.ConnectAndAuthenticateChatClient(ChannelSession.Connection);
                if (this.Client != null)
                {
                    this.Client.OnClearMessagesOccurred += ChatClient_OnClearMessagesOccurred;
                    this.Client.OnDeleteMessageOccurred += ChatClient_OnDeleteMessageOccurred;
                    this.Client.OnMessageOccurred += ChatClient_OnMessageOccurred;
                    this.Client.OnPollEndOccurred += ChatClient_OnPollEndOccurred;
                    this.Client.OnPollStartOccurred += ChatClient_OnPollStartOccurred;
                    this.Client.OnPurgeMessageOccurred += ChatClient_OnPurgeMessageOccurred;
                    this.Client.OnUserJoinOccurred += ChatClient_OnUserJoinOccurred;
                    this.Client.OnUserLeaveOccurred += ChatClient_OnUserLeaveOccurred;
                    this.Client.OnUserUpdateOccurred += ChatClient_OnUserUpdateOccurred;
                    this.Client.OnDisconnectOccurred += StreamerClient_OnDisconnectOccurred;
                    if (ChannelSession.Settings.DiagnosticLogging)
                    {
                        this.Client.OnPacketSentOccurred += WebSocketClient_OnPacketSentOccurred;
                        this.Client.OnMethodOccurred += WebSocketClient_OnMethodOccurred;
                        this.Client.OnReplyOccurred += WebSocketClient_OnReplyOccurred;
                        this.Client.OnEventOccurred += WebSocketClient_OnEventOccurred;
                    }

                    await ChannelSession.ActiveUsers.AddOrUpdateUsers(await ChannelSession.Connection.GetChatUsers(ChannelSession.Channel, Math.Max(ChannelSession.Channel.viewersCurrent, 1)));

                    if (ChannelSession.IsStreamer)
                    {
                        ChannelSession.PreMadeChatCommands.Clear();
                        foreach (PreMadeChatCommand command in ReflectionHelper.CreateInstancesOfImplementingType<PreMadeChatCommand>())
                        {
#pragma warning disable CS0612 // Type or member is obsolete
                            if (!(command is ObsoletePreMadeCommand))
                            {
                                ChannelSession.PreMadeChatCommands.Add(command);
                            }
#pragma warning restore CS0612 // Type or member is obsolete
                        }

                        foreach (PreMadeChatCommandSettings commandSetting in ChannelSession.Settings.PreMadeChatCommandSettings)
                        {
                            PreMadeChatCommand command = ChannelSession.PreMadeChatCommands.FirstOrDefault(c => c.Name.Equals(commandSetting.Name));
                            if (command != null)
                            {
                                command.UpdateFromSettings(commandSetting);
                            }
                        }
                    }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Run(async () => { await this.ChannelRefreshBackground(); }, this.backgroundThreadCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Run(async () => { await this.TimerCommandsBackground(); }, this.backgroundThreadCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    return true;
                }
            }
            return false;
        }

        private async Task<ChatClient> ConnectAndAuthenticateChatClient(MixerConnectionWrapper connection)
        {
            ChatClient client = await this.RunAsync(ChatClient.CreateFromChannel(connection.Connection, ChannelSession.Channel));
            if (client != null)
            {
                if (await this.RunAsync(client.Connect()) && await this.RunAsync(client.Authenticate()))
                {
                    return client;
                }
                else
                {
                    MixItUp.Base.Util.Logger.Log("Failed to connect & authenticate Chat client");
                }
            }
            return null;
        }

        private ChatClient GetBotClient(bool sendAsStreamer = false) { return (this.BotClient != null && !sendAsStreamer) ? this.BotClient : this.Client; }

        private string SplitLargeMessage(string message, out string subMessage)
        {
            subMessage = null;
            if (message.Length > 360)
            {
                string message360 = message.Substring(0, 360);
                int splitIndex = message360.LastIndexOf(' ');
                if (splitIndex > 0 && (splitIndex + 1) < message.Length)
                {
                    subMessage = message.Substring(splitIndex + 1);
                    message = message.Substring(0, splitIndex);
                }
            }
            return message;
        }

        #region Chat Update Methods

        private async Task<UserViewModel> AddUser(ChatUserEventModel chatUserEvent)
        {
            return await this.AddUser(new ChatUserModel() { userId = chatUserEvent.id, userName = chatUserEvent.username, userRoles = chatUserEvent.roles });
        }

        public async Task<UserViewModel> AddUser(ChatMessageEventModel chatMessageEvent)
        {
            return await this.AddUser(new ChatUserModel() { userId = chatMessageEvent.user_id, userName = chatMessageEvent.user_name, userRoles = chatMessageEvent.user_roles });
        }

        private async Task<UserViewModel> AddUser(ChatUserModel chatUser)
        {
            return await ChannelSession.ActiveUsers.AddOrUpdateUser(chatUser);
        }

        private async Task<ChatMessageViewModel> AddMessage(ChatMessageEventModel messageEvent)
        {
            UserViewModel user = await this.AddUser(messageEvent);
            if (user == null)
            {
                user = new UserViewModel(messageEvent);
            }
            ChatMessageViewModel message = new ChatMessageViewModel(messageEvent, user);

            Util.Logger.LogDiagnostic(string.Format("Message Received - {0}", message.ToString()));

            if (!message.IsWhisper && !this.userEntranceCommands.Contains(user.ID))
            {
                this.userEntranceCommands.Add(user.ID);
                if (user.Data.EntranceCommand != null)
                {
                    await user.Data.EntranceCommand.Perform(user);
                }
            }

            if (this.Messages.ContainsKey(message.ID))
            {
                return null;
            }
            this.Messages[message.ID] = message;

            if (this.DisableChat && !message.ID.Equals(Guid.Empty))
            {
                Util.Logger.LogDiagnostic(string.Format("Deleting Message As Chat Disabled - {0}", message.Message));
                await this.DeleteMessage(message);
                return message;
            }

            if (!await ModerationHelper.MeetsChatInteractiveParticipationRequirement(user))
            {
                Util.Logger.LogDiagnostic(string.Format("Deleting Message As User does not meet requirement - {0} - {1}", ChannelSession.Settings.ModerationChatInteractiveParticipation, message.Message));

                await this.DeleteMessage(message);

                await ModerationHelper.SendChatInteractiveParticipationWhisper(user, isChat: true);

                return message;
            }

            string moderationReason = await message.ShouldBeModerated();
            if (!string.IsNullOrEmpty(moderationReason))
            {
                Util.Logger.LogDiagnostic(string.Format("Message Should Be Moderated - {0}", message.ToString()));

                bool shouldBeModerated = true;
                PermissionsCommandBase command = this.CheckMessageForCommand(message);
                if (command != null && string.IsNullOrEmpty(await ModerationHelper.ShouldBeFilteredWordModerated(user, message.Message)))
                {
                    shouldBeModerated = false;
                }

                if (shouldBeModerated)
                {
                    Util.Logger.LogDiagnostic(string.Format("Moderation Being Performed - {0}", message.ToString()));

                    message.ModerationReason = moderationReason;

                    await this.DeleteMessage(message);

                    await user.AddModerationStrike(moderationReason);

                    return message;
                }
            }

            if (!string.IsNullOrEmpty(ChannelSession.Settings.NotificationChatMessageSoundFilePath))
            {
                await ChannelSession.Services.AudioService.Play(ChannelSession.Settings.NotificationChatMessageSoundFilePath, 100);
            }

            if (!string.IsNullOrEmpty(ChannelSession.Settings.NotificationChatTaggedSoundFilePath) && message.IsUserTagged)
            {
                await ChannelSession.Services.AudioService.Play(ChannelSession.Settings.NotificationChatTaggedSoundFilePath, 100);
            }

            if (!string.IsNullOrEmpty(ChannelSession.Settings.NotificationChatWhisperSoundFilePath) && message.IsWhisper)
            {
                await ChannelSession.Services.AudioService.Play(ChannelSession.Settings.NotificationChatWhisperSoundFilePath, 100);
            }

            if (!await this.CheckMessageForCommandAndRun(message))
            {
                if (message.IsWhisper && ChannelSession.Settings.TrackWhispererNumber && !message.IsStreamerOrBot())
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

                    await ChannelSession.Chat.Whisper(message.User.UserName, $"You are whisperer #{message.User.WhispererNumber}.", false);
                }
            }

            return message;
        }

        private async Task<bool> CheckMessageForCommandAndRun(ChatMessageViewModel message)
        {
            PermissionsCommandBase command = this.CheckMessageForCommand(message);
            if (command != null)
            {
                Util.Logger.LogDiagnostic(string.Format("Command Found For Message - {0} - {1}", message.ToString(), command.ToString()));

                await this.RunMessageCommand(message, command);
                return true;
            }
            return false;
        }

        private PermissionsCommandBase CheckMessageForCommand(ChatMessageViewModel message)
        {
            Util.Logger.LogDiagnostic(string.Format("Checking Message For Command - {0}", message.ToString()));

            if (!ChannelSession.Settings.AllowCommandWhispering && message.IsWhisper)
            {
                return null;
            }

            if (ChannelSession.BotUser != null && ChannelSession.Settings.IgnoreBotAccountCommands && message.User != null && message.User.ID.Equals(ChannelSession.BotUser.id))
            {
                return null;
            }

            if (ChannelSession.Settings.CommandsOnlyInYourStream && !message.IsInUsersChannel)
            {
                return null;
            }

            if (ChannelSession.IsStreamer && !message.User.MixerRoles.Contains(MixerRoleEnum.Banned))
            {
                GlobalEvents.ChatCommandMessageReceived(message);

                List<PermissionsCommandBase> commandsToCheck = new List<PermissionsCommandBase>(ChannelSession.AllEnabledChatCommands);
                commandsToCheck.AddRange(message.User.Data.CustomCommands);

                PermissionsCommandBase command = commandsToCheck.FirstOrDefault(c => c.MatchesCommand(message.Message));
                if (command == null)
                {
                    command = commandsToCheck.FirstOrDefault(c => c.ContainsCommand(message.Message));
                }

                return command;
            }

            return null;
        }

        private async Task RunMessageCommand(ChatMessageViewModel message, PermissionsCommandBase command)
        {
            await command.Perform(message.User, command.GetArgumentsFromText(message.Message));

            if (ChannelSession.Settings.DeleteChatCommandsWhenRun)
            {
                Util.Logger.LogDiagnostic(string.Format("Deleting Message As Chat Command - {0}", message.Message));
                await this.DeleteMessage(message);
            }
        }

        #endregion Chat Update Methods

        #region Refresh Methods

        private async Task ChannelRefreshBackground()
        {
            await BackgroundTaskWrapper.RunBackgroundTask(this.backgroundThreadCancellationTokenSource, async (tokenSource) =>
            {
                tokenSource.Token.ThrowIfCancellationRequested();

                await ChannelSession.RefreshChannel();
                await Task.Delay(30000, tokenSource.Token);

                tokenSource.Token.ThrowIfCancellationRequested();

                await ChannelSession.RefreshChannel();
                await Task.Delay(30000, tokenSource.Token);

                tokenSource.Token.ThrowIfCancellationRequested();

                foreach (UserViewModel user in await ChannelSession.ActiveUsers.GetAllWorkableUsers())
                {
                    user.UpdateMinuteData();
                }

                foreach (UserCurrencyViewModel currency in ChannelSession.Settings.Currencies.Values)
                {
                    await currency.UpdateUserData();
                }

                await ChannelSession.SaveSettings();

                tokenSource.Token.ThrowIfCancellationRequested();

                await ChannelSession.SaveSettings();
            });
        }

        private async Task TimerCommandsBackground()
        {
            int timerCommandIndex = 0;
            await BackgroundTaskWrapper.RunBackgroundTask(this.backgroundThreadCancellationTokenSource, async (tokenSource) =>
            {
                DateTimeOffset startTime = DateTimeOffset.Now;
                int startMessageCount = this.Messages.Count;

                tokenSource.Token.ThrowIfCancellationRequested();

                await Task.Delay(1000 * 60 * ChannelSession.Settings.TimerCommandsInterval, tokenSource.Token);

                tokenSource.Token.ThrowIfCancellationRequested();

                IEnumerable<TimerCommand> timers = ChannelSession.Settings.TimerCommands.Where(c => c.IsEnabled);
                if (timers.Count() > 0)
                {
                    timerCommandIndex = timerCommandIndex % timers.Count();
                    TimerCommand command = timers.ElementAt(timerCommandIndex);

                    while ((this.Messages.Count - startMessageCount) < ChannelSession.Settings.TimerCommandsMinimumMessages)
                    {
                        tokenSource.Token.ThrowIfCancellationRequested();
                        await Task.Delay(1000 * 10, tokenSource.Token);
                    }

                    await command.Perform();

                    timerCommandIndex++;
                }
            });
        }

        #endregion Refresh Methods

        #region Chat Event Handlers

        private async void ChatClient_OnMessageOccurred(object sender, ChatMessageEventModel e)
        {
            ChatMessageViewModel message = await this.AddMessage(e);
            if (message != null)
            {
                this.OnMessageOccurred(sender, message);
            }
        }

        private async void BotChatClient_OnMessageOccurred(object sender, ChatMessageEventModel e)
        {
            ChatMessageViewModel message = new ChatMessageViewModel(e);
            if (message.IsWhisper)
            {
                message = await this.AddMessage(e);
                if (message != null)
                {
                    this.OnMessageOccurred(sender, message);
                }
            }
        }

        private void ChatClient_OnDeleteMessageOccurred(object sender, ChatDeleteMessageEventModel e)
        {
            this.OnDeleteMessageOccurred(sender, e);
        }

        private async void ChatClient_OnPurgeMessageOccurred(object sender, ChatPurgeMessageEventModel e)
        {
            UserViewModel user = await ChannelSession.ActiveUsers.GetUserByID(e.user_id);
            if (user != null)
            {
                this.OnUserPurgeOccurred(sender, new Tuple<UserViewModel, string>(user, e.moderator?.user_name));

                if (ChannelSession.Constellation.CanUserRunEvent(user, EnumHelper.GetEnumName(OtherEventTypeEnum.MixerUserPurge)))
                {
                    ChannelSession.Constellation.LogUserRunEvent(user, EnumHelper.GetEnumName(OtherEventTypeEnum.MixerUserPurge));
                    await ChannelSession.Constellation.RunEventCommand(ChannelSession.Constellation.FindMatchingEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.MixerUserPurge)), user);
                }
            }
        }

        private void ChatClient_OnClearMessagesOccurred(object sender, ChatClearMessagesEventModel e)
        {
            this.OnClearMessagesOccurred(sender, new EventArgs());
        }

        private void ChatClient_OnPollStartOccurred(object sender, ChatPollEventModel e) { }

        private void ChatClient_OnPollEndOccurred(object sender, ChatPollEventModel e) { }

        private async void ChatClient_OnUserJoinOccurred(object sender, ChatUserEventModel e)
        {
            UserViewModel user = await this.AddUser(e);
            if (user != null)
            {
                this.OnUserJoinOccurred(sender, user);
            }
        }

        private async void ChatClient_OnUserLeaveOccurred(object sender, ChatUserEventModel e)
        {
            UserViewModel user = await ChannelSession.ActiveUsers.RemoveUser(e.id);
            if (user != null)
            {
                this.OnUserLeaveOccurred(sender, user);
            }
        }

        private async void ChatClient_OnUserUpdateOccurred(object sender, ChatUserEventModel e)
        {
            UserViewModel user = await this.AddUser(e);
            if (user != null)
            {
                this.OnUserUpdateOccurred(sender, user);

                if (e.roles != null && e.roles.Count() > 0 && e.roles.Where(r => !string.IsNullOrEmpty(r)).Contains(EnumHelper.GetEnumName(MixerRoleEnum.Banned)))
                {
                    if (ChannelSession.Constellation.CanUserRunEvent(user, EnumHelper.GetEnumName(OtherEventTypeEnum.MixerUserBan)))
                    {
                        ChannelSession.Constellation.LogUserRunEvent(user, EnumHelper.GetEnumName(OtherEventTypeEnum.MixerUserBan));
                        await ChannelSession.Constellation.RunEventCommand(ChannelSession.Constellation.FindMatchingEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.MixerUserBan)), user);
                    }
                }
            }
        }

        private async void StreamerClient_OnDisconnectOccurred(object sender, WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred("Streamer Chat");

            // Force background tasks to stop before reconnecting
            this.backgroundThreadCancellationTokenSource.Cancel();

            do
            {
                await Task.Delay(2500);
            }
            while (!await this.Connect());

            ChannelSession.ReconnectionOccurred("Streamer Chat");
        }

        private async void BotClient_OnDisconnectOccurred(object sender, WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred("Bot Chat");

            do
            {
                await Task.Delay(2500);
            }
            while (!await this.ConnectBot());

            ChannelSession.ReconnectionOccurred("Bot Chat");
        }

        #endregion Chat Event Handlers
    }
}
