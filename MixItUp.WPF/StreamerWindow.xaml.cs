using MixItUp.Base;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF
{
    /// <summary>
    /// Interaction logic for StreamerWindow.xaml
    /// </summary>
    public partial class StreamerWindow : LoadingWindowBase
    {
        public StreamerWindow()
        {
            InitializeComponent();
            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            this.Title += " - v" + Assembly.GetEntryAssembly().GetName().Version.ToString();

            this.Chat.EnableCommands = true;

            ChannelSession.OnDisconectionOccurred += ChannelSession_OnDisconectionOccurred;

            await this.Chat.Initialize(this);
            await this.Commands.Initialize(this);
            await this.Quotes.Initialize(this);
            await this.Timers.Initialize(this);
            await this.Interactive.Initialize(this);
            await this.Events.Initialize(this);
            await this.Giveaway.Initialize(this);
            await this.Services.Initialize(this);
            await this.About.Initialize(this);
            await this.Currency.Initialize(this);
            await this.Moderation.Initialize(this);
        }

        protected override async Task OnClosing()
        {
            await ChannelSession.Settings.Save();
            await ChannelSession.Close();
            Application.Current.Shutdown();
        }

        private async void ChannelSession_OnDisconectionOccurred(object sender, System.EventArgs e)
        {
            await MessageBoxHelper.ShowMessageDialog("Disconnection occurred, attempting to reconnect...");
        }
    }
}
