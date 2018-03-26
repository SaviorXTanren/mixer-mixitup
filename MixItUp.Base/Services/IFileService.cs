using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IFileService
    {
        Task CreateDirectory(string path);

        Task<string> ReadFile(string filePath);
        Task SaveFile(string filePath, string data);
        Task AppendFile(string filePath, string data);

        string ShowOpenFolderDialog();

        string ShowOpenFileDialog();
        string ShowOpenFileDialog(string filter);

        string ShowSaveFileDialog(string fileName);
        string ShowSaveFileDialog(string fileName, string filter);
    }
}
