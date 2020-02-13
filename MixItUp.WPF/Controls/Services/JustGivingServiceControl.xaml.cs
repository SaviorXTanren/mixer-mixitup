using MixItUp.Base.ViewModel.Controls.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for JustGivingServiceControl.xaml
    /// </summary>
    public partial class JustGivingServiceControl : ServiceControlBase
    {
        private JustGivingServiceControlViewModel viewModel;

        public JustGivingServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new JustGivingServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnLoaded();
        }
    }
}
