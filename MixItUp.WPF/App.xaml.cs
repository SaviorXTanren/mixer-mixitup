using MixItUp.Base;
using MixItUp.Base.Util;
using MixItUp.Desktop.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
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

        public void SwitchTheme(bool isDark = false)
        {
            var existingResourceDictionary = Application.Current.Resources.MergedDictionaries.Where(rd => rd.Source != null)
                .SingleOrDefault(rd => Regex.Match(rd.Source.OriginalString, @"(\/MaterialDesignThemes.Wpf;component\/Themes\/MaterialDesignTheme\.)((Light)|(Dark))").Success);
            if (existingResourceDictionary == null)
            {
                throw new ApplicationException("Unable to find Light/Dark base theme in Application resources.");
            }

            var source = $"pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.{(isDark ? "Dark" : "Light")}.xaml";
            var newResourceDictionary = new ResourceDictionary() { Source = new Uri(source) };

            Application.Current.Resources.MergedDictionaries.Remove(existingResourceDictionary);
            Application.Current.Resources.MergedDictionaries.Add(newResourceDictionary);

            var existingMahAppsResourceDictionary = Application.Current.Resources.MergedDictionaries.Where(rd => rd.Source != null)
                .SingleOrDefault(rd => Regex.Match(rd.Source.OriginalString, @"(\/MahApps.Metro;component\/Styles\/Accents\/)((BaseLight)|(BaseDark))").Success);

            if (existingMahAppsResourceDictionary == null)
            {
                return;
            }

            source = $"pack://application:,,,/MahApps.Metro;component/Styles/Accents/{(isDark ? "BaseDark" : "BaseLight")}.xaml";
            var newMahAppsResourceDictionary = new ResourceDictionary { Source = new Uri(source) };

            Application.Current.Resources.MergedDictionaries.Remove(existingMahAppsResourceDictionary);
            Application.Current.Resources.MergedDictionaries.Add(newMahAppsResourceDictionary);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            DesktopServicesHandler desktopServicesHandler = new DesktopServicesHandler();
            desktopServicesHandler.Initialize();

            Logger.Initialize(desktopServicesHandler.FileService);
            SerializerHelper.Initialize(desktopServicesHandler.FileService);

            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            ChannelSession.Initialize(desktopServicesHandler);

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

                Logger.Log("CRASH OCCURRED! CRASH EXCEPTION BELOW:");
                Logger.Log(ex);

                if (MessageBox.Show("Whoops! Looks like we ran into an issue and we'll have to close the program. Would you like to submit a bug to help us improve Mix It Up?", "Mix It Up - Crash", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Process.Start("https://github.com/SaviorXTanren/mixer-mixitup/issues");
                }
            }
        }
    }
}
