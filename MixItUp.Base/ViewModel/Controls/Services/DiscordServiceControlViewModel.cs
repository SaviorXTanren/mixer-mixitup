using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Services
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

        public DiscordServiceControlViewModel()
            : base("Discord")
        {
            this.LogInCommand = this.CreateCommand(async (parameter) =>
            {
                ChannelSession.Settings.DiscordCustomClientID = null;
                ChannelSession.Settings.DiscordCustomClientSecret = null;
                ChannelSession.Settings.DiscordCustomBotToken = null;

                if (this.CustomApplication)
                {
                    if (string.IsNullOrEmpty(this.CustomClientID))
                    {
                        await DialogHelper.ShowMessage("Please enter a valid Custom Client ID");
                        return;
                    }

                    if (string.IsNullOrEmpty(this.CustomClientSecret))
                    {
                        await DialogHelper.ShowMessage("Please enter a valid Custom Client Secret");
                        return;
                    }

                    if (string.IsNullOrEmpty(this.CustomBotToken))
                    {
                        await DialogHelper.ShowMessage("Please enter a valid Custom Bot Token");
                        return;
                    }

                    ChannelSession.Settings.DiscordCustomClientID = this.CustomClientID;
                    ChannelSession.Settings.DiscordCustomClientSecret = this.CustomClientSecret;
                    ChannelSession.Settings.DiscordCustomBotToken = this.CustomBotToken;
                }

                Result result = await ChannelSession.Services.Discord.Connect();
                if (result.Success)
                {
                    this.IsConnected = true;
                }
                else
                {
                    await this.ShowConnectFailureMessage(result);
                }
            });

            this.LogOutCommand = this.CreateCommand(async (parameter) =>
            {
                await ChannelSession.Services.Discord.Disconnect();

                ChannelSession.Settings.DiscordOAuthToken = null;
                ChannelSession.Settings.DiscordServer = null;
                ChannelSession.Settings.DiscordCustomClientID = null;
                ChannelSession.Settings.DiscordCustomClientSecret = null;
                ChannelSession.Settings.DiscordCustomBotToken = null;

                this.IsConnected = false;
            });

            this.IsConnected = ChannelSession.Services.Discord.IsConnected;
        }
    }
}
