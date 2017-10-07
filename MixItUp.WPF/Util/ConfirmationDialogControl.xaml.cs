using System.Windows.Controls;

namespace MixItUp.WPF.Util
{
    /// <summary>
    /// Interaction logic for ConfirmationDialogControl.xaml
    /// </summary>
    public partial class ConfirmationDialogControl : UserControl
    {
        public ConfirmationDialogControl(string message)
        {
            this.DataContext = message;

            InitializeComponent();
        }
    }
}
