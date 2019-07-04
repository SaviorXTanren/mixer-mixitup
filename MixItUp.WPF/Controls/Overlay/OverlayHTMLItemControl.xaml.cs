using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Controls.Overlay;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayHTMLItemControl.xaml
    /// </summary>
    public partial class OverlayHTMLItemControl : OverlayItemControl
    {
        private OverlayHTMLItemViewModel viewModel;

        public OverlayHTMLItemControl()
        {
            InitializeComponent();

            this.viewModel = new OverlayHTMLItemViewModel();
        }

        public OverlayHTMLItemControl(OverlayHTMLItemModel item)
        {
            InitializeComponent();

            this.viewModel = new OverlayHTMLItemViewModel(item);
        }

        public override void SetItem(OverlayItemModelBase item)
        {
            if (item != null)
            {
                this.viewModel = new OverlayHTMLItemViewModel((OverlayHTMLItemModel)item);
            }
        }

        public override OverlayItemModelBase GetItem()
        {
            return this.viewModel.GetOverlayItem();
        }

        protected override async Task OnLoaded()
        {
            this.DataContext = this.viewModel;
            await this.viewModel.OnLoaded();
        }
    }
}
