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
    /// Interaction logic for IFTTTServiceControl.xaml
    /// </summary>
    public partial class IFTTTServiceControl : ServicesControlBase
    {
        public IFTTTServiceControl()
        {
            InitializeComponent();
        }

        protected override Task OnLoaded()
        {
            this.SetHeaderText("IFTTT");

            if (ChannelSession.Settings.IFTTTOAuthToken != null)
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
            this.IFTTTWebHookKeyTextBox.IsEnabled = false;

            if (string.IsNullOrEmpty(this.IFTTTWebHookKeyTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("No IFTTT Web Hook key was specified.");
            }
            else
            {
                await this.groupBoxControl.window.RunAsyncOperation(async () =>
                {
                    if (!await ChannelSession.Services.InitializeIFTTT(this.IFTTTWebHookKeyTextBox.Text))
                    {
                        await MessageBoxHelper.ShowMessageDialog("Unable to authenticate with IFTTT. Please ensure you correctly input your IFTTT Web Hook key.");
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
            this.IFTTTWebHookKeyTextBox.IsEnabled = true;
        }

        private async void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                await ChannelSession.Services.DisconnectIFTTT();
            });
            ChannelSession.Settings.IFTTTOAuthToken = null;

            this.ExistingAccountGrid.Visibility = Visibility.Collapsed;
            this.NewLoginGrid.Visibility = Visibility.Visible;

            this.SetCompletedIcon(visible: false);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            ProcessHelper.LaunchLink(e.Uri.AbsoluteUri);
        }
    }
}
