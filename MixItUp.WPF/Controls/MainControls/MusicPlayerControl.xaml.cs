using MixItUp.Base.ViewModel;
using MixItUp.Base.ViewModel.MainControls;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for MusicPlayerControl.xaml
    /// </summary>
    public partial class MusicPlayerControl : MainControlBase
    {
        private MusicPlayerMainControlViewModel viewModel;

        public MusicPlayerControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new MusicPlayerMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);
            await this.viewModel.OnOpen();
            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.viewModel.OnVisible();
        }
    }
}
