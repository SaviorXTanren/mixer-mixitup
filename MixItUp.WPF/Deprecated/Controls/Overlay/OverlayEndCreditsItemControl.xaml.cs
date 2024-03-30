using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.Overlay;
using System;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayEndCreditsItemControl.xaml
    /// </summary>
    [Obsolete]
    public partial class OverlayEndCreditsItemControl : OverlayItemControl
    {
        public OverlayEndCreditsItemControl()
        {
            InitializeComponent();
        }

        public OverlayEndCreditsItemControl(OverlayEndCreditsItemViewModel viewModel)
            : this()
        {
            this.ViewModel = viewModel;
        }

        protected override async Task OnLoaded()
        {
            this.SectionTextFontComboBox.ItemsSource = ServiceManager.Get<IFileService>().GetInstalledFonts();
            this.ItemTextFontComboBox.ItemsSource = ServiceManager.Get<IFileService>().GetInstalledFonts();

            await base.OnLoaded();
        }
    }
}
