using MixItUp.Base;
using MixItUp.WPF.Controls.About;
using MixItUp.WPF.Controls.Chat;
using MixItUp.WPF.Controls.Currency;
using MixItUp.WPF.Controls.Events;
using MixItUp.WPF.Controls.Giveaway;
using MixItUp.WPF.Controls.Interactive;
using MixItUp.WPF.Controls.Quotes;
using MixItUp.WPF.Controls.Services;
using MixItUp.WPF.Controls.Timers;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows;
using System;
using System.IO;
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
        public string restoredSettingsFilePath = null;

        public MainWindow()
        {
            InitializeComponent();
            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            ChannelSession.OnDisconectionOccurred += ChannelSession_OnDisconectionOccurred;
            ChannelSession.OnReconectionOccurred += ChannelSession_OnReconectionOccurred;

            this.Title += " - v" + Assembly.GetEntryAssembly().GetName().Version.ToString();

            await this.MainMenu.Initialize(this);

            await this.MainMenu.AddMenuItem("Chat", new ChatControl() { EnableCommands = true });
            if (ChannelSession.Settings.IsStreamer)
            {
                await this.MainMenu.AddMenuItem("Commands", new ChatCommandsControl());
                await this.MainMenu.AddMenuItem("Interactive", new InteractiveControl());
                await this.MainMenu.AddMenuItem("Events", new EventsControl());
                await this.MainMenu.AddMenuItem("Timers", new TimerControl());
                await this.MainMenu.AddMenuItem("Currency", new CurrencyControl());
                await this.MainMenu.AddMenuItem("Quotes", new QuoteControl());
                await this.MainMenu.AddMenuItem("Giveaway", new GiveawayControl());
                await this.MainMenu.AddMenuItem("Services", new ServicesControl());
            }
            await this.MainMenu.AddMenuItem("Moderation", new ChatModerationControl());
            await this.MainMenu.AddMenuItem("About", new AboutControl());
        }

        protected override async Task OnClosing()
        {
            if (!string.IsNullOrEmpty(this.restoredSettingsFilePath))
            {
                File.Copy(this.restoredSettingsFilePath, ChannelSession.Settings.GetSettingsFileName(), overwrite: true);
            }
            else
            {
                await ChannelSession.Settings.Save();
            }
            Application.Current.Shutdown();
        }

        private async void ChannelSession_OnDisconectionOccurred(object sender, System.EventArgs e)
        {
            this.IsEnabled = false;
            await this.Dispatcher.Invoke<Task>(async () => { await MessageBoxHelper.ShowMessageDialog("Disconnection occurred, attempting to reconnect..."); });
        }

        private async void ChannelSession_OnReconectionOccurred(object sender, EventArgs e)
        {
            this.IsEnabled = true;
            await this.Dispatcher.Invoke<Task>(async () => { await MessageBoxHelper.ShowMessageDialog("Successfully reconnected to Mixer services"); });
        }
    }
}
