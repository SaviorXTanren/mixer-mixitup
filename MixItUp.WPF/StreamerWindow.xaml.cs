using Mixer.Base.Model.Channel;
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
        private ChannelModel channel;

        public StreamerWindow(ChannelModel channel)
        {
            this.channel = channel;

            InitializeComponent();

            this.SetStatusBar(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            await MixerAPIHandler.InitializeChatClient(channel);
            await MixerAPIHandler.InitializeConstellationClient();
        }

        protected override async Task OnClosing()
        {
            await MixerAPIHandler.Close();
            Application.Current.Shutdown();
        }
    }
}
