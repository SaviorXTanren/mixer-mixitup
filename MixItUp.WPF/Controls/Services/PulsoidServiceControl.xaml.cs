using MixItUp.Base.ViewModel.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for PulsoidServiceControl.xaml
    /// </summary>
    public partial class PulsoidServiceControl : ServiceControlBase
    {
        private PulsoidServiceControlViewModel viewModel;

        public PulsoidServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new PulsoidServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnOpen();
        }
    }
}