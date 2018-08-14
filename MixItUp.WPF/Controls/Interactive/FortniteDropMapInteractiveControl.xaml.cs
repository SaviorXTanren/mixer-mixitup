using Mixer.Base.Model.Interactive;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Interactive
{
    /// <summary>
    /// Interaction logic for FortniteDropMapInteractiveControl.xaml
    /// </summary>
    public partial class FortniteDropMapInteractiveControl : DropMapInterativeGameControl
    {
        public FortniteDropMapInteractiveControl(InteractiveGameModel game, InteractiveGameVersionModel version)
            : base(game, version)
        {
            InitializeComponent();

            this.MaxTimeTextBox.Text = "30";
        }

        protected override void UpdateTimerUI(int timeLeft)
        {
            this.TimerTextBlock.Text = timeLeft.ToString();
            this.TimerStackPanel.Visibility = Visibility.Visible;
        }

        protected override string ComputeLocation(Point point)
        {
            return string.Format("{0} {1}", (char)((int)(point.X / 10.0) + 65), (int)(point.Y / 10.0) + 1);
        }

        protected override async Task UpdateWinnerUI(uint winner, string username, string location)
        {
            this.TimerStackPanel.Visibility = Visibility.Collapsed;

            this.DropLocationTextBlock.Text = location;
            await this.WinnerAvatar.SetImageUrl("https://mixer.com/api/v1/users/" + winner + "/avatar");
            this.WinnerAvatar.SetSize(80);
            this.WinnerTextBlock.Text = username;

            this.DropLocationStackPanel.Visibility = Visibility.Visible;
            this.WinnerStackPanel.Visibility = Visibility.Visible;
        }

        protected override Task OnLoaded()
        {
            this.Initialize(this.LocationPointsCanvas);
            return Task.FromResult(0);
        }

        protected override async Task GameConnectedInternal()
        {
            this.MaxTimeTextBox.IsEnabled = false;

            this.TimerStackPanel.Visibility = Visibility.Collapsed;
            this.DropLocationStackPanel.Visibility = Visibility.Collapsed;
            this.WinnerStackPanel.Visibility = Visibility.Collapsed;

            this.TimerTextBlock.Text = string.Empty;
            this.DropLocationTextBlock.Text = string.Empty;
            this.WinnerAvatar.SetSize(80);
            this.WinnerTextBlock.Text = string.Empty;

            await base.GameConnectedInternal();
        }

        protected override Task GameDisconnectedInternal()
        {
            this.MaxTimeTextBox.IsEnabled = true;

            return Task.FromResult(0);
        }

        private void MaxTimeTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.MaxTimeTextBox.Text) && int.TryParse(this.MaxTimeTextBox.Text, out int time) && time > 0)
            {
                this.maxTime = time;
            }
            else
            {
                this.MaxTimeTextBox.Text = "0";
            }
        }
    }
}
