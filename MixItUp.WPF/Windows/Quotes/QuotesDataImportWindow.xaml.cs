using MixItUp.Base.ViewModel.Quotes;
using System.Threading.Tasks;

namespace MixItUp.WPF.Windows.Quotes
{
    /// <summary>
    /// Interaction logic for QuotesDataImportWindow.xaml
    /// </summary>
    public partial class QuotesDataImportWindow : LoadingWindowBase
    {
        private QuotesDataImportWindowViewModel viewModel;

        public QuotesDataImportWindow()
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override Task OnLoaded()
        {
            this.DataContext = this.viewModel = new QuotesDataImportWindowViewModel();
            return Task.CompletedTask;
        }
    }
}
