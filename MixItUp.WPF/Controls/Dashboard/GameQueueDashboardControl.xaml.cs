using MixItUp.Base.ViewModel.MainControls;
using MixItUp.WPF.Util;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Dashboard
{
    /// <summary>
    /// Interaction logic for GameQueueDashboardControl.xaml
    /// </summary>
    public partial class GameQueueDashboardControl : DashboardControlBase
    {
        private GameQueueMainControlViewModel viewModel;

        public GameQueueDashboardControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new GameQueueMainControlViewModel(this.Window.ViewModel);
            await this.viewModel.OnLoaded();
        }

        private async void MoveUpButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(() =>
            {
                QueueUser queueUser = FrameworkElementHelpers.GetDataContext<QueueUser>(sender);
                this.viewModel.MoveUpCommand.Execute(queueUser.user);
                return Task.FromResult(0);
            });
        }

        private async void MoveDownButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(() =>
            {
                QueueUser queueUser = FrameworkElementHelpers.GetDataContext<QueueUser>(sender);
                this.viewModel.MoveDownCommand.Execute(queueUser.user);
                return Task.FromResult(0);
            });
        }

        private async void DeleteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(() =>
            {
                QueueUser queueUser = FrameworkElementHelpers.GetDataContext<QueueUser>(sender);
                this.viewModel.DeleteCommand.Execute(queueUser.user);
                return Task.FromResult(0);
            });
        }
    }
}
