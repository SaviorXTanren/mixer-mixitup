using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Controls.Overlay;
using MixItUp.WPF.Util;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlaySparkCrystalItemControl.xaml
    /// </summary>
    public partial class OverlaySparkCrystalItemControl : OverlayItemControl
    {
        private OverlaySparkCrystalItemViewModel viewModel;

        public OverlaySparkCrystalItemControl()
        {
            InitializeComponent();

            this.viewModel = new OverlaySparkCrystalItemViewModel();
        }

        public OverlaySparkCrystalItemControl(OverlaySparkCrystalItemModel item)
        {
            InitializeComponent();

            this.viewModel = new OverlaySparkCrystalItemViewModel(item);
        }

        public override OverlayItemViewModelBase GetViewModel() { return this.viewModel; }

        public override OverlayItemModelBase GetItem()
        {
            return this.viewModel.GetOverlayItem();
        }

        protected override async Task OnLoaded()
        {
            this.TextFontComboBox.ItemsSource = InstalledFonts.GetInstalledFonts();

            this.DataContext = this.viewModel;
            await this.viewModel.OnLoaded();
        }
    }
}
