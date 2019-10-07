using MixItUp.Base;
using MixItUp.Base.ViewModel.Controls.Dashboard;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Dashboard
{
    /// <summary>
    /// Interaction logic for NotificationsDashboardControl.xaml
    /// </summary>
    public partial class AlertsDashboardControl : DashboardControlBase
    {
        private AlertsDashboardControlViewModel viewModel;

        private ScrollViewer scrollViewer;

        public AlertsDashboardControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new AlertsDashboardControlViewModel(this.Window.ViewModel);
            await this.viewModel.OnLoaded();

            await base.InitializeInternal();
        }

        private void NotificationsListView_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
        {
            if (this.scrollViewer == null)
            {
                this.scrollViewer = (ScrollViewer)e.OriginalSource;
            }

            if (this.scrollViewer != null)
            {
                if (ChannelSession.Settings.LatestChatAtTop)
                {
                    this.scrollViewer.ScrollToTop();
                }
                else
                {
                    this.scrollViewer.ScrollToBottom();
                }
            }
        }
    }
}
