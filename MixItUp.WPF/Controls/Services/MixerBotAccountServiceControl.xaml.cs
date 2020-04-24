using MixItUp.Base.ViewModel.Controls.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for MixerBotAccountServiceControl.xaml
    /// </summary>
    public partial class MixerBotAccountServiceControl : ServiceControlBase
    {
        private MixerBotAccountServiceControlViewModel viewModel;

        public MixerBotAccountServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new MixerBotAccountServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnLoaded();
        }
    }

    public class MixerBotAccountServiceControlViewModel : ServiceControlViewModelBase
    {
        public MixerBotAccountServiceControlViewModel() : base("Mixer Bot Account") { }
    }
}