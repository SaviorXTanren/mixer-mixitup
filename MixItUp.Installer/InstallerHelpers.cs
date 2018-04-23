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

        public static bool DownloadMixItUp()
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
                    if (Directory.Exists(InstallDirectory))
                    {
                        Directory.Delete(InstallDirectory, recursive: true);
                    }

                    Directory.CreateDirectory(InstallDirectory);
                    if (Directory.Exists(InstallDirectory))
                    {
                        ZipFile.ExtractToDirectory(updateFilePath, InstallDirectory);
                        return true;
                    }
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
