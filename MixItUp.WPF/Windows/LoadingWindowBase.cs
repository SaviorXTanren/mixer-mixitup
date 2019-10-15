using MixItUp.Base.ViewModel.Window;
using MixItUp.WPF.Controls;
using MixItUp.WPF.Util;
using StreamingClient.Base.Util;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Windows
{
    public class LoadingWindowBase : Window
    {
        public WindowViewModelBase ViewModel { get; private set; }

        private int asyncOperationCount = 0;

        private LoadingStatusBar statusBar;

        public LoadingWindowBase(WindowViewModelBase viewModel)
            : this()
        {
            this.DataContext = this.ViewModel = viewModel;
        }

        public LoadingWindowBase()
        {
            this.Loaded += LoadingWindowBase_Loaded;
            this.Closing += LoadingWindowBase_Closing;
            this.Activated += LoadingWindowBase_Activated;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        public void Initialize(LoadingStatusBar statusBar)
        {
            this.statusBar = statusBar;
        }

        public async Task RunAsyncOperation(Func<Task> action)
        {
            this.StartAsyncOperation();

            await action();

            this.EndAsyncOperation();
        }

        public async Task<T> RunAsyncOperation<T>(Func<Task<T>> action)
        {
            this.StartAsyncOperation();

            T result = await action();

            this.EndAsyncOperation();

            return result;
        }

        protected void ShowMainWindow(Window window)
        {
            Application.Current.MainWindow = window;
            window.Show();
        }

        protected void StartAsyncOperation()
        {
            try
            {
                this.asyncOperationCount++;
                this.IsEnabled = false;
                this.statusBar.ShowProgressBar();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        protected void EndAsyncOperation()
        {
            try
            {
                this.asyncOperationCount--;
                if (this.asyncOperationCount == 0)
                {
                    this.statusBar.HideProgressBar();
                    this.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        protected virtual Task OnLoaded() { return Task.FromResult(0); }

        protected virtual Task OnClosing() { return Task.FromResult(0); }

        private async void LoadingWindowBase_Loaded(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                await this.OnLoaded();
            });
        }

        private async void LoadingWindowBase_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                await this.OnClosing();
            });
        }

        private void LoadingWindowBase_Activated(object sender, EventArgs e)
        {
            MessageBoxHelper.SetLastActiveWindow(this);
            WPFDialogShower.SetLastActiveWindow(this);
        }
    }
}
