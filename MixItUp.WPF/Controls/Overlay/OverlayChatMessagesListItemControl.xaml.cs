using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Controls.Overlay;
using MixItUp.WPF.Util;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayChatMessagesListItemControl.xaml
    /// </summary>
    public partial class OverlayChatMessagesListItemControl : OverlayItemControl
    {
        private OverlayChatMessagesListItemViewModel viewModel;

        public OverlayChatMessagesListItemControl()
        {
            InitializeComponent();

            this.viewModel = new OverlayChatMessagesListItemViewModel();
        }

        public OverlayChatMessagesListItemControl(OverlayChatMessagesListItemModel item)
        {
            InitializeComponent();

            this.viewModel = new OverlayChatMessagesListItemViewModel(item);
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
