using MixItUp.Base.ViewModel.Controls.MainControls;
using System.Threading.Tasks;
using System.Windows.Controls;

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
                Button button = (Button)sender;
                QueueUser queueUser = (QueueUser)button.DataContext;
                this.viewModel.MoveUpCommand.Execute(queueUser.user);
                return Task.FromResult(0);
            });
        }

        private async void MoveDownButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(() =>
            {
                Button button = (Button)sender;
                QueueUser queueUser = (QueueUser)button.DataContext;
                this.viewModel.MoveDownCommand.Execute(queueUser.user);
                return Task.FromResult(0);
            });
        }

        private async void DeleteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(() =>
            {
                Button button = (Button)sender;
                QueueUser queueUser = (QueueUser)button.DataContext;
                this.viewModel.DeleteCommand.Execute(queueUser.user);
                return Task.FromResult(0);
            });
        }
    }
}
