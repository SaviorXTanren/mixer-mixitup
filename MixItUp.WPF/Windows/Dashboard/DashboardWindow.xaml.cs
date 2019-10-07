using System.Threading.Tasks;
using MixItUp.Base.ViewModel.Window.Dashboard;
using MixItUp.WPF.Controls.Dashboard;

namespace MixItUp.WPF.Windows.Dashboard
{
    /// <summary>
    /// Interaction logic for DashboardWindow.xaml
    /// </summary>
    public partial class DashboardWindow : LoadingWindowBase
    {
        private DashboardWindowViewModel viewModel;

        public DashboardWindow()
            : base(new DashboardWindowViewModel())
        {
            InitializeComponent();

            this.viewModel = (DashboardWindowViewModel)this.ViewModel;

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            this.viewModel.ChatControl = new ChatDashboardControl();
            this.viewModel.NotificationsControl = new NotificationsDashboardControl();
            this.viewModel.GameQueueControl = new GameQueueDashboardControl();
            this.viewModel.SongRequestsControl = new SongRequestsDashboardControl();

            await base.OnLoaded();
        }
    }
}
