using Microsoft.Win32;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Files
{
    public class WindowsFileService : IFileService
    {
        private static SemaphoreSlim createLock = new SemaphoreSlim(1);
        private static SemaphoreSlim appendLock = new SemaphoreSlim(1);

        public Task CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return Task.FromResult(0);
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

        public async Task SaveFile(string filePath, string data)
        {
            try
            {
                await WindowsFileService.createLock.WaitAsync();
                using (StreamWriter writer = new StreamWriter(File.Open(filePath, FileMode.Create)))
                {
                    await writer.WriteAsync(data);
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

        public async Task AppendFile(string filePath, string data)
        {
            try
            {
                await WindowsFileService.appendLock.WaitAsync();
                using (StreamWriter writer = new StreamWriter(File.Open(filePath, FileMode.Append)))
                {
                    await writer.WriteAsync(data);
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
    }
}
