using Microsoft.Win32;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Files
{
    public class WindowsFileService : IFileService
    {
        private static SemaphoreSlim createLock = new SemaphoreSlim(1);
        private static SemaphoreSlim appendLock = new SemaphoreSlim(1);

        public string ImageFileFilter() { return "All Picture Files|*.bmp;*.gif;*.jpg;*.jpeg;*.png;|All files (*.*)|*.*"; }
        public string MusicFileFilter() { return "MP3 Files (*.mp3)|*.mp3|All files (*.*)|*.*"; }
        public string VideoFileFilter() { return "MP4/WEBM Files|*.mp4;*.webm|All files (*.*)|*.*"; }
        public string HTMLFileFilter() { return "HTML Files (*.html)|*.html|All files (*.*)|*.*"; }

        public async Task CopyFile(string sourcePath, string destinationPath)
        {
            if (File.Exists(sourcePath))
            {
                string destinationDirectory = Path.GetDirectoryName(destinationPath);
                await this.CreateDirectory(destinationDirectory);
                File.Copy(sourcePath, destinationPath);
            }
        }

        public Task CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return Task.FromResult(0);
        }

        public async Task CopyDirectory(string sourceDirectoryPath, string destinationDirectoryPath)
        {
            if (Directory.Exists(sourceDirectoryPath))
            {
                await this.CreateDirectory(destinationDirectoryPath);
                foreach (string filepath in Directory.GetFiles(sourceDirectoryPath))
                {
                    File.Copy(filepath, filepath.Replace(sourceDirectoryPath, destinationDirectoryPath));
                }
            }
        }

        public Task<IEnumerable<string>> GetFilesInDirectory(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                return Task.FromResult<IEnumerable<string>>(Directory.GetFiles(directoryPath));
            }
            return Task.FromResult<IEnumerable<string>>(new List<string>());
        }

        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public async Task<string> ReadFile(string filePath)
        {
            try
            {
                using (StreamReader reader = new StreamReader(File.OpenRead(filePath)))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<byte[]> ReadFileAsBytes(string filePath)
        {
            try
            {
                using (FileStream reader = File.OpenRead(filePath))
                {
                    byte[] data = new byte[reader.Length];
                    await reader.ReadAsync(data, 0, data.Length);
                    return data;
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task SaveFile(string filePath, string data)
        {
            try
            {
                await WindowsFileService.createLock.WaitAsync();
                using (StreamWriter writer = new StreamWriter(File.Open(filePath, FileMode.Create)))
                {
                    if (!string.IsNullOrEmpty(data))
                    {
                        await writer.WriteAsync(data);
                    }
                    await writer.FlushAsync();
                }
                WindowsFileService.createLock.Release();
            }
            catch (Exception ex)
            {
                WindowsFileService.createLock.Release();
                Logger.Log(ex);
            }
        }

        public async Task SaveFileAsBytes(string filePath, byte[] data)
        {
            try
            {
                using (FileStream reader = File.OpenWrite(filePath))
                {
                    await reader.WriteAsync(data, 0, data.Length);
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public async Task AppendFile(string filePath, string data)
        {
            try
            {
                await WindowsFileService.appendLock.WaitAsync();
                using (StreamWriter writer = new StreamWriter(File.Open(filePath, FileMode.Append)))
                {
                    if (!string.IsNullOrEmpty(data))
                    {
                        await writer.WriteAsync(data);
                    }
                    await writer.FlushAsync();
                }
                WindowsFileService.appendLock.Release();
            }
            catch (Exception ex)
            {
                WindowsFileService.appendLock.Release();
                Logger.Log(ex);
            }
        }

        public string ShowOpenFolderDialog()
        { 
            using (var folderDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    return folderDialog.SelectedPath;
                }
            }
            return null;
        }

        public string ShowOpenFileDialog() { return this.ShowOpenFileDialog("All files (*.*)|*.*"); }

        public string ShowOpenFileDialog(string filter)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = filter;
            fileDialog.CheckFileExists = true;
            fileDialog.CheckPathExists = true;
            if (fileDialog.ShowDialog() == true)
            {
                return fileDialog.FileName;
            }
            return null;
        }

        public string ShowSaveFileDialog(string fileName) { return this.ShowSaveFileDialog(fileName, "All files (*.*)|*.*"); }

        public string ShowSaveFileDialog(string fileName, string filter)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Filter = filter;
            fileDialog.CheckPathExists = true;
            fileDialog.FileName = fileName;
            if (fileDialog.ShowDialog() == true)
            {
                return fileDialog.FileName;
            }
            return null;
        }

        public async Task ZipFiles(string destinationFilePath, IEnumerable<string> filePathsToBeAdded)
        {
            string tempDirectory = Path.Combine(this.GetTempFolder(), Path.GetRandomFileName());
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
            Directory.CreateDirectory(tempDirectory);

            foreach (string filePathToBeAdded in filePathsToBeAdded)
            {
                await this.CopyFile(filePathToBeAdded, Path.Combine(tempDirectory, Path.GetFileName(filePathToBeAdded)));
            }

            ZipFile.CreateFromDirectory(tempDirectory, destinationFilePath);
        }

        public async Task UnzipFiles(string zipFilePath, string destinationFolderPath)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (File.Exists(zipFilePath))
                    {
                        using (FileStream stream = File.OpenRead(zipFilePath))
                        {
                            ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read);
                            foreach (ZipArchiveEntry file in archive.Entries)
                            {
                                string fullPath = Path.Combine(destinationFolderPath, file.FullName);
                                if (string.IsNullOrEmpty(file.Name))
                                {
                                    string directoryPath = Path.GetDirectoryName(fullPath);
                                    if (!Directory.Exists(directoryPath))
                                    {
                                        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                                    }
                                }
                                else
                                {
                                    file.ExtractToFile(fullPath, true);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            });
        }

        public string GetTempFolder() { return Path.GetTempPath(); }

        public string GetApplicationDirectory() { return Path.GetDirectoryName(typeof(IFileService).Assembly.Location); }

        public string GetApplicationVersion() { return Assembly.GetEntryAssembly().GetName().Version.ToString().Trim(); }
    }
}
