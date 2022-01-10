using MixItUp.Base.ViewModel.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for TipeeeStreamServiceControl.xaml
    /// </summary>
    public partial class TipeeeStreamServiceControl : ServiceControlBase
    {
        private TipeeeStreamServiceControlViewModel viewModel;

        public TipeeeStreamServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new TipeeeStreamServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnOpen();
        }
    }
}
