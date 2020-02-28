using Mixer.Base.Model.MixPlay;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Model.MixPlay;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Controls.MainControls;
using MixItUp.Base.ViewModel.MixPlay;
using MixItUp.Base.ViewModel.Window;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Controls.Dialogs;
using MixItUp.WPF.Controls.Interactive;
using MixItUp.WPF.Windows.Command;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for MixPlayControl.xaml
    /// </summary>
    public partial class MixPlayControl : MainControlBase
    {
        private MixPlayMainControlViewModel viewModel;

        public MixPlayControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.DataContext = this.viewModel = new MixPlayMainControlViewModel((MainWindowViewModel)this.Window.ViewModel);
            this.viewModel.CustomMixPlaySelected += ViewModel_CustomMixPlaySelected;
            await this.viewModel.OnLoaded();

            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.viewModel.OnVisible();

            await base.OnVisibilityChanged();
        }

        private void ViewModel_CustomMixPlaySelected(object sender, System.EventArgs e)
        {
            if (this.viewModel.SelectedGame.id == MixPlaySharedProjectModel.FortniteDropMap.GameID)
            {
                this.viewModel.CustomMixPlayControl = new DropMapInteractiveControl(DropMapTypeEnum.Fortnite, this.viewModel.SelectedGame, this.viewModel.SelectedGameVersion);
            }
            else if (this.viewModel.SelectedGame.id == MixPlaySharedProjectModel.PUBGDropMap.GameID)
            {
                this.viewModel.CustomMixPlayControl = new DropMapInteractiveControl(DropMapTypeEnum.PUBG, this.viewModel.SelectedGame, this.viewModel.SelectedGameVersion);
            }
            else if (this.viewModel.SelectedGame.id == MixPlaySharedProjectModel.RealmRoyaleDropMap.GameID)
            {
                this.viewModel.CustomMixPlayControl = new DropMapInteractiveControl(DropMapTypeEnum.RealmRoyale, this.viewModel.SelectedGame, this.viewModel.SelectedGameVersion);
            }
            else if (this.viewModel.SelectedGame.id == MixPlaySharedProjectModel.BlackOps4DropMap.GameID)
            {
                this.viewModel.CustomMixPlayControl = new DropMapInteractiveControl(DropMapTypeEnum.BlackOps4, this.viewModel.SelectedGame, this.viewModel.SelectedGameVersion);
            }
            else if (this.viewModel.SelectedGame.id == MixPlaySharedProjectModel.ApexLegendsDropMap.GameID)
            {
                this.viewModel.CustomMixPlayControl = new DropMapInteractiveControl(DropMapTypeEnum.ApexLegends, this.viewModel.SelectedGame, this.viewModel.SelectedGameVersion);
            }
            else if (this.viewModel.SelectedGame.id == MixPlaySharedProjectModel.SuperAnimalRoyaleDropMap.GameID)
            {
                this.viewModel.CustomMixPlayControl = new DropMapInteractiveControl(DropMapTypeEnum.SuperAnimalRoyale, this.viewModel.SelectedGame, this.viewModel.SelectedGameVersion);
            }
            else if (this.viewModel.SelectedGame.id == MixPlaySharedProjectModel.MixerPaint.GameID)
            {
                this.viewModel.CustomMixPlayControl = new MixerPaintInteractiveControl(this.viewModel.SelectedGame, this.viewModel.SelectedGameVersion);
            }
            else if (this.viewModel.SelectedGame.id == MixPlaySharedProjectModel.FlySwatter.GameID)
            {
                this.viewModel.CustomMixPlayControl = new FlySwatterInteractiveControl(this.viewModel.SelectedGame, this.viewModel.SelectedGameVersion);
            }
        }

        private async void GroupsButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.viewModel.SelectedGame != null && this.viewModel.SelectedScene != null)
            {
                InteractiveSceneUserGroupsDialogControl dialogControl = new InteractiveSceneUserGroupsDialogControl(this.viewModel.SelectedGame, this.viewModel.SelectedScene);
                await this.Window.RunAsyncOperation(async () =>
                {
                    await DialogHelper.ShowCustom(dialogControl);
                });
            }
        }

        private async void CommandButtons_EditClicked(object sender, RoutedEventArgs e)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            MixPlayControlViewModel command = (MixPlayControlViewModel)commandButtonsControl.DataContext;
            if (command != null)
            {
                string commandType = string.Empty;
                if (command.Command is MixPlayButtonCommand) { commandType = "Button"; }
                if (command.Command is MixPlayJoystickCommand) { commandType = "Joystick"; }
                if (command.Command is MixPlayTextBoxCommand) { commandType = "Text Box"; }

                string controlType = string.Empty;
                if (command.Control is MixPlayButtonControlModel) { controlType = "Button"; }
                if (command.Control is MixPlayJoystickControlModel) { controlType = "Joystick"; }
                if (command.Control is MixPlayTextBoxControlModel) { controlType = "Text Box"; }

                if (!commandType.Equals(controlType))
                {
                    await DialogHelper.ShowMessage(string.Format("The control you are trying to edit has been changed from a {0} to a {1} and can not be edited. You must either change this control back to its previous type, change the control ID to something different, or delete the command to start fresh.", commandType, controlType));
                    return;
                }

                CommandWindow window = null;
                if (command.Command is MixPlayButtonCommand)
                {
                    window = new CommandWindow(new MixPlayButtonCommandDetailsControl(this.viewModel.SelectedGame, this.viewModel.SelectedGameVersion, command));
                }
                else if (command.Command is MixPlayJoystickCommand)
                {
                    window = new CommandWindow(new MixPlayJoystickCommandDetailsControl(this.viewModel.SelectedGame, this.viewModel.SelectedGameVersion, command));
                }
                else if (command.Command is MixPlayTextBoxCommand)
                {
                    window = new CommandWindow(new MixPlayTextBoxCommandDetailsControl(this.viewModel.SelectedGame, this.viewModel.SelectedGameVersion, command));
                }

                if (window != null)
                {
                    window.CommandSaveSuccessfully += Window_CommandSaveSuccessfully;
                    window.Show();
                }
            }
        }

        private async void CommandButtons_DeleteClicked(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
                MixPlayControlViewModel command = (MixPlayControlViewModel)commandButtonsControl.DataContext;
                if (command != null)
                {
                    ChannelSession.Settings.MixPlayCommands.Remove(command.Command);
                    await ChannelSession.SaveSettings();
                    this.viewModel.RefreshControls();
                }
            });
        }

        private void AddCommandButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            MixPlayControlViewModel command = (MixPlayControlViewModel)button.DataContext;

            CommandWindow window = null;
            if (command.Control is MixPlayButtonControlModel)
            {
                window = new CommandWindow(new MixPlayButtonCommandDetailsControl(this.viewModel.SelectedGame, this.viewModel.SelectedGameVersion, command.Button));
            }
            else if (command.Control is MixPlayJoystickControlModel)
            {
                window = new CommandWindow(new MixPlayJoystickCommandDetailsControl(this.viewModel.SelectedGame, this.viewModel.SelectedGameVersion, command.Joystick));
            }
            else if (command.Control is MixPlayTextBoxControlModel)
            {
                window = new CommandWindow(new MixPlayTextBoxCommandDetailsControl(this.viewModel.SelectedGame, this.viewModel.SelectedGameVersion, command.TextBox));
            }

            if (window != null)
            {
                window.CommandSaveSuccessfully += Window_CommandSaveSuccessfully;
                window.Show();
            }
        }

        private void Window_CommandSaveSuccessfully(object sender, CommandBase command)
        {
            this.viewModel.RefreshControls();
        }
    }
}
