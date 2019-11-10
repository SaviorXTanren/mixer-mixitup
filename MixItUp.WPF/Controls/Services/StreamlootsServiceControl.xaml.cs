using MixItUp.Base;
using MixItUp.Base.Util;
using MixItUp.WPF.Util;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for StreamlootsServiceControl.xaml
    /// </summary>
    public partial class StreamlootsServiceControl : ServicesControlBase
    {
        private const string StreamlootsStreamURLFormat = "https://widgets.streamloots.com/alerts/";

        public StreamlootsServiceControl()
        {
            InitializeComponent();
        }

        protected override Task OnLoaded()
        {
            this.SetHeaderText("Streamloots");

            if (ChannelSession.Settings.StreamlootsOAuthToken != null)
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
            this.StreamlootsURLTextBox.IsEnabled = false;

            if (string.IsNullOrEmpty(this.StreamlootsURLTextBox.Text) && (!this.StreamlootsURLTextBox.Text.StartsWith(StreamlootsStreamURLFormat) || !int.TryParse(this.StreamlootsURLTextBox.Text, out int ID)))
            {
                await DialogHelper.ShowMessage("Please enter a valid Streamloots URL (" + StreamlootsStreamURLFormat + ").");
            }
            else
            {
                string streamlootsID = this.StreamlootsURLTextBox.Text;
                streamlootsID = streamlootsID.Replace(StreamlootsStreamURLFormat, "");

                await this.groupBoxControl.window.RunAsyncOperation(async () =>
                {
                    if (!await ChannelSession.Services.InitializeStreamloots(streamlootsID))
                    {
                        await DialogHelper.ShowMessage("Unable to authenticate with Streamloots. Please ensure you correctly input your Streamloots URL.");
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
            this.StreamlootsURLTextBox.IsEnabled = true;
        }

        private async void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                await ChannelSession.Services.DisconnectStreamloots();
            });
            ChannelSession.Settings.StreamlootsOAuthToken = null;

            this.ExistingAccountGrid.Visibility = Visibility.Collapsed;
            this.NewLoginGrid.Visibility = Visibility.Visible;

            this.SetCompletedIcon(visible: false);
        }
    }
}
