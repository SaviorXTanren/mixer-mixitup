using MixItUp.Base.ViewModel.Overlay;
using System;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayYouTubeItemControl.xaml
    /// </summary>
    [Obsolete]
    public partial class OverlayYouTubeItemControl : OverlayItemControl
    {
        public OverlayYouTubeItemControl()
        {
            InitializeComponent();
        }

        public OverlayYouTubeItemControl(OverlayYouTubeItemViewModel viewModel)
             : this()
        {
            this.ViewModel = viewModel;
        }
    }
}
