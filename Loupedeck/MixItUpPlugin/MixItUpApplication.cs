using Loupedeck;
using System;
using System.IO;

namespace MixItUp.LoupedeckPlugin
{
    // This class can be used to connect the Loupedeck plugin to an application.

    public class MixItUpApplication : ClientApplication
    {
        public static readonly string DefaultInstallDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MixItUp");

        public MixItUpApplication()
        {
        }

        // This method can be used to link the plugin to a Windows application.
        protected override string GetProcessName() => "MixItUp";

        // This method can be used to link the plugin to a macOS application.
        protected override string GetBundleName() => "";

        // This method can be used to check whether the application is installed or not.
        public override ClientApplicationStatus GetApplicationStatus()
        {
            //return Directory.Exists(DefaultInstallDirectory) && File.Exists(Path.Combine(DefaultInstallDirectory, this.GetProcessName()))
            //    ? ClientApplicationStatus.Installed
            //    : ClientApplicationStatus.NotInstalled;
            return ClientApplicationStatus.NotInstalled;
        }
    }
}
