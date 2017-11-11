using Mixer.Base.Clients;
using Mixer.Base.Model.Chat;
using Mixer.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.MixerAPI
{
    public class ChatClientWrapper : MixerRequestWrapperBase
    {
        public event EventHandler<ChatClearMessagesEventModel> OnClearMessagesOccurred = delegate { };
        public event EventHandler<ChatDeleteMessageEventModel> OnDeleteMessageOccurred = delegate { };
        public event EventHandler<ChatPollEventModel> OnPollEndOccurred = delegate { };
        public event EventHandler<ChatPollEventModel> OnPollStartOccurred = delegate { };
        public event EventHandler<ChatUserEventModel> OnUserTimeoutOccurred = delegate { };
        public event EventHandler<ChatUserEventModel> OnUserUpdateOccurred = delegate { };
        public event EventHandler<ChatUserEventModel> OnUserLeaveOccurred = delegate { };
        public event EventHandler<ChatUserEventModel> OnUserJoinOccurred = delegate { };
        public event EventHandler<ChatMessageEventModel> OnMessageOccurred = delegate { };
        public event EventHandler<ChatPurgeMessageEventModel> OnPurgeMessageOccurred = delegate { };

        public static LockedDictionary<uint, UserViewModel> ChatUsers { get; private set; }

        private static object userUpdateLock = new object();

        public ChatClient StreamerClient { get; private set; }
        public ChatClient BotClient { get; private set; }

        public ChatClientWrapper(ChatClient client)
        {
            this.StreamerClient = client;
            if (ChatClientWrapper.ChatUsers == null)
            {
                ChatClientWrapper.ChatUsers = new LockedDictionary<uint, UserViewModel>();
            }

            this.StreamerClient.OnClearMessagesOccurred += ChatClient_OnClearMessagesOccurred;
            this.StreamerClient.OnDeleteMessageOccurred += ChatClient_OnDeleteMessageOccurred;
            this.StreamerClient.OnMessageOccurred += ChatClient_OnMessageOccurred;
            this.StreamerClient.OnPollEndOccurred += ChatClient_OnPollEndOccurred;
            this.StreamerClient.OnPollStartOccurred += ChatClient_OnPollStartOccurred;
            this.StreamerClient.OnPurgeMessageOccurred += ChatClient_OnPurgeMessageOccurred;
            this.StreamerClient.OnUserJoinOccurred += ChatClient_OnUserJoinOccurred;
            this.StreamerClient.OnUserLeaveOccurred += ChatClient_OnUserLeaveOccurred;
            this.StreamerClient.OnUserTimeoutOccurred += ChatClient_OnUserTimeoutOccurred;
            this.StreamerClient.OnUserUpdateOccurred += ChatClient_OnUserUpdateOccurred;
        }

        public ChatClient GetBotClient(bool sendAsStreamer = false) { return (this.BotClient != null && !sendAsStreamer) ? this.BotClient : this.StreamerClient; }

        public async Task<bool> ConnectAndAuthenticate() { return await this.RunAsync(this.StreamerClient.Connect()) && await this.RunAsync(this.StreamerClient.Authenticate()); }
        public async Task<bool> ConnectAndAuthenticateBot(ChatClient botClient)
        {
            this.BotClient = botClient;
            if (this.BotClient != null)
            {
                return await this.RunAsync(this.BotClient.Connect()) && await this.RunAsync(this.BotClient.Authenticate());
            }
            return false;
        }

        public async Task SendMessage(string message, bool sendAsStreamer = false) { await this.RunAsync(this.GetBotClient(sendAsStreamer).SendMessage(message)); }

        public async Task Whisper(string username, string message, bool sendAsStreamer = false) { await this.RunAsync(this.GetBotClient(sendAsStreamer).Whisper(username, message)); }

        public async Task DeleteMessage(Guid id) { await this.RunAsync(this.StreamerClient.DeleteMessage(id)); }

        public async Task ClearMessages() { await this.RunAsync(this.StreamerClient.ClearMessages()); }

        public async Task PurgeUser(string username) { await this.RunAsync(this.StreamerClient.PurgeUser(username)); }

        public async Task TimeoutUser(string username, uint durationInSeconds) { await this.RunAsync(this.StreamerClient.TimeoutUser(username, durationInSeconds)); }

        public async Task Disconnect() { await this.RunAsync(this.StreamerClient.Disconnect()); }
        public async Task DisconnectBot()
        {
            if (this.BotClient != null)
            {
                await this.RunAsync(this.StreamerClient.Disconnect());
            }
            this.BotClient = null;
        }

        #region Chat Update Methods

        public async Task RefreshAllChat()
        {
            Dictionary<uint, UserViewModel> refreshUsers = new Dictionary<uint, UserViewModel>();
            foreach (ChatUserModel chatUser in await ChannelSession.Connection.GetChatUsers(ChannelSession.Channel))
            {
                UserViewModel user = new UserViewModel(chatUser);
                if (user.ID > 0)
                {
                    await user.SetDetails(checkForFollow: false);
                    refreshUsers[user.ID] = user;
                }
            }

            if (refreshUsers.Count > 0)
            {
                Dictionary<UserModel, DateTimeOffset?> chatFollowers = await ChannelSession.Connection.CheckIfFollows(ChannelSession.Channel, refreshUsers.Values.Select(u => u.GetModel()));
                foreach (var kvp in chatFollowers)
                {
                    refreshUsers[kvp.Key.id].IsFollower = true;
                }
            }

            lock (userUpdateLock)
            {
                ChatClientWrapper.ChatUsers.Clear();
                foreach (UserViewModel user in refreshUsers.Values)
                {
                    ChatClientWrapper.ChatUsers[user.ID] = user;
                }
            }
        }

        public async Task GetAndAddUser(uint userID)
        {
            ChatUserModel user = await ChannelSession.Connection.GetChatUser(ChannelSession.Channel, userID);
            if (user != null)
            {
                await this.SetDetailsAndAddUser(new UserViewModel(user));
            }
        }

        public async Task SetDetailsAndAddUser(UserViewModel user)
        {
            await user.SetDetails();
            this.AddUser(user);
        }

        public void AddUser(UserViewModel user)
        {
            lock (userUpdateLock)
            {
                if (!ChatClientWrapper.ChatUsers.ContainsKey(user.ID))
                {
                    ChatClientWrapper.ChatUsers[user.ID] = user;
                }
            }
        }

        public void RemoveUser(UserViewModel user)
        {
            lock (userUpdateLock)
            {
                ChatClientWrapper.ChatUsers.Remove(user.ID);
            }
        }

        #endregion Chat Update Methods

        #region Chat Event Handlers

        private void ChatClient_OnClearMessagesOccurred(object sender, ChatClearMessagesEventModel e) { this.OnClearMessagesOccurred(sender, e); }

        private void ChatClient_OnDeleteMessageOccurred(object sender, ChatDeleteMessageEventModel e) { this.OnDeleteMessageOccurred(sender, e); }

        private void ChatClient_OnMessageOccurred(object sender, ChatMessageEventModel e) { this.OnMessageOccurred(sender, e); }

        private void ChatClient_OnPollStartOccurred(object sender, ChatPollEventModel e) { this.OnPollStartOccurred(sender, e); }

        private void ChatClient_OnPollEndOccurred(object sender, ChatPollEventModel e) { this.OnPollEndOccurred(sender, e); }

        private void ChatClient_OnPurgeMessageOccurred(object sender, ChatPurgeMessageEventModel e) { this.OnPurgeMessageOccurred(sender, e); }

        private async void ChatClient_OnUserJoinOccurred(object sender, ChatUserEventModel e)
        {
            UserViewModel user = new UserViewModel(e);
            await this.SetDetailsAndAddUser(user);

            GlobalEvents.ChatUserJoined(user);

            this.OnUserJoinOccurred(sender, e);
        }

        private void ChatClient_OnUserLeaveOccurred(object sender, ChatUserEventModel e)
        {
            UserViewModel user = new UserViewModel(e);
            this.RemoveUser(user);

            this.OnUserLeaveOccurred(sender, e);
        }

        private void ChatClient_OnUserTimeoutOccurred(object sender, ChatUserEventModel e)
        {
            UserViewModel user = new UserViewModel(e);
            this.RemoveUser(user);

            this.OnUserTimeoutOccurred(sender, e);
        }

        private async void ChatClient_OnUserUpdateOccurred(object sender, ChatUserEventModel e)
        {
            UserViewModel user = new UserViewModel(e);
            this.RemoveUser(user);
            await this.SetDetailsAndAddUser(user);

            GlobalEvents.ChatUserJoined(user);

            this.OnUserUpdateOccurred(sender, e);
        }

        #endregion Chat Event Handlers
    }
}
