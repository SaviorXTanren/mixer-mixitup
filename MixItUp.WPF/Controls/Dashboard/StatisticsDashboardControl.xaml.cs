using MixItUp.Base;
using MixItUp.WPF.Controls.Statistics;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Dashboard
{
    /// <summary>
    /// Interaction logic for StatisticsDashboardControl.xaml
    /// </summary>
    public partial class StatisticsDashboardControl : DashboardControlBase
    {
        private ObservableCollection<StatisticsOverviewControl> statisticControls = new ObservableCollection<StatisticsOverviewControl>();

        public StatisticsDashboardControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.StatisticsOverviewListView.ItemsSource = this.statisticControls;

            this.statisticControls.Add(new StatisticsOverviewControl(ChannelSession.Statistics.ViewerTracker));
            this.statisticControls.Add(new StatisticsOverviewControl(ChannelSession.Statistics.FollowTracker));
            this.statisticControls.Add(new StatisticsOverviewControl(ChannelSession.Statistics.AllSubsTracker));
            this.statisticControls.Add(new StatisticsOverviewControl(ChannelSession.Statistics.SparksEmbersTracker));
            this.statisticControls.Add(new StatisticsOverviewControl(ChannelSession.Statistics.DonationsTracker));

            foreach (StatisticsOverviewControl statisticControl in this.statisticControls)
            {
                statisticControl.HideName();
            }

            return base.InitializeInternal();
        }
    }
}
