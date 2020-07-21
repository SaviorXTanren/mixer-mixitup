using MixItUp.Base.ViewModel.Controls.Settings;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Settings
{
    /// <summary>
    /// Interaction logic for OverlaySettingsControl.xaml
    /// </summary>
    public partial class OverlaySettingsControl : SettingsControlBase
    {
        private OverlaySettingsControlViewModel viewModel;

        public OverlaySettingsControl()
        {
            InitializeComponent();

            this.DataContext = this.viewModel = new OverlaySettingsControlViewModel();
        }

        protected override async Task InitializeInternal()
        {
            await this.viewModel.OnLoaded();
            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.InitializeInternal();
        }

        private void CopyEndpointURLButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            OverlayEndpointListingViewModel overlay = (OverlayEndpointListingViewModel)button.DataContext;
            Clipboard.SetText(overlay.Address);
        }
    }
}