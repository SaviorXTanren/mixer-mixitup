using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Controls.Overlay;
using MixItUp.WPF.Util;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayEventListControl.xaml
    /// </summary>
    public partial class OverlayEventListControl : OverlayItemControl
    {
        private OverlayEventListItemViewModel viewModel;

        public OverlayEventListControl()
        {
            InitializeComponent();

            this.viewModel = new OverlayEventListItemViewModel();
        }

        public OverlayEventListControl(OverlayEventListItemModel item)
        {
            InitializeComponent();

            this.viewModel = new OverlayEventListItemViewModel(item);
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
