using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Controls.Services;
using System.Windows.Navigation;

namespace MixItUp.WPF.Controls.Services
{
    public class ServiceControlBase : LoadingControlBase
    {
        public ServiceControlViewModelBase ViewModel { get; protected set; }

        protected ServiceContainerControl containerControl { get; private set; }

        public void Initialize(ServiceContainerControl containerControl) { this.containerControl = containerControl; }

        protected virtual void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            if (e.Uri.IsAbsoluteUri)
            {
                ProcessHelper.LaunchLink(e.Uri.AbsoluteUri);
            }
            else
            {
                ProcessHelper.LaunchFolder(e.Uri.OriginalString);
            }
        }
    }
}
