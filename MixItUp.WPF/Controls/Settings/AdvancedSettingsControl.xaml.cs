using MixItUp.Base.ViewModel.Settings;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Settings
{
    /// <summary>
    /// Interaction logic for AdvancedSettingsControl.xaml
    /// </summary>
    public partial class AdvancedSettingsControl : SettingsControlBase
    {
        private AdvancedSettingsControlViewModel viewModel;

        public AdvancedSettingsControl()
        {
            InitializeComponent();

            this.DataContext = this.viewModel = new AdvancedSettingsControlViewModel();
        }

        protected override async Task InitializeInternal()
        {
            await this.viewModel.OnOpen();
            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.InitializeInternal();
        }
    }
}
