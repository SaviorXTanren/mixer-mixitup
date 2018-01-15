using MixItUp.Base;
using MixItUp.WPF.Controls.MainControls;
using MixItUp.WPF.Windows;
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

        private bool RestartApplication = false;

        private bool shutdownStarted = false;
        private bool shutdownComplete = false;

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += MainWindow_Closing;
            this.Initialize(this.StatusBar);
        }

        public void Restart()
        {
            this.RestartApplication = true;
            this.Close();
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
            this.Title += " - v" + Assembly.GetEntryAssembly().GetName().Version.ToString();

            await this.MainMenu.Initialize(this);

            await this.MainMenu.AddMenuItem("Chat", new ChatControl());
            if (ChannelSession.Settings.IsStreamer)
            {
                await this.MainMenu.AddMenuItem("Channel", new ChannelControl());
                await this.MainMenu.AddMenuItem("Commands", new ChatCommandsControl());
                await this.MainMenu.AddMenuItem("Interactive", new InteractiveControl());
                await this.MainMenu.AddMenuItem("Events", new EventsControl());
                await this.MainMenu.AddMenuItem("Timers", new TimerControl());
                await this.MainMenu.AddMenuItem("Action Groups", new ActionGroupControl());
                await this.MainMenu.AddMenuItem("Currency & Rank", new CurrencyAndRankControl());
                await this.MainMenu.AddMenuItem("Games", new GamesControl());
                await this.MainMenu.AddMenuItem("Giveaway", new GiveawayControl());
                await this.MainMenu.AddMenuItem("Game Queue", new GameQueueControl());
                await this.MainMenu.AddMenuItem("Quotes", new QuoteControl());
            }
            this.MainMenu.AddMenuItem("Statistics", "http://mixdash.cc");
            await this.MainMenu.AddMenuItem("Moderation", new ModerationControl());
            if (ChannelSession.Settings.IsStreamer)
            {
                await this.MainMenu.AddMenuItem("Services", new ServicesControl());
            }
            await this.MainMenu.AddMenuItem("About", new AboutControl());
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!this.shutdownStarted)
            {
                e.Cancel = true;
                this.shutdownStarted = true;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                this.StartShutdownProcess();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            else if (!this.shutdownComplete)
            {
                e.Cancel = true;
            }
        }

        private async Task StartShutdownProcess()
        {
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
                if (!await ChannelSession.Services.Settings.SaveAndValidate(ChannelSession.Settings))
                {
                    await Task.Delay(1000);
                    await ChannelSession.Services.Settings.SaveAndValidate(ChannelSession.Settings);
                }
            }

            await Task.Delay(2000);

            this.shutdownComplete = true;

            this.Close();
            if (this.RestartApplication)
            {
                Process.Start(Application.ResourceAssembly.Location);
            }
        }
    }
}
