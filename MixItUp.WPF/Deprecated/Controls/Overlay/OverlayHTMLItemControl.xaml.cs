using MixItUp.Base.ViewModel.Overlay;
using System;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayHTMLItemControl.xaml
    /// </summary>
    [Obsolete]
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
