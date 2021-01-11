using MixItUp.Base.ViewModel.Overlay;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayImageItemControl.xaml
    /// </summary>
    public partial class OverlayImageItemControl : OverlayItemControl
    {
        public OverlayImageItemControl()
        {
            InitializeComponent();
        }

        public OverlayImageItemControl(OverlayImageItemViewModel viewModel)
            : this()
        {
            this.ViewModel = viewModel;
        }
    }
}
