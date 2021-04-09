using MixItUp.Base.ViewModel;
using MixItUp.Base.ViewModel.MainControls;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for CommandHistoryControl.xaml
    /// </summary>
    public partial class CommandHistoryControl : MainControlBase
    {
        private CommandHistoryMainControlViewModel viewModel;

        public CommandHistoryControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new CommandHistoryMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);
            return Task.FromResult(0);
        }
    }
}
