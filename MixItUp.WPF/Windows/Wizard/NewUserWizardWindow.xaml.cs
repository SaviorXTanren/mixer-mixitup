using System.Threading.Tasks;

namespace MixItUp.WPF.Windows.Wizard
{
    /// <summary>
    /// Interaction logic for NewUserWizardWindow.xaml
    /// </summary>
    public partial class NewUserWizardWindow : LoadingWindowBase
    {
        public NewUserWizardWindow()
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override Task OnLoaded()
        {
            return base.OnLoaded();
        }
    }
}
