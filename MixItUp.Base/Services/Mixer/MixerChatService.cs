using Mixer.Base.Clients;
using Mixer.Base.Model.Chat;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Mixer
{
    public interface IMixerChatService
    {

    }

    public class MixerChatService : MixerWebSocketServiceBase, IMixerChatService
    {
        public ChatClient StreamerClient { get; private set; }
        public ChatClient BotClient { get; private set; }

        public event EventHandler<ChatMessageViewModel> OnMessageOccurred = delegate { };
        public event EventHandler<Guid> OnDeleteMessageOccurred = delegate { };
        public event EventHandler OnClearMessagesOccurred = delegate { };

        public event EventHandler<IEnumerable<UserViewModel>> OnUsersJoinOccurred = delegate { };
        public event EventHandler<UserViewModel> OnUserUpdateOccurred = delegate { };
        public event EventHandler<IEnumerable<UserViewModel>> OnUsersLeaveOccurred = delegate { };
        public event EventHandler<Tuple<UserViewModel, UserViewModel>> OnUserPurgeOccurred = delegate { };
        public event EventHandler<UserViewModel> OnUserBanOccurred = delegate { };

        public event EventHandler<ChatPollEventModel> OnPollEndOccurred = delegate { };

        private const int userJoinLeaveEventsTotalToProcess = 25;
        private SemaphoreSlim userJoinLeaveEventsSemaphore = new SemaphoreSlim(1);
        private Dictionary<uint, ChatUserEventModel> userJoinEvents = new Dictionary<uint, ChatUserEventModel>();
        private Dictionary<uint, ChatUserEventModel> userLeaveEvents = new Dictionary<uint, ChatUserEventModel>();

        private CancellationTokenSource cancellationTokenSource;

        public MixerChatService() { }

        public async Task<bool> ConnectStreamer()
        {
            return await this.AttemptConnect(async () =>
            {
                if (ChannelSession.MixerStreamerConnection != null)
                {
                    this.cancellationTokenSource = new CancellationTokenSource();

                    this.StreamerClient = await this.ConnectAndAuthenticateChatClient(ChannelSession.MixerStreamerConnection);
                    if (this.StreamerClient != null)
                    {
                        this.StreamerClient.OnClearMessagesOccurred += ChatClient_OnClearMessagesOccurred;
                        this.StreamerClient.OnDeleteMessageOccurred += ChatClient_OnDeleteMessageOccurred;
                        this.StreamerClient.OnMessageOccurred += ChatClient_OnMessageOccurred;
                        this.StreamerClient.OnPollEndOccurred += ChatClient_OnPollEndOccurred;
                        this.StreamerClient.OnPollStartOccurred += ChatClient_OnPollStartOccurred;
                        this.StreamerClient.OnPurgeMessageOccurred += ChatClient_OnPurgeMessageOccurred;
                        this.StreamerClient.OnUserJoinOccurred += ChatClient_OnUserJoinOccurred;
                        this.StreamerClient.OnUserLeaveOccurred += ChatClient_OnUserLeaveOccurred;
                        this.StreamerClient.OnUserUpdateOccurred += ChatClient_OnUserUpdateOccurred;
                        this.StreamerClient.OnSkillAttributionOccurred += Client_OnSkillAttributionOccurred;
                        this.StreamerClient.OnDisconnectOccurred += StreamerClient_OnDisconnectOccurred;
                        if (ChannelSession.Settings.DiagnosticLogging)
                        {
                            this.StreamerClient.OnPacketSentOccurred += WebSocketClient_OnPacketSentOccurred;
                            this.StreamerClient.OnMethodOccurred += WebSocketClient_OnMethodOccurred;
                            this.StreamerClient.OnReplyOccurred += WebSocketClient_OnReplyOccurred;
                            this.StreamerClient.OnEventOccurred += WebSocketClient_OnEventOccurred;
                        }

                        AsyncRunner.RunBackgroundTask(this.cancellationTokenSource.Token, this.ChatterJoinLeaveBackground, 2500);
                        AsyncRunner.RunBackgroundTask(this.cancellationTokenSource.Token, this.ChatterRefreshBackground, 300000);

                        return true;
                    }
                }
                await this.DisconnectStreamer();
                return false;
            });
        }

        public async Task DisconnectStreamer()
        {
            await this.RunAsync(async () =>
            {
                if (this.StreamerClient != null)
                {
                    this.StreamerClient.OnClearMessagesOccurred -= ChatClient_OnClearMessagesOccurred;
                    this.StreamerClient.OnDeleteMessageOccurred -= ChatClient_OnDeleteMessageOccurred;
                    this.StreamerClient.OnMessageOccurred -= ChatClient_OnMessageOccurred;
                    this.StreamerClient.OnPollEndOccurred -= ChatClient_OnPollEndOccurred;
                    this.StreamerClient.OnPollStartOccurred -= ChatClient_OnPollStartOccurred;
                    this.StreamerClient.OnPurgeMessageOccurred -= ChatClient_OnPurgeMessageOccurred;
                    this.StreamerClient.OnUserJoinOccurred -= ChatClient_OnUserJoinOccurred;
                    this.StreamerClient.OnUserLeaveOccurred -= ChatClient_OnUserLeaveOccurred;
                    this.StreamerClient.OnUserUpdateOccurred -= ChatClient_OnUserUpdateOccurred;
                    this.StreamerClient.OnSkillAttributionOccurred -= Client_OnSkillAttributionOccurred;
                    this.StreamerClient.OnDisconnectOccurred -= StreamerClient_OnDisconnectOccurred;
                    if (ChannelSession.Settings.DiagnosticLogging)
                    {
                        this.StreamerClient.OnPacketSentOccurred -= WebSocketClient_OnPacketSentOccurred;
                        this.StreamerClient.OnMethodOccurred -= WebSocketClient_OnMethodOccurred;
                        this.StreamerClient.OnReplyOccurred -= WebSocketClient_OnReplyOccurred;
                        this.StreamerClient.OnEventOccurred -= WebSocketClient_OnEventOccurred;
                    }

                    await this.RunAsync(this.StreamerClient.Disconnect());
                }

                this.StreamerClient = null;
                if (this.cancellationTokenSource != null)
                {
                    this.cancellationTokenSource.Cancel();
                    this.cancellationTokenSource = null;
                }
            });
        }

        public async Task<bool> ConnectBot()
        {
            if (ChannelSession.MixerBotConnection != null)
            {
                return await this.AttemptConnect(async () =>
                {
                    if (ChannelSession.MixerBotConnection != null)
                    {
                        this.BotClient = await this.ConnectAndAuthenticateChatClient(ChannelSession.MixerBotConnection);
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
                        return false;
                    }
                    await this.DisconnectBot();
                    return false;
                });
            }
            return true;
        }

        public async Task DisconnectBot()
        {
            await this.RunAsync(async () =>
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
            });
        }

        public async Task SendMessage(string message, bool sendAsStreamer = false)
        {
            await this.RunAsync(async () =>
            {
                ChatClient client = this.GetChatClient(sendAsStreamer);
                if (client != null)
                {
                    message = this.SplitLargeMessage(message, out string subMessage);

                    await client.SendMessage(message);

                    // Adding delay to prevent messages from arriving in wrong order
                    await Task.Delay(250);

                    if (!string.IsNullOrEmpty(subMessage))
                    {
                        await this.SendMessage(subMessage, sendAsStreamer: sendAsStreamer);
                    }
                }
            });
        }

        public async Task Whisper(string username, string message, bool sendAsStreamer = false)
        {
            await this.RunAsync(async () =>
            {
                ChatClient client = this.GetChatClient(sendAsStreamer);
                if (client != null)
                {
                    if (!string.IsNullOrEmpty(username))
                    {
                        message = this.SplitLargeMessage(message, out string subMessage);

                        await client.Whisper(username, message);

                        // Adding delay to prevent messages from arriving in wrong order
                        await Task.Delay(250);

                        if (!string.IsNullOrEmpty(subMessage))
                        {
                            await this.Whisper(username, subMessage, sendAsStreamer: sendAsStreamer);
                        }
                    }
                }
            });
        }

        public async Task<ChatMessageEventModel> WhisperWithResponse(string username, string message, bool sendAsStreamer = false)
        {
            return await this.RunAsync(async () =>
            {
                ChatClient client = this.GetChatClient(sendAsStreamer);
                if (client != null)
                {
                    message = this.SplitLargeMessage(message, out string subMessage);

                    ChatMessageEventModel firstChatMessage = await client.WhisperWithResponse(username, message);

                    // Adding delay to prevent messages from arriving in wrong order
                    await Task.Delay(250);

                    if (!string.IsNullOrEmpty(subMessage))
                    {
                        await this.WhisperWithResponse(username, subMessage, sendAsStreamer: sendAsStreamer);
                    }

                    return firstChatMessage;
                }
                return null;
            });
        }

        public async Task DeleteMessage(ChatMessageViewModel message)
        {
            await this.RunAsync(async () =>
            {
                Logger.Log(LogLevel.Debug, string.Format("Deleting Message - {0}", message.PlainTextMessage));

                await this.StreamerClient.DeleteMessage(Guid.Parse(message.ID));
            });
        }

        public async Task ClearMessages()
        {
            await this.RunAsync(async () =>
            {
                await this.StreamerClient.ClearMessages();
            });
        }

        public async Task PurgeUser(string username)
        {
            await this.RunAsync(async () =>
            {
                await this.StreamerClient.PurgeUser(username);
            });
        }

        public async Task TimeoutUser(string username, uint durationInSeconds)
        {
            await this.RunAsync(async () =>
            {
                await this.StreamerClient.TimeoutUser(username, durationInSeconds);
            });
        }

        public async Task BanUser(UserViewModel user)
        {
            await this.RunAsync(async () =>
            {
                await ChannelSession.MixerStreamerConnection.AddUserRoles(ChannelSession.MixerChannel, user.GetModel(), new List<MixerRoleEnum>() { MixerRoleEnum.Banned });
                await user.RefreshDetails(true);
            });
        }

        public async Task UnBanUser(UserViewModel user)
        {
            await this.RunAsync(async () =>
            {
                await ChannelSession.MixerStreamerConnection.RemoveUserRoles(ChannelSession.MixerChannel, user.GetModel(), new List<MixerRoleEnum>() { MixerRoleEnum.Banned });
                await user.RefreshDetails(true);
            });
        }

        public async Task ModUser(UserViewModel user)
        {
            await this.RunAsync(async () =>
            {
                await ChannelSession.MixerStreamerConnection.AddUserRoles(ChannelSession.MixerChannel, user.GetModel(), new List<MixerRoleEnum>() { MixerRoleEnum.Mod });
                await user.RefreshDetails(true);
            });
        }

        public async Task UnModUser(UserViewModel user)
        {
            await this.RunAsync(async () =>
            {
                await ChannelSession.MixerStreamerConnection.RemoveUserRoles(ChannelSession.MixerChannel, user.GetModel(), new List<MixerRoleEnum>() { MixerRoleEnum.Mod });
                await user.RefreshDetails(true);
            });
        }

        public async Task StartPoll(string question, IEnumerable<string> answers, uint lengthInSeconds)
        {
            await this.RunAsync(async () =>
            {
                await this.StreamerClient.StartVote(question, answers, lengthInSeconds);
            });
        }

        public async Task<IEnumerable<ChatMessageEventModel>> GetChatHistory(uint maxMessages)
        {
            return await this.RunAsync(async () =>
            {
                if (this.StreamerClient != null)
                {
                    return await this.StreamerClient.GetChatHistory(maxMessages);
                }
                return new List<ChatMessageEventModel>();
            });
        }

        private ChatClient GetChatClient(bool sendAsStreamer = false) { return (this.BotClient != null && !sendAsStreamer) ? this.BotClient : this.StreamerClient; }

        private async Task<ChatClient> ConnectAndAuthenticateChatClient(MixerConnectionService connection)
        {
            ChatClient client = await this.RunAsync(ChatClient.CreateFromChannel(connection.Connection, ChannelSession.MixerChannel));
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

        #region Refresh Methods

        private async Task ChatterJoinLeaveBackground(CancellationToken cancellationToken)
        {
            List<ChatUserEventModel> joinsToProcess = new List<ChatUserEventModel>();
            await this.userJoinLeaveEventsSemaphore.WaitAndRelease(() =>
            {
                for (int i = 0; i < userJoinLeaveEventsTotalToProcess && i < this.userJoinEvents.Count(); i++)
                {
                    ChatUserEventModel chatUser = this.userJoinEvents.Values.First();
                    joinsToProcess.Add(chatUser);
                    this.userJoinEvents.Remove(chatUser.id);
                }
                return Task.FromResult(0);
            });

            if (joinsToProcess.Count > 0)
            {
                IEnumerable<UserViewModel> processedUsers = await ChannelSession.ActiveUsers.AddOrUpdateUsers(joinsToProcess.Select(u => u.GetUser()));
                this.OnUsersJoinOccurred(this, processedUsers);
            }

            List<ChatUserEventModel> leavesToProcess = new List<ChatUserEventModel>();
            await this.userJoinLeaveEventsSemaphore.WaitAndRelease(() =>
            {
                for (int i = 0; i < userJoinLeaveEventsTotalToProcess && i < this.userLeaveEvents.Count(); i++)
                {
                    ChatUserEventModel chatUser = this.userLeaveEvents.Values.First();
                    leavesToProcess.Add(chatUser);
                    this.userLeaveEvents.Remove(chatUser.id);
                }
                return Task.FromResult(0);
            });

            if (leavesToProcess.Count > 0)
            {
                IEnumerable<UserViewModel> processedUsers = await ChannelSession.ActiveUsers.RemoveUsers(leavesToProcess.Select(u => u.id));
                this.OnUsersLeaveOccurred(this, processedUsers);
            }
        }

        private async Task ChatterRefreshBackground(CancellationToken cancellationToken)
        {
            //IEnumerable<ChatUserModel> chatUsers = await ChannelSession.MixerStreamerConnection.GetChatUsers(ChannelSession.MixerChannel, int.MaxValue);
            //chatUsers = chatUsers.Where(u => u.userId.HasValue);
            //HashSet<uint> chatUserIDs = new HashSet<uint>(chatUsers.Select(u => u.userId.GetValueOrDefault()));

            //IEnumerable<UserViewModel> existingUsers = await ChannelSession.ActiveUsers.GetAllUsers();
            //HashSet<uint> existingUsersIDs = new HashSet<uint>(existingUsers.Select(u => u.ID));

            //Dictionary<uint, ChatUserModel> usersToAdd = chatUsers.ToDictionary(u => u.userId.GetValueOrDefault(), u => u);
            //List<uint> usersToRemove = new List<uint>();

            //foreach (uint userID in existingUsersIDs)
            //{
            //    usersToAdd.Remove(userID);
            //    if (!chatUserIDs.Contains(userID))
            //    {
            //        usersToRemove.Add(userID);
            //    }
            //}

            //foreach (ChatUserModel user in usersToAdd.Values)
            //{
            //    this.ChatClient_OnUserJoinOccurred(this, new ChatUserEventModel()
            //    {
            //        id = user.userId.GetValueOrDefault(),
            //        username = user.userName,
            //        roles = user.userRoles,
            //    });
            //}

            //foreach (uint userID in usersToRemove)
            //{
            //    this.ChatClient_OnUserLeaveOccurred(this, new ChatUserEventModel() { id = userID });
            //}
        }

        #endregion Refresh Methods

        #region Chat Event Handlers

        private void ChatClient_OnMessageOccurred(object sender, ChatMessageEventModel e)
        {
            this.OnMessageOccurred(sender, new ChatMessageViewModel(e));
        }

        private void BotChatClient_OnMessageOccurred(object sender, ChatMessageEventModel e)
        {
            if (!string.IsNullOrEmpty(e.target))
            {
                this.OnMessageOccurred(sender, new ChatMessageViewModel(e));
            }
        }

        private void ChatClient_OnDeleteMessageOccurred(object sender, ChatDeleteMessageEventModel e)
        {
            this.OnDeleteMessageOccurred(sender, e.id);
        }

        private async void ChatClient_OnPurgeMessageOccurred(object sender, ChatPurgeMessageEventModel e)
        {
            UserViewModel user = await ChannelSession.ActiveUsers.GetUserByID(e.user_id);
            if (user != null)
            {
                UserViewModel modUser = null;
                if (e.moderator != null)
                {
                    modUser = new UserViewModel(e.moderator);
                }
                this.OnUserPurgeOccurred(sender, new Tuple<UserViewModel, UserViewModel>(user, modUser));
            }
        }

        private void ChatClient_OnClearMessagesOccurred(object sender, ChatClearMessagesEventModel e)
        {
            this.OnClearMessagesOccurred(sender, new EventArgs());
        }

        private void ChatClient_OnPollStartOccurred(object sender, ChatPollEventModel e) { }

        private void ChatClient_OnPollEndOccurred(object sender, ChatPollEventModel e) { this.OnPollEndOccurred(sender, e); }

        private async void ChatClient_OnUserJoinOccurred(object sender, ChatUserEventModel chatUser)
        {
            await this.userJoinLeaveEventsSemaphore.WaitAndRelease(() =>
            {
                this.userJoinEvents[chatUser.id] = chatUser;
                return Task.FromResult(0);
            });
        }

        private async void ChatClient_OnUserLeaveOccurred(object sender, ChatUserEventModel chatUser)
        {
            await this.userJoinLeaveEventsSemaphore.WaitAndRelease(() =>
            {
                this.userLeaveEvents[chatUser.id] = chatUser;
                return Task.FromResult(0);
            });
        }

        private async void ChatClient_OnUserUpdateOccurred(object sender, ChatUserEventModel chatUser)
        {
            UserViewModel user = await ChannelSession.ActiveUsers.AddOrUpdateUser(chatUser.GetUser());
            if (user != null)
            {
                this.OnUserUpdateOccurred(sender, user);
                if (chatUser.roles != null && chatUser.roles.Count() > 0 && chatUser.roles.Where(r => !string.IsNullOrEmpty(r)).Contains(EnumHelper.GetEnumName(MixerRoleEnum.Banned)))
                {
                    this.OnUserBanOccurred(sender, user);
                }
            }
        }

        private async void Client_OnSkillAttributionOccurred(object sender, ChatSkillAttributionEventModel skillAttribution)
        {

        }

        private async void StreamerClient_OnDisconnectOccurred(object sender, WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred("Streamer Chat");

            await this.DisconnectStreamer();
            do
            {
                await Task.Delay(2500);
            }
            while (!await this.ConnectStreamer());

            ChannelSession.ReconnectionOccurred("Streamer Chat");
        }

        private async void BotClient_OnDisconnectOccurred(object sender, WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred("Bot Chat");

            await this.DisconnectBot();
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
