using MixItUp.Base.ViewModel.Overlay;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayHTMLItemControl.xaml
    /// </summary>
    public partial class OverlayHTMLItemControl : OverlayItemControl
    {
        public OverlayHTMLItemControl()
        {
            InitializeComponent();
        }

        public OverlayHTMLItemControl(OverlayHTMLItemViewModel viewModel)
            : this()
        {
            this.ViewModel = viewModel;
        }
    }
}
