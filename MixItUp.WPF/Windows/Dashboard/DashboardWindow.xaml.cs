using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using MixItUp.Base.ViewModel.Window.Dashboard;
using MixItUp.WPF.Controls.Dashboard;

namespace MixItUp.WPF.Windows.Dashboard
{
    /// <summary>
    /// Interaction logic for DashboardWindow.xaml
    /// </summary>
    public partial class DashboardWindow : LoadingWindowBase
    {
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

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

        private void TogglePin_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.IsPinned = !this.viewModel.IsPinned;

            if (this.viewModel.IsPinned)
            {
                SetWindowPos(new WindowInteropHelper(this).Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
            }
            else
            {
                SetWindowPos(new WindowInteropHelper(this).Handle, HWND_NOTOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
            }
        }
    }
}
