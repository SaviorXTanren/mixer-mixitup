using MixItUp.Base.Model.Currency;
using MixItUp.Base.ViewModel.MainControls;
using MixItUp.Base.ViewModel;
using MixItUp.WPF.Windows.Currency;
using System.Threading.Tasks;
using System.Windows.Controls;

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
            await this.viewModel.OnOpen();
            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.viewModel.OnVisible();
        }

        private void EditButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            StreamPassModel seasonPass = (StreamPassModel)button.DataContext;
            StreamPassWindow window = new StreamPassWindow(seasonPass);
            window.Closed += Window_Closed;
            window.Show();
        }

        private async void CopyButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            StreamPassModel streamPass = (StreamPassModel)button.DataContext;
            await this.viewModel.Copy(streamPass);
        }

        private async void DeleteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            StreamPassModel streamPass = (StreamPassModel)button.DataContext;
            await this.viewModel.Delete(streamPass);
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
