using Mixer.Base.Model.Chat;
using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Chat;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.PopOut;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for ChatControl.xaml
    /// </summary>
    public partial class ChatControl : MainControlBase
    {
        public static BitmapImage SubscriberBadgeBitmap { get; private set; }

        public ObservableCollection<ChatUserControl> UserControls = new ObservableCollection<ChatUserControl>();
        public ObservableCollection<ChatMessageControl> MessageControls = new ObservableCollection<ChatMessageControl>();

        private CancellationTokenSource backgroundThreadCancellationTokenSource = new CancellationTokenSource();

        private SemaphoreSlim userUpdateLock = new SemaphoreSlim(1);
        private SemaphoreSlim messageUpdateLock = new SemaphoreSlim(1);

        private int totalMessages = 0;

        public ChatControl(bool isPopOut = false)
        {
            InitializeComponent();

            if (isPopOut)
            {
                this.PopOutChatButton.Visibility = Visibility.Collapsed;
            }

            if (!ChannelSession.Settings.IsStreamer)
            {
                this.DisableChatButton.Visibility = Visibility.Collapsed;
            }
        }

        protected override Task InitializeInternal()
        {
            this.Window.Closing += Window_Closing;

            this.ChatList.ItemsSource = this.MessageControls;
            this.UserList.ItemsSource = this.UserControls;

            ChannelSession.Chat.OnMessageOccurred += ChatClient_OnMessageOccurred;
            ChannelSession.Chat.OnDeleteMessageOccurred += ChatClient_OnDeleteMessageOccurred;
            ChannelSession.Chat.OnPurgeMessageOccurred += ChatClient_OnPurgeMessageOccurred;
            ChannelSession.Chat.OnClearMessagesOccurred += Chat_OnClearMessagesOccurred;
            ChannelSession.Chat.OnUserJoinOccurred += ChatClient_OnUserJoinOccurred;
            ChannelSession.Chat.OnUserLeaveOccurred += ChatClient_OnUserLeaveOccurred;
            ChannelSession.Chat.OnUserUpdateOccurred += ChatClient_OnUserUpdateOccurred;

            if (ChannelSession.Channel.badge != null && !string.IsNullOrEmpty(ChannelSession.Channel.badge.url))
            {
                ChatControl.SubscriberBadgeBitmap = new BitmapImage();
                ChatControl.SubscriberBadgeBitmap.BeginInit();
                ChatControl.SubscriberBadgeBitmap.UriSource = new Uri(ChannelSession.Channel.badge.url, UriKind.Absolute);
                ChatControl.SubscriberBadgeBitmap.EndInit();
            }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () => { await this.ChatRefreshBackground(); }, this.backgroundThreadCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return Task.FromResult(0);
        }

        protected override Task OnVisibilityChanged()
        {
            if (ChannelSession.Chat.BotClient != null)
            {
                this.SendChatAsComboBox.ItemsSource = new List<string>() { "Streamer", "Bot" };
                this.SendChatAsComboBox.SelectedIndex = 1;
            }
            else
            {
                this.SendChatAsComboBox.ItemsSource = new List<string>() { "Streamer" };
                this.SendChatAsComboBox.SelectedIndex = 0;
            }

            if (this.MessageControls.Count > 0)
            {
                this.ChatList.ScrollIntoView(this.MessageControls.Last());
            }

            return Task.FromResult(0);
        }

        private async Task ChatRefreshBackground()
        {
            await BackgroundTaskWrapper.RunBackgroundTask(this.backgroundThreadCancellationTokenSource, async (tokenSource) =>
            {
                tokenSource.Token.ThrowIfCancellationRequested();

                await this.Dispatcher.InvokeAsync<Task>(async () =>
                {
                    await this.RefreshUserList();

                    this.ViewersCountTextBlock.Text = ChannelSession.Channel.viewersCurrent.ToString();
                    this.ChatCountTextBlock.Text = ChannelSession.Chat.ChatUsers.Count.ToString();
                });

                tokenSource.Token.ThrowIfCancellationRequested();

                await Task.Delay(1000 * 30);
            });
        }

        #region Chat Update Methods

        private async Task RefreshUserList()
        {
            await userUpdateLock.WaitAsync();

            this.UserControls.Clear();
            var orderedUsers = ChannelSession.Chat.ChatUsers.Values.ToList().OrderByDescending(u => u.PrimarySortableRole).ThenBy(u => u.UserName).ToList();
            foreach (UserViewModel user in orderedUsers)
            {
                this.UserControls.Add(new ChatUserControl(user));
            }

            userUpdateLock.Release();
        }

        private async Task AddMessage(ChatMessageViewModel message)
        {
            await messageUpdateLock.WaitAsync();

            ChatMessageControl messageControl = new ChatMessageControl(message);

            this.totalMessages++;
            this.MessageControls.Add(messageControl);
            while (this.MessageControls.Count > ChannelSession.Settings.MaxMessagesInChat)
            {
                this.MessageControls.RemoveAt(0);
            }

            this.ChatList.ScrollIntoView(messageControl);

            messageUpdateLock.Release();
        }

        private async Task ShowUserDialog(UserViewModel user)
        {
            UserDialogResult result = await MessageBoxHelper.ShowUserDialog(user);

            if (result == UserDialogResult.Purge)
            {
                await ChannelSession.Chat.PurgeUser(user.UserName);
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

        #region UI Events

        private void PopOutChatButton_Click(object sender, RoutedEventArgs e)
        {
            PopOutWindow window = new PopOutWindow(new ChatControl(isPopOut: true));
            window.Show();
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
                ChannelSession.Chat.DisableChat = true;
                this.DisableChatButton.Visibility = Visibility.Collapsed;
                this.EnableChatButton.Visibility = Visibility.Visible;
            }
        }

        private void EnableChatButton_Click(object sender, RoutedEventArgs e)
        {
            ChannelSession.Chat.DisableChat = false;
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

                Match whisperRegexMatch = ChatMessageViewModel.WhisperRegex.Match(message);
                if (whisperRegexMatch != null && whisperRegexMatch.Success)
                {
                    message = message.Substring(whisperRegexMatch.Value.Length);

                    Match usernNameMatch = ChatMessageViewModel.UserNameTagRegex.Match(whisperRegexMatch.Value);
                    string username = usernNameMatch.Value;
                    username = username.Trim();
                    username = username.Replace("@", "");

                    await this.Window.RunAsyncOperation((Func<Task>)(async () =>
                    {
                        await ChannelSession.Chat.Whisper(username, message, (this.SendChatAsComboBox.SelectedIndex == 0));
                    }));
                }
                else
                {
                    await this.Window.RunAsyncOperation((Func<Task>)(async () =>
                    {
                        await ChannelSession.Chat.SendMessage(message, (this.SendChatAsComboBox.SelectedIndex == 0));
                    }));
                }
            }
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            await ChannelSession.Chat.Disconnect();
        }

        private async void UserList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.UserList.SelectedIndex >= 0)
            {
                ChatUserControl userControl = (ChatUserControl)this.UserList.SelectedItem;
                this.UserList.SelectedIndex = -1;
                await this.ShowUserDialog(userControl.User);
            }
        }

        private void MessageCopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.ChatList.SelectedItem != null)
            {
                ChatMessageControl control = (ChatMessageControl)this.ChatList.SelectedItem;
                Clipboard.SetText(control.Message.Message);
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

        #endregion UI Events

        #region Chat Event Handlers

        private async void ChatClient_OnMessageOccurred(object sender, ChatMessageViewModel message)
        {
            await this.AddMessage(message);
        }

        private void ChatClient_OnDeleteMessageOccurred(object sender, Guid messageID)
        {
            ChatMessageControl message = this.MessageControls.FirstOrDefault(msg => msg.Message.ID.Equals(messageID));
            if (message != null)
            {
                message.DeleteMessage();
            }
        }

        private void ChatClient_OnPurgeMessageOccurred(object sender, uint userID)
        {
            IEnumerable<ChatMessageControl> userMessages = this.MessageControls.Where(msg => msg.Message.User != null && msg.Message.User.ID.Equals(userID));
            if (userMessages != null)
            {
                foreach (ChatMessageControl message in userMessages)
                {
                    message.DeleteMessage();
                }
            }
        }

        private void Chat_OnClearMessagesOccurred(object sender, EventArgs e)
        {
            this.MessageControls.Clear();
        }

        private async void ChatClient_OnUserJoinOccurred(object sender, UserViewModel user)
        {
            await this.RefreshUserList();
        }

        private async void ChatClient_OnUserUpdateOccurred(object sender, UserViewModel user)
        {
            await this.RefreshUserList();
        }

        private async void ChatClient_OnUserLeaveOccurred(object sender, UserViewModel user)
        {
            await this.RefreshUserList();
        }

        #endregion Chat Event Handlers
    }
}
