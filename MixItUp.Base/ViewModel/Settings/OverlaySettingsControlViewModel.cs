using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Settings
{
    public class OverlayEndpointListingViewModel : UIViewModelBase
    {
        public Guid ID { get { return this.Model.ID; } }
        public string Name
        {
            get { return this.Model.Name; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (!this.viewModel.Endpoints.Any(e => e.Name.Equals(value, StringComparison.OrdinalIgnoreCase) && e.ID != this.ID))
                    {
                        this.Model.Name = value;
                        this.NotifyPropertyChanged();
                    }
                }
            }
        }

        public string Address { get { return this.Model.Address; } }

        public bool CanDelete { get { return this.ID != Guid.Empty; } }

        public ICommand DeleteCommand { get; set; }

        public OverlayEndpointV3Model Model { get; private set; }

        private OverlaySettingsControlViewModel viewModel;

        public OverlayEndpointListingViewModel(OverlaySettingsControlViewModel viewModel, OverlayEndpointV3Model model)
        {
            this.viewModel = viewModel;
            this.Model = model;

            this.DeleteCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<OverlayV3Service>().DisconnectOverlayEndpointService(this.Model.ID);
                ChannelSession.Settings.OverlayEndpointsV3.Remove(this.Model);
                this.viewModel.Endpoints.Remove(this);
            });
        }
    }

    public class OverlaySettingsControlViewModel : UIViewModelBase
    {
        public ObservableCollection<OverlayEndpointListingViewModel> Endpoints { get; set; } = new ObservableCollection<OverlayEndpointListingViewModel>();

        public int PortNumber
        {
            get { return this.portNumber; }
            set
            {
                this.portNumber = value;
                this.NotifyPropertyChanged();
            }
        }
        private int portNumber;

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

        public ICommand UpdatePortNumberCommand { get; set; }

        public ICommand AddCommand { get; set; }

        public OverlaySettingsControlViewModel()
        {
            this.UpdatePortNumberCommand = this.CreateCommand(async () =>
            {
                if (this.PortNumber <= 0 || this.PortNumber > 65353)
                {
                    await DialogHelper.ShowMessage(Resources.OverlayPortNumberValidRange);
                    return;
                }

                await ServiceManager.Get<OverlayV3Service>().Disconnect();

                ChannelSession.Settings.OverlayPortNumber = this.PortNumber;

                Result result = await ServiceManager.Get<OverlayV3Service>().Connect();
                if (!result.Success)
                {
                    await ServiceManager.Get<OverlayV3Service>().Disconnect();
                    await DialogHelper.ShowMessage(Resources.OverlayPortNumberFailedToUpdate + Environment.NewLine + Environment.NewLine + result.ToString());
                }
            });

            this.AddCommand = this.CreateCommand(() =>
            {
                if (!string.IsNullOrEmpty(this.NewEndpointName))
                {
                    if (!this.Endpoints.Any(e => e.Name.Equals(this.NewEndpointName)))
                    {
                        OverlayEndpointV3Model overlayEndpoint = new OverlayEndpointV3Model(this.NewEndpointName);
                        ChannelSession.Settings.OverlayEndpointsV3.Add(overlayEndpoint);

                        ServiceManager.Get<OverlayV3Service>().ConnectOverlayEndpointService(overlayEndpoint);

                        this.Endpoints.Add(new OverlayEndpointListingViewModel(this, overlayEndpoint));
                    }
                }
                this.NewEndpointName = string.Empty;
            });
        }

        protected override Task OnOpenInternal()
        {
            this.PortNumber = ChannelSession.Settings.OverlayPortNumber;

            this.Endpoints.ClearAndAddRange(ServiceManager.Get<OverlayV3Service>().GetOverlayEndpoints().Select(oe => new OverlayEndpointListingViewModel(this, oe)));

            return Task.CompletedTask;
        }
    }
}
