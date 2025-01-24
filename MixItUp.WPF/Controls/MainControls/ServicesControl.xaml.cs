using MixItUp.Base.Util;
using MixItUp.WPF.Controls.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for ServicesControl.xaml
    /// </summary>
    public partial class ServicesControl : MainControlBase
    {
        private ObservableCollection<UserControl> services = new ObservableCollection<UserControl>();

        public ServicesControl()
        {
            InitializeComponent();

            this.ServicesListView.ItemsSource = services;
        }

        protected override async Task InitializeInternal()
        {
            List<ServiceContainerControl> services = new List<ServiceContainerControl>();

            services.Add(new ServiceContainerControl(this.Window, new AmazonPollyServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new CrowdControlServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new DeveloperAPIServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new DiscordServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new DonorDriveServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new IFTTTServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new InfiniteAlbumServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new JustGivingServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new LoupeDeckServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new LumiaStreamServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new MeldStudioServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new MicrosoftAzureSpeechServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new MtionStudioServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new OBSStudioServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new OverlayServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new OvrStreamServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new PatreonServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new PixelChatServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new PolyPopServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new PulsoidServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new RainmakerServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new SAMMIServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new StreamAvatarsServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new StreamDeckServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new StreamElementsServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new StreamlabsServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new StreamlabsDesktopServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new StreamlootsServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new TiltifyServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new TipeeeStreamServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new TITSServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new TreatStreamServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new TTSMonsterServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new VoicemodServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new VTSPogServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new VTubeStudioServiceControl()));
            services.Add(new ServiceContainerControl(this.Window, new XSplitServiceControl()));

            this.services.ClearAndAddRange(services);

            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.InitializeInternal();
        }
    }
}
