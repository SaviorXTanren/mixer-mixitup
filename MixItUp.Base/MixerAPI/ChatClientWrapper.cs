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
    public class ChatClientWrapper : MixerRequestWrapperBase
    {
        public event EventHandler<ChatMessageViewModel> OnMessageOccurred = delegate { };
        public event EventHandler<Guid> OnDeleteMessageOccurred = delegate { };
        public event EventHandler<uint> OnPurgeMessageOccurred = delegate { };
        public event EventHandler OnClearMessagesOccurred = delegate { };

        public event EventHandler<UserViewModel> OnUserJoinOccurred = delegate { };
        public event EventHandler<UserViewModel> OnUserUpdateOccurred = delegate { };
        public event EventHandler<UserViewModel> OnUserLeaveOccurred = delegate { };

        private CancellationTokenSource backgroundThreadCancellationTokenSource = new CancellationTokenSource();

        public LockedDictionary<uint, UserViewModel> ChatUsers { get; private set; }
        public List<ChatMessageViewModel> Messages { get; private set; }

        public bool DisableChat { get; set; }

        public ChatClient Client { get; private set; }
        public ChatClient BotClient { get; private set; }

        private object userUpdateLock = new object();
        private object messageUpdateLock = new object();

        public ChatClientWrapper()
        {
            this.ChatUsers = new LockedDictionary<uint, UserViewModel>();
            this.Messages = new List<ChatMessageViewModel>();
        }

        public async Task<bool> Connect()
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

                    Dictionary<UserModel, DateTimeOffset?> chatFollowers = await ChannelSession.Connection.CheckIfFollows(ChannelSession.Channel, this.ChatUsers.Values.Select(u => u.GetModel()));
                    foreach (var kvp in chatFollowers)
                    {
                        this.ChatUsers[kvp.Key.id].SetFollowDate(kvp.Value);
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
                    this.BotClient.OnMethodOccurred -= WebSocketClient_OnMethodOccurred;
                    this.BotClient.OnReplyOccurred -= WebSocketClient_OnReplyOccurred;
                    this.BotClient.OnEventOccurred -= WebSocketClient_OnEventOccurred;
                }

                await this.RunAsync(this.BotClient.Disconnect());
            }
            this.BotClient = null;
        }

        public async Task SendMessage(string message, bool sendAsStreamer = false) { await this.RunAsync(this.GetBotClient(sendAsStreamer).SendMessage(message)); }

        public async Task Whisper(string username, string message, bool sendAsStreamer = false) { await this.RunAsync(this.GetBotClient(sendAsStreamer).Whisper(username, message)); }

        public async Task DeleteMessage(Guid id) { await this.RunAsync(this.Client.DeleteMessage(id)); }

        public async Task ClearMessages() { await this.RunAsync(this.Client.ClearMessages()); }

        public async Task PurgeUser(string username) { await this.RunAsync(this.Client.PurgeUser(username)); }

        public async Task TimeoutUser(string username, uint durationInSeconds) { await this.RunAsync(this.Client.TimeoutUser(username, durationInSeconds)); }

        public async Task UpdateEachUser(Func<UserViewModel, Task> userUpdateFunction)
        {
            foreach (UserViewModel chatUser in this.ChatUsers.Values.ToList())
            {
                await userUpdateFunction(chatUser);
            }
            await ChannelSession.SaveSettings();
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
            }
            return null;
        }

        private ChatClient GetBotClient(bool sendAsStreamer = false) { return (this.BotClient != null && !sendAsStreamer) ? this.BotClient : this.Client; }

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

        private async Task AddMessage(ChatMessageViewModel message)
        {
            if (!this.ChatUsers.ContainsKey(message.User.ID))
            {
                await this.AddUser(message.User);
            }

            if (this.DisableChat && !message.ID.Equals(Guid.Empty))
            {
                await this.DeleteMessage(message.ID);
                return;
            }

            string moderationReason;
            if (message.ShouldBeModerated(out moderationReason))
            {
                await this.DeleteMessage(message.ID);

                string whisperMessage = " due to chat moderation for the following reason: " + moderationReason + ". Please watch what you type in chat or further actions will be taken.";

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
                return;
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
            await this.AddMessage(message);

            this.OnMessageOccurred(sender, message);
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
            this.Messages.Clear();

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
            await this.AddUser(user);

            this.OnUserUpdateOccurred(sender, user);
        }

        private async void StreamerClient_OnDisconnectOccurred(object sender, WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred();

            do
            {
                await this.Disconnect();

                await Task.Delay(2000);
            } while (!await this.Connect());

            ChannelSession.ReconnectionOccurred();
        }

        private async void BotClient_OnDisconnectOccurred(object sender, WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred();

            do
            {
                await this.DisconnectBot();

                await Task.Delay(2000);
            } while (!await this.ConnectBot());

            ChannelSession.ReconnectionOccurred();
        }

        #endregion Chat Event Handlers
    }
}
