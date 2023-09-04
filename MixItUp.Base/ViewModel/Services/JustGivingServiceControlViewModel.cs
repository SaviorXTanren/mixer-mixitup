using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Services
{
    public class JustGivingServiceControlViewModel : ServiceControlViewModelBase
    {
        public JustGivingFundraiser Fundraiser
        {
            get
            {
                if (ServiceManager.Get<JustGivingService>().IsConnected)
                {
                    return ServiceManager.Get<JustGivingService>().Fundraiser;
                }
                return null;
            }
        }

        public string WebPageURL
        {
            get { return this.webPageURL; }
            set
            {
                this.webPageURL = value;
                this.NotifyPropertyChanged();
            }
        }
        private string webPageURL;

        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }

        public override string WikiPageName { get { return "justgiving"; } }

        public JustGivingServiceControlViewModel()
            : base(Resources.JustGiving)
        {
            this.LogInCommand = this.CreateCommand(async () =>
            {
                ChannelSession.Settings.JustGivingPageShortName = null;
                if (!string.IsNullOrEmpty(this.WebPageURL))
                {
                    ChannelSession.Settings.JustGivingPageShortName = this.WebPageURL;
                    ChannelSession.Settings.JustGivingPageShortName = ChannelSession.Settings.JustGivingPageShortName.Replace("https://www.justgiving.com/", string.Empty);
                    ChannelSession.Settings.JustGivingPageShortName = ChannelSession.Settings.JustGivingPageShortName.Replace("https://www.justgiving.com/fundraising/", string.Empty);
                    int urlParametersIndex = ChannelSession.Settings.JustGivingPageShortName.IndexOf("?");
                    if (urlParametersIndex > 0)
                    {
                        ChannelSession.Settings.JustGivingPageShortName = ChannelSession.Settings.JustGivingPageShortName.Substring(0, urlParametersIndex);
                    }

                    Result result = await ServiceManager.Get<JustGivingService>().Connect();
                    if (result.Success)
                    {
                        this.IsConnected = true;
                        this.NotifyPropertyChanged(nameof(this.Fundraiser));
                    }
                    else
                    {
                        await this.ShowConnectFailureMessage(result);
                        ChannelSession.Settings.JustGivingPageShortName = null;
                    }
                }
            });

            this.LogOutCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<JustGivingService>().Disconnect();

                ChannelSession.Settings.JustGivingPageShortName = null;

                this.NotifyPropertyChanged(nameof(this.Fundraiser));

                this.IsConnected = false;
            });

            this.IsConnected = ServiceManager.Get<JustGivingService>().IsConnected;
        }

        protected override Task OnOpenInternal()
        {
            if (this.IsConnected)
            {
                this.NotifyPropertyChanged(nameof(this.Fundraiser));
            }
            return Task.CompletedTask;
        }
    }
}
