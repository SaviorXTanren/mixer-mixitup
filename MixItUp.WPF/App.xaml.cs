using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using MixItUp.Base;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.WPF.Services;
using MixItUp.WPF.Services.DeveloperAPI;
using MixItUp.WPF.Util;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace MixItUp.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private bool crashObtained = false;

        public App()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(Int32.MaxValue));

            try
            {
                // We need to load the language setting VERY early, so this is the minimal code necessary to get this value
                ServiceManager.Add<IDatabaseService>(new WindowsDatabaseService());
                ServiceManager.Add<IFileService>(new WindowsFileService());
                ServiceManager.Add<IInputService>(new WindowsInputService());
                ServiceManager.Add<IImageService>(new WindowsImageService());
                ServiceManager.Add<IAudioService>(new WindowsAudioService());
                ServiceManager.Add<IDeveloperAPIService>(new WindowsDeveloperAPIService());
                ServiceManager.Add<ITelemetryService>(new WindowsTelemetryService());
                ServiceManager.Add<IMusicPlayerService>(new WindowsMusicPlayerService());
                ServiceManager.Add<IProcessService>(new WindowsProcessService());
                ServiceManager.Add<IScriptRunnerService>(new WindowsScriptRunnerService());
                ServiceManager.Add(new WindowsMicrosoftAzureSpeechService());
                ServiceManager.Add(new StreamlabsService(new WindowsSocketIOConnection()));
                ServiceManager.Add(new RainmakerService(new WindowsSocketIOConnection()));
                ServiceManager.Add(new StreamElementsService(new WindowsSocketIOConnection()));
                ServiceManager.Add(new TipeeeStreamService(new WindowsSocketIOConnection()));
                ServiceManager.Add(new TreatStreamService(new WindowsSocketIOConnection()));
                ServiceManager.Add<IOvrStreamService>(new WindowsOvrStreamService());
                ServiceManager.Add<IOBSStudioService>(new WindowsOBSService());
                ServiceManager.Add(new WindowsSpeechService());
                ServiceManager.Add(new WindowsAmazonPollyService());

                ChannelSession.Initialize().Wait();

                System.Threading.Thread.CurrentThread.CurrentUICulture = Languages.GetLanguageLocaleCultureInfo();
            }
            catch { }
        }

        public void SwitchTheme(string colorScheme, string backgroundColorName, string fullThemeName)
        {
            string baseTheme = null;
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

                bool containsBaseTheme = false;
                foreach (string key in newMDCResourceDictionary.Keys)
                {
                    if (key.Equals("BaseTheme"))
                    {
                        containsBaseTheme = true;
                    }
                }

                if (containsBaseTheme)
                {
                    baseTheme = (string)newMDCResourceDictionary["BaseTheme"];
                }
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

            var themeSource = new Uri($"pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.{backgroundColorName}.xaml");
            if (!string.IsNullOrEmpty(baseTheme))
            {
                themeSource = new Uri($"Themes/MixItUpBaseTheme.{baseTheme}.xaml", UriKind.Relative);
            }
            var newMDTResourceDictionary = new ResourceDictionary() { Source = themeSource };

            Application.Current.Resources.MergedDictionaries.Add(newMDTResourceDictionary);

            // Change Mix It Up Light/Dark Theme
            var existingMIUResourceDictionary = Application.Current.Resources.MergedDictionaries.Where(rd => rd.Source != null)
                .SingleOrDefault(rd => Regex.Match(rd.Source.OriginalString, @"(MixItUpBackgroundColor\.)").Success);
            Application.Current.Resources.MergedDictionaries.Remove(existingMIUResourceDictionary);

            var newMIUResourceDictionary = new ResourceDictionary() { Source = new Uri($"Themes/MixItUpBackgroundColor.{backgroundColorName}.xaml", UriKind.Relative) };
            Application.Current.Resources.MergedDictionaries.Add(newMIUResourceDictionary);

            LiveCharts.Configure(config =>
            {
                config.AddSkiaSharp().AddDefaultMappers();
                config.AddLightTheme();
            });
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ActivationProtocolHandler.Initialize();

            RegistryHelpers.RegisterFileAssociation();
            RegistryHelpers.RegisterURIActivationProtocol();
            // Disabled for now until we can figure out why anti-virus hates it
            // RegistryHelpers.RegisterUninstaller();

            FileLoggerHandler.Initialize();

            DispatcherHelper.RegisterDispatcher(new WindowsDispatcher(this.Dispatcher));

            DialogHelper.Initialize(new WPFDialogShower());

            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            try
            {
                WindowsIdentity id = WindowsIdentity.GetCurrent();
                ChannelSession.IsElevated = id.Owner != id.User;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            Logger.ForceLog(LogLevel.Information, "Application Version: " + ServiceManager.Get<IFileService>().GetApplicationVersion());
            Logger.AlwaysLogFullStackTraceWithExceptions = true;
            if (ChannelSession.IsDebug() || (ChannelSession.AppSettings != null && ChannelSession.AppSettings.DiagnosticLogging))
            {
                Logger.SetLogLevel(LogLevel.Debug);
            }
            else
            {
                Logger.SetLogLevel(LogLevel.Error);
            }

            this.SwitchTheme(ChannelSession.AppSettings.ColorScheme, ChannelSession.AppSettings.BackgroundColor, ChannelSession.AppSettings.FullThemeName);

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ActivationProtocolHandler.Close();

            base.OnExit(e);
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

                if (ServiceManager.Has<ITelemetryService>())
                {
                    ServiceManager.Get<ITelemetryService>().TrackException(ex);
                }

                try
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (Exception e in ex.UnwrapException())
                    {
                        sb.AppendLine(e.ToString());
                        sb.AppendLine();
                    }

                    using (StreamWriter writer = File.AppendText(FileLoggerHandler.CurrentLogFilePath))
                    {
                        writer.WriteLine("CRASHING EXCEPTION: " + Environment.NewLine + sb.ToString() + Environment.NewLine + Environment.StackTrace);
                    }
                }
                catch (Exception) { }

                ServiceManager.Get<IProcessService>().LaunchProgram("MixItUp.Reporter.exe", $"{FileLoggerHandler.CurrentLogFilePath} {ChannelSession.Settings?.Name ?? "NONE"}");

                Task.Delay(3000).Wait();
            }
        }
    }
}
