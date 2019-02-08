using Mixer.Base.Model.Interactive;
using MixItUp.Base;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MixItUp.WPF.Controls.Interactive
{
    public class PUBGMap
    {
        public string Name { get; set; }
        public string Map { get; set; }
    }

    /// <summary>
    /// Interaction logic for DropMapInteractiveControl.xaml
    /// </summary>
    public partial class DropMapInteractiveControl : DropMapInterativeGameControl
    {
        private const string MapSelectionSettingProperty = "MapSelection";

        private const string ErangelMapName = "Erangel";
        private const string MiramarMapName = "Miramar";
        private const string SanhokMapName = "Sanhok";
        private const string VikendiMapName = "Vikendi";

        private ObservableCollection<PUBGMap> maps = new ObservableCollection<PUBGMap>();

        public DropMapInteractiveControl(DropMapTypeEnum dropMapType, InteractiveGameModel game, InteractiveGameVersionModel version)
            : base(game, version)
        {
            InitializeComponent();

            this.MapComboBox.ItemsSource = this.maps;

            this.maps.Add(new PUBGMap() { Name = ErangelMapName, Map = "map1.png" });
            this.maps.Add(new PUBGMap() { Name = MiramarMapName, Map = "map2.png" });
            this.maps.Add(new PUBGMap() { Name = SanhokMapName, Map = "map3.png" });
            this.maps.Add(new PUBGMap() { Name = VikendiMapName, Map = "map4.png" });

            this.Initialize(dropMapType, this.MapImage, this.LocationPointsCanvas);

            string mapImagePath = null;
            switch (this.dropMapType)
            {
                case DropMapTypeEnum.Fortnite:
                    mapImagePath = "/Assets/Images/FortniteDropMap/map.png";
                    break;
                case DropMapTypeEnum.PUBG:
                    this.MapComboBox.Visibility = Visibility.Visible;
                    mapImagePath = "/Assets/Images/PUBGDropMap/map1.png";
                    break;
                case DropMapTypeEnum.RealmRoyale:
                    mapImagePath = "/Assets/Images/RealmRoyaleDropMap/map.png";
                    break;
                case DropMapTypeEnum.BlackOps4:
                    mapImagePath = "/Assets/Images/BlackOps4DropMap/map.jpg";
                    break;
                case DropMapTypeEnum.ApexLegends:
                    mapImagePath = "/Assets/Images/ApexLegendsDropMap/map.png";
                    break;
            }

            BitmapImage bitmapMapImage = new BitmapImage();
            bitmapMapImage.BeginInit();
            bitmapMapImage.UriSource = new Uri("pack://application:,,," + mapImagePath);
            bitmapMapImage.EndInit();
            this.MapImage.Source = bitmapMapImage;

            this.MapImage.SizeChanged += MapImage_SizeChanged;

            this.MaxTimeTextBox.Text = this.maxTime.ToString();
            this.SparkCostTextBox.Text = this.sparkCost.ToString();

            JObject settings = this.GetCustomSettings();
            if (settings.ContainsKey(MapSelectionSettingProperty))
            {
                this.MapComboBox.SelectedIndex = settings[MapSelectionSettingProperty].ToObject<int>();
            }
            else
            {
                this.MapComboBox.SelectedIndex = 0;
            }
        }

        protected override void UpdateTimerUI(int timeLeft)
        {
            this.TimerTextBlock.Text = timeLeft.ToString();
            this.TimerStackPanel.Visibility = Visibility.Visible;
        }

        protected override string ComputeLocation(Point point)
        {
            double gridSize = 1.0;
            switch (this.dropMapType)
            {
                case DropMapTypeEnum.Fortnite:
                    gridSize = 10.0;
                    break;
                case DropMapTypeEnum.PUBG:
                case DropMapTypeEnum.RealmRoyale:
                case DropMapTypeEnum.BlackOps4:
                case DropMapTypeEnum.ApexLegends:
                    gridSize = 12.5;
                    break;
            }

            if (this.dropMapType == DropMapTypeEnum.PUBG)
            {
                return string.Format("{0} {1}", (char)((int)(point.X / gridSize) + 65), (char)((int)(point.Y / gridSize) + 73));
            }
            else
            {
                return string.Format("{0} {1}", (char)((int)(point.X / gridSize) + 65), (int)(point.Y / gridSize) + 1);
            }
        }

        protected override async Task UpdateWinnerUI(uint winner, string username, string location)
        {
            this.TimerStackPanel.Visibility = Visibility.Hidden;

            this.DropLocationTextBlock.Text = location;
            await this.WinnerAvatar.SetUserAvatarUrl(winner);
            this.WinnerAvatar.SetSize(80);
            this.WinnerTextBlock.Text = username;

            this.DropLocationStackPanel.Visibility = Visibility.Visible;
            this.WinnerStackPanel.Visibility = Visibility.Visible;
        }

        protected override async Task<bool> GameConnectedInternal()
        {
            this.Dispatcher.Invoke(() =>
            {
                this.MapComboBox.IsEnabled = false;
                this.MaxTimeTextBox.IsEnabled = false;
                this.SparkCostTextBox.IsEnabled = false;

                this.TimerStackPanel.Visibility = Visibility.Hidden;
                this.DropLocationStackPanel.Visibility = Visibility.Hidden;
                this.WinnerStackPanel.Visibility = Visibility.Hidden;

                this.TimerTextBlock.Text = string.Empty;
                this.DropLocationTextBlock.Text = string.Empty;
                this.WinnerAvatar.SetSize(80);
                this.WinnerTextBlock.Text = string.Empty;
            });

            if (await base.GameConnectedInternal())
            {
                if (this.dropMapType == DropMapTypeEnum.PUBG)
                {
                    JObject settings = this.GetCustomSettings();
                    settings[MapSelectionSettingProperty] = this.MapComboBox.SelectedIndex;
                    this.SaveCustomSettings(settings);

                    PUBGMap map = (PUBGMap)this.MapComboBox.SelectedItem;

                    InteractiveConnectedButtonControlModel control = new InteractiveConnectedButtonControlModel() { controlID = this.positionButton.controlID };
                    control.meta["map"] = map.Map;
                    await ChannelSession.Interactive.UpdateControls(this.scene, new List<InteractiveControlModel>() { control });
                }

                return true;
            }
            return false;
        }

        protected override Task GameDisconnectedInternal()
        {
            this.MapComboBox.IsEnabled = true;
            this.MaxTimeTextBox.IsEnabled = true;
            this.SparkCostTextBox.IsEnabled = true;

            return Task.FromResult(0);
        }

        private void MapComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.MapComboBox.SelectedIndex >= 0)
            {
                string mapImagePath = "/Assets/Images/PUBGDropMap/";
                PUBGMap map = (PUBGMap)this.MapComboBox.SelectedItem;
                if (map.Name.Equals(ErangelMapName))
                {
                    mapImagePath += "map1.png";
                }
                else if (map.Name.Equals(MiramarMapName))
                {
                    mapImagePath += "map2.png";
                }
                else if (map.Name.Equals(SanhokMapName))
                {
                    mapImagePath += "map3.png";
                }

                JObject settings = this.GetCustomSettings();
                settings[MapSelectionSettingProperty] = this.MapComboBox.SelectedIndex;
                this.SaveCustomSettings(settings);
            }
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

        private void MapImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.OnUIResize();
        }
    }
}
