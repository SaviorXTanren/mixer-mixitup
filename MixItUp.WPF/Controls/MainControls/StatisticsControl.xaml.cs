using MixItUp.Base;
using MixItUp.Base.Statistics;
using MixItUp.Base.Util;
using MixItUp.WPF.Controls.Statistics;
using MixItUp.WPF.Util;
using NetOffice.ExcelApi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        //private void AutoExportCheckBox_Checked(object sender, RoutedEventArgs e) { ChannelSession.Settings.AutoExportStatistics = this.AutoExportCheckBox.IsChecked.GetValueOrDefault(); }

        private async void ExportStatsButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                string fileName = ChannelSession.Services.FileService.ShowSaveFileDialog(string.Format("Stream Statistics - {0}.xls", ChannelSession.Statistics.StartTime.ToString("MM-dd-yy HH-mm")));
                if (!string.IsNullOrEmpty(fileName))
                {
                    bool result = await Task.Run(() =>
                    {
                        try
                        {
                            using (NetOffice.ExcelApi.Application application = new NetOffice.ExcelApi.Application())
                            {
                                application.DisplayAlerts = false;

                                using (Workbook workbook = application.Workbooks.Add())
                                {
                                    foreach (StatisticDataTrackerBase statistic in ChannelSession.Statistics.Statistics)
                                    {
                                        using (Worksheet worksheet = (Worksheet)workbook.Worksheets.Add())
                                        {
                                            worksheet.Name = statistic.Name;

                                            IEnumerable<string> headers = statistic.GetExportHeaders();
                                            IEnumerable<List<string>> data = statistic.GetExportData();

                                            for (int i = 0; i < headers.Count(); i++)
                                            {
                                                Range range = worksheet.Cells[1, 1 + i];
                                                range.Value = headers.ElementAt(i);
                                                range.ColumnWidth = 25;
                                            }

                                            for (int i = 0; i < data.Count(); i++)
                                            {
                                                for (int j = 0; j < data.ElementAt(i).Count(); j++)
                                                {
                                                    Range range = worksheet.Cells[2 + i, 1 + j];
                                                    range.Value = data.ElementAt(i)[j];
                                                }
                                            }
                                        }
                                    }

                                    workbook.SaveAs(fileName);
                                }

                                application.Quit();
                            }
                            return true;
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(ex);
                        }
                        return false;
                    });

                    if (!result)
                    {
                        await MessageBoxHelper.ShowMessageDialog("We were unable to build the spreadsheet for your statistics." + Environment.NewLine +
                            "Please send a bug report to help diagnose this issue.");
                    }
                }
            });
        }
    }
}
