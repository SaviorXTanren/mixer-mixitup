using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace MixItUp.WPF.Services
{
    public class WindowsProcessService : IProcessService
    {
        public void LaunchLink(string url)
        {
            try
            {
                if (!string.IsNullOrEmpty(url))
                {
                    ProcessStartInfo processInfo = new ProcessStartInfo(url)
                    {
                        UseShellExecute = true
                    };
                    Process.Start(processInfo);
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public void LaunchFolder(string folderPath)
        {
            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo(this.GetRootedPath(folderPath))
                {
                    UseShellExecute = true
                };
                Process.Start(processInfo);
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public void LaunchProgram(string filePath, string arguments = "")
        {
            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo(this.GetRootedPath(filePath), arguments)
                {
                    UseShellExecute = true,
                };
                Process.Start(processInfo);
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public IEnumerable<Process> GetProcessesByName(string name)
        {
            try
            {
                return Process.GetProcessesByName(name);
            }
            catch (Exception ex) { Logger.Log(ex); }
            return new List<Process>();
        }

        private string GetRootedPath(string path)
        {
            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), path);
            }
            return path;
        }
    }
}
