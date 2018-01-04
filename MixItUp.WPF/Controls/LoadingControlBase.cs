using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls
{
    public class LoadingControlBase : NotifyPropertyChangedUserControl
    {
        public LoadingControlBase()
        {
            this.Loaded += LoadingControlBase_Loaded;
            this.IsVisibleChanged += LoadingControlBase_IsVisibleChanged;
        }

        protected virtual Task OnLoaded() { return Task.FromResult(0); }

        private async void LoadingControlBase_Loaded(object sender, RoutedEventArgs e) { await this.OnLoaded(); }

        protected virtual Task OnVisibilityChanged() { return Task.FromResult(0); }

        private async void LoadingControlBase_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) { await this.OnVisibilityChanged(); }
    }
}
