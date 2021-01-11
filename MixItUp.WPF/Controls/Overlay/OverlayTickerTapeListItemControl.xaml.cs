using MixItUp.Base.ViewModel.Overlay;
using MixItUp.WPF.Util;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayTickerTapeListItemControl.xaml
    /// </summary>
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
            this.TextFontComboBox.ItemsSource = InstalledFonts.GetInstalledFonts();

            await base.OnLoaded();
        }
    }
}
