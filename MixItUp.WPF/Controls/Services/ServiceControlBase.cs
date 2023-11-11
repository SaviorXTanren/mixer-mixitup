using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.Services;
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
                ServiceManager.Get<IProcessService>().LaunchLink(e.Uri.AbsoluteUri);
            }
            else
            {
                ServiceManager.Get<IProcessService>().LaunchFolder(e.Uri.OriginalString);
            }
        }
    }
}
