using MixItUp.Base.Util;
using MixItUp.WPF.Controls.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for ServicesControl.xaml
    /// </summary>
    public partial class ServicesControl : MainControlBase
    {
        private ThreadSafeObservableCollection<UserControl> services = new ThreadSafeObservableCollection<UserControl>();

        public ServicesControl()
        {
            InitializeComponent();

            this.ServicesListView.ItemsSource = services;
        }

        protected override async Task InitializeInternal()
        {
            List<ServiceContainerControl> services = new List<ServiceContainerControl>();

            services.Add(new ServiceContainerControl(this.Window, new OverlayServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new OBSStudioServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new StreamlabsOBSServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new XSplitServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new OvrStreamServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new StreamlabsServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new StreamElementsServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new TipeeeStreamServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new TreatStreamServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new RainmakerServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new PixelChatServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new VoicemodServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new VTubeStudioServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new PatreonServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new StreamlootsServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new TiltifyServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new ExtraLifeServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new JustGivingServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new TwitterServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new DiscordServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new StreamDeckServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new IFTTTServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new StreamAvatarsServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new DeveloperAPIServiceControl()));

            this.services.ClearAndAddRange(services);

            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.InitializeInternal();
        }
    }
}
