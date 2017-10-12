using MixItUp.WPF.Controls;
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
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        public void Initialize(LoadingStatusBar statusBar)
        {
            this.statusBar = statusBar;
        }

        public new void Close()
        {
            this.OnClosing().Wait();
            base.Close();
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

        protected virtual Task OnClosing() { return Task.FromResult(0); }

        private async void LoadingWindowBase_Loaded(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                await this.OnLoaded();
            });
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
