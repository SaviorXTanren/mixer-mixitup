using MixItUp.Base.ViewModel.Overlay;
using MixItUp.WPF.Util;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayEventListItemControl.xaml
    /// </summary>
    public partial class OverlayEventListItemControl : OverlayItemControl
    {
        public OverlayEventListItemControl()
        {
            InitializeComponent();
        }

        public OverlayEventListItemControl(OverlayEventListItemViewModel viewModel)
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
