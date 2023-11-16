using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Settings
{
    public class OverlayEndpointListingViewModel : UIViewModelBase
    {
        public string Name { get; set; }
        public int Port { get; set; }

        public string Address { get { return string.Format(OverlayEndpointService.RegularOverlayHttpListenerServerAddressFormat, this.Port); } }

        public bool CanDelete { get { return !ServiceManager.Get<OverlayService>().DefaultOverlayName.Equals(this.Name); } }

        public ICommand DeleteCommand { get; set; }

        private OverlaySettingsControlViewModel viewModel;

        public OverlayEndpointListingViewModel(OverlaySettingsControlViewModel viewModel, string name, int port)
        {
            this.viewModel = viewModel;
            this.Name = name;
            this.Port = port;

            this.DeleteCommand = this.CreateCommand(async () =>
            {
                ChannelSession.Settings.OverlayCustomNameAndPorts.Remove(this.Name);
                await ServiceManager.Get<OverlayService>().RemoveOverlay(this.Name);
                this.viewModel.Endpoints.Remove(this);
            });
        }
    }

    public class OverlaySettingsControlViewModel : UIViewModelBase
    {
        public ObservableCollection<OverlayEndpointListingViewModel> Endpoints { get; set; } = new ObservableCollection<OverlayEndpointListingViewModel>();

        public string NewEndpointName
        {
            get { return this.newEndpointName; }
            set
            {
                this.newEndpointName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string newEndpointName;

        public ICommand AddCommand { get; set; }

        public OverlaySettingsControlViewModel()
        {
            this.AddCommand = this.CreateCommand(async () =>
            {
                if (!string.IsNullOrEmpty(this.NewEndpointName))
                {
                    if (!this.Endpoints.Any(p => p.Name.Equals(this.NewEndpointName)))
                    {
                        int port = this.Endpoints.Max(o => o.Port) + 1;
                        OverlayEndpointListingViewModel overlay = new OverlayEndpointListingViewModel(this, this.NewEndpointName, port);

                        ChannelSession.Settings.OverlayCustomNameAndPorts[overlay.Name] = overlay.Port;
                        await ServiceManager.Get<OverlayService>().AddOverlay(overlay.Name, overlay.Port);
                        this.Endpoints.Add(overlay);
                    }
                }
                this.NewEndpointName = string.Empty;
            });
        }

        protected override Task OnOpenInternal()
        {
            this.Endpoints.ClearAndAddRange(ServiceManager.Get<OverlayService>().AllOverlayNameAndPorts.OrderBy(kvp => kvp.Value).Select(kvp => new OverlayEndpointListingViewModel(this, kvp.Key, kvp.Value)));
            return Task.CompletedTask;
        }
    }
}
