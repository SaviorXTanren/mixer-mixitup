using MixItUp.Base;
using MixItUp.Base.Localization;
using MixItUp.Base.Util;
using MixItUp.Desktop.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace MixItUp.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ApplicationSettings AppSettings;

        private bool crashObtained = false;

        public void SwitchTheme(string colorScheme, string backgroundColorName, string fullThemeName)
        {
            colorScheme = colorScheme.Replace(" ", "");
            if (string.IsNullOrEmpty(fullThemeName) || fullThemeName.Equals("None"))
            {
                fullThemeName = null;
            }

            // Change Material Design Color Scheme
            var existingMDCResourceDictionary = Application.Current.Resources.MergedDictionaries.Where(rd => rd.Source != null)
                .SingleOrDefault(rd => Regex.Match(rd.Source.OriginalString, @"(\/MaterialDesignColors;component\/Themes\/Recommended\/Primary\/MaterialDesignColor\.)").Success);
            if (existingMDCResourceDictionary == null)
            {
                throw new ApplicationException("Unable to find Color scheme in Application resources.");
            }
            Application.Current.Resources.MergedDictionaries.Remove(existingMDCResourceDictionary);

            var newMDCResourceDictionary = new ResourceDictionary();
            if (!string.IsNullOrEmpty(fullThemeName))
            {
                newMDCResourceDictionary.Source = new Uri($"Themes/MixItUpTheme.{fullThemeName}.xaml", UriKind.Relative);
                SolidColorBrush mainApplicationBackground = (SolidColorBrush)newMDCResourceDictionary["MainApplicationBackground"];
                backgroundColorName = (mainApplicationBackground.ToString().Equals("#FFFFFFFF")) ? "Light" : "Dark";
            }
            else
            {
                newMDCResourceDictionary.Source = new Uri($"pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Primary/MaterialDesignColor.{colorScheme}.xaml");
            }
            Application.Current.Resources.MergedDictionaries.Add(newMDCResourceDictionary);

            // Change Material Design Light/Dark Theme
            var existingMDTResourceDictionary = Application.Current.Resources.MergedDictionaries.Where(rd => rd.Source != null)
                .SingleOrDefault(rd => Regex.Match(rd.Source.OriginalString, @"(\/MaterialDesignThemes.Wpf;component\/Themes\/MaterialDesignTheme\.)((Light)|(Dark))").Success);
            if (existingMDTResourceDictionary == null)
            {
                throw new ApplicationException("Unable to find Light/Dark base theme in Application resources.");
            }
            Application.Current.Resources.MergedDictionaries.Remove(existingMDTResourceDictionary);

            var themeSource = $"pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.{backgroundColorName}.xaml";
            var newMDTResourceDictionary = new ResourceDictionary() { Source = new Uri(themeSource) };

            Application.Current.Resources.MergedDictionaries.Add(newMDTResourceDictionary);

            // Change Mix It Up Light/Dark Theme
            var existingMIUResourceDictionary = Application.Current.Resources.MergedDictionaries.Where(rd => rd.Source != null)
                .SingleOrDefault(rd => Regex.Match(rd.Source.OriginalString, @"(MixItUpBackgroundColor\.)").Success);
            Application.Current.Resources.MergedDictionaries.Remove(existingMIUResourceDictionary);

            var newMIUResourceDictionary = new ResourceDictionary() { Source = new Uri($"Themes/MixItUpBackgroundColor.{backgroundColorName}.xaml", UriKind.Relative) };
            Application.Current.Resources.MergedDictionaries.Add(newMIUResourceDictionary);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            App.AppSettings = ApplicationSettings.Load();
            this.SwitchTheme(App.AppSettings.ColorScheme, App.AppSettings.BackgroundColor, App.AppSettings.FullThemeName);

            LocalizationHandler.SetCurrentLanguage(App.AppSettings.Language);

            DesktopServicesHandler desktopServicesHandler = new DesktopServicesHandler();
            desktopServicesHandler.Initialize();

            Logger.Initialize(desktopServicesHandler.FileService);
            SerializerHelper.Initialize(desktopServicesHandler.FileService);

            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            ChannelSession.Initialize(desktopServicesHandler);

            Logger.Log("Application Version: " + ChannelSession.Services.FileService.GetApplicationVersion());

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

                if (ChannelSession.Services.Telemetry != null)
                {
                    ChannelSession.Services.Telemetry.TrackException(ex);
                }

                try
                {
                    using (StreamWriter writer = File.AppendText(Logger.CurrentLogFilePath))
                    {
                        writer.WriteLine("CRASHING EXCEPTION: " + Environment.NewLine + ex.ToString());
                    }
                }
                catch (Exception) { }
                Logger.Log(ex, includeFullStackTrace: false, isCrashing: true);

                string reporterFilePath = Path.Combine(ChannelSession.Services.FileService.GetApplicationDirectory(), "MixItUp.Reporter.exe");
                ProcessStartInfo processStartInfo = new ProcessStartInfo(reporterFilePath, string.Format("{0} {1}", (ChannelSession.User != null) ? ChannelSession.User.id : 0, Logger.CurrentLogFilePath));
                Process.Start(processStartInfo);

                Task.Delay(1000).Wait();
            }
        }
    }
}
