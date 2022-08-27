using MixItUp.Base.Util;
using System.Windows;
using System.Windows.Navigation;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayItemContainerV3Control.xaml
    /// </summary>
    public partial class OverlayItemContainerV3Control : LoadingControlBase
    {
        public static readonly DependencyProperty ContentControlProperty =
            DependencyProperty.Register(
            "ContentControl",
            typeof(object),
            typeof(OverlayItemContainerV3Control),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(OnContentControlChanged)
            ));

        public OverlayItemContainerV3Control()
        {
            InitializeComponent();
        }

        public object ContentControl
        {
            get { return (object)GetValue(ContentControlProperty); }
            set { SetValue(ContentControlProperty, value); }
        }

        protected void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            ProcessHelper.LaunchLink(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private static void OnContentControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OverlayItemContainerV3Control control = (OverlayItemContainerV3Control)d;
            if (control != null)
            {
                control.InnerContent.Content = control.ContentControl;
            }
        }
    }
}
