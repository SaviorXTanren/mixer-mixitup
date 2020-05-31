using MixItUp.Base.ViewModel.Controls.MainControls;
using MixItUp.Base.ViewModel.Window;
using MixItUp.WPF.Windows.Currency;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for StreamPassControl.xaml
    /// </summary>
    public partial class StreamPassControl : MainControlBase
    {
        private StreamPassMainControlViewModel viewModel;

        public StreamPassControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new StreamPassMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);
            await this.viewModel.OnLoaded();
            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.viewModel.OnVisible();
        }

        private void EditButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void AddStreamPassButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            StreamPassWindow window = new StreamPassWindow();
            window.Closed += Window_Closed;
            window.Show();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            this.viewModel.Refresh();
        }
    }
}
