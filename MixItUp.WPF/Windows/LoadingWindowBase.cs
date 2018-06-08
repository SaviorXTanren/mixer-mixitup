using MixItUp.WPF.Controls;
using MixItUp.WPF.Util;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Windows
{
    public class LoadingWindowBase : Window
    {
        private int asyncOperationCount = 0;

        private LoadingStatusBar statusBar;

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

        protected virtual Task OnLoaded() { return Task.FromResult(0); }

        protected void ShowMainWindow(Window window)
        {
            Application.Current.MainWindow = window;
            window.Show();
        }

        private async void LoadingWindowBase_Loaded(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                await this.OnLoaded();
            });
        }

        protected virtual Task OnClosing() { return Task.FromResult(0); }

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
        }

        private void StartAsyncOperation()
        {
            this.asyncOperationCount++;
            this.IsEnabled = false;
            this.statusBar.ShowProgressBar();
        }

        private void EndAsyncOperation()
        {
            this.asyncOperationCount--;
            if (this.asyncOperationCount == 0)
            {
                this.statusBar.HideProgressBar();
                this.IsEnabled = true;
            }
        }
    }
}
