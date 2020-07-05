using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Controls.Overlay;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayClipPlaybackItemControl.xaml
    /// </summary>
    public partial class OverlayClipPlaybackItemControl : OverlayItemControl
    {
        private OverlayClipPlaybackItemViewModel viewModel;

        public OverlayClipPlaybackItemControl()
        {
            InitializeComponent();

            this.viewModel = new OverlayClipPlaybackItemViewModel();
        }

        public OverlayClipPlaybackItemControl(OverlayClipPlaybackItemModel item)
        {
            InitializeComponent();

            this.viewModel = new OverlayClipPlaybackItemViewModel(item);
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