using MixItUp.Base;
using MixItUp.Base.Util;
using MixItUp.WPF.Util;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for OvrStreamServiceControl.xaml
    /// </summary>
    public partial class OvrStreamServiceControl : ServicesControlBase
    {
        public OvrStreamServiceControl()
        {
            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            this.SetHeaderText("OvrStream");

            if (!string.IsNullOrEmpty(ChannelSession.Settings.OvrStreamServerIP))
            {
                this.OvrStreamIPAddressTextBox.Text = ChannelSession.Settings.OvrStreamServerIP;

                this.OvrStreamEnableConnectionButton.Visibility = Visibility.Collapsed;
                this.OvrStreamDisableConnectionButton.Visibility = Visibility.Visible;

                this.SetCompletedIcon(visible: true);
            }
            else
            {
                this.OvrStreamIPAddressTextBox.Text = ChannelSession.DefaultOvrStreamConnection;
            }

            await base.OnLoaded();
        }

        private async void OvrStreamEnableConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.OvrStreamServerIP = this.OvrStreamIPAddressTextBox.Text;

            await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                if (await ChannelSession.Services.InitializeOvrStream())
                {
                    this.OvrStreamEnableConnectionButton.Visibility = Visibility.Collapsed;
                    this.OvrStreamDisableConnectionButton.Visibility = Visibility.Visible;

                    this.SetCompletedIcon(visible: true);
                }
                else
                {
                    ChannelSession.Settings.OvrStreamServerIP = null;
                    await MessageBoxHelper.ShowMessageDialog("Could not connect to OvrStream. Please make sure OvrStream is running.");
                }
            });
        }

        private async void OvrStreamDisableConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            await ChannelSession.Services.DisconnectOvrStream();

            ChannelSession.Settings.OvrStreamServerIP = null;

            this.OvrStreamEnableConnectionButton.Visibility = Visibility.Visible;
            this.OvrStreamDisableConnectionButton.Visibility = Visibility.Collapsed;

            this.SetCompletedIcon(visible: false);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            ProcessHelper.LaunchLink(e.Uri.AbsoluteUri);
        }
    }
}
