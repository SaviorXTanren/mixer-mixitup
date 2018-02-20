using Mixer.Base.Clients;
using Mixer.Base.Model.Chat;
using Mixer.Base.Model.User;
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
        private const string HypeBotUserName = "HypeBot";
        private const string BoomTVUserName = "boomtvmod";

        public event EventHandler<ChatMessageViewModel> OnMessageOccurred = delegate { };
        public event EventHandler<Guid> OnDeleteMessageOccurred = delegate { };
        public event EventHandler<uint> OnPurgeMessageOccurred = delegate { };
        public event EventHandler OnClearMessagesOccurred = delegate { };

        public event EventHandler<UserViewModel> OnUserJoinOccurred = delegate { };
        public event EventHandler<UserViewModel> OnUserUpdateOccurred = delegate { };
        public event EventHandler<UserViewModel> OnUserLeaveOccurred = delegate { };

        private CancellationTokenSource backgroundThreadCancellationTokenSource = new CancellationTokenSource();

        public LockedDictionary<uint, UserViewModel> ChatUsers { get; private set; }
        public Dictionary<Guid, ChatMessageViewModel> Messages { get; private set; }

        public bool DisableChat { get; set; }

        public ChatClient Client { get; private set; }
        public ChatClient BotClient { get; private set; }

        private object userUpdateLock = new object();
        private object messageUpdateLock = new object();

        public ChatClientWrapper()
        {
            this.ChatUsers = new LockedDictionary<uint, UserViewModel>();
            this.Messages = new Dictionary<Guid, ChatMessageViewModel>();
        }

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
                this.Client.OnUserTimeoutOccurred -= ChatClient_OnUserTimeoutOccurred;
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

                if (!string.IsNullOrEmpty(subMessage))
                {
                    await this.Whisper(username, subMessage, sendAsStreamer: sendAsStreamer);
                }
            }
        }

        public async Task DeleteMessage(Guid id)
        {
            if (this.Client != null)
            {
                await this.RunAsync(this.Client.DeleteMessage(id));
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

        public async Task UpdateEachUser(Func<UserViewModel, Task> userUpdateFunction, bool includeBots = false)
        {
            List<UserViewModel> users = this.ChatUsers.Values.ToList();

            if (!includeBots)
            {
                users.RemoveAll(u => HypeBotUserName.Equals(u.UserName));
                users.RemoveAll(u => BoomTVUserName.Equals(u.UserName));
                if (ChannelSession.BotUser != null)
                {
                    users.RemoveAll(u => ChannelSession.BotUser.username.Equals(u.UserName));
                }
            }

            foreach (UserViewModel user in users)
            {
                await userUpdateFunction(user);
            }

            await ChannelSession.SaveSettings();
        }

        protected override async Task<bool> ConnectInternal()
        {
            if (ChannelSession.Connection != null)
            {
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
                    this.Client.OnUserTimeoutOccurred += ChatClient_OnUserTimeoutOccurred;
                    this.Client.OnUserUpdateOccurred += ChatClient_OnUserUpdateOccurred;
                    this.Client.OnDisconnectOccurred += StreamerClient_OnDisconnectOccurred;
                    if (ChannelSession.Settings.DiagnosticLogging)
                    {
                        this.Client.OnPacketSentOccurred += WebSocketClient_OnPacketSentOccurred;
                        this.Client.OnMethodOccurred += WebSocketClient_OnMethodOccurred;
                        this.Client.OnReplyOccurred += WebSocketClient_OnReplyOccurred;
                        this.Client.OnEventOccurred += WebSocketClient_OnEventOccurred;
                    }

                    foreach (ChatUserModel chatUser in await ChannelSession.Connection.GetChatUsers(ChannelSession.Channel, Math.Max(ChannelSession.Channel.viewersCurrent, 1)))
                    {
                        UserViewModel user = new UserViewModel(chatUser);
                        await user.SetDetails(checkForFollow: false);
                        this.ChatUsers[user.ID] = user;
                    }

                    if (this.ChatUsers.Count > 0)
                    {
                        Dictionary<UserModel, DateTimeOffset?> chatFollowers = await ChannelSession.Connection.CheckIfFollows(ChannelSession.Channel, this.ChatUsers.Values.Select(u => u.GetModel()));
                        foreach (var kvp in chatFollowers)
                        {
                            try
                            {
                                if (this.ChatUsers.ContainsKey(kvp.Key.id) && kvp.Value != null)
                                {
                                    this.ChatUsers[kvp.Key.id].SetFollowDate();
                                }
                            }
                            catch (Exception ex) { Logger.Log(ex); }
                        }
                    }

                    if (ChannelSession.IsStreamer)
                    {
                        ChannelSession.PreMadeChatCommands.Clear();
                        foreach (PreMadeChatCommand command in ReflectionHelper.CreateInstancesOfImplementingType<PreMadeChatCommand>())
                        {
                            ChannelSession.PreMadeChatCommands.Add(command);
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
                    Task.Run(async () => { await this.ChatUserRefreshBackground(); }, this.backgroundThreadCancellationTokenSource.Token);
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
                    Logger.Log("Failed to connect & authenticate Chat client");
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

        public async Task GetAndAddUser(uint userID)
        {
            ChatUserModel user = await ChannelSession.Connection.GetChatUser(ChannelSession.Channel, userID);
            if (user != null)
            {
                await this.AddUser(new UserViewModel(user));
            }
        }

        private async Task AddUser(UserViewModel user)
        {
            await user.SetDetails();
            lock (userUpdateLock)
            {
                if (!this.ChatUsers.ContainsKey(user.ID))
                {
                    this.ChatUsers[user.ID] = user;
                }
            }
        }

        private void RemoveUser(UserViewModel user)
        {
            lock (userUpdateLock)
            {
                this.ChatUsers.Remove(user.ID);
            }
        }

        private async Task<bool> AddMessage(ChatMessageViewModel message)
        {
            if (this.Messages.ContainsKey(message.ID))
            {
                return false;
            }
            this.Messages[message.ID] = message;

            if (!this.ChatUsers.ContainsKey(message.User.ID))
            {
                await this.AddUser(message.User);
            }

            if (this.DisableChat && !message.ID.Equals(Guid.Empty))
            {
                await this.DeleteMessage(message.ID);
                return true;
            }

            string moderationReason;
            if (message.ShouldBeModerated(out moderationReason))
            {
                await this.DeleteMessage(message.ID);

                string whisperMessage = " due to chat moderation for the following reason: " + moderationReason + ". Please watch what you type in chat or further actions may be taken.";

                message.User.ChatOffenses++;
                if (ChannelSession.Settings.ModerationTimeout5MinuteOffenseCount > 0 && message.User.ChatOffenses >= ChannelSession.Settings.ModerationTimeout5MinuteOffenseCount)
                {
                    await this.Whisper(message.User.UserName, "You have been timed out from chat for 5 minutes" + whisperMessage);
                    await this.TimeoutUser(message.User.UserName, 300);
                }
                else if (ChannelSession.Settings.ModerationTimeout1MinuteOffenseCount > 0 && message.User.ChatOffenses >= ChannelSession.Settings.ModerationTimeout1MinuteOffenseCount)
                {
                    await this.Whisper(message.User.UserName, "You have been timed out from chat for 1 minute" + whisperMessage);
                    await this.TimeoutUser(message.User.UserName, 60);
                }
                else
                {
                    await this.Whisper(message.User.UserName, "Your message has been deleted" + whisperMessage);
                }
                return true;
            }

            if (ChannelSession.IsStreamer && ChatMessageCommandViewModel.IsCommand(message) && !message.User.Roles.Contains(UserRole.Banned))
            {
                ChatMessageCommandViewModel messageCommand = new ChatMessageCommandViewModel(message);

                GlobalEvents.ChatCommandMessageReceived(messageCommand);

                PermissionsCommandBase command = ChannelSession.AllChatCommands.FirstOrDefault(c => c.ContainsCommand(messageCommand.CommandName));
                if (command != null)
                {
                    await command.Perform(message.User, messageCommand.CommandArguments);
                }
            }

            return true;
        }

        #endregion Chat Update Methods

        #region Refresh Methods

        private async Task ChannelRefreshBackground()
        {
            await BackgroundTaskWrapper.RunBackgroundTask(this.backgroundThreadCancellationTokenSource, async (tokenSource) =>
            {
                tokenSource.Token.ThrowIfCancellationRequested();

                await ChannelSession.RefreshChannel();
                await Task.Delay(30000);

                tokenSource.Token.ThrowIfCancellationRequested();

                await ChannelSession.RefreshChannel();
                await Task.Delay(30000);

                tokenSource.Token.ThrowIfCancellationRequested();

                List<UserCurrencyViewModel> currenciesToUpdate = new List<UserCurrencyViewModel>();
                foreach (UserCurrencyViewModel currency in ChannelSession.Settings.Currencies.Values)
                {
                    if (currency.IsActive)
                    {
                        currenciesToUpdate.Add(currency);
                    }
                }

                await this.UpdateEachUser((user) =>
                {
                    user.Data.ViewingMinutes++;
                    foreach (UserCurrencyViewModel currency in currenciesToUpdate)
                    {
                        if ((user.Data.ViewingMinutes % currency.AcquireInterval) == 0)
                        {
                            user.Data.AddCurrencyAmount(currency, currency.AcquireAmount);
                            if (user.Roles.Contains(UserRole.Subscriber))
                            {
                                user.Data.AddCurrencyAmount(currency, currency.SubscriberBonus);
                            }
                        }
                    }
                    return Task.FromResult(0);
                });

                tokenSource.Token.ThrowIfCancellationRequested();

                await ChannelSession.SaveSettings();
            });
        }

        private async Task ChatUserRefreshBackground()
        {
            Dictionary<uint, UserViewModel> users = this.ChatUsers.ToDictionary();
            if (users.Count > 0)
            {
                foreach (UserViewModel user in users.Values)
                {
                    await user.SetDetails(checkForFollow: false);
                }

                Dictionary<UserModel, DateTimeOffset?> chatFollowers = await ChannelSession.Connection.CheckIfFollows(ChannelSession.Channel, users.Values.Select(u => u.GetModel()));
                foreach (var kvp in chatFollowers)
                {
                    users[kvp.Key.id].SetFollowDate(kvp.Value);
                }
            }
        }

        private async Task TimerCommandsBackground()
        {
            int timerCommandIndex = 0;
            await BackgroundTaskWrapper.RunBackgroundTask(this.backgroundThreadCancellationTokenSource, async (tokenSource) =>
            {
                DateTimeOffset startTime = DateTimeOffset.Now;
                int startMessageCount = this.Messages.Count;

                tokenSource.Token.ThrowIfCancellationRequested();

                await Task.Delay(1000 * 60 * ChannelSession.Settings.TimerCommandsInterval);

                tokenSource.Token.ThrowIfCancellationRequested();

                if (ChannelSession.Settings.TimerCommands.Count > 0)
                {
                    TimerCommand command = ChannelSession.Settings.TimerCommands[timerCommandIndex];

                    while ((this.Messages.Count - startMessageCount) < ChannelSession.Settings.TimerCommandsMinimumMessages)
                    {
                        tokenSource.Token.ThrowIfCancellationRequested();
                        await Task.Delay(1000 * 10);
                    }

                    await command.Perform();

                    timerCommandIndex++;
                    timerCommandIndex = timerCommandIndex % ChannelSession.Settings.TimerCommands.Count;
                }
            });
        }

        #endregion Refresh Methods

        #region Chat Event Handlers

        private async void ChatClient_OnMessageOccurred(object sender, ChatMessageEventModel e)
        {
            ChatMessageViewModel message = new ChatMessageViewModel(e);
            if (await this.AddMessage(message))
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(() => { this.OnMessageOccurred(sender, message); });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        private async void ChatClient_OnDeleteMessageOccurred(object sender, ChatDeleteMessageEventModel e)
        {
            await this.DeleteMessage(e.id);

            this.OnDeleteMessageOccurred(sender, e.id);
        }

        private void ChatClient_OnPurgeMessageOccurred(object sender, ChatPurgeMessageEventModel e)
        {
            this.OnPurgeMessageOccurred(sender, e.user_id);
        }

        private void ChatClient_OnClearMessagesOccurred(object sender, ChatClearMessagesEventModel e)
        {
            this.OnClearMessagesOccurred(sender, new EventArgs());
        }

        private void ChatClient_OnPollStartOccurred(object sender, ChatPollEventModel e) { }

        private void ChatClient_OnPollEndOccurred(object sender, ChatPollEventModel e) { }

        private async void ChatClient_OnUserJoinOccurred(object sender, ChatUserEventModel e)
        {
            UserViewModel user = new UserViewModel(e);
            await this.AddUser(user);

            this.OnUserJoinOccurred(sender, user);
        }

        private void ChatClient_OnUserLeaveOccurred(object sender, ChatUserEventModel e)
        {
            UserViewModel user = new UserViewModel(e);
            this.RemoveUser(user);

            this.OnUserLeaveOccurred(sender, user);
        }

        private void ChatClient_OnUserTimeoutOccurred(object sender, ChatUserEventModel e) { }

        private async void ChatClient_OnUserUpdateOccurred(object sender, ChatUserEventModel e)
        {
            UserViewModel user = new UserViewModel(e);
            this.RemoveUser(user);

            ChatUserModel chatUser = await ChannelSession.Connection.GetChatUser(ChannelSession.Channel, user.ID);
            if (chatUser != null)
            {
                user = new UserViewModel(chatUser);
                await this.AddUser(user);

                this.OnUserUpdateOccurred(sender, user);
            }
        }

        private async void StreamerClient_OnDisconnectOccurred(object sender, WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred("Streamer Chat");

            do
            {
                await this.Disconnect();

                await Task.Delay(2000);
            } while (!await this.Connect());

            ChannelSession.ReconnectionOccurred("Streamer Chat");
        }

        private async void BotClient_OnDisconnectOccurred(object sender, WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred("Bot Chat");

            do
            {
                await this.DisconnectBot();

                await Task.Delay(2000);
            } while (!await this.ConnectBot());

            ChannelSession.ReconnectionOccurred("Bot Chat");
        }

        #endregion Chat Event Handlers
    }
}
