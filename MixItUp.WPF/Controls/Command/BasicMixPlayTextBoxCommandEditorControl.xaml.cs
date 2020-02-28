using Mixer.Base.Model.MixPlay;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.MixPlay;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.WPF.Controls.Actions;
using MixItUp.WPF.Windows.Command;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for BasicMixPlayTextBoxCommandEditorControl.xaml
    /// </summary>
    public partial class BasicMixPlayTextBoxCommandEditorControl : CommandEditorControlBase
    {
        private CommandWindow window;

        private BasicCommandTypeEnum commandType;
        private MixPlayGameModel game;
        private MixPlayGameVersionModel version;
        private MixPlayTextBoxControlModel textBox;

        private MixPlayTextBoxCommand command;

        private ActionControlBase actionControl;

        public BasicMixPlayTextBoxCommandEditorControl(CommandWindow window, MixPlayGameModel game, MixPlayGameVersionModel version, MixPlayTextBoxControlModel textBox, MixPlayTextBoxCommand command)
            : this(window, game, version, textBox)
        {
            this.command = command;
        }

        public BasicMixPlayTextBoxCommandEditorControl(CommandWindow window, MixPlayGameModel game, MixPlayGameVersionModel version, MixPlayTextBoxControlModel textBox, BasicCommandTypeEnum commandType)
            : this(window, game, version, textBox)
        {
            this.commandType = commandType;
        }

        private BasicMixPlayTextBoxCommandEditorControl(CommandWindow window, MixPlayGameModel game, MixPlayGameVersionModel version, MixPlayTextBoxControlModel textBox)
        {
            this.window = window;
            this.game = game;
            this.version = version;
            this.textBox = textBox;

            InitializeComponent();
        }

        public override CommandBase GetExistingCommand() { return this.command; }

        protected override async Task OnLoaded()
        {
            this.TextValueSpecialIdentifierTextBlock.Text = SpecialIdentifierStringBuilder.InteractiveTextBoxTextEntrySpecialIdentifierHelpText;

            this.SparkCostTextBox.Text = this.textBox.cost.ToString();
            if (this.command != null)
            {
                this.UseChatModerationCheckBox.IsChecked = this.command.UseChatModeration;

                if (this.command.Actions.First() is ChatAction)
                {
                    this.actionControl = new ChatActionControl((ChatAction)this.command.Actions.First());
                }
                else if (this.command.Actions.First() is SoundAction)
                {
                    this.actionControl = new SoundActionControl((SoundAction)this.command.Actions.First());
                }
            }
            else
            {
                if (this.commandType == BasicCommandTypeEnum.Chat)
                {
                    this.actionControl = new ChatActionControl(null);
                }
                else if (this.commandType == BasicCommandTypeEnum.Sound)
                {
                    this.actionControl = new SoundActionControl(null);
                }
            }

            this.ActionControlControl.Content = this.actionControl;

            await base.OnLoaded();
        }

        private async void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            await SaveAndClose(false);
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            await SaveAndClose(true);
        }

        private async Task SaveAndClose(bool isBasic)
        {
            await this.window.RunAsyncOperation((System.Func<Task>)(async () =>
            {
                int sparkCost = 0;
                if (!int.TryParse(this.SparkCostTextBox.Text, out sparkCost) || sparkCost < 0)
                {
                    await DialogHelper.ShowMessage("Spark cost must be 0 or greater");
                    return;
                }

                ActionBase action = this.actionControl.GetAction();
                if (action == null)
                {
                    if (this.actionControl is ChatActionControl)
                    {
                        await DialogHelper.ShowMessage("The chat message must not be empty");
                    }
                    else if (this.actionControl is SoundActionControl)
                    {
                        await DialogHelper.ShowMessage(MixItUp.Base.Resources.EmptySoundFilePath);
                    }
                    return;
                }

                RequirementViewModel requirements = new RequirementViewModel();

                if (this.command == null)
                {
                    this.command = new MixPlayTextBoxCommand(this.game, this.textBox, requirements);
                    ChannelSession.Settings.MixPlayCommands.Add(this.command);
                }

                this.textBox.cost = sparkCost;
                await ChannelSession.MixerUserConnection.UpdateMixPlayGameVersion(this.version);

                this.command.UseChatModeration = this.UseChatModerationCheckBox.IsChecked.GetValueOrDefault();
                this.command.IsBasic = isBasic;
                this.command.Actions.Clear();
                this.command.Actions.Add(action);

                await ChannelSession.SaveSettings();

                this.window.Close();

                if (!isBasic)
                {
                    await Task.Delay(250);
                    CommandWindow window = new CommandWindow(new MixPlayTextBoxCommandDetailsControl(this.game, this.version, new MixPlayControlViewModel(this.game, this.textBox) { Command = this.command }));
                    window.CommandSaveSuccessfully += (sender, cmd) => this.CommandSavedSuccessfully(cmd);
                    window.Show();
                }
            }));
        }
    }
}
