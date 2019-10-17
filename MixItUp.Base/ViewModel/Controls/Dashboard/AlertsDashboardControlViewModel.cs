using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Window;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.Dashboard
{
    public class AlertsDashboardControlViewModel : WindowControlViewModelBase
    {
        public ObservableCollection<ChatMessageViewModel> Messages { get; private set; } = new ObservableCollection<ChatMessageViewModel>();

        public IEnumerable<ChatCommand> ContextMenuChatCommands { get { return ChannelSession.Services.Chat.ChatMenuCommands; } }

        public event EventHandler ContextMenuCommandsChanged = delegate { };

        public AlertsDashboardControlViewModel(WindowViewModelBase windowViewModel)
            : base(windowViewModel)
        {
            GlobalEvents.OnAlertMessageReceived += GlobalEvents_OnAlertMessageReceived;
        }

        protected override async Task OnLoadedInternal()
        {
            await base.OnLoadedInternal();

            ChannelSession.Services.Chat.ChatCommandsReprocessed += Chat_ChatCommandsReprocessed;
        }

        private async void GlobalEvents_OnAlertMessageReceived(object sender, AlertChatMessageViewModel message)
        {
            await DispatcherHelper.InvokeDispatcher(() =>
            {
                try
                {
                    if (ChannelSession.Settings.LatestChatAtTop)
                    {
                        this.Messages.Insert(0, message);
                    }
                    else
                    {
                        this.Messages.Add(message);
                    }

                    if (this.Messages.Count > ChannelSession.Settings.MaxMessagesInChat)
                    {
                        ChatMessageViewModel removedMessage = (ChannelSession.Settings.LatestChatAtTop) ? this.Messages.Last() : this.Messages.First();
                        this.Messages.Remove(removedMessage);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
                return Task.FromResult(0);
            });
        }

        private void Chat_ChatCommandsReprocessed(object sender, EventArgs e)
        {
            this.ContextMenuCommandsChanged(this, new EventArgs());
        }
    }
}
