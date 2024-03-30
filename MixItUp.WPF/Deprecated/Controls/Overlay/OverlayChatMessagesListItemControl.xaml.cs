using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.Overlay;
using System;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayChatMessagesListItemControl.xaml
    /// </summary>
    [Obsolete]
    public partial class OverlayChatMessagesListItemControl : OverlayItemControl
    {
        public OverlayChatMessagesListItemControl()
        {
            InitializeComponent();
        }

        public OverlayChatMessagesListItemControl(OverlayChatMessagesListItemViewModel viewModel)
            : this()
        {
            this.ViewModel = viewModel;
        }

        protected override async Task OnLoaded()
        {
            this.TextFontComboBox.ItemsSource = ServiceManager.Get<IFileService>().GetInstalledFonts();

            await base.OnLoaded();
        }
    }
}
