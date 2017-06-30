using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Chat;
using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for ChatControl.xaml
    /// </summary>
    public partial class ChatControl : UserControl
    {
        public ChannelModel Channel { get; private set; }

        public int ViewerCount { get; private set; }

        public ObservableCollection<ChatUserControl> UserControls = new ObservableCollection<ChatUserControl>();
        public List<ChatUserViewModel> Users = new List<ChatUserViewModel>();

        public ObservableCollection<ChatMessageControl> MessageControls = new ObservableCollection<ChatMessageControl>();
        public List<ChatMessageViewModel> Messages = new List<ChatMessageViewModel>();

        public ChatControl()
        {
            InitializeComponent();
        }

        public async Task Initialize(ChannelModel channel)
        {
            this.Channel = channel;
            if (await MixerAPIHandler.InitializeChatClient(this.Channel))
            {
                this.ChatList.ItemsSource = this.MessageControls;
                this.UserList.ItemsSource = this.UserControls;

                foreach (ChatUserModel user in await MixerAPIHandler.MixerClient.Chats.GetUsers(this.Channel))
                {
                    this.AddUser(new ChatUserViewModel(user));
                }

                MixerAPIHandler.ChatClient.ClearMessagesOccurred += ChatClient_ClearMessagesOccurred;
                MixerAPIHandler.ChatClient.DeleteMessageOccurred += ChatClient_DeleteMessageOccurred;
                MixerAPIHandler.ChatClient.DisconnectOccurred += ChatClient_DisconnectOccurred;
                MixerAPIHandler.ChatClient.MessageOccurred += ChatClient_MessageOccurred;
                MixerAPIHandler.ChatClient.PollEndOccurred += ChatClient_PollEndOccurred;
                MixerAPIHandler.ChatClient.PollStartOccurred += ChatClient_PollStartOccurred;
                MixerAPIHandler.ChatClient.PurgeMessageOccurred += ChatClient_PurgeMessageOccurred;
                MixerAPIHandler.ChatClient.UserJoinOccurred += ChatClient_UserJoinOccurred;
                MixerAPIHandler.ChatClient.UserLeaveOccurred += ChatClient_UserLeaveOccurred;
                MixerAPIHandler.ChatClient.UserTimeoutOccurred += ChatClient_UserTimeoutOccurred;
                MixerAPIHandler.ChatClient.UserUpdateOccurred += ChatClient_UserUpdateOccurred;
            }
        }

        private void AddUser(ChatUserViewModel user)
        {
            if (!this.Users.Contains(user))
            {
                this.Users.Add(user);
                this.Users = this.Users.OrderByDescending(u => u.PrimaryRole).ThenBy(u => u.UserName).ToList();
                this.UserControls.Insert(this.Users.IndexOf(user), new ChatUserControl(user));

                this.UserCountTextBlock.Text = this.Users.Count.ToString();
            }
        }

        private void RemoveUser(ChatUserViewModel user)
        {
            ChatUserControl userControl = this.UserControls.FirstOrDefault(u => u.User.Equals(user));
            if (userControl != null)
            {
                this.UserControls.Remove(userControl);
                this.Users.Remove(userControl.User);

                this.UserCountTextBlock.Text = this.Users.Count.ToString();
            }
        }

        private void AddMessage(ChatMessageViewModel message)
        {
            this.Messages.Add(message);
            this.MessageControls.Add(new ChatMessageControl(message));
        }

        private async void ChatClearMessagesButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await MixerAPIHandler.ChatClient.ClearMessages();
        }

        #region Context Menu Events

        private async void MessageDeleteMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.ChatList.SelectedItem != null)
            {
                ChatMessageControl control = (ChatMessageControl)this.ChatList.SelectedItem;
                await MixerAPIHandler.ChatClient.DeleteMessage(control.Message.ID);
            }
        }

        private async void MessageUserPurgeMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.ChatList.SelectedItem != null)
            {
                ChatMessageControl control = (ChatMessageControl)this.ChatList.SelectedItem;
                if (control.Message.User != null)
                {
                    await MixerAPIHandler.ChatClient.PurgeUser(control.Message.User.UserName);
                }
            }
        }

        private async void MessageUserTimeout1MenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.ChatList.SelectedItem != null)
            {
                ChatMessageControl control = (ChatMessageControl)this.ChatList.SelectedItem;
                if (control.Message.User != null)
                {
                    await MixerAPIHandler.ChatClient.TimeoutUser(control.Message.User.UserName, 60);
                }
            }
        }

        private async void MessageUserTimeout5MenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.ChatList.SelectedItem != null)
            {
                ChatMessageControl control = (ChatMessageControl)this.ChatList.SelectedItem;
                if (control.Message.User != null)
                {
                    await MixerAPIHandler.ChatClient.TimeoutUser(control.Message.User.UserName, 300);
                }
            }
        }

        private async void UserPurgeMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.UserList.SelectedItem != null)
            {
                ChatUserControl control = (ChatUserControl)this.UserList.SelectedItem;
                await MixerAPIHandler.ChatClient.PurgeUser(control.User.UserName);
            }
        }

        private async void UserTimeout1MenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.UserList.SelectedItem != null)
            {
                ChatUserControl control = (ChatUserControl)this.UserList.SelectedItem;
                await MixerAPIHandler.ChatClient.TimeoutUser(control.User.UserName, 60);
            }
        }

        private async void UserTimeout5MenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.UserList.SelectedItem != null)
            {
                ChatUserControl control = (ChatUserControl)this.UserList.SelectedItem;
                await MixerAPIHandler.ChatClient.TimeoutUser(control.User.UserName, 300);
            }
        }

        #endregion Context Menu Events

        #region Chat Event Handlers

        private void ChatClient_DisconnectOccurred(object sender, WebSocketCloseStatus e)
        {
            // Show Re-Connecting...
        }

        private void ChatClient_ClearMessagesOccurred(object sender, EventArgs e)
        {
            this.AddMessage(new ChatMessageViewModel("--- MESSAGES CLEARED ---"));
        }

        private void ChatClient_DeleteMessageOccurred(object sender, Guid messageID)
        {
            ChatMessageControl message = this.MessageControls.FirstOrDefault(msg => msg.Message.ID.Equals(messageID));
            if (message != null)
            {
                message.Message.IsDeleted = true;
            }
        }

        private void ChatClient_MessageOccurred(object sender, ChatMessageEventModel e)
        {
            this.AddMessage(new ChatMessageViewModel(e));
        }

        private void ChatClient_PollEndOccurred(object sender, ChatPollEventModel e)
        {
            
        }

        private void ChatClient_PollStartOccurred(object sender, ChatPollEventModel e)
        {
            
        }

        private void ChatClient_PurgeMessageOccurred(object sender, uint userID)
        {
            IEnumerable<ChatMessageControl> userMessages = this.MessageControls.Where(msg => msg.Message.User != null && msg.Message.User.ID.Equals(userID));
            if (userMessages != null)
            {
                foreach (ChatMessageControl message in userMessages)
                {
                    message.Message.IsDeleted = true;
                }
            }
        }

        private void ChatClient_UserJoinOccurred(object sender, ChatUserEventModel e)
        {
            ChatUserViewModel user = new ChatUserViewModel(e);
            this.AddUser(user);
        }

        private void ChatClient_UserLeaveOccurred(object sender, ChatUserEventModel e)
        {
            ChatUserViewModel user = new ChatUserViewModel(e);
            this.RemoveUser(user);
        }

        private void ChatClient_UserTimeoutOccurred(object sender, ChatUserEventModel e)
        {
            
        }

        private void ChatClient_UserUpdateOccurred(object sender, ChatUserEventModel e)
        {
            ChatUserViewModel user = new ChatUserViewModel(e);
            this.RemoveUser(user);
            this.AddUser(user);
        }

        #endregion Chat Event Handlers
    }
}
