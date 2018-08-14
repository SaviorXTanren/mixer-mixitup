using Mixer.Base.Model.Interactive;
using MixItUp.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Interactive
{
    public class PUBGMap
    {
        public string Name { get; set; }
        public string Map { get; set; }
    }

    /// <summary>
    /// Interaction logic for PUBGDropMapInteractiveControl.xaml
    /// </summary>
    public partial class PUBGDropMapInteractiveControl : DropMapInterativeGameControl
    {
        private const string ErangelMapName = "Erangel";
        private const string MiramarMapName = "Miramar";
        private const string SanhokMapName = "Sanhok";

        private ObservableCollection<PUBGMap> maps = new ObservableCollection<PUBGMap>();

        public PUBGDropMapInteractiveControl(InteractiveGameModel game, InteractiveGameVersionModel version)
            : base(game, version)
        {
            InitializeComponent();

            this.MapComboBox.ItemsSource = this.maps;

            this.maps.Add(new PUBGMap() { Name = ErangelMapName, Map = "map1.png" });
            this.maps.Add(new PUBGMap() { Name = MiramarMapName, Map = "map2.png" });
            this.maps.Add(new PUBGMap() { Name = SanhokMapName, Map = "map3.png" });

            this.MapComboBox.SelectedIndex = 0;

            this.MaxTimeTextBox.Text = "30";
        }

        protected override void UpdateTimerUI(int timeLeft)
        {
            this.TimerTextBlock.Text = timeLeft.ToString();
            this.TimerStackPanel.Visibility = Visibility.Visible;
        }

        protected override string ComputeLocation(Point point)
        {
            return string.Format("{0} {1}", (char)((int)(point.X / 12.5) + 65), (char)((int)(point.Y / 12.5) + 73));
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
            this.MapComboBox.IsEnabled = false;
            this.MaxTimeTextBox.IsEnabled = false;

            this.TimerStackPanel.Visibility = Visibility.Collapsed;
            this.DropLocationStackPanel.Visibility = Visibility.Collapsed;
            this.WinnerStackPanel.Visibility = Visibility.Collapsed;

            this.TimerTextBlock.Text = string.Empty;
            this.DropLocationTextBlock.Text = string.Empty;
            this.WinnerAvatar.SetSize(80);
            this.WinnerTextBlock.Text = string.Empty;

            await base.GameConnectedInternal();

            PUBGMap map = (PUBGMap)this.MapComboBox.SelectedItem;

            InteractiveControlModel control = new InteractiveControlModel() { controlID = this.positionButton.controlID };
            control.meta["map"] = map.Map;
            await ChannelSession.Interactive.UpdateControls(this.scene, new List<InteractiveControlModel>() { control });
        }

        protected override Task GameDisconnectedInternal()
        {
            this.MapComboBox.IsEnabled = true;
            this.MaxTimeTextBox.IsEnabled = true;

            return Task.FromResult(0);
        }

        private void MapComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.MapComboBox.SelectedIndex >= 0)
            {
                this.ErangelMap.Visibility = Visibility.Collapsed;
                this.MiramarMap.Visibility = Visibility.Collapsed;
                this.SanhokMap.Visibility = Visibility.Collapsed;

                PUBGMap map = (PUBGMap)this.MapComboBox.SelectedItem;
                if (map.Name.Equals(ErangelMapName))
                {
                    this.ErangelMap.Visibility = Visibility.Visible;
                }
                else if (map.Name.Equals(MiramarMapName))
                {
                    this.MiramarMap.Visibility = Visibility.Visible;
                }
                else if (map.Name.Equals(SanhokMapName))
                {
                    this.SanhokMap.Visibility = Visibility.Visible;
                }
            }
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
