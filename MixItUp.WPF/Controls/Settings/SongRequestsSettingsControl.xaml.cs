using MixItUp.Base.ViewModel.Controls.Settings;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Settings
{
    /// <summary>
    /// Interaction logic for SongRequestsSettingsControl.xaml
    /// </summary>
    public partial class SongRequestsSettingsControl : SettingsControlBase
    {
        private SongRequestsSettingsControlViewModel viewModel;

        public SongRequestsSettingsControl()
        {
            InitializeComponent();
            this.DataContext = this.viewModel = new SongRequestsSettingsControlViewModel();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.viewModel.OnVisible();
            await base.OnVisibilityChanged();
        }
    }
}
