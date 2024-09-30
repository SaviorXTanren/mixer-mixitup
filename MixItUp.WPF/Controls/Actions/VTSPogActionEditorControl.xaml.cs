using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.Actions;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for VTSPogActionEditorControl.xaml
    /// </summary>
    public partial class VTSPogActionEditorControl : ActionEditorControlBase
    {
        public VTSPogActionEditorControl()
        {
            InitializeComponent();
        }

        private void FileBrowseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.DataContext is VTSPogActionEditorControlViewModel)
            {
                string filePath = ServiceManager.Get<IFileService>().ShowOpenFileDialog();
                if (!string.IsNullOrEmpty(filePath))
                {
                    ((VTSPogActionEditorControlViewModel)this.DataContext).AudioFilePath = filePath;
                }
            }
        }
    }
}
