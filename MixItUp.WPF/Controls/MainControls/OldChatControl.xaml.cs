using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Chat;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.MixerAPI;
using MixItUp.Base.Model.Chat;
using MixItUp.Base.Model.Skill;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Chat;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.PopOut;
using MixItUp.WPF.Windows.Users;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MixItUp.WPF.Controls.MainControls
{
    public class SortedUserObservableCollection
    {
        public ICollection<ChatUserControl> Collection { get { return collection; } }

        private ObservableCollection<ChatUserControl> collection = new ObservableCollection<ChatUserControl>();
        private Dictionary<uint, ChatUserControl> existingUsers = new Dictionary<uint, ChatUserControl>();
        private Dictionary<uint, MixerRoleEnum> cachedUserRoles = new Dictionary<uint, MixerRoleEnum>();

        private SemaphoreSlim collectionChangeSemaphore = new SemaphoreSlim(1);

        public async Task Add(UserViewModel user)
        {
            if (user != null)
            {
                await this.collectionChangeSemaphore.WaitAndRelease(() =>
                {
                    if (user != null && !this.existingUsers.ContainsKey(user.ID))
                    {
                        this.cachedUserRoles[user.ID] = user.PrimaryRole;

                        int insertIndex = 0;
                        for (insertIndex = 0; insertIndex < this.collection.Count; insertIndex++)
                        {
                            ChatUserControl userControl = this.collection[insertIndex];
                            if (userControl != null)
                            {
                                UserViewModel currentUser = userControl.User;
                                if (currentUser != null)
                                {
                                    if (!this.cachedUserRoles.ContainsKey(currentUser.ID))
                                    {
                                        this.cachedUserRoles[currentUser.ID] = currentUser.PrimaryRole;
                                    }

                                    if (this.cachedUserRoles[currentUser.ID] == this.cachedUserRoles[user.ID])
                                    {
                                        if (!string.IsNullOrEmpty(user.UserName) && currentUser.UserName.CompareTo(user.UserName) > 0)
                                        {
                                            break;
                                        }
                                    }
                                    else if (this.cachedUserRoles[currentUser.ID] < this.cachedUserRoles[user.ID])
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
        }

        public async Task<bool> Contains(UserViewModel user)
        {
            return await this.collectionChangeSemaphore.WaitAndRelease(() =>
            {
                return Task.FromResult(this.existingUsers.ContainsKey(user.ID));
            });
        }

        public async Task Remove(UserViewModel user) { await this.Remove(new List<UserViewModel>() { user }); }

        public async Task Remove(IEnumerable<UserViewModel> users)
        {
            await this.collectionChangeSemaphore.WaitAndRelease(() =>
            {
                foreach (UserViewModel user in users)
                {
                    if (this.existingUsers.ContainsKey(user.ID))
                    {
                        this.collection.Remove(this.existingUsers[user.ID]);
                        this.existingUsers.Remove(user.ID);
                    }
                }
                return Task.FromResult(0);
            });
        }
    }

    /// <summary>
    /// Interaction logic for ChatControl.xaml
    /// </summary>
    public partial class OldChatControl : MainControlBase, IDisposable
    {
        public static BitmapImage SubscriberBadgeBitmap { get; private set; }

        public SortedUserObservableCollection UserControls = new SortedUserObservableCollection();
        public ObservableCollection<ChatMessageControl> MessageControls = new ObservableCollection<ChatMessageControl>();

        private SemaphoreSlim userUpdateLock = new SemaphoreSlim(1);
        private SemaphoreSlim messageUpdateLock = new SemaphoreSlim(1);
        private SemaphoreSlim gifSkillPopoutLock = new SemaphoreSlim(1);

        private bool isPopOut;
        private int totalMessages = 0;
        private ScrollViewer chatListScrollViewer;
        private bool lockChatList = true;
        private List<string> messageHistory = new List<string>();
        private int activeMessageHistory = 0;
        private int indexOfTag = 0;

        public OldChatControl(bool isPopOut = false)
        {
            InitializeComponent();

            this.isPopOut = isPopOut;
            if (this.isPopOut)
            {
                this.PopOutChatButton.Visibility = Visibility.Collapsed;
            }

            if (!ChannelSession.Settings.IsStreamer)
            {
                this.DisableChatButton.Visibility = Visibility.Collapsed;
            }

            GlobalEvents.OnSkillUseOccurred += GlobalEvents_OnSkillUseOccurred;
        }

        protected override async Task InitializeInternal()
        {
            GlobalEvents.OnChatFontSizeChanged += GlobalEvents_OnChatFontSizeChanged;

            GlobalEvents.OnFollowOccurred += Constellation_OnFollowOccurred;
            GlobalEvents.OnUnfollowOccurred += Constellation_OnUnfollowOccurred;
            GlobalEvents.OnHostOccurred += Constellation_OnHostedOccurred;
            GlobalEvents.OnSubscribeOccurred += Constellation_OnSubscribedOccurred;
            GlobalEvents.OnResubscribeOccurred += Constellation_OnResubscribedOccurred;
            GlobalEvents.OnSubscriptionGiftedOccurred += GlobalEvents_OnSubscriptionGiftedOccurred;

            ChannelSession.Interactive.OnInteractiveControlUsed += Interactive_OnInteractiveControlUsed;

            this.ChatList.ItemsSource = this.MessageControls;
            this.UserList.ItemsSource = this.UserControls.Collection;

            ChannelSession.Chat.OnMessageOccurred += ChatClient_OnMessageOccurred;
            ChannelSession.Chat.OnDeleteMessageOccurred += ChatClient_OnDeleteMessageOccurred;
            ChannelSession.Chat.OnClearMessagesOccurred += Chat_OnClearMessagesOccurred;
            ChannelSession.Chat.OnUsersJoinOccurred += ChatClient_OnUsersJoinOccurred;
            ChannelSession.Chat.OnUsersLeaveOccurred += ChatClient_OnUsersLeaveOccurred;
            ChannelSession.Chat.OnUserUpdateOccurred += ChatClient_OnUserUpdateOccurred;
            ChannelSession.Chat.OnUserPurgeOccurred += ChatClient_OnUserPurgeOccurred;

            if (ChannelSession.MixerChannel.badge != null && ChannelSession.MixerChannel.badge != null && !string.IsNullOrEmpty(ChannelSession.MixerChannel.badge.url))
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        var bytes = await client.DownloadDataTaskAsync(new Uri(ChannelSession.MixerChannel.badge.url, UriKind.Absolute));
                        OldChatControl.SubscriberBadgeBitmap = new BitmapImage();
                        OldChatControl.SubscriberBadgeBitmap.BeginInit();
                        OldChatControl.SubscriberBadgeBitmap.CacheOption = BitmapCacheOption.OnLoad;
                        OldChatControl.SubscriberBadgeBitmap.StreamSource = new MemoryStream(bytes);
                        OldChatControl.SubscriberBadgeBitmap.EndInit();
                    }
                }
                catch (Exception ex) { Logger.Log(ex); }
            }

            if (!ChannelSession.Settings.HideChatUserList)
            {
                await userUpdateLock.WaitAndRelease(async () =>
                {
                    foreach (UserViewModel user in (await ChannelSession.ActiveUsers.GetAllUsers()).ToList())
                    {
                        await this.UserControls.Add(user);
                    }

                    await this.RefreshViewerChatterCounts();
                });
            }

            IEnumerable<ChatMessageEventModel> oldMessages = await ChannelSession.Chat.GetChatHistory(50);
            if (oldMessages != null)
            {
                foreach (ChatMessageEventModel message in oldMessages)
                {
                    await AddMessage(new ChatMessageViewModel(message));
                }
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

        private async Task RefreshUsersInList(IEnumerable<UserViewModel> users)
        {
            if (!ChannelSession.Settings.HideChatUserList)
            {
                await userUpdateLock.WaitAndRelease(async () =>
                {
                    await this.UserControls.Remove(users);

                    foreach (UserViewModel user in users.Where(u => u.IsInChat))
                    {
                        await this.UserControls.Add(user);
                    }

                    await this.RefreshViewerChatterCounts();
                });
            }
        }

        private async Task RemoveUsersInList(IEnumerable<UserViewModel> users)
        {
            if (!ChannelSession.Settings.HideChatUserList)
            {
                await userUpdateLock.WaitAndRelease(async () =>
                {
                    await this.UserControls.Remove(users);

                    await this.RefreshViewerChatterCounts();
                });
            }
        }

        private async Task RefreshViewerChatterCounts()
        {
            this.ViewersCountTextBlock.Text = ChannelSession.MixerChannel.viewersCurrent.ToString();
            this.ChatCountTextBlock.Text = (await ChannelSession.ActiveUsers.Count()).ToString();
        }

        private async Task AddMessage(ChatMessageViewModel message)
        {
            await this.messageUpdateLock.WaitAndRelease(() =>
            {
                ChatMessageControl messageControl = new ChatMessageControl();
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

                if (!this.isPopOut)
                {
                    
                }

                return Task.FromResult(0);
            });
        }

        private async Task ShowUserDialog(UserViewModel user)
        {
            if (user != null && !user.IsAnonymous)
            {
                UserDialogResult result = await MessageBoxHelper.ShowUserDialog(user);

                switch (result)
                {
                    case UserDialogResult.Purge:
                        await ChannelSession.Chat.PurgeUser(user.UserName);
                        break;
                    case UserDialogResult.Timeout1:
                        await ChannelSession.Chat.TimeoutUser(user.UserName, 60);
                        break;
                    case UserDialogResult.Timeout5:
                        await ChannelSession.Chat.TimeoutUser(user.UserName, 300);
                        break;
                    case UserDialogResult.Ban:
                        if (await MessageBoxHelper.ShowConfirmationDialog(string.Format("This will ban the user {0} from this channel. Are you sure?", user.UserName)))
                        {
                            await ChannelSession.Chat.BanUser(user);
                        }
                        break;
                    case UserDialogResult.Unban:
                        await ChannelSession.Chat.UnBanUser(user);
                        break;
                    case UserDialogResult.Follow:
                        ExpandedChannelModel channelToFollow = await ChannelSession.MixerStreamerConnection.GetChannel(user.ChannelID);
                        await ChannelSession.MixerStreamerConnection.Follow(channelToFollow, ChannelSession.MixerStreamerUser);
                        break;
                    case UserDialogResult.Unfollow:
                        ExpandedChannelModel channelToUnfollow = await ChannelSession.MixerStreamerConnection.GetChannel(user.ChannelID);
                        await ChannelSession.MixerStreamerConnection.Unfollow(channelToUnfollow, ChannelSession.MixerStreamerUser);
                        break;
                    case UserDialogResult.PromoteToMod:
                        if (await MessageBoxHelper.ShowConfirmationDialog(string.Format("This will promote the user {0} to a moderator of this channel. Are you sure?", user.UserName)))
                        {
                            await ChannelSession.Chat.ModUser(user);
                        }
                        break;
                    case UserDialogResult.DemoteFromMod:
                        if (await MessageBoxHelper.ShowConfirmationDialog(string.Format("This will demote the user {0} from a moderator of this channel. Are you sure?", user.UserName)))
                        {
                            await ChannelSession.Chat.UnModUser(user);
                        }
                        break;
                    case UserDialogResult.MixerPage:
                        Process.Start($"https://mixer.com/{user.UserName}");
                        break;
                    case UserDialogResult.EditUser:
                        UserDataEditorWindow window = new UserDataEditorWindow(ChannelSession.Settings.UserData[user.ID]);
                        await Task.Delay(100);
                        window.Show();
                        await Task.Delay(100);
                        window.Focus();
                        break;
                    case UserDialogResult.Close:
                    default:
                        // Just close
                        break;
                }
            }
        }

        #endregion Chat Update Methods

        #region UI Events

        private void PopOutChatButton_Click(object sender, RoutedEventArgs e)
        {
            PopOutWindow window = new PopOutWindow("Chat", new OldChatControl(isPopOut: true));
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
            this.ChatLockButtonIcon.Foreground = (this.lockChatList) ? Brushes.Green : Brushes.Red;
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
                this.HideIntellisense();

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
                        List<MixerChatEmoteModel> emotes = MixerChatEmoteModel.FindMatchingEmoticons(tag).ToList();
                        if (emotes.ToList().Count > 0)
                        {
                            emotes = emotes.Take(5).Reverse().ToList();
                            this.indexOfTag = this.ChatMessageTextBox.Text.LastIndexOf(tag);
                            EmoticonIntellisenseListBox.ItemsSource = emotes;

                            // Select the bottom icon
                            EmoticonIntellisenseListBox.SelectedIndex = emotes.Count - 1;

                            Rect positionOfCarat = this.ChatMessageTextBox.GetRectFromCharacterIndex(this.ChatMessageTextBox.CaretIndex, true);
                            Point topLeftOffset = this.ChatMessageTextBox.TransformToAncestor(this).Transform(new Point(positionOfCarat.Left, positionOfCarat.Top));

                            ShowEmoticonIntellisense(topLeftOffset.X, topLeftOffset.Y, emotes.Count);
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
            MixerChatEmoteModel emoticon = EmoticonIntellisenseListBox.SelectedItem as MixerChatEmoteModel;
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

                    await this.Window.RunAsyncOperation(async () =>
                    {
                        ChatMessageEventModel response = await ChannelSession.Chat.WhisperWithResponse(username, message, ShouldSendAsStreamer());
                        if (response != null)
                        {
                            await this.AddMessage(new ChatMessageViewModel(response));
                        }
                    });
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
                try
                {
                    Clipboard.SetText(control.Message.PlainTextMessage);
                }
                catch (Exception ex) { Logger.Log(ex); }
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

        private async void UserInformationMenuItem_Click(object sender, RoutedEventArgs e)
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
                        //message.DeleteMessage(deleteEvent.moderator?.user_name);
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
                            //message.DeleteMessage(purgeEvent.Item2);
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

        private async void ChatClient_OnUsersJoinOccurred(object sender, IEnumerable<UserViewModel> users)
        {
            await this.Dispatcher.InvokeAsync<Task>(async () =>
            {
                await this.RefreshUsersInList(users);
            });

            if (ChannelSession.Settings.ChatShowUserJoinLeave && users.Count() < 5)
            {
                foreach (UserViewModel user in users)
                {
                    await this.AddAlertMessage(string.Format("{0} Joined Chat", user.UserName), user, ChannelSession.Settings.ChatUserJoinLeaveColorScheme);
                }
            }
        }

        private async void ChatClient_OnUserUpdateOccurred(object sender, UserViewModel user)
        {
            await this.Dispatcher.InvokeAsync<Task>(async () =>
            {
                await this.RefreshUsersInList(new List<UserViewModel>() { user });
            });
        }

        private async void ChatClient_OnUsersLeaveOccurred(object sender, IEnumerable<UserViewModel> users)
        {
            await this.Dispatcher.InvokeAsync<Task>(async () =>
            {
                await this.RemoveUsersInList(users);
            });

            if (ChannelSession.Settings.ChatShowUserJoinLeave && users.Count() < 5)
            {
                foreach (UserViewModel user in users)
                {
                    await this.AddAlertMessage(string.Format("{0} Left Chat", user.UserName), user, ChannelSession.Settings.ChatUserJoinLeaveColorScheme);
                }
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

        private async void GlobalEvents_OnSubscriptionGiftedOccurred(object sender, Tuple<UserViewModel, UserViewModel> e)
        {
            if (ChannelSession.Settings.ChatShowEventAlerts)
            {
                await this.AddAlertMessage(string.Format("{0} Gifted A Subscription To {1}", e.Item1.UserName, e.Item2.UserName), e.Item1, ChannelSession.Settings.ChatEventAlertsColorScheme);
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
                await ChannelSession.Chat.Whisper(ChannelSession.MixerStreamerUser.username, message);
            }
        }

        private async void GlobalEvents_OnSkillUseOccurred(object sender, SkillUsageModel skill)
        {
            if (skill.SkillInstance != null)
            {
                await this.Dispatcher.InvokeAsync<Task>(async () =>
                {
                    await this.AddMessage(new ChatMessageViewModel(skill.SkillInstance, skill.User));
                });

                if (skill.SkillInstance != null && skill.SkillInstance.IsGif)
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Run(async () =>
                    {
                        await this.gifSkillPopoutLock.WaitAndRelease(async () =>
                        {
                            await this.Dispatcher.InvokeAsync<Task>(async () =>
                            {
                                await this.GifSkillPopout.ShowGif(skill);
                            });
                        });
                    });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
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
