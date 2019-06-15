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

        public OverlayEventListControl(OverlayEventList item)
        {
            InitializeComponent();

            this.viewModel = new OverlayEventListItemViewModel(item);
        }

        public override void SetItem(OverlayItemBase item)
        {
            if (item != null)
            {
                this.viewModel = new OverlayEventListItemViewModel((OverlayEventList)item);
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
