using MixItUp.Base.ViewModel.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for MeldStudioServiceControl.xaml
    /// </summary>
    public partial class MeldStudioServiceControl : ServiceControlBase
    {
        private MeldStudioServiceControlViewModel viewModel;

        public MeldStudioServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new MeldStudioServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnOpen();
        }
    }
}
