using MixItUp.WPF.Controls.MainControls;
using MixItUp.WPF.Controls.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Settings
{
    /// <summary>
    /// Interaction logic for ServicesSettingsControl.xaml
    /// </summary>
    public partial class ServicesSettingsControl : MainControlBase
    {
        private ObservableCollection<ServicesGroupBoxControl> services = new ObservableCollection<ServicesGroupBoxControl>();

        public ServicesSettingsControl()
        {
            InitializeComponent();

            this.ServicesListView.ItemsSource = this.services;
        }

        protected override async Task InitializeInternal()
        {
            this.services.Clear();

            this.services.Add(new ServicesGroupBoxControl(this.Window, new MixerBotAccountServiceControl()));
            this.services.Add(new ServicesGroupBoxControl(this.Window, new OverlayServiceControl()));
            this.services.Add(new ServicesGroupBoxControl(this.Window, new OBSStudioServiceControl()));
            this.services.Add(new ServicesGroupBoxControl(this.Window, new XSplitServiceControl()));
            this.services.Add(new ServicesGroupBoxControl(this.Window, new TwitterServiceControl()));
            this.services.Add(new ServicesGroupBoxControl(this.Window, new StreamlabsServiceControl()));
            this.services.Add(new ServicesGroupBoxControl(this.Window, new DiscordServiceControl()));
            //this.services.Add(new ServicesGroupBoxControl(this.Window, new GameWispServiceControl()));
            this.services.Add(new ServicesGroupBoxControl(this.Window, new SpotifyServiceControl()));
            this.services.Add(new ServicesGroupBoxControl(this.Window, new DeveloperAPIServiceControl()));

            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.InitializeInternal();
        }
    }
}
