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

        public override void SetOverlayItem(OverlayItemModelBase item)
        {
            if (item != null)
            {
                this.viewModel = new OverlayTimerTrainItemViewModel((OverlayTimerTrainItemModel)item);
            }
        }

        public override OverlayItemModelBase GetOverlayItem()
        {
            return this.viewModel.GetOverlayItem();
        }

        public override void SetItem(OverlayItemBase item)
        {
            if (item != null)
            {
                this.viewModel = new OverlayTimerTrainItemViewModel((OverlayTimerTrain)item);
            }
        }

        public override OverlayItemBase GetItem()
        {
            return this.viewModel.GetItem();
        }

        protected override async Task OnLoaded()
        {
            this.TextFontComboBox.ItemsSource = InstalledFonts.GetInstalledFonts();

            this.DataContext = this.viewModel;
            await this.viewModel.OnLoaded();
        }
    }
}
