using MixItUp.Base;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Util;
using MixItUp.WPF.Services;
using MixItUp.WPF.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
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
        private bool crashObtained = false;

        private static readonly Dictionary<LanguageOptions, string> LanguageMaps = new Dictionary<LanguageOptions, string>
        {
            // { LanguageOptions.Default, "DO NOT USE, MAPS TO MACHINE CULTURE" },

            { LanguageOptions.Dutch, "nl-NL" },
            { LanguageOptions.English, "en-US" },
            { LanguageOptions.German, "de-DE" },
            { LanguageOptions.Spanish, "es-ES" },
            { LanguageOptions.Japanese, "ja-JP" },
            { LanguageOptions.French, "fr-FR" },
            { LanguageOptions.Portuguese, "pt-BR" },

            { LanguageOptions.Pseudo, "qps-ploc" },
        };

        public App()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            try
            {
                // We need to load the language setting VERY early, so this is the minimal code necessary to get this value
                WindowsServicesManager servicesManager = new WindowsServicesManager();
                servicesManager.Initialize();
                ChannelSession.Initialize(servicesManager).Wait();
                var selectedLanguageTask = ApplicationSettingsV2Model.Load();
                selectedLanguageTask.Wait();

                var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
                if (LanguageMaps.TryGetValue(selectedLanguageTask.Result.LanguageOption, out string locale))
                {
                    culture = new System.Globalization.CultureInfo(locale);
                }

                System.Threading.Thread.CurrentThread.CurrentCulture = culture;
                System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
            }
            catch { }
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

        protected override async void OnStartup(StartupEventArgs e)
        {
            WindowsServicesManager servicesManager = new WindowsServicesManager();
            servicesManager.Initialize();

            FileLoggerHandler.Initialize(servicesManager.FileService);

            DispatcherHelper.RegisterDispatcher(async (func) =>
            {
                await this.Dispatcher.Invoke(async () =>
                {
                    await func();
                });
            });
            DialogHelper.Initialize(new WPFDialogShower());

            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            await ChannelSession.Initialize(servicesManager);

            WindowsIdentity id = WindowsIdentity.GetCurrent();
            ChannelSession.IsElevated = id.Owner != id.User;

            Logger.ForceLog(LogLevel.Information, "Application Version: " + ChannelSession.Services.FileService.GetApplicationVersion());
            if (ChannelSession.IsDebug())
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

                ProcessHelper.LaunchProgram("MixItUp.Reporter.exe", string.Format("{0} {1}", (ChannelSession.MixerUser != null) ? ChannelSession.MixerUser.id : 0, FileLoggerHandler.CurrentLogFilePath));

                Task.Delay(3000).Wait();
            }
        }
    }
}
