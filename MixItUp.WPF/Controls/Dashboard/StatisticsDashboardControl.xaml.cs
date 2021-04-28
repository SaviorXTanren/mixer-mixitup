using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.WPF.Controls.Statistics;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Dashboard
{
    /// <summary>
    /// Interaction logic for StatisticsDashboardControl.xaml
    /// </summary>
    public partial class StatisticsDashboardControl : DashboardControlBase
    {
        private ThreadSafeObservableCollection<StatisticsOverviewControl> statisticControls = new ThreadSafeObservableCollection<StatisticsOverviewControl>();

        public StatisticsDashboardControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.StatisticsOverviewListView.ItemsSource = this.statisticControls;

            this.statisticControls.Add(new StatisticsOverviewControl(ServiceManager.Get<StatisticsService>().ViewerTracker));
            this.statisticControls.Add(new StatisticsOverviewControl(ServiceManager.Get<StatisticsService>().FollowTracker));
            this.statisticControls.Add(new StatisticsOverviewControl(ServiceManager.Get<StatisticsService>().AllSubsTracker));
            this.statisticControls.Add(new StatisticsOverviewControl(ServiceManager.Get<StatisticsService>().DonationsTracker));
            this.statisticControls.Add(new StatisticsOverviewControl(ServiceManager.Get<StatisticsService>().BitsTracker));

            foreach (StatisticsOverviewControl statisticControl in this.statisticControls)
            {
                statisticControl.HideName();
            }

            return base.InitializeInternal();
        }
    }
}
