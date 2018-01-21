using MixItUp.Base;
using MixItUp.Base.Util;
using MixItUp.WPF.Util;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for OverlayServiceControl.xaml
    /// </summary>
    public partial class OverlayServiceControl : ServicesControlBase
    {
        public OverlayServiceControl()
        {
            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            this.SetHeaderText("Mix It Up Overlay");

            this.OverlaySourceRefreshTextBox.Text = ChannelSession.Settings.OverlaySourceName;

            if (ChannelSession.Settings.EnableOverlay)
            {
                ChannelSession.Services.OverlayServer.Disconnected += OverlayServer_Disconnected;
                this.EnableOverlayButton.Visibility = Visibility.Collapsed;
                this.DisableOverlayButton.Visibility = Visibility.Visible;
                this.TestOverlayButton.IsEnabled = true;

                this.SetCompletedIcon(visible: true);
            }

            await base.OnLoaded();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(e.Uri.AbsoluteUri);
                e.Handled = true;
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        private async void EnableOverlayButton_Click(object sender, RoutedEventArgs e)
        {
            await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                await this.ConnectOverlayService();
                await ChannelSession.SaveSettings();
            });
        }

        private async void DisableOverlayButton_Click(object sender, RoutedEventArgs e)
        {
            await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                await this.DisconnectOverlayService();
                await ChannelSession.SaveSettings();
            });
        }

        private async void TestOverlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (ChannelSession.Services.OverlayServer != null)
            {
                await this.groupBoxControl.window.RunAsyncOperation(async () =>
                {
                    if (await ChannelSession.Services.OverlayServer.TestConnection())
                    {
                        await MessageBoxHelper.ShowMessageDialog("Overlay connection test successful!");
                    }
                    else
                    {
                        string message = "Overlay connection test failed, please ensure you have the Mix It Up Overlay page visible and running in your streaming software.";
                        message += Environment.NewLine + Environment.NewLine;
                        message += "If you launched your streaming software before Mix It Up, try refreshing the webpage source in your streaming software.";
                        await MessageBoxHelper.ShowMessageDialog(message);
                    }
                });
            }
        }

        private async void OverlayServer_Disconnected(object sender, System.EventArgs e)
        {
            await this.Dispatcher.Invoke<Task>(async () =>
            {
                ChannelSession.DisconnectionOccurred();

                await this.DisconnectOverlayService();
                await this.ConnectOverlayService();

                ChannelSession.ReconnectionOccurred();
            });
        }

        private async Task ConnectOverlayService()
        {
            await ChannelSession.Services.InitializeOverlayServer();
            ChannelSession.Services.OverlayServer.Disconnected += OverlayServer_Disconnected;

            ChannelSession.Settings.EnableOverlay = true;

            this.EnableOverlayButton.Visibility = Visibility.Collapsed;
            this.DisableOverlayButton.Visibility = Visibility.Visible;
            this.TestOverlayButton.IsEnabled = true;

            this.SetCompletedIcon(visible: true);
        }

        private async Task DisconnectOverlayService()
        {
            this.EnableOverlayButton.Visibility = Visibility.Visible;
            this.DisableOverlayButton.Visibility = Visibility.Collapsed;
            this.TestOverlayButton.IsEnabled = false;

            ChannelSession.Services.OverlayServer.Disconnected -= OverlayServer_Disconnected;
            await ChannelSession.Services.DisconnectOverlayServer();

            ChannelSession.Settings.EnableOverlay = false;

            this.SetCompletedIcon(visible: false);
        }

        private void OverlaySourceRefreshTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ChannelSession.Settings.OverlaySourceName = this.OverlaySourceRefreshTextBox.Text;
        }
    }
}
