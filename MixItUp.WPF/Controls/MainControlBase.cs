using MixItUp.WPF.Windows;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls
{
    public class MainControlBase : UserControl
    {
        public LoadingWindowBase Window { get; private set; }

        public async Task Initialize(LoadingWindowBase window)
        {
            this.Window = window;
            await this.InitializeInternal();
        }

        protected virtual Task InitializeInternal() { return Task.FromResult(0); }
    }
}
