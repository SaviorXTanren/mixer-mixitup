using MixItUp.Base.Actions;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MixItUp.WPF.Controls.Actions
{
    public abstract class ActionControlBase : UserControl
    {
        protected ActionContainerControl containerControl;

        public ActionControlBase(ActionContainerControl containerControl)
        {
            this.containerControl = containerControl;

            this.Loaded += ActionControlBase_Loaded;
        }

        public virtual Task OnLoaded() { return Task.FromResult(0); }

        public abstract ActionBase GetAction();

        protected void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private async void ActionControlBase_Loaded(object sender, RoutedEventArgs e) { await this.OnLoaded(); }
    }
}
