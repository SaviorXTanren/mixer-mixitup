using MixItUp.Base;
using MixItUp.WPF.Util;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for OBSStudioServiceControl.xaml
    /// </summary>
    public partial class OBSStudioServiceControl : ServicesControlBase
    {
        public OBSStudioServiceControl()
        {
            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            this.SetHeaderText("OBS Studio");

            if (!string.IsNullOrEmpty(ChannelSession.Settings.OBSStudioServerIP))
            {
                await ChannelSession.Services.InitializeOBSWebsocket();

                this.OBSStudioIPAddressTextBox.Text = ChannelSession.Settings.OBSStudioServerIP;
                this.OBSStudioPasswordTextBox.Password = ChannelSession.Settings.OBSStudioServerPassword;

                this.OBSStudioEnableConnectionButton.Visibility = Visibility.Collapsed;
                this.OBSStudioDisableConnectionButton.Visibility = Visibility.Visible;
            }
            else
            {
                this.OBSStudioIPAddressTextBox.Text = ChannelSession.DefaultOBSStudioConnection;
            }

            await base.OnLoaded();
        }

        private async void OBSStudioEnableConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.OBSStudioServerIP = this.OBSStudioIPAddressTextBox.Text;
            ChannelSession.Settings.OBSStudioServerPassword = this.OBSStudioPasswordTextBox.Password;

            await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                if (await ChannelSession.Services.InitializeOBSWebsocket())
                {
                    this.OBSStudioEnableConnectionButton.Visibility = Visibility.Collapsed;
                    this.OBSStudioDisableConnectionButton.Visibility = Visibility.Visible;
                }
                else
                {
                    ChannelSession.Settings.OBSStudioServerIP = null;
                    ChannelSession.Settings.OBSStudioServerPassword = null;

                    await MessageBoxHelper.ShowMessageDialog("Could not connect to OBS Studio. Please make sure OBS Studio is running, the obs-websocket plugin is installed, and the connection and password match your settings in OBS Studio");
                }
            });
        }

        private async void OBSStudioDisableConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            await ChannelSession.Services.DisconnectOBSStudio();

            ChannelSession.Settings.OBSStudioServerIP = null;
            ChannelSession.Settings.OBSStudioServerPassword = null;

            this.OBSStudioEnableConnectionButton.Visibility = Visibility.Visible;
            this.OBSStudioDisableConnectionButton.Visibility = Visibility.Collapsed;
        }
    }
}
