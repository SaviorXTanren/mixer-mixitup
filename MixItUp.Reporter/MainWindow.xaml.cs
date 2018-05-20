using MaterialDesignThemes.Wpf;
using MixItUp.Base.Model.API;
using MixItUp.Base.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.Reporter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MixItUpService service = new MixItUpService();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void ReportIssueButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(this.IssueDescriptionTextBox.Text))
                {
                    await this.ShowDialog("Please include a description of the issue");
                    return;
                }

                this.StatusBar.Visibility = Visibility.Visible;
                this.IsEnabled = false;

                string logContents = null;
                if (File.Exists(App.LogFilePath))
                {
                    logContents = File.ReadAllText(App.LogFilePath);
                }

                await this.service.SendIssueReport(new IssueReportEvent()
                {
                    MixerUserID = App.MixerUserID,
                    Description = this.IssueDescriptionTextBox.Text,
                    LogContents = logContents,
                });

                this.IsEnabled = true;
                this.StatusBar.Visibility = Visibility.Hidden;

                await this.ShowDialog("Thank you for reporting this issue!" + Environment.NewLine + Environment.NewLine + "We'll look into it as soon as we can.");
                this.Close();
            }
            catch (Exception) { }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async Task ShowDialog(string message)
        {
            object dialogObj = this.FindName("MDDialogHost");
            if (dialogObj != null && dialogObj is DialogHost)
            {
                this.IsEnabled = false;
                await ((DialogHost)dialogObj).ShowDialog(new BasicDialogControl(message));
                this.IsEnabled = true;
            }
        }
    }
}
