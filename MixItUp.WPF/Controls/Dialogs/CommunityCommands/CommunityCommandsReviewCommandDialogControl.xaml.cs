using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Dialogs.CommunityCommands
{
    /// <summary>
    /// Interaction logic for CommunityCommandsReviewCommandDialogControl.xaml
    /// </summary>
    public partial class CommunityCommandsReviewCommandDialogControl : UserControl
    {
        public int Rating { get { return this.RatingsBar.Value; } }

        public string Review { get { return this.TextEntryTextBox.Text; } }

        public CommunityCommandsReviewCommandDialogControl()
        {
            InitializeComponent();
        }
    }
}
