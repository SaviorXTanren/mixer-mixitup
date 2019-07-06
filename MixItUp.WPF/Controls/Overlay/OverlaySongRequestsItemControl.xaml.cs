using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Controls.Overlay;
using MixItUp.WPF.Util;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlaySongRequestsItemControl.xaml
    /// </summary>
    public partial class OverlaySongRequestsItemControl : OverlayItemControl
    {
        private OverlaySongRequestsItemViewModel viewModel;

        public OverlaySongRequestsItemControl()
        {
            InitializeComponent();

            this.viewModel = new OverlaySongRequestsItemViewModel();
        }

        public OverlaySongRequestsItemControl(OverlaySongRequestsListItemModel item)
        {
            InitializeComponent();

            this.viewModel = new OverlaySongRequestsItemViewModel(item);
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
