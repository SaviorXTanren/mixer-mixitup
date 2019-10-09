using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Window;
using StreamingClient.Base.Util;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MixItUp.Base.ViewModel.Controls.Dashboard
{
    public class AlertsDashboardControlViewModel : WindowControlViewModelBase
    {
        public ObservableCollection<ChatMessageViewModel> Messages { get; private set; } = new ObservableCollection<ChatMessageViewModel>();

        public AlertsDashboardControlViewModel(WindowViewModelBase windowViewModel)
            : base(windowViewModel)
        {
            GlobalEvents.OnAlertMessageReceived += GlobalEvents_OnAlertMessageReceived;
        }

        private void GlobalEvents_OnAlertMessageReceived(object sender, AlertChatMessageViewModel message)
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
        }
    }
}
