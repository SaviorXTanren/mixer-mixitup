using MixItUp.Base.Model.API;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MixItUp.Installer
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public const string InstallerLogFileName = "MixItUp-Installer-Log.txt";
        public const string ShortcutFileName = "Mix It Up.lnk";

        public const string ApplicationSettingsFileName = "ApplicationSettings.xml";

        public const string MixItUpProcessName = "MixItUp";
        public const string AutoHosterProcessName = "MixItUp.AutoHoster";

        private static readonly Version minimumOSVersion = new Version(6, 2, 0, 0);

        public static readonly string InstallDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MixItUp");
        public static readonly string StartMenuDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Mix It Up");

        public static string InstallSettingsDirectory { get { return Path.Combine(MainWindowViewModel.InstallSettingsDirectory, "Settings"); } }

        public static string ZipDownloadFilePath { get { return Path.Combine(Path.GetTempPath(), "MixItUp.zip"); } }

        public static bool IsMixItUpAlreadyInstalled() { return Directory.Exists(InstallDirectory); }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsUpdate
        {
            get { return this.isUpdate; }
            private set
            {
                this.isUpdate = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsInstall");
            }
        }
        private bool isUpdate;

        public bool IsInstall { get { return !this.IsUpdate; } }

        public bool IsPreview
        {
            get { return this.isPreview; }
            private set
            {
                this.isPreview = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool isPreview;

        public bool IsOperationBeingPerformed
        {
            get { return this.isOperationBeingPerformed; }
            private set
            {
                this.isOperationBeingPerformed = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool isOperationBeingPerformed;

        public bool IsOperationIndeterminate
        {
            get { return this.isOperationIndeterminate; }
            private set
            {
                this.isOperationIndeterminate = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool isOperationIndeterminate;

        public int OperationProgress
        {
            get { return this.operationProgress; }
            private set
            {
                this.operationProgress = value;
                this.NotifyPropertyChanged();
            }
        }
        private int operationProgress;

        public string DisplayText1
        {
            get { return this.displayText1; }
            private set
            {
                this.displayText1 = value;
                this.NotifyPropertyChanged();
            }
        }
        private string displayText1;

        public bool ErrorOccurred
        {
            get { return this.errorOccurred; }
            private set
            {
                this.errorOccurred = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool errorOccurred;

        public string DisplayText2
        {
            get { return this.displayText2; }
            private set
            {
                this.displayText2 = value;
                this.NotifyPropertyChanged();
            }
        }
        private string displayText2;

        public MainWindowViewModel()
        {
            if (Directory.Exists(InstallDirectory))
            {
                this.IsUpdate = true;
                string applicationSettingsFilePath = Path.Combine(InstallDirectory, ApplicationSettingsFileName);
                if (File.Exists(applicationSettingsFilePath))
                {
                    using (StreamReader reader = new StreamReader(File.OpenRead(applicationSettingsFilePath)))
                    {
                        JObject jobj = JObject.Parse(reader.ReadToEnd());
                        if (jobj != null && jobj.ContainsKey("PreviewProgram"))
                        {
                            this.IsPreview = jobj["PreviewProgram"].ToObject<bool>();
                        }
                    }
                }
            }

            this.DisplayText1 = "Preparing installation...";
            this.isOperationBeingPerformed = true;
            this.IsOperationIndeterminate = true;
        }

        public bool CheckCompatability()
        {
            if (Environment.OSVersion.Version < minimumOSVersion)
            {
                this.ShowError("Mix It Up only runs on Windows 8 & higher.", "If incorrect, please contact support@mixitupapp.com");
                return false;
            }
            return true;
        }

        public async Task<bool> Run()
        {
            bool result = false;

            await Task.Run(async () =>
            {
                try
                {
                    if (!this.IsUpdate || await this.WaitForMixItUpToClose())
                    {
                        MixItUpUpdateModel update = await this.GetUpdateData();
                        if (update != null)
                        {
                            if (await this.DownloadZipArchive(update))
                            {
                                if (this.InstallMixItUp())
                                {
                                    if (this.IsUpdate || this.CreateMixItUpShortcut())
                                    {
                                        result = true;
                                    }
                                }
                                else
                                {
                                    this.ShowError("Failed to install, please reboot your machine & try again.", "If this occurs again, contact support@mixitupapp.com");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.WriteToLogFile(ex.ToString());
                }
            });

            if (!result && !this.ErrorOccurred)
            {
                this.ShowError(string.Format("{0} File Created", InstallerLogFileName), "Contact support@mixitupapp.com with the file to help diagnose this issue.");
            }
            return result;
        }

        public void Launch()
        {
            Process.Start(Path.Combine(MainWindowViewModel.StartMenuDirectory, MainWindowViewModel.ShortcutFileName));
        }

        protected void NotifyPropertyChanged([CallerMemberName]string name = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private async Task<bool> WaitForMixItUpToClose()
        {
            this.DisplayText1 = "Waiting for Mix It Up to close...";
            this.IsOperationIndeterminate = true;
            this.OperationProgress = 0;

            for (int i = 0; i < 10; i++)
            {
                bool isRunning = false;
                foreach (Process clsProcess in Process.GetProcesses())
                {
                    if (clsProcess.ProcessName.Equals(MixItUpProcessName) || clsProcess.ProcessName.Equals(AutoHosterProcessName))
                    {
                        isRunning = true;
                        if (i == 5)
                        {
                            clsProcess.CloseMainWindow();
                        }
                    }
                }

                if (!isRunning)
                {
                    return true;
                }
                await Task.Delay(1000);
            }
            return false;
        }

        private async Task<MixItUpUpdateModel> GetUpdateData()
        {
            this.DisplayText1 = "Finding latest version...";
            this.IsOperationIndeterminate = true;
            this.OperationProgress = 0;

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync((this.IsPreview) ? "https://mixitupapi.azurewebsites.net/api/updates/preview"
                    : "https://mixitupapi.azurewebsites.net/api/updates");
                if (response.IsSuccessStatusCode)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    JObject jobj = JObject.Parse(responseString);
                    return jobj.ToObject<MixItUpUpdateModel>();
                }
            }
            return null;
        }

        private async Task<bool> DownloadZipArchive(MixItUpUpdateModel update)
        {
            this.DisplayText1 = "Downloading files...";
            this.IsOperationIndeterminate = false;
            this.OperationProgress = 0;

            bool downloadComplete = false;

            WebClient client = new WebClient();
            client.DownloadProgressChanged += (s, e) =>
            {
                this.OperationProgress = e.ProgressPercentage;
            };

            client.DownloadFileCompleted += (s, e) =>
            {
                downloadComplete = true;
                this.OperationProgress = 100;
            };

            client.DownloadFileAsync(new Uri(update.ZipArchiveLink), ZipDownloadFilePath);

            while (!downloadComplete)
            {
                await Task.Delay(1000);
            }

            client.Dispose();

            return File.Exists(ZipDownloadFilePath);
        }

        private bool InstallMixItUp()
        {
            this.DisplayText1 = "Installing files...";
            this.IsOperationIndeterminate = false;
            this.OperationProgress = 0;

            try
            {
                if (File.Exists(ZipDownloadFilePath))
                {
                    Directory.CreateDirectory(InstallDirectory);
                    if (Directory.Exists(InstallDirectory))
                    {
                        ZipArchive archive = ZipFile.Open(ZipDownloadFilePath, ZipArchiveMode.Read);
                        double current = 0;
                        double total = archive.Entries.Count;
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            string filePath = Path.Combine(InstallDirectory, entry.FullName);
                            string directoryPath = Path.GetDirectoryName(filePath);
                            if (!Directory.Exists(directoryPath))
                            {
                                Directory.CreateDirectory(directoryPath);
                            }
                            entry.ExtractToFile(filePath, overwrite: true);

                            current++;
                            this.OperationProgress = (int)((current / total) * 100);
                        }
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                this.WriteToLogFile(ex.ToString());
            }
            return false;
        }

        private bool CreateMixItUpShortcut()
        {
            this.DisplayText1 = "Creating Start Menu shortcut...";
            this.IsOperationIndeterminate = true;
            this.OperationProgress = 0;

            if (Directory.Exists(StartMenuDirectory))
            {
                Directory.Delete(StartMenuDirectory, recursive: true);
            }

            Directory.CreateDirectory(StartMenuDirectory);
            if (Directory.Exists(StartMenuDirectory))
            {
                string tempLinkFilePath = Path.Combine(InstallDirectory, "Mix It Up.link");
                if (File.Exists(tempLinkFilePath))
                {
                    string shortcutLinkFilePath = Path.Combine(StartMenuDirectory, ShortcutFileName);
                    File.Copy(tempLinkFilePath, shortcutLinkFilePath);

                    return true;
                }
            }
            return false;
        }

        private void ShowError(string message1, string message2)
        {
            this.IsOperationBeingPerformed = false;
            this.ErrorOccurred = true;
            this.DisplayText1 = message1;
            this.DisplayText2 = message2;
        }

        private void WriteToLogFile(string text)
        {
            File.WriteAllText(InstallerLogFileName, text);
        }
    }
}
