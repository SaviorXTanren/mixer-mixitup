using MixItUp.Base.Model.Commands;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Commands;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Commands
{
    /// <summary>
    /// Interaction logic for CustomCommandListingControl.xaml
    /// </summary>
    public partial class CustomCommandListingControl : UserControl
    {
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(CustomCommandListingControl), new PropertyMetadata(string.Empty));

        public CustomCommandListingControl()
        {
            InitializeComponent();

            this.Loaded += CustomCommandListingControl_Loaded;
        }

        private void CustomCommandListingControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.GroupBox.Header = this.Header;
        }

        private void CommandControl_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(FrameworkElementHelpers.GetDataContext<CustomCommandModel>(sender));
            window.ForceShow();
        }
    }
}
