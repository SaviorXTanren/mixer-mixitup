using Mixer.Base.Clients;
using Mixer.Base.Model.Chat;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Skill;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
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

    public class MixerChatService : MixerWebSocketWrapper, IMixerChatService
    {
        public ChatClient Client { get; private set; }
        public ChatClient BotClient { get; private set; }

        public event EventHandler<ChatMessageViewModel> OnMessageOccurred = delegate { };
        public event EventHandler<Guid> OnDeleteMessageOccurred = delegate { };
        public event EventHandler OnClearMessagesOccurred = delegate { };

        public event EventHandler<IEnumerable<UserViewModel>> OnUsersJoinOccurred = delegate { };
        public event EventHandler<UserViewModel> OnUserUpdateOccurred = delegate { };
        public event EventHandler<IEnumerable<UserViewModel>> OnUsersLeaveOccurred = delegate { };
        public event EventHandler<Tuple<UserViewModel, UserViewModel>> OnUserPurgeOccurred = delegate { };

        public event EventHandler<ChatPollEventModel> OnPollEndOccurred { add { this.Client.OnPollEndOccurred += value; } remove { this.Client.OnPollEndOccurred -= value; } }

        private const int userJoinLeaveEventsTotalToProcess = 25;
        private SemaphoreSlim userJoinLeaveEventsSemaphore = new SemaphoreSlim(1);
        private Dictionary<uint, ChatUserEventModel> userJoinEvents = new Dictionary<uint, ChatUserEventModel>();
        private Dictionary<uint, ChatUserEventModel> userLeaveEvents = new Dictionary<uint, ChatUserEventModel>();

        public MixerChatService() { }

        public async Task<bool> Connect()
        {
            return await this.AttemptConnect();
        }

        public async Task<bool> ConnectBot()
        {
            if (ChannelSession.MixerBotConnection != null)
            {
                return await this.RunAsync(async () =>
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
                });
            }
            return false;
        }

        public async Task Disconnect()
        {
            await this.RunAsync(async () =>
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
                    this.Client.OnSkillAttributionOccurred -= Client_OnSkillAttributionOccurred;
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
            });
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
            if (!string.IsNullOrEmpty(username) && this.GetBotClient(sendAsStreamer) != null)
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

        public async Task<ChatMessageEventModel> WhisperWithResponse(string username, string message, bool sendAsStreamer = false)
        {
            if (this.GetBotClient(sendAsStreamer) != null)
            {
                message = this.SplitLargeMessage(message, out string subMessage);

                ChatMessageEventModel firstChatMessage = await this.RunAsync(this.GetBotClient(sendAsStreamer).WhisperWithResponse(username, message));

                // Adding delay to prevent messages from arriving in wrong order
                await Task.Delay(250);

                if (!string.IsNullOrEmpty(subMessage))
                {
                    await this.WhisperWithResponse(username, subMessage, sendAsStreamer: sendAsStreamer);
                }

                return firstChatMessage;
            }

            return null;
        }

        public async Task DeleteMessage(ChatMessageViewModel message)
        {
            if (this.Client != null)
            {
                Util.Logger.LogDiagnostic(string.Format("Deleting Message - {0}", message.PlainTextMessage));

                await this.RunAsync(this.Client.DeleteMessage(Guid.Parse(message.ID)));
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
                await ChannelSession.MixerStreamerConnection.AddUserRoles(ChannelSession.MixerChannel, user.GetModel(), new List<MixerRoleEnum>() { MixerRoleEnum.Banned });
                await user.RefreshDetails(true);
            }
        }

        public async Task UnBanUser(UserViewModel user)
        {
            if (this.Client != null)
            {
                await ChannelSession.MixerStreamerConnection.RemoveUserRoles(ChannelSession.MixerChannel, user.GetModel(), new List<MixerRoleEnum>() { MixerRoleEnum.Banned });
                await user.RefreshDetails(true);
            }
        }

        public async Task ModUser(UserViewModel user)
        {
            if (this.Client != null)
            {
                await ChannelSession.MixerStreamerConnection.AddUserRoles(ChannelSession.MixerChannel, user.GetModel(), new List<MixerRoleEnum>() { MixerRoleEnum.Mod });
                await user.RefreshDetails(true);
            }
        }

        public async Task UnModUser(UserViewModel user)
        {
            if (this.Client != null)
            {
                await ChannelSession.MixerStreamerConnection.RemoveUserRoles(ChannelSession.MixerChannel, user.GetModel(), new List<MixerRoleEnum>() { MixerRoleEnum.Mod });
                await user.RefreshDetails(true);
            }
        }

        public async Task StartPoll(string question, IEnumerable<string> answers, uint lengthInSeconds)
        {
            if (this.Client != null)
            {
                await this.Client.StartVote(question, answers, lengthInSeconds);
            }
        }

        public async Task<IEnumerable<ChatMessageEventModel>> GetChatHistory(uint maxMessages)
        {
            if (this.Client != null)
            {
                return await this.Client.GetChatHistory(maxMessages);
            }
            return new List<ChatMessageEventModel>();
        }

        protected override async Task<bool> ConnectInternal()
        {
            if (ChannelSession.MixerStreamerConnection != null)
            {
                this.backgroundThreadCancellationTokenSource = new CancellationTokenSource();

                this.Client = await this.ConnectAndAuthenticateChatClient(ChannelSession.MixerStreamerConnection);
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
                    this.Client.OnSkillAttributionOccurred += Client_OnSkillAttributionOccurred;
                    this.Client.OnDisconnectOccurred += StreamerClient_OnDisconnectOccurred;
                    if (ChannelSession.Settings.DiagnosticLogging)
                    {
                        this.Client.OnPacketSentOccurred += WebSocketClient_OnPacketSentOccurred;
                        this.Client.OnMethodOccurred += WebSocketClient_OnMethodOccurred;
                        this.Client.OnReplyOccurred += WebSocketClient_OnReplyOccurred;
                        this.Client.OnEventOccurred += WebSocketClient_OnEventOccurred;
                    }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Run(async () => { await this.ChatterJoinLeaveBackground(); }, this.backgroundThreadCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Run(async () => { await this.ChatterRefreshBackground(); }, this.backgroundThreadCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    return true;
                }
                return false;
            }
            return false;
        }

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

        #region Refresh Methods

        private async Task ChatterJoinLeaveBackground()
        {
            await BackgroundTaskWrapper.RunBackgroundTask(this.backgroundThreadCancellationTokenSource, async (tokenSource) =>
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

                tokenSource.Token.ThrowIfCancellationRequested();

                await Task.Delay(2500, tokenSource.Token);

                tokenSource.Token.ThrowIfCancellationRequested();
            });
        }

        private async Task ChatterRefreshBackground()
        {
            await BackgroundTaskWrapper.RunBackgroundTask(this.backgroundThreadCancellationTokenSource, async (tokenSource) =>
            {
                await Task.Delay(300000, tokenSource.Token);

                tokenSource.Token.ThrowIfCancellationRequested();

                IEnumerable<ChatUserModel> chatUsers = await ChannelSession.MixerStreamerConnection.GetChatUsers(ChannelSession.MixerChannel, int.MaxValue);
                chatUsers = chatUsers.Where(u => u.userId.HasValue);
                HashSet<uint> chatUserIDs = new HashSet<uint>(chatUsers.Select(u => u.userId.GetValueOrDefault()));

                IEnumerable<UserViewModel> existingUsers = await ChannelSession.ActiveUsers.GetAllUsers();
                HashSet<uint> existingUsersIDs = new HashSet<uint>(existingUsers.Select(u => u.ID));

                Dictionary<uint, ChatUserModel> usersToAdd = chatUsers.ToDictionary(u => u.userId.GetValueOrDefault(), u => u);
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

                tokenSource.Token.ThrowIfCancellationRequested();
            });
        }

        #endregion Refresh Methods

        #region Chat Event Handlers

        private void ChatClient_OnMessageOccurred(object sender, ChatMessageEventModel e)
        {
            ChatMessageViewModel message = new ChatMessageViewModel(e);
            if (message != null)
            {
                this.OnMessageOccurred(sender, message);
                if (message.IsChatSkill && message.IsInUsersChannel)
                {
                    if (SkillUsageModel.IsSparksChatSkill(message.ChatSkill))
                    {
                        GlobalEvents.SparkUseOccurred(new Tuple<UserViewModel, int>(message.User, (int)message.ChatSkill.cost));
                    }
                    else if (SkillUsageModel.IsEmbersChatSkill(message.ChatSkill))
                    {
                        GlobalEvents.EmberUseOccurred(new UserEmberUsageModel(message.User, (int)message.ChatSkill.cost, message.PlainTextMessage));
                    }

                    GlobalEvents.SkillUseOccurred(new SkillUsageModel(message.User, message.ChatSkill, message.PlainTextMessage));
                }
            }
        }

        private void BotChatClient_OnMessageOccurred(object sender, ChatMessageEventModel e)
        {
            ChatMessageViewModel message = new ChatMessageViewModel(e);
            if (message.IsWhisper)
            {
                this.OnMessageOccurred(sender, message);
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

        private void ChatClient_OnPollEndOccurred(object sender, ChatPollEventModel e) { }

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
                    if (ChannelSession.Constellation.CanUserRunEvent(user, EnumHelper.GetEnumName(OtherEventTypeEnum.MixerUserBan)))
                    {
                        ChannelSession.Constellation.LogUserRunEvent(user, EnumHelper.GetEnumName(OtherEventTypeEnum.MixerUserBan));
                        await ChannelSession.Constellation.RunEventCommand(ChannelSession.Constellation.FindMatchingEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.MixerUserBan)), user);
                    }
                }
            }
        }

        private async void Client_OnSkillAttributionOccurred(object sender, ChatSkillAttributionEventModel skillAttribution)
        {
            if (!ChannelSession.Constellation.AvailableSkills.ContainsKey(skillAttribution.skill.skill_id))
            {
                ChatUserModel chatUser = skillAttribution.GetUser();
                UserViewModel user = await ChannelSession.ActiveUsers.AddOrUpdateUser(chatUser);
                if (user == null)
                {
                    user = new UserViewModel(chatUser);
                }
                else
                {
                    await user.RefreshDetails();
                }
                user.UpdateLastActivity();

                string message = null;
                if (skillAttribution.message != null && skillAttribution.message.message != null && skillAttribution.message.message.Length > 0)
                {
                    ChatMessageViewModel messageModel = new ChatMessageViewModel(skillAttribution.message, user);
                    message = messageModel.PlainTextMessage;
                }

                GlobalEvents.SkillUseOccurred(new SkillUsageModel(user, skillAttribution.skill, message));
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
