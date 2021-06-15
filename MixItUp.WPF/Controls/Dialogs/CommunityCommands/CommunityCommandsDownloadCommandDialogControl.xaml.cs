using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Commands;
using MixItUp.Base.ViewModel.CommunityCommands;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Dialogs.CommunityCommands
{
    /// <summary>
    /// Interaction logic for CommunityCommandsDownloadCommandDialogControl.xaml
    /// </summary>
    public partial class CommunityCommandsDownloadCommandDialogControl : UserControl
    {
        private CommunityCommandDetailsViewModel command;

        public CommunityCommandsDownloadCommandDialogControl(CommunityCommandDetailsViewModel command)
        {
            InitializeComponent();

            this.command = command;
        }

        private async void DownloadToFileButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (await CommandEditorWindowViewModelBase.ExportCommandToFile(command.Command))
            {
                DialogHelper.CloseCurrent();
            }
        }
    }
}
