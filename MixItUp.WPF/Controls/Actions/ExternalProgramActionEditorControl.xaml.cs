using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.Actions;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for ExternalProgramActionEditorControl.xaml
    /// </summary>
    public partial class ExternalProgramActionEditorControl : ActionEditorControlBase
    {
        public ExternalProgramActionEditorControl()
        {
            InitializeComponent();
        }

        private void ProgramFileBrowseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string filePath = ServiceManager.Get<IFileService>().ShowOpenFileDialog();
            if (!string.IsNullOrEmpty(filePath) && this.DataContext is ExternalProgramActionEditorControlViewModel)
            {
                ((ExternalProgramActionEditorControlViewModel)this.DataContext).FilePath = filePath;
            }
        }
    }
}
