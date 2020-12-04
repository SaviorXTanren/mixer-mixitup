using MixItUp.Base.ViewModel.Settings;
using StreamingClient.Base.Util;
using System;
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
            try
            {
                Clipboard.SetText(overlay.Address);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }
}