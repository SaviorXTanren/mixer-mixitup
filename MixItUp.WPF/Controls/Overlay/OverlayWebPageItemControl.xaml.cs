using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Controls.Overlay;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayWebPageItemControl.xaml
    /// </summary>
    public partial class OverlayWebPageItemControl : OverlayItemControl
    {
        private OverlayWebPageItemViewModel viewModel;

        public OverlayWebPageItemControl()
        {
            InitializeComponent();

            this.viewModel = new OverlayWebPageItemViewModel();
        }

        public OverlayWebPageItemControl(OverlayWebPageItem item)
        {
            InitializeComponent();

            this.viewModel = new OverlayWebPageItemViewModel(item);
        }

        public override void SetItem(OverlayItemBase item)
        {
            if (item != null)
            {
                this.viewModel = new OverlayWebPageItemViewModel((OverlayWebPageItem)item);
            }
        }

        public override OverlayItemBase GetItem()
        {
            return this.viewModel.GetItem();
        }

        protected override async Task OnLoaded()
        {
            this.DataContext = this.viewModel;
            await this.viewModel.OnLoaded();
        }
    }
}
