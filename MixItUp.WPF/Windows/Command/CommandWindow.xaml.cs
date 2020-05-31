using Mixer.Base.Model.MixPlay;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Controls.Store;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Windows.Command
{
    /// <summary>
    /// Interaction logic for CommandWindow.xaml
    /// </summary>
    public partial class CommandWindow : LoadingWindowBase
    {
        public event EventHandler<CommandBase> CommandSaveSuccessfully;

        private CommandEditorControlBase commandEditorControl;

        private CommandDetailsControlBase commandDetailsControl;

        public CommandWindow(CommandDetailsControlBase commandDetailsControl)
            : this()
        {
            this.commandDetailsControl = commandDetailsControl;
        }

        private CommandWindow()
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        public void DownloadCommandFromStore(Guid storeListingID, IEnumerable<ActionBase> actions)
        {
            this.ShowCommandEditor(new AdvancedCommandEditorControl(this, this.commandDetailsControl, storeListingID, actions));
        }

        protected override async Task OnLoaded()
        {
            if (this.commandDetailsControl != null)
            {
                CommandBase command = this.commandDetailsControl.GetExistingCommand();

                if (this.commandDetailsControl is MixPlayJoystickCommandDetailsControl)
                {
                    this.ShowCommandEditor(new EmptyCommandEditorControl(this, this.commandDetailsControl));
                }
                else if (command != null && command.Actions.Count > 0)
                {
                    if (command.IsBasic)
                    {
                        if (command is ChatCommand)
                        {
                            this.ShowCommandEditor(new BasicChatCommandEditorControl(this, (ChatCommand)command));
                        }
                        else if (command is MixPlayButtonCommand)
                        {
                            MixPlayButtonCommandDetailsControl interactiveCommandDetails = (MixPlayButtonCommandDetailsControl)this.commandDetailsControl;
                            this.ShowCommandEditor(new BasicMixPlayButtonCommandEditorControl(this, interactiveCommandDetails.Game, interactiveCommandDetails.Version,
                                interactiveCommandDetails.Control, (MixPlayButtonCommand)command));
                        }
                        else if (command is MixPlayTextBoxCommand)
                        {
                            MixPlayTextBoxCommandDetailsControl interactiveCommandDetails = (MixPlayTextBoxCommandDetailsControl)this.commandDetailsControl;
                            this.ShowCommandEditor(new BasicMixPlayTextBoxCommandEditorControl(this, interactiveCommandDetails.Game, interactiveCommandDetails.Version,
                                interactiveCommandDetails.Control, (MixPlayTextBoxCommand)command));
                        }
                        else if (command is EventCommand)
                        {
                            this.ShowCommandEditor(new BasicEventCommandEditorControl(this, (EventCommand)command));
                        }
                        else if (command is TimerCommand)
                        {
                            this.ShowCommandEditor(new BasicTimerCommandEditorControl(this, (TimerCommand)command));
                        }
                    }
                    else
                    {
                        this.ShowCommandEditor(new AdvancedCommandEditorControl(this, this.commandDetailsControl));
                    }
                }
                else if (this.commandDetailsControl is CustomCommandDetailsControl || this.commandDetailsControl is ActionGroupCommandDetailsControl)
                {
                    this.BasicChatCommandButton.Visibility = Visibility.Collapsed;
                    this.BasicSoundCommandButton.Visibility = Visibility.Collapsed;
                }
            }

            await base.OnLoaded();
        }

        private void LoadingWindowBase_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.commandEditorControl != null)
            {
                this.commandEditorControl.OnWindowSizeChanged(e.NewSize);
            }
        }

        private void BasicChatCommandButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.commandDetailsControl is ChatCommandDetailsControl)
            {
                this.ShowCommandEditor(new BasicChatCommandEditorControl(this, BasicCommandTypeEnum.Chat, ((ChatCommandDetailsControl)this.commandDetailsControl).AutoAddToChatCommands));
            }
            else if (this.commandDetailsControl is MixPlayButtonCommandDetailsControl)
            {
                MixPlayButtonCommandDetailsControl interactiveCommandDetails = (MixPlayButtonCommandDetailsControl)this.commandDetailsControl;
                this.ShowCommandEditor(new BasicMixPlayButtonCommandEditorControl(this, interactiveCommandDetails.Game, interactiveCommandDetails.Version,
                    (MixPlayButtonControlModel)interactiveCommandDetails.Control, BasicCommandTypeEnum.Chat));
            }
            else if (this.commandDetailsControl is MixPlayTextBoxCommandDetailsControl)
            {
                MixPlayTextBoxCommandDetailsControl interactiveCommandDetails = (MixPlayTextBoxCommandDetailsControl)this.commandDetailsControl;
                this.ShowCommandEditor(new BasicMixPlayTextBoxCommandEditorControl(this, interactiveCommandDetails.Game, interactiveCommandDetails.Version,
                    (MixPlayTextBoxControlModel)interactiveCommandDetails.Control, BasicCommandTypeEnum.Chat));
            }
            else if (this.commandDetailsControl is EventCommandDetailsControl)
            {
                EventCommandDetailsControl eventCommandDetails = (EventCommandDetailsControl)this.commandDetailsControl;
                this.ShowCommandEditor(new BasicEventCommandEditorControl(this, eventCommandDetails.EventType, BasicCommandTypeEnum.Chat));
            }
            else if (this.commandDetailsControl is TimerCommandDetailsControl)
            {
                this.ShowCommandEditor(new BasicTimerCommandEditorControl(this, BasicCommandTypeEnum.Chat));
            }
        }

        private void BasicSoundCommandButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.commandDetailsControl is ChatCommandDetailsControl)
            {
                this.ShowCommandEditor(new BasicChatCommandEditorControl(this, BasicCommandTypeEnum.Sound, ((ChatCommandDetailsControl)this.commandDetailsControl).AutoAddToChatCommands));
            }
            if (this.commandDetailsControl is MixPlayButtonCommandDetailsControl)
            {
                MixPlayButtonCommandDetailsControl interactiveCommandDetails = (MixPlayButtonCommandDetailsControl)this.commandDetailsControl;
                this.ShowCommandEditor(new BasicMixPlayButtonCommandEditorControl(this, interactiveCommandDetails.Game, interactiveCommandDetails.Version,
                    (MixPlayButtonControlModel)interactiveCommandDetails.Control, BasicCommandTypeEnum.Sound));
            }
            if (this.commandDetailsControl is MixPlayTextBoxCommandDetailsControl)
            {
                MixPlayTextBoxCommandDetailsControl interactiveCommandDetails = (MixPlayTextBoxCommandDetailsControl)this.commandDetailsControl;
                this.ShowCommandEditor(new BasicMixPlayTextBoxCommandEditorControl(this, interactiveCommandDetails.Game, interactiveCommandDetails.Version,
                    (MixPlayTextBoxControlModel)interactiveCommandDetails.Control, BasicCommandTypeEnum.Sound));
            }
            else if (this.commandDetailsControl is EventCommandDetailsControl)
            {
                EventCommandDetailsControl eventCommandDetails = (EventCommandDetailsControl)this.commandDetailsControl;
                this.ShowCommandEditor(new BasicEventCommandEditorControl(this, eventCommandDetails.EventType, BasicCommandTypeEnum.Sound));
            }
            else if (this.commandDetailsControl is TimerCommandDetailsControl)
            {
                this.ShowCommandEditor(new BasicTimerCommandEditorControl(this, BasicCommandTypeEnum.Sound));
            }
        }

        private void AdvancedCommandButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.ShowCommandEditor(new AdvancedCommandEditorControl(this, this.commandDetailsControl));
        }

        private void ShowCommandEditor(CommandEditorControlBase editor)
        {
            this.CommandSelectionGrid.Visibility = Visibility.Collapsed;
            this.MainContentControl.Visibility = Visibility.Visible;
            this.MainContentControl.Content = this.commandEditorControl = editor;
            this.commandEditorControl.OnCommandSaveSuccessfully += CommandEditorControl_OnCommandSaveSuccessfully;
        }

        private void CommandEditorControl_OnCommandSaveSuccessfully(object sender, CommandBase e)
        {
            if (this.CommandSaveSuccessfully != null)
            {
                this.CommandSaveSuccessfully(this, e);
            }
        }

        private void DownloadFromStoreButton_Click(object sender, RoutedEventArgs e)
        {
            this.CommandSelectionGrid.Visibility = Visibility.Collapsed;
            this.MainContentControl.Visibility = Visibility.Visible;
            this.MainContentControl.Content = new MainStoreControl(this);
        }
    }
}
