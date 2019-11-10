using MixItUp.Base;
using MixItUp.Base.Util;
using MixItUp.WPF.Util;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for StreamlabsOBSServiceControl.xaml
    /// </summary>
    public partial class StreamlabsOBSServiceControl : ServicesControlBase
    {
        public StreamlabsOBSServiceControl()
        {
            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            this.SetHeaderText("Streamlabs OBS");

            if (ChannelSession.Settings.EnableStreamlabsOBSConnection)
            {
                this.EnableStreamlabsOBSConnectionButton.Visibility = Visibility.Collapsed;
                this.DisableStreamlabsOBSConnectionButton.Visibility = Visibility.Visible;

                this.TestStreamlabsOBSConnectionButton.IsEnabled = true;

                this.SetCompletedIcon(visible: true);
            }

            await base.OnLoaded();
        }

        private async void EnableStreamlabsOBSConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                if (await ChannelSession.Services.InitializeStreamlabsOBSService())
                {
                    ChannelSession.Settings.EnableStreamlabsOBSConnection = true;
                    await ChannelSession.SaveSettings();

                    this.EnableStreamlabsOBSConnectionButton.Visibility = Visibility.Collapsed;
                    this.DisableStreamlabsOBSConnectionButton.Visibility = Visibility.Visible;
                    this.TestStreamlabsOBSConnectionButton.IsEnabled = true;
                }
                else
                {
                    await DialogHelper.ShowMessage("Streamlabs OBS service failed to start, please ensure Streamlabs OBS is currently running. If it continues to fail to connect, try running Mix It Up as Administrator.");
                }
            });
        }

        private async void DisableStreamlabsOBSConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                await ChannelSession.Services.DisconnectStreamlabsOBSService();
                ChannelSession.Settings.EnableStreamlabsOBSConnection = false;
                await ChannelSession.SaveSettings();

                this.EnableStreamlabsOBSConnectionButton.Visibility = Visibility.Visible;
                this.DisableStreamlabsOBSConnectionButton.Visibility = Visibility.Collapsed;
                this.TestStreamlabsOBSConnectionButton.IsEnabled = false;
            });
        }

        private async void TestStreamlabsOBSConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (ChannelSession.Services.StreamlabsOBSService != null)
            {
                await this.groupBoxControl.window.RunAsyncOperation(async () =>
                {
                    if (await ChannelSession.Services.StreamlabsOBSService.TestConnection())
                    {
                        await DialogHelper.ShowMessage("Streamlabs OBS connection test successful!");
                    }
                    else
                    {
                        await DialogHelper.ShowMessage("Streamlabs OBS connection test failed, please ensure that Streamlabs OBS is running.");
                    }
                });
            }
        }
    }
}
