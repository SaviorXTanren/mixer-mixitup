using MixItUp.Base.Model;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Glimesh;
using MixItUp.Base.Services.Trovo;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class ChatMainControlViewModel : ChatListControlViewModel
    {
        public IEnumerable<UserV2ViewModel> DisplayUsers { get; private set; }

        public bool ShowViewerAndChatterNumbers { get { return !ChannelSession.Settings.HideViewerAndChatterNumbers; } }

        public bool ShowChatUserList { get { return !ChannelSession.Settings.HideChatUserList; } }

        public int ViewersCount
        {
            get { return this.viewersCount; }
            set
            {
                this.viewersCount = value;
                this.NotifyPropertyChanged();
            }
        }
        private int viewersCount;

        public int ChattersCount
        {
            get { return this.chattersCount; }
            set
            {
                this.chattersCount = value;
                this.NotifyPropertyChanged();
            }
        }
        private int chattersCount;

        public ICommand ClearChatCommand { get; private set; }

        public string EnableDisableChatButtonText
        {
            get
            {
                if (ServiceManager.Get<ChatService>().DisableChat)
                {
                    return MixItUp.Base.Resources.EnableChat;
                }
                else
                {
                    return MixItUp.Base.Resources.DisableChat;
                }
            }
        }
        public ICommand EnableDisableChatCommand { get; private set; }

        public string PauseUnpauseCommandsButtonText
        {
            get
            {
                if (ServiceManager.Get<CommandService>().IsPaused)
                {
                    return MixItUp.Base.Resources.UnpauseCommands;
                }
                else
                {
                    return MixItUp.Base.Resources.PauseCommands;
                }
            }
        }
        public ICommand PauseUnpauseCommandsCommand { get; private set; }

        public ChatMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            this.ClearChatCommand = this.CreateCommand(async () =>
            {
                if (await DialogHelper.ShowConfirmation(MixItUp.Base.Resources.ClearChatConfirmation))
                {
                    await ServiceManager.Get<ChatService>().ClearMessages(StreamingPlatformTypeEnum.All);
                }
            });

            this.EnableDisableChatCommand = this.CreateCommand(async () =>
            {
                if (!ServiceManager.Get<ChatService>().DisableChat && !await DialogHelper.ShowConfirmation(MixItUp.Base.Resources.DisableChatConfirmation))
                {
                    return;
                }

                ServiceManager.Get<ChatService>().DisableChat = !ServiceManager.Get<ChatService>().DisableChat;

                this.NotifyPropertyChanged("EnableDisableChatButtonText");
            });

            this.PauseUnpauseCommandsCommand = this.CreateCommand(async () =>
            {
                if (!ServiceManager.Get<CommandService>().IsPaused && !await DialogHelper.ShowConfirmation(MixItUp.Base.Resources.PauseCommandsConfirmation))
                {
                    return;
                }

                if (ServiceManager.Get<CommandService>().IsPaused)
                {
                    await ServiceManager.Get<CommandService>().Unpause();
                }
                else
                {
                    await ServiceManager.Get<CommandService>().Pause();
                }

                this.NotifyPropertyChanged("PauseUnpauseCommandsButtonText");
            });

            GlobalEvents.OnChatVisualSettingsChanged += GlobalEvents_OnChatVisualSettingsChanged;
        }

        protected override async Task OnOpenInternal()
        {
            await base.OnOpenInternal();

            ServiceManager.Get<UserService>().DisplayUsersUpdated += ChatService_DisplayUsersUpdated;
            this.DisplayUsers = ServiceManager.Get<UserService>().DisplayUsers;

            this.Messages.CollectionChanged += Messages_CollectionChanged;

            this.RefreshNumbers();
        }

        protected override async Task OnVisibleInternal()
        {
            await base.OnVisibleInternal();

            this.NotifyPropertyChanged("ShowViewerAndChatterNumbers");
            this.NotifyPropertyChanged("ShowChatUserList");
            this.NotifyPropertyChanged("EnableDisableChatButtonText");
            this.NotifyPropertyChanged("PauseUnpauseCommandsButtonText");
        }

        private void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.RefreshNumbers();
        }

        private void RefreshNumbers()
        {
            int viewerCount = 0;
            if (ServiceManager.Get<TwitchSessionService>().IsConnected && ServiceManager.Get<TwitchSessionService>().IsLive)
            {
                viewerCount += (int)ServiceManager.Get<TwitchSessionService>().Stream?.viewer_count;
            }
            if (ServiceManager.Get<TrovoSessionService>().IsConnected && ServiceManager.Get<TrovoSessionService>().IsLive)
            {
                viewerCount += (int)ServiceManager.Get<TrovoSessionService>().Channel?.current_viewers;
            }
            if (ServiceManager.Get<GlimeshSessionService>().IsConnected && ServiceManager.Get<GlimeshSessionService>().IsLive)
            {
                viewerCount += ServiceManager.Get<GlimeshSessionService>().User?.channel?.stream?.countViewers ?? 0;
            }
            this.ViewersCount = viewerCount;

            this.ChattersCount = ServiceManager.Get<UserService>().ActiveUserCount;
        }

        private void ChatService_DisplayUsersUpdated(object sender, EventArgs e)
        {
            this.DisplayUsers = ServiceManager.Get<UserService>().DisplayUsers;
            this.NotifyPropertyChanged("DisplayUsers");
        }

        private void GlobalEvents_OnChatVisualSettingsChanged(object sender, EventArgs e)
        {
            ChatService_DisplayUsersUpdated(sender, e);
        }
    }
}
