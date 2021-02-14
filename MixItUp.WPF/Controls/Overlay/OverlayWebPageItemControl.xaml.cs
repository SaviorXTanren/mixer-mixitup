using MixItUp.Base.ViewModel.Overlay;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayWebPageItemControl.xaml
    /// </summary>
    public partial class OverlayWebPageItemControl : OverlayItemControl
    {
        public OverlayWebPageItemControl()
        {
            InitializeComponent();
        }

        public OverlayWebPageItemControl(OverlayWebPageItemViewModel viewModel)
            : this()
        {
            this.ViewModel = viewModel;
        }
    }
}
