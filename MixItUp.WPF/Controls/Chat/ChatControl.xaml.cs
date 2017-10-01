using Mixer.Base.Model.Chat;
using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.Base.Chat;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel;
using MixItUp.Base.ViewModel.Chat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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

        private bool blockChat = false;

        public ChatControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.Window.Closing += Window_Closing;

            if (await ChannelSession.InitializeChatClient())
            {
                this.ChatList.ItemsSource = this.MessageControls;
                this.UserList.ItemsSource = this.UserControls;

                this.RefreshViewerAndChatCount();

                foreach (ChatUserModel user in await ChannelSession.MixerConnection.Chats.GetUsers(ChannelSession.Channel))
                {
                    await this.AddUser(new UserViewModel(user), checkIfFollows: false);
                }

                if (ChannelSession.ChatUsers.Count > 0)
                {
                    Dictionary<UserModel, DateTimeOffset?> chatFollowers = await ChannelSession.MixerConnection.Channels.CheckIfFollows(ChannelSession.Channel, ChannelSession.ChatUsers.Values.Select(u => u.GetModel()));
                    foreach (var kvp in chatFollowers)
                    {
                        ChannelSession.ChatUsers[kvp.Key.id].Roles.Add(UserRole.Follower);
                    }
                }

                ChannelSession.ChatClient.OnClearMessagesOccurred += ChatClient_OnClearMessagesOccurred;
                ChannelSession.ChatClient.OnDeleteMessageOccurred += ChatClient_OnDeleteMessageOccurred;
                ChannelSession.ChatClient.OnDisconnectOccurred += ChatClient_OnDisconnectOccurred;
                ChannelSession.ChatClient.OnMessageOccurred += ChatClient_OnMessageOccurred;
                ChannelSession.ChatClient.OnPollEndOccurred += ChatClient_OnPollEndOccurred;
                ChannelSession.ChatClient.OnPollStartOccurred += ChatClient_OnPollStartOccurred;
                ChannelSession.ChatClient.OnPurgeMessageOccurred += ChatClient_OnPurgeMessageOccurred;
                ChannelSession.ChatClient.OnUserJoinOccurred += ChatClient_OnUserJoinOccurred;
                ChannelSession.ChatClient.OnUserLeaveOccurred += ChatClient_OnUserLeaveOccurred;
                ChannelSession.ChatClient.OnUserTimeoutOccurred += ChatClient_OnUserTimeoutOccurred;
                ChannelSession.ChatClient.OnUserUpdateOccurred += ChatClient_OnUserUpdateOccurred;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(async () => { await this.ChannelRefreshBackground(); }, this.backgroundThreadCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(async () => { await this.TimerCommandsBackground(); }, this.backgroundThreadCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                if (this.EnableCommands)
                {
                    ChannelSession.PreMadeChatCommands.Add(new UptimeChatCommand());
                    ChannelSession.PreMadeChatCommands.Add(new GameChatCommand());
                    ChannelSession.PreMadeChatCommands.Add(new TitleChatCommand());
                    ChannelSession.PreMadeChatCommands.Add(new TimeoutChatCommand());
                    ChannelSession.PreMadeChatCommands.Add(new PurgeChatCommand());
                    ChannelSession.PreMadeChatCommands.Add(new StreamerAgeChatCommand());
                    ChannelSession.PreMadeChatCommands.Add(new MixerAgeChatCommand());
                    ChannelSession.PreMadeChatCommands.Add(new FollowAgeChatCommand());
                    ChannelSession.PreMadeChatCommands.Add(new SparksChatCommand());
                    ChannelSession.PreMadeChatCommands.Add(new QuoteChatCommand());
                    ChannelSession.PreMadeChatCommands.Add(new GiveawayChatCommand());

                    foreach (PreMadeChatCommandSettings setting in ChannelSession.Settings.PreMadeChatCommandSettings)
                    {
                        PreMadeChatCommand command = ChannelSession.PreMadeChatCommands.FirstOrDefault(c => c.Name.Equals(setting.Name));
                        if (command != null)
                        {
                            command.UpdateFromSettings(setting);
                        }
                    }
                }
            }
        }

        private async Task ChannelRefreshBackground()
        {
            while (!this.backgroundThreadCancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    await ChannelSession.RefreshChannel();
                    this.RefreshViewerAndChatCount();

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

                    await Task.Delay(1000 * 60 * ChannelSession.Settings.TimerCommandsInterval);
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
            await ChannelSession.ChatClient.ClearMessages();
        }

        private void BlockChatButton_Click(object sender, RoutedEventArgs e)
        {
            this.blockChat = true;
            this.BlockChatButton.Visibility = Visibility.Collapsed;
            this.EnableChatButton.Visibility = Visibility.Visible;
        }

        private void EnableChatButton_Click(object sender, RoutedEventArgs e)
        {
            this.blockChat = false;
            this.BlockChatButton.Visibility = Visibility.Visible;
            this.EnableChatButton.Visibility = Visibility.Collapsed;
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
                    await ChannelSession.ChatClient.SendMessage(message);
                });
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.backgroundThreadCancellationTokenSource.Cancel();
        }

        #region Chat Update Methods

        private async Task AddUser(UserViewModel user, bool checkIfFollows = true)
        {
            if (!ChannelSession.ChatUsers.ContainsKey(user.ID))
            {
                ChannelSession.ChatUsers.Add(user.ID, user);

                if (checkIfFollows)
                {
                    DateTimeOffset? followDate = await ChannelSession.MixerConnection.Channels.CheckIfFollows(ChannelSession.Channel, user.GetModel());
                    if (followDate != null)
                    {
                        user.Roles.Add(UserRole.Follower);
                    }
                }

                if (user.ID > 0)
                {
                    UserWithChannelModel userWithChannel = await ChannelSession.MixerConnection.Users.GetUser(user.GetModel());
                    user.AvatarLink = userWithChannel.avatarUrl;
                }

                var orderedUsers = ChannelSession.ChatUsers.Values.OrderByDescending(u => u.PrimaryRole).ThenBy(u => u.UserName).ToList();
                this.UserControls.Insert(orderedUsers.IndexOf(user), new ChatUserControl(user));

                this.RefreshViewerAndChatCount();
            }
        }

        private void RemoveUser(UserViewModel user)
        {
            ChatUserControl userControl = this.UserControls.FirstOrDefault(u => u.User.Equals(user));
            if (userControl != null)
            {
                this.UserControls.Remove(userControl);
                ChannelSession.ChatUsers.Remove(userControl.User.ID);

                this.RefreshViewerAndChatCount();
            }
        }

        private void RefreshViewerAndChatCount()
        {
            this.ViewersCountTextBlock.Text = ChannelSession.Channel.viewersCurrent.ToString();
            this.ChatCountTextBlock.Text = ChannelSession.ChatUsers.Count.ToString();
        }

        private async void AddMessage(ChatMessageViewModel message)
        {
            ChatMessageControl messageControl = new ChatMessageControl(message);

            this.Messages.Add(message);
            this.MessageControls.Add(messageControl);

            this.ChatList.ScrollIntoView(messageControl);

            await this.AddUser(message.User);

            if (this.blockChat && !message.ID.Equals(Guid.Empty))
            {
                messageControl.DeleteMessage();
                await ChannelSession.ChatClient.DeleteMessage(message.ID);
            }
            else
            {
                if (!messageControl.Message.IsWhisper && messageControl.Message.User.PrimaryRole < UserRole.Mod && messageControl.Message.ShouldBeModerated())
                {
                    await ChannelSession.ChatClient.DeleteMessage(messageControl.Message.ID);
                    messageControl.DeleteMessage();

                    messageControl.Message.User.ChatOffenses++;
                    if (ChannelSession.Settings.Timeout5MinuteOffenseCount > 0 && messageControl.Message.User.ChatOffenses >= ChannelSession.Settings.Timeout5MinuteOffenseCount)
                    {
                        await ChannelSession.ChatClient.Whisper(messageControl.Message.User.UserName, "You have been timed out from chat for 5 minutes due to chat moderation. Please watch what you type in chat or further actions will be taken.");
                        await ChannelSession.ChatClient.TimeoutUser(messageControl.Message.User.UserName, 300);
                    }
                    else if (ChannelSession.Settings.Timeout1MinuteOffenseCount > 0 && messageControl.Message.User.ChatOffenses >= ChannelSession.Settings.Timeout1MinuteOffenseCount)
                    {
                        await ChannelSession.ChatClient.Whisper(messageControl.Message.User.UserName, "You have been timed out from chat for 1 minute due to chat moderation. Please watch what you type in chat or further actions will be taken.");
                        await ChannelSession.ChatClient.TimeoutUser(messageControl.Message.User.UserName, 60);
                    }
                    else
                    {
                        await ChannelSession.ChatClient.Whisper(messageControl.Message.User.UserName, "Your message has been deleted due to chat moderation. Please watch what you type in chat or further actions will be taken.");
                    }
                }
                else if (this.EnableCommands && ChatMessageCommand.IsCommand(message) && !message.User.Roles.Contains(UserRole.Banned))
                {
                    ChatMessageCommand messageCommand = new ChatMessageCommand(message);

                    ChatCommand command = ChannelSession.PreMadeChatCommands.FirstOrDefault(c => c.ContainsCommand(messageCommand.CommandName));
                    if (command == null)
                    {
                        command = ChannelSession.Settings.ChatCommands.FirstOrDefault(c => c.ContainsCommand(messageCommand.CommandName));
                    }

                    if (command != null)
                    {
                        if (message.User.Roles.Any(r => r >= command.Permissions))
                        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                            command.Perform(message.User, messageCommand.CommandArguments);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        }
                        else
                        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                            ChannelSession.BotChatClient.Whisper(message.User.UserName, "You do not permission to run this command");
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        }
                    }
                }
            }
        }

        private async Task PurgeUser(UserViewModel user)
        {
            await ChannelSession.ChatClient.PurgeUser(user.UserName);
            foreach (ChatMessageControl messageControl in this.MessageControls)
            {
                if (messageControl.Message.User.Equals(user) && !messageControl.Message.IsWhisper)
                {
                    messageControl.DeleteMessage();
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
                if (!control.Message.IsWhisper)
                {
                    await ChannelSession.ChatClient.DeleteMessage(control.Message.ID);
                    control.DeleteMessage();
                }
            }
        }

        private async void MessageUserPurgeMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.ChatList.SelectedItem != null)
            {
                ChatMessageControl control = (ChatMessageControl)this.ChatList.SelectedItem;
                if (control.Message.User != null)
                {
                    await this.PurgeUser(control.Message.User);
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
                    await ChannelSession.ChatClient.TimeoutUser(control.Message.User.UserName, 60);
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
                    await ChannelSession.ChatClient.TimeoutUser(control.Message.User.UserName, 300);
                }
            }
        }

        private async void UserPurgeMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.UserList.SelectedItem != null)
            {
                ChatUserControl control = (ChatUserControl)this.UserList.SelectedItem;
                await this.PurgeUser(control.User);
            }
        }

        private async void UserTimeout1MenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.UserList.SelectedItem != null)
            {
                ChatUserControl control = (ChatUserControl)this.UserList.SelectedItem;
                await ChannelSession.ChatClient.TimeoutUser(control.User.UserName, 60);
            }
        }

        private async void UserTimeout5MenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.UserList.SelectedItem != null)
            {
                ChatUserControl control = (ChatUserControl)this.UserList.SelectedItem;
                await ChannelSession.ChatClient.TimeoutUser(control.User.UserName, 300);
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
        }

        private void ChatClient_OnDeleteMessageOccurred(object sender, ChatDeleteMessageEventModel e)
        {
            ChatMessageControl message = this.MessageControls.FirstOrDefault(msg => msg.Message.ID.Equals(e.id));
            if (message != null)
            {
                message.DeleteMessage();
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
                    message.DeleteMessage();
                }
            }
        }

        private async void ChatClient_OnUserJoinOccurred(object sender, ChatUserEventModel e)
        {
            UserViewModel user = new UserViewModel(e);
            await this.AddUser(user);
        }

        private void ChatClient_OnUserLeaveOccurred(object sender, ChatUserEventModel e)
        {
            UserViewModel user = new UserViewModel(e);
            this.RemoveUser(user);
        }

        private void ChatClient_OnUserTimeoutOccurred(object sender, ChatUserEventModel e)
        {
            
        }

        private async void ChatClient_OnUserUpdateOccurred(object sender, ChatUserEventModel e)
        {
            UserViewModel user = new UserViewModel(e);
            this.RemoveUser(user);
            await this.AddUser(user);
        }

        #endregion Chat Event Handlers
    }
}
