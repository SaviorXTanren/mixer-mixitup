using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Dashboard
{
    public class AlertsDashboardControlViewModel : WindowControlViewModelBase
    {
        public ThreadSafeObservableCollection<AlertChatMessageViewModel> Messages { get; private set; } = new ThreadSafeObservableCollection<AlertChatMessageViewModel>();

        public IEnumerable<CommandModelBase> ContextMenuChatCommands { get { return ServiceManager.Get<ChatService>().ChatMenuCommands.ToList(); } }

        public event EventHandler ContextMenuCommandsChanged = delegate { };

        public AlertsDashboardControlViewModel(UIViewModelBase windowViewModel) : base(windowViewModel) { }

        protected override async Task OnOpenInternal()
        {
            await base.OnOpenInternal();

            this.Messages = ServiceManager.Get<AlertsService>().Alerts;
            ServiceManager.Get<ChatService>().ChatCommandsReprocessed += Chat_ChatCommandsReprocessed;
        }

        private void Chat_ChatCommandsReprocessed(object sender, EventArgs e)
        {
            this.ContextMenuCommandsChanged(this, new EventArgs());
        }
    }
}
