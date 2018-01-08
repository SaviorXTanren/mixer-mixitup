using MixItUp.Base;
using MixItUp.WPF.Util;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for XSplitServiceControl.xaml
    /// </summary>
    public partial class XSplitServiceControl : ServicesControlBase
    {
        public XSplitServiceControl()
        {
            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            this.SetHeaderText("XSplit");

            if (ChannelSession.Settings.EnableXSplitConnection)
            {
                await this.ConnectXSplitService();
            }

            await base.OnLoaded();
        }

        private async void EnableXSplitConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                await this.ConnectXSplitService();
                await ChannelSession.SaveSettings();
            });
        }

        private async void DisableXSplitConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                await this.DisconnectXSplitService();
                await ChannelSession.SaveSettings();
            });
        }

        private async void TestXSplitConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (ChannelSession.Services.XSplitServer != null)
            {
                await this.groupBoxControl.window.RunAsyncOperation(async () =>
                {
                    if (await ChannelSession.Services.XSplitServer.TestConnection())
                    {
                        await MessageBoxHelper.ShowMessageDialog("XSplit connection test successful!");
                    }
                    else
                    {
                        await MessageBoxHelper.ShowMessageDialog("XSplit connection test failed, please ensure you have the Mix It Up XSplit extension added and open in XSplit.");
                    }
                });
            }
        }

        private async void XSplitServer_Disconnected(object sender, System.EventArgs e)
        {
            await this.Dispatcher.Invoke<Task>(async () =>
            {
                await this.DisconnectXSplitService();
                await this.ConnectXSplitService();
            });
        }

        public async Task ConnectXSplitService()
        {
            ChannelSession.Settings.EnableXSplitConnection = true;
            this.EnableXSplitConnectionButton.Visibility = Visibility.Collapsed;
            this.DisableXSplitConnectionButton.Visibility = Visibility.Visible;

            await ChannelSession.Services.InitializeXSplitServer();
            ChannelSession.Services.XSplitServer.Disconnected += XSplitServer_Disconnected;

            this.TestXSplitConnectionButton.IsEnabled = true;
        }

        private async Task DisconnectXSplitService()
        {
            this.EnableXSplitConnectionButton.Visibility = Visibility.Visible;
            this.DisableXSplitConnectionButton.Visibility = Visibility.Collapsed;
            this.TestXSplitConnectionButton.IsEnabled = false;

            ChannelSession.Services.XSplitServer.Disconnected -= XSplitServer_Disconnected;
            await ChannelSession.Services.DisconnectXSplitServer();

            ChannelSession.Settings.EnableXSplitConnection = false;
        }
    }
}
