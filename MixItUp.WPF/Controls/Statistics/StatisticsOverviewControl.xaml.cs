using MaterialDesignThemes.Wpf;
using Mixer.Base.Util;
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

        public StatisticsOverviewControl(StatisticDataTrackerBase dataTracker)
        {
            this.dataTracker = dataTracker;
            this.icon = EnumHelper.GetEnumValueFromString<PackIconKind>(this.dataTracker.IconName);

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
