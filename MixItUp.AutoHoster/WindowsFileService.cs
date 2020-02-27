using MixItUp.Base.Services;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MixItUp.AutoHoster
{
    public class WindowsFileService : IFileService
    {
        private static readonly List<string> webPathPrefixes = new List<string>() { "http://", "https://", "www." };

        private static SemaphoreSlim fileLock = new SemaphoreSlim(1);

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
                File.Copy(sourcePath, destinationPath, overwrite: true);
            }
        }

        public Task DeleteFile(string filePath)
        {
            File.Delete(filePath);
            return Task.FromResult(0);
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
                string safeFilePath = filePath.ToFilePathString();
                if (File.Exists(filePath))
                {
                    using (StreamReader reader = new StreamReader(File.OpenRead(filePath)))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }
                else if (File.Exists(safeFilePath))
                {
                    using (StreamReader reader = new StreamReader(File.OpenRead(safeFilePath)))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }
                else if (webPathPrefixes.Any(p => filePath.StartsWith(p, StringComparison.InvariantCultureIgnoreCase)))
                {
                    using (WebClient client = new WebClient())
                    {
                        return await Task.Run(async () => { return await client.DownloadStringTaskAsync(filePath); });
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task<byte[]> ReadFileAsBytes(string filePath)
        {
            try
            {
                string safeFilePath = filePath.ToFilePathString();
                if (File.Exists(filePath))
                {
                    using (FileStream reader = File.OpenRead(filePath))
                    {
                        byte[] data = new byte[reader.Length];
                        await reader.ReadAsync(data, 0, data.Length);
                        return data;
                    }
                }
                else if (File.Exists(safeFilePath))
                {
                    using (FileStream reader = File.OpenRead(safeFilePath))
                    {
                        byte[] data = new byte[reader.Length];
                        await reader.ReadAsync(data, 0, data.Length);
                        return data;
                    }
                }
                else if (webPathPrefixes.Any(p => filePath.StartsWith(p, StringComparison.InvariantCultureIgnoreCase)))
                {
                    using (WebClient client = new WebClient())
                    {
                        return await Task.Run(async () => { return await client.DownloadDataTaskAsync(filePath); });
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public async Task SaveFile(string filePath, string data)
        {
            await WindowsFileService.fileLock.WaitAndRelease(async () =>
            {
                using (StreamWriter writer = new StreamWriter(File.Open(filePath, FileMode.Create)))
                {
                    if (!string.IsNullOrEmpty(data))
                    {
                        await writer.WriteAsync(data);
                    }
                    await writer.FlushAsync();
                }
            });
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
            await WindowsFileService.fileLock.WaitAndRelease(async () =>
            {
                using (StreamWriter writer = new StreamWriter(File.Open(filePath, FileMode.Append)))
                {
                    if (!string.IsNullOrEmpty(data))
                    {
                        await writer.WriteAsync(data);
                    }
                    await writer.FlushAsync();
                }
            });
        }

        public string ShowOpenFolderDialog()
        {
            throw new NotImplementedException();
        }

        public string ShowOpenFileDialog() { return this.ShowOpenFileDialog("All files (*.*)|*.*"); }

        public string ShowOpenFileDialog(string filter)
        {
            throw new NotImplementedException();
        }

        public string ShowSaveFileDialog(string fileName) { return this.ShowSaveFileDialog(fileName, "All files (*.*)|*.*"); }

        public string ShowSaveFileDialog(string fileName, string filter)
        {
            throw new NotImplementedException();
        }

        public Task ZipFiles(string destinationFilePath, IEnumerable<string> filePathsToBeAdded)
        {
            throw new NotImplementedException();
        }

        public Task UnzipFiles(string zipFilePath, string destinationFolderPath)
        {
            throw new NotImplementedException();
        }

        public string GetTempFolder() { return Path.GetTempPath(); }

        public string GetApplicationDirectory() { return Path.GetDirectoryName(typeof(IFileService).Assembly.Location); }

        public string GetApplicationVersion() { return Assembly.GetEntryAssembly().GetName().Version.ToString().Trim(); }
    }
}
