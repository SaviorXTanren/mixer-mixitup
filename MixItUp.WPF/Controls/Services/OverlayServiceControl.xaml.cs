using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.Services;
using MixItUp.WPF.Util;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for OverlayServiceControl.xaml
    /// </summary>
    public partial class OverlayServiceControl : ServiceControlBase
    {
        private OverlayServiceControlViewModel viewModel;

        public OverlayServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new OverlayServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnOpen();
        }

        private void OpenEndpointURLButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ServiceManager.Get<IProcessService>().LaunchLink(ChannelSession.Settings.OverlayEndpointsV3.First(oe => oe.ID == Guid.Empty).Address);
        }

        private async void CopyEndpointURLButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await UIHelpers.CopyToClipboard(ChannelSession.Settings.OverlayEndpointsV3.First(oe => oe.ID == Guid.Empty).Address);
        }
    }
}
