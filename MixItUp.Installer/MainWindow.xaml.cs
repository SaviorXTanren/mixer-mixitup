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
        private static readonly Version minimumOSVersion = new Version(6, 2, 0, 0);

        private static bool? response = null;

        private bool isUpdate = false;

        public MainWindow()
        {
            InitializeComponent();

            this.isUpdate = ((App)App.Current).IsUpdate;

            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (Environment.OSVersion.Version < minimumOSVersion)
            {
                this.ShowError("Thank you for using Mix It Up, but unfortunately we only support Windows 8 & higher. If you are running Windows 8 or higher and see this message, please contact Mix It Up support for assistance.");
                this.Close();
                return;
            }

            if (this.isUpdate)
            {
                this.InstallingTextBlock1.Visibility = Visibility.Collapsed;
                this.InstallingTextBlock2.Visibility = Visibility.Collapsed;
                this.UpdatingTextBlock.Visibility = Visibility.Visible;
            }

            await Task.Run(async () =>
            {
                try
                {
                    if (InstallerHelpers.DownloadMixItUp())
                    {
                        if (!this.isUpdate)
                        {
                            if (InstallerHelpers.IsMixItUpAlreadyInstalled())
                            {
                                this.ShowMessage("We've detected that Mix It Up is already installed. Would you like us to keep your existing settings?");

                                do
                                {
                                    await Task.Delay(1000);
                                } while (response == null);

                                this.ShowRegularView();

                                InstallerHelpers.DeleteExistingInstallation(response.GetValueOrDefault());
                            }
                        }

                        InstallerHelpers.InstallMixItUp();

                        bool launch = true;
                        if (!this.isUpdate)
                        {
                            launch = InstallerHelpers.CreateMixItUpShortcut();
                        }

                        if (launch)
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

        private void ShowMessage(string message)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.InstallProgressBar.Visibility = Visibility.Collapsed;
                this.InstallingTextBlock1.Visibility = Visibility.Collapsed;
                this.InstallingTextBlock2.Visibility = Visibility.Collapsed;
                this.UpdatingTextBlock.Visibility = Visibility.Collapsed;

                this.MessageTextBlock.Visibility = Visibility.Visible;
                this.MessageTextBlock.Text = message;
                this.MessageYesNoGrid.Visibility = Visibility.Visible;
            });
        }

        private void ShowError(string message)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.InstallProgressBar.Visibility = Visibility.Collapsed;
                this.InstallingTextBlock1.Visibility = Visibility.Collapsed;
                this.InstallingTextBlock2.Visibility = Visibility.Collapsed;
                this.UpdatingTextBlock.Visibility = Visibility.Collapsed;

                this.ErrorTextBlock.Visibility = Visibility.Visible;
                this.ErrorTextBlock.Text = message;
            });
        }

        private void ShowRegularView()
        {
            this.Dispatcher.Invoke(() =>
            {
                this.ErrorTextBlock.Visibility = Visibility.Collapsed;
                this.MessageTextBlock.Visibility = Visibility.Collapsed;
                this.MessageYesNoGrid.Visibility = Visibility.Collapsed;

                this.InstallProgressBar.Visibility = Visibility.Visible;
                this.InstallingTextBlock1.Visibility = Visibility.Visible;
                this.InstallingTextBlock2.Visibility = Visibility.Visible;
            });
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            response = true;
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            response = false;
        }
    }
}
