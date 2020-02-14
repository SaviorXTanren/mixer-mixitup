using MixItUp.Base.ViewModel.Controls.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for TwitterServiceControl.xaml
    /// </summary>
    public partial class TwitterServiceControl : ServiceControlBase
    {
        private TwitterServiceControlViewModel viewModel;

        public TwitterServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new TwitterServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnLoaded();
        }
    }
}
