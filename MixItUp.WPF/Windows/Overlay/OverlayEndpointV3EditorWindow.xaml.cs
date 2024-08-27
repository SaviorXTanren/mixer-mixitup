using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Overlay;
using System.Threading.Tasks;

namespace MixItUp.WPF.Windows.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayEndpointV3EditorWindow.xaml
    /// </summary>
    public partial class OverlayEndpointV3EditorWindow : LoadingWindowBase
    {
        private OverlayEndpointV3EditorViewModel viewModel;

        public OverlayEndpointV3EditorWindow(OverlayEndpointV3Model endpoint)
        {
            this.ViewModel = this.viewModel = new OverlayEndpointV3EditorViewModel(endpoint);

            this.viewModel.OnCloseRequested += ViewModel_OnCloseRequested;

            InitializeComponent();

            this.Initialize(this.StatusBar);

            this.ViewModel.StartLoadingOperationOccurred += (sender, eventArgs) => { this.StartLoadingOperation(); };
            this.ViewModel.EndLoadingOperationOccurred += (sender, eventArgs) => { this.EndLoadingOperation(); };
        }

        protected override async Task OnLoaded()
        {
            this.DataContext = this.viewModel;

            await this.viewModel.OnOpen();
            await base.OnLoaded();
        }

        protected override async Task OnClosing()
        {
            await this.ViewModel.OnClosed();
            await base.OnClosing();
        }

        private void ViewModel_OnCloseRequested(object sender, System.EventArgs e)
        {
            this.Close();
        }
    }
}
