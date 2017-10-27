using System.Threading.Tasks;
using MixItUp.Base.Services;
using System;
using System.IO;

namespace MixItUp.Mobile.Services
{
    public class AndroidFileService : IFileService
    {
        public Task CreateDirectory(string path)
        {
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), path);
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
                return await Task.Run(() =>
                {
                    return File.ReadAllText(filePath);
                });
            }
            catch (Exception) { }
            return null;
        }

        public async Task SaveFile(string filePath, string data, bool create = true)
        {
            try
            {
                await Task.Run(() =>
                {
                    if (create)
                    {
                        File.WriteAllText(filePath, data);
                    }
                    else
                    {
                        File.AppendAllText(filePath, data);
                    }
                });
            }
            catch (Exception) { }
        }

        public string ShowOpenFileDialog() { throw new NotImplementedException(); }

        public string ShowOpenFileDialog(string filter) { throw new NotImplementedException(); }

        public string ShowSaveFileDialog(string fileName) { throw new NotImplementedException(); }

        public string ShowSaveFileDialog(string fileName, string filter) { throw new NotImplementedException(); }
    }
}
