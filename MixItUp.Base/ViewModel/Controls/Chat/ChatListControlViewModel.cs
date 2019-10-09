using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.Chat;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Mixer;
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
                List<string> results = new List<string>() { "Streamer" };
                if (ChannelSession.Services.Chat.MixerChatService.IsBotConnected)
                {
                    results.Add("Bot");
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

        public IEnumerable<ChatCommand> ContextMenuChatCommands { get { return ChannelSession.Services.Chat.ChatMenuCommands; } }

        public event EventHandler<MixerSkillChatMessageViewModel> GifSkillOccured = delegate { };
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

                        await ChannelSession.Services.Chat.Whisper(username, message, this.SendAsStreamer, waitForResponse: true);
                    }
                    else if (ChatAction.ClearRegex.IsMatch(this.SendMessageText))
                    {
                        await ChannelSession.Services.Chat.ClearMessages();
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
            GlobalEvents.OnChatMessageReceived += GlobalEvents_OnChatMessageReceived;
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

        private void GlobalEvents_OnChatMessageReceived(object sender, ChatMessageViewModel message)
        {
            if (message is MixerSkillChatMessageViewModel)
            {
                MixerSkillChatMessageViewModel skillMessage = (MixerSkillChatMessageViewModel)message;
                if (skillMessage.Skill.Type == MixerSkillTypeEnum.Gif)
                {
                    this.GifSkillOccured(this, skillMessage);
                }
            }
        }
    }
}
