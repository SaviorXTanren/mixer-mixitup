using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Model;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModel.Window;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Chat
{
    public class ChatListControlViewModel : WindowControlViewModelBase
    {
        public ObservableCollection<ChatMessageViewModel> Messages { get; private set; }

        public IEnumerable<string> SendAsOptions
        {
            get
            {
                List<string> results = new List<string>() { MixItUp.Base.Resources.Streamer };
                //if (ChannelSession.Services.Chat.MixerChatService.IsBotConnected)
                //{
                //    results.Add(MixItUp.Base.Resources.Bot);
                //}
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

        public IEnumerable<ChatCommand> ContextMenuChatCommands { get { return ChannelSession.Services.Chat.ChatMenuCommands; } }

        public event EventHandler MessageSentOccurred = delegate { };
        public event EventHandler ScrollingLockChanged = delegate { };
        public event EventHandler ContextMenuCommandsChanged = delegate { };

        public ICommand SendMessageCommand { get; private set; }

        public ICommand ScrollingLockCommand { get; private set; }

        public ChatListControlViewModel(WindowViewModelBase windowViewModel)
            : base(windowViewModel)
        {
            this.SendMessageCommand = this.CreateCommand(async (parameter) =>
            {
                if (!string.IsNullOrEmpty(this.SendMessageText))
                {
                    if (ChatAction.WhisperRegex.IsMatch(this.SendMessageText))
                    {
                        Match whisperRegexMatch = ChatAction.WhisperRegex.Match(this.SendMessageText);

                        string message = this.SendMessageText.Substring(whisperRegexMatch.Value.Length);

                        Match userNameMatch = ChatAction.UserNameTagRegex.Match(whisperRegexMatch.Value);
                        string username = userNameMatch.Value;
                        username = username.Trim();
                        username = username.Replace("@", "");

                        await ChannelSession.Services.Chat.Whisper(StreamingPlatformTypeEnum.All, username, message, this.SendAsStreamer, waitForResponse: true);
                    }
                    else if (ChatAction.ClearRegex.IsMatch(this.SendMessageText))
                    {
                        await ChannelSession.Services.Chat.ClearMessages();
                    }
                    else if (ChatAction.TimeoutRegex.IsMatch(this.SendMessageText))
                    {
                        string[] splits = this.SendMessageText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (splits.Length == 3)
                        {
                            string username = splits[1];
                            username = username.Trim();
                            username = username.Replace("@", "");
                            UserViewModel user = ChannelSession.Services.User.GetUserByUsername(username);
                            if (user != null)
                            {
                                if (uint.TryParse(splits[2], out uint amount) && amount > 0)
                                {
                                    await ChannelSession.Services.Chat.TimeoutUser(user, amount);
                                }
                                else
                                {
                                    await ChannelSession.Services.Chat.AddMessage(new AlertChatMessageViewModel("The timeout amount specified must be greater than 0"));
                                }
                            }
                            else
                            {
                                await ChannelSession.Services.Chat.AddMessage(new AlertChatMessageViewModel("No user could be found with that name"));
                            }
                        }
                    }
                    else if (ChatAction.BanRegex.IsMatch(this.SendMessageText))
                    {
                        string[] splits = this.SendMessageText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (splits.Length == 2)
                        {
                            string username = splits[1];
                            username = username.Trim();
                            username = username.Replace("@", "");
                            UserViewModel user = ChannelSession.Services.User.GetUserByUsername(username);
                            if (user != null)
                            {
                                await ChannelSession.Services.Chat.BanUser(user);
                            }
                        }
                        else
                        {
                            await ChannelSession.Services.Chat.AddMessage(new AlertChatMessageViewModel("No user could be found with that name"));
                        }
                    }
                    else
                    {
                        await ChannelSession.Services.Chat.SendMessage(this.SendMessageText, sendAsStreamer: this.SendAsStreamer);
                    }

                    this.SentMessageHistory.Remove(this.SendMessageText);
                    this.SentMessageHistory.Insert(0, this.SendMessageText);
                    this.SentMessageHistoryIndex = -1;

                    this.SendMessageText = string.Empty;
                    this.MessageSentOccurred(this, new EventArgs());
                }
            });

            this.ScrollingLockCommand = this.CreateCommand((parameter) =>
            {
                this.IsScrollingLocked = !this.IsScrollingLocked;
                this.ScrollingLockChanged(this, new EventArgs());
                return Task.FromResult(0);
            });

            ChannelSession.Services.Chat.ChatCommandsReprocessed += Chat_ChatCommandsReprocessed;
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

        protected override async Task OnLoadedInternal()
        {
            await base.OnLoadedInternal();

            this.Messages = ChannelSession.Services.Chat.Messages;
        }

        protected override async Task OnVisibleInternal()
        {
            await base.OnVisibleInternal();

            this.NotifyPropertyChanged("SendAsOptions");
        }

        private void Chat_ChatCommandsReprocessed(object sender, EventArgs e)
        {
            this.ContextMenuCommandsChanged(this, new EventArgs());
        }
    }
}
