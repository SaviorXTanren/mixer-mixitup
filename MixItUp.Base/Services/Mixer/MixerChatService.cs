using Mixer.Base.Clients;
using Mixer.Base.Model.Chat;
using Mixer.Base.Model.Client;
using Mixer.Base.Model.User;
using MixItUp.Base.Commands;
using MixItUp.Base.Model;
using MixItUp.Base.Model.Chat;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Mixer;
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
        event EventHandler<MixerChatMessageViewModel> OnMessageOccurred;
        event EventHandler<Tuple<Guid, UserViewModel>> OnDeleteMessageOccurred;
        event EventHandler OnClearMessagesOccurred;

        event EventHandler<IEnumerable<UserViewModel>> OnUsersJoinOccurred;
        event EventHandler<UserViewModel> OnUserUpdateOccurred;
        event EventHandler<IEnumerable<UserViewModel>> OnUsersLeaveOccurred;
        event EventHandler<Tuple<UserViewModel, UserViewModel>> OnUserPurgeOccurred;
        event EventHandler<UserViewModel> OnUserBanOccurred;

        event EventHandler<ChatPollEventModel> OnPollEndOccurred;

        bool IsBotConnected { get; }

        Task<ExternalServiceResult> ConnectUser();
        Task DisconnectStreamer();

        Task<ExternalServiceResult> ConnectBot();
        Task DisconnectBot();

        Task SendMessage(string message, bool sendAsStreamer = false);
        Task Whisper(string username, string message, bool sendAsStreamer = false);
        Task<ChatMessageEventModel> WhisperWithResponse(string username, string message, bool sendAsStreamer = false);

        Task<IEnumerable<ChatMessageEventModel>> GetChatHistory(uint maxMessages);
        Task DeleteMessage(ChatMessageViewModel message);
        Task ClearMessages();

        Task PurgeUser(string username);
        Task TimeoutUser(string username, uint durationInSeconds);

        Task BanUser(UserViewModel user);
        Task UnbanUser(UserViewModel user);
        Task ModUser(UserViewModel user);
        Task UnmodUser(UserViewModel user);

        Task StartPoll(string question, IEnumerable<string> answers, uint lengthInSeconds);
    }

    public class MixerChatService : MixerPlatformServiceBase, IMixerChatService
    {
        public event EventHandler<MixerChatMessageViewModel> OnMessageOccurred = delegate { };
        public event EventHandler<Tuple<Guid, UserViewModel>> OnDeleteMessageOccurred = delegate { };
        public event EventHandler OnClearMessagesOccurred = delegate { };

        public event EventHandler<IEnumerable<UserViewModel>> OnUsersJoinOccurred = delegate { };
        public event EventHandler<UserViewModel> OnUserUpdateOccurred = delegate { };
        public event EventHandler<IEnumerable<UserViewModel>> OnUsersLeaveOccurred = delegate { };
        public event EventHandler<Tuple<UserViewModel, UserViewModel>> OnUserPurgeOccurred = delegate { };
        public event EventHandler<UserViewModel> OnUserBanOccurred = delegate { };

        public event EventHandler<ChatPollEventModel> OnPollEndOccurred = delegate { };

        private ChatClient streamerClient;
        private ChatClient botClient;

        private const int userJoinLeaveEventsTotalToProcess = 25;
        private SemaphoreSlim userJoinLeaveEventsSemaphore = new SemaphoreSlim(1);
        private Dictionary<uint, ChatUserEventModel> userJoinEvents = new Dictionary<uint, ChatUserEventModel>();
        private Dictionary<uint, ChatUserEventModel> userLeaveEvents = new Dictionary<uint, ChatUserEventModel>();

        private CancellationTokenSource cancellationTokenSource;

        public MixerChatService() { }

        #region Interface Methods

        public bool IsBotConnected { get { return this.botClient != null && this.botClient.Connected; } }

        public async Task<ExternalServiceResult> ConnectUser()
        {
            if (ChannelSession.MixerUserConnection != null)
            {
                return await this.AttemptConnect(async () =>
                {
                    this.cancellationTokenSource = new CancellationTokenSource();

                    ExternalServiceResult<ChatClient> result = await this.ConnectAndAuthenticateChatClient(ChannelSession.MixerUserConnection);
                    if (result.Success)
                    {
                        this.streamerClient = result.Result;

                        this.streamerClient.OnClearMessagesOccurred += ChatClient_OnClearMessagesOccurred;
                        this.streamerClient.OnDeleteMessageOccurred += ChatClient_OnDeleteMessageOccurred;
                        this.streamerClient.OnMessageOccurred += ChatClient_OnMessageOccurred;
                        this.streamerClient.OnPollEndOccurred += ChatClient_OnPollEndOccurred;
                        this.streamerClient.OnPollStartOccurred += ChatClient_OnPollStartOccurred;
                        this.streamerClient.OnPurgeMessageOccurred += ChatClient_OnPurgeMessageOccurred;
                        this.streamerClient.OnUserJoinOccurred += ChatClient_OnUserJoinOccurred;
                        this.streamerClient.OnUserLeaveOccurred += ChatClient_OnUserLeaveOccurred;
                        this.streamerClient.OnUserUpdateOccurred += ChatClient_OnUserUpdateOccurred;
                        this.streamerClient.OnSkillAttributionOccurred += Client_OnSkillAttributionOccurred;
                        this.streamerClient.OnDisconnectOccurred += StreamerClient_OnDisconnectOccurred;
                        if (ChannelSession.Settings.DiagnosticLogging)
                        {
                            this.streamerClient.OnPacketSentOccurred += WebSocketClient_OnPacketSentOccurred;
                            this.streamerClient.OnMethodOccurred += WebSocketClient_OnMethodOccurred;
                            this.streamerClient.OnReplyOccurred += WebSocketClient_OnReplyOccurred;
                            this.streamerClient.OnEventOccurred += WebSocketClient_OnEventOccurred;
                        }

                        AsyncRunner.RunAsyncInBackground(async () =>
                        {
                            await ChannelSession.MixerUserConnection.GetChatUsers(ChannelSession.MixerChannel, (users) =>
                            {
                                foreach (ChatUserModel user in users)
                                {
                                    this.ChatClient_OnUserJoinOccurred(this, new ChatUserEventModel()
                                    {
                                        id = user.userId.GetValueOrDefault(),
                                        username = user.userName,
                                        roles = user.userRoles,
                                    });
                                }
                                return Task.FromResult(0);
                            }, uint.MaxValue);

                            AsyncRunner.RunBackgroundTask(this.cancellationTokenSource.Token, 300000, this.ChatterRefreshBackground);
                        });
                        AsyncRunner.RunBackgroundTask(this.cancellationTokenSource.Token, 2500, this.ChatterJoinLeaveBackground);
                    }
                    else
                    {
                        await this.DisconnectStreamer();
                    }
                    return result;
                });
            }
            return new ExternalServiceResult("Mixer connection has not been established");
        }

        public async Task DisconnectStreamer()
        {
            await this.RunAsync(async () =>
            {
                if (this.streamerClient != null)
                {
                    this.streamerClient.OnClearMessagesOccurred -= ChatClient_OnClearMessagesOccurred;
                    this.streamerClient.OnDeleteMessageOccurred -= ChatClient_OnDeleteMessageOccurred;
                    this.streamerClient.OnMessageOccurred -= ChatClient_OnMessageOccurred;
                    this.streamerClient.OnPollEndOccurred -= ChatClient_OnPollEndOccurred;
                    this.streamerClient.OnPollStartOccurred -= ChatClient_OnPollStartOccurred;
                    this.streamerClient.OnPurgeMessageOccurred -= ChatClient_OnPurgeMessageOccurred;
                    this.streamerClient.OnUserJoinOccurred -= ChatClient_OnUserJoinOccurred;
                    this.streamerClient.OnUserLeaveOccurred -= ChatClient_OnUserLeaveOccurred;
                    this.streamerClient.OnUserUpdateOccurred -= ChatClient_OnUserUpdateOccurred;
                    this.streamerClient.OnSkillAttributionOccurred -= Client_OnSkillAttributionOccurred;
                    this.streamerClient.OnDisconnectOccurred -= StreamerClient_OnDisconnectOccurred;
                    if (ChannelSession.Settings.DiagnosticLogging)
                    {
                        this.streamerClient.OnPacketSentOccurred -= WebSocketClient_OnPacketSentOccurred;
                        this.streamerClient.OnMethodOccurred -= WebSocketClient_OnMethodOccurred;
                        this.streamerClient.OnReplyOccurred -= WebSocketClient_OnReplyOccurred;
                        this.streamerClient.OnEventOccurred -= WebSocketClient_OnEventOccurred;
                    }

                    await this.RunAsync(this.streamerClient.Disconnect());
                }

                this.streamerClient = null;
                if (this.cancellationTokenSource != null)
                {
                    this.cancellationTokenSource.Cancel();
                    this.cancellationTokenSource = null;
                }
            });
        }

        public async Task<ExternalServiceResult> ConnectBot()
        {
            if (ChannelSession.MixerBotConnection != null)
            {
                return await this.AttemptConnect(async () =>
                {
                    ExternalServiceResult<ChatClient> result = await this.ConnectAndAuthenticateChatClient(ChannelSession.MixerBotConnection);
                    if (result.Success)
                    {
                        this.botClient = result.Result;

                        this.botClient.OnMessageOccurred += BotChatClient_OnMessageOccurred;
                        this.botClient.OnDisconnectOccurred += BotClient_OnDisconnectOccurred;
                        this.botClient.OnReplyOccurred += BotClient_OnReplyOccurred;
                        if (ChannelSession.Settings.DiagnosticLogging)
                        {
                            this.botClient.OnPacketSentOccurred += WebSocketClient_OnPacketSentOccurred;
                            this.botClient.OnMethodOccurred += WebSocketClient_OnMethodOccurred;
                            this.botClient.OnReplyOccurred += WebSocketClient_OnReplyOccurred;
                            this.botClient.OnEventOccurred += WebSocketClient_OnEventOccurred;
                        }
                    }
                    else
                    {
                        await this.DisconnectBot();
                    }
                    return result;
                });
            }
            return new ExternalServiceResult("Mixer connection has not been established");
        }

        public async Task DisconnectBot()
        {
            await this.RunAsync(async () =>
            {
                if (this.botClient != null)
                {
                    this.botClient.OnMessageOccurred -= BotChatClient_OnMessageOccurred;
                    this.botClient.OnDisconnectOccurred -= BotClient_OnDisconnectOccurred;
                    this.botClient.OnReplyOccurred -= BotClient_OnReplyOccurred;
                    if (ChannelSession.Settings.DiagnosticLogging)
                    {
                        this.botClient.OnPacketSentOccurred -= WebSocketClient_OnPacketSentOccurred;
                        this.botClient.OnMethodOccurred -= WebSocketClient_OnMethodOccurred;
                        this.botClient.OnReplyOccurred -= WebSocketClient_OnReplyOccurred;
                        this.botClient.OnEventOccurred -= WebSocketClient_OnEventOccurred;
                    }

                    await this.RunAsync(this.botClient.Disconnect());
                }
                this.botClient = null;
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

        public async Task<IEnumerable<ChatMessageEventModel>> GetChatHistory(uint maxMessages)
        {
            return await this.RunAsync(async () =>
            {
                if (this.streamerClient != null)
                {
                    return await this.streamerClient.GetChatHistory(maxMessages);
                }
                return new List<ChatMessageEventModel>();
            });
        }

        public async Task DeleteMessage(ChatMessageViewModel message)
        {
            await this.RunAsync(async () =>
            {
                Logger.Log(LogLevel.Debug, string.Format("Deleting Message - {0}", message.PlainTextMessage));

                await this.streamerClient.DeleteMessage(Guid.Parse(message.ID));
            });
        }

        public async Task ClearMessages()
        {
            await this.RunAsync(async () =>
            {
                await this.streamerClient.ClearMessages();
            });
        }

        public async Task PurgeUser(string username)
        {
            await this.RunAsync(async () =>
            {
                await this.streamerClient.PurgeUser(username);
            });
        }

        public async Task TimeoutUser(string username, uint durationInSeconds)
        {
            await this.RunAsync(async () =>
            {
                await this.streamerClient.TimeoutUser(username, durationInSeconds);
            });
        }

        public async Task BanUser(UserViewModel user)
        {
            await this.RunAsync(async () =>
            {
                await ChannelSession.MixerUserConnection.AddUserRoles(ChannelSession.MixerChannel, user.GetModel(), new List<UserRoleEnum>() { UserRoleEnum.Banned });
                await user.RefreshDetails(force: true);
            });
        }

        public async Task UnbanUser(UserViewModel user)
        {
            await this.RunAsync(async () =>
            {
                await ChannelSession.MixerUserConnection.RemoveUserRoles(ChannelSession.MixerChannel, user.GetModel(), new List<UserRoleEnum>() { UserRoleEnum.Banned });
                await user.RefreshDetails(force: true);
            });
        }

        public async Task ModUser(UserViewModel user)
        {
            await this.RunAsync(async () =>
            {
                await ChannelSession.MixerUserConnection.AddUserRoles(ChannelSession.MixerChannel, user.GetModel(), new List<UserRoleEnum>() { UserRoleEnum.Mod });
                await user.RefreshDetails(force: true);
            });
        }

        public async Task UnmodUser(UserViewModel user)
        {
            await this.RunAsync(async () =>
            {
                await ChannelSession.MixerUserConnection.RemoveUserRoles(ChannelSession.MixerChannel, user.GetModel(), new List<UserRoleEnum>() { UserRoleEnum.Mod });
                await user.RefreshDetails(force: true);
            });
        }

        public async Task StartPoll(string question, IEnumerable<string> answers, uint lengthInSeconds)
        {
            await this.RunAsync(async () =>
            {
                await this.streamerClient.StartVote(question, answers, lengthInSeconds);
            });
        }

        private ChatClient GetChatClient(bool sendAsStreamer = false) { return (this.botClient != null && !sendAsStreamer) ? this.botClient : this.streamerClient; }

        private async Task<ExternalServiceResult<ChatClient>> ConnectAndAuthenticateChatClient(MixerConnectionService connection)
        {
            ChatClient client = await this.RunAsync(ChatClient.CreateFromChannel(connection.Connection, ChannelSession.MixerChannel));
            if (client != null)
            {
                if (await this.RunAsync(client.Connect()))
                {
                    if (await this.RunAsync(client.Authenticate()))
                    {
                        return new ExternalServiceResult<ChatClient>(client);
                    }
                    return new ExternalServiceResult<ChatClient>("Failed to authenticate to Mixer chat");
                }
                return new ExternalServiceResult<ChatClient>("Failed to connect Mixer chat");
            }
            return new ExternalServiceResult<ChatClient>("Failed to create chat client from connection");
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

        #endregion Interface Methods

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
                List<UserViewModel> processedUsers = new List<UserViewModel>();
                foreach (ChatUserEventModel chatUser in joinsToProcess)
                {
                    UserViewModel user = await ChannelSession.Services.User.AddOrUpdateUser(chatUser);
                    if (user != null)
                    {
                        processedUsers.Add(user);
                    }
                }
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
                List<UserViewModel> processedUsers = new List<UserViewModel>();
                foreach (ChatUserEventModel chatUser in leavesToProcess)
                {
                    UserViewModel user = await ChannelSession.Services.User.RemoveUser(chatUser);
                    if (user != null)
                    {
                        processedUsers.Add(user);
                    }
                }
                this.OnUsersLeaveOccurred(this, processedUsers);
            }
        }

        private async Task ChatterRefreshBackground(CancellationToken cancellationToken)
        {
            List<ChatUserModel> chatUsers = new List<ChatUserModel>();
            await ChannelSession.MixerUserConnection.GetChatUsers(ChannelSession.MixerChannel, (users) =>
            {
                chatUsers.AddRange(users);
                return Task.FromResult(0);
            }, uint.MaxValue);

            chatUsers = chatUsers.Where(u => u.userId.HasValue).ToList();
            List<uint> chatUserIDs = new List<uint>(chatUsers.Select(u => u.userId.GetValueOrDefault()));

            IEnumerable<UserViewModel> existingUsers = ChannelSession.Services.User.GetAllUsers();
            List<uint> existingUsersIDs = new List<uint>(existingUsers.Select(u => u.MixerID));

            Dictionary<uint, ChatUserModel> usersToAdd = new Dictionary<uint, ChatUserModel>();
            foreach (ChatUserModel user in chatUsers)
            {
                usersToAdd[user.userId.GetValueOrDefault()] = user;
            }

            List<uint> usersToRemove = new List<uint>();
            foreach (uint userID in existingUsersIDs)
            {
                usersToAdd.Remove(userID);
                if (!chatUserIDs.Contains(userID))
                {
                    usersToRemove.Add(userID);
                }
            }

            foreach (ChatUserModel user in usersToAdd.Values)
            {
                this.ChatClient_OnUserJoinOccurred(this, new ChatUserEventModel()
                {
                    id = user.userId.GetValueOrDefault(),
                    username = user.userName,
                    roles = user.userRoles,
                });
            }

            foreach (uint userID in usersToRemove)
            {
                this.ChatClient_OnUserLeaveOccurred(this, new ChatUserEventModel() { id = userID });
            }
        }

        #endregion Refresh Methods

        #region Chat Event Handlers

        private void ChatClient_OnMessageOccurred(object sender, ChatMessageEventModel e)
        {
            if (e.message != null)
            {
                if (e.message.ContainsSkill)
                {
                    MixerSkillChatMessageViewModel message = new MixerSkillChatMessageViewModel(e);
                    this.OnMessageOccurred(sender, message);
                    this.ProcessSkill(message);
                }
                else
                {
                    this.OnMessageOccurred(sender, new MixerChatMessageViewModel(e));
                }
            }
        }

        private void BotChatClient_OnMessageOccurred(object sender, ChatMessageEventModel e)
        {
            if (!string.IsNullOrEmpty(e.target))
            {
                this.OnMessageOccurred(sender, new MixerChatMessageViewModel(e));
            }
        }

        private async void ChatClient_OnDeleteMessageOccurred(object sender, ChatDeleteMessageEventModel e)
        {
            if (e != null && e.moderator != null)
            {
                UserViewModel moderator = ChannelSession.Services.User.GetUserByMixerID(e.moderator.user_id);
                if (moderator == null)
                {
                    moderator = await ChannelSession.Services.User.AddOrUpdateUser(e.moderator.ToChatUserModel());
                }

                this.OnDeleteMessageOccurred(sender, new Tuple<Guid, UserViewModel>(e.id, moderator));
            }
        }

        private async void ChatClient_OnPurgeMessageOccurred(object sender, ChatPurgeMessageEventModel e)
        {
            UserViewModel user = ChannelSession.Services.User.GetUserByMixerID(e.user_id);
            if (user != null)
            {
                UserViewModel moderator = null;
                if (e.moderator != null)
                {
                    moderator = ChannelSession.Services.User.GetUserByMixerID(e.moderator.user_id);
                    if (moderator == null)
                    {
                        moderator = await ChannelSession.Services.User.AddOrUpdateUser(e.moderator.ToChatUserModel());
                    }
                }
                this.OnUserPurgeOccurred(sender, new Tuple<UserViewModel, UserViewModel>(user, moderator));
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
            UserViewModel user = await ChannelSession.Services.User.AddOrUpdateUser(chatUser.GetUser());
            if (user != null)
            {
                try
                {
                    if (user.Data.ViewingMinutes == 0)
                    {
                        await ChannelSession.Services.Events.PerformEvent(new EventTrigger(EventTypeEnum.MixerChatUserFirstJoin, user));
                    }
                }
                catch (Exception ex) { Logger.Log(ex); }

                this.OnUserUpdateOccurred(sender, user);
                if (chatUser.roles != null && chatUser.roles.Count() > 0 && chatUser.roles.Where(r => !string.IsNullOrEmpty(r)).Contains(EnumHelper.GetEnumName(UserRoleEnum.Banned)))
                {
                    this.OnUserBanOccurred(sender, user);
                }
            }
        }

        private async void Client_OnSkillAttributionOccurred(object sender, ChatSkillAttributionEventModel skillAttribution)
        {
            MixerSkillChatMessageViewModel message = new MixerSkillChatMessageViewModel(skillAttribution);

            // Add artificial delay to ensure skill event data from Constellation was received.
            for (int i = 0; i < 8; i++)
            {
                await Task.Delay(250);
                if (ChannelSession.Services.Events.MixerEventService.SkillEventsTriggered.ContainsKey(skillAttribution.id))
                {
                    message.Skill.SetPayload(ChannelSession.Services.Events.MixerEventService.SkillEventsTriggered[skillAttribution.id]);
                    ChannelSession.Services.Events.MixerEventService.SkillEventsTriggered.Remove(skillAttribution.id);
                    break;
                }
            }

            this.OnMessageOccurred(sender, message);

            this.ProcessSkill(message);
        }

        protected async void BotClient_OnReplyOccurred(object sender, ReplyPacket e)
        {
            if (e.errorObject != null)
            {
                if (e.errorObject.ContainsKey("code") && e.errorObject.ContainsKey("message") && e.errorObject["code"].ToString().Equals("4007"))
                {
                    await ChannelSession.Services.Chat.AddMessage(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Mixer, await ChannelSession.GetCurrentUser(),
                        "The Bot account could not send the last message for the following reason: " + e.errorObject["message"]));
                }
            }
            else if (e.error != null)
            {
                await ChannelSession.Services.Chat.AddMessage(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Mixer, await ChannelSession.GetCurrentUser(),
                    "Bot account error: " + e.error.ToString()));
            }
        }

        private void ProcessSkill(MixerSkillChatMessageViewModel message)
        {
            if (message.IsInUsersChannel)
            {
                GlobalEvents.SkillUseOccurred(message);
                if (message.Skill.Cost > 0)
                {
                    if (message.Skill.CostType == MixerSkillCostTypeEnum.Sparks)
                    {
                        GlobalEvents.SparkUseOccurred(new Tuple<UserViewModel, uint>(message.User, message.Skill.Cost));
                    }
                    else if (message.Skill.CostType == MixerSkillCostTypeEnum.Embers)
                    {
                        GlobalEvents.EmberUseOccurred(new UserEmberUsageModel(message));
                    }
                }
            }
        }

        private async void StreamerClient_OnDisconnectOccurred(object sender, WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred("Mixer Streamer Chat");

            ExternalServiceResult result;
            await this.DisconnectStreamer();
            do
            {
                await Task.Delay(2500);

                result = await this.ConnectUser();
            }
            while (!result.Success);

            ChannelSession.ReconnectionOccurred("Mixer Streamer Chat");
        }

        private async void BotClient_OnDisconnectOccurred(object sender, WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred("Mixer Bot Chat");

            ExternalServiceResult result;
            await this.DisconnectBot();
            do
            {
                await Task.Delay(2500);

                result = await this.ConnectBot();
            }
            while (!result.Success);

            ChannelSession.ReconnectionOccurred("Mixer Bot Chat");
        }

        #endregion Chat Event Handlers
    }
}
