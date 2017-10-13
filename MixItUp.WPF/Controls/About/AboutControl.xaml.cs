using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace MixItUp.WPF.Controls.About
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

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private void MixerButton_Click(object sender, RoutedEventArgs e) { Process.Start("https://mixer.com/SaviorXTanren"); }

        private void TwitterButton_Click(object sender, RoutedEventArgs e) { Process.Start("https://twitter.com/MixItUpApp"); }

        private void GithubButton_Click(object sender, RoutedEventArgs e) { Process.Start("https://github.com/SaviorXTanren/mixer-mixitup"); }
    }
}
