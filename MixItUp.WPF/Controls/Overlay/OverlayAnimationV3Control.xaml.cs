using MixItUp.Base.Util;
using System.Windows;
using System.Windows.Navigation;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayAnimationV3Control.xaml
    /// </summary>
    public partial class OverlayAnimationV3Control : LoadingControlBase
    {
        public OverlayAnimationV3Control()
        {
            InitializeComponent();

            this.Loaded += OverlayItemAnimationV3Control_Loaded;
        }

        private void OverlayItemAnimationV3Control_Loaded(object sender, RoutedEventArgs e)
        {
            this.AnimationsMayNotWork.Visibility = SystemParameters.ClientAreaAnimation ? Visibility.Collapsed : Visibility.Visible;
        }

        protected void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            ProcessHelper.LaunchLink(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}
