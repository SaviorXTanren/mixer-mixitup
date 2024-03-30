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
    /// Interaction logic for OverlayTimerItemControl.xaml
    /// </summary>
    [Obsolete]
    public partial class OverlayTimerItemControl : OverlayItemControl
    {
        public OverlayTimerItemControl()
        {
            InitializeComponent();
        }

        public OverlayTimerItemControl(OverlayTimerItemViewModel viewModel)
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
            CommandEditorWindow window = new CommandEditorWindow(CommandTypeEnum.Custom, MixItUp.Base.Resources.TimerTimerComplete);
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
                ((OverlayTimerItemViewModel)this.ViewModel).TimerCompleteCommand = null;
            }
        }

        private void Window_CommandSaved(object sender, CommandModelBase command)
        {
            ((OverlayTimerItemViewModel)this.ViewModel).TimerCompleteCommand = (CustomCommandModel)command;
        }
    }
}
