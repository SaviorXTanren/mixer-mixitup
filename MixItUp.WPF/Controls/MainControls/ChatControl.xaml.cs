using Mixer.Base.Model.Chat;
using Mixer.Base.Model.Skills;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.MixerAPI;
using MixItUp.Base.Model.Skill;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Chat;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.PopOut;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MixItUp.WPF.Controls.MainControls
{
    public class SortedUserObservableCollection
    {
        public ICollection<ChatUserControl> Collection { get { return collection; } }

        private ObservableCollection<ChatUserControl> collection = new ObservableCollection<ChatUserControl>();
        private Dictionary<uint, ChatUserControl> existingUsers = new Dictionary<uint, ChatUserControl>();

        private SemaphoreSlim collectionChangeSemaphore = new SemaphoreSlim(1);

        public async Task Add(UserViewModel user)
        {
            await this.collectionChangeSemaphore.WaitAndRelease(() =>
            {
                if (!this.existingUsers.ContainsKey(user.ID))
                {
                    int insertIndex = 0;
                    for (insertIndex = 0; insertIndex < this.collection.Count; insertIndex++)
                    {
                        ChatUserControl userControl = this.collection[insertIndex];
                        if (userControl != null)
                        {
                            UserViewModel currentUser = userControl.User;
                            if (currentUser != null)
                            {
                                if (currentUser.PrimarySortableRole == user.PrimarySortableRole)
                                {
                                    if (currentUser.UserName.CompareTo(user.UserName) > 0)
                                    {
                                        break;
                                    }
                                }
                                else if (currentUser.PrimarySortableRole < user.PrimarySortableRole)
                                {
                                    break;
                                }
                            }
                        }
                    }

                    ChatUserControl control = new ChatUserControl(user);
                    this.existingUsers[user.ID] = control;

                    if (insertIndex < this.collection.Count)
                    {
                        this.collection.Insert(insertIndex, control);
                    }
                    else
                    {
                        this.collection.Add(control);
                    }
                }
                return Task.FromResult(0);
            });
        }

        public async Task<bool> Contains(UserViewModel user)
        {
            return await this.collectionChangeSemaphore.WaitAndRelease(() =>
            {
                return Task.FromResult(this.existingUsers.ContainsKey(user.ID));
            });
        }

        public async Task Remove(UserViewModel user)
        {
            await this.collectionChangeSemaphore.WaitAndRelease(() =>
            {
                if (this.existingUsers.ContainsKey(user.ID))
                {
                    this.collection.Remove(this.existingUsers[user.ID]);
                    this.existingUsers.Remove(user.ID);
                }
                return Task.FromResult(0);
            });
        }
    }

    /// <summary>
    /// Interaction logic for ChatControl.xaml
    /// </summary>
    public partial class ChatControl : MainControlBase, IDisposable
    {
        public static BitmapImage SubscriberBadgeBitmap { get; private set; }

        public SortedUserObservableCollection UserControls = new SortedUserObservableCollection();
        public ObservableCollection<ChatMessageControl> MessageControls = new ObservableCollection<ChatMessageControl>();

        private CancellationTokenSource backgroundThreadCancellationTokenSource = new CancellationTokenSource();

        private SemaphoreSlim userUpdateLock = new SemaphoreSlim(1);
        private SemaphoreSlim messageUpdateLock = new SemaphoreSlim(1);

        private int totalMessages = 0;
        private ScrollViewer chatListScrollViewer;
        private bool lockChatList = true;
        private List<string> messageHistory = new List<string>();
        private int activeMessageHistory = 0;
        private int indexOfTag = 0;

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

            GlobalEvents.OnSkillOccurred += GlobalEvents_OnSkillOccurred;
        }

        protected override async Task InitializeInternal()
        {
            GlobalEvents.OnChatFontSizeChanged += GlobalEvents_OnChatFontSizeChanged;

            GlobalEvents.OnFollowOccurred += Constellation_OnFollowOccurred;
            GlobalEvents.OnUnfollowOccurred += Constellation_OnUnfollowOccurred;
            GlobalEvents.OnHostOccurred += Constellation_OnHostedOccurred;
            GlobalEvents.OnSubscribeOccurred += Constellation_OnSubscribedOccurred;
            GlobalEvents.OnResubscribeOccurred += Constellation_OnResubscribedOccurred;

            ChannelSession.Interactive.OnInteractiveControlUsed += Interactive_OnInteractiveControlUsed;

            this.ChatList.ItemsSource = this.MessageControls;
            this.UserList.ItemsSource = this.UserControls.Collection;

            ChannelSession.Chat.OnMessageOccurred += ChatClient_OnMessageOccurred;
            ChannelSession.Chat.OnDeleteMessageOccurred += ChatClient_OnDeleteMessageOccurred;
            ChannelSession.Chat.OnClearMessagesOccurred += Chat_OnClearMessagesOccurred;
            ChannelSession.Chat.OnUserJoinOccurred += ChatClient_OnUserJoinOccurred;
            ChannelSession.Chat.OnUserLeaveOccurred += ChatClient_OnUserLeaveOccurred;
            ChannelSession.Chat.OnUserUpdateOccurred += ChatClient_OnUserUpdateOccurred;
            ChannelSession.Chat.OnUserPurgeOccurred += ChatClient_OnUserPurgeOccurred;

            if (ChannelSession.Channel.badge != null && ChannelSession.Channel.badge != null && !string.IsNullOrEmpty(ChannelSession.Channel.badge.url))
            {
                using (WebClient client = new WebClient())
                {
                    var bytes = await client.DownloadDataTaskAsync(new Uri(ChannelSession.Channel.badge.url, UriKind.Absolute));
                    ChatControl.SubscriberBadgeBitmap = new BitmapImage();
                    ChatControl.SubscriberBadgeBitmap.BeginInit();
                    ChatControl.SubscriberBadgeBitmap.CacheOption = BitmapCacheOption.OnLoad;
                    ChatControl.SubscriberBadgeBitmap.StreamSource = new MemoryStream(bytes);
                    ChatControl.SubscriberBadgeBitmap.EndInit();
                }
            }

            if (!ChannelSession.Settings.HideChatUserList)
            {
                await userUpdateLock.WaitAndRelease(async () =>
                {
                    foreach (UserViewModel user in await ChannelSession.ActiveUsers.GetAllUsers())
                    {
                        if (user.IsInChat)
                        {
                            await this.UserControls.Add(user);
                        }
                    }

                    await this.RefreshViewerChatterCounts();
                });
            }

            IEnumerable<ChatMessageEventModel> oldMessages = await ChannelSession.Chat.GetChatHistory(50);
            foreach (ChatMessageEventModel message in oldMessages)
            {
                await AddMessage(ChatMessageViewModel.CreateChatMessageViewModel(message));
            }
        }

        protected override Task OnVisibilityChanged()
        {
            List<string> chatVoices = new List<string>() { "Streamer" };
            if (ChannelSession.Chat.BotClient != null)
            {
                chatVoices.Add("Bot");
            }

            List<string> chatterList = this.SendChatAsComboBox.ItemsSource as List<string>;
            if (chatterList == null || chatterList.Count != chatVoices.Count)
            {
                this.SendChatAsComboBox.ItemsSource = chatVoices;
                this.SendChatAsComboBox.SelectedIndex = 0;
            }

            this.ViewerChatterNumbersGrid.Visibility = (ChannelSession.Settings.HideViewerAndChatterNumbers) ? Visibility.Collapsed : Visibility.Visible;

            this.ChatSplitter.Visibility = this.UserList.Visibility = (ChannelSession.Settings.HideChatUserList) ? Visibility.Collapsed : Visibility.Visible;

            return Task.FromResult(0);
        }

        #region Chat Update Methods

        private async Task RefreshSpecificUserInList(UserViewModel user)
        {
            if (!ChannelSession.Settings.HideChatUserList)
            {
                await userUpdateLock.WaitAndRelease(async () =>
                {
                    if (user.IsInChat)
                    {
                        await this.UserControls.Remove(user);
                        await this.UserControls.Add(user);
                    }

                    await this.RefreshViewerChatterCounts();
                });
            }
        }

        private async Task RemoveSpecificUserInList(UserViewModel user)
        {
            if (!ChannelSession.Settings.HideChatUserList)
            {
                await userUpdateLock.WaitAndRelease(async () =>
                {
                    await this.UserControls.Remove(user);

                    await this.RefreshViewerChatterCounts();
                });
            }
        }

        private async Task RefreshViewerChatterCounts()
        {
            this.ViewersCountTextBlock.Text = ChannelSession.Channel.viewersCurrent.ToString();
            this.ChatCountTextBlock.Text = (await ChannelSession.ActiveUsers.Count()).ToString();
        }

        private async Task AddMessage(ChatMessageViewModel message)
        {
            await this.messageUpdateLock.WaitAndRelease(() =>
            {
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

                Logger.LogChatEvent(message.ToString());

                return Task.FromResult(0);
            });
        }

        private async Task ShowUserDialog(UserViewModel user)
        {
            if (user != null)
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

        private async void ChatMessageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                HideIntellisense();

                string tag = this.ChatMessageTextBox.Text.Split(' ').LastOrDefault();
                if (!string.IsNullOrEmpty(tag))
                {
                    if (tag.StartsWith("@"))
                    {
                        string filter = tag.Substring(1);

                        List<UserViewModel> users = (await ChannelSession.ActiveUsers.GetAllUsers()).ToList();
                        if (!string.IsNullOrEmpty(filter))
                        {
                            users = users.Where(u => !string.IsNullOrEmpty(u.UserName) && u.UserName.StartsWith(filter, StringComparison.InvariantCultureIgnoreCase)).ToList();
                        }
                        users = users.OrderBy(u => u.UserName).Take(5).Reverse().ToList();

                        if (users.Count > 0)
                        {
                            this.indexOfTag = this.ChatMessageTextBox.Text.LastIndexOf(tag);
                            UsernameIntellisenseListBox.ItemsSource = users;

                            // Select the bottom user
                            UsernameIntellisenseListBox.SelectedIndex = users.Count - 1;

                            Rect positionOfCarat = this.ChatMessageTextBox.GetRectFromCharacterIndex(this.ChatMessageTextBox.CaretIndex, true);
                            Point topLeftOffset = this.ChatMessageTextBox.TransformToAncestor(this).Transform(new Point(positionOfCarat.Left, positionOfCarat.Top));

                            ShowUserIntellisense(topLeftOffset.X, topLeftOffset.Y, users.Count);
                        }
                    }
                    else
                    {
                        List<EmoticonImage> emoticonImages = ShouldSendAsStreamer() ?
                            ChannelSession.FindMatchingEmoticonsForUser(tag).ToList() :
                            ChannelSession.FindMatchingEmoticonsForBot(tag).ToList();
                        if (emoticonImages.Count > 0)
                        {
                            emoticonImages = emoticonImages.Take(5).Reverse().ToList();
                            this.indexOfTag = this.ChatMessageTextBox.Text.LastIndexOf(tag);
                            EmoticonIntellisenseListBox.ItemsSource = emoticonImages;

                            // Select the bottom icon
                            EmoticonIntellisenseListBox.SelectedIndex = emoticonImages.Count - 1;

                            Rect positionOfCarat = this.ChatMessageTextBox.GetRectFromCharacterIndex(this.ChatMessageTextBox.CaretIndex, true);
                            Point topLeftOffset = this.ChatMessageTextBox.TransformToAncestor(this).Transform(new Point(positionOfCarat.Left, positionOfCarat.Top));

                            ShowEmoticonIntellisense(topLeftOffset.X, topLeftOffset.Y, emoticonImages.Count);
                        }
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        private void ShowUserIntellisense(double x, double y, int count)
        {
            int itemHeight = count * 40;
            Canvas.SetLeft(this.UsernameIntellisense, x + 10);
            Canvas.SetTop(this.UsernameIntellisense, y - itemHeight - 40);
            this.UsernameIntellisense.UpdateLayout();

            if (!this.UsernameIntellisense.IsPopupOpen)
            {
                this.UsernameIntellisense.IsPopupOpen = true;
            }
        }

        private void ShowEmoticonIntellisense(double x, double y, int count)
        {
            int itemHeight = count * 40;
            Canvas.SetLeft(this.EmoticonIntellisense, x + 10);
            Canvas.SetTop(this.EmoticonIntellisense, y - itemHeight - 40);
            this.EmoticonIntellisense.UpdateLayout();

            if (!this.EmoticonIntellisense.IsPopupOpen)
            {
                this.EmoticonIntellisense.IsPopupOpen = true;
            }
        }

        private void HideIntellisense()
        {
            if (this.UsernameIntellisense.IsPopupOpen)
            {
                this.UsernameIntellisense.IsPopupOpen = false;
            }

            if (this.EmoticonIntellisense.IsPopupOpen)
            {
                this.EmoticonIntellisense.IsPopupOpen = false;
            }
        }

        private void ChatMessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.SendChatMessageButton_Click(this, new RoutedEventArgs());
            }
        }

        private void ChatMessageTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (this.UsernameIntellisense.IsPopupOpen)
            {
                switch (e.Key)
                {
                    case Key.Escape:
                        HideIntellisense();
                        e.Handled = true;
                        break;
                    case Key.Tab:
                    case Key.Enter:
                        SelectIntellisenseUser();
                        e.Handled = true;
                        break;
                    case Key.Up:
                        UsernameIntellisenseListBox.SelectedIndex = MathHelper.Clamp(UsernameIntellisenseListBox.SelectedIndex - 1, 0, UsernameIntellisenseListBox.Items.Count - 1);
                        e.Handled = true;
                        break;
                    case Key.Down:
                        UsernameIntellisenseListBox.SelectedIndex = MathHelper.Clamp(UsernameIntellisenseListBox.SelectedIndex + 1, 0, UsernameIntellisenseListBox.Items.Count - 1);
                        e.Handled = true;
                        break;
                }
            }
            else if (this.EmoticonIntellisense.IsPopupOpen)
            {
                switch (e.Key)
                {
                    case Key.Escape:
                        HideIntellisense();
                        e.Handled = true;
                        break;
                    case Key.Tab:
                    case Key.Enter:
                        SelectIntellisenseEmoticon();
                        e.Handled = true;
                        break;
                    case Key.Up:
                        EmoticonIntellisenseListBox.SelectedIndex = MathHelper.Clamp(EmoticonIntellisenseListBox.SelectedIndex - 1, 0, EmoticonIntellisenseListBox.Items.Count - 1);
                        e.Handled = true;
                        break;
                    case Key.Down:
                        EmoticonIntellisenseListBox.SelectedIndex = MathHelper.Clamp(EmoticonIntellisenseListBox.SelectedIndex + 1, 0, EmoticonIntellisenseListBox.Items.Count - 1);
                        e.Handled = true;
                        break;
                }
            }
            else
            {
                switch (e.Key)
                {
                    case Key.Up:
                        if (activeMessageHistory > 0)
                        {
                            activeMessageHistory--;
                            this.ChatMessageTextBox.Text = messageHistory[activeMessageHistory];
                        }
                        this.ChatMessageTextBox.CaretIndex = this.ChatMessageTextBox.Text.Length;
                        break;

                    case Key.Down:
                        if (activeMessageHistory < messageHistory.Count)
                        {
                            activeMessageHistory++;
                            if (activeMessageHistory == messageHistory.Count)
                            {
                                this.ChatMessageTextBox.Text = string.Empty;
                            }
                            else
                            {
                                this.ChatMessageTextBox.Text = messageHistory[activeMessageHistory];
                            }
                        }
                        this.ChatMessageTextBox.CaretIndex = this.ChatMessageTextBox.Text.Length;
                        break;
                }
            }
        }

        private void UsernameIntellisenseListBox_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            SelectIntellisenseUser();
        }

        private void EmoticonIntellisenseListBox_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            SelectIntellisenseEmoticon();
        }

        private void SelectIntellisenseUser()
        {
            UserViewModel user = UsernameIntellisenseListBox.SelectedItem as UserViewModel;
            if (user != null)
            {
                if (this.indexOfTag == 0)
                {
                    this.ChatMessageTextBox.Text = "@" + user.UserName + " ";
                }
                else
                {
                    this.ChatMessageTextBox.Text = this.ChatMessageTextBox.Text.Substring(0, this.indexOfTag) + "@" + user.UserName + " ";
                }

                this.ChatMessageTextBox.CaretIndex = this.ChatMessageTextBox.Text.Length;
            }
            HideIntellisense();
        }

        private void SelectIntellisenseEmoticon()
        {
            EmoticonImage emoticon = EmoticonIntellisenseListBox.SelectedItem as EmoticonImage;
            if (emoticon != null)
            {
                if (this.indexOfTag == 0)
                {
                    this.ChatMessageTextBox.Text = emoticon.Name + " ";
                }
                else
                {
                    this.ChatMessageTextBox.Text = this.ChatMessageTextBox.Text.Substring(0, this.indexOfTag) + emoticon.Name + " ";
                }

                this.ChatMessageTextBox.CaretIndex = this.ChatMessageTextBox.Text.Length;
            }
            HideIntellisense();
        }

        private async void SendChatMessageButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.ChatMessageTextBox.Text))
            {
                string message = this.ChatMessageTextBox.Text;

                if (messageHistory.Contains(message))
                {
                    // Remove so we can move to the end
                    messageHistory.Remove(message);
                }

                messageHistory.Add(message);
                activeMessageHistory = messageHistory.Count;

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
                        ChatMessageEventModel response = await ChannelSession.Chat.WhisperWithResponse(username, message, ShouldSendAsStreamer());
                        if (response != null)
                        {
                            await this.AddMessage(ChatMessageViewModel.CreateChatMessageViewModel(response));
                        }
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
                        await ChannelSession.Chat.SendMessage(message, ShouldSendAsStreamer());
                    }));
                }

                this.ChatMessageTextBox.Focus();
            }
        }

        private bool ShouldSendAsStreamer()
        {
            return this.SendChatAsComboBox.SelectedIndex == 0;
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
                    await ChannelSession.Chat.DeleteMessage(control.Message);
                }
            }
        }

        private void MessageWhisperUserMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.ChatList.SelectedItem != null)
            {
                ChatMessageControl control = (ChatMessageControl)this.ChatList.SelectedItem;
                if (control.Message.User != null)
                {
                    this.ChatMessageTextBox.Text = $"/w @{control.Message.User.UserName} ";
                    this.ChatMessageTextBox.Focus();
                    this.ChatMessageTextBox.CaretIndex = this.ChatMessageTextBox.Text.Length;
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

        private async void ChatClient_OnDeleteMessageOccurred(object sender, ChatDeleteMessageEventModel deleteEvent)
        {
            await this.Dispatcher.InvokeAsync<Task>(async () =>
            {
                await this.messageUpdateLock.WaitAndRelease(() =>
                {
                    ChatMessageControl message = this.MessageControls.FirstOrDefault(msg => msg.Message.ID.Equals(deleteEvent.id));
                    if (message != null)
                    {
                        message.DeleteMessage(deleteEvent.moderator?.user_name);
                        if (ChannelSession.Settings.HideDeletedMessages)
                        {
                            this.MessageControls.Remove(message);
                        }
                    }
                    return Task.FromResult(0);
                });
            });
        }

        private async void ChatClient_OnUserPurgeOccurred(object sender, Tuple<UserViewModel, string> purgeEvent)
        {
            await this.Dispatcher.InvokeAsync<Task>(async () =>
            {
                await this.messageUpdateLock.WaitAndRelease(() =>
                {
                    List<ChatMessageControl> messagesToDelete = new List<ChatMessageControl>();

                    IEnumerable<ChatMessageControl> userMessages = this.MessageControls.Where(msg => msg.Message.User != null && msg.Message.User.ID.Equals(purgeEvent.Item1.ID));
                    if (userMessages != null)
                    {
                        foreach (ChatMessageControl message in userMessages)
                        {
                            message.DeleteMessage(purgeEvent.Item2);
                            messagesToDelete.Add(message);
                        }
                    }

                    if (ChannelSession.Settings.HideDeletedMessages)
                    {
                        foreach (ChatMessageControl messageToDelete in messagesToDelete)
                        {
                            this.MessageControls.Remove(messageToDelete);
                        }
                    }
                    return Task.FromResult(0);
                });
            });
        }

        private async void Chat_OnClearMessagesOccurred(object sender, EventArgs e)
        {
            await this.Dispatcher.InvokeAsync<Task>(async () =>
            {
                await this.messageUpdateLock.WaitAndRelease(() =>
                {
                    this.MessageControls.Clear();
                    return Task.FromResult(0);
                });
            });
        }

        private async void ChatClient_OnUserJoinOccurred(object sender, UserViewModel user)
        {
            await this.Dispatcher.InvokeAsync<Task>(async () =>
            {
                await this.RefreshSpecificUserInList(user);
            });

            if (ChannelSession.Settings.ChatShowUserJoinLeave)
            {
                await this.AddAlertMessage(string.Format("{0} Joined Chat", user.UserName), user, ChannelSession.Settings.ChatUserJoinLeaveColorScheme);
            }
        }

        private async void ChatClient_OnUserUpdateOccurred(object sender, UserViewModel user)
        {
            await this.Dispatcher.InvokeAsync<Task>(async () =>
            {
                await this.RefreshSpecificUserInList(user);
            });
        }

        private async void ChatClient_OnUserLeaveOccurred(object sender, UserViewModel user)
        {
            await this.Dispatcher.InvokeAsync<Task>(async () =>
            {
                await this.RemoveSpecificUserInList(user);
            });

            if (ChannelSession.Settings.ChatShowUserJoinLeave)
            {
                await this.AddAlertMessage(string.Format("{0} Left Chat", user.UserName), user, ChannelSession.Settings.ChatUserJoinLeaveColorScheme);
            }
        }

        private async void Constellation_OnFollowOccurred(object sender, UserViewModel user)
        {
            if (ChannelSession.Settings.ChatShowEventAlerts)
            {
                await this.AddAlertMessage(string.Format("{0} Followed", user.UserName), user, ChannelSession.Settings.ChatEventAlertsColorScheme);
            }
        }

        private async void Constellation_OnUnfollowOccurred(object sender, UserViewModel user)
        {
            if (ChannelSession.Settings.ChatShowEventAlerts)
            {
                await this.AddAlertMessage(string.Format("{0} Unfollowed", user.UserName), user, ChannelSession.Settings.ChatEventAlertsColorScheme);
            }
        }

        private async void Constellation_OnHostedOccurred(object sender, Tuple<UserViewModel, int> e)
        {
            if (ChannelSession.Settings.ChatShowEventAlerts)
            {
                await this.AddAlertMessage(string.Format("{0} Hosted With {1} Viewers", e.Item1.UserName, e.Item2), e.Item1, ChannelSession.Settings.ChatEventAlertsColorScheme);
            }
        }

        private async void Constellation_OnSubscribedOccurred(object sender, UserViewModel user)
        {
            if (ChannelSession.Settings.ChatShowEventAlerts)
            {
                await this.AddAlertMessage(string.Format("{0} Subscribed", user.UserName), user, ChannelSession.Settings.ChatEventAlertsColorScheme);
            }
        }

        private async void Constellation_OnResubscribedOccurred(object sender, Tuple<UserViewModel, int> e)
        {
            if (ChannelSession.Settings.ChatShowEventAlerts)
            {
                await this.AddAlertMessage(string.Format("{0} Re-Subscribed For {1} Months", e.Item1.UserName, e.Item2), e.Item1, ChannelSession.Settings.ChatEventAlertsColorScheme);
            }
        }

        private async void Interactive_OnInteractiveControlUsed(object sender, InteractiveInputEvent e)
        {
            if (ChannelSession.Settings.ChatShowInteractiveAlerts && e.User != null && e.Command != null)
            {
                await this.AddAlertMessage(string.Format("{0} Used The \"{1}\" Interactive Control", e.User.UserName, e.Command.Name), e.User, ChannelSession.Settings.ChatInteractiveAlertsColorScheme);
            }
        }

        private async Task AddAlertMessage(string message, UserViewModel user = null, string colorScheme = null)
        {
            await this.Dispatcher.InvokeAsync<Task>(async () =>
            {
                await this.AddMessage(new ChatMessageViewModel(message, user, colorScheme));
            });

            if (ChannelSession.Settings.WhisperAllAlerts)
            {
                await ChannelSession.Chat.Whisper(ChannelSession.User.username, message);
            }
        }

        private async void GlobalEvents_OnSkillOccurred(object sender, Tuple<UserViewModel, SkillInstanceModel> skill)
        {
            await this.Dispatcher.InvokeAsync<Task>(async () =>
            {
                await this.AddMessage(new ChatMessageViewModel(skill.Item2, skill.Item1));
            });
        }

        #endregion Chat Event Handlers

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    this.backgroundThreadCancellationTokenSource.Dispose();
                    this.userUpdateLock.Dispose();
                    this.messageUpdateLock.Dispose();
                }

                // Free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
