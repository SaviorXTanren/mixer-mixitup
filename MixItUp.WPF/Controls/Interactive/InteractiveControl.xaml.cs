using MixItUp.WPF.Overlay;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Interactive
{
    /// <summary>
    /// Interaction logic for InteractiveControl.xaml
    /// </summary>
    public partial class InteractiveControl : UserControl
    {
        public InteractiveControl()
        {
            InitializeComponent();
        }

        private void Test_Click(object click, RoutedEventArgs eEvents)
        {
            OverlayWebServer server = new OverlayWebServer();
            server.Start();

            server.SetOverlayImage("C:\\Users\\Matthew\\Downloads\\friday-the-13-1024x576.jpg", 3);
        }
    }
}
