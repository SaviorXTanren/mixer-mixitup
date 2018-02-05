using MixItUp.WPF.Windows;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;

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

        protected virtual Task OnVisibilityChanged() { return Task.FromResult(0); }

        protected void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private async void MainControlBase_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e) { await this.OnVisibilityChanged(); }
    }
}
