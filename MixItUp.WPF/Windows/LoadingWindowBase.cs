using MixItUp.WPF.Controls;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Windows
{
    public class LoadingWindowBase : Window
    {
        public LoadingStatusBar statusBar { get; private set; }

        public LoadingWindowBase()
        {
            this.Loaded += LoadingWindowBase_Loaded;
            this.Closing += LoadingWindowBase_Closing;
        }

        public void SetStatusBar(LoadingStatusBar statusBar) { this.statusBar = statusBar; }

        public async Task RunAsyncOperation(Func<Task> action)
        {
            this.IsEnabled = false;
            this.statusBar.ShowProgressBar();

            await action();

            this.statusBar.HideProgressBar();
            this.IsEnabled = true;
        }

        public async Task<T> RunAsyncOperation<T>(Func<Task<T>> action)
        {
            this.IsEnabled = false;
            this.statusBar.ShowProgressBar();

            T result = await action();

            this.statusBar.HideProgressBar();
            this.IsEnabled = true;

            return result;
        }

        protected virtual Task OnLoaded() { return Task.FromResult(0); }

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
    }
}
