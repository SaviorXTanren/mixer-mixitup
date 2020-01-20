using MixItUp.Base.ViewModel.Window.Wizard;
using System.ComponentModel;
using System.Threading.Tasks;

namespace MixItUp.WPF.Windows.Wizard
{
    /// <summary>
    /// Interaction logic for NewUserWizardWindow.xaml
    /// </summary>
    public partial class NewUserWizardWindow : LoadingWindowBase
    {
        private NewUserWizardWindowViewModel viewModel;

        public NewUserWizardWindow()
            : base(new NewUserWizardWindowViewModel())
        {
            this.viewModel = (NewUserWizardWindowViewModel)this.ViewModel;
            this.viewModel.WizardCompleteEvent += ViewModel_WizardCompleteEvent;

            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnLoaded();

            await base.OnLoaded();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (this.viewModel.WizardComplete)
            {
                this.ShowMainWindow(new MainWindow());
            }
            base.OnClosing(e);
        }

        private void ViewModel_WizardCompleteEvent(object sender, System.EventArgs e)
        {
            this.Close();
        }
    }
}
