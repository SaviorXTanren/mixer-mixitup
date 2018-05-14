using System.Diagnostics;
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

        private void TwitterButton_Click(object sender, RoutedEventArgs e) { Process.Start("https://twitter.com/MixItUpApp"); }

        private void DiscordButton_Click(object sender, RoutedEventArgs e) { Process.Start("https://discord.gg/taj4Gj4"); }

        private void YouTubeButton_Click(object sender, RoutedEventArgs e) { Process.Start("https://www.youtube.com/channel/UCcY0vKI9yqcMTgh8OzSnRSA"); }

        private void MixerButton_Click(object sender, RoutedEventArgs e) { Process.Start("https://mixer.com/team/MixItUp"); }

        private void WikiButton_Click(object sender, RoutedEventArgs e) { Process.Start("https://github.com/SaviorXTanren/mixer-mixitup/wiki"); }

        private void GithubButton_Click(object sender, RoutedEventArgs e) { Process.Start("https://github.com/SaviorXTanren/mixer-mixitup"); }

        private void SaviorXTanrenMixerButton_Click(object sender, RoutedEventArgs e) { Process.Start("https://mixer.com/SaviorXTanren"); }

        private void VerbatimTMixerButton_Click(object sender, RoutedEventArgs e) { Process.Start("https://mixer.com/VerbatimT"); }

        private void TyrenDesMixerButton_Click(object sender, RoutedEventArgs e) { Process.Start("https://mixer.com/TyrenDes"); }
    }
}
