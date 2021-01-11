using MixItUp.Base.ViewModel.Overlay;
using MixItUp.WPF.Util;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayTextItemControl.xaml
    /// </summary>
    public partial class OverlayTextItemControl : OverlayItemControl
    {
        public OverlayTextItemControl()
        {
            InitializeComponent();
        }

        public OverlayTextItemControl(OverlayTextItemViewModel viewModel)
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
