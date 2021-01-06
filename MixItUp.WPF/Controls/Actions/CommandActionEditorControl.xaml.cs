using MixItUp.Base.ViewModel.Actions;
using MixItUp.WPF.Windows.Commands;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for CommandActionEditorControl.xaml
    /// </summary>
    public partial class CommandActionEditorControl : ActionEditorControlBase
    {
        public CommandActionEditorControl()
        {
            InitializeComponent();
        }

        private void EditCommandButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CommandActionEditorControlViewModel viewModel = (CommandActionEditorControlViewModel)this.DataContext;
            if (viewModel != null && viewModel.SelectedCommand != null)
            {
                CommandEditorWindow window = new CommandEditorWindow(viewModel.SelectedCommand);
                window.Show();
            }
        }
    }
}
