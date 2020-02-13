using MaterialDesignThemes.Wpf;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Chat.Mixer;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Mixer;
using MixItUp.Base.ViewModel.Controls.Chat;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for ChatListControl.xaml
    /// </summary>
    public partial class ChatListControl : LoadingWindowControlBase
    {
        private ScrollViewer chatListScrollViewer;

        private ChatListControlViewModel viewModel;

        private int indexOfLastIntellisenseText;

        private SemaphoreSlim gifSkillPopoutLock = new SemaphoreSlim(1);
        private object itemsLock = new object();

        private List<object> defaultContextMenuItems = new List<object>();

        public ChatListControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.viewModel = new ChatListControlViewModel(this.Window.ViewModel);

            this.viewModel.GifSkillOccured += ViewModel_GifSkillOccured;
            this.viewModel.MessageSentOccurred += ViewModel_MessageSentOccurred;
            this.viewModel.ScrollingLockChanged += ViewModel_ScrollingLockChanged;
            this.viewModel.ContextMenuCommandsChanged += ViewModel_ContextMenuCommandsChanged;

            await this.viewModel.OnLoaded();
            this.DataContext = this.viewModel;

            BindingOperations.EnableCollectionSynchronization(this.viewModel.Messages, itemsLock);

            foreach (object item in this.ChatList.ContextMenu.Items)
            {
                this.defaultContextMenuItems.Add(item);
            }
            this.ViewModel_ContextMenuCommandsChanged(this, new EventArgs());
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.viewModel.OnVisible();
            await base.OnVisibilityChanged();
        }

        private void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.ChatList.Items.Refresh();
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

        private async void ViewModel_ContextMenuCommandsChanged(object sender, EventArgs e)
        {
            await DispatcherHelper.InvokeDispatcher(() =>
            {
                this.ChatList.ContextMenu.Items.Clear();
                foreach (var item in this.defaultContextMenuItems)
                {
                    this.ChatList.ContextMenu.Items.Add(item);
                }

                if (viewModel.ContextMenuChatCommands.Count() > 0)
                {
                    this.ChatList.ContextMenu.Items.Add(new Separator());
                    foreach (ChatCommand command in viewModel.ContextMenuChatCommands)
                    {
                        MenuItem menuItem = new MenuItem();
                        menuItem.Header = command.Name;
                        menuItem.DataContext = command;
                        menuItem.Click += this.ContextMenuChatCommand_Click;
                        this.ChatList.ContextMenu.Items.Add(menuItem);
                    }
                }

                return Task.FromResult(0);
            });
        }

        private void ChatMessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.viewModel.SendMessageText = this.ChatMessageTextBox.Text;
                this.viewModel.SendMessageCommand.Execute(null);
            }
        }

        private void ChatMessageTextBox_TextChanged(object sender, TextChangedEventArgs textChanged)
        {
            try
            {
                this.viewModel.SendMessageText = this.ChatMessageTextBox.Text;

                this.HideIntellisense();

                string tag = this.ChatMessageTextBox.Text.Split(' ').LastOrDefault();
                if (!string.IsNullOrEmpty(tag))
                {
                    if (tag.StartsWith("@"))
                    {
                        string filter = tag.Substring(1);

                        IEnumerable<UserViewModel> users = ChannelSession.Services.User.GetAllUsers();
                        if (!string.IsNullOrEmpty(filter))
                        {
                            users = users.Where(u => !string.IsNullOrEmpty(u.Username) && u.Username.StartsWith(filter, StringComparison.InvariantCultureIgnoreCase)).ToList();
                        }

                        if (users.Count() > 0)
                        {
                            users = users.OrderBy(u => u.Username).Take(5).Reverse().ToList();
                            this.ShowIntellisense(tag, this.UsernameIntellisense, this.UsernameIntellisenseListBox, users.ToList());
                        }
                    }
                    else if (tag.StartsWith(":"))
                    {
                        List<MixerChatEmoteModel> emotes = MixerChatEmoteModel.FindMatchingEmoticons(tag).ToList();
                        if (emotes.Count > 0)
                        {
                            emotes = emotes.Take(5).Reverse().ToList();
                            this.ShowIntellisense(tag, this.EmoticonIntellisense, this.EmoticonIntellisenseListBox, emotes);
                        }
                    }
                    else if (ChannelSession.Settings.ShowMixrElixrEmotes)
                    {
                        var emotes = ChannelSession.Services.Chat.MixrElixrEmotes.Where(e => e.Key.StartsWith(tag, StringComparison.InvariantCultureIgnoreCase));
                        if (emotes.Count() > 0)
                        {
                            emotes = emotes.Take(5).Reverse();
                            this.ShowIntellisense(tag, this.EmoticonIntellisense, this.EmoticonIntellisenseListBox, emotes.Select(e => e.Value).ToList());
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
                    await ChannelSession.Services.Chat.DeleteMessage(message);
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
                    this.viewModel.SendMessageText = $"/w @{message.User.Username} ";
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
                    await ChatUserDialogControl.ShowUserDialog(message.User);
                }
            }
        }

        private async void ContextMenuChatCommand_Click(object sender, RoutedEventArgs e)
        {
            if (this.ChatList.SelectedItem != null && this.ChatList.SelectedItem is ChatMessageViewModel)
            {
                ChatMessageViewModel message = (ChatMessageViewModel)this.ChatList.SelectedItem;
                if (message.User != null)
                {
                    if (e.Source is MenuItem)
                    {
                        MenuItem menuItem = (MenuItem)e.Source;
                        if (menuItem.DataContext != null && menuItem.DataContext is ChatCommand)
                        {
                            ChatCommand command = (ChatCommand)menuItem.DataContext;
                            await command.Perform(message.Platform, arguments: new List<string>() { message.User.Username });
                        }
                    }
                }
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
            if (this.EmoticonIntellisenseListBox.SelectedItem is MixerChatEmoteModel)
            {
                MixerChatEmoteModel emoticon = this.EmoticonIntellisenseListBox.SelectedItem as MixerChatEmoteModel;
                if (emoticon != null)
                {
                    this.SelectIntellisenseItem(emoticon.Name);
                }
            }
            else
            {
                MixrElixrEmoteModel emoticon = this.EmoticonIntellisenseListBox.SelectedItem as MixrElixrEmoteModel;
                if (emoticon != null)
                {
                    this.SelectIntellisenseItem(emoticon.code);
                }
            }
            this.HideIntellisense();
        }

        private void SelectIntellisenseUser()
        {
            UserViewModel user = UsernameIntellisenseListBox.SelectedItem as UserViewModel;
            if (user != null)
            {
                this.SelectIntellisenseItem("@" + user.Username);
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
    }
}
