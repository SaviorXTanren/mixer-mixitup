using MixItUp.Base;
using MixItUp.Base.Statistics;
using MixItUp.WPF.Controls.Statistics;
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

        public StatisticsControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.AutoExportCheckBox.IsChecked = ChannelSession.Settings.AutoExportStatistics;

            this.StatisticsOverviewListView.ItemsSource = this.statisticOverviewControls;
            foreach (StatisticDataTrackerBase statistic in ChannelSession.Statistics.Statistics)
            {
                this.statisticOverviewControls.Add(new StatisticsOverviewControl(statistic));
            }

            return base.InitializeInternal();
        }

        private void StatisticsOverviewListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void AutoExportCheckBox_Checked(object sender, RoutedEventArgs e) { ChannelSession.Settings.AutoExportStatistics = this.AutoExportCheckBox.IsChecked.GetValueOrDefault(); }

        private async void ExportStatsButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                string fileName = ChannelSession.Services.FileService.ShowSaveFileDialog(ChannelSession.Statistics.GetDefaultFileName());
                if (!string.IsNullOrEmpty(fileName))
                {
                    await ChannelSession.Statistics.Export(fileName);
                }
            });
        }
    }
}
