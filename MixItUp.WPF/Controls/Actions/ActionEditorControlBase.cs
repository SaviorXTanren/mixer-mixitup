using MixItUp.Base.Services;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MixItUp.WPF.Controls.Actions
{
    public class ActionEditorControlBase : UserControl
    {
        protected void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            ServiceManager.Get<IProcessService>().LaunchLink(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}
