using MixItUp.WPF.Controls.MainControls;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Settings
{
    /// <summary>
    /// Interaction logic for GeneralSettingsControl.xaml
    /// </summary>
    public partial class GeneralSettingsControl : MainControlBase
    {
        public GeneralSettingsControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.InitializeInternal();
        }
    }
}
