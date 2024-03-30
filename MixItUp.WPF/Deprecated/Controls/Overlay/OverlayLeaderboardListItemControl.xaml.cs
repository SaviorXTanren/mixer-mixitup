using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.Overlay;
using MixItUp.WPF.Controls.Commands;
using MixItUp.WPF.Windows.Commands;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayLeaderboardListItemControl.xaml
    /// </summary>
    [Obsolete]
    public partial class OverlayLeaderboardListItemControl : OverlayItemControl
    {
        public OverlayLeaderboardListItemControl()
        {
            InitializeComponent();
        }

        public OverlayLeaderboardListItemControl(OverlayLeaderboardListItemViewModel viewModel)
            : this()
        {
            this.ViewModel = viewModel;
        }

        protected override async Task OnLoaded()
        {
            this.TextFontComboBox.ItemsSource = ServiceManager.Get<IFileService>().GetInstalledFonts();

            await base.OnLoaded();
        }

        private void AddCommandButton_Click(object sender, RoutedEventArgs e)
        {
            CommandEditorWindow window = new CommandEditorWindow(CommandTypeEnum.Custom, MixItUp.Base.Resources.OverlayLeaderboardNewLeader);
            window.CommandSaved += Window_CommandSaved;
            window.ForceShow();
        }

        private void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CustomCommandModel command = ((CommandListingButtonsControl)sender).GetCommandFromCommandButtons<CustomCommandModel>();
            if (command != null)
            {
                CommandEditorWindow window = CommandEditorWindow.GetCommandEditorWindow(command);
                window.CommandSaved += Window_CommandSaved;
                window.ForceShow();
            }
        }

        private void CommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            CustomCommandModel command = ((CommandListingButtonsControl)sender).GetCommandFromCommandButtons<CustomCommandModel>();
            if (command != null)
            {
                ((OverlayLeaderboardListItemViewModel)this.ViewModel).NewLeaderCommand = null;
            }
        }

        private void Window_CommandSaved(object sender, CommandModelBase command)
        {
            ((OverlayLeaderboardListItemViewModel)this.ViewModel).NewLeaderCommand = (CustomCommandModel)command;
        }
    }
}
