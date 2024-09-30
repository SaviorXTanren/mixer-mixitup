using MixItUp.Base.ViewModel.Overlay;
using System;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayVideoItemControl.xaml
    /// </summary>
    [Obsolete]
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
