using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Controls.Overlay;
using MixItUp.WPF.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayTextItemControl.xaml
    /// </summary>
    public partial class OverlayTextItemControl : OverlayItemControl
    {
        private OverlayTextItemViewModel viewModel;

        public OverlayTextItemControl()
        {
            InitializeComponent();

            this.viewModel = new OverlayTextItemViewModel();
        }

        public OverlayTextItemControl(OverlayTextItemModel item)
        {
            InitializeComponent();

            this.viewModel = new OverlayTextItemViewModel(item);
        }

        public override void SetItem(OverlayItemModelBase item)
        {
            if (item != null)
            {
                this.viewModel = new OverlayTextItemViewModel((OverlayTextItemModel)item);
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
