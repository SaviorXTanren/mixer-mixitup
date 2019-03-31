using System.Threading.Tasks;
using MixItUp.Base.ViewModel.Controls.Settings;

namespace MixItUp.WPF.Controls.Settings
{
    /// <summary>
    /// Interaction logic for RemoteSettingsControl.xaml
    /// </summary>
    public partial class RemoteSettingsControl : SettingsControlBase
    {
        private RemoteSettingsControlViewModel viewModel;

        public RemoteSettingsControl()
        {
            InitializeComponent();

            this.DataContext = this.viewModel = new RemoteSettingsControlViewModel();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.viewModel.OnVisible();
            await base.OnVisibilityChanged();
        }
    }
}
