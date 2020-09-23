using System.Windows;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for OverlayActionEditorControl.xaml
    /// </summary>
    public partial class OverlayActionEditorControl : ActionEditorControlBase
    {
        public OverlayActionEditorControl()
        {
            InitializeComponent();

            this.Loaded += OverlayActionEditorControl_Loaded;
        }

        private void OverlayActionEditorControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.AnimationsMayNotWork.Visibility = System.Windows.SystemParameters.ClientAreaAnimation ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
