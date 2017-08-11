using Mixer.Base.Model.Channel;
using Mixer.Base.Model.User;
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
        public StreamerWindow(PrivatePopulatedUserModel user, ExpandedChannelModel channel)
        {
            InitializeComponent();

            this.Initialize(user, channel, this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            await this.Chat.Initialize(this);
            await this.Interactive.Initialize(this);
            await this.Events.Initialize(this);
            await this.Commands.Initialize(this);
        }

        protected override async Task OnClosing()
        {
            await MixerAPIHandler.Close();
            Application.Current.Shutdown();
        }
    }
}
