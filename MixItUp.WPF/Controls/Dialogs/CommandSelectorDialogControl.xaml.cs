using MixItUp.Base.ViewModel.Dialogs;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for CommandSelectorDialogControl.xaml
    /// </summary>
    public partial class CommandSelectorDialogControl : UserControl
    {
        public CommandSelectorDialogControlViewModel ViewModel { get; private set; }

        public CommandSelectorDialogControl()
        {
            InitializeComponent();

            this.DataContext = this.ViewModel = new CommandSelectorDialogControlViewModel();
        }
    }
}
