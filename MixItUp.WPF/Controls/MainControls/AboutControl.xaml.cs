using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for AboutControl.xaml
    /// </summary>
    public partial class AboutControl : MainControlBase
    {
        public AboutControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.VersionTextBlock.Text = Assembly.GetEntryAssembly().GetName().Version.ToString();

            return base.InitializeInternal();
        }

        private void IssueReportHyperlink_Click(object sender, RoutedEventArgs e)
        {
            ServiceManager.Get<IProcessService>().LaunchProgram("MixItUp.Reporter.exe", $"{FileLoggerHandler.CurrentLogFilePath} {ChannelSession.Settings?.Name ?? "NONE"}");
        }

        private void TwitterButton_Click(object sender, RoutedEventArgs e) { ServiceManager.Get<IProcessService>().LaunchLink("https://twitter.com/MixItUpApp"); }

        private void DiscordButton_Click(object sender, RoutedEventArgs e) { ServiceManager.Get<IProcessService>().LaunchLink("https://mixitupapp.com/discord"); }

        private void YouTubeButton_Click(object sender, RoutedEventArgs e) { ServiceManager.Get<IProcessService>().LaunchLink("https://www.youtube.com/c/MixItUpApp"); }

        private void Patreon_Click(object sender, RoutedEventArgs e) { ServiceManager.Get<IProcessService>().LaunchLink("https://www.patreon.com/mixitupapp"); }

        private void WikiButton_Click(object sender, RoutedEventArgs e) { ServiceManager.Get<IProcessService>().LaunchLink("https://wiki.mixitupapp.com/"); }

        private void GithubButton_Click(object sender, RoutedEventArgs e) { ServiceManager.Get<IProcessService>().LaunchLink("https://github.com/SaviorXTanren/mixer-mixitup"); }

        private void SaviorXTanrenButton_Click(object sender, RoutedEventArgs e) { ServiceManager.Get<IProcessService>().LaunchLink("https://twitch.tv/SaviorXTanren"); }

        private void VerbatimTButton_Click(object sender, RoutedEventArgs e) { ServiceManager.Get<IProcessService>().LaunchLink("https://twitch.tv/Verbatim_T"); }

        private void TyrenDesButton_Click(object sender, RoutedEventArgs e) { ServiceManager.Get<IProcessService>().LaunchLink("https://twitch.tv/TyrenDes"); }
    }
}
