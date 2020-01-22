using Mixer.Base.Model.MixPlay;
using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.Base.MixerAPI;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.Desktop.Services;
using MixItUp.WPF.Controls.Users;
using MixItUp.WPF.Util;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MixItUp.WPF.Controls.Interactive
{
    public enum DropMapTypeEnum
    {
        Fortnite,
        PUBG,
        RealmRoyale,
        BlackOps4,
        ApexLegends,
        SuperAnimalRoyale
    }

    public class PUBGMap
    {
        public string Name { get; set; }
        public string Map { get; set; }
    }

    /// <summary>
    /// Interaction logic for DropMapInteractiveControl.xaml
    /// </summary>
    public partial class DropMapInteractiveControl : CustomInteractiveGameControl
    {
        private const string MaxTimeSettingProperty = "MaxTime";
        private const string SparkCostSettingProperty = "SparkCost";

        private const string MapSelectionSettingProperty = "MapSelection";

        private const string ErangelMapName = "Erangel";
        private const string MiramarMapName = "Miramar";
        private const string SanhokMapName = "Sanhok";
        private const string VikendiMapName = "Vikendi";

        private DropMapTypeEnum dropMapType;

        private int maxTime = 30;
        private int sparkCost = 0;

        private MixPlayConnectedSceneModel scene;
        private MixPlayConnectedButtonControlModel positionButton;
        private MixPlayConnectedButtonControlModel winnerButton;

        private Dictionary<uint, UserProfileAvatarControl> userAvatars = new Dictionary<uint, UserProfileAvatarControl>();
        private Dictionary<uint, Point> userPoints = new Dictionary<uint, Point>();

        private Canvas canvas;
        private Image image;

        private ObservableCollection<PUBGMap> maps = new ObservableCollection<PUBGMap>();

        public DropMapInteractiveControl(DropMapTypeEnum dropMapType, MixPlayGameModel game, MixPlayGameVersionModel version)
            : base(game, version)
        {
            InitializeComponent();

            this.MapComboBox.ItemsSource = this.maps;

            this.maps.Add(new PUBGMap() { Name = ErangelMapName, Map = "map1.png" });
            this.maps.Add(new PUBGMap() { Name = MiramarMapName, Map = "map2.png" });
            this.maps.Add(new PUBGMap() { Name = SanhokMapName, Map = "map3.png" });
            this.maps.Add(new PUBGMap() { Name = VikendiMapName, Map = "map4.png" });

            this.dropMapType = dropMapType;

            JObject settings = this.GetCustomSettings();
            if (settings.ContainsKey(MaxTimeSettingProperty))
            {
                this.maxTime = settings[MaxTimeSettingProperty].ToObject<int>();
            }
            if (settings.ContainsKey(SparkCostSettingProperty))
            {
                this.sparkCost = settings[SparkCostSettingProperty].ToObject<int>();
            }

            this.MapImage.SizeChanged += MapImage_SizeChanged;
        }

        protected override async Task OnLoaded()
        {
            this.image = this.MapImage;
            this.canvas = this.LocationPointsCanvas;

            this.MaxTimeTextBox.Text = this.maxTime.ToString();
            this.SparkCostTextBox.Text = this.sparkCost.ToString();

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
                case DropMapTypeEnum.SuperAnimalRoyale:
                    mapImagePath = "/Assets/Images/SuperAnimalRoyaleDropMap/map.png";
                    break;
            }

            this.ChangeMapImage(mapImagePath);

            if (this.dropMapType == DropMapTypeEnum.PUBG)
            {
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

            await base.OnLoaded();
        }

        protected void UpdateTimerUI(int timeLeft)
        {
            this.TimerTextBlock.Text = timeLeft.ToString();
            this.TimerStackPanel.Visibility = Visibility.Visible;
        }

        protected string ComputeLocation(Point point)
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
                case DropMapTypeEnum.SuperAnimalRoyale:
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

        protected async Task UpdateWinnerUI(uint winner, string username, string location)
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

            this.SaveSettings();

            MixPlayConnectedSceneGroupCollectionModel sceneGroups = await ChannelSession.Interactive.GetScenes();
            if (sceneGroups != null)
            {
                this.scene = sceneGroups.scenes.FirstOrDefault();
                if (this.scene != null)
                {
                    this.positionButton = this.scene.buttons.FirstOrDefault(c => c.controlID.Equals("position"));
                    this.winnerButton = this.scene.buttons.FirstOrDefault(c => c.controlID.Equals("winner"));
                }
            }

            if (this.positionButton == null || this.winnerButton == null)
            {
                Logger.Log("Could not get position or winner buttons");
                return false;
            }

            if (this.sparkCost > 0)
            {
                this.positionButton.cost = this.sparkCost;
                await ChannelSession.Interactive.UpdateControls(this.scene, new List<MixPlayControlModel>() { this.positionButton });

                await ChannelSession.Interactive.RefreshCachedControls();
            }

            if (this.dropMapType == DropMapTypeEnum.PUBG)
            {
                JObject settings = this.GetCustomSettings();
                settings[MapSelectionSettingProperty] = this.MapComboBox.SelectedIndex;
                this.SaveCustomSettings(settings);

                PUBGMap map = (PUBGMap)this.MapComboBox.SelectedItem;

                MixPlayConnectedButtonControlModel control = new MixPlayConnectedButtonControlModel() { controlID = this.positionButton.controlID };
                control.meta["map"] = map.Map;
                await ChannelSession.Interactive.UpdateControls(this.scene, new List<MixPlayControlModel>() { control });
            }

            this.userAvatars.Clear();
            this.userPoints.Clear();
            if (this.canvas != null)
            {
                this.canvas.Children.Clear();
            }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                if (this.scene != null && this.winnerButton != null)
                {
                    MixPlayConnectedButtonControlModel control = null;

                    for (int i = 0; i < (maxTime + 1); i++)
                    {
                        await Task.Delay(1000);

                        int timeLeft = maxTime - i;

                        this.Dispatcher.Invoke(() =>
                        {
                            this.UpdateTimerUI(timeLeft);
                        });

                        control = new MixPlayConnectedButtonControlModel() { controlID = this.winnerButton.controlID };
                        control.meta["timeleft"] = timeLeft;
                        await ChannelSession.Interactive.UpdateControls(this.scene, new List<MixPlayControlModel>() { control });
                    }

                    if (this.userPoints.Count > 0)
                    {
                        var users = this.userPoints.Keys.ToList();
                        int index = RandomHelper.GenerateRandomNumber(users.Count);
                        uint winner = users[index];
                        Point point = this.userPoints[winner];
                        UserProfileAvatarControl avatar = this.userAvatars[winner];

                        UserModel user = await ChannelSession.MixerUserConnection.GetUser(winner);

                        string username = (user != null) ? user.username : "Unknown";
                        string location = this.ComputeLocation(point);

                        this.Dispatcher.InvokeAsync(async () =>
                        {
                            await this.UpdateWinnerUI(winner, username, location);
                            this.canvas.Children.Clear();
                            this.canvas.Children.Add(avatar);
                        });

                        control = new MixPlayConnectedButtonControlModel() { controlID = this.winnerButton.controlID };
                        control.meta["userID"] = winner;
                        control.meta["username"] = username;
                        control.meta["location"] = location;
                        await ChannelSession.Interactive.UpdateControls(this.scene, new List<MixPlayControlModel>() { control });

                        await ChannelSession.Services.Chat.SendMessage(string.Format("Winner: @{0}, Drop Location: {1}", username, location));
                    }
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return await base.GameConnectedInternal();
        }

        protected override async Task GameDisconnectedInternal()
        {
            this.MapComboBox.IsEnabled = true;
            this.MaxTimeTextBox.IsEnabled = true;
            this.SparkCostTextBox.IsEnabled = true;

            await base.GameDisconnectedInternal();
        }

        protected override async Task OnInteractiveControlUsed(UserViewModel user, MixPlayGiveInputModel input, InteractiveConnectedControlCommand command)
        {
            try
            {
                if (input.input.controlID.Equals("position") && user != null && input.input.meta.ContainsKey("x") && input.input.meta.ContainsKey("y"))
                {
                    if (this.scene != null && this.positionButton != null && input.input.meta["x"] != null && input.input.meta["y"] != null)
                    {
                        Point point = new Point() { X = (double)input.input.meta["x"], Y = (double)input.input.meta["y"] };

                        this.userPoints[user.MixerID] = point;

                        MixPlayConnectedButtonControlModel control = new MixPlayConnectedButtonControlModel() { controlID = this.positionButton.controlID };
                        control.meta["userID"] = user.MixerID;
                        control.meta["x"] = point.X;
                        control.meta["y"] = point.Y;
                        await ChannelSession.Interactive.UpdateControls(this.scene, new List<MixPlayControlModel>() { control });

                        await this.Dispatcher.InvokeAsync(async () =>
                        {
                            if (!this.userAvatars.ContainsKey(user.MixerID))
                            {
                                UserProfileAvatarControl avatarControl = new UserProfileAvatarControl();
                                await avatarControl.SetUserAvatarUrl(user);
                                avatarControl.SetSize(20);
                                this.userAvatars[user.MixerID] = avatarControl;

                                this.canvas.Children.Add(avatarControl);
                            }

                            this.PositionUserPointOnCanvas(user, point);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
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
                else if (map.Name.Equals(VikendiMapName))
                {
                    mapImagePath += "map4.png";
                }

                JObject settings = this.GetCustomSettings();
                settings[MapSelectionSettingProperty] = this.MapComboBox.SelectedIndex;
                this.SaveCustomSettings(settings);

                this.ChangeMapImage(mapImagePath);
            }
        }

        private void MaxTimeTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.MaxTimeTextBox.Text) && int.TryParse(this.MaxTimeTextBox.Text, out int time) && time > 0)
            {
                this.maxTime = time;
                this.SaveSettings();
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
                this.SaveSettings();
            }
            else
            {
                this.SparkCostTextBox.Text = "0";
            }
        }

        protected void SaveSettings()
        {
            JObject settings = this.GetCustomSettings();
            settings[MaxTimeSettingProperty] = this.maxTime;
            settings[SparkCostSettingProperty] = this.sparkCost;
            this.SaveCustomSettings(settings);
        }

        private void MapImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (var userPoint in this.userPoints)
            {
                UserViewModel user = new UserViewModel() { MixerID = userPoint.Key };
                this.PositionUserPointOnCanvas(user, userPoint.Value);
            }
        }

        private void PositionUserPointOnCanvas(UserViewModel user, Point point)
        {
            UserProfileAvatarControl avatarControl = this.userAvatars[user.MixerID];

            double canvasX = ((point.X / 100.0) * this.image.ActualWidth);
            double canvasY = ((point.Y / 100.0) * this.image.ActualHeight);

            Canvas.SetLeft(avatarControl, canvasX - 10);
            Canvas.SetTop(avatarControl, canvasY - 10);
        }

        private void ChangeMapImage(string path)
        {
            this.MapImage.Source = WindowsImageService.LoadLocal(new Uri("pack://application:,,," + path));
        }
    }
}
