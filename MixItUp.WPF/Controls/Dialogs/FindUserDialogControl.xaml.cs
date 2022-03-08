using MixItUp.Base.ViewModel.Dialogs;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for FindUserDialogControl.xaml
    /// </summary>
    public partial class FindUserDialogControl : UserControl
    {
        public FindUserDialogControlViewModel ViewModel { get; private set; }

        public FindUserDialogControl()
        {
            InitializeComponent();

            this.DataContext = this.ViewModel = new FindUserDialogControlViewModel();
        }
    }
}
