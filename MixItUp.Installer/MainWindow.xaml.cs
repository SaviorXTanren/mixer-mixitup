using IWshRuntimeLibrary;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
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

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                try
                {
                    string autoUpdaterFile = null;
                    using (WebClient client = new WebClient())
                    {
                        autoUpdaterFile = client.DownloadString(new System.Uri("https://updates.mixitupapp.com/AutoUpdater.xml"));
                    }

                    if (!string.IsNullOrEmpty(autoUpdaterFile))
                    {
                        string updateURL = autoUpdaterFile;
                        updateURL = updateURL.Substring(updateURL.IndexOf("<url>"));
                        updateURL = updateURL.Replace("<url>", "");
                        updateURL = updateURL.Substring(0, updateURL.IndexOf("</url>"));

                        string updateFilePath = Path.Combine(Path.GetTempPath(), "MixItUp.zip");
                        using (WebClient client = new WebClient())
                        {
                            client.DownloadFile(updateURL, updateFilePath);
                        }

                        if (System.IO.File.Exists(updateFilePath))
                        {
                            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MixItUp");

                            if (Directory.Exists(folderPath))
                            {
                                Directory.Delete(folderPath, recursive: true);
                            }

                            Directory.CreateDirectory(folderPath);
                            if (Directory.Exists(folderPath))
                            {
                                ZipFile.ExtractToDirectory(updateFilePath, folderPath);

                                string applicationPath = Path.Combine(folderPath, "MixItUp.exe");
                                if (System.IO.File.Exists(applicationPath))
                                {
                                    string shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Mix It Up");

                                    if (Directory.Exists(shortcutPath))
                                    {
                                        Directory.Delete(shortcutPath, recursive: true);
                                    }

                                    Directory.CreateDirectory(shortcutPath);
                                    if (Directory.Exists(shortcutPath))
                                    {
                                        WshShell wsh = new WshShell();
                                        IWshRuntimeLibrary.IWshShortcut shortcut = wsh.CreateShortcut(Path.Combine(shortcutPath, "Mix It Up.lnk")) as IWshRuntimeLibrary.IWshShortcut;
                                        shortcut.Arguments = "";
                                        shortcut.TargetPath = applicationPath;
                                        shortcut.WindowStyle = 1;
                                        shortcut.Description = "Mix It Up";
                                        shortcut.WorkingDirectory = folderPath;
                                        shortcut.IconLocation = applicationPath;
                                        shortcut.Save();

                                        Process.Start(applicationPath);
                                        this.Dispatcher.Invoke(() =>
                                        {
                                            this.Close();
                                        });
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex) { Console.WriteLine(ex); }

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
