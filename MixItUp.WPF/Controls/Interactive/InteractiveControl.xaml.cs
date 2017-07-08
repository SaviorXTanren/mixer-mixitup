using MixItUp.Base.Actions;
using MixItUp.Base.Overlay;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Interactive
{
    /// <summary>
    /// Interaction logic for InteractiveControl.xaml
    /// </summary>
    public partial class InteractiveControl : UserControl
    {
        private OverlayWebServer server;

        public InteractiveControl()
        {
            InitializeComponent();

            this.server = new OverlayWebServer();
            this.server.Start();

            OverlayAction action = new OverlayAction();
            action.FilePath = "th.jpg";
            action.Duration = 3;
            action.Horizontal = 70;
            action.Vertical = 70;

            action.Perform(null).Wait();
        }
    }
}
