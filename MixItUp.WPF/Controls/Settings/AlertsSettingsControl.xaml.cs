using MixItUp.Base.ViewModel.Controls.Settings;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Settings
{
    /// <summary>
    /// Interaction logic for AlertsSettingsControl.xaml
    /// </summary>
    public partial class AlertsSettingsControl : SettingsControlBase
    {
        private AlertsSettingsControlViewModel viewModel;

        public AlertsSettingsControl()
        {
            InitializeComponent();

            this.DataContext = this.viewModel = new AlertsSettingsControlViewModel();
        }

        protected override async Task InitializeInternal()
        {
            await this.viewModel.OnLoaded();
            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.InitializeInternal();
        }
    }
}
