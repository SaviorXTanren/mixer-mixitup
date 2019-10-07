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

        private ChatDashboardControl chatControl = new ChatDashboardControl();
        private NotificationsDashboardControl notificationsControl = new NotificationsDashboardControl();
        private GameQueueDashboardControl gameQueueControl = new GameQueueDashboardControl();
        private SongRequestsDashboardControl songRequestsControl = new SongRequestsDashboardControl();

        public DashboardWindow()
            : base(new DashboardWindowViewModel())
        {
            InitializeComponent();

            this.viewModel = (DashboardWindowViewModel)this.ViewModel;

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            await this.chatControl.Initialize(this);
            await this.notificationsControl.Initialize(this);
            await this.gameQueueControl.Initialize(this);
            await this.songRequestsControl.Initialize(this);

            this.viewModel.ChatControl = this.chatControl;
            this.viewModel.NotificationsControl = this.notificationsControl;
            this.viewModel.GameQueueControl = this.gameQueueControl;
            this.viewModel.SongRequestsControl = this.songRequestsControl;

            await base.OnLoaded();
        }
    }
}
