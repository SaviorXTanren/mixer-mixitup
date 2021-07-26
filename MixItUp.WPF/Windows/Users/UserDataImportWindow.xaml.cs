using MixItUp.Base.ViewModel.User;
using System.Threading.Tasks;

namespace MixItUp.WPF.Windows.Users
{
    /// <summary>
    /// Interaction logic for UserDataImportWindow.xaml
    /// </summary>
    public partial class UserDataImportWindow : LoadingWindowBase
    {
        private UserDataImportWindowViewModel viewModel;

        public UserDataImportWindow()
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override Task OnLoaded()
        {
            this.DataContext = this.viewModel = new UserDataImportWindowViewModel();
            return Task.CompletedTask;
        }
    }
}
