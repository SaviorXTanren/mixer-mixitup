using MixItUp.WPF.Windows;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Dashboard
{
    public abstract class DashboardControlBase : UserControl
    {
        public LoadingWindowBase Window { get; private set; }

        public DashboardControlBase()
        {
            this.IsVisibleChanged += DashboardControlBase_IsVisibleChanged;
            this.GotFocus += DashboardControlBase_GotFocus;
        }

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

        protected virtual Task OnVisibilityChanged() { return Task.CompletedTask; }

        private async void DashboardControlBase_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                return;
            }

            if (this.Window != null)
            {
                await this.Window.RunAsyncOperation(async () =>
                {
                    await this.OnVisibilityChanged();
                });
            }
        }

        private async void DashboardControlBase_GotFocus(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                return;
            }

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
