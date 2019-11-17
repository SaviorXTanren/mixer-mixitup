using MixItUp.Base;
using MixItUp.Base.Util;
using MixItUp.Desktop.Services;
using MixItUp.WPF.Util;
using StreamingClient.Base.Util;
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

        public App()
        {
            // NOTE: Uncomment the lines below to test other cultures
            //System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("de-DE");
            //System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            //System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
        }

        public void SwitchTheme(string colorScheme, string backgroundColorName, string fullThemeName)
        {
            colorScheme = colorScheme.Replace(" ", "");

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

            DesktopServicesHandler desktopServicesHandler = new DesktopServicesHandler();
            desktopServicesHandler.Initialize();

            FileLoggerHandler.Initialize(desktopServicesHandler.FileService);

            DispatcherHelper.RegisterDispatcher(async (func) =>
            {
                await this.Dispatcher.Invoke(async () =>
                {
                    await func();
                });
            });
            SerializerHelper.Initialize(desktopServicesHandler.FileService);
            DialogHelper.Initialize(new WPFDialogShower());

            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            ChannelSession.Initialize(desktopServicesHandler);

            Logger.SetLogLevel(LogLevel.Information);
            Logger.Log(LogLevel.Information, "Application Version: " + ChannelSession.Services.FileService.GetApplicationVersion());
            Logger.SetLogLevel(LogLevel.Error);

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
                    using (StreamWriter writer = File.AppendText(FileLoggerHandler.CurrentLogFilePath))
                    {
                        writer.WriteLine("CRASHING EXCEPTION: " + Environment.NewLine + ex.ToString() + Environment.NewLine + Environment.StackTrace);
                    }
                }
                catch (Exception) { }

                ProcessHelper.LaunchProgram("MixItUp.Reporter.exe", string.Format("{0} {1}", (ChannelSession.MixerStreamerUser != null) ? ChannelSession.MixerStreamerUser.id : 0, FileLoggerHandler.CurrentLogFilePath));

                Task.Delay(3000).Wait();
            }
        }
    }
}
