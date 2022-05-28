using MixItUp.Base.ViewModel.Overlay;
using MixItUp.WPF.Util;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayTimerTrainItemControl.xaml
    /// </summary>
    public partial class OverlayTimerTrainItemControl : OverlayItemControl
    {
        public OverlayTimerTrainItemControl()
        {
            InitializeComponent();
        }

        public OverlayTimerTrainItemControl(OverlayTimerTrainItemViewModel viewModel)
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
