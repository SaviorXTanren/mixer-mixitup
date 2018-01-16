using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Services
{
    public abstract class ServicesControlBase : UserControl
    {
        protected ServicesGroupBoxControl groupBoxControl { get; private set; }

        public ServicesControlBase()
        {
            this.Loaded += ServicesControlBase_Loaded;
        }

        public void Initialize(ServicesGroupBoxControl groupBoxControl) { this.groupBoxControl = groupBoxControl; }

        protected virtual Task OnLoaded() { return Task.FromResult(0); }

        protected void SetHeaderText(string text) { this.groupBoxControl.SetHeaderText(text); }

        protected void SetCompletedIcon(bool visible) { this.groupBoxControl.SetCompletedIcon(visible); }

        private async void ServicesControlBase_Loaded(object sender, System.Windows.RoutedEventArgs e) { await this.OnLoaded(); }
    }
}
