using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.Overlay;
using System;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayTickerTapeListItemControl.xaml
    /// </summary>
    [Obsolete]
    public partial class OverlayTickerTapeListItemControl : OverlayItemControl
    {
        public OverlayTickerTapeListItemControl()
        {
            InitializeComponent();
        }

        public OverlayTickerTapeListItemControl(OverlayTickerTapeListItemViewModel viewModel)
            : this()
        {
            this.ViewModel = viewModel;
        }

        protected override async Task OnLoaded()
        {
            this.TextFontComboBox.ItemsSource = ServiceManager.Get<IFileService>().GetInstalledFonts();

            await base.OnLoaded();
        }
    }
}
