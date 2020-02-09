using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Window;
using MixItUp.WPF.Controls.MainControls;
using MixItUp.WPF.Windows;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

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

        private MainWindowViewModel viewModel;

        public MainWindow()
            : base(new MainWindowViewModel())
        {
            InitializeComponent();

            this.Closing += MainWindow_Closing;
            this.Initialize(this.StatusBar);

            this.viewModel = (MainWindowViewModel)this.ViewModel;
            this.viewModel.StartLoadingOperationOccurred += (sender, args) =>
            {
                this.StartAsyncOperation();
            };
            this.viewModel.EndLoadingOperationOccurred += (sender, args) =>
            {
                this.EndAsyncOperation();
            };

            if (ChannelSession.AppSettings.Width > 0)
            {
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.Height = ChannelSession.AppSettings.Height;
                this.Width = ChannelSession.AppSettings.Width;
                this.Top = ChannelSession.AppSettings.Top;
                this.Left = ChannelSession.AppSettings.Left;

                var rect = new System.Drawing.Rectangle((int)this.Left, (int)this.Top, (int)this.Width, (int)this.Height);
                var screen = System.Windows.Forms.Screen.FromRectangle(rect);
                if (!screen.Bounds.Contains(rect))
                {
                    // Off the bottom of the screen?
                    if (this.Top + this.Height > screen.Bounds.Top + screen.Bounds.Height)
                    {
                        this.Top = screen.Bounds.Top + screen.Bounds.Height - this.Height;
                    }

                    // Off the right side of the screen?
                    if (this.Left + this.Width > screen.Bounds.Left + screen.Bounds.Width)
                    {
                        this.Left = screen.Bounds.Left + screen.Bounds.Width - this.Width;
                    }

                    // Off the top of the screen?
                    if (this.Top < screen.Bounds.Top)
                    {
                        this.Top = screen.Bounds.Top;
                    }

                    // Off the left side of the screen?
                    if (this.Left < screen.Bounds.Left)
                    {
                        this.Left = screen.Bounds.Left;
                    }
                }

                if (ChannelSession.AppSettings.IsMaximized)
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
            ChannelSession.Services.InputService.Initialize(new WindowInteropHelper(this).Handle);
            foreach (HotKeyConfiguration hotKeyConfiguration in ChannelSession.Settings.HotKeys.Values)
            {
                ChannelSession.Services.InputService.RegisterHotKey(hotKeyConfiguration.Modifiers, hotKeyConfiguration.Key);
            }

            if (ChannelSession.Settings.IsStreamer)
            {
                this.Title += " - Streamer";
            }
            else
            {
                this.Title += " - Moderator";
            }

            if (!string.IsNullOrEmpty(ChannelSession.MixerChannel?.token))
            {
                this.Title += " - " + ChannelSession.MixerChannel.token;
            }

            this.Title += " - v" + Assembly.GetEntryAssembly().GetName().Version.ToString();

            await this.MainMenu.Initialize(this);

            if (ChannelSession.Settings.IsStreamer)
            {
                await this.MainMenu.AddMenuItem("Accounts", new AccountsControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki");
            }
            await this.MainMenu.AddMenuItem("Chat", new ChatControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/Chat");
            if (ChannelSession.Settings.IsStreamer)
            {
                await this.MainMenu.AddMenuItem("Channel", new ChannelControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/Channel");
                await this.MainMenu.AddMenuItem("Commands", new ChatCommandsControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/Commands");
                await this.MainMenu.AddMenuItem("Events", new EventsControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/Events");
                await this.MainMenu.AddMenuItem("MixPlay", new MixPlayControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/MixPlay");
                await this.MainMenu.AddMenuItem("Timers", new TimerControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/Timers");
                await this.MainMenu.AddMenuItem("Action Groups", new ActionGroupControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/Action-Groups");
                await this.MainMenu.AddMenuItem("Remote", new RemoteControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/Remote");
                await this.MainMenu.AddMenuItem("Users", new UsersControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/Users");
                await this.MainMenu.AddMenuItem("Currency/Rank/Inventory", new CurrencyRankInventoryControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/Currency,-Rank,-&-Inventory");
                await this.MainMenu.AddMenuItem("Overlay Widgets", new OverlayWidgetsControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/Overlay-Widgets");
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
                await this.MainMenu.AddMenuItem("Auto-Hoster", new AutoHosterControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/Auto-Hoster");
                await this.MainMenu.AddMenuItem("Services", new ServicesControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki/Services");
            }
            await this.MainMenu.AddMenuItem("About", new AboutControl(), "https://github.com/SaviorXTanren/mixer-mixitup/wiki");

            this.MainMenu.MenuItemSelected("Chat");
        }

        private async Task StartShutdownProcess()
        {
            if (WindowState == WindowState.Maximized)
            {
                // Use the RestoreBounds as the current values will be 0, 0 and the size of the screen
                ChannelSession.AppSettings.Top = RestoreBounds.Top;
                ChannelSession.AppSettings.Left = RestoreBounds.Left;
                ChannelSession.AppSettings.Height = RestoreBounds.Height;
                ChannelSession.AppSettings.Width = RestoreBounds.Width;
                ChannelSession.AppSettings.IsMaximized = true;
            }
            else
            {
                ChannelSession.AppSettings.Top = this.Top;
                ChannelSession.AppSettings.Left = this.Left;
                ChannelSession.AppSettings.Height = this.Height;
                ChannelSession.AppSettings.Width = this.Width;
                ChannelSession.AppSettings.IsMaximized = false;
            }

            Properties.Settings.Default.Save();

            this.ShuttingDownGrid.Visibility = Visibility.Visible;
            this.MainMenu.Visibility = Visibility.Collapsed;

            if (!string.IsNullOrEmpty(this.RestoredSettingsFilePath))
            {
                string settingsFilePath = ChannelSession.Settings.SettingsFilePath;
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

            await ChannelSession.AppSettings.Save();

            await ChannelSession.Close();

            this.shutdownComplete = true;

            this.Close();
            if (this.restartApplication)
            {
                ProcessHelper.LaunchFolder(Application.ResourceAssembly.Location);
            }
        }

        private async void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Activate();
            if (!this.shutdownStarted)
            {
                e.Cancel = true;
                if (await DialogHelper.ShowConfirmation("Are you sure you wish to exit Mix It Up?"))
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
