using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Dialogs.CommunityCommands
{
    /// <summary>
    /// Interaction logic for CommunityCommandsReportCommandDialogControl.xaml
    /// </summary>
    public partial class CommunityCommandsReportCommandDialogControl : UserControl
    {
        public string Report { get { return this.ReportTextBox.Text; } }

        public CommunityCommandsReportCommandDialogControl()
        {
            InitializeComponent();
        }
    }
}
