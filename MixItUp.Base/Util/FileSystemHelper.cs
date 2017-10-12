using Microsoft.Win32;

namespace MixItUp.Base.Util
{
    public static class FileSystemHelper
    {
        public static string ShowOpenFileDialog() { return FileSystemHelper.ShowOpenFileDialog("All files (*.*)|*.*"); }

        public static string ShowOpenFileDialog(string filter)
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

        public static string ShowSaveFileDialog(string fileName) { return FileSystemHelper.ShowSaveFileDialog(fileName, "All files (*.*)|*.*"); }

        public static string ShowSaveFileDialog(string fileName, string filter)
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
