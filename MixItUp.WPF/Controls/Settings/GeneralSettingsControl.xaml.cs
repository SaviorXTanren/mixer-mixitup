using MixItUp.Base.ViewModel.Controls.Settings;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Settings
{
    /// <summary>
    /// Interaction logic for GeneralSettingsControl.xaml
    /// </summary>
    public partial class GeneralSettingsControl : SettingsControlBase
    {
        private GeneralSettingsControlViewModel viewModel;

        public GeneralSettingsControl()
        {
            InitializeComponent();

            this.DataContext = this.viewModel = new GeneralSettingsControlViewModel();
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
