using System.Collections.Generic;
using System.Diagnostics;

namespace MixItUp.Base.Services
{
    public interface IProcessService
    {
        void LaunchLink(string url);

        void LaunchFolder(string folderPath);

        void LaunchProgram(string filePath, string arguments = "");

        IEnumerable<Process> GetProcessesByName(string name);
    }
}
