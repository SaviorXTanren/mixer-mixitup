using MixItUp.WPF.Windows;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls
{
    public class LoadingWindowControlBase : LoadingControlBase
    {
        public LoadingWindowBase Window { get; private set; }

        public LoadingWindowControlBase() { this.IsVisibleChanged += LoadingWindowControlBase_IsVisibleChanged; }

        public async Task Initialize(LoadingWindowBase window)
        {
            this.Window = window;
            await this.Window.RunAsyncOperation(async () =>
            {
                await this.InitializeInternal();
                await this.OnVisibilityChanged();
            });
        }

        protected virtual Task InitializeInternal() { return Task.CompletedTask; }

        protected virtual async void LoadingWindowControlBase_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                if (this.Window != null)
                {
                    await this.Window.RunAsyncOperation(async () =>
                    {
                        await this.OnVisibilityChanged();
                    });
                }
            }
        }
    }
}
