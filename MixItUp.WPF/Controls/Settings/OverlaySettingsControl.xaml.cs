using MixItUp.Base;
using MixItUp.Overlay;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Settings
{
    public class OverlayEndpointListing
    {
        public string Name { get; set; }
        public int Port { get; set; }

        public string Address { get { return string.Format(OverlayService.RegularOverlayHttpListenerServerAddressFormat, this.Port); } }

        public bool CanDelete { get { return !ChannelSession.Services.OverlayServers.DefaultOverlayName.Equals(this.Name); } }

        public OverlayEndpointListing(string name, int port)
        {
            this.Name = name;
            this.Port = port;
        }
    }

    /// <summary>
    /// Interaction logic for OverlaySettingsControl.xaml
    /// </summary>
    public partial class OverlaySettingsControl : SettingsControlBase
    {
        private ObservableCollection<OverlayEndpointListing> overlays = new ObservableCollection<OverlayEndpointListing>();

        public OverlaySettingsControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.OverlayEndpointsItemsControl.ItemsSource = this.overlays;
            this.overlays.Clear();
            foreach (var kvp in ChannelSession.AllOverlayNameAndPorts.OrderBy(kvp => kvp.Value))
            {
                this.overlays.Add(new OverlayEndpointListing(kvp.Key, kvp.Value));
            }

            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.InitializeInternal();
        }

        private async void AddOverlayButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (!string.IsNullOrEmpty(this.OverlayNameTextBox.Text))
                {
                    if (!this.overlays.Any(p => p.Name.Equals(this.OverlayNameTextBox.Text)))
                    {
                        int port = this.overlays.Max(o => o.Port) + 1;
                        OverlayEndpointListing overlay = new OverlayEndpointListing(this.OverlayNameTextBox.Text, port);

                        ChannelSession.Settings.OverlayCustomNameAndPorts[overlay.Name] = overlay.Port;
                        await ChannelSession.Services.OverlayServers.AddOverlay(overlay.Name, overlay.Port);
                        this.overlays.Add(overlay);
                    }
                }
                this.OverlayNameTextBox.Text = string.Empty;
                return Task.FromResult(0);
            });
        }

        private async void DeleteOverlayButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                Button button = (Button)sender;
                OverlayEndpointListing overlay = (OverlayEndpointListing)button.DataContext;

                ChannelSession.Settings.OverlayCustomNameAndPorts.Remove(overlay.Name);
                await ChannelSession.Services.OverlayServers.RemoveOverlay(overlay.Name);
                this.overlays.Remove(overlay);
            });
        }
    }
}