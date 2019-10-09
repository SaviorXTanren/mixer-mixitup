using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Dashboard
{
    /// <summary>
    /// Interaction logic for ChatDashboardControl.xaml
    /// </summary>
    public partial class ChatDashboardControl : DashboardControlBase
    {
        public ChatDashboardControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            await base.InitializeInternal();

            await this.ChatList.Initialize(this.Window);
        }
    }
}
