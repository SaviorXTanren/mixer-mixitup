using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for RemoteControl.xaml
    /// </summary>
    public partial class RemoteControl : MainControlBase
    {
        public RemoteControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            return base.InitializeInternal();
        }

        private void ProfilesListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}
