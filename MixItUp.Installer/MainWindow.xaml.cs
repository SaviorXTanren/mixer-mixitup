using System.Windows;

namespace MixItUp.Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this.viewModel = new MainWindowViewModel();

            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.viewModel.CheckCompatability())
            {
                if (await this.viewModel.Run())
                {
                    this.viewModel.Launch();
                    this.Close();
                }
            }
        }
    }
}
