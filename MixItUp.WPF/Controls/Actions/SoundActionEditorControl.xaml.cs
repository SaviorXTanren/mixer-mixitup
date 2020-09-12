using MixItUp.Base;
using MixItUp.Base.ViewModel.Controls.Actions;

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
            string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog(ChannelSession.Services.FileService.MusicFileFilter());
            if (this.DataContext is SoundActionEditorControlViewModel)
            {
                ((SoundActionEditorControlViewModel)this.DataContext).FilePath = filePath;
            }
        }
    }
}
