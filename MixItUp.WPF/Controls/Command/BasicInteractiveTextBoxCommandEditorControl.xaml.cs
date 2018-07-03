using Mixer.Base.Model.Interactive;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.WPF.Controls.Actions;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for BasicInteractiveTextBoxCommandEditorControl.xaml
    /// </summary>
    public partial class BasicInteractiveTextBoxCommandEditorControl : CommandEditorControlBase
    {
        private CommandWindow window;

        private BasicCommandTypeEnum commandType;
        private InteractiveGameListingModel game;
        private InteractiveGameVersionModel version;
        private InteractiveSceneModel scene;
        private InteractiveTextBoxControlModel textBox;

        private InteractiveTextBoxCommand command;

        private ActionControlBase actionControl;

        public BasicInteractiveTextBoxCommandEditorControl(CommandWindow window, InteractiveGameListingModel game, InteractiveGameVersionModel version, InteractiveTextBoxCommand command)
        {
            this.window = window;
            this.game = game;
            this.version = version;
            this.command = command;

            InitializeComponent();
        }

        public BasicInteractiveTextBoxCommandEditorControl(CommandWindow window, InteractiveGameListingModel game, InteractiveGameVersionModel version, InteractiveSceneModel scene,
            InteractiveTextBoxControlModel textBox, BasicCommandTypeEnum commandType)
        {
            this.window = window;
            this.game = game;
            this.version = version;
            this.scene = scene;
            this.textBox = textBox;
            this.commandType = commandType;

            InitializeComponent();
        }

        public override CommandBase GetExistingCommand() { return this.command; }

        protected override async Task OnLoaded()
        {
            this.TextValueSpecialIdentifierTextBlock.Text = SpecialIdentifierStringBuilder.InteractiveTextBoxTextEntrySpecialIdentifierHelpText;

            if (this.command != null)
            {
                if (this.game != null)
                {
                    this.scene = this.version.controls.scenes.FirstOrDefault(s => s.sceneID.Equals(this.command.SceneID));
                    this.textBox = this.command.TextBox;
                }

                this.SparkCostTextBox.Text = this.command.TextBox.cost.ToString();
                this.UseChatModerationCheckBox.IsChecked = this.command.UseChatModeration;

                if (this.command.Actions.First() is ChatAction)
                {
                    this.actionControl = new ChatActionControl(null, (ChatAction)this.command.Actions.First());
                }
                else if (this.command.Actions.First() is SoundAction)
                {
                    this.actionControl = new SoundActionControl(null, (SoundAction)this.command.Actions.First());
                }
            }
            else
            {
                this.SparkCostTextBox.Text = this.textBox.cost.ToString();

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

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            await this.window.RunAsyncOperation(async () =>
            {
                int sparkCost = 0;
                if (!int.TryParse(this.SparkCostTextBox.Text, out sparkCost) || sparkCost < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("Spark cost must be 0 or greater");
                    return;
                }

                ActionBase action = this.actionControl.GetAction();
                if (action == null)
                {
                    if (this.actionControl is ChatActionControl)
                    {
                        await MessageBoxHelper.ShowMessageDialog("The chat message must not be empty");
                    }
                    else if (this.actionControl is SoundActionControl)
                    {
                        await MessageBoxHelper.ShowMessageDialog("The sound file path must not be empty");
                    }
                    return;
                }

                RequirementViewModel requirements = new RequirementViewModel();

                if (this.command == null)
                {
                    this.command = new InteractiveTextBoxCommand(this.game, this.scene, this.textBox, requirements);
                    ChannelSession.Settings.InteractiveCommands.Add(this.command);
                }

                this.command.TextBox.cost = sparkCost;
                await ChannelSession.Connection.UpdateInteractiveGameVersion(this.version);

                this.command.UseChatModeration = this.UseChatModerationCheckBox.IsChecked.GetValueOrDefault();
                this.command.IsBasic = true;
                this.command.Actions.Clear();
                this.command.Actions.Add(action);

                await ChannelSession.SaveSettings();

                this.window.Close();
            });
        }
    }
}
