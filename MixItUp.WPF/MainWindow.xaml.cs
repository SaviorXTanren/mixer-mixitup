using MaterialDesignThemes.Wpf;
using MixItUp.Base;
using MixItUp.Base.Util;
using MixItUp.WPF.Controls.MainControls;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF
{
    /// <summary>
    /// Interaction logic for StreamerWindow.xaml
    /// </summary>
    public partial class MainWindow : LoadingWindowBase
    {
        public string RestoredSettingsFilePath = null;

        private bool restartApplication = false;

        private bool shutdownStarted = false;
        private bool shutdownComplete = false;

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += MainWindow_Closing;
            this.Initialize(this.StatusBar);

            if (App.AppSettings.Width > 0)
            {
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.Height = App.AppSettings.Height;
                this.Width = App.AppSettings.Width;
                this.Top = App.AppSettings.Top;
                this.Left = App.AppSettings.Left;

                if (App.AppSettings.IsMaximized)
                {
                    WindowState = WindowState.Maximized;
                }
            }
        }

        public void Restart()
        {
            this.restartApplication = true;
            this.Close();
        }

        public void ReRunWizard()
        {
            ChannelSession.Settings.ReRunWizard = true;
            this.Restart();
        }

        protected override async Task OnLoaded()
        {
            if (ChannelSession.Settings.IsStreamer)
            {
                this.Title += " - Streamer";
            }
            else
            {
                this.Title += " - Moderator";
            }

            if (!string.IsNullOrEmpty(ChannelSession.Channel?.user?.username))
            {
                this.Title += " - " + ChannelSession.Channel.user.username;
            }

            this.Title += " - v" + Assembly.GetEntryAssembly().GetName().Version.ToString();

            await this.MainMenu.Initialize(this);

            await this.MainMenu.AddMenuItem("Chat", new ChatControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/Chat");
            if (ChannelSession.Settings.IsStreamer)
            {
                await this.MainMenu.AddMenuItem("Channel", new ChannelControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/Channel");
                await this.MainMenu.AddMenuItem("Commands", new ChatCommandsControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/Commands");
                await this.MainMenu.AddMenuItem("MixPlay", new InteractiveControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/MixPlay");
                await this.MainMenu.AddMenuItem("Events", new EventsControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/Events");
                await this.MainMenu.AddMenuItem("Timers", new TimerControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/Timers");
                await this.MainMenu.AddMenuItem("Action Groups", new ActionGroupControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/Action-Groups");
                await this.MainMenu.AddMenuItem("Remote", new RemoteControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/Remote");
                await this.MainMenu.AddMenuItem("Users", new UsersControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/Users");
                await this.MainMenu.AddMenuItem("Currency & Rank", new CurrencyAndRankControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/Currency-&-Rank");
                await this.MainMenu.AddMenuItem("Games", new GamesControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/Games");
                await this.MainMenu.AddMenuItem("Giveaway", new GiveawayControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/Giveaways");
                await this.MainMenu.AddMenuItem("Game Queue", new GameQueueControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/Game-Queue");
                await this.MainMenu.AddMenuItem("Song Requests", new SongRequestControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/Song-Requests");
                await this.MainMenu.AddMenuItem("Quotes", new QuoteControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/Quotes");
            }
            await this.MainMenu.AddMenuItem("Statistics", new StatisticsControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/Statistics");
            if (ChannelSession.Settings.IsStreamer)
            {
                await this.MainMenu.AddMenuItem("Moderation", new ModerationControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/Moderation");
                await this.MainMenu.AddMenuItem("Services", new ServicesControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/Services");
            }
            await this.MainMenu.AddMenuItem("About", new AboutControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki");
        }

        private async Task StartShutdownProcess()
        {
            if (WindowState == WindowState.Maximized)
            {
                // Use the RestoreBounds as the current values will be 0, 0 and the size of the screen
                App.AppSettings.Top = RestoreBounds.Top;
                App.AppSettings.Left = RestoreBounds.Left;
                App.AppSettings.Height = RestoreBounds.Height;
                App.AppSettings.Width = RestoreBounds.Width;
                App.AppSettings.IsMaximized = true;
            }
            else
            {
                App.AppSettings.Top = this.Top;
                App.AppSettings.Left = this.Left;
                App.AppSettings.Height = this.Height;
                App.AppSettings.Width = this.Width;
                App.AppSettings.IsMaximized = false;
            }

            Properties.Settings.Default.Save();

            this.ShuttingDownGrid.Visibility = Visibility.Visible;
            this.MainMenu.Visibility = Visibility.Collapsed;

            if (!string.IsNullOrEmpty(this.RestoredSettingsFilePath))
            {
                string settingsFilePath = ChannelSession.Services.Settings.GetFilePath(ChannelSession.Settings);
                string settingsFolder = Path.GetDirectoryName(settingsFilePath);
                using (ZipArchive zipFile = ZipFile.Open(this.RestoredSettingsFilePath, ZipArchiveMode.Read))
                {
                    foreach (ZipArchiveEntry entry in zipFile.Entries)
                    {
                        string filePath = Path.Combine(settingsFolder, entry.Name);
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                    }
                    zipFile.ExtractToDirectory(settingsFolder);
                }
            }
            else
            {
                for (int i = 0; i < 5 && !await ChannelSession.Services.Settings.SaveAndValidate(ChannelSession.Settings); i++)
                {
                    await Task.Delay(1000);
                }
            }

            App.AppSettings.Save();

            await ChannelSession.Close();

            this.shutdownComplete = true;

            this.Close();
            if (this.restartApplication)
            {
                Process.Start(Application.ResourceAssembly.Location);
            }
        }

        private async void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Activate();
            if (!this.shutdownStarted)
            {
                e.Cancel = true;
                if (await MessageBoxHelper.ShowConfirmationDialog("Are you sure you wish to exit Mix It Up?"))
                {
                    this.shutdownStarted = true;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    this.StartShutdownProcess();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
            else if (!this.shutdownComplete)
            {
                e.Cancel = true;
            }
        }
    }
}
