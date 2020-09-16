using MixItUp.Base;
using MixItUp.Base.ViewModel.Controls.Actions;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for StreamingSoftwareActionEditorControl.xaml
    /// </summary>
    public partial class StreamingSoftwareActionEditorControl : ActionEditorControlBase
    {
        public StreamingSoftwareActionEditorControl()
        {
            InitializeComponent();
        }

        private void SourceLoadTextFromBrowseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.DataContext is StreamingSoftwareActionEditorControlViewModel)
            {
                string filePath = ChannelSession.Services.FileService.ShowSaveFileDialog(((StreamingSoftwareActionEditorControlViewModel)this.DataContext).SourceTextFilePath);
                if (!string.IsNullOrEmpty(filePath))
                {
                    ((StreamingSoftwareActionEditorControlViewModel)this.DataContext).SourceTextFilePath = filePath;
                }
            }
        }

        private void SourceWebPageBrowseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.DataContext is StreamingSoftwareActionEditorControlViewModel)
            {
                string filePath = ChannelSession.Services.FileService.ShowSaveFileDialog(((StreamingSoftwareActionEditorControlViewModel)this.DataContext).SourceWebPageFilePath);
                if (!string.IsNullOrEmpty(filePath))
                {
                    ((StreamingSoftwareActionEditorControlViewModel)this.DataContext).SourceWebPageFilePath = filePath;
                }
            }
        }
    }
}
