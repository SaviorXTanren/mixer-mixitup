using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Controls.Overlay;
using MixItUp.WPF.Util;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayTimerTrainControl.xaml
    /// </summary>
    public partial class OverlayTimerTrainControl : OverlayItemControl
    {
        private OverlayTimerTrainItemViewModel viewModel;

        public OverlayTimerTrainControl()
        {
            InitializeComponent();

            this.viewModel = new OverlayTimerTrainItemViewModel();
        }

        public OverlayTimerTrainControl(OverlayTimerTrainItemModel item)
        {
            InitializeComponent();

            this.viewModel = new OverlayTimerTrainItemViewModel(item);
        }

        public override OverlayItemViewModelBase GetViewModel() { return this.viewModel; }

        public override void SetItem(OverlayItemModelBase item)
        {
            if (item != null)
            {
                this.viewModel = new OverlayTimerTrainItemViewModel((OverlayTimerTrainItemModel)item);
            }
        }

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
