using MixItUp.Base;
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

            await this.Chat.Initialize(this);
            await this.Commands.Initialize(this);
            await this.Quotes.Initialize(this);
            await this.Timers.Initialize(this);
            await this.Interactive.Initialize(this);
            await this.Events.Initialize(this);
            await this.Giveaway.Initialize(this);
            await this.Services.Initialize(this);
            await this.About.Initialize(this);
        }

        protected override async Task OnClosing()
        {
            await ChannelSession.Settings.Save();
            ChannelSession.Close();
            Application.Current.Shutdown();
        }
    }
}
