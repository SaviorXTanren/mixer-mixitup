using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Controls.Overlay;
using MixItUp.WPF.Util;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayGameQueueItemControl.xaml
    /// </summary>
    public partial class OverlayGameQueueItemControl : OverlayItemControl
    {
        private OverlayGameQueueItemViewModel viewModel;

        public OverlayGameQueueItemControl()
        {
            InitializeComponent();

            this.viewModel = new OverlayGameQueueItemViewModel();
        }

        public OverlayGameQueueItemControl(OverlayGameQueueListItemModel item)
        {
            InitializeComponent();

            this.viewModel = new OverlayGameQueueItemViewModel(item);
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
