using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.Actions;

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
                string filePath = ServiceManager.Get<IFileService>().ShowSaveFileDialog(((StreamingSoftwareActionEditorControlViewModel)this.DataContext).SourceTextFilePath, MixItUp.Base.Resources.TextFileFormatFilter);
                if (!string.IsNullOrEmpty(filePath))
                {
                    ((StreamingSoftwareActionEditorControlViewModel)this.DataContext).SourceTextFilePath = filePath;
                }
            }
        }

        private void SourceImageBrowseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.DataContext is StreamingSoftwareActionEditorControlViewModel)
            {
                string filePath = ServiceManager.Get<IFileService>().ShowOpenFileDialog(MixItUp.Base.Resources.ImageFileFormatFilter);
                if (!string.IsNullOrEmpty(filePath))
                {
                    ((StreamingSoftwareActionEditorControlViewModel)this.DataContext).SourceImageFilePath = filePath;
                }
            }
        }

        private void SourceVideoBrowseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.DataContext is StreamingSoftwareActionEditorControlViewModel)
            {
                string filePath = ServiceManager.Get<IFileService>().ShowOpenFileDialog(MixItUp.Base.Resources.VideoFileFormatFilter);
                if (!string.IsNullOrEmpty(filePath))
                {
                    ((StreamingSoftwareActionEditorControlViewModel)this.DataContext).SourceMediaFilePath = filePath;
                }
            }
        }

        private void SourceWebPageBrowseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.DataContext is StreamingSoftwareActionEditorControlViewModel)
            {
                string filePath = ServiceManager.Get<IFileService>().ShowOpenFileDialog(MixItUp.Base.Resources.HTMLFileFormatFilter);
                if (!string.IsNullOrEmpty(filePath))
                {
                    ((StreamingSoftwareActionEditorControlViewModel)this.DataContext).SourceWebPageFilePath = filePath;
                }
            }
        }
    }
}
