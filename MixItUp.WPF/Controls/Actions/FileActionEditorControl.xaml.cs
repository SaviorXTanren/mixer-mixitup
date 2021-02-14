using MixItUp.Base;
using MixItUp.Base.ViewModel.Actions;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for FileActionEditorControl.xaml
    /// </summary>
    public partial class FileActionEditorControl : ActionEditorControlBase
    {
        public FileActionEditorControl()
        {
            InitializeComponent();
        }

        private void FileBrowseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.DataContext is FileActionEditorControlViewModel)
            {
                string filePath = null;
                if (((FileActionEditorControlViewModel)this.DataContext).ShowSaveToFileGrid)
                {
                    filePath = ChannelSession.Services.FileService.ShowSaveFileDialog("");
                }
                else if (((FileActionEditorControlViewModel)this.DataContext).ShowReadFromFileGrid)
                {
                    filePath = ChannelSession.Services.FileService.ShowOpenFileDialog();
                }

                if (!string.IsNullOrEmpty(filePath))
                {
                    ((FileActionEditorControlViewModel)this.DataContext).FilePath = filePath;
                }
            }
        }
    }
}
