using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IAlertsService
    {
        ObservableCollection<AlertChatMessageViewModel> Alerts { get; }

        Task AddAlert(AlertChatMessageViewModel alert);
    }

    public class AlertsService : IAlertsService
    {
        public ObservableCollection<AlertChatMessageViewModel> Alerts { get; private set; } = new ObservableCollection<AlertChatMessageViewModel>().EnableSync();
        private LockedDictionary<string, AlertChatMessageViewModel> alertsLookup = new LockedDictionary<string, AlertChatMessageViewModel>();

        public async Task AddAlert(AlertChatMessageViewModel alert)
        {
            if (!string.IsNullOrEmpty(alert.Color))
            {
                await DispatcherHelper.InvokeDispatcher(() =>
                {
                    this.alertsLookup[alert.ID] = alert;
                    this.Alerts.Insert(0, alert);

                    if (this.Alerts.Count > ChannelSession.Settings.MaxMessagesInChat)
                    {
                        AlertChatMessageViewModel removedAlert = this.Alerts.Last();
                        this.alertsLookup.Remove(removedAlert.ID);
                        this.Alerts.Remove(removedAlert);
                    }

                    return Task.FromResult(0);
                });

                await ServiceManager.Get<ChatService>().WriteToChatEventLog(alert);

                if (!ChannelSession.Settings.OnlyShowAlertsInDashboard)
                {
                    await ServiceManager.Get<ChatService>().AddMessage(alert);
                }
            }
        }
    }
}
