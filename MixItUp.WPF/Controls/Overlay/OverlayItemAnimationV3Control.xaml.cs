using MixItUp.Base.Util;
using System.Windows;
using System.Windows.Navigation;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayItemAnimationV3Control.xaml
    /// </summary>
    public partial class OverlayItemAnimationV3Control : LoadingControlBase
    {
        public OverlayItemAnimationV3Control()
        {
            InitializeComponent();

            this.Loaded += OverlayItemAnimationV3Control_Loaded;
        }

        private void OverlayItemAnimationV3Control_Loaded(object sender, RoutedEventArgs e)
        {
            this.AnimationsMayNotWork1.Visibility = this.AnimationsMayNotWork2.Visibility = this.AnimationsMayNotWork3.Visibility =
                SystemParameters.ClientAreaAnimation ? Visibility.Collapsed : Visibility.Visible;
        }

        protected void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            ProcessHelper.LaunchLink(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}
