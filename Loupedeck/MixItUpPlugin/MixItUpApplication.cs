using Loupedeck;
using System;
using System.IO;

namespace MixItUp.LoupedeckPlugin
{
    public class MixItUpApplication : ClientApplication
    {
        public static readonly string DefaultInstallDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MixItUp");

        public MixItUpApplication()
        {
        }

        // This method can be used to link the plugin to a Windows application and enable dynamic workspaces
        //protected override string GetProcessName() => "MixItUp";

        public override ClientApplicationStatus GetApplicationStatus()
        {
            return Directory.Exists(DefaultInstallDirectory) && File.Exists(Path.Combine(DefaultInstallDirectory, "MixItUp.exe"))
                ? ClientApplicationStatus.Installed
                : ClientApplicationStatus.NotInstalled;
        }
    }
}
