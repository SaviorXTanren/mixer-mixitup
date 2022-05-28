using MixItUp.Base.ViewModel.Overlay;
using MixItUp.WPF.Util;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayEndCreditsItemControl.xaml
    /// </summary>
    public partial class OverlayEndCreditsItemControl : OverlayItemControl
    {
        public OverlayEndCreditsItemControl()
        {
            InitializeComponent();
        }

        public OverlayEndCreditsItemControl(OverlayEndCreditsItemViewModel viewModel)
            : this()
        {
            this.ViewModel = viewModel;
        }

        protected override async Task OnLoaded()
        {
            this.SectionTextFontComboBox.ItemsSource = InstalledFonts.GetInstalledFonts();
            this.ItemTextFontComboBox.ItemsSource = InstalledFonts.GetInstalledFonts();

            await base.OnLoaded();
        }
    }
}
