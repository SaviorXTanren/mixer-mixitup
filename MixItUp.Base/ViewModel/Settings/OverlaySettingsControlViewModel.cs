using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Settings
{
    public class OverlayEndpointListingViewModel : UIViewModelBase
    {
        public Guid ID { get { return this.model.ID; } }
        public string Name { get { return this.model.Name; } }
        public int PortNumber { get { return this.model.PortNumber; } }

        public string Address { get { return string.Format(OverlayEndpointService.RegularOverlayHttpListenerServerAddressFormat, this.PortNumber); } }

        public bool CanDelete { get { return !OverlayEndpointV3Model.DefaultOverlayName.Equals(this.Name); } }

        public ICommand DeleteCommand { get; set; }

        private OverlaySettingsControlViewModel viewModel;
        private OverlayEndpointV3Model model;

        public OverlayEndpointListingViewModel(OverlaySettingsControlViewModel viewModel, OverlayEndpointV3Model model)
        {
            this.viewModel = viewModel;
            this.model = model;

            this.DeleteCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<OverlayService>().RemoveOverlayEndpoint(this.model.ID);
                ChannelSession.Settings.OverlayEndpointsV3.Remove(this.model);
                this.viewModel.Endpoints.Remove(this);
            });
        }
    }

    public class OverlaySettingsControlViewModel : UIViewModelBase
    {
        public ThreadSafeObservableCollection<OverlayEndpointListingViewModel> Endpoints { get; set; } = new ThreadSafeObservableCollection<OverlayEndpointListingViewModel>();

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

        public int NewEndpointPortNumber
        {
            get { return this.newEndpointPortNumber; }
            set
            {
                this.newEndpointPortNumber = value;
                this.NotifyPropertyChanged();
            }
        }
        private int newEndpointPortNumber;

        public ICommand AddCommand { get; set; }

        public OverlaySettingsControlViewModel()
        {
            this.AddCommand = this.CreateCommand(async () =>
            {
                if (!string.IsNullOrEmpty(this.NewEndpointName) && this.NewEndpointPortNumber > 0 && this.NewEndpointPortNumber < Math.Pow(2, 16))
                {
                    if (!this.Endpoints.Any(e => e.Name.Equals(this.NewEndpointName)) && !this.Endpoints.Any(e => e.PortNumber == this.NewEndpointPortNumber))
                    {
                        OverlayEndpointV3Model overlayEndpoint = new OverlayEndpointV3Model(this.NewEndpointPortNumber, this.NewEndpointName);

                        if (await ServiceManager.Get<OverlayV3Service>().AddOverlayEndpoint(overlayEndpoint))
                        {
                            ChannelSession.Settings.OverlayEndpointsV3.Add(overlayEndpoint);
                            this.Endpoints.Add(new OverlayEndpointListingViewModel(this, overlayEndpoint));
                        }
                    }
                }
                this.NewEndpointName = string.Empty;
                this.NewEndpointPortNumber = this.Endpoints.Max(o => o.PortNumber) + 1;
            });
        }

        protected override Task OnOpenInternal()
        {
            this.Endpoints.ClearAndAddRange(ServiceManager.Get<OverlayV3Service>().GetOverlayEndpoints().Select(oe => new OverlayEndpointListingViewModel(this, oe)));

            this.NewEndpointPortNumber = this.Endpoints.Max(o => o.PortNumber) + 1;

            return Task.CompletedTask;
        }
    }
}
