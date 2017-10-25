using MixItUp.Base.Util;
using MixItUp.Desktop.Files;
using MixItUp.Desktop.Services;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace MixItUp.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private bool crashObtained = false;

        protected override void OnStartup(StartupEventArgs e)
        {
            Logger.Initialize(new WindowsFileService());
            SerializerHelper.Initialize(new WindowsFileService());

            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            
            ChannelSession.Initialize(new DesktopServicesHandler());

            base.OnStartup(e);
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) { this.HandleCrash(e.Exception); }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) { this.HandleCrash((Exception)e.ExceptionObject); }

        private void HandleCrash(Exception ex)
        {
            if (!this.crashObtained)
            {
                #if DEBUG
                    Debugger.Break();
                #endif

                this.crashObtained = true;

                Logger.Log(ex);

                if (MessageBox.Show("Whoops! Looks like we ran into an issue and we'll have to close the program. Would you like to submit a bug to help us improve Mix It Up?", "Mix It Up - Crash", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Process.Start("https://github.com/SaviorXTanren/mixer-mixitup/issues");
                }
            }
        }
    }
}
