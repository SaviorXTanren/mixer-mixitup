using MixItUp.Base.Model;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Threading;
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

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

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

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            AsyncRunner.RunAsyncBackground(this.MinuteBackgroundThread, this.cancellationTokenSource.Token, 60000);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            ChatService.OnChatVisualSettingsChanged += ChatService_OnChatVisualSettingsChanged;
        }

        protected override async Task OnOpenInternal()
        {
            await base.OnOpenInternal();

            ServiceManager.Get<UserService>().DisplayUsersUpdated += ChatService_DisplayUsersUpdated;
            this.DisplayUsers = ServiceManager.Get<UserService>().DisplayUsers;
        }

        protected override async Task OnVisibleInternal()
        {
            await base.OnVisibleInternal();

            this.NotifyPropertyChanged("ShowViewerAndChatterNumbers");
            this.NotifyPropertyChanged("ShowChatUserList");
            this.NotifyPropertyChanged("EnableDisableChatButtonText");
            this.NotifyPropertyChanged("PauseUnpauseCommandsButtonText");
        }

        private void ChatService_DisplayUsersUpdated(object sender, EventArgs e)
        {
            this.DisplayUsers = ServiceManager.Get<UserService>().DisplayUsers;
            this.NotifyPropertyChanged("DisplayUsers");
        }

        private void ChatService_OnChatVisualSettingsChanged(object sender, EventArgs e)
        {
            ChatService_DisplayUsersUpdated(sender, e);
        }

        private Task MinuteBackgroundThread(CancellationToken cancellationToken)
        {
            this.ViewersCount = ServiceManager.Get<ChatService>().GetViewerCount();
            this.ChattersCount = ServiceManager.Get<UserService>().ActiveUserCount;

            return Task.CompletedTask;
        }
    }
}
