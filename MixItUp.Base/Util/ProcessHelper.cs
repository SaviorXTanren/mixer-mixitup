using System.Diagnostics;
using System.IO;

namespace MixItUp.Base.Util
{
    public static class ProcessHelper
    {
        public static void LaunchLink(string url)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo(url)
            {
                UseShellExecute = true
            };
            Process.Start(processInfo);
        }

        public static void LaunchFolder(string folderPath)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo(ProcessHelper.GetRootedPath(folderPath))
            {
                UseShellExecute = true
            };
            Process.Start(processInfo);
        }

        public static void LaunchProgram(string filePath, string arguments = "")
        {
            ProcessStartInfo processInfo = new ProcessStartInfo(ProcessHelper.GetRootedPath(filePath), arguments)
            {
                UseShellExecute = true,
            };
            Process.Start(processInfo);
        }

        private static string GetRootedPath(string path)
        {
            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), path);
            }
            return path;
        }
    }
}
