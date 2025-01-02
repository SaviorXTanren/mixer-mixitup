using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Services.Twitch.New;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Chat
{
    public class ChatListControlViewModel : WindowControlViewModelBase
    {
        public static readonly Regex UserNameTagRegex = new Regex(@"@\w+");
        public static readonly Regex WhisperRegex = new Regex(@"^/w(hisper)? @\w+ ", RegexOptions.IgnoreCase);
        public static readonly Regex ClearRegex = new Regex(@"^/clear$", RegexOptions.IgnoreCase);
        public static readonly Regex TimeoutRegex = new Regex(@"^/timeout @\w+ \d+$", RegexOptions.IgnoreCase);
        public static readonly Regex BanRegex = new Regex(@"^/ban @\w+$", RegexOptions.IgnoreCase);

        public ThreadSafeObservableCollection<ChatMessageViewModel> Messages { get; private set; }

        public int AlternationCount { get { return (ChannelSession.Settings.UseAlternatingBackgroundColors) ? 2 : 1; } }

        public IEnumerable<string> SendAsOptions
        {
            get
            {
                List<string> results = new List<string>() { MixItUp.Base.Resources.Streamer };
                if (ServiceManager.Get<TwitchSession>().IsBotConnected)
                {
                    results.Add(MixItUp.Base.Resources.Bot);
                }
                return results;
            }
        }

        public int SendAsIndex
        {
            get { return this.sendAsIndex; }
            set
            {
                this.sendAsIndex = value;
                this.NotifyPropertyChanged();
            }
        }
        private int sendAsIndex = 0;

        public bool SendAsStreamer { get { return (this.SendAsIndex == 0); } }

        public string SendMessageText
        {
            get { return this.sendMessageText; }
            set
            {
                this.sendMessageText = value;
                this.NotifyPropertyChanged();
            }
        }
        private string sendMessageText;

        public List<string> SentMessageHistory { get; set; } = new List<string>();
        private int SentMessageHistoryIndex = 0;

        public bool IsScrollingLocked
        {
            get { return this.isScrollingLocked; }
            set
            {
                this.isScrollingLocked = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("LockIconColor");
            }
        }
        private bool isScrollingLocked = true;

        public string LockIconColor { get { return (this.IsScrollingLocked) ? "Green" : "Red"; } }

        public IEnumerable<CommandModelBase> ContextMenuChatCommands { get { return ServiceManager.Get<ChatService>().ChatMenuCommands.ToList(); } }

        public event EventHandler MessageSentOccurred = delegate { };
        public event EventHandler ScrollingLockChanged = delegate { };
        public event EventHandler ContextMenuCommandsChanged = delegate { };

        public ICommand SendMessageCommand { get; private set; }

        public ICommand ScrollingLockCommand { get; private set; }

        public ChatListControlViewModel(UIViewModelBase windowViewModel)
            : base(windowViewModel)
        {
            this.SendMessageCommand = this.CreateCommand(async () =>
            {
                if (!string.IsNullOrEmpty(this.SendMessageText))
                {
                    if (ChatListControlViewModel.WhisperRegex.IsMatch(this.SendMessageText))
                    {
                        Match whisperRegexMatch = ChatListControlViewModel.WhisperRegex.Match(this.SendMessageText);

                        string message = this.SendMessageText.Substring(whisperRegexMatch.Value.Length);

                        Match userNameMatch = ChatListControlViewModel.UserNameTagRegex.Match(whisperRegexMatch.Value);
                        string username = UserService.SanitizeUsername(userNameMatch.Value);

                        await ServiceManager.Get<ChatService>().Whisper(username, StreamingPlatformTypeEnum.All, message, this.SendAsStreamer);
                    }
                    else if (ChatListControlViewModel.ClearRegex.IsMatch(this.SendMessageText))
                    {
                        await ServiceManager.Get<ChatService>().ClearMessages(StreamingPlatformTypeEnum.Twitch);
                    }
                    else if (ChatListControlViewModel.TimeoutRegex.IsMatch(this.SendMessageText))
                    {
                        string[] splits = this.SendMessageText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (splits.Length == 3)
                        {
                            string username = UserService.SanitizeUsername(splits[1]);
                            UserV2ViewModel user = ServiceManager.Get<UserService>().GetActiveUserByPlatform(StreamingPlatformTypeEnum.All, platformUsername: username);
                            if (user != null)
                            {
                                if (int.TryParse(splits[2], out int amount) && amount > 0)
                                {
                                    await ServiceManager.Get<ChatService>().TimeoutUser(user, amount);
                                }
                                else
                                {
                                    await ServiceManager.Get<ChatService>().AddMessage(new AlertChatMessageViewModel(MixItUp.Base.Resources.ChatTimeoutAmountMustBeGreaterThanZero));
                                }
                            }
                            else
                            {
                                await ServiceManager.Get<ChatService>().AddMessage(new AlertChatMessageViewModel(MixItUp.Base.Resources.UserNotFound));
                            }
                        }
                    }
                    else if (ChatListControlViewModel.BanRegex.IsMatch(this.SendMessageText))
                    {
                        string[] splits = this.SendMessageText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (splits.Length == 2)
                        {
                            string username = UserService.SanitizeUsername(splits[1]);
                            UserV2ViewModel user = ServiceManager.Get<UserService>().GetActiveUserByPlatform(StreamingPlatformTypeEnum.All, platformUsername: username);
                            if (user != null)
                            {
                                await ServiceManager.Get<ChatService>().BanUser(user);
                            }
                        }
                        else
                        {
                            await ServiceManager.Get<ChatService>().AddMessage(new AlertChatMessageViewModel(MixItUp.Base.Resources.UserNotFound));
                        }
                    }
                    else
                    {
                        await ServiceManager.Get<ChatService>().SendMessage(this.SendMessageText, StreamingPlatformTypeEnum.All, sendAsStreamer: this.SendAsStreamer);
                    }

                    this.SentMessageHistory.Remove(this.SendMessageText);
                    this.SentMessageHistory.Insert(0, this.SendMessageText);
                    this.SentMessageHistoryIndex = -1;

                    this.SendMessageText = string.Empty;
                    this.MessageSentOccurred(this, new EventArgs());
                }
            });

            this.ScrollingLockCommand = this.CreateCommand(() =>
            {
                this.IsScrollingLocked = !this.IsScrollingLocked;
                this.ScrollingLockChanged(this, new EventArgs());
            });

            ChatService.OnChatVisualSettingsChanged += ChatService_OnChatVisualSettingsChanged;
            ServiceManager.Get<ChatService>().ChatCommandsReprocessed += Chat_ChatCommandsReprocessed;
        }

        public void MoveSentMessageHistoryUp()
        {
            if (this.SentMessageHistory.Count > 0 && this.SentMessageHistoryIndex < (this.SentMessageHistory.Count - 1))
            {
                this.SentMessageHistoryIndex++;
                this.SendMessageText = this.SentMessageHistory[this.SentMessageHistoryIndex];
            }
        }

        public void MoveSentMessageHistoryDown()
        {
            if (this.SentMessageHistory.Count > 0 && this.SentMessageHistoryIndex >= 0)
            {
                this.SentMessageHistoryIndex--;
                if (this.SentMessageHistoryIndex < 0)
                {
                    this.SendMessageText = string.Empty;
                }
                else
                {
                    this.SendMessageText = this.SentMessageHistory[this.SentMessageHistoryIndex];
                }
            }
        }

        protected override async Task OnOpenInternal()
        {
            await base.OnOpenInternal();

            this.Messages = ServiceManager.Get<ChatService>().Messages;
        }

        protected override async Task OnVisibleInternal()
        {
            await base.OnVisibleInternal();

            this.NotifyPropertyChanged("SendAsOptions");
        }

        private void ChatService_OnChatVisualSettingsChanged(object sender, EventArgs e)
        {
            this.NotifyPropertyChanged("AlternationCount");
            this.Messages.Clear();
        }

        private void Chat_ChatCommandsReprocessed(object sender, EventArgs e)
        {
            this.ContextMenuCommandsChanged(this, new EventArgs());
        }
    }
}
