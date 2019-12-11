using MixItUp.Base;
using MixItUp.Base.Model.SongRequests;
using MixItUp.Base.ViewModel.Controls.MainControls;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Dashboard
{
    /// <summary>
    /// Interaction logic for SongRequestsDashboardControl.xaml
    /// </summary>
    public partial class SongRequestsDashboardControl : DashboardControlBase
    {
        private SongRequestsMainControlViewModel viewModel;

        public SongRequestsDashboardControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new SongRequestsMainControlViewModel(this.Window.ViewModel);
            await this.viewModel.OnLoaded();
        }
    }
}
