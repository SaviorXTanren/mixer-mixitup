using Microsoft.Win32;
using MixItUp.Base.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Files
{
    public class WindowsFileService : IFileService
    {
        public Task CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return Task.FromResult(0);
        }

        public async Task<string> OpenFile(string filePath)
        {
            try
            {
                using (StreamReader reader = new StreamReader(File.OpenRead(filePath)))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            catch (Exception) { }
            return null;
        }

        public async Task SaveFile(string filePath, string data, bool create = true)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(File.Open(filePath, create ? FileMode.Create : FileMode.Append)))
                {
                    await writer.WriteAsync(data);
                }
            }
            catch (Exception) { }
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
