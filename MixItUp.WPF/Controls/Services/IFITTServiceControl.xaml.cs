using MixItUp.Base;
using MixItUp.WPF.Util;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for IFITTServiceControl.xaml
    /// </summary>
    public partial class IFITTServiceControl : ServicesControlBase
    {
        public IFITTServiceControl()
        {
            InitializeComponent();
        }

        protected override Task OnLoaded()
        {
            this.SetHeaderText("IFITT");

            if (ChannelSession.Settings.IFITTOAuthToken != null)
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
            this.IFITTWebHookKeyTextBox.IsEnabled = false;

            if (string.IsNullOrEmpty(this.IFITTWebHookKeyTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("No IFITT Web Hook key was specified.");
            }
            else
            {
                await this.groupBoxControl.window.RunAsyncOperation(async () =>
                {
                    if (!await ChannelSession.Services.InitializeIFITT(this.IFITTWebHookKeyTextBox.Text))
                    {
                        await MessageBoxHelper.ShowMessageDialog("Unable to authenticate with IFITT. Please ensure you correctly input your IFITT Web Hook key.");
                    }
                    else
                    {
                        this.NewLoginGrid.Visibility = Visibility.Collapsed;
                        this.ExistingAccountGrid.Visibility = Visibility.Visible;

                        this.SetCompletedIcon(visible: true);
                    }
                });
            }

            this.LogInButton.IsEnabled = true;
            this.IFITTWebHookKeyTextBox.IsEnabled = true;
        }

        private async void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                await ChannelSession.Services.DisconnectIFITT();
            });
            ChannelSession.Settings.IFITTOAuthToken = null;

            this.ExistingAccountGrid.Visibility = Visibility.Collapsed;
            this.NewLoginGrid.Visibility = Visibility.Visible;

            this.SetCompletedIcon(visible: false);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }
    }
}
