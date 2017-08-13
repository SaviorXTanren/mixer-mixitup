using Mixer.Base.Model.Channel;
using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.WPF.Windows;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF
{
    /// <summary>
    /// Interaction logic for ModeratorWindow.xaml
    /// </summary>
    public partial class ModeratorWindow : LoadingWindowBase
    {
        public ModeratorWindow(PrivatePopulatedUserModel user, ExpandedChannelModel channel)
        {
            InitializeComponent();

            this.Initialize(user, channel, this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            await MixerAPIHandler.LoadSettings(this.Channel);

            await this.Chat.Initialize(this);
            await this.Events.Initialize(this);
        }

        protected override async Task OnClosing()
        {
            await MixerAPIHandler.Close();
            Application.Current.Shutdown();
        }
    }
}
