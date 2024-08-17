using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.Overlay;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Commands;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayGoalV3Control.xaml
    /// </summary>
    public partial class OverlayGoalV3Control : LoadingControlBase
    {
        public OverlayGoalV3Control()
        {
            InitializeComponent();
        }

        private void CommandListingButtonsControl_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            OverlayGoalSegmentV3ViewModel outcome = FrameworkElementHelpers.GetDataContext<OverlayGoalSegmentV3ViewModel>(sender);
            if (outcome.Command != null)
            {
                CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(outcome.Command);
                window.CommandSaved += (object s, CommandModelBase command) => { outcome.Command = (CustomCommandModel)command; };
                window.ForceShow();
            }
        }
    }
}
