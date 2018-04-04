using MixItUp.Base;
using MixItUp.WPF.Util;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for SpotifyServiceControl.xaml
    /// </summary>
    public partial class SpotifyServiceControl : ServicesControlBase
    {
        public SpotifyServiceControl()
        {
            InitializeComponent();
        }

        protected override Task OnLoaded()
        {
            this.SetHeaderText("Spotify");

            if (ChannelSession.Settings.SpotifyOAuthToken != null)
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
                return await ChannelSession.Services.InitializeSpotify();
            });

            if (!result)
            {
                await MessageBoxHelper.ShowMessageDialog("Unable to authenticate with Spotify. Please ensure you approved access for the application in a timely manner.");
            }
            else
            {
                await this.groupBoxControl.window.RunAsyncOperation(async () =>
                {
                    if (ChannelSession.Services.Spotify.Profile == null)
                    {
                        await MessageBoxHelper.ShowMessageDialog("We were unable to get your user data, please try to authenticate again with Spotify.");
                        await ChannelSession.Services.DisconnectSpotify();
                    }
                    else if (!ChannelSession.Services.Spotify.Profile.IsPremium)
                    {
                        await MessageBoxHelper.ShowMessageDialog("You do not have Spotify Premium, which is required for this feature.");
                        await ChannelSession.Services.DisconnectSpotify();
                    }
                    else
                    {
                        this.NewLoginGrid.Visibility = Visibility.Collapsed;
                        this.ExistingAccountGrid.Visibility = Visibility.Visible;

                        this.SetCompletedIcon(visible: true);
                    }
                });
            }
        }

        private async void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                await ChannelSession.Services.DisconnectSpotify();
            });
            ChannelSession.Settings.SpotifyOAuthToken = null;

            this.ExistingAccountGrid.Visibility = Visibility.Collapsed;
            this.NewLoginGrid.Visibility = Visibility.Visible;

            this.SetCompletedIcon(visible: false);
        }
    }
}
