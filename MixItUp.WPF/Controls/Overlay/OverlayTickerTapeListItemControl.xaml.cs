using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Controls.Overlay;
using MixItUp.WPF.Util;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayTickerTapeListItemControl.xaml
    /// </summary>
    public partial class OverlayTickerTapeListItemControl : OverlayItemControl
    {
        private OverlayTickerTapeListItemViewModel viewModel;

        public OverlayTickerTapeListItemControl()
        {
            InitializeComponent();

            this.viewModel = new OverlayTickerTapeListItemViewModel();
        }

        public OverlayTickerTapeListItemControl(OverlayTickerTapeListItemModel item)
        {
            InitializeComponent();

            this.viewModel = new OverlayTickerTapeListItemViewModel(item);
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
