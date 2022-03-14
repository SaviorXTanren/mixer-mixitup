using MixItUp.Base.ViewModel.Settings;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Settings
{
    /// <summary>
    /// Interaction logic for NotificationsSettingsControl.xaml
    /// </summary>
    public partial class NotificationsSettingsControl : SettingsControlBase
    {
        private NotificationsSettingsControlViewModel viewModel;

        public NotificationsSettingsControl()
        {
            InitializeComponent();

            this.DataContext = this.viewModel = new NotificationsSettingsControlViewModel();
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
