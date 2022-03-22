using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.Actions;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for SoundActionEditorControl.xaml
    /// </summary>
    public partial class SoundActionEditorControl : ActionEditorControlBase
    {
        public SoundActionEditorControl()
        {
            InitializeComponent();
        }

        private void SoundFileBrowseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string filePath = ServiceManager.Get<IFileService>().ShowOpenFileDialog(ServiceManager.Get<IFileService>().SoundFileFilter());
            if (this.DataContext is SoundActionEditorControlViewModel)
            {
                ((SoundActionEditorControlViewModel)this.DataContext).FilePath = filePath;
            }
        }
    }
}
