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

        public ChatClient Client { get; private set; }

        public ChatClientWrapper(ChatClient client)
        {
            this.Client = client;
            if (ChatClientWrapper.ChatUsers == null)
            {
                ChatClientWrapper.ChatUsers = new LockedDictionary<uint, UserViewModel>();
            }

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
        }

        public async Task<bool> ConnectAndAuthenticate() { return await this.RunAsync(this.Client.Connect()) && await this.RunAsync(this.Client.Authenticate()); }

        public async Task SendMessage(string message) { await this.RunAsync(this.Client.SendMessage(message)); }

        public async Task Whisper(string username, string message) { await this.RunAsync(this.Client.Whisper(username, message)); }

        public async Task DeleteMessage(Guid id) { await this.RunAsync(this.Client.DeleteMessage(id)); }

        public async Task ClearMessages() { await this.RunAsync(this.Client.ClearMessages()); }

        public async Task PurgeUser(string username) { await this.RunAsync(this.Client.PurgeUser(username)); }

        public async Task TimeoutUser(string username, uint durationInSeconds) { await this.RunAsync(this.Client.TimeoutUser(username, durationInSeconds)); }

        public async Task Disconnect() { await this.RunAsync(this.Client.Disconnect()); }

        #region Chat Update Methods

        public async Task RefreshAllChat()
        {
            await ChannelSession.RefreshChannel();
 
            Dictionary<uint, UserViewModel> refreshUsers = new Dictionary<uint, UserViewModel>();
            foreach (ChatUserModel chatUser in await ChannelSession.Connection.GetChatUsers(ChannelSession.Channel))
            {
                UserViewModel user = new UserViewModel(chatUser);
                if (user.ID > 0)
                {
                    await user.SetDetails(checkForFollow: false);
                    refreshUsers.Add(user.ID, user);
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
            UserModel user = await ChannelSession.Connection.GetUser(userID);
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
