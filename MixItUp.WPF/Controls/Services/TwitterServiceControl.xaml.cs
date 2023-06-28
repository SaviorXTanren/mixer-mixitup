using MixItUp.Base;
using MixItUp.Base.ViewModel.Services;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for TwitterServiceControl.xaml
    /// </summary>
    public partial class TwitterServiceControl : ServiceControlBase
    {
        private TwitterServiceControlViewModel viewModel;

        public TwitterServiceControl()
        {
            this.DataContext = this.ViewModel = this.viewModel = new TwitterServiceControlViewModel();

            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            await this.viewModel.OnOpen();
        }
    }

    public class TwitterServiceControlViewModel : ServiceControlViewModelBase
    {
        public override string WikiPageName { get { return "twitter"; } }

        public TwitterServiceControlViewModel() : base(Resources.Twitter) { }
    }
}