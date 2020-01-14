using MixItUp.Base.ViewModel.Controls.Services;

namespace MixItUp.WPF.Controls.Services
{
    public class ServiceControlBase : LoadingControlBase
    {
        public ServiceControlViewModelBase ViewModel { get; protected set; }

        protected ServiceContainerControl containerControl { get; private set; }

        public void Initialize(ServiceContainerControl containerControl) { this.containerControl = containerControl; }
    }
}
