using Mixer.Base.Model.Interactive;
using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.Base.MixerAPI;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Interactive
{
    public class Point
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    /// <summary>
    /// Interaction logic for FortniteDropMapInteractiveControl.xaml
    /// </summary>
    public partial class FortniteDropMapInteractiveControl : CustomInteractiveGameControl
    {
        private Dictionary<uint, UserProfileAvatarControl> userAvatars = new Dictionary<uint, UserProfileAvatarControl>();
        private Dictionary<uint, Point> userPoints = new Dictionary<uint, Point>();

        private InteractiveConnectedSceneModel scene;
        private InteractiveConnectedButtonControlModel positionButton;
        private InteractiveConnectedButtonControlModel winnerButton;

        public FortniteDropMapInteractiveControl(InteractiveGameModel game, InteractiveGameVersionModel version)
            : base(game, version)
        {
            InitializeComponent();
        }

        protected override async Task GameConnectedInternal()
        {
            InteractiveConnectedSceneGroupCollectionModel sceneGroups = await ChannelSession.Interactive.GetScenes();
            if (sceneGroups != null)
            {
                this.scene = sceneGroups.scenes.FirstOrDefault();
                if (this.scene != null)
                {
                    this.positionButton = this.scene.buttons.FirstOrDefault(c => c.controlID.Equals("position"));
                    this.winnerButton = this.scene.buttons.FirstOrDefault(c => c.controlID.Equals("winner"));
                }
            }

            this.userAvatars.Clear();
            this.userPoints.Clear();
            this.LocationPointsCanvas.Children.Clear();

            this.TimerStackPanel.Visibility = Visibility.Collapsed;
            this.DropLocationStackPanel.Visibility = Visibility.Collapsed;
            this.WinnerStackPanel.Visibility = Visibility.Collapsed;

            this.TimerTextBlock.Text = string.Empty;
            this.DropLocationTextBlock.Text = string.Empty;
            this.WinnerAvatar.SetSize(80);
            this.WinnerTextBlock.Text = string.Empty;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                if (this.scene != null && this.winnerButton != null)
                {
                    InteractiveControlModel control = null;

                    for (int i = 0; i < 31; i++)
                    {
                        await Task.Delay(1000);

                        int timeLeft = 30 - i;

                        this.Dispatcher.Invoke(() =>
                        {
                            this.TimerTextBlock.Text = timeLeft.ToString();
                            this.TimerStackPanel.Visibility = Visibility.Visible;
                        });

                        control = new InteractiveControlModel() { controlID = this.winnerButton.controlID };
                        control.meta["timeleft"] = timeLeft;
                        await ChannelSession.Interactive.UpdateControls(this.scene, new List<InteractiveControlModel>() { control });
                    }

                    if (this.userPoints.Count > 0)
                    {
                        var users = this.userPoints.Keys.ToList();
                        Random random = new Random();
                        int index = random.Next(users.Count);
                        uint winner = users[index];
                        Point point = this.userPoints[winner];

                        UserModel user = await ChannelSession.Connection.GetUser(winner);

                        string username = (user != null) ? user.username : "Unknown";
                        string location = string.Format("{0}{1}", (char)((int)(point.X / 10.0) + 65), (int)(point.Y / 10.0) + 1);

                        await this.Dispatcher.InvokeAsync(async () =>
                        {
                            this.TimerStackPanel.Visibility = Visibility.Collapsed;

                            this.DropLocationTextBlock.Text = location;
                            await this.WinnerAvatar.SetImageUrl("https://mixer.com/api/v1/users/" + winner + "/avatar");
                            this.WinnerAvatar.SetSize(80);
                            this.WinnerTextBlock.Text = username;

                            this.DropLocationStackPanel.Visibility = Visibility.Visible;
                            this.WinnerStackPanel.Visibility = Visibility.Visible;
                        });

                        control = new InteractiveControlModel() { controlID = this.winnerButton.controlID };
                        control.meta["userID"] = winner;
                        control.meta["username"] = username;
                        control.meta["location"] = location;
                        await ChannelSession.Interactive.UpdateControls(this.scene, new List<InteractiveControlModel>() { control });
                    }
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        protected override async Task OnInteractiveControlUsed(UserViewModel user, InteractiveGiveInputModel input, InteractiveConnectedControlCommand command)
        {
            if (input.input.controlID.Equals("position") && user != null && input.input.meta.ContainsKey("x") && input.input.meta.ContainsKey("y"))
            {
                if (this.scene != null && this.positionButton != null)
                {
                    Point point = new Point() { X = (double)input.input.meta["x"], Y = (double)input.input.meta["y"] };

                    this.userPoints[user.ID] = point;

                    InteractiveControlModel control = new InteractiveControlModel() { controlID = this.positionButton.controlID };
                    control.meta["userID"] = user.ID;
                    control.meta["x"] = point.X;
                    control.meta["y"] = point.Y;
                    await ChannelSession.Interactive.UpdateControls(this.scene, new List<InteractiveControlModel>() { control });

                    double xPosition = ((point.X / 100.0) * this.LocationPointsCanvas.ActualWidth);
                    double yPosition = ((point.Y / 100.0) * this.LocationPointsCanvas.ActualHeight);

                    UserProfileAvatarControl avatarControl = null;
                    if (!this.userAvatars.ContainsKey(user.ID))
                    {
                        avatarControl = new UserProfileAvatarControl();
                        await avatarControl.SetImageUrl("https://mixer.com/api/v1/users/" + user.ID + "/avatar");
                        avatarControl.SetSize(20);

                        this.LocationPointsCanvas.Children.Add(avatarControl);
                        this.userAvatars[user.ID] = avatarControl;
                    }
                    avatarControl = this.userAvatars[user.ID];

                    Canvas.SetLeft(avatarControl, xPosition - 10);
                    Canvas.SetTop(avatarControl, yPosition - 10);
                }
            }
        }
    }
}
