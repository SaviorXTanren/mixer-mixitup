using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Services
{
    public class JustGivingServiceControlViewModel : ServiceControlViewModelBase
    {
        public ObservableCollection<JustGivingFundraiser> Fundraisers { get; set; } = new ObservableCollection<JustGivingFundraiser>();

        public JustGivingFundraiser SelectedFundraiser
        {
            get { return this.selectedFundraiser; }
            set
            {
                this.selectedFundraiser = value;
                this.NotifyPropertyChanged();

                if (this.SelectedFundraiser != null)
                {
                    ChannelSession.Settings.JustGivingPageShortName = this.SelectedFundraiser.pageShortName;
                }
                else
                {
                    ChannelSession.Settings.JustGivingPageShortName = null;
                }
            }
        }
        private JustGivingFundraiser selectedFundraiser;

        public ICommand LogInCommand { get; set; }
        public ICommand LogOutCommand { get; set; }

        public JustGivingServiceControlViewModel()
            : base("JustGiving")
        {
            this.LogInCommand = this.CreateCommand(async (parameter) =>
            {
                Result result = await ChannelSession.Services.JustGiving.Connect();
                if (result.Success)
                {
                    this.IsConnected = true;
                    await this.RefreshFundraisers();
                }
                else
                {
                    await this.ShowConnectFailureMessage(result);
                }
            });

            this.LogOutCommand = this.CreateCommand(async (parameter) =>
            {
                await ChannelSession.Services.JustGiving.Disconnect();

                ChannelSession.Settings.JustGivingOAuthToken = null;
                ChannelSession.Settings.JustGivingPageShortName = null;

                this.IsConnected = false;
            });

            this.IsConnected = ChannelSession.Services.JustGiving.IsConnected;
        }

        protected override async Task OnLoadedInternal()
        {
            if (this.IsConnected)
            {
                await this.RefreshFundraisers();
                this.SelectedFundraiser = this.Fundraisers.FirstOrDefault(f => f.pageShortName.Equals(ChannelSession.Settings.JustGivingPageShortName));
            }
        }

        public async Task RefreshFundraisers()
        {
            this.Fundraisers.Clear();

            IEnumerable<JustGivingFundraiser> fundraisers = await ChannelSession.Services.JustGiving.GetCurrentFundraisers();
            if (fundraisers != null)
            {
                foreach (JustGivingFundraiser fundraiser in fundraisers)
                {
                    if (fundraiser.IsActive)
                    {
                        this.Fundraisers.Add(fundraiser);
                    }
                }
            }
        }
    }
}
