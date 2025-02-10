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

        public const string OldApplicationSettingsFileName = "ApplicationSettings.xml";
        public const string NewApplicationSettingsFileName = "ApplicationSettings.json";

        public const string MixItUpProcessName = "MixItUp";
        public const string AutoHosterProcessName = "MixItUp.AutoHoster";

        private static readonly Version minimumOSVersion = new Version(6, 2, 0, 0);

        public static readonly string DefaultInstallDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MixItUp");
        public static readonly string StartMenuDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Mix It Up");

        public static string InstallSettingsDirectory { get { return Path.Combine(MainWindowViewModel.InstallSettingsDirectory, "Settings"); } }

        public static byte[] ZipArchiveData { get; set; }

        public static string StartMenuShortCutFilePath { get { return Path.Combine(StartMenuDirectory, ShortcutFileName); } }
        public static string DesktopShortCutFilePath { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), ShortcutFileName); } }

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

        public bool IsTest
        {
            get { return this.isTest; }
            private set
            {
                this.isTest = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool isTest;

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

        public string SpecificErrorMessage
        {
            get { return this.specificErrorMessage; }
            private set
            {
                this.specificErrorMessage = value;
                this.NotifyPropertyChanged();
            }
        }
        private string specificErrorMessage;

        public string HyperlinkAddress
        {
            get { return this.hyperlinkAddress; }
            private set
            {
                this.hyperlinkAddress = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ShowHyperlinkAddress");
            }
        }
        private string hyperlinkAddress;

        public bool ShowHyperlinkAddress { get { return !string.IsNullOrEmpty(this.HyperlinkAddress); } }

        private string installDirectory;

        public MainWindowViewModel()
        {
            this.installDirectory = DefaultInstallDirectory;

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 2)
            {
                this.installDirectory = args[1];
            }

            if (Directory.Exists(this.installDirectory))
            {
                this.IsUpdate = true;
                string applicationSettingsFilePath = Path.Combine(this.installDirectory, NewApplicationSettingsFileName);
                if (!File.Exists(applicationSettingsFilePath))
                {
                    applicationSettingsFilePath = Path.Combine(this.installDirectory, OldApplicationSettingsFileName);
                }

                if (File.Exists(applicationSettingsFilePath))
                {
                    using (StreamReader reader = new StreamReader(File.OpenRead(applicationSettingsFilePath)))
                    {
                        JObject jobj = JObject.Parse(reader.ReadToEnd());
                        if (jobj != null)
                        {
                            if (jobj.ContainsKey("PreviewProgram"))
                            {
                                this.IsPreview = jobj["PreviewProgram"].ToObject<bool>();
                            }

                            if (jobj.ContainsKey("TestBuild"))
                            {
                                this.IsTest = jobj["TestBuild"].ToObject<bool>();
                            }
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
                    File.Delete(InstallerLogFileName);

                    if (!this.IsUpdate || await this.WaitForMixItUpToClose())
                    {
                        MixItUpUpdateModel update = await this.GetUpdateData();

                        if (this.IsPreview)
                        {
                            MixItUpUpdateModel preview = await this.GetUpdateData(preview: true);
                            if (preview != null && preview.SystemVersion > update.SystemVersion)
                            {
                                update = preview;
                            }
                        }

                        if (this.IsTest)
                        {
                            MixItUpUpdateModel test = await this.GetUpdateData(test: true);
                            if (test != null && test.SystemVersion > update.SystemVersion)
                            {
                                update = test;
                            }
                        }

                        if (update != null)
                        {
                            if (await this.DownloadZipArchive(update))
                            {
                                if (this.InstallMixItUp() && this.CreateMixItUpShortcut())
                                {
                                    result = true;
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
                if (!string.IsNullOrEmpty(this.SpecificErrorMessage))
                {
                    this.HyperlinkAddress = InstallerLogFileName;
                    this.ShowError(string.Format("{0} file created:", InstallerLogFileName), this.SpecificErrorMessage);
                }
                else
                {
                    this.HyperlinkAddress = InstallerLogFileName;
                    this.ShowError(string.Format("{0} file created:", InstallerLogFileName), "Please visit our support Discord or send an email to support@mixitupapp.com with the contents of this file.");
                }
            }
            return result;
        }

        public void Launch()
        {
            if (Path.Equals(this.installDirectory, DefaultInstallDirectory))
            {
                if (File.Exists(StartMenuShortCutFilePath))
                {
                    ProcessStartInfo processInfo = new ProcessStartInfo(StartMenuShortCutFilePath)
                    {
                        UseShellExecute = true
                    };
                    Process.Start(processInfo);
                }
                else if (File.Exists(DesktopShortCutFilePath))
                {
                    ProcessStartInfo processInfo = new ProcessStartInfo(DesktopShortCutFilePath)
                    {
                        UseShellExecute = true
                    };
                    Process.Start(processInfo);
                }
            }
            else
            {
                Process.Start(Path.Combine(this.installDirectory, "MixItUp.exe"));
            }
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

        private async Task<MixItUpUpdateModel> GetUpdateData(bool preview = false, bool test = false)
        {
            this.DisplayText1 = "Finding latest version...";
            this.IsOperationIndeterminate = true;
            this.OperationProgress = 0;

            MixItUpUpdateModel update = await this.GetUpdateDataV2(preview, test);
            if (update != null)
            {
                return update;
            }

            string url = "https://api.mixitupapp.com/api/updates";
            if (preview)
            {
                url = "https://api.mixitupapp.com/api/updates/preview";
            }
            else if (test)
            {
                url = "https://api.mixitupapp.com/api/updates/test";
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 5);

                    HttpResponseMessage response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseString = await response.Content.ReadAsStringAsync();
                        JObject jobj = JObject.Parse(responseString);
                        return jobj.ToObject<MixItUpUpdateModel>();
                    }
                    else
                    {
                        this.WriteToLogFile($"{url} - {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    }
                }
            }
            catch (Exception ex)
            {
                this.WriteToLogFile(ex.ToString());
            }

            return null;
        }

        private async Task<MixItUpUpdateModel> GetUpdateDataV2(bool preview = false, bool test = false)
        {
            this.DisplayText1 = "Finding latest version...";
            this.IsOperationIndeterminate = true;
            this.OperationProgress = 0;

            string type = "public";
            if (preview)
            {
                type = "preview";
            }
            else if (test)
            {
                type = "test";
            }

            string url = $"https://raw.githubusercontent.com/mixitupapp/mixitupdesktop-data/main/Updates/{type}.json";

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.Timeout = new TimeSpan(0, 0, 5 * (i + 1));

                        HttpResponseMessage response = await client.GetAsync(url);
                        if (response.IsSuccessStatusCode)
                        {
                            string responseString = await response.Content.ReadAsStringAsync();
                            JObject jobj = JObject.Parse(responseString);
                            MixItUpUpdateV2Model update = jobj.ToObject<MixItUpUpdateV2Model>();
                            if (update != null)
                            {
                                return new MixItUpUpdateModel(update);
                            }
                        }
                        else
                        {
                            this.WriteToLogFile($"{url} - {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.WriteToLogFile(ex.ToString());
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

            client.DownloadDataCompleted += (s, e) =>
            {
                downloadComplete = true;
                this.OperationProgress = 100;
                if (e.Error == null && !e.Cancelled)
                {
                    ZipArchiveData = e.Result;
                }
                else if (e.Error != null)
                {
                    this.WriteToLogFile(e.Error.ToString());
                }
            };

            client.DownloadDataAsync(new Uri(update.ZipArchiveLink));

            while (!downloadComplete)
            {
                await Task.Delay(1000);
            }

            client.Dispose();

            return (ZipArchiveData != null && ZipArchiveData.Length > 0);
        }

        private bool InstallMixItUp()
        {
            this.DisplayText1 = "Installing files...";
            this.IsOperationIndeterminate = false;
            this.OperationProgress = 0;

            try
            {
                if (ZipArchiveData != null && ZipArchiveData.Length > 0)
                {
                    Directory.CreateDirectory(this.installDirectory);
                    if (Directory.Exists(this.installDirectory))
                    {
                        using (MemoryStream zipStream = new MemoryStream(ZipArchiveData))
                        {
                            ZipArchive archive = new ZipArchive(zipStream);
                            double current = 0;
                            double total = archive.Entries.Count;
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                var fullName = entry.FullName;
                                if (entry.FullName.StartsWith("Mix It Up/"))
                                {
                                    fullName = entry.FullName.Substring("Mix It Up/".Length);
                                }

                                string filePath = Path.Combine(this.installDirectory, fullName);
                                string directoryPath = Path.GetDirectoryName(filePath);
                                if (!Directory.Exists(directoryPath))
                                {
                                    Directory.CreateDirectory(directoryPath);
                                }

                                if (Path.HasExtension(filePath))
                                {
                                    entry.ExtractToFile(filePath, overwrite: true);
                                }

                                current++;
                                this.OperationProgress = (int)((current / total) * 100);
                            }
                            return true;
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException uaex)
            {
                this.SpecificErrorMessage = "We were unable to update due to a file lock issue. Please try rebooting your PC and then running the update. You can also download and re-run our installer to update your installation.";
                this.WriteToLogFile(uaex.ToString());
            }
            catch (IOException ioex)
            {
                this.SpecificErrorMessage = "We were unable to update due to a file lock issue. Please try rebooting your PC and then running the update. You can also download and re-run our installer to update your installation.";
                this.WriteToLogFile(ioex.ToString());
            }
            catch (WebException wex)
            {
                this.SpecificErrorMessage = "We were unable to update due to a network issue, please try again later. If this issue persists, please try restarting your PC and/or router or flush the DNS cache on your computer.";
                this.WriteToLogFile(wex.ToString());
            }
            catch (Exception ex)
            {
                this.WriteToLogFile(ex.ToString());
            }
            return false;
        }

        private bool CreateMixItUpShortcut()
        {
            try
            {
                this.DisplayText1 = "Creating Start Menu & Desktop shortcuts...";
                this.IsOperationIndeterminate = true;
                this.OperationProgress = 0;

                if (!Directory.Exists(StartMenuDirectory))
                {
                    Directory.CreateDirectory(StartMenuDirectory);
                }

                if (Directory.Exists(StartMenuDirectory))
                {
                    string tempLinkFilePath = Path.Combine(DefaultInstallDirectory, "Mix It Up.link");
                    if (File.Exists(tempLinkFilePath))
                    {
                        File.Copy(tempLinkFilePath, StartMenuShortCutFilePath, overwrite: true);
                        if (File.Exists(StartMenuShortCutFilePath))
                        {
                            return true;
                        }
                        else
                        {
                            File.Copy(tempLinkFilePath, DesktopShortCutFilePath, overwrite: true);
                            if (File.Exists(DesktopShortCutFilePath))
                            {
                                this.ShowError("We were unable to create the Start Menu shortcut.", "You can instead use the Desktop shortcut to launch Mix It Up");
                            }
                            else
                            {
                                this.ShowError("We were unable to create the Start Menu & Desktop shortcuts.", "Email support@mixitupapp.com to help diagnose this issue further.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.WriteToLogFile(ex.ToString());
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
            File.AppendAllText(InstallerLogFileName, text + Environment.NewLine + Environment.NewLine);
        }
    }
}
