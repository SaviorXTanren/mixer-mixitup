using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for StreamAvatarsServiceControl.xaml
    /// </summary>
    public partial class StreamAvatarsServiceControl : ServicesControlBase
    {
        public StreamAvatarsServiceControl()
        {
            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            this.SetHeaderText("Stream Avatars");
            await base.OnLoaded();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }
    }
}
