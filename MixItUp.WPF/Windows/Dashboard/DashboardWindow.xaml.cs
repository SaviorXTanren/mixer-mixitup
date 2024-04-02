using MixItUp.Base;
using MixItUp.Base.ViewModel.Dashboard;
using MixItUp.WPF.Controls.Dashboard;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

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
        private GameQueueDashboardControl gameQueueControl = new GameQueueDashboardControl();
        private QuickCommandsDashboardControl quickCommandsControl = new QuickCommandsDashboardControl();
        private RedemptionStoreDashboardControl redemptionStoreControl = new RedemptionStoreDashboardControl();

        public DashboardWindow()
            : base(new DashboardWindowViewModel())
        {
            InitializeComponent();

            this.viewModel = (DashboardWindowViewModel)this.ViewModel;

            this.Initialize(this.StatusBar);

            if (ChannelSession.AppSettings.DashboardWidth > 0)
            {
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.Height = ChannelSession.AppSettings.DashboardHeight;
                this.Width = ChannelSession.AppSettings.DashboardWidth;
                this.Top = ChannelSession.AppSettings.DashboardTop;
                this.Left = ChannelSession.AppSettings.DashboardLeft;
                this.viewModel.IsPinned = ChannelSession.AppSettings.IsDashboardPinned;
                RefreshPin();

                var rect = new System.Drawing.Rectangle((int)this.Left, (int)this.Top, (int)this.Width, (int)this.Height);
                var screen = System.Windows.Forms.Screen.FromRectangle(rect);
                if (!screen.Bounds.Contains(rect))
                {
                    // Off the bottom of the screen?
                    if (this.Top + this.Height > screen.Bounds.Top + screen.Bounds.Height)
                    {
                        this.Top = screen.Bounds.Top + screen.Bounds.Height - this.Height;
                    }

                    // Off the right side of the screen?
                    if (this.Left + this.Width > screen.Bounds.Left + screen.Bounds.Width)
                    {
                        this.Left = screen.Bounds.Left + screen.Bounds.Width - this.Width;
                    }

                    // Off the top of the screen?
                    if (this.Top < screen.Bounds.Top)
                    {
                        this.Top = screen.Bounds.Top;
                    }

                    // Off the left side of the screen?
                    if (this.Left < screen.Bounds.Left)
                    {
                        this.Left = screen.Bounds.Left;
                    }
                }

                if (ChannelSession.AppSettings.IsMaximized)
                {
                    WindowState = WindowState.Maximized;
                }
            }
        }

        protected override async Task OnLoaded()
        {
            await this.chatControl.Initialize(this);
            await this.alertsControl.Initialize(this);
            await this.gameQueueControl.Initialize(this);
            await this.quickCommandsControl.Initialize(this);
            await this.redemptionStoreControl.Initialize(this);

            this.viewModel.ChatControl = this.chatControl;
            this.viewModel.AlertsControl = this.alertsControl;
            this.viewModel.GameQueueControl = this.gameQueueControl;
            this.viewModel.QuickCommandsControl = this.quickCommandsControl;
            this.viewModel.RedemptionStoreControl = this.redemptionStoreControl;

            await this.viewModel.OnOpen();

            await base.OnLoaded();
        }

        protected override Task OnClosing()
        {
            if (this.WindowState == WindowState.Maximized)
            {
                // Use the RestoreBounds as the current values will be 0, 0 and the size of the screen
                ChannelSession.AppSettings.DashboardTop = RestoreBounds.Top;
                ChannelSession.AppSettings.DashboardLeft = RestoreBounds.Left;
                ChannelSession.AppSettings.DashboardHeight = RestoreBounds.Height;
                ChannelSession.AppSettings.DashboardWidth = RestoreBounds.Width;
                ChannelSession.AppSettings.IsDashboardMaximized = true;
            }
            else
            {
                ChannelSession.AppSettings.DashboardTop = this.Top;
                ChannelSession.AppSettings.DashboardLeft = this.Left;
                ChannelSession.AppSettings.DashboardHeight = this.Height;
                ChannelSession.AppSettings.DashboardWidth = this.Width;
                ChannelSession.AppSettings.IsDashboardMaximized = false;
            }

            ChannelSession.AppSettings.IsDashboardPinned = this.viewModel.IsPinned;

            return base.OnClosing();
        }

        private void TogglePin_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.IsPinned = !this.viewModel.IsPinned;
            RefreshPin();
        }

        private void RefreshPin()
        {
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
