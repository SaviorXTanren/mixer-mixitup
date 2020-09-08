using MixItUp.Base;
using MixItUp.Base.ViewModel.Controls.Actions;

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
            string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog();
            if (!string.IsNullOrEmpty(filePath) && this.DataContext is DiscordActionEditorControlViewModel)
            {
                ((DiscordActionEditorControlViewModel)this.DataContext).UploadFilePath = filePath;
            }
        }
    }
}
