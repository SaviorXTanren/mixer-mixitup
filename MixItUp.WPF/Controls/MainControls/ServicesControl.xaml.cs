using MixItUp.WPF.Controls.Services;
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
            this.services.Clear();

            this.services.Add(new ServicesGroupBoxControl(this.Window, new MixerBotAccountServiceControl()));
            this.services.Add(new ServicesGroupBoxControl(this.Window, new OverlayServiceControl()));
            this.services.Add(new ServicesGroupBoxControl(this.Window, new OBSStudioServiceControl()));
            this.services.Add(new ServicesGroupBoxControl(this.Window, new StreamlabsOBSServiceControl()));
            this.services.Add(new ServicesGroupBoxControl(this.Window, new XSplitServiceControl()));
            this.services.Add(new ServicesGroupBoxControl(this.Window, new OvrStreamServiceControl()));
            this.services.Add(new ServiceContainerControl(this.Window, new StreamlabsServiceControl()));
            this.services.Add(new ServiceContainerControl(this.Window, new TipeeeStreamServiceControl()));
            this.services.Add(new ServiceContainerControl(this.Window, new TreatStreamServiceControl()));
            this.services.Add(new ServiceContainerControl(this.Window, new StreamJarServiceControl()));
            this.services.Add(new ServiceContainerControl(this.Window, new PatreonServiceControl()));
            this.services.Add(new ServiceContainerControl(this.Window, new StreamlootsServiceControl()));
            this.services.Add(new ServiceContainerControl(this.Window, new TiltifyServiceControl()));
            this.services.Add(new ServiceContainerControl(this.Window, new ExtraLifeServiceControl()));
            this.services.Add(new ServiceContainerControl(this.Window, new JustGivingServiceControl()));
            this.services.Add(new ServicesGroupBoxControl(this.Window, new TwitterServiceControl()));
            this.services.Add(new ServicesGroupBoxControl(this.Window, new DiscordServiceControl()));
            this.services.Add(new ServicesGroupBoxControl(this.Window, new StreamDeckServiceControl()));
            this.services.Add(new ServiceContainerControl(this.Window, new IFTTTServiceControl()));
            this.services.Add(new ServiceContainerControl(this.Window, new StreamAvatarsServiceControl()));
            this.services.Add(new ServicesGroupBoxControl(this.Window, new DeveloperAPIServiceControl()));

            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.InitializeInternal();
        }
    }
}
