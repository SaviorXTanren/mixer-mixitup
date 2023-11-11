using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.Services;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for OBSStudioServiceControl.xaml
    /// </summary>
    public partial class OBSStudioServiceControl : ServiceControlBase
    {
        private OBSStudioServiceControlViewModel viewModel;

        public OBSStudioServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new OBSStudioServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnOpen();

            this.viewModel.Password = () =>
            {
                return Dispatcher.Invoke(() => { return this.PasswordBox.Password; });
            };
        }

        protected override void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            ServiceManager.Get<IProcessService>().LaunchFolder("Assets\\OBS");
        }
    }
}
