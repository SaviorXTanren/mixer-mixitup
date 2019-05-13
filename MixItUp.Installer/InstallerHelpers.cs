using System;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace MixItUp.Installer
{
    public static class InstallerHelpers
    {
        public const string ShortcutFileName = "Mix It Up.lnk";

        public static readonly string InstallDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MixItUp");
        public static readonly string StartMenuDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Mix It Up");

        public static string InstallSettingsDirectory { get { return Path.Combine(InstallerHelpers.InstallSettingsDirectory, "Settings"); } }

        public static string ZipDownloadFilePath { get { return Path.Combine(Path.GetTempPath(), "MixItUp.zip"); } }

        public static bool IsMixItUpAlreadyInstalled() { return Directory.Exists(InstallDirectory); }

        public static bool DownloadMixItUp()
        {
            string autoUpdaterFile = null;
            using (WebClient client = new WebClient())
            {
                autoUpdaterFile = client.DownloadString(new System.Uri("https://raw.githubusercontent.com/SaviorXTanren/mixer-mixitup/master/MixItUp.WPF/AutoUpdater.xml"));
            }

            if (!string.IsNullOrEmpty(autoUpdaterFile))
            {
                string updateURL = autoUpdaterFile;
                updateURL = updateURL.Substring(updateURL.IndexOf("<url>"));
                updateURL = updateURL.Replace("<url>", "");
                updateURL = updateURL.Substring(0, updateURL.IndexOf("</url>"));

                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(updateURL, ZipDownloadFilePath);
                }
            }
            return System.IO.File.Exists(ZipDownloadFilePath);
        }

        public static bool DeleteExistingInstallation(bool keepSettings)
        {
            if (Directory.Exists(InstallDirectory))
            {
                if (keepSettings)
                {
                    foreach (string directory in Directory.GetDirectories(InstallDirectory))
                    {
                        if (!directory.EndsWith("Settings"))
                        {
                            Directory.Delete(directory, recursive: true);
                        }
                    }

                    foreach (string file in Directory.GetFiles(InstallDirectory))
                    {
                        File.Delete(file);
                    }
                }
                else
                {
                    Directory.Delete(InstallDirectory, recursive: true);
                }
            }
            return true;
        }

        public static bool InstallMixItUp()
        {
            if (System.IO.File.Exists(ZipDownloadFilePath))
            {
                Directory.CreateDirectory(InstallDirectory);
                if (Directory.Exists(InstallDirectory))
                {
                    ZipArchive archive = ZipFile.Open(ZipDownloadFilePath, ZipArchiveMode.Read);
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string filePath = Path.Combine(InstallDirectory, entry.FullName);
                        string directoryPath = Path.GetDirectoryName(filePath);
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }
                        entry.ExtractToFile(filePath, overwrite: true);
                    }
                    return true;
                }
            }
            return false;
        }

        public static bool CreateMixItUpShortcut()
        {
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
    }
}
