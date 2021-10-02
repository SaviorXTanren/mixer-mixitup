using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Webhooks;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class WebhookCommandItemViewModel
    {
        public Webhook Webhook { get; set; }
        public WebhookCommandModel Command { get; set; }

        public string Name => Command?.Name ?? Webhook.Id.ToString();

        public WebhookCommandItemViewModel(Webhook webhook, WebhookCommandModel command)
        {
            this.Webhook = webhook;
            this.Command = command;
        }

        public bool IsNewCommand { get { return this.Command == null; } }

        public bool IsExistingCommand { get { return this.Command != null; } }
    }

    public class WebhooksMainControlViewModel : WindowControlViewModelBase
    {
        public ThreadSafeObservableCollection<WebhookCommandItemViewModel> WebhookCommands { get; set; } = new ThreadSafeObservableCollection<WebhookCommandItemViewModel>();

        public int MaxNumberOfWebhooks
        {
            get { return this.maxNumberOfWebhooks; }
            set
            {
                this.maxNumberOfWebhooks = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("CanCreateMoreWebhooks");
                this.NotifyPropertyChanged("AddButtonText");
            }
        }
        private int maxNumberOfWebhooks = 0;
        public bool CanCreateMoreWebhooks { get { return this.WebhookCommands.Count < this.MaxNumberOfWebhooks; } }

        public string AddButtonText => $"{Resources.AddWebhook} [{this.WebhookCommands.Count} / {this.MaxNumberOfWebhooks}]";

        public WebhooksMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
        }

        protected override async Task OnLoadedInternal()
        {
            await base.OnLoadedInternal();
            await RefreshCommands();
        }

        public async Task RefreshCommands()
        {
            try
            {
                this.WebhookCommands.Clear();

                GetWebhooksResponseModel response = await ChannelSession.Services.WebhookService.GetWebhooks();
                foreach (var webhook in response.Webhooks)
                {
                    var command = ChannelSession.Services.Command.WebhookCommands.FirstOrDefault(c => c.ID == webhook.Id);
                    this.WebhookCommands.Add(new WebhookCommandItemViewModel(webhook, command));
                }

                MaxNumberOfWebhooks = response.MaxNumberOfWebhooks;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }
}
