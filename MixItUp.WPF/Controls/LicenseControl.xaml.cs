using MixItUp.Base.Util;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MixItUp.WPF.Controls
{
    /// <summary>
    /// Interaction logic for LicenseControl.xaml
    /// </summary>
    public partial class LicenseControl : UserControl
    {
        public LicenseControl()
        {
            InitializeComponent();
        }

        public void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            ProcessHelper.LaunchLink(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}
