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
        private AlertsDashboardControl alertsControl = new AlertsDashboardControl();
        private StatisticsDashboardControl statisticsControl = new StatisticsDashboardControl();
        private GameQueueDashboardControl gameQueueControl = new GameQueueDashboardControl();
        private QuickCommandsDashboardControl quickCommandsControl = new QuickCommandsDashboardControl();

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
            await this.alertsControl.Initialize(this);
            await this.statisticsControl.Initialize(this);
            await this.gameQueueControl.Initialize(this);
            await this.quickCommandsControl.Initialize(this);

            this.viewModel.ChatControl = this.chatControl;
            this.viewModel.AlertsControl = this.alertsControl;
            this.viewModel.StatisticsControl = this.statisticsControl;
            this.viewModel.GameQueueControl = this.gameQueueControl;
            this.viewModel.QuickCommandsControl = this.quickCommandsControl;

            await this.viewModel.OnLoaded();

            await base.OnLoaded();
        }
    }
}
