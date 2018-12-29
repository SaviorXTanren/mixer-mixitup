using MixItUp.Base;
using MixItUp.WPF.Util;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for TreatStreamServiceControl.xaml
    /// </summary>
    public partial class TreatStreamServiceControl : ServicesControlBase
    {
        public TreatStreamServiceControl()
        {
            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            this.SetHeaderText("TreatStream");

            if (ChannelSession.Settings.TreatStreamOAuthToken != null)
            {
                this.ExistingAccountGrid.Visibility = Visibility.Visible;

                this.SetCompletedIcon(visible: true);
            }
            else
            {
                this.NewLoginGrid.Visibility = Visibility.Visible;
            }

            await base.OnLoaded();
        }

        private async void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                await ChannelSession.Services.InitializeTreatStream();
            });

            if (ChannelSession.Services.TreatStream == null)
            {
                await MessageBoxHelper.ShowMessageDialog("Unable to authenticate with TreatStream. Please ensure you approved access for the application in a timely manner.");
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
                await ChannelSession.Services.DisconnectTreatStream();
            });
            ChannelSession.Settings.TreatStreamOAuthToken = null;

            this.ExistingAccountGrid.Visibility = Visibility.Collapsed;
            this.NewLoginGrid.Visibility = Visibility.Visible;

            this.SetCompletedIcon(visible: false);
        }
    }
}
