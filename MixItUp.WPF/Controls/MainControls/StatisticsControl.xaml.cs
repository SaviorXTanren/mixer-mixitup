using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.WPF.Controls.Statistics;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for StatisticsControl.xaml
    /// </summary>
    public partial class StatisticsControl : MainControlBase
    {
        private ThreadSafeObservableCollection<StatisticsOverviewControl> statisticOverviewControls = new ThreadSafeObservableCollection<StatisticsOverviewControl>();

        public StatisticsControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.StatisticsOverviewListView.ItemsSource = this.statisticOverviewControls;
            this.statisticOverviewControls.AddRange(ServiceManager.Get<StatisticsService>().Statistics.Select(s => new StatisticsOverviewControl(s)));

            return base.InitializeInternal();
        }

        //private void AutoExportCheckBox_Checked(object sender, RoutedEventArgs e) { ChannelSession.Settings.AutoExportStatistics = this.AutoExportCheckBox.IsChecked.GetValueOrDefault(); }

        //private async void ExportStatsButton_Click(object sender, RoutedEventArgs e)
        //{
        //    await this.Window.RunAsyncOperation(async () =>
        //    {
        //        string fileName = ServiceManager.Get<IFileService>().ShowSaveFileDialog(string.Format("Stream Statistics - {0}.xls", ServiceManager.Get<StatisticsService>().StartTime.ToString("MM-dd-yy HH-mm")));
        //        if (!string.IsNullOrEmpty(fileName))
        //        {
        //            bool result = await Task.Run(() =>
        //            {
        //                try
        //                {
        //                    using (NetOffice.ExcelApi.Application application = new NetOffice.ExcelApi.Application())
        //                    {
        //                        application.DisplayAlerts = false;

        //                        using (Workbook workbook = application.Workbooks.Add())
        //                        {
        //                            foreach (StatisticDataTrackerModelBase statistic in ServiceManager.Get<StatisticsService>().Statistics)
        //                            {
        //                                using (Worksheet worksheet = (Worksheet)workbook.Worksheets.Add())
        //                                {
        //                                    worksheet.Name = statistic.Name;

        //                                    IEnumerable<string> headers = statistic.GetExportHeaders();
        //                                    IEnumerable<List<string>> data = statistic.GetExportData();

        //                                    for (int i = 0; i < headers.Count(); i++)
        //                                    {
        //                                        Range range = worksheet.Cells[1, 1 + i];
        //                                        range.Value = headers.ElementAt(i);
        //                                        range.ColumnWidth = 25;
        //                                    }

        //                                    for (int i = 0; i < data.Count(); i++)
        //                                    {
        //                                        for (int j = 0; j < data.ElementAt(i).Count(); j++)
        //                                        {
        //                                            Range range = worksheet.Cells[2 + i, 1 + j];
        //                                            range.Value = data.ElementAt(i)[j];
        //                                        }
        //                                    }
        //                                }
        //                            }

        //                            workbook.SaveAs(fileName);
        //                        }

        //                        application.Quit();
        //                    }
        //                    return true;
        //                }
        //                catch (Exception ex)
        //                {
        //                    Logger.Log(ex);
        //                }
        //                return false;
        //            });

        //            if (!result)
        //            {
        //                // NOTE: If this gets uncommented, move to resources file!
        //                await DialogHelper.ShowMessage("We were unable to build the spreadsheet for your statistics. Please ensure you have Microsoft Excel installed, otherwise we can not properly export your statistics data.");
        //            }
        //        }
        //    });
        //}
    }
}
