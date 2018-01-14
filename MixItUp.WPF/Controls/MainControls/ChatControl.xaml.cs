using Mixer.Base.Model.Chat;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.MixerAPI;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Chat;
using MixItUp.WPF.Util;
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

                ChannelSession.Chat.OnDeleteMessageOccurred += ChatClient_OnDeleteMessageOccurred;
                ChannelSession.Chat.OnMessageOccurred += ChatClient_OnMessageOccurred;
                ChannelSession.Chat.OnPurgeMessageOccurred += ChatClient_OnPurgeMessageOccurred;
                ChannelSession.Chat.OnUserJoinOccurred += ChatClient_OnUserJoinOccurred;
                ChannelSession.Chat.OnUserLeaveOccurred += ChatClient_OnUserLeaveOccurred;
                ChannelSession.Chat.OnUserTimeoutOccurred += ChatClient_OnUserTimeoutOccurred;
                ChannelSession.Chat.OnUserUpdateOccurred += ChatClient_OnUserUpdateOccurred;

                if (ChannelSession.Channel.badge != null && !string.IsNullOrEmpty(ChannelSession.Channel.badge.url))
                {
                    ChatControl.SubscriberBadgeBitmap = new BitmapImage();
                    ChatControl.SubscriberBadgeBitmap.BeginInit();
                    ChatControl.SubscriberBadgeBitmap.UriSource = new Uri(ChannelSession.Channel.badge.url, UriKind.Absolute);
                    ChatControl.SubscriberBadgeBitmap.EndInit();
                }

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

        protected override Task OnVisibilityChanged()
        {
            if (ChannelSession.Chat.BotClient != null && ChannelSession.Chat.BotClient.Authenticated)
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

        private async Task ChannelRefreshBackground()
        {
            int totalMinutes = 0;
            bool addViewingMinutes = false;

            await BackgroundTaskWrapper.RunBackgroundTask(this.backgroundThreadCancellationTokenSource, async (tokenSource) =>
            {
                tokenSource.Token.ThrowIfCancellationRequested();

                await this.RefreshAllChat();

                tokenSource.Token.ThrowIfCancellationRequested();

                if (addViewingMinutes)
                {
                    List<UserCurrencyViewModel> currenciesToUpdate = new List<UserCurrencyViewModel>();
                    foreach (UserCurrencyViewModel currency in ChannelSession.Settings.Currencies.Values)
                    {
                        if (currency.Enabled && currency.AcquireInterval > 0)
                        {
                            currenciesToUpdate.Add(currency);
                        }
                    }

                    await ChannelSession.Chat.UpdateEachUser((user) =>
                    {
                        user.Data.ViewingMinutes++;
                        foreach (UserCurrencyViewModel currency in currenciesToUpdate)
                        {
                            if ((user.Data.ViewingMinutes % currency.AcquireInterval) == 0)
                            {
                                user.Data.AddCurrencyAmount(currency, currency.AcquireAmount);
                                if (user.Roles.Contains(UserRole.Subscriber))
                                {
                                    user.Data.AddCurrencyAmount(currency, currency.SubscriberBonus);
                                }
                            }
                        }
                        return Task.FromResult(0);
                    });

                    totalMinutes++;
                }
                addViewingMinutes = !addViewingMinutes;

                await ChannelSession.SaveSettings();

                tokenSource.Token.ThrowIfCancellationRequested();

                await Task.Delay(1000 * 30);
            });
        }

        private async Task TimerCommandsBackground()
        {
            int timerCommandIndex = 0;
            await BackgroundTaskWrapper.RunBackgroundTask(this.backgroundThreadCancellationTokenSource, async (tokenSource) =>
            {
                DateTimeOffset startTime = DateTimeOffset.Now;
                int startMessageCount = this.totalMessages;

                tokenSource.Token.ThrowIfCancellationRequested();

                await Task.Delay(1000 * 60 * ChannelSession.Settings.TimerCommandsInterval);

                tokenSource.Token.ThrowIfCancellationRequested();

                if (ChannelSession.Settings.TimerCommands.Count > 0)
                {
                    TimerCommand command = ChannelSession.Settings.TimerCommands[timerCommandIndex];

                    while ((this.totalMessages - startMessageCount) < ChannelSession.Settings.TimerCommandsMinimumMessages)
                    {
                        tokenSource.Token.ThrowIfCancellationRequested();
                        await Task.Delay(1000 * 10);
                    }

                    await command.Perform();

                    timerCommandIndex++;
                    timerCommandIndex = timerCommandIndex % ChannelSession.Settings.TimerCommands.Count;
                }
            });
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.backgroundThreadCancellationTokenSource.Cancel();
        }

        #region Chat Update Methods

        private async Task RefreshAllChat()
        {
            await ChannelSession.RefreshChannel();

            await ChannelSession.Chat.RefreshAllChat();

            await this.RefreshUserList();
        }

        private async Task RefreshUserList()
        {
            await this.Dispatcher.BeginInvoke(new Action(() =>
            {
                lock (userUpdateLock)
                {
                    this.UserControls.Clear();
                    var orderedUsers = ChannelSession.Chat.ChatUsers.Values.ToList().OrderByDescending(u => u.PrimarySortableRole).ThenBy(u => u.UserName).ToList();
                    foreach (UserViewModel user in orderedUsers)
                    {
                        this.UserControls.Add(new ChatUserControl(user));
                    }
                    this.UpdateUserViewCount();
                }
            }));
        }

        private void UpdateUserViewCount()
        {
            this.ViewersCountTextBlock.Text = ChannelSession.Channel.viewersCurrent.ToString();
            this.ChatCountTextBlock.Text = ChannelSession.Chat.ChatUsers.Count.ToString();
        }

        private async void AddMessage(ChatMessageViewModel message)
        {
            ChatMessageControl messageControl = new ChatMessageControl(message);

            GlobalEvents.MessageReceived(message);

            await ChannelSession.Chat.SetDetailsAndAddUser(message.User);

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
                if (messageControl.Message.ShouldBeModerated(out moderationReason))
                {
                    await ChannelSession.Chat.DeleteMessage(messageControl.Message.ID);
                    messageControl.DeleteMessage(moderationReason);

                    string whisperMessage = " due to chat moderation for the following reason: " + moderationReason + ". Please watch what you type in chat or further actions will be taken.";

                    messageControl.Message.User.ChatOffenses++;
                    if (ChannelSession.Settings.ModerationTimeout5MinuteOffenseCount > 0 && messageControl.Message.User.ChatOffenses >= ChannelSession.Settings.ModerationTimeout5MinuteOffenseCount)
                    {
                        await ChannelSession.Chat.Whisper(messageControl.Message.User.UserName, "You have been timed out from chat for 5 minutes" + whisperMessage);
                        await ChannelSession.Chat.TimeoutUser(messageControl.Message.User.UserName, 300);
                    }
                    else if (ChannelSession.Settings.ModerationTimeout1MinuteOffenseCount > 0 && messageControl.Message.User.ChatOffenses >= ChannelSession.Settings.ModerationTimeout1MinuteOffenseCount)
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

                    PermissionsCommandBase command = ChannelSession.AllChatCommands.FirstOrDefault(c => c.ContainsCommand(messageCommand.CommandName));
                    if (command != null)
                    {
                        await command.Perform(message.User, messageCommand.CommandArguments);
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
                }
                control.DeleteMessage();
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
            await this.RefreshUserList();
        }

        private async void ChatClient_OnUserLeaveOccurred(object sender, ChatUserEventModel e)
        {
            await this.RefreshUserList();
        }

        private async void ChatClient_OnUserTimeoutOccurred(object sender, ChatUserEventModel e)
        {
            await this.RefreshUserList();
        }

        private async void ChatClient_OnUserUpdateOccurred(object sender, ChatUserEventModel e)
        {
            await this.RefreshUserList();
        }

        #endregion Chat Event Handlers
    }
}
