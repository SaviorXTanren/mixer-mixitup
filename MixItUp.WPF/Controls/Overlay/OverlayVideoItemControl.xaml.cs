using MixItUp.Base.ViewModel.Overlay;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayVideoItemControl.xaml
    /// </summary>
    public partial class OverlayVideoItemControl : OverlayItemControl
    {
        public OverlayVideoItemControl()
        {
            InitializeComponent();
        }

        public OverlayVideoItemControl(OverlayVideoItemViewModel viewModel)
            : this()
        {
            this.ViewModel = viewModel;
        }
    }
}
