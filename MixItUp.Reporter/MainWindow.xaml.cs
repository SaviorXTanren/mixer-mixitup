using MaterialDesignThemes.Wpf;
using MixItUp.Base.Model.API;
using MixItUp.Base.Services;
using System;
using System.Diagnostics;
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
                    await this.ShowMessageDialog("Please include a description of the issue");
                    return;
                }

                if (string.IsNullOrEmpty(this.ReplyEmailTextBox.Text))
                {
                    await this.ShowMessageDialog("Please include a reply email so we can respond with how to resolve your issue");
                    return;
                }

                this.StatusBar.Visibility = Visibility.Visible;
                this.IsEnabled = false;

                string logContents = null;
                if (File.Exists(App.LogFilePath))
                {
                    logContents = File.ReadAllText(App.LogFilePath);
                }

                await this.service.SendIssueReport(new IssueReportModel()
                {
                    Username = App.Username,
                    EmailAddress = this.ReplyEmailTextBox.Text,
                    Description = this.IssueDescriptionTextBox.Text,
                    LogContents = logContents,
                });

                this.IsEnabled = true;
                this.StatusBar.Visibility = Visibility.Hidden;

                await this.ShowMessageDialog("Thank you for reporting this issue! Bug reports typically take at least a day or two to respond to." + Environment.NewLine + Environment.NewLine + "If you need more immediate assistance, please visit our Discord server: https://mixitupapp.com/discord");
            }
            catch (Exception ex) { Console.WriteLine(ex); }

            this.Close();
        }

        private void OpenLogFileButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo(App.LogFilePath)
            {
                UseShellExecute = true
            };
            Process.Start(processInfo);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async Task ShowMessageDialog(string message)
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
