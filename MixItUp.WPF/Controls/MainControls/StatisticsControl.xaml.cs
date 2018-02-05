using MaterialDesignThemes.Wpf;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.Base.MixerAPI;
using MixItUp.Base.Statistics;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Statistics;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for StatisticsControl.xaml
    /// </summary>
    public partial class StatisticsControl : MainControlBase
    {
        private ObservableCollection<StatisticsOverviewControl> statisticOverviewControls = new ObservableCollection<StatisticsOverviewControl>();

        private EventStatisticsDataTracker followTracker = new EventStatisticsDataTracker("Follows");

        public StatisticsControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.AutoExportCheckBox.IsChecked = ChannelSession.Settings.AutoExportStatistics;

            this.StatisticsOverviewListView.ItemsSource = this.statisticOverviewControls;

            this.statisticOverviewControls.Add(new StatisticsOverviewControl(new NumberStatisticDataTracker("Viewers", (StatisticDataTrackerBase stats) =>
            {
                NumberStatisticDataTracker numberStats = (NumberStatisticDataTracker)stats;
                numberStats.AddValue((int)ChannelSession.Channel.viewersCurrent);
                return Task.FromResult(0);
            }), PackIconKind.Eye));

            this.statisticOverviewControls.Add(new StatisticsOverviewControl(new NumberStatisticDataTracker("Chatters", (StatisticDataTrackerBase stats) =>
            {
                NumberStatisticDataTracker numberStats = (NumberStatisticDataTracker)stats;
                numberStats.AddValue(ChannelSession.Chat.ChatUsers.Count);
                return Task.FromResult(0);
            }), PackIconKind.MessageTextOutline));

            this.statisticOverviewControls.Add(new StatisticsOverviewControl(this.followTracker, PackIconKind.AccountPlus));

            ChannelSession.Constellation.OnEventOccurred += Constellation_OnEventOccurred;

            return base.InitializeInternal();
        }

        private void StatisticsOverviewListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void AutoExportCheckBox_Checked(object sender, RoutedEventArgs e) { ChannelSession.Settings.AutoExportStatistics = this.AutoExportCheckBox.IsChecked.GetValueOrDefault(); }

        private void ExportStatsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Constellation_OnEventOccurred(object sender, Mixer.Base.Model.Constellation.ConstellationLiveEventModel e)
        {
            ChannelModel channel = null;
            UserViewModel user = null;

            JToken userToken;
            if (e.payload.TryGetValue("user", out userToken))
            {
                user = new UserViewModel(userToken.ToObject<UserModel>());

                JToken subscribeStartToken;
                if (e.payload.TryGetValue("since", out subscribeStartToken))
                {
                    user.SubscribeDate = subscribeStartToken.ToObject<DateTimeOffset>();
                }
            }
            else if (e.payload.TryGetValue("hoster", out userToken))
            {
                channel = userToken.ToObject<ChannelModel>();
                user = new UserViewModel(channel.id, channel.token);
            }

            if (e.channel.Equals(ConstellationClientWrapper.ChannelFollowEvent.ToString()) && user != null)
            {
                this.followTracker.OnStatisticEventOccurred(user.UserName);
            }
        }
    }
}
