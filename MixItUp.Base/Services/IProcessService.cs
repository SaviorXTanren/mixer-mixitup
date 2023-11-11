using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IProcessService
    {
        Task<int> GetCPUUsage();

        float GetMemoryUsage();

        void LaunchLink(string url);

        void LaunchFolder(string folderPath);

        void LaunchProgram(string filePath, string arguments = "");

        IEnumerable<Process> GetProcessesByName(string name);
    }
}
