using Mixer.Base.Model.Interactive;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Interactive
{
    /// <summary>
    /// Interaction logic for RealmRoyaleDropMapInteractiveControl.xaml
    /// </summary>
    public partial class RealmRoyaleDropMapInteractiveControl : DropMapInterativeGameControl
    {
        public RealmRoyaleDropMapInteractiveControl(InteractiveGameModel game, InteractiveGameVersionModel version)
            : base(game, version)
        {
            InitializeComponent();

            this.Initialize(this.LocationPointsCanvas);

            this.MaxTimeTextBox.Text = this.maxTime.ToString();
            this.SparkCostTextBox.Text = this.sparkCost.ToString();
        }

        protected override void UpdateTimerUI(int timeLeft)
        {
            this.TimerTextBlock.Text = timeLeft.ToString();
            this.TimerStackPanel.Visibility = Visibility.Visible;
        }

        protected override string ComputeLocation(Point point)
        {
            return string.Format("{0} {1}", (char)((int)(point.X / 12.5) + 65), (int)(point.Y / 12.5) + 1);
        }

        protected override async Task UpdateWinnerUI(uint winner, string username, string location)
        {
            this.TimerStackPanel.Visibility = Visibility.Collapsed;

            this.DropLocationTextBlock.Text = location;
            await this.WinnerAvatar.SetUserAvatarUrl(winner);
            this.WinnerAvatar.SetSize(80);
            this.WinnerTextBlock.Text = username;

            this.DropLocationStackPanel.Visibility = Visibility.Visible;
            this.WinnerStackPanel.Visibility = Visibility.Visible;
        }

        protected override async Task GameConnectedInternal()
        {
            this.Dispatcher.Invoke(() =>
            {
                this.MaxTimeTextBox.IsEnabled = false;
                this.SparkCostTextBox.IsEnabled = false;

                this.TimerStackPanel.Visibility = Visibility.Collapsed;
                this.DropLocationStackPanel.Visibility = Visibility.Collapsed;
                this.WinnerStackPanel.Visibility = Visibility.Collapsed;

                this.TimerTextBlock.Text = string.Empty;
                this.DropLocationTextBlock.Text = string.Empty;
                this.WinnerAvatar.SetSize(80);
                this.WinnerTextBlock.Text = string.Empty;
            });

            await base.GameConnectedInternal();
        }

        protected override Task GameDisconnectedInternal()
        {
            this.MaxTimeTextBox.IsEnabled = true;
            this.SparkCostTextBox.IsEnabled = true;

            return Task.FromResult(0);
        }

        private void MaxTimeTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.MaxTimeTextBox.Text) && int.TryParse(this.MaxTimeTextBox.Text, out int time) && time > 0)
            {
                this.maxTime = time;
                this.SaveDropMapSettings();
            }
            else
            {
                this.MaxTimeTextBox.Text = "0";
            }
        }

        private void SparkCostTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.SparkCostTextBox.Text) && int.TryParse(this.SparkCostTextBox.Text, out int cost) && cost >= 0)
            {
                this.sparkCost = cost;
                this.SaveDropMapSettings();
            }
            else
            {
                this.SparkCostTextBox.Text = "0";
            }
        }
    }
}
