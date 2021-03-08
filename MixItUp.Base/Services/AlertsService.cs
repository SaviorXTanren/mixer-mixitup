using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IAlertsService
    {
        ThreadSafeObservableCollection<AlertChatMessageViewModel> Alerts { get; }

        Task AddAlert(AlertChatMessageViewModel alert);
    }

    public class AlertsService : IAlertsService
    {
        public ThreadSafeObservableCollection<AlertChatMessageViewModel> Alerts { get; private set; } = new ThreadSafeObservableCollection<AlertChatMessageViewModel>();
        private LockedDictionary<string, AlertChatMessageViewModel> alertsLookup = new LockedDictionary<string, AlertChatMessageViewModel>();

        public async Task AddAlert(AlertChatMessageViewModel alert)
        {
            if (!string.IsNullOrEmpty(alert.Color))
            {
                this.alertsLookup[alert.ID] = alert;
                this.Alerts.Insert(0, alert);

                if (this.Alerts.Count > ChannelSession.Settings.MaxMessagesInChat)
                {
                    AlertChatMessageViewModel removedAlert = this.Alerts.Last();
                    this.alertsLookup.Remove(removedAlert.ID);
                    this.Alerts.Remove(removedAlert);
                }

                await ServiceManager.Get<ChatService>().WriteToChatEventLog(alert);

                if (!ChannelSession.Settings.OnlyShowAlertsInDashboard)
                {
                    await ServiceManager.Get<ChatService>().AddMessage(alert);
                }
            }
        }
    }
}
