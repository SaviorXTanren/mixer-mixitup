using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (InstallerHelpers.DownloadMixItUp())
                    {
                        if (InstallerHelpers.CreateMixItUpShortcut())
                        {
                            Process.Start(Path.Combine(InstallerHelpers.StartMenuDirectory, InstallerHelpers.ShortcutFileName));
                            this.Dispatcher.Invoke(() =>
                            {
                                this.Close();
                            });
                        }
                    }
                    return;
                }
                catch (Exception ex) { System.IO.File.WriteAllText("MixItUp-Installer-Log.txt", ex.ToString()); }

                this.ShowError("We were unable to download the latest Mix It Up version. Please contact support@mixitupapp.com for assistance.");
            });
        }

        private void ShowError(string message)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.InstallProgressBar.Visibility = Visibility.Collapsed;
                this.InstallingTextBlock1.Visibility = Visibility.Collapsed;
                this.InstallingTextBlock2.Visibility = Visibility.Collapsed;

                this.ErrorTextBlock.Visibility = Visibility.Visible;
                this.ErrorTextBlock.Text = message;
            });
        }
    }
}
