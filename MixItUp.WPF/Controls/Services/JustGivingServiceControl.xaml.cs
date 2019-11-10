using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.WPF.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for JustGivingServiceControl.xaml
    /// </summary>
    public partial class JustGivingServiceControl : ServicesControlBase
    {
        public JustGivingServiceControl()
        {
            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            this.SetHeaderText("JustGiving");

            if (ChannelSession.Settings.JustGivingOAuthToken != null)
            {
                this.ExistingAccountGrid.Visibility = Visibility.Visible;

                this.SetCompletedIcon(visible: true);

                await this.LoadFundraisers();
                if (!string.IsNullOrEmpty(ChannelSession.Settings.JustGivingPageShortName))
                {
                    IEnumerable<JustGivingFundraiser> fundraisers = (IEnumerable<JustGivingFundraiser>)this.FundraiserComboBox.ItemsSource;
                    if (fundraisers != null && fundraisers.Count() > 0)
                    {
                        this.FundraiserComboBox.SelectedItem = fundraisers.FirstOrDefault(f => f.pageShortName.Equals(ChannelSession.Settings.JustGivingPageShortName));
                    }
                }
            }
            else
            {
                this.NewLoginGrid.Visibility = Visibility.Visible;
            }

            await base.OnLoaded();
        }

        private async void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            bool result = await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                return await ChannelSession.Services.InitializeJustGiving();
            });

            if (!result)
            {
                await MessageBoxHelper.ShowMessageDialog("Unable to authenticate with JustGiving. Please ensure you approved access for the application in a timely manner.");
            }
            else
            {
                this.NewLoginGrid.Visibility = Visibility.Collapsed;
                this.ExistingAccountGrid.Visibility = Visibility.Visible;

                this.SetCompletedIcon(visible: true);

                await this.LoadFundraisers();
            }
        }

        private async void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                await ChannelSession.Services.DisconnectJustGiving();
            });
            ChannelSession.Settings.JustGivingOAuthToken = null;

            this.ExistingAccountGrid.Visibility = Visibility.Collapsed;
            this.NewLoginGrid.Visibility = Visibility.Visible;

            this.SetCompletedIcon(visible: false);
        }

        private async Task LoadFundraisers()
        {
            if (ChannelSession.Services.JustGiving != null)
            {
                IEnumerable<JustGivingFundraiser> fundraisers = await ChannelSession.Services.JustGiving.GetCurrentFundraisers();
                if (fundraisers != null)
                {
                    this.FundraiserComboBox.ItemsSource = fundraisers;
                }
            }
        }

        private void FundraiserComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.FundraiserComboBox.SelectedIndex >= 0)
            {
                JustGivingFundraiser fundraiser = (JustGivingFundraiser)this.FundraiserComboBox.SelectedItem;
                if (fundraiser != null)
                {
                    ChannelSession.Settings.JustGivingPageShortName = fundraiser.pageShortName;
                }
            }
        }
    }
}
