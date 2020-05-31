using MixItUp.Base.Model.Currency;
using MixItUp.Base.ViewModel.Window.Currency;
using System.Threading.Tasks;

namespace MixItUp.WPF.Windows.Currency
{
    /// <summary>
    /// Interaction logic for StreamPassWindow.xaml
    /// </summary>
    public partial class StreamPassWindow : LoadingWindowBase
    {
        private StreamPassWindowViewModel viewModel;

        public StreamPassWindow()
        {
            this.viewModel = new StreamPassWindowViewModel();

            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        public StreamPassWindow(StreamPassModel seasonPass)
        {
            this.viewModel = new StreamPassWindowViewModel(seasonPass);

            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            this.DataContext = this.viewModel;
            await this.viewModel.OnLoaded();
            await base.OnLoaded();
        }

        private void StartDateButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void EndDateButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void LevelCommandButtons_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void DeleteLevelButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void DefaultLevelUpNewCommandButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void DefaultLevelUpCommandButtons_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void DefaultLevelUpCommandButtons_DeleteClicked(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
