using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Overlay;
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

        public OverlayWebPageItemControl(OverlayWebPageItemModel item)
        {
            InitializeComponent();

            this.viewModel = new OverlayWebPageItemViewModel(item);
        }

        public override OverlayItemViewModelBase GetViewModel() { return this.viewModel; }

        public override OverlayItemModelBase GetItem()
        {
            return this.viewModel.GetOverlayItem();
        }

        protected override async Task OnLoaded()
        {
            if (this.DataContext is OverlayWebPageItemViewModel)
            {
                this.viewModel = (OverlayWebPageItemViewModel)this.DataContext;
            }
            else
            {
                this.DataContext = this.viewModel;
            }
            await this.viewModel.OnLoaded();
        }
    }
}
