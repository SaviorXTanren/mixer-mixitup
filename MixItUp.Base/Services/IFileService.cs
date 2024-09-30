using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IFileService
    {
        string TextFileFilter();
        string ImageFileFilter();
        string SoundFileFilter();
        string VideoFileFilter();
        string HTMLFileFilter();

        Task CopyFile(string sourcePath, string destinationPath);
        Task DeleteFile(string filePath);

        Task CreateDirectory(string path);
        Task CopyDirectory(string sourcePath, string destinationPath);

        Task<IEnumerable<string>> GetFilesInDirectory(string directoryPath);

        bool FileExists(string filePath);
        bool IsURLPath(string filePath);
        long GetFileSize(string filePath);
        Task<string> ReadFile(string filePath);
        Task<byte[]> ReadFileAsBytes(string filePath);
        Task SaveFile(string filePath, string data);
        Task SaveFile(string filePath, byte[] data);
        Task SaveFile(string filePath, Stream data);
        Task AppendFile(string filePath, string data);

        string ShowOpenFolderDialog();

        string ShowOpenFileDialog();
        string ShowOpenFileDialog(string filter);
        IEnumerable<string> ShowMultiselectOpenFileDialog(string filter);

        string ShowSaveFileDialog(string fileName);
        string ShowSaveFileDialog(string fileName, string filter);

        Task ZipFiles(string destinationFilePath, IEnumerable<string> filePathsToBeAdded);
        Task UnzipFiles(string zipFilePath, string destinationFolderPath);

        string GetTempFolder();
        string GetApplicationDirectory();
        string GetApplicationVersion();

        IEnumerable<string> GetInstalledFonts();

        string ExpandEnvironmentVariablesInFilePath(string path);
    }
}
