using MixItUp.Base;
using MixItUp.WPF.Util;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for GawkBoxServiceControl.xaml
    /// </summary>
    public partial class GawkBoxServiceControl : ServicesControlBase
    {
        private const string GawkBoxStreamURLFormat = "https://stream.gawkbox.com/stream/";

        public GawkBoxServiceControl()
        {
            InitializeComponent();
        }

        protected override Task OnLoaded()
        {
            this.SetHeaderText("GawkBox");

            if (ChannelSession.Settings.GawkBoxOAuthToken != null)
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
            this.GawkBoxURLTextBox.IsEnabled = false;

            if (string.IsNullOrEmpty(this.GawkBoxURLTextBox.Text) && (!this.GawkBoxURLTextBox.Text.StartsWith(GawkBoxStreamURLFormat) || !int.TryParse(this.GawkBoxURLTextBox.Text, out int ID)))
            {
                await MessageBoxHelper.ShowMessageDialog("Please enter a valid GawkBox URL (" + GawkBoxStreamURLFormat + ") or your GawkBox ID.");
            }
            else
            {
                string gawkBoxID = this.GawkBoxURLTextBox.Text;
                gawkBoxID = gawkBoxID.Replace(GawkBoxStreamURLFormat, "");

                await this.groupBoxControl.window.RunAsyncOperation(async () =>
                {
                    if (!await ChannelSession.Services.InitializeGawkBox(gawkBoxID))
                    {
                        await MessageBoxHelper.ShowMessageDialog("Unable to authenticate with GawkBox. Please ensure you correctly input your GawkBox URL/ID.");
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
            this.GawkBoxURLTextBox.IsEnabled = true;
        }

        private async void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                await ChannelSession.Services.DisconnectGawkBox();
            });
            ChannelSession.Settings.GawkBoxOAuthToken = null;

            this.ExistingAccountGrid.Visibility = Visibility.Collapsed;
            this.NewLoginGrid.Visibility = Visibility.Visible;

            this.SetCompletedIcon(visible: false);
        }
    }
}
