using MixItUp.Base.ViewModel.Controls.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for ExtraLifeServiceControl.xaml
    /// </summary>
    public partial class ExtraLifeServiceControl : ServiceControlBase
    {
        private ExtraLifeServiceControlViewModel viewModel;

        public ExtraLifeServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new ExtraLifeServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnLoaded();
        }
    }
}
