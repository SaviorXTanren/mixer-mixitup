using MixItUp.Base;
using MixItUp.WPF.Util;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for DeveloperAPIServiceControl.xaml
    /// </summary>
    public partial class DeveloperAPIServiceControl : ServicesControlBase
    {
        public DeveloperAPIServiceControl()
        {
            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            this.SetHeaderText("Developer API");

            if (ChannelSession.Settings.EnableDeveloperAPI)
            {
                this.EnableDeveloperAPIButton.Visibility = Visibility.Collapsed;
                this.DisableDeveloperAPIButton.Visibility = Visibility.Visible;

                this.SetCompletedIcon(visible: true);
            }

            await base.OnLoaded();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start("https://github.com/SaviorXTanren/mixer-mixitup/wiki/Developer-API");
        }

        private async void EnableDeveloperAPIButton_Click(object sender, RoutedEventArgs e)
        {
            await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                if (await ChannelSession.Services.InitializeDeveloperAPI())
                {
                    this.EnableDeveloperAPIButton.Visibility = Visibility.Collapsed;
                    this.DisableDeveloperAPIButton.Visibility = Visibility.Visible;

                    this.SetCompletedIcon(visible: true);

                    ChannelSession.Settings.EnableDeveloperAPI = true;
                }
                else
                {
                    await MessageBoxHelper.ShowMessageDialog("Could not enable the Developer APIs. Consider rebooting and trying again.");
                }
            });
        }

        private async void DisableDeveloperAPIButton_Click(object sender, RoutedEventArgs e)
        {
            await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                await ChannelSession.Services.DisconnectDeveloperAPI();

                ChannelSession.Settings.EnableDeveloperAPI = false;

                this.EnableDeveloperAPIButton.Visibility = Visibility.Visible;
                this.DisableDeveloperAPIButton.Visibility = Visibility.Collapsed;

                this.SetCompletedIcon(visible: false);
            });
        }
    }
}
