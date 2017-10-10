using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Dialogs
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
