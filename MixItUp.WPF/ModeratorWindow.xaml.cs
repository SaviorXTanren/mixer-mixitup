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
        public ModeratorWindow()
        {
            InitializeComponent();
            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            await this.Chat.Initialize(this);
            await this.Events.Initialize(this);
        }

        protected override async Task OnClosing()
        {
            await ChannelSession.Settings.SaveSettings();
            ChannelSession.Close();
            Application.Current.Shutdown();
        }
    }
}
