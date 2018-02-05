using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IFileService
    {
        Task CreateDirectory(string path);

        Task<string> OpenFile(string filePath);

        Task CreateFile(string filePath, string data);
        Task AppendFile(string filePath, string data);

        string ShowOpenFolderDialog();

        string ShowOpenFileDialog();
        string ShowOpenFileDialog(string filter);

        string ShowSaveFileDialog(string fileName);
        string ShowSaveFileDialog(string fileName, string filter);
    }
}
