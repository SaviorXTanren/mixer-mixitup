using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModel.Controls.Overlay;
using MixItUp.WPF.Util;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayChatMessagesControl.xaml
    /// </summary>
    public partial class OverlayChatMessagesControl : OverlayItemControl
    {
        private OverlayChatMessagesItemViewModel viewModel;

        public OverlayChatMessagesControl()
        {
            InitializeComponent();

            this.viewModel = new OverlayChatMessagesItemViewModel();
        }

        public OverlayChatMessagesControl(OverlayItemModelBase item)
        {
            InitializeComponent();

            this.viewModel = new OverlayChatMessagesItemViewModel((OverlayChatMessagesListItemModel)item);
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
