using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.WPF.Windows.Command;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for EmptyCommandEditorControl.xaml
    /// </summary>
    public partial class EmptyCommandEditorControl : CommandEditorControlBase
    {
        private CommandWindow window;

        private CommandDetailsControlBase commandDetailsControl;

        private CommandBase newCommand = null;

        public EmptyCommandEditorControl(CommandWindow window, CommandDetailsControlBase commandDetailsControl)
        {
            this.window = window;
            this.commandDetailsControl = commandDetailsControl;

            InitializeComponent();

            this.MainContent.Content = this.commandDetailsControl;
        }

        public override CommandBase GetExistingCommand() { return this.commandDetailsControl.GetExistingCommand(); }

        protected override async Task OnLoaded()
        {
            await this.commandDetailsControl.Initialize();

            await base.OnLoaded();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.newCommand = await this.GetNewCommand();
            if (this.newCommand != null)
            {
                this.CommandSavedSuccessfully(this.newCommand);

                await this.window.RunAsyncOperation(async () => { await ChannelSession.SaveSettings(); });

                this.window.Close();
            }
        }

        private async Task<CommandBase> GetNewCommand()
        {
            if (!await this.commandDetailsControl.Validate())
            {
                return null;
            }

            return await this.window.RunAsyncOperation(async () => { return await this.commandDetailsControl.GetNewCommand(); });
        }
    }
}
