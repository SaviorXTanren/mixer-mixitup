using Mixer.Base.Model.Chat;
using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for ChatControl.xaml
    /// </summary>
    public partial class ChatControl : MainControlBase
    {
        public static BitmapImage SubscriberBadgeBitmap { get; private set; }

        private const string UserNameTagRegexPattern = " @\\w+ ";

        private static readonly Regex UserNameTagRegex = new Regex(UserNameTagRegexPattern);
        private static readonly Regex WhisperRegex = new Regex("/w" + UserNameTagRegexPattern);

        private static object userUpdateLock = new object();

        public bool EnableCommands { get; set; }

        public ObservableCollection<ChatUserControl> UserControls = new ObservableCollection<ChatUserControl>();

        public ObservableCollection<ChatMessageControl> MessageControls = new ObservableCollection<ChatMessageControl>();

        private CancellationTokenSource backgroundThreadCancellationTokenSource = new CancellationTokenSource();

        private bool disableChat = false;
        private int totalMessages = 0;

        public ChatControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.Window.Closing += Window_Closing;

            if (await ChannelSession.ConnectChat())
            {
                this.ChatList.ItemsSource = this.MessageControls;
                this.UserList.ItemsSource = this.UserControls;

                ChannelSession.Chat.Client.OnClearMessagesOccurred += ChatClient_OnClearMessagesOccurred;
                ChannelSession.Chat.Client.OnDeleteMessageOccurred += ChatClient_OnDeleteMessageOccurred;
                ChannelSession.Chat.Client.OnDisconnectOccurred += ChatClient_OnDisconnectOccurred;
                ChannelSession.Chat.Client.OnMessageOccurred += ChatClient_OnMessageOccurred;
                ChannelSession.Chat.Client.OnPollEndOccurred += ChatClient_OnPollEndOccurred;
                ChannelSession.Chat.Client.OnPollStartOccurred += ChatClient_OnPollStartOccurred;
                ChannelSession.Chat.Client.OnPurgeMessageOccurred += ChatClient_OnPurgeMessageOccurred;
                ChannelSession.Chat.Client.OnUserJoinOccurred += ChatClient_OnUserJoinOccurred;
                ChannelSession.Chat.Client.OnUserLeaveOccurred += ChatClient_OnUserLeaveOccurred;
                ChannelSession.Chat.Client.OnUserTimeoutOccurred += ChatClient_OnUserTimeoutOccurred;
                ChannelSession.Chat.Client.OnUserUpdateOccurred += ChatClient_OnUserUpdateOccurred;

                if (ChannelSession.Channel.badge != null && !string.IsNullOrEmpty(ChannelSession.Channel.badge.url))
                {
                    ChatControl.SubscriberBadgeBitmap = new BitmapImage();
                    ChatControl.SubscriberBadgeBitmap.BeginInit();
                    ChatControl.SubscriberBadgeBitmap.UriSource = new Uri(ChannelSession.Channel.badge.url, UriKind.Absolute);
                    ChatControl.SubscriberBadgeBitmap.EndInit();
                }

                await this.RefreshAllChat();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(async () => { await this.ChannelRefreshBackground(); }, this.backgroundThreadCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(async () => { await this.TimerCommandsBackground(); }, this.backgroundThreadCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                if (this.EnableCommands)
                {
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
            }
        }

        private async Task ChannelRefreshBackground()
        {
            while (!this.backgroundThreadCancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    this.backgroundThreadCancellationTokenSource.Token.ThrowIfCancellationRequested();

                    await Task.Delay(1000 * 30);

                    this.backgroundThreadCancellationTokenSource.Token.ThrowIfCancellationRequested();

                    await this.RefreshAllChat();
                }
                catch (Exception ex) { string str = ex.ToString(); }
            }

            this.backgroundThreadCancellationTokenSource.Token.ThrowIfCancellationRequested();
        }

        private async Task TimerCommandsBackground()
        {
            int timerCommandIndex = 0;
            while (!this.backgroundThreadCancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    DateTimeOffset startTime = DateTimeOffset.Now;
                    int startMessageCount = this.totalMessages;

                    await Task.Delay(1000 * 60 * ChannelSession.Settings.TimerCommandsInterval);
                    if (ChannelSession.Settings.TimerCommands.Count > 0)
                    {
                        TimerCommand command = ChannelSession.Settings.TimerCommands[timerCommandIndex];

                        while ((this.totalMessages - startMessageCount) < ChannelSession.Settings.TimerCommandsMinimumMessages)
                        {
                            await Task.Delay(1000 * 10);
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
            if (await MessageBoxHelper.ShowConfirmationDialog("This will clear all Chat for the stream. Are you sure?"))
            {
                await ChannelSession.Chat.ClearMessages();
                this.MessageControls.Clear();
            }
        }

        private async void DisableChatButton_Click(object sender, RoutedEventArgs e)
        {
            if (await MessageBoxHelper.ShowConfirmationDialog("This will disable chat for all users. Are you sure?"))
            {
                this.disableChat = true;
                this.DisableChatButton.Visibility = Visibility.Collapsed;
                this.EnableChatButton.Visibility = Visibility.Visible;
            }
        }

        private void EnableChatButton_Click(object sender, RoutedEventArgs e)
        {
            this.disableChat = false;
            this.DisableChatButton.Visibility = Visibility.Visible;
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

                Match whisperRegexMatch = WhisperRegex.Match(message);
                if (whisperRegexMatch != null && whisperRegexMatch.Success)
                {
                    message = message.Substring(whisperRegexMatch.Value.Length);

                    Match usernNameMatch = UserNameTagRegex.Match(whisperRegexMatch.Value);
                    string username = usernNameMatch.Value;
                    username = username.Trim();
                    username = username.Replace("@", "");

                    await this.Window.RunAsyncOperation(async () =>
                    {
                        await ChannelSession.Chat.Whisper(username, message);
                    });
                }
                else
                {
                    await this.Window.RunAsyncOperation(async () =>
                    {
                        await ChannelSession.Chat.SendMessage(message);
                    });
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.backgroundThreadCancellationTokenSource.Cancel();
        }

        #region Chat Update Methods

        private async Task RefreshAllChat()
        {
            await ChannelSession.RefreshChannel();

            Dictionary<uint, UserViewModel> refreshUsers = new Dictionary<uint, UserViewModel>();
            foreach (ChatUserModel chatUser in await ChannelSession.Connection.GetChatUsers(ChannelSession.Channel))
            {
                UserViewModel user = new UserViewModel(chatUser);
                if (user.ID > 0)
                {
                    await SetUserDetails(user, checkForFollow: false);
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
                ChannelSession.ChatUsers.Clear();
                foreach (UserViewModel user in refreshUsers.Values)
                {
                    ChannelSession.ChatUsers[user.ID] = user;
                }
            }

            await this.Dispatcher.BeginInvoke(new Action(() =>
            {
                lock (userUpdateLock)
                {
                    this.RefreshUserList();
                }
            }));
        }

        private async Task SetDetailsAndAddUser(UserViewModel user)
        {
            await this.SetUserDetails(user);
            this.AddUser(user);
        }

        private async Task SetUserDetails(UserViewModel user, bool checkForFollow = true)
        {
            if (user.ID > 0)
            {
                UserWithChannelModel userWithChannel = await ChannelSession.Connection.GetUser(user.GetModel());
                if (!string.IsNullOrEmpty(userWithChannel.avatarUrl))
                {
                    user.AvatarLink = userWithChannel.avatarUrl;
                }
            }

            if (checkForFollow)
            {
                DateTimeOffset? followDate = await ChannelSession.Connection.CheckIfFollows(ChannelSession.Channel, user.GetModel());
                if (followDate != null)
                {
                    user.IsFollower = true;
                }
            }
        }

        private void AddUser(UserViewModel user)
        {
            lock (userUpdateLock)
            {
                if (!ChannelSession.ChatUsers.ContainsKey(user.ID))
                {
                    ChannelSession.ChatUsers[user.ID] = user;
                    this.RefreshUserList();
                }
            }
        }

        private void RemoveUser(UserViewModel user)
        {
            lock (userUpdateLock)
            {
                ChatUserControl userControl = this.UserControls.FirstOrDefault(u => u.User.Equals(user));
                if (userControl != null)
                {
                    ChannelSession.ChatUsers.Remove(userControl.User.ID);
                    this.UserControls.Remove(userControl);
                }
                this.UpdateUserViewCount();
            }
        }

        private void RefreshUserList()
        {
            this.UserControls.Clear();
            var orderedUsers = ChannelSession.ChatUsers.Values.OrderByDescending(u => u.PrimarySortableRole).ThenBy(u => u.UserName).ToList();
            foreach (UserViewModel user in orderedUsers)
            {
                this.UserControls.Add(new ChatUserControl(user));
            }
            this.UpdateUserViewCount();
        }

        private void UpdateUserViewCount()
        {
            this.ViewersCountTextBlock.Text = ChannelSession.Channel.viewersCurrent.ToString();
            this.ChatCountTextBlock.Text = ChannelSession.ChatUsers.Count.ToString();
        }

        private async void AddMessage(ChatMessageViewModel message)
        {
            ChatMessageControl messageControl = new ChatMessageControl(message);

            GlobalEvents.MessageReceived(message);

            await this.SetDetailsAndAddUser(message.User);

            this.totalMessages++;
            this.MessageControls.Add(messageControl);
            while (this.MessageControls.Count > ChannelSession.Settings.MaxMessagesInChat)
            {
                this.MessageControls.RemoveAt(0);
            }

            this.ChatList.ScrollIntoView(messageControl);

            if (this.disableChat && !message.ID.Equals(Guid.Empty))
            {
                messageControl.DeleteMessage();
                await ChannelSession.Chat.DeleteMessage(message.ID);
            }
            else
            {
                string moderationReason;
                if (!messageControl.Message.IsWhisper && messageControl.Message.User.PrimaryRole < UserRole.Mod && messageControl.Message.ShouldBeModerated(out moderationReason))
                {
                    await ChannelSession.Chat.DeleteMessage(messageControl.Message.ID);
                    messageControl.DeleteMessage(moderationReason);

                    string whisperMessage = " due to chat moderation for the following reason: " + moderationReason + ". Please watch what you type in chat or further actions will be taken.";

                    messageControl.Message.User.ChatOffenses++;
                    if (ChannelSession.Settings.Timeout5MinuteOffenseCount > 0 && messageControl.Message.User.ChatOffenses >= ChannelSession.Settings.Timeout5MinuteOffenseCount)
                    {
                        await ChannelSession.Chat.Whisper(messageControl.Message.User.UserName, "You have been timed out from chat for 5 minutes" + whisperMessage);
                        await ChannelSession.Chat.TimeoutUser(messageControl.Message.User.UserName, 300);
                    }
                    else if (ChannelSession.Settings.Timeout1MinuteOffenseCount > 0 && messageControl.Message.User.ChatOffenses >= ChannelSession.Settings.Timeout1MinuteOffenseCount)
                    {
                        await ChannelSession.Chat.Whisper(messageControl.Message.User.UserName, "You have been timed out from chat for 1 minute" + whisperMessage);
                        await ChannelSession.Chat.TimeoutUser(messageControl.Message.User.UserName, 60);
                    }
                    else
                    {
                        await ChannelSession.Chat.Whisper(messageControl.Message.User.UserName, "Your message has been deleted" + whisperMessage);
                    }
                }
                else if (this.EnableCommands && ChatMessageCommandViewModel.IsCommand(message) && !message.User.Roles.Contains(UserRole.Banned))
                {
                    ChatMessageCommandViewModel messageCommand = new ChatMessageCommandViewModel(message);

                    GlobalEvents.ChatCommandMessageReceived(messageCommand);

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
                            ChannelSession.BotChat.Whisper(message.User.UserName, "You do not permission to run this command");
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        }
                    }
                }
            }
        }

        private async Task ShowUserDialog(UserViewModel user)
        {
            UserDialogResult result = await MessageBoxHelper.ShowUserDialog(user);

            if (result == UserDialogResult.Purge)
            {
                await ChannelSession.Chat.PurgeUser(user.UserName);
                foreach (ChatMessageControl messageControl in this.MessageControls)
                {
                    if (messageControl.Message.User.Equals(user) && !messageControl.Message.IsWhisper)
                    {
                        messageControl.DeleteMessage();
                    }
                }
            }
            else if (result == UserDialogResult.Timeout1)
            {
                await ChannelSession.Chat.TimeoutUser(user.UserName, 60);
            }
            else if (result == UserDialogResult.Timeout5)
            {
                await ChannelSession.Chat.TimeoutUser(user.UserName, 300);
            }
            else if (result == UserDialogResult.Ban)
            {
                if (await MessageBoxHelper.ShowConfirmationDialog(string.Format("This will ban the user {0} from this channel. Are you sure?", user.UserName)))
                {
                    await ChannelSession.Connection.AddUserRoles(ChannelSession.Channel, user.GetModel(), new List<UserRole>() { UserRole.Banned });
                }
            }
            else if (result == UserDialogResult.Unban)
            {
                await ChannelSession.Connection.RemoveUserRoles(ChannelSession.Channel, user.GetModel(), new List<UserRole>() { UserRole.Banned });
            }
        }

        #endregion Chat Update Methods

        #region Context Menu Events

        private async void UserList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.UserList.SelectedIndex >= 0)
            {
                ChatUserControl userControl = (ChatUserControl)this.UserList.SelectedItem;
                this.UserList.SelectedIndex = -1;
                await this.ShowUserDialog(userControl.User);
            }
        }

        private async void MessageDeleteMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.ChatList.SelectedItem != null)
            {
                ChatMessageControl control = (ChatMessageControl)this.ChatList.SelectedItem;
                if (!control.Message.IsWhisper)
                {
                    await ChannelSession.Chat.DeleteMessage(control.Message.ID);
                    control.DeleteMessage();
                }
            }
        }

        private async void MessageUserInformationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.ChatList.SelectedItem != null)
            {
                ChatMessageControl control = (ChatMessageControl)this.ChatList.SelectedItem;
                await this.ShowUserDialog(control.Message.User);
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
            await this.SetDetailsAndAddUser(user);
        }

        private void ChatClient_OnUserLeaveOccurred(object sender, ChatUserEventModel e)
        {
            UserViewModel user = new UserViewModel(e);
            this.RemoveUser(user);
        }

        private void ChatClient_OnUserTimeoutOccurred(object sender, ChatUserEventModel e)
        {
            UserViewModel user = new UserViewModel(e);
            this.RemoveUser(user);
        }

        private async void ChatClient_OnUserUpdateOccurred(object sender, ChatUserEventModel e)
        {
            UserViewModel user = new UserViewModel(e);
            this.RemoveUser(user);
            await this.SetDetailsAndAddUser(user);
        }

        #endregion Chat Event Handlers
    }
}
