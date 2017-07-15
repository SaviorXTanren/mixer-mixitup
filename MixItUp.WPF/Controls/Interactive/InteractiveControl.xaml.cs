using MixItUp.Base.Actions;
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

            this.Loaded += InteractiveControl_Loaded;
        }

        private void InteractiveControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
