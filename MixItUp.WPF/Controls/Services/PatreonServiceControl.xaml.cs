using MixItUp.Base;
using MixItUp.WPF.Util;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for PatreonServiceControl.xaml
    /// </summary>
    public partial class PatreonServiceControl : ServicesControlBase
    {
        public PatreonServiceControl()
        {
            InitializeComponent();
        }

        protected override Task OnLoaded()
        {
            this.SetHeaderText("Patreon");

            if (ChannelSession.Settings.PatreonOAuthToken != null)
            {
                this.ExistingAccountGrid.Visibility = Visibility.Visible;

                this.SetCompletedIcon(visible: true);
            }
            else
            {
                this.NewLoginGrid.Visibility = Visibility.Visible;
            }

            return base.OnLoaded();
        }

        private async void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            bool result = await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                return await ChannelSession.Services.InitializePatreon();
            });

            if (!result)
            {
                await MessageBoxHelper.ShowMessageDialog("Unable to authenticate with Patreon. Please ensure you approved access for the application in a timely manner.");
            }
            else
            {
                this.NewLoginGrid.Visibility = Visibility.Collapsed;
                this.ExistingAccountGrid.Visibility = Visibility.Visible;

                this.SetCompletedIcon(visible: true);
            }
        }

        private async void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                await ChannelSession.Services.DisconnectPatreon();
            });
            ChannelSession.Settings.PatreonOAuthToken = null;

            this.ExistingAccountGrid.Visibility = Visibility.Collapsed;
            this.NewLoginGrid.Visibility = Visibility.Visible;

            this.SetCompletedIcon(visible: false);
        }
    }
}
