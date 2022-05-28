using MixItUp.WPF.Util;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayTextItemV3Control.xaml
    /// </summary>
    public partial class OverlayTextItemV3Control : LoadingControlBase
    {
        public OverlayTextItemV3Control()
        {
            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            this.FontNamesComboBox.ItemsSource = InstalledFonts.GetInstalledFonts();

            await base.OnLoaded();
        }
    }
}
