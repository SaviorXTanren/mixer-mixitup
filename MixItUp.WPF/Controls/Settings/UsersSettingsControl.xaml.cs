using MixItUp.Base.ViewModel.Settings;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Settings
{
    /// <summary>
    /// Interaction logic for UsersSettingsControl.xaml
    /// </summary>
    public partial class UsersSettingsControl : SettingsControlBase
    {
        private UsersSettingsControlViewModel viewModel;

        public UsersSettingsControl()
        {
            InitializeComponent();

            this.DataContext = this.viewModel = new UsersSettingsControlViewModel();
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