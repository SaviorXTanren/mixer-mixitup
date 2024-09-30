using MixItUp.Base.ViewModel.Overlay;
using System;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayWebPageItemControl.xaml
    /// </summary>
    [Obsolete]
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
