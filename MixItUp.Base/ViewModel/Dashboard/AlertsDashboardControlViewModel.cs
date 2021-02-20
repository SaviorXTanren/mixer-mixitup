using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Dashboard
{
    public class AlertsDashboardControlViewModel : WindowControlViewModelBase
    {
        public ObservableCollection<AlertChatMessageViewModel> Messages { get; private set; } = new ObservableCollection<AlertChatMessageViewModel>();

        public IEnumerable<CommandModelBase> ContextMenuChatCommands { get { return ChannelSession.Services.Chat.ChatMenuCommands.ToList(); } }

        public event EventHandler ContextMenuCommandsChanged = delegate { };

        public AlertsDashboardControlViewModel(UIViewModelBase windowViewModel) : base(windowViewModel) { }

        protected override async Task OnLoadedInternal()
        {
            await base.OnLoadedInternal();

            this.Messages = ChannelSession.Services.Alerts.Alerts;
            ChannelSession.Services.Chat.ChatCommandsReprocessed += Chat_ChatCommandsReprocessed;
        }

        private void Chat_ChatCommandsReprocessed(object sender, EventArgs e)
        {
            this.ContextMenuCommandsChanged(this, new EventArgs());
        }
    }
}
