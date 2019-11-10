using MixItUp.Base;
using MixItUp.Base.Util;
using MixItUp.WPF.Util;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for TwitterServiceControl.xaml
    /// </summary>
    public partial class TwitterServiceControl : ServicesControlBase
    {
        public TwitterServiceControl()
        {
            InitializeComponent();
        }

        protected override Task OnLoaded()
        {
            this.SetHeaderText("Twitter");

            if (ChannelSession.Settings.TwitterOAuthToken != null)
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
            this.LogInButton.IsEnabled = false;
            this.AuthorizationPinTextBox.IsEnabled = true;
            this.AuthorizePinButton.IsEnabled = true;

            bool result = await ChannelSession.Services.InitializeTwitter();

            this.LogInButton.IsEnabled = true;
            this.AuthorizationPinTextBox.IsEnabled = false;
            this.AuthorizationPinTextBox.Clear();
            this.AuthorizePinButton.IsEnabled = false;

            if (!result)
            {
                await DialogHelper.ShowMessage("Unable to authenticate with Twitter. Please ensure you approved access for the application in a timely manner.");
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
                await ChannelSession.Services.DisconnectTwitter();
            });
            ChannelSession.Settings.TwitterOAuthToken = null;

            this.ExistingAccountGrid.Visibility = Visibility.Collapsed;
            this.NewLoginGrid.Visibility = Visibility.Visible;

            this.SetCompletedIcon(visible: false);
        }

        private void AuthorizePinButton_Click(object sender, RoutedEventArgs e)
        {
            if (ChannelSession.Services.Twitter != null && !string.IsNullOrEmpty(this.AuthorizationPinTextBox.Text))
            {
                ChannelSession.Services.Twitter.SetAuthPin(this.AuthorizationPinTextBox.Text);
            }
        }
    }
}
