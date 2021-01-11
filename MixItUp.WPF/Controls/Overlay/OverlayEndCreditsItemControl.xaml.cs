using MixItUp.Base.Model.Overlay;
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
        private OverlayEndCreditsItemViewModel viewModel;

        public OverlayEndCreditsItemControl()
        {
            InitializeComponent();

            this.viewModel = new OverlayEndCreditsItemViewModel();
        }

        public OverlayEndCreditsItemControl(OverlayEndCreditsItemModel item)
        {
            InitializeComponent();

            this.viewModel = new OverlayEndCreditsItemViewModel(item);
        }

        public override OverlayItemViewModelBase GetViewModel() { return this.viewModel; }

        public override OverlayItemModelBase GetItem()
        {
            return this.viewModel.GetOverlayItem();
        }

        protected override async Task OnLoaded()
        {
            this.SectionTextFontComboBox.ItemsSource = InstalledFonts.GetInstalledFonts();
            this.ItemTextFontComboBox.ItemsSource = InstalledFonts.GetInstalledFonts();

            this.DataContext = this.viewModel;
            await this.viewModel.OnLoaded();
        }
    }
}
