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

        protected virtual Task OnLoaded() { return Task.CompletedTask; }

        private async void LoadingControlBase_Loaded(object sender, RoutedEventArgs e) { await this.OnLoaded(); }

        protected virtual Task OnVisibilityChanged() { return Task.CompletedTask; }

        private async void LoadingControlBase_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsLoaded)
            {
                await this.OnVisibilityChanged();
            }
        }
    }
}
