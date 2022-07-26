using MixItUp.Base.Util;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayItemContainerV3Control.xaml
    /// </summary>
    public partial class OverlayItemContainerV3Control : UserControl
    {
        public static readonly DependencyProperty InnerContentProperty =
            DependencyProperty.Register(
            "InnerContent",
            typeof(UserControl),
            typeof(OverlayItemContainerV3Control),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(OnInnerContentChanged)
            ));

        public OverlayItemContainerV3Control()
        {
            InitializeComponent();

            this.Loaded += OverlayActionEditorControl_Loaded;
        }

        public UserControl InnerContent
        {
            get { return (UserControl)GetValue(InnerContentProperty); }
            set { SetValue(InnerContentProperty, value); }
        }

        private static void OnInnerContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OverlayItemContainerV3Control control = (OverlayItemContainerV3Control)d;
            if (control != null && control.InnerContent != null)
            {
                control.ItemContentControl.Content = control.InnerContent;
            }
        }

        private void OverlayActionEditorControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.AnimationsMayNotWork.Visibility = System.Windows.SystemParameters.ClientAreaAnimation ? Visibility.Collapsed : Visibility.Visible;
        }

        protected void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            ProcessHelper.LaunchLink(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}
