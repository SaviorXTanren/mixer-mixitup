using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.Overlay;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Commands;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayWheelV3Control.xaml
    /// </summary>
    public partial class OverlayWheelV3Control : LoadingControlBase
    {
        public OverlayWheelV3Control()
        {
            InitializeComponent();
        }

        private void CommandListingButtonsControl_EditClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            OverlayWheelOutcomeV3ViewModel outcome = FrameworkElementHelpers.GetDataContext<OverlayWheelOutcomeV3ViewModel>(sender);
            if (outcome.Command != null)
            {
                CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(outcome.Command);
                window.CommandSaved += (object s, CommandModelBase command) => { outcome.Command = (CustomCommandModel)command; };
                window.ForceShow();
            }
        }
    }
}
