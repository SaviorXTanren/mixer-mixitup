using MaterialDesignThemes.Wpf;
using Mixer.Base.Model.Channel;
using MixItUp.Base;
using MixItUp.Base.Model.Chat.Mixer;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Mixer;
using MixItUp.Base.ViewModel.Controls.MainControls;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModel.Window;
using MixItUp.WPF.Controls.Dialogs;
using MixItUp.WPF.Windows.Users;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private ScrollViewer chatListScrollViewer;

        private ChatMainControlViewModel viewModel;

        private int indexOfLastIntellisenseText;

        private SemaphoreSlim gifSkillPopoutLock = new SemaphoreSlim(1);

        public ChatControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.viewModel = new ChatMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);

            this.viewModel.GifSkillOccured += ViewModel_GifSkillOccured;
            this.viewModel.MessageSentOccurred += ViewModel_MessageSentOccurred;
            this.viewModel.ScrollingLockChanged += ViewModel_ScrollingLockChanged;

            await this.viewModel.OnLoaded();
            this.DataContext = this.viewModel;
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.viewModel.OnVisible();
            await base.OnVisibilityChanged();
        }

        private void ViewModel_GifSkillOccured(object sender, MixerSkillChatMessageViewModel skillMessage)
        {
            if (!string.IsNullOrEmpty(skillMessage.Skill.Image))
            {
                Task.Run(() =>
                {
                    this.gifSkillPopoutLock.WaitAndRelease(async () =>
                    {
                        await this.Dispatcher.InvokeAsync(async () =>
                        {
                            await this.GifSkillPopout.ShowGif(skillMessage);
                        });
                    });
                });
            }
        }

        private async void ViewModel_MessageSentOccurred(object sender, EventArgs e)
        {
            await Task.Delay(100);
            this.ChatMessageTextBox.Focus();
        }

        private void ViewModel_ScrollingLockChanged(object sender, System.EventArgs e)
        {
            this.ChatLockButtonIcon.Kind = (this.viewModel.IsScrollingLocked) ? MaterialDesignThemes.Wpf.PackIconKind.LockOutline : MaterialDesignThemes.Wpf.PackIconKind.LockOpenOutline;
            if (this.chatListScrollViewer != null)
            {
                this.chatListScrollViewer.VerticalScrollBarVisibility = (this.viewModel.IsScrollingLocked) ? ScrollBarVisibility.Hidden : ScrollBarVisibility.Visible;
                if (this.viewModel.IsScrollingLocked)
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
        }

        private void ChatMessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.viewModel.SendMessageText = this.ChatMessageTextBox.Text;
                this.viewModel.SendMessageCommand.Execute(null);
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

                        if (users.Count > 0)
                        {
                            users = users.OrderBy(u => u.UserName).Take(5).Reverse().ToList();
                            this.ShowIntellisense(tag, this.UsernameIntellisense, this.UsernameIntellisenseListBox, users);
                        }
                    }
                    else if (tag.StartsWith(":"))
                    {
                        List<MixerChatEmoteModel> emotes = MixerChatEmoteModel.FindMatchingEmoticons(tag).ToList();
                        if (emotes.ToList().Count > 0)
                        {
                            emotes = emotes.Take(5).Reverse().ToList();
                            this.ShowIntellisense(tag, this.EmoticonIntellisense, this.EmoticonIntellisenseListBox, emotes);
                        }
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        private void ChatMessageTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (this.UsernameIntellisense.IsPopupOpen)
            {
                switch (e.Key)
                {
                    case Key.Escape:
                        this.HideIntellisense();
                        e.Handled = true;
                        break;
                    case Key.Tab:
                    case Key.Enter:
                        this.SelectIntellisenseUser();
                        e.Handled = true;
                        break;
                    case Key.Up:
                        this.UsernameIntellisenseListBox.SelectedIndex = MathHelper.Clamp(this.UsernameIntellisenseListBox.SelectedIndex - 1, 0, this.UsernameIntellisenseListBox.Items.Count - 1);
                        e.Handled = true;
                        break;
                    case Key.Down:
                        this.UsernameIntellisenseListBox.SelectedIndex = MathHelper.Clamp(this.UsernameIntellisenseListBox.SelectedIndex + 1, 0, this.UsernameIntellisenseListBox.Items.Count - 1);
                        e.Handled = true;
                        break;
                }
            }
            else if (this.EmoticonIntellisense.IsPopupOpen)
            {
                switch (e.Key)
                {
                    case Key.Escape:
                        this.HideIntellisense();
                        e.Handled = true;
                        break;
                    case Key.Tab:
                    case Key.Enter:
                        this.SelectIntellisenseEmoticon();
                        e.Handled = true;
                        break;
                    case Key.Up:
                        EmoticonIntellisenseListBox.SelectedIndex = MathHelper.Clamp(this.EmoticonIntellisenseListBox.SelectedIndex - 1, 0, this.EmoticonIntellisenseListBox.Items.Count - 1);
                        e.Handled = true;
                        break;
                    case Key.Down:
                        this.EmoticonIntellisenseListBox.SelectedIndex = MathHelper.Clamp(this.EmoticonIntellisenseListBox.SelectedIndex + 1, 0, this.EmoticonIntellisenseListBox.Items.Count - 1);
                        e.Handled = true;
                        break;
                }
            }
            else
            {
                switch (e.Key)
                {
                    case Key.Up:
                        this.viewModel.MoveSentMessageHistoryUp();
                        this.ChatMessageTextBox.CaretIndex = this.ChatMessageTextBox.Text.Length;
                        break;

                    case Key.Down:
                        this.viewModel.MoveSentMessageHistoryDown();
                        this.ChatMessageTextBox.CaretIndex = this.ChatMessageTextBox.Text.Length;
                        break;
                }
            }
        }

        private void UsernameIntellisenseListBox_PreviewMouseUp(object sender, MouseButtonEventArgs e) { this.SelectIntellisenseUser(); }

        private void EmoticonIntellisenseListBox_PreviewMouseUp(object sender, MouseButtonEventArgs e) { this.SelectIntellisenseEmoticon(); }

        private void ChatList_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (this.chatListScrollViewer == null)
            {
                this.chatListScrollViewer = (ScrollViewer)e.OriginalSource;
            }

            if (this.viewModel.IsScrollingLocked && this.chatListScrollViewer != null)
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

        private void ChatLockButton_MouseEnter(object sender, MouseEventArgs e) { this.ChatLockButton.Opacity = 1; }

        private void ChatLockButton_MouseLeave(object sender, MouseEventArgs e) { this.ChatLockButton.Opacity = 0.3; }

        private void MessageCopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.ChatList.SelectedItem != null && this.ChatList.SelectedItem is ChatMessageViewModel)
            {
                ChatMessageViewModel message = (ChatMessageViewModel)this.ChatList.SelectedItem;
                try
                {
                    Clipboard.SetText(message.PlainTextMessage);
                }
                catch (Exception ex) { Logger.Log(ex); }
            }
        }

        private async void MessageDeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.ChatList.SelectedItem != null && this.ChatList.SelectedItem is ChatMessageViewModel)
            {
                ChatMessageViewModel message = (ChatMessageViewModel)this.ChatList.SelectedItem;
                if (!message.IsWhisper)
                {
                    await ChannelSession.Services.ChatService.DeleteMessage(message);
                }
            }
        }

        private void MessageWhisperUserMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.ChatList.SelectedItem != null && this.ChatList.SelectedItem is ChatMessageViewModel)
            {
                ChatMessageViewModel message = (ChatMessageViewModel)this.ChatList.SelectedItem;
                if (message.User != null)
                {
                    this.viewModel.SendMessageText = $"/w @{message.User.UserName} ";
                    this.ChatMessageTextBox.Focus();
                    this.ChatMessageTextBox.CaretIndex = this.ChatMessageTextBox.Text.Length;
                }
            }
        }

        private async void UserInformationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.ChatList.SelectedItem != null && this.ChatList.SelectedItem is ChatMessageViewModel)
            {
                ChatMessageViewModel message = (ChatMessageViewModel)this.ChatList.SelectedItem;
                if (message.User != null)
                {
                    await this.ShowUserDialog(message.User);
                }
            }
        }

        private async void UserList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.UserList.SelectedItem != null && this.UserList.SelectedItem is UserViewModel)
            {
                UserViewModel user = (UserViewModel)this.UserList.SelectedItem;
                this.UserList.SelectedIndex = -1;
                await this.ShowUserDialog(user);
            }
        }

        private void ShowIntellisense<T>(string text, PopupBox intellisense, ListBox listBox, List<T> items)
        {
            this.indexOfLastIntellisenseText = this.ChatMessageTextBox.Text.LastIndexOf(text);
            listBox.ItemsSource = items;
            listBox.SelectedIndex = items.Count - 1;

            Rect positionOfCarat = this.ChatMessageTextBox.GetRectFromCharacterIndex(this.ChatMessageTextBox.CaretIndex, true);
            Point topLeftOffset = this.ChatMessageTextBox.TransformToAncestor(this).Transform(new Point(positionOfCarat.Left, positionOfCarat.Top));

            int itemHeight = items.Count * 40;
            Canvas.SetLeft(intellisense, topLeftOffset.X + 10);
            Canvas.SetTop(intellisense, topLeftOffset.Y - itemHeight - 40);
            intellisense.UpdateLayout();

            if (!intellisense.IsPopupOpen)
            {
                intellisense.IsPopupOpen = true;
            }
        }

        private void SelectIntellisenseEmoticon()
        {
            MixerChatEmoteModel emoticon = EmoticonIntellisenseListBox.SelectedItem as MixerChatEmoteModel;
            if (emoticon != null)
            {
                this.SelectIntellisenseItem(emoticon.Name);
            }
            this.HideIntellisense();
        }

        private void SelectIntellisenseUser()
        {
            UserViewModel user = UsernameIntellisenseListBox.SelectedItem as UserViewModel;
            if (user != null)
            {
                this.SelectIntellisenseItem("@" + user.UserName);
            }
            this.HideIntellisense();
        }

        private void SelectIntellisenseItem(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                if (this.indexOfLastIntellisenseText == 0)
                {
                    this.ChatMessageTextBox.Text = name + " ";
                }
                else
                {
                    this.ChatMessageTextBox.Text = this.ChatMessageTextBox.Text.Substring(0, this.indexOfLastIntellisenseText) + name + " ";
                }
                this.ChatMessageTextBox.CaretIndex = this.ChatMessageTextBox.Text.Length;
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

        private async Task ShowUserDialog(UserViewModel user)
        {
            if (user != null && !user.IsAnonymous)
            {
                object result = await DialogHelper.ShowCustom(new UserDialogControl(user));
                if (result != null)
                {
                    UserDialogResult dialogResult = EnumHelper.GetEnumValueFromString<UserDialogResult>(result.ToString());
                    switch (dialogResult)
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
                            if (await DialogHelper.ShowConfirmation(string.Format("This will ban the user {0} from this channel. Are you sure?", user.UserName)))
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
                            if (await DialogHelper.ShowConfirmation(string.Format("This will promote the user {0} to a moderator of this channel. Are you sure?", user.UserName)))
                            {
                                await ChannelSession.Chat.ModUser(user);
                            }
                            break;
                        case UserDialogResult.DemoteFromMod:
                            if (await DialogHelper.ShowConfirmation(string.Format("This will demote the user {0} from a moderator of this channel. Are you sure?", user.UserName)))
                            {
                                await ChannelSession.Chat.UnModUser(user);
                            }
                            break;
                        case UserDialogResult.MixerPage:
                            ProcessHelper.LaunchLink($"https://mixer.com/{user.UserName}");
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
        }
    }
}
