using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.Overlay;
using System;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayTextItemControl.xaml
    /// </summary>
    [Obsolete]
    public partial class OverlayTextItemControl : OverlayItemControl
    {
        public OverlayTextItemControl()
        {
            InitializeComponent();
        }

        public OverlayTextItemControl(OverlayTextItemViewModel viewModel)
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
