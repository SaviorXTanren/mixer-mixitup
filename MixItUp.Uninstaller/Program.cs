using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MixItUp.Uninstaller
{
    public class Program
    {
        public const string MixItUpProcessName = "MixItUp";
        public static readonly string MixItUpStartMenuDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Mix It Up");
        public static readonly string DefaultInstallDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MixItUp");
        public const string MixItUpShortcutFileName = "Mix It Up.lnk";
        public static string MixItUpDesktopShortcutFilePath { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), MixItUpShortcutFileName); } }

        public const string SoftwareClassesRegistryPathPrefx = "SOFTWARE\\Classes\\";
        public const string MixItUpCommandFileExtension = ".miucommand";
        public const string MixItUpOldCommandFileExtension = ".mixitupc";
        public const string FileAssociationProgramID = "MixItUp.MIUCommand.1";
        public const string URIProtocolActivationHeader = "mixitup";

        public const string UninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        public static readonly Guid UninstallGuid = new Guid("9BED7BA2-4237-4826-B4C3-F3BB97F01151");

        [Flags]
        private enum MoveFileFlags
        {
            None = 0,
            ReplaceExisting = 1,
            CopyAllowed = 2,
            DelayUntilReboot = 4,
            WriteThrough = 8,
            CreateHardlink = 16,
            FailIfNotTrackable = 32,
        }

        private static class NativeMethods
        {
            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, MoveFileFlags dwFlags);
        }

        public static void Main(string[] args)
        {
            var tempPath = Path.GetTempPath();

            if (!Assembly.GetExecutingAssembly().Location.StartsWith(DefaultInstallDirectory, StringComparison.OrdinalIgnoreCase) &&
                !Assembly.GetExecutingAssembly().Location.StartsWith(tempPath, StringComparison.OrdinalIgnoreCase))
            {
                // Wait, we are running from some unexpected location, do nothing
                return;
            }

            // Uninstall process
            ////////////////////
            // Copy self to temp dir, run exe with old path
            if (args.Length == 0)
            {
                CopySelfToTempDirAndRun();
                return;
            }

            // Wait for processes to close
            System.Threading.Thread.Sleep(1000);
            WaitForMixItUpToClose();

            // Delete shortcuts
            DeleteShortcuts();

            // Delete registry
            DeleteRegistryChanges();

            // Delete files and Backup Settings
            string oldPath = args[0];
            DeleteFiles(oldPath);

            // Queue temp file for deletion
            NativeMethods.MoveFileEx(Assembly.GetExecutingAssembly().Location, null, MoveFileFlags.DelayUntilReboot);
        }

        private static void CopySelfToTempDirAndRun()
        {
            try
            {
                var tempPath = Path.GetTempPath();
                var randomDir = Guid.NewGuid().ToString();

                var tempDir = Path.Combine(tempPath, randomDir);
                Directory.CreateDirectory(tempDir);

                var exe = Assembly.GetExecutingAssembly().Location;
                var oldDir = Path.GetDirectoryName(exe);
                var fileName = Path.GetFileName(exe);
                var tempExe = Path.Combine(tempDir, fileName);

                File.Copy(exe, tempExe);

                Process.Start(tempExe, $"\"{oldDir}\"");
            }
            catch (Exception ex) { Log(ex.ToString()); }
        }

        private static void WaitForMixItUpToClose()
        {
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    bool isRunning = false;
                    foreach (Process clsProcess in Process.GetProcesses())
                    {
                        if (clsProcess.ProcessName.Equals(MixItUpProcessName))
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
                        return;
                    }

                    System.Threading.Thread.Sleep(1000);
                }
            }
            catch (Exception ex) { Log(ex.ToString()); }
        }

        private static void DeleteShortcuts()
        {
            try
            {
                if (Directory.Exists(MixItUpStartMenuDirectory))
                {
                    Directory.Delete(MixItUpStartMenuDirectory, true);
                }
            }

            catch (Exception ex) { Log(ex.ToString()); }
            try
            {
                if (File.Exists(MixItUpDesktopShortcutFilePath))
                {
                    File.Delete(MixItUpDesktopShortcutFilePath);
                }
            }
            catch (Exception ex) { Log(ex.ToString()); }
        }

        private static void DeleteRegistryChanges()
        {
            try
            {
                if (KeyExists(SoftwareClassesRegistryPathPrefx + URIProtocolActivationHeader))
                {
                    DeleteSubKeyTree(SoftwareClassesRegistryPathPrefx, URIProtocolActivationHeader);
                }
            }
            catch (Exception ex) { Log(ex.ToString()); }

            try
            {
                if (KeyExists(SoftwareClassesRegistryPathPrefx + MixItUpCommandFileExtension))
                {
                    DeleteSubKeyTree(SoftwareClassesRegistryPathPrefx, MixItUpCommandFileExtension);
                }
            }
            catch (Exception ex) { Log(ex.ToString()); }

            try
            {
                if (KeyExists(SoftwareClassesRegistryPathPrefx + FileAssociationProgramID))
                {
                    DeleteSubKeyTree(SoftwareClassesRegistryPathPrefx, FileAssociationProgramID);
                }
            }
            catch (Exception ex) { Log(ex.ToString()); }

            try
            {
                string guidText = UninstallGuid.ToString("B");
                if (KeyExists($@"{UninstallKey}\{guidText}"))
                {
                    DeleteSubKeyTree(UninstallKey, guidText);
                }
            }
            catch (Exception ex) { Log(ex.ToString()); }
        }

        private static void DeleteSubKeyTree(string registryPath, string keyToDelete)
        {
            using (var registryPathKey = Registry.CurrentUser.OpenSubKey(registryPath, true))
            {
                if (registryPathKey != null && !string.IsNullOrWhiteSpace(keyToDelete))
                {
                    try { registryPathKey.DeleteSubKeyTree(keyToDelete, throwOnMissingSubKey: false); }
                    catch (Exception ex) { Log(ex.ToString()); }
                }
            }
        }

        private static bool KeyExists(string path) { return Registry.CurrentUser.OpenSubKey(path) != null; }

        private static void DeleteFiles(string installDir)
        {
            string[] filePaths = Directory.GetFiles(installDir);
            foreach (string filePath in filePaths)
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex) { Log(ex.ToString()); }
            }

            string[] dirPaths = Directory.GetDirectories(installDir);
            foreach (string dirPath in dirPaths)
            {
                try
                {
                    string dirName = new DirectoryInfo(dirPath).Name;

                    // NOTE: Disabling the rename for now, this means settings will persist between uninstall and reinstall
                    // If a user wants to clear their settings, they can do that from inside Mix It Up (or by manually deleting the settings folder).

                    // If the Settings dir is found rename to some new date
                    // This will ensure that a reinstall won't use it, but it won't get deleted either
                    //if (string.Equals(dirName, "Settings", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    Directory.Move(dirPath, Path.Combine(installDir, $"Settings -{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}"));
                    //}

                    // Don't ever delete a folder that starts with "Settings"
                    if (dirName.StartsWith("Settings", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // Delete!
                    Directory.Delete(dirPath, true);
                }
                catch (Exception ex) { Log(ex.ToString()); }
            }
        }

        private static void Log(string message)
        {
            // TODO: Log
        }
    }
}
