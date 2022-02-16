using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class DiscordServiceControlViewModel : ServiceControlViewModelBase
    {
        public bool CustomApplication
        {
            get { return this.customApplication; }
            set
            {
                this.customApplication = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool customApplication;

        public string CustomClientID
        {
            get { return this.customClientID; }
            set
            {
                this.customClientID = value;
                this.NotifyPropertyChanged();
            }
        }
        private string customClientID;

        public string CustomClientSecret
        {
            get { return this.customClientSecret; }
            set
            {
                this.customClientSecret = value;
                this.NotifyPropertyChanged();
            }
        }
        private string customClientSecret;

        public string CustomBotToken
        {
            get { return this.customBotToken; }
            set
            {
                this.customBotToken = value;
                this.NotifyPropertyChanged();
            }
        }
        private string customBotToken;

        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }

        public override string WikiPageName { get { return "discord"; } }

        public DiscordServiceControlViewModel()
            : base(Resources.Discord)
        {
            this.LogInCommand = this.CreateCommand(async () =>
            {
                ChannelSession.Settings.DiscordCustomClientID = null;
                ChannelSession.Settings.DiscordCustomClientSecret = null;
                ChannelSession.Settings.DiscordCustomBotToken = null;

                if (this.CustomApplication)
                {
                    if (string.IsNullOrEmpty(this.CustomClientID))
                    {
                        await DialogHelper.ShowMessage(Resources.DiscordInvalidClientId);
                        return;
                    }

                    if (string.IsNullOrEmpty(this.CustomClientSecret))
                    {
                        await DialogHelper.ShowMessage(Resources.DiscordInvalidSecret);
                        return;
                    }

                    if (string.IsNullOrEmpty(this.CustomBotToken))
                    {
                        await DialogHelper.ShowMessage(Resources.DiscordInvalidBotToken);
                        return;
                    }

                    ChannelSession.Settings.DiscordCustomClientID = this.CustomClientID;
                    ChannelSession.Settings.DiscordCustomClientSecret = this.CustomClientSecret;
                    ChannelSession.Settings.DiscordCustomBotToken = this.CustomBotToken;
                }

                Result result = await ServiceManager.Get<DiscordService>().Connect();
                if (result.Success)
                {
                    this.IsConnected = true;
                }
                else
                {
                    await this.ShowConnectFailureMessage(result);
                }
            });

            this.LogOutCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<DiscordService>().Disconnect();

                ChannelSession.Settings.DiscordOAuthToken = null;
                ChannelSession.Settings.DiscordServer = null;
                ChannelSession.Settings.DiscordCustomClientID = null;
                ChannelSession.Settings.DiscordCustomClientSecret = null;
                ChannelSession.Settings.DiscordCustomBotToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<DiscordService>().IsConnected;
        }
    }
}
