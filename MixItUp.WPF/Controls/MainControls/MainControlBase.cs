using MixItUp.WPF.Windows;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.MainControls
{
    public class MainControlBase : UserControl
    {
        public LoadingWindowBase Window { get; private set; }

        public MainControlBase() { this.IsVisibleChanged += MainControlBase_IsVisibleChanged; }

        public async Task Initialize(LoadingWindowBase window)
        {
            this.Window = window;
            await this.InitializeInternal();
        }

        protected virtual Task InitializeInternal() { return Task.FromResult(0); }

        protected virtual Task VisibilityChanged() { return Task.FromResult(0); }

        private async void MainControlBase_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e) { await this.VisibilityChanged(); }
    }
}
