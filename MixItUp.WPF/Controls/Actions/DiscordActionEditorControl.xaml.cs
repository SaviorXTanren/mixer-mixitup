using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.Actions;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for DiscordActionEditorControl.xaml
    /// </summary>
    public partial class DiscordActionEditorControl : ActionEditorControlBase
    {
        public DiscordActionEditorControl()
        {
            InitializeComponent();
        }

        private void FilePathBrowseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string filePath = ServiceManager.Get<IFileService>().ShowOpenFileDialog();
            if (!string.IsNullOrEmpty(filePath) && this.DataContext is DiscordActionEditorControlViewModel)
            {
                ((DiscordActionEditorControlViewModel)this.DataContext).UploadFilePath = filePath;
            }
        }
    }
}
