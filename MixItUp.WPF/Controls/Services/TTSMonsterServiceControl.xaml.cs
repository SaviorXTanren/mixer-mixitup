using MixItUp.Base.ViewModel.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for TTSMonsterServiceControl.xaml
    /// </summary>
    public partial class TTSMonsterServiceControl : ServiceControlBase
    {
        private TTSMonsterServiceControlViewModel viewModel;

        public TTSMonsterServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new TTSMonsterServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnOpen();
        }
    }
}
