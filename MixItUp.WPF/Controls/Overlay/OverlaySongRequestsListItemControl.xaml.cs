using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Controls.Overlay;
using MixItUp.WPF.Util;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlaySongRequestsListItemControl.xaml
    /// </summary>
    public partial class OverlaySongRequestsListItemControl : OverlayItemControl
    {
        private OverlaySongRequestsListItemViewModel viewModel;

        public OverlaySongRequestsListItemControl()
        {
            InitializeComponent();

            this.viewModel = new OverlaySongRequestsListItemViewModel();
        }

        public OverlaySongRequestsListItemControl(OverlaySongRequestsListItemModel item)
        {
            InitializeComponent();

            this.viewModel = new OverlaySongRequestsListItemViewModel(item);
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
