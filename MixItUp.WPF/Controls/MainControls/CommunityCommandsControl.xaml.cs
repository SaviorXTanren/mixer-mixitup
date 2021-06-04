using MixItUp.Base.ViewModel;
using MixItUp.Base.ViewModel.MainControls;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for CommunityCommandsControl.xaml
    /// </summary>
    public partial class CommunityCommandsControl : MainControlBase
    {
        private CommunityCommandsMainControlViewModel viewModel;

        public CommunityCommandsControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new CommunityCommandsMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);
            await this.viewModel.OnLoaded();
            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.viewModel.OnVisible();
        }
    }
}
