using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Controls.Overlay;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayStreamClipItemControl.xaml
    /// </summary>
    public partial class OverlayStreamClipItemControl : OverlayItemControl
    {
        private OverlayStreamClipItemViewModel viewModel;

        public OverlayStreamClipItemControl()
        {
            InitializeComponent();

            this.viewModel = new OverlayStreamClipItemViewModel();
        }

        public OverlayStreamClipItemControl(OverlayStreamClipItemModel item)
        {
            InitializeComponent();

            this.viewModel = new OverlayStreamClipItemViewModel(item);
        }

        public override OverlayItemViewModelBase GetViewModel() { return this.viewModel; }

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
