using Mixer.Base.Model.Interactive;
using MixItUp.Base;
using MixItUp.Base.MixerAPI;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Interactive
{
    public class PlayerData : NotifyPropertyChangedBase
    {
        public UserViewModel User { get; set; }
        public int Total
        {
            get { return this.total; }
            set
            {
                this.total = value;
                this.NotifyPropertyChanged();
            }
        }
        private int total;

        public PlayerData(UserViewModel user, int total)
        {
            this.User = user;
            this.Total = total;
        }
    }

    /// <summary>
    /// Interaction logic for FlySwatterInteractiveControl.xaml
    /// </summary>
    public partial class FlySwatterInteractiveControl : CustomInteractiveGameControl
    {
        private const string MaxTimeSettingProperty = "MaxTime";

        private MixPlayConnectedSceneModel scene;
        private MixPlayConnectedButtonControlModel gameStartButton;
        private MixPlayConnectedButtonControlModel timeLeftButton;
        private MixPlayConnectedButtonControlModel flyHitButton;
        private MixPlayConnectedButtonControlModel gameEndButton;
        private MixPlayConnectedButtonControlModel resultsButton;

        private int maxTime = 30;

        private LockedDictionary<UserViewModel, PlayerData> userTotals = new LockedDictionary<UserViewModel, PlayerData>();
        private ObservableCollection<PlayerData> userCollection = new ObservableCollection<PlayerData>();
        private PlayerData winner;

        private SemaphoreSlim inputLock = new SemaphoreSlim(1);

        public FlySwatterInteractiveControl(MixPlayGameModel game, MixPlayGameVersionModel version)
            : base(game, version)
        {
            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            this.ParticipantsList.ItemsSource = this.userCollection;

            JObject settings = this.GetCustomSettings();
            if (settings.ContainsKey(MaxTimeSettingProperty))
            {
                this.maxTime = settings[MaxTimeSettingProperty].ToObject<int>();
            }

            this.MaxTimeTextBox.Text = this.maxTime.ToString();

            await base.OnLoaded();
        }

        protected override async Task<bool> GameConnectedInternal()
        {
            MixPlayConnectedSceneGroupCollectionModel sceneGroups = await ChannelSession.Interactive.GetScenes();
            if (sceneGroups != null)
            {
                this.scene = sceneGroups.scenes.FirstOrDefault();
                if (this.scene != null)
                {
                    this.gameStartButton = this.scene.buttons.FirstOrDefault(c => c.controlID.Equals("gameStart"));
                    this.timeLeftButton = this.scene.buttons.FirstOrDefault(c => c.controlID.Equals("timeLeft"));
                    this.flyHitButton = this.scene.buttons.FirstOrDefault(c => c.controlID.Equals("flyHit"));
                    this.gameEndButton = this.scene.buttons.FirstOrDefault(c => c.controlID.Equals("gameEnd"));
                    this.resultsButton = this.scene.buttons.FirstOrDefault(c => c.controlID.Equals("results"));
                    if (this.gameStartButton != null && this.timeLeftButton != null && this.flyHitButton != null && this.gameEndButton != null && this.resultsButton != null)
                    {
                        this.WinnerGrid.DataContext = null;
                        this.userCollection.Clear();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        Task.Run(async () =>
                        {
                            try
                            {
                                userTotals.Clear();
                                winner = null;

                                this.gameStartButton.meta["timeLeft"] = this.maxTime;
                                await ChannelSession.Interactive.UpdateControls(scene, new List<MixPlayControlModel>() { this.gameStartButton });

                                for (int i = this.maxTime; i >= 0; i--)
                                {
                                    this.timeLeftButton.meta["timeLeft"] = i;
                                    await ChannelSession.Interactive.UpdateControls(scene, new List<MixPlayControlModel>() { this.timeLeftButton });

                                    await this.Dispatcher.InvokeAsync(() =>
                                    {
                                        this.TimeLeftTextBlock.Text = i.ToString();
                                    });
                                    await Task.Delay(1000);
                                }

                                await Task.Delay(2000);

                                winner = userTotals.Values.OrderByDescending(user => user.Total).FirstOrDefault();
                                if (winner != null && winner.User.GetParticipantModels().Count() > 0)
                                {
                                    this.resultsButton.meta["winner"] = JObject.FromObject(winner.User.GetParticipantModels().FirstOrDefault());
                                    await ChannelSession.Interactive.UpdateControls(scene, new List<MixPlayControlModel>() { this.resultsButton });

                                    this.Dispatcher.InvokeAsync(() =>
                                    {
                                        this.WinnerGrid.DataContext = this.winner;
                                    });

                                    ChannelSession.Chat.SendMessage(string.Format("Winner: @{0}, Total Flies: {1}", winner.User.UserName, winner.Total));
                                }
                            }
                            catch (Exception ex) { Logger.Log(ex.ToString()); }
                        });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                        return true;
                    }
                }
            }
            return false;
        }

        protected override async Task OnInteractiveControlUsed(UserViewModel user, MixPlayGiveInputModel input, InteractiveConnectedControlCommand command)
        {
            if (user != null && !user.IsAnonymous && (input.input.controlID.Equals("gameEnd") || input.input.controlID.Equals("flyHit"))
                && input.input.meta.TryGetValue("total", out JToken totalToken))
            {
                int total = totalToken.ToObject<int>();

                await this.inputLock.WaitAndRelease(async () =>
                {
                    if (!userTotals.ContainsKey(user))
                    {
                        userTotals[user] = new PlayerData(user, total);
                        await this.Dispatcher.InvokeAsync(() =>
                        {
                            this.userCollection.Add(userTotals[user]);
                        });
                    }
                });

                userTotals[user].Total = total;
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

        protected void SaveSettings()
        {
            JObject settings = this.GetCustomSettings();
            settings[MaxTimeSettingProperty] = this.maxTime;
            this.SaveCustomSettings(settings);
        }
    }
}
