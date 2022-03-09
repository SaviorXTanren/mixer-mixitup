using MixItUp.Base.ViewModel.Dialogs;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for AddUserDialogControl.xaml
    /// </summary>
    public partial class AddUserDialogControl : UserControl
    {
        public AddUserDialogControlViewModel ViewModel { get; private set; }

        public AddUserDialogControl()
        {
            InitializeComponent();

            this.DataContext = this.ViewModel = new AddUserDialogControlViewModel();
        }
    }
}
