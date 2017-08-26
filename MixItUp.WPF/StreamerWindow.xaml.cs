using MixItUp.Base;
using MixItUp.WPF.Windows;
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
            this.Chat.EnableCommands = true;

            await this.Chat.Initialize(this);
            await this.Commands.Initialize(this);
            await this.Timers.Initialize(this);
            await this.Interactive.Initialize(this);
            await this.Events.Initialize(this);
        }

        protected override async Task OnClosing()
        {
            await ChannelSession.Settings.SaveSettings();
            MixerAPIHandler.Close();
            Application.Current.Shutdown();
        }
    }
}
