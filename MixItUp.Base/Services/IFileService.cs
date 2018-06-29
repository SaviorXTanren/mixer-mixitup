using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IFileService
    {
        string ImageFileFilter();
        string MusicFileFilter();
        string VideoFileFilter();
        string HTMLFileFilter();

        Task CopyFile(string sourcePath, string destinationPath);

        Task CreateDirectory(string path);
        Task CopyDirectory(string sourcePath, string destinationPath);

        Task<IEnumerable<string>> GetFilesInDirectory(string directoryPath);

        bool FileExists(string filePath);
        Task<string> ReadFile(string filePath);
        Task<byte[]> ReadFileAsBytes(string filePath);
        Task SaveFile(string filePath, string data);
        Task SaveFileAsBytes(string filePath, byte[] data);
        Task AppendFile(string filePath, string data);

        string ShowOpenFolderDialog();

        string ShowOpenFileDialog();
        string ShowOpenFileDialog(string filter);

        string ShowSaveFileDialog(string fileName);
        string ShowSaveFileDialog(string fileName, string filter);

        Task ZipFiles(string destinationFilePath, IEnumerable<string> filePathsToBeAdded);
        Task UnzipFiles(string zipFilePath, string destinationFolderPath);

        string GetTempFolder();
        string GetApplicationDirectory();
        string GetApplicationVersion();
    }
}
