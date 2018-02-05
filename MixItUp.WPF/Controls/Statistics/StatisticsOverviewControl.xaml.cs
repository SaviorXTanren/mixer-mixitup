using MaterialDesignThemes.Wpf;
using MixItUp.Base.Statistics;
using System;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Statistics
{
    /// <summary>
    /// Interaction logic for StatisticsOverviewControl.xaml
    /// </summary>
    public partial class StatisticsOverviewControl : LoadingControlBase
    {
        private PackIconKind icon;
        private StatisticDataTrackerBase dataTracker;

        public StatisticsOverviewControl(StatisticDataTrackerBase dataTracker, PackIconKind icon)
        {
            this.dataTracker = dataTracker;
            this.icon = icon;

            InitializeComponent();
        }

        protected override Task OnLoaded()
        {
            this.StatisticNameTextBlock.Text = this.dataTracker.Name;
            this.StatisticIcon.Kind = this.icon;

            Task.Run(async () =>
            {
                while (true)
                {
                    await this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.RefreshOverview();
                    }));

                    await Task.Delay(60000);
                }
            });

            return Task.FromResult(0);
        }

        protected override Task OnVisibilityChanged()
        {
            this.RefreshOverview();

            return Task.FromResult(0);
        }

        public void RefreshOverview()
        {
            this.StatisticOverviewDataTextBlock.Text = this.dataTracker.ToString();
        }
    }
}
