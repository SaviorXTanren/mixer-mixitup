using MixItUp.Base.ViewModel.Settings;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Settings
{
    /// <summary>
    /// Interaction logic for CountersSettingsControl.xaml
    /// </summary>
    public partial class CountersSettingsControl : SettingsControlBase
    {
        private CountersSettingsControlViewModel viewModel;

        public CountersSettingsControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new CountersSettingsControlViewModel();
            await this.viewModel.OnOpen();

            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.viewModel.OnVisible();
        }
    }
}