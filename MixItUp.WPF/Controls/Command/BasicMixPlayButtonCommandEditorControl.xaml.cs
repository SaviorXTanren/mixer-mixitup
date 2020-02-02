using Mixer.Base.Model.MixPlay;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.MixPlay;
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
    /// Interaction logic for BasicMixPlayButtonCommandEditorControl.xaml
    /// </summary>
    public partial class BasicMixPlayButtonCommandEditorControl : CommandEditorControlBase
    {
        private CommandWindow window;

        private BasicCommandTypeEnum commandType;
        private MixPlayGameModel game;
        private MixPlayGameVersionModel version;
        private MixPlaySceneModel scene;
        private MixPlayButtonControlModel button;

        private MixPlayButtonCommand command;

        private ActionControlBase actionControl;

        public BasicMixPlayButtonCommandEditorControl(CommandWindow window, MixPlayGameModel game, MixPlayGameVersionModel version, MixPlayButtonControlModel button, MixPlayButtonCommand command)
            : this(window, game, version, button)
        {
            this.command = command;
        }

        public BasicMixPlayButtonCommandEditorControl(CommandWindow window, MixPlayGameModel game, MixPlayGameVersionModel version, MixPlayButtonControlModel button, BasicCommandTypeEnum commandType)
                        : this(window, game, version, button)
        {
            this.commandType = commandType;
        }

        private BasicMixPlayButtonCommandEditorControl(CommandWindow window, MixPlayGameModel game, MixPlayGameVersionModel version, MixPlayButtonControlModel button)
        {
            this.window = window;
            this.game = game;
            this.version = version;
            this.button = button;

            InitializeComponent();
        }

        public override CommandBase GetExistingCommand() { return this.command; }

        protected override async Task OnLoaded()
        {
            this.CooldownTypeComboBox.ItemsSource = new List<string>() { "By Itself", "With All Buttons" };
            this.CooldownTypeComboBox.SelectedIndex = 0;

            this.SparkCostTextBox.Text = this.button.cost.ToString();
            if (this.command != null)
            {
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

                if (this.CooldownTypeComboBox.SelectedIndex < 0)
                {
                    await DialogHelper.ShowMessage("A cooldown type must be selected");
                    return;
                }

                int cooldown = 0;
                if (!string.IsNullOrEmpty(this.CooldownTextBox.Text))
                {
                    if (!int.TryParse(this.CooldownTextBox.Text, out cooldown) || cooldown < 0)
                    {
                        await DialogHelper.ShowMessage("Cooldown must be 0 or greater");
                        return;
                    }
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
                        await DialogHelper.ShowMessage("The sound file path must not be empty");
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
                    this.command = new MixPlayButtonCommand(this.game, this.button, MixPlayButtonCommandTriggerType.MouseKeyDown, requirements);
                    ChannelSession.Settings.MixPlayCommands.Add(this.command);
                }
                else
                {
                    this.command.Requirements = requirements;
                }

                this.button.cost = sparkCost;
                await ChannelSession.MixerUserConnection.UpdateMixPlayGameVersion(this.version);

                this.command.IsBasic = isBasic;
                this.command.Actions.Clear();
                this.command.Actions.Add(action);

                await ChannelSession.SaveSettings();

                this.window.Close();

                if (!isBasic)
                {
                    await Task.Delay(250);
                    CommandWindow window = new CommandWindow(new MixPlayButtonCommandDetailsControl(this.game, this.version, new MixPlayControlViewModel(this.game, this.button) { Command = this.command }));
                    window.CommandSaveSuccessfully += (sender, cmd) => this.CommandSavedSuccessfully(cmd);
                    window.Show();
                }
            }));
        }
    }
}
