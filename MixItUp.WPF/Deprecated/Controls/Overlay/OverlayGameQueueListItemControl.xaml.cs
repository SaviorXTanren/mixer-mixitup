using MixItUp.Base.ViewModel.Overlay;
using MixItUp.WPF.Util;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayGameQueueListItemControl.xaml
    /// </summary>
    public partial class OverlayGameQueueListItemControl : OverlayItemControl
    {
        public OverlayGameQueueListItemControl()
        {
            InitializeComponent();
        }

        public OverlayGameQueueListItemControl(OverlayGameQueueListItemViewModel viewModel)
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
