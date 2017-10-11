using MixItUp.Base;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows;
using System.Reflection;
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
            this.Title += " - v" + Assembly.GetEntryAssembly().GetName().Version.ToString();

            ChannelSession.OnDisconectionOccurred += ChannelSession_OnDisconectionOccurred;

            await this.Chat.Initialize(this);
            await this.About.Initialize(this);
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
