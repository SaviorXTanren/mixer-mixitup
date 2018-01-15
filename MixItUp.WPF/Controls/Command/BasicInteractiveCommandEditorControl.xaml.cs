using Mixer.Base.Model.Interactive;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
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
    /// Interaction logic for BasicInteractiveCommandEditorControl.xaml
    /// </summary>
    public partial class BasicInteractiveCommandEditorControl : CommandEditorControlBase
    {
        private CommandWindow window;

        private BasicCommandTypeEnum commandType;
        private InteractiveGameListingModel game;
        private InteractiveGameVersionModel version;
        private InteractiveSceneModel scene;
        private InteractiveButtonControlModel button;

        private InteractiveCommand command;

        private ActionControlBase actionControl;

        public BasicInteractiveCommandEditorControl(CommandWindow window, InteractiveCommand command)
        {
            this.window = window;
            this.command = command;

            InitializeComponent();
        }

        public BasicInteractiveCommandEditorControl(CommandWindow window, InteractiveGameListingModel game, InteractiveGameVersionModel version, InteractiveSceneModel scene,
            InteractiveButtonControlModel button, BasicCommandTypeEnum commandType)
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

            if (this.command != null)
            {
                IEnumerable<InteractiveGameListingModel> games = await ChannelSession.Connection.GetOwnedInteractiveGames(ChannelSession.Channel);
                this.game = games.FirstOrDefault(g => g.name.Equals(this.command.GameID));
                if (this.game != null)
                {
                    this.version = this.game.versions.First();
                    this.version = await ChannelSession.Connection.GetInteractiveGameVersion(this.version);
                    this.scene = this.version.controls.scenes.FirstOrDefault(s => s.sceneID.Equals(this.command.SceneID));
                    this.button = this.command.Button;
                }
            }

            if (this.command != null)
            {
                this.SparkCostTextBox.Text = this.command.Button.cost.ToString();
                if (!string.IsNullOrEmpty(this.command.CooldownGroup))
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
                    this.actionControl = new ChatActionControl(null, (ChatAction)this.command.Actions.First());
                }
                else if (this.command.Actions.First() is SoundAction)
                {
                    this.actionControl = new SoundActionControl(null, (SoundAction)this.command.Actions.First());
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

            this.ActionControlControl.Content = this.actionControl;

            await base.OnLoaded();
        }

        private void CooldownTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.CooldownTypeComboBox.SelectedIndex == 1 && ChannelSession.Settings.InteractiveCooldownGroups.ContainsKey(InteractiveCommand.BasicCommandCooldownGroup))
            {
                this.CooldownTextBox.Text = ChannelSession.Settings.InteractiveCooldownGroups[InteractiveCommand.BasicCommandCooldownGroup].ToString();
            }
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

                if (this.command == null)
                {
                    this.command = new InteractiveCommand(this.game, this.scene, this.button, InteractiveButtonCommandTriggerType.MouseDown);
                    ChannelSession.Settings.InteractiveCommands.Add(this.command);
                }

                this.command.Button.cost = sparkCost;
                if (this.CooldownTypeComboBox.SelectedIndex == 0)
                {
                    this.command.IndividualCooldown = cooldown;
                }
                else
                {
                    this.command.CooldownGroup = InteractiveCommand.BasicCommandCooldownGroup;
                    ChannelSession.Settings.InteractiveCooldownGroups[InteractiveCommand.BasicCommandCooldownGroup] = cooldown;
                }
                await ChannelSession.Connection.UpdateInteractiveGameVersion(this.version);

                this.command.IsBasic = true;
                this.command.Actions.Clear();
                this.command.Actions.Add(action);

                await ChannelSession.SaveSettings();

                this.window.Close();
            });
        }
    }
}
