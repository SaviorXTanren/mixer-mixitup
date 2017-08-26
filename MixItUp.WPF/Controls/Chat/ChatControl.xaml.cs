using Mixer.Base.Model.Chat;
using Mixer.Base.Model.User;
using Mixer.Base.ViewModel.Chat;
using MixItUp.Base;
using MixItUp.Base.Chat;
using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for ChatControl.xaml
    /// </summary>
    public partial class ChatControl : MainControlBase
    {
        public bool EnableCommands { get; set; }

        public ObservableCollection<ChatUserControl> UserControls = new ObservableCollection<ChatUserControl>();

        public ObservableCollection<ChatMessageControl> MessageControls = new ObservableCollection<ChatMessageControl>();
        public List<ChatMessageViewModel> Messages = new List<ChatMessageViewModel>();

        private CancellationTokenSource backgroundThreadCancellationTokenSource = new CancellationTokenSource();

        public ChatControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.Window.Closing += Window_Closing;

            if (await MixerAPIHandler.InitializeChatClient(ChannelSession.Channel))
            {
                this.ChatList.ItemsSource = this.MessageControls;
                this.UserList.ItemsSource = this.UserControls;

                this.RefreshViewerCount();

                foreach (ChatUserModel user in await MixerAPIHandler.MixerConnection.Chats.GetUsers(ChannelSession.Channel))
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
                Task.Run(async () => { await this.ChannelRefreshBackground(); }, this.backgroundThreadCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(async () => { await this.TimerCommandsBackground(); }, this.backgroundThreadCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        private async Task ChannelRefreshBackground()
        {
            while (!this.backgroundThreadCancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    await ChannelSession.RefreshChannel();
                    this.RefreshViewerCount();

                    this.backgroundThreadCancellationTokenSource.Token.ThrowIfCancellationRequested();

                    await Task.Delay(1000 * 30);
                }
                catch (Exception) { }
            }

            this.backgroundThreadCancellationTokenSource.Token.ThrowIfCancellationRequested();
        }

        private async Task TimerCommandsBackground()
        {
            int timerCommandIndex = 0;
            while (!this.backgroundThreadCancellationTokenSource.Token.IsCancellationRequested)
            {               
                int startMessageCount = this.Messages.Count;
                try
                {
                    DateTimeOffset startTime = DateTimeOffset.Now;

                    Thread.Sleep(1000 * 60 * ChannelSession.Settings.TimerCommandsInterval);
                    if (ChannelSession.Settings.TimerCommands.Count > 0)
                    {
                        TimerCommand command = ChannelSession.Settings.TimerCommands[timerCommandIndex];

                        while ((this.Messages.Count - startMessageCount) <= ChannelSession.Settings.TimerCommandsMinimumMessages)
                        {
                            Thread.Sleep(1000 * 10);
                        }

                        await command.Perform();

                        timerCommandIndex++;
                        timerCommandIndex = timerCommandIndex % ChannelSession.Settings.TimerCommands.Count;
                    }
                }
                catch (ThreadAbortException) { return; }
                catch (Exception) { }
            }

            this.backgroundThreadCancellationTokenSource.Token.ThrowIfCancellationRequested();
        }

        private async void ChatClearMessagesButton_Click(object sender, RoutedEventArgs e)
        {
            await MixerAPIHandler.ChatClient.ClearMessages();
        }

        private void ChatMessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.SendChatMessageButton_Click(this, new RoutedEventArgs());
                this.ChatMessageTextBox.Focus();
            }
        }

        private async void SendChatMessageButton_Click(object sender, RoutedEventArgs e)
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.backgroundThreadCancellationTokenSource.Cancel();
        }

        private void ChatCommandEditButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ChatCommand command = (ChatCommand)button.DataContext;

            CommandDetailsWindow commandWindow = new CommandDetailsWindow(command);
            commandWindow.Show();
        }

        private void ChatCommandDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ChatCommand command = (ChatCommand)button.DataContext;
            ChannelSession.Settings.ChatCommands.Remove(command);
        }

        private void AddChatCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandDetailsWindow commandWindow = new CommandDetailsWindow(CommandTypeEnum.Chat);
            commandWindow.Show();
        }

        #region Chat Update Methods

        private void AddUser(ChatUserViewModel user)
        {
            if (!ChannelSession.ChatUsers.ContainsKey(user.ID))
            {
                ChannelSession.ChatUsers.Add(user.ID, user);
                var orderedUsers = ChannelSession.ChatUsers.Values.OrderByDescending(u => u.PrimaryRole).ThenBy(u => u.UserName).ToList();
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
                ChannelSession.ChatUsers.Remove(userControl.User.ID);

                this.RefreshViewerCount();
            }
        }

        private void RefreshViewerCount()
        {
            this.ViewersCountTextBlock.Text = string.Format("Viewers: {0} (Users: {1})", ChannelSession.Channel.viewersCurrent, ChannelSession.ChatUsers.Count);
        }

        private void AddMessage(ChatMessageViewModel message)
        {
            this.Messages.Add(message);
            this.MessageControls.Add(new ChatMessageControl(message));

            if (this.EnableCommands && ChatMessageCommand.IsCommand(message) && !message.User.Roles.Contains(UserRole.Banned))
            {
                ChatMessageCommand messageCommand = new ChatMessageCommand(message);
                ChatCommand command = ChannelSession.Settings.ChatCommands.FirstOrDefault(c => c.Command.Equals(messageCommand.CommandName));
                if (command != null)
                {
                    if (message.User.Roles.Any(r => r >= command.LowestAllowedRole))
                    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        command.Perform(message.User, messageCommand.CommandArguments);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                    else
                    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        MixerAPIHandler.ChatClient.Whisper(message.User.UserName, "You do not permission to run this command");
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                }
            }
        }

        #endregion Chat Update Methods

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
