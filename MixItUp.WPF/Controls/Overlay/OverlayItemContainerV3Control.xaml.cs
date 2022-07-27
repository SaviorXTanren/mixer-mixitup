using MixItUp.Base.Util;
using System.Windows.Navigation;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayItemContainerV3Control.xaml
    /// </summary>
    public partial class OverlayItemContainerV3Control : LoadingControlBase
    {
        public OverlayItemContainerV3Control()
        {
            InitializeComponent();
        }

        protected void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            ProcessHelper.LaunchLink(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}
