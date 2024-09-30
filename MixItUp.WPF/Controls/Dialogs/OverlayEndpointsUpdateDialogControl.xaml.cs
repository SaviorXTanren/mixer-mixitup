using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Settings;
using MixItUp.WPF.Util;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for OverlayEndpointsUpdateDialogControl.xaml
    /// </summary>
    public partial class OverlayEndpointsUpdateDialogControl : UserControl
    {
        public ObservableCollection<OverlayEndpointListingViewModel> Endpoints { get; set; } = new ObservableCollection<OverlayEndpointListingViewModel>();

        public OverlayEndpointsUpdateDialogControl()
        {
            InitializeComponent();

            this.Loaded += OverlayEndpointsUpdateDialogControl_Loaded;
        }

        private void OverlayEndpointsUpdateDialogControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.EndpointsItemsControl.ItemsSource = this.Endpoints;
            this.Endpoints.AddRange(ServiceManager.Get<OverlayV3Service>().GetOverlayEndpoints().Select(oe => new OverlayEndpointListingViewModel(null, oe)));
        }

        private void LaunchEndpointURLButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            OverlayEndpointListingViewModel overlay = (OverlayEndpointListingViewModel)button.DataContext;
            ServiceManager.Get<IProcessService>().LaunchLink(overlay.Address);
        }

        private async void CopyEndpointURLButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            OverlayEndpointListingViewModel overlay = (OverlayEndpointListingViewModel)button.DataContext;
            await UIHelpers.CopyToClipboard(overlay.Address);
        }
    }
}
