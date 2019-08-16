using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModel.Window;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.MainControls
{
    public class ChatMainControlViewModel : MainControlViewModelBase
    {
        public byte[] SubscriberBadgeBytes { get; private set; }

        public ObservableCollection<ChatMessageViewModel> Messages { get; private set; }
        public IEnumerable<UserViewModel> DisplayUsers { get; private set; }

        public uint ViewersCount
        {
            get { return this.viewersCount; }
            set
            {
                this.viewersCount = value;
                this.NotifyPropertyChanged();
            }
        }
        private uint viewersCount;

        public uint ChattersCount
        {
            get { return this.chattersCount; }
            set
            {
                this.chattersCount = value;
                this.NotifyPropertyChanged();
            }
        }
        private uint chattersCount;

        public string EnableDisableButtonText
        {
            get { return this.enableDisableButtonText; }
            set
            {
                this.enableDisableButtonText = value;
                this.NotifyPropertyChanged();
            }
        }
        private string enableDisableButtonText = "Disable Chat";

        public IEnumerable<string> SendAsOptions
        {
            get
            {
                List<string> results = new List<string>() { "Streamer" };
                if (ChannelSession.Services.ChatService.MixerChatService.IsBotConnected)
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

        public event EventHandler ScrollingLockChanged = delegate { };

        public ICommand ClearChatCommand { get; private set; }
        public ICommand EnableDisableChatCommand { get; private set; }

        public ICommand SendMessageCommand { get; private set; }

        public ICommand ScrollingLockCommand { get; private set; }

        public ChatMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            this.ClearChatCommand = this.CreateCommand(async (parameter) =>
            {
                await ChannelSession.Services.ChatService.ClearMessages();
            });

            this.EnableDisableChatCommand = this.CreateCommand((parameter) =>
            {
                ChannelSession.Services.ChatService.DisableChat = !ChannelSession.Services.ChatService.DisableChat;
                if (ChannelSession.Services.ChatService.DisableChat)
                {
                    this.EnableDisableButtonText = "Enable Chat";
                }
                else
                {
                    this.EnableDisableButtonText = "Disable Chat";
                }
                return Task.FromResult(0);
            });

            this.SendMessageCommand = this.CreateCommand((parameter) =>
            {
                if (!string.IsNullOrEmpty(this.SendMessageText))
                {
                    ChannelSession.Services.ChatService.SendMessage(Model.PlatformTypeEnum.Mixer, this.SendMessageText, sendAsStreamer: (this.SendAsIndex == 0));
                    this.SendMessageText = string.Empty;
                }
                return Task.FromResult(0);
            });

            this.ScrollingLockCommand = this.CreateCommand((parameter) =>
            {
                this.IsScrollingLocked = !this.IsScrollingLocked;
                this.ScrollingLockChanged(this, new EventArgs());
                return Task.FromResult(0);
            });
        }

        protected override async Task OnLoadedInternal()
        {
            ChannelSession.Services.ChatService.DisplayUsersUpdated += ChatService_DisplayUsersUpdated;
            this.Messages = ChannelSession.Services.ChatService.Messages;
            this.DisplayUsers = ChannelSession.Services.ChatService.DisplayUsers;

            this.Messages.CollectionChanged += Messages_CollectionChanged;

            if (ChannelSession.MixerChannel.badge != null && ChannelSession.MixerChannel.badge != null && !string.IsNullOrEmpty(ChannelSession.MixerChannel.badge.url))
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        this.SubscriberBadgeBytes = await client.DownloadDataTaskAsync(new Uri(ChannelSession.MixerChannel.badge.url, UriKind.Absolute));
                    }
                }
                catch (Exception ex) { Logger.Log(ex); }
            }

            this.RefreshNumbers();
        }

        protected override async Task OnVisibleInternal()
        {
            this.NotifyPropertyChanged("SendAsOptions");
            await base.OnVisibleInternal();
        }

        private void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.RefreshNumbers();
        }

        private void RefreshNumbers()
        {
            this.ViewersCount = ChannelSession.MixerChannel.viewersCurrent;
            this.ChattersCount = (uint)ChannelSession.Services.ChatService.AllUsers.Count;
        }

        private void ChatService_DisplayUsersUpdated(object sender, EventArgs e)
        {
            this.DisplayUsers = ChannelSession.Services.ChatService.DisplayUsers;
            this.NotifyPropertyChanged("DisplayUsers");
        }
    }
}
