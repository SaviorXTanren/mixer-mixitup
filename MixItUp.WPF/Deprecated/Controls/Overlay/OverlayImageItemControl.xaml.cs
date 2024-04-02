using MixItUp.Base.ViewModel.Overlay;
using System;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayImageItemControl.xaml
    /// </summary>
    [Obsolete]
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
