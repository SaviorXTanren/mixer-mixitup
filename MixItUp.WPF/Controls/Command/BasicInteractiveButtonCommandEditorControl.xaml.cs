using Mixer.Base.Model.MixPlay;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
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
    /// Interaction logic for BasicInteractiveButtonCommandEditorControl.xaml
    /// </summary>
    public partial class BasicInteractiveButtonCommandEditorControl : CommandEditorControlBase
    {
        private CommandWindow window;

        private BasicCommandTypeEnum commandType;
        private MixPlayGameModel game;
        private MixPlayGameVersionModel version;
        private MixPlaySceneModel scene;
        private MixPlayButtonControlModel button;

        private MixPlayButtonCommand command;

        private ActionControlBase actionControl;

        public BasicInteractiveButtonCommandEditorControl(CommandWindow window, MixPlayGameModel game, MixPlayGameVersionModel version, MixPlayButtonCommand command)
        {
            this.window = window;
            this.game = game;
            this.version = version;
            this.command = command;

            InitializeComponent();
        }

        public BasicInteractiveButtonCommandEditorControl(CommandWindow window, MixPlayGameModel game, MixPlayGameVersionModel version, MixPlaySceneModel scene,
            MixPlayButtonControlModel button, BasicCommandTypeEnum commandType)
        {
            this.window = window;
            this.game = game;
            this.version = version;
            this.scene = scene;
            this.button = button;
            this.commandType = commandType;

            InitializeComponent();
        }

        public override CommandBase GetExistingCommand() { return this.command; }

        protected override async Task OnLoaded()
        {
            this.CooldownTypeComboBox.ItemsSource = new List<string>() { "By Itself", "With All Buttons" };
            this.CooldownTypeComboBox.SelectedIndex = 0;

            if (this.command != null)
            {
                if (this.game != null)
                {
                    this.scene = this.version.controls.scenes.FirstOrDefault(s => s.sceneID.Equals(this.command.SceneID));
                    this.button = this.command.Button;
                }

                this.SparkCostTextBox.Text = this.command.Button.cost.ToString();
                if (this.command.Requirements.Cooldown != null && this.command.Requirements.Cooldown.IsGroup)
                {
                    this.CooldownTypeComboBox.SelectedIndex = 1;
                }
                else
                {
                    this.CooldownTypeComboBox.SelectedIndex = 0;
                    this.CooldownTextBox.Text = this.command.CooldownAmount.ToString();
                }

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
                this.SparkCostTextBox.Text = this.button.cost.ToString();
                this.CooldownTextBox.Text = "0";

                if (this.commandType == BasicCommandTypeEnum.Chat)
                {
                    this.actionControl = new ChatActionControl(null);
                }
                else if (this.commandType == BasicCommandTypeEnum.Sound)
                {
                    this.actionControl = new SoundActionControl(null);
                }
            }

            if (string.IsNullOrEmpty(this.SparkCostTextBox.Text))
            {
                this.SparkCostTextBox.Text = "0";
            }

            this.ActionControlControl.Content = this.actionControl;

            await base.OnLoaded();
        }

        private void CooldownTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.CooldownTypeComboBox.SelectedIndex == 1)
            {
                if (ChannelSession.Settings.CooldownGroups.ContainsKey(MixPlayButtonCommand.BasicCommandCooldownGroup))
                {
                    this.CooldownTextBox.Text = ChannelSession.Settings.CooldownGroups[MixPlayButtonCommand.BasicCommandCooldownGroup].ToString();
                }
                else
                {
                    this.CooldownTextBox.Text = "0";
                }
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            await this.window.RunAsyncOperation((System.Func<Task>)(async () =>
            {
                int sparkCost = 0;
                if (!int.TryParse(this.SparkCostTextBox.Text, out sparkCost) || sparkCost < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("Spark cost must be 0 or greater");
                    return;
                }

                if (this.CooldownTypeComboBox.SelectedIndex < 0)
                {
                    await MessageBoxHelper.ShowMessageDialog("A cooldown type must be selected");
                    return;
                }

                int cooldown = 0;
                if (!string.IsNullOrEmpty(this.CooldownTextBox.Text))
                {
                    if (!int.TryParse(this.CooldownTextBox.Text, out cooldown) || cooldown < 0)
                    {
                        await MessageBoxHelper.ShowMessageDialog("Cooldown must be 0 or greater");
                        return;
                    }
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
                if (this.CooldownTypeComboBox.SelectedIndex == 0)
                {
                    requirements.Cooldown = new CooldownRequirementViewModel(CooldownTypeEnum.Individual, cooldown);
                }
                else
                {
                    requirements.Cooldown = new CooldownRequirementViewModel(CooldownTypeEnum.Group, MixPlayButtonCommand.BasicCommandCooldownGroup, cooldown);
                }

                if (this.command == null)
                {
                    this.command = new MixPlayButtonCommand(this.game, this.scene, this.button, MixPlayButtonCommandTriggerType.MouseKeyDown, requirements);
                    ChannelSession.Settings.MixPlayCommands.Add(this.command);
                }
                else
                {
                    this.command.Requirements = requirements;
                }

                this.command.Button.cost = sparkCost;
                await ChannelSession.MixerStreamerConnection.UpdateMixPlayGameVersion(this.version);

                this.command.IsBasic = true;
                this.command.Actions.Clear();
                this.command.Actions.Add(action);

                await ChannelSession.SaveSettings();

                this.window.Close();
            }));
        }
    }
}
