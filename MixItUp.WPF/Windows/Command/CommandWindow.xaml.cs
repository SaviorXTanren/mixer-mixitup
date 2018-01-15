using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.Command;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Windows.Command
{
    /// <summary>
    /// Interaction logic for CommandWindow.xaml
    /// </summary>
    public partial class CommandWindow : LoadingWindowBase
    {
        public event EventHandler<CommandBase> CommandSaveSuccessfully { add { this.commandEditorControl.OnCommandSaveSuccessfully += value; } remove { this.commandEditorControl.OnCommandSaveSuccessfully -= value; } }

        private CommandEditorControlBase commandEditorControl;

        private CommandDetailsControlBase commandDetailsControl;
        private BasicChatCommand basicChatCommand;

        public CommandWindow(CommandDetailsControlBase commandDetailsControl)
            : this()
        {
            this.commandDetailsControl = commandDetailsControl;
        }

        public CommandWindow(BasicChatCommand basicChatCommand)
            : this()
        {
            this.basicChatCommand = basicChatCommand;
        }

        private CommandWindow()
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            if (this.commandDetailsControl != null && (this.commandDetailsControl.GetExistingCommand() != null || !(this.commandDetailsControl is ChatCommandDetailsControl)))
            {
                this.ShowCommandEditor(new AdvancedCommandEditorControl(this, this.commandDetailsControl));
            }
            else if (this.basicChatCommand is BasicChatCommand)
            {
                this.ShowCommandEditor(new BasicChatCommandEditorControl(this, this.basicChatCommand));
            }

            await base.OnLoaded();
        }

        private void BasicChatCommandButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.ShowCommandEditor(new BasicChatCommandEditorControl(this));
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
        }
    }
}
