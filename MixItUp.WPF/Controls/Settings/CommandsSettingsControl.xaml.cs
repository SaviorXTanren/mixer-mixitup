using MixItUp.Base.ViewModel.Controls.Settings;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Settings
{
    /// <summary>
    /// Interaction logic for CommandsSettingsControl.xaml
    /// </summary>
    public partial class CommandsSettingsControl : SettingsControlBase
    {
        private CommandsSettingsControlViewModel viewModel;

        public CommandsSettingsControl()
        {
            InitializeComponent();

            this.DataContext = this.viewModel = new CommandsSettingsControlViewModel();
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
