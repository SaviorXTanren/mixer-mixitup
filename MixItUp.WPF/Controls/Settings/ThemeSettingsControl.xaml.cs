using MixItUp.Base.ViewModel.Settings;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Settings
{
    /// <summary>
    /// Interaction logic for ThemeSettingsControl.xaml
    /// </summary>
    public partial class ThemeSettingsControl : SettingsControlBase
    {
        private ThemeSettingsControlViewModel viewModel;

        public ThemeSettingsControl()
        {
            InitializeComponent();

            this.DataContext = this.viewModel = new ThemeSettingsControlViewModel();
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
