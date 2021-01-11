using MixItUp.Base.ViewModel.Overlay;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayYouTubeItemControl.xaml
    /// </summary>
    public partial class OverlayYouTubeItemControl : OverlayItemControl
    {
        public OverlayYouTubeItemControl()
        {
            InitializeComponent();
        }

        public OverlayYouTubeItemControl(OverlayYouTubeItemViewModel viewModel)
        {
            this.ViewModel = viewModel;
        }
    }
}
