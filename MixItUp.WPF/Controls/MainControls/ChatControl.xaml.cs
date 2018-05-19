using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.MixerAPI;
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
using System.Windows.Controls;
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
        private ScrollViewer chatListScrollViewer;
        private bool lockChatList = true;

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
            GlobalEvents.OnChatFontSizeChanged += GlobalEvents_OnChatFontSizeChanged;

            ChannelSession.Constellation.OnFollowOccurred += Constellation_OnFollowOccurred;
            ChannelSession.Constellation.OnUnfollowOccurred += Constellation_OnUnfollowOccurred;
            ChannelSession.Constellation.OnHostedOccurred += Constellation_OnHostedOccurred;
            ChannelSession.Constellation.OnSubscribedOccurred += Constellation_OnSubscribedOccurred;
            ChannelSession.Constellation.OnResubscribedOccurred += Constellation_OnResubscribedOccurred;

            ChannelSession.Interactive.OnInteractiveControlUsed += Interactive_OnInteractiveControlUsed;

            this.ChatList.ItemsSource = this.MessageControls;
            this.UserList.ItemsSource = this.UserControls;

            ChannelSession.Chat.OnMessageOccurred += ChatClient_OnMessageOccurred;
            ChannelSession.Chat.OnDeleteMessageOccurred += ChatClient_OnDeleteMessageOccurred;
            ChannelSession.Chat.OnPurgeMessageOccurred += ChatClient_OnPurgeMessageOccurred;
            ChannelSession.Chat.OnClearMessagesOccurred += Chat_OnClearMessagesOccurred;
            ChannelSession.Chat.OnUserJoinOccurred += ChatClient_OnUserJoinOccurred;
            ChannelSession.Chat.OnUserLeaveOccurred += ChatClient_OnUserLeaveOccurred;
            ChannelSession.Chat.OnUserUpdateOccurred += ChatClient_OnUserUpdateOccurred;

            if (ChannelSession.Channel.badge != null && ChannelSession.Channel.badge != null && !string.IsNullOrEmpty(ChannelSession.Channel.badge.url))
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

            List<UserViewModel> users = (await ChannelSession.ChannelUsers.GetAllUsers()).ToList();
            users = users.OrderByDescending(u => u.PrimarySortableRole).ThenBy(u => u.UserName).ToList();
            foreach (UserViewModel user in users)
            {
                this.UserControls.Add(new ChatUserControl(user));
            }

            this.ViewersCountTextBlock.Text = ChannelSession.Channel.viewersCurrent.ToString();
            this.ChatCountTextBlock.Text = (await ChannelSession.ChannelUsers.Count()).ToString();

            userUpdateLock.Release();
        }

        private async Task AddMessage(ChatMessageViewModel message)
        {
            await messageUpdateLock.WaitAsync();

            ChatMessageControl messageControl = new ChatMessageControl(message);
            if (ChannelSession.Settings.LatestChatAtTop)
            {
                this.MessageControls.Insert(0, messageControl);
            }
            else
            {
                this.MessageControls.Add(messageControl);
            }
            this.totalMessages++;

            while (this.MessageControls.Count > ChannelSession.Settings.MaxMessagesInChat)
            {
                if (ChannelSession.Settings.LatestChatAtTop)
                {
                    this.MessageControls.RemoveAt(this.MessageControls.Count - 1);
                }
                else
                {
                    this.MessageControls.RemoveAt(0);
                }
            }

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
                    await ChannelSession.Chat.BanUser(user);
                }
            }
            else if (result == UserDialogResult.Unban)
            {
                await ChannelSession.Chat.UnBanUser(user);
            }
        }

        #endregion Chat Update Methods

        #region UI Events

        private void PopOutChatButton_Click(object sender, RoutedEventArgs e)
        {
            PopOutWindow window = new PopOutWindow("Chat", new ChatControl(isPopOut: true));
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

        private void GlobalEvents_OnChatFontSizeChanged(object sender, EventArgs e)
        {
            foreach (ChatMessageControl control in this.MessageControls)
            {
                control.UpdateSizing();
            }
        }

        private void ChatList_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (this.chatListScrollViewer == null)
            {
                this.chatListScrollViewer = (ScrollViewer)e.OriginalSource;
            }

            if (this.lockChatList && this.chatListScrollViewer != null)
            {
                if (ChannelSession.Settings.LatestChatAtTop)
                {
                    this.chatListScrollViewer.ScrollToTop();
                }
                else
                {
                    this.chatListScrollViewer.ScrollToBottom();
                }
            }
        }

        private void ChatLockButton_MouseEnter(object sender, MouseEventArgs e)
        {
            this.ChatLockButton.Opacity = 1;
        }

        private void ChatLockButton_MouseLeave(object sender, MouseEventArgs e)
        {
            this.ChatLockButton.Opacity = 0.3;
        }

        private void ChatLockButton_Click(object sender, RoutedEventArgs e)
        {
            this.lockChatList = !this.lockChatList;
            this.chatListScrollViewer.VerticalScrollBarVisibility = (this.lockChatList) ? ScrollBarVisibility.Hidden : ScrollBarVisibility.Visible;
            this.ChatLockButtonIcon.Kind = (this.lockChatList) ? MaterialDesignThemes.Wpf.PackIconKind.LockOutline : MaterialDesignThemes.Wpf.PackIconKind.LockOpenOutline;
            if (this.lockChatList)
            {
                if (ChannelSession.Settings.LatestChatAtTop)
                {
                    this.chatListScrollViewer.ScrollToTop();
                }
                else
                {
                    this.chatListScrollViewer.ScrollToBottom();
                }
            }
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

                if (ChatAction.WhisperRegex.IsMatch(message))
                {
                    Match whisperRegexMatch = ChatAction.WhisperRegex.Match(message);

                    message = message.Substring(whisperRegexMatch.Value.Length);

                    Match usernNameMatch = ChatAction.UserNameTagRegex.Match(whisperRegexMatch.Value);
                    string username = usernNameMatch.Value;
                    username = username.Trim();
                    username = username.Replace("@", "");

                    await this.Window.RunAsyncOperation((Func<Task>)(async () =>
                    {
                        await ChannelSession.Chat.Whisper(username, message, (this.SendChatAsComboBox.SelectedIndex == 0));
                    }));
                }
                else if (ChatAction.ClearRegex.IsMatch(message))
                {
                    await ChannelSession.Chat.ClearMessages();
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
                if (control.Message.User != null)
                {
                    await this.ShowUserDialog(control.Message.User);
                }
            }
        }

        #endregion UI Events

        #region Chat Event Handlers

        private async void ChatClient_OnMessageOccurred(object sender, ChatMessageViewModel message)
        {
            await this.Dispatcher.InvokeAsync<Task>(async () =>
            {
                await this.AddMessage(message);
            });
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
            this.Dispatcher.Invoke(() => this.MessageControls.Clear());
        }

        private async void ChatClient_OnUserJoinOccurred(object sender, UserViewModel user)
        {
            await this.Dispatcher.InvokeAsync<Task>(async () =>
            {
                await this.RefreshUserList();
                if (ChannelSession.Settings.ChatShowUserJoinLeave)
                {
                    await this.AddMessage(new ChatMessageViewModel(string.Format("---  {0} Joined Chat  ---", user.UserName), user, ChannelSession.Settings.ChatUserJoinLeaveColorScheme));
                }
            });
        }

        private async void ChatClient_OnUserUpdateOccurred(object sender, UserViewModel user)
        {
            await this.Dispatcher.InvokeAsync<Task>(async () =>
            {
                await this.RefreshUserList();
            });
        }

        private async void ChatClient_OnUserLeaveOccurred(object sender, UserViewModel user)
        {
            await this.Dispatcher.InvokeAsync<Task>(async () =>
            {
                await this.RefreshUserList();
                if (ChannelSession.Settings.ChatShowUserJoinLeave)
                {
                    await this.AddMessage(new ChatMessageViewModel(string.Format("---  {0} Left Chat  ---", user.UserName), user, ChannelSession.Settings.ChatUserJoinLeaveColorScheme));
                }
            });
        }

        private async void Constellation_OnFollowOccurred(object sender, UserViewModel user)
        {
            if (ChannelSession.Settings.ChatShowEventAlerts)
            {
                await this.Dispatcher.InvokeAsync<Task>(async () =>
                {
                    await this.AddMessage(new ChatMessageViewModel(string.Format("---  {0} Followed  ---", user.UserName), user, ChannelSession.Settings.ChatEventAlertsColorScheme));
                });
            }
        }

        private async void Constellation_OnUnfollowOccurred(object sender, UserViewModel user)
        {
            if (ChannelSession.Settings.ChatShowEventAlerts)
            {
                await this.Dispatcher.InvokeAsync<Task>(async () =>
                {
                    await this.AddMessage(new ChatMessageViewModel(string.Format("---  {0} Unfollowed  ---", user.UserName), user, ChannelSession.Settings.ChatEventAlertsColorScheme));
                });
            }
        }

        private async void Constellation_OnHostedOccurred(object sender, Tuple<UserViewModel, int> e)
        {
            if (ChannelSession.Settings.ChatShowEventAlerts)
            {
                await this.Dispatcher.InvokeAsync<Task>(async () =>
                {
                    await this.AddMessage(new ChatMessageViewModel(string.Format("---  {0} Hosted With {1} Viewers  ---", e.Item1.UserName, e.Item2), e.Item1, ChannelSession.Settings.ChatEventAlertsColorScheme));
                });
            }
        }

        private async void Constellation_OnSubscribedOccurred(object sender, UserViewModel user)
        {
            if (ChannelSession.Settings.ChatShowEventAlerts)
            {
                await this.Dispatcher.InvokeAsync<Task>(async () =>
                {
                    await this.AddMessage(new ChatMessageViewModel(string.Format("---  {0} Subscribed  ---", user.UserName), user, ChannelSession.Settings.ChatEventAlertsColorScheme));
                });
            }
        }

        private async void Constellation_OnResubscribedOccurred(object sender, Tuple<UserViewModel, int> e)
        {
            if (ChannelSession.Settings.ChatShowEventAlerts)
            {
                await this.Dispatcher.InvokeAsync<Task>(async () =>
                {
                    await this.AddMessage(new ChatMessageViewModel(string.Format("---  {0} Re-Subscribed For {1} Months  ---", e.Item1.UserName, e.Item2), e.Item1, ChannelSession.Settings.ChatEventAlertsColorScheme));
                });
            }
        }

        private async void Interactive_OnInteractiveControlUsed(object sender, Tuple<UserViewModel, InteractiveConnectedControlCommand> e)
        {
            if (ChannelSession.Settings.ChatShowInteractiveAlerts)
            {
                await this.Dispatcher.InvokeAsync<Task>(async () =>
                {
                    await this.AddMessage(new ChatMessageViewModel(string.Format("---  {0} Used The \"{1}\" Interactive Control  ---", e.Item1.UserName, e.Item2.Name), e.Item1, ChannelSession.Settings.ChatInteractiveAlertsColorScheme));
                });
            }
        }

        #endregion Chat Event Handlers
    }
}
