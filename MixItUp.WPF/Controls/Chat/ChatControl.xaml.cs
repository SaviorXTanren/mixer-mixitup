using Mixer.Base.Model.Chat;
using Mixer.Base.Model.User;
using Mixer.Base.ViewModel.Chat;
using MixItUp.Base;
using MixItUp.Base.Chat;
using MixItUp.Base.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for ChatControl.xaml
    /// </summary>
    public partial class ChatControl : MainControlBase
    {
        public ObservableCollection<ChatUserControl> UserControls = new ObservableCollection<ChatUserControl>();
        public Dictionary<uint, ChatUserViewModel> Users = new Dictionary<uint, ChatUserViewModel>();

        public ObservableCollection<ChatMessageControl> MessageControls = new ObservableCollection<ChatMessageControl>();
        public List<ChatMessageViewModel> Messages = new List<ChatMessageViewModel>();

        private CancellationTokenSource channelRefreshCancellationTokenSource = new CancellationTokenSource();

        public ChatControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.Window.Closing += Window_Closing;

            if (await MixerAPIHandler.InitializeChatClient(this.Window.Channel))
            {
                this.ChatList.ItemsSource = this.MessageControls;
                this.UserList.ItemsSource = this.UserControls;

                this.RefreshViewerCount();

                foreach (ChatUserModel user in await MixerAPIHandler.MixerConnection.Chats.GetUsers(this.Window.Channel))
                {
                    this.AddUser(new ChatUserViewModel(user));
                }

                MixerAPIHandler.ChatClient.OnClearMessagesOccurred += ChatClient_OnClearMessagesOccurred;
                MixerAPIHandler.ChatClient.OnDeleteMessageOccurred += ChatClient_OnDeleteMessageOccurred;
                MixerAPIHandler.ChatClient.OnDisconnectOccurred += ChatClient_OnDisconnectOccurred;
                MixerAPIHandler.ChatClient.OnMessageOccurred += ChatClient_OnMessageOccurred;
                MixerAPIHandler.ChatClient.OnPollEndOccurred += ChatClient_OnPollEndOccurred;
                MixerAPIHandler.ChatClient.OnPollStartOccurred += ChatClient_OnPollStartOccurred;
                MixerAPIHandler.ChatClient.OnPurgeMessageOccurred += ChatClient_OnPurgeMessageOccurred;
                MixerAPIHandler.ChatClient.OnUserJoinOccurred += ChatClient_OnUserJoinOccurred;
                MixerAPIHandler.ChatClient.OnUserLeaveOccurred += ChatClient_OnUserLeaveOccurred;
                MixerAPIHandler.ChatClient.OnUserTimeoutOccurred += ChatClient_OnUserTimeoutOccurred;
                MixerAPIHandler.ChatClient.OnUserUpdateOccurred += ChatClient_OnUserUpdateOccurred;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(async () =>
                {
                    while (!this.channelRefreshCancellationTokenSource.Token.IsCancellationRequested)
                    {
                        try
                        {
                            await this.Window.RefreshChannel();
                            this.RefreshViewerCount();

                            this.channelRefreshCancellationTokenSource.Token.ThrowIfCancellationRequested();

                            await Task.Delay(1000 * 30);
                        }
                        catch (Exception) { }
                    }

                    this.channelRefreshCancellationTokenSource.Token.ThrowIfCancellationRequested();

                }, this.channelRefreshCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        private void AddUser(ChatUserViewModel user)
        {
            if (!this.Users.ContainsKey(user.ID))
            {
                this.Users.Add(user.ID, user);
                var orderedUsers = this.Users.Values.OrderByDescending(u => u.PrimaryRole).ThenBy(u => u.UserName).ToList();
                this.UserControls.Insert(orderedUsers.IndexOf(user), new ChatUserControl(user));

                this.RefreshViewerCount();
            }
        }

        private void RemoveUser(ChatUserViewModel user)
        {
            ChatUserControl userControl = this.UserControls.FirstOrDefault(u => u.User.Equals(user));
            if (userControl != null)
            {
                this.UserControls.Remove(userControl);
                this.Users.Remove(userControl.User.ID);

                this.RefreshViewerCount();
            }
        }

        private void AddMessage(ChatMessageViewModel message)
        {
            this.Messages.Add(message);
            this.MessageControls.Add(new ChatMessageControl(message));

            if (MixerAPIHandler.Settings != null && ChatMessageCommand.IsCommand(message))
            {
                ChatMessageCommand messageCommand = new ChatMessageCommand(message);
                ChatCommand command = MixerAPIHandler.Settings.ChatCommands.FirstOrDefault(c => c.Command.Equals(messageCommand.CommandName));
                if (command != null)
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    command.Perform(message.User, messageCommand.CommandArguments);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
        }

        private void RefreshViewerCount()
        {
            this.ViewersCountTextBlock.Text = string.Format("Viewers: {0} (Users: {1})", this.Window.Channel.viewersCurrent, this.Users.Count);
        }

        private async void ChatClearMessagesButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await MixerAPIHandler.ChatClient.ClearMessages();
        }

        private async Task SendChatMessage()
        {
            if (!string.IsNullOrEmpty(this.ChatMessageTextBox.Text))
            {
                string message = this.ChatMessageTextBox.Text;
                this.ChatMessageTextBox.Text = string.Empty;
                await this.Window.RunAsyncOperation(async () =>
                {
                    await MixerAPIHandler.ChatClient.SendMessage(message);
                });
            }
        }

        private async void ChatMessageTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await this.SendChatMessage();
                this.ChatMessageTextBox.Focus();
            }
        }

        private async void SendChatMessageButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.SendChatMessage();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.channelRefreshCancellationTokenSource.Cancel();
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

        private void ChatClient_OnDisconnectOccurred(object sender, WebSocketCloseStatus e)
        {
            // Show Re-Connecting...
        }

        private void ChatClient_OnClearMessagesOccurred(object sender, ChatClearMessagesEventModel e)
        {
            this.AddMessage(new ChatMessageViewModel("--- MESSAGES CLEARED ---"));
        }

        private void ChatClient_OnDeleteMessageOccurred(object sender, ChatDeleteMessageEventModel e)
        {
            ChatMessageControl message = this.MessageControls.FirstOrDefault(msg => msg.Message.ID.Equals(e.id));
            if (message != null)
            {
                message.Message.IsDeleted = true;
            }
        }

        private void ChatClient_OnMessageOccurred(object sender, ChatMessageEventModel e)
        {
            this.AddMessage(new ChatMessageViewModel(e));
        }

        private void ChatClient_OnPollEndOccurred(object sender, ChatPollEventModel e)
        {
            
        }

        private void ChatClient_OnPollStartOccurred(object sender, ChatPollEventModel e)
        {
            
        }

        private void ChatClient_OnPurgeMessageOccurred(object sender, ChatPurgeMessageEventModel e)
        {
            IEnumerable<ChatMessageControl> userMessages = this.MessageControls.Where(msg => msg.Message.User != null && msg.Message.User.ID.Equals(e.user_id));
            if (userMessages != null)
            {
                foreach (ChatMessageControl message in userMessages)
                {
                    message.Message.IsDeleted = true;
                }
            }
        }

        private void ChatClient_OnUserJoinOccurred(object sender, ChatUserEventModel e)
        {
            ChatUserViewModel user = new ChatUserViewModel(e);
            this.AddUser(user);
        }

        private void ChatClient_OnUserLeaveOccurred(object sender, ChatUserEventModel e)
        {
            ChatUserViewModel user = new ChatUserViewModel(e);
            this.RemoveUser(user);
        }

        private void ChatClient_OnUserTimeoutOccurred(object sender, ChatUserEventModel e)
        {
            
        }

        private void ChatClient_OnUserUpdateOccurred(object sender, ChatUserEventModel e)
        {
            ChatUserViewModel user = new ChatUserViewModel(e);
            this.RemoveUser(user);
            this.AddUser(user);
        }

        #endregion Chat Event Handlers
    }
}
