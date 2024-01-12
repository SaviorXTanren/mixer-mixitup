using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.ViewModel.Overlay;
using MixItUp.WPF.Controls.Overlay;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Windows.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayWidgetV3EditorWindow.xaml
    /// </summary>
    public partial class OverlayWidgetV3EditorWindow : LoadingWindowBase
    {
        private OverlayWidgetV3ViewModel viewModel;

        public OverlayWidgetV3EditorWindow()
        {
            this.ViewModel = this.viewModel = new OverlayWidgetV3ViewModel(OverlayItemV3Type.StreamBoss);

            InitializeComponent();

            this.Initialize();
        }

        public OverlayWidgetV3EditorWindow(OverlayWidgetV3Model item)
        {
            this.ViewModel = this.viewModel = new OverlayWidgetV3ViewModel(item);

            InitializeComponent();

            this.Initialize();
        }

        protected override async Task OnLoaded()
        {
            this.DataContext = this.viewModel;

            this.AssignOverlayTypeControl(this.viewModel.Item.Type);

            await this.viewModel.OnOpen();
            await base.OnLoaded();
        }

        private void AssignOverlayTypeControl(OverlayItemV3Type type)
        {
            UserControl overlayControl = null;
            if (type == OverlayItemV3Type.Label)
            {
                overlayControl = new OverlayLabelV3Control();
            }
            else if (type == OverlayItemV3Type.StreamBoss)
            {
                overlayControl = new OverlayStreamBossV3Control();
            }

            if (overlayControl != null)
            {
                this.InnerContent.Content = overlayControl;
            }
        }

        private void Initialize()
        {
            this.Initialize(this.StatusBar);

            this.ViewModel.StartLoadingOperationOccurred += (sender, eventArgs) => { this.StartLoadingOperation(); };
            this.ViewModel.EndLoadingOperationOccurred += (sender, eventArgs) => { this.EndLoadingOperation(); };
            this.viewModel.OnCloseRequested += ViewModel_OnCloseRequested;
        }

        private void ViewModel_OnCloseRequested(object sender, System.EventArgs e)
        {
            this.Close();
        }
    }
}
