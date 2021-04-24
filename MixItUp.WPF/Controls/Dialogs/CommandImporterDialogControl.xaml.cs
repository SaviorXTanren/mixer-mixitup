using MixItUp.Base.ViewModel.Dialogs;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for CommandImporterDialogControl.xaml
    /// </summary>
    public partial class CommandImporterDialogControl : UserControl
    {
        public CommandImporterDialogControlViewModel ViewModel { get; private set; }

        public CommandImporterDialogControl()
        {
            InitializeComponent();

            this.DataContext = this.ViewModel = new CommandImporterDialogControlViewModel();
        }

        private void CreateNewCommandTextBlock_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) { this.ViewModel.IsNewCommandSelected = true; }

        private void AddToExistingCommandTextBlock_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) { this.ViewModel.IsExistingCommandSelected = true; }
    }
}
