using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.Actions;
using System.Collections.Generic;

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
            IEnumerable<string> filePaths = ServiceManager.Get<IFileService>().ShowMultiselectOpenFileDialog(ServiceManager.Get<IFileService>().SoundFileFilter());
            if (this.DataContext is SoundActionEditorControlViewModel)
            {
                if (filePaths != null)
                {
                    ((SoundActionEditorControlViewModel)this.DataContext).FilePath = string.Join("|", filePaths);
                }
            }
        }
    }
}
