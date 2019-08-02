using Mixer.Base.Model.Interactive;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.WPF.Util;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for InteractiveButtonCommandDetailsControl.xaml
    /// </summary>
    public partial class InteractiveButtonCommandDetailsControl : CommandDetailsControlBase
    {
        public InteractiveGameModel Game { get; private set; }
        public InteractiveGameVersionModel Version { get; private set; }
        public InteractiveSceneModel Scene { get; private set; }
        public InteractiveButtonControlModel Control { get; private set; }

        private InteractiveButtonCommand command;

        public InteractiveButtonCommandDetailsControl(InteractiveGameModel game, InteractiveGameVersionModel version, InteractiveButtonCommand command)
        {
            this.Game = game;
            this.Version = version;
            this.command = command;
            this.Control = command.Button;

            InitializeComponent();
        }

        public InteractiveButtonCommandDetailsControl(InteractiveGameModel game, InteractiveGameVersionModel version, InteractiveSceneModel scene, InteractiveButtonControlModel control)
        {
            this.Game = game;
            this.Version = version;
            this.Scene = scene;
            this.Control = control;

            InitializeComponent();
        }

        public override Task Initialize()
        {
            this.Requirements.SettingsRequirement.HideDeleteChatCommandWhenRun();

            this.ButtonTriggerComboBox.ItemsSource = EnumHelper.GetEnumNames<InteractiveButtonCommandTriggerType>();
            this.HeldRateTextBox.Text = "1";

            if (this.Control != null)
            {
                this.NameTextBox.Text = this.Control.controlID;
                this.ButtonTriggerComboBox.IsEnabled = true;
                this.ButtonTriggerComboBox.SelectedItem = EnumHelper.GetEnumName(InteractiveButtonCommandTriggerType.MouseKeyDown);
                this.SparkCostTextBox.IsEnabled = true;
                this.SparkCostTextBox.Text = this.Control.cost.ToString();
            }

            if (this.Scene != null)
            {
                this.SceneTextBox.Text = this.Scene.sceneID;
            }

            if (this.command != null)
            {
                this.SceneTextBox.Text = this.command.SceneID;
                this.ButtonTriggerComboBox.SelectedItem = EnumHelper.GetEnumName(this.command.Trigger);
                this.HeldRateTextBox.Text = this.command.HeldRate.ToString();
                this.UnlockedControl.Unlocked = this.command.Unlocked;
                this.Requirements.SetRequirements(this.command.Requirements);

                if (this.Game != null)
                {
                    this.Scene = this.Version.controls.scenes.FirstOrDefault(s => s.sceneID.Equals(this.command.SceneID));
                }
            }

            return Task.FromResult(0);
        }

        public override async Task<bool> Validate()
        {
            if (this.ButtonTriggerComboBox.SelectedIndex < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("An trigger type must be selected");
                return false;
            }

            InteractiveButtonCommandTriggerType trigger = EnumHelper.GetEnumValueFromString<InteractiveButtonCommandTriggerType>((string)this.ButtonTriggerComboBox.SelectedItem);
            if (trigger == InteractiveButtonCommandTriggerType.MouseKeyHeld)
            {
                if (string.IsNullOrEmpty(this.HeldRateTextBox.Text) || !int.TryParse(this.HeldRateTextBox.Text, out int heldRate) || heldRate < 1)
                {
                    await MessageBoxHelper.ShowMessageDialog("A valid held rate of 1 or greater must be entered");
                    return false;
                }
            }

            if (!int.TryParse(this.SparkCostTextBox.Text, out int sparkCost) || sparkCost < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("A valid spark cost must be entered");
                return false;
            }

            if (!await this.Requirements.Validate())
            {
                return false;
            }

            return true;
        }

        public override CommandBase GetExistingCommand() { return this.command; }

        public override async Task<CommandBase> GetNewCommand()
        {
            if (await this.Validate())
            {
                InteractiveButtonCommandTriggerType trigger = EnumHelper.GetEnumValueFromString<InteractiveButtonCommandTriggerType>((string)this.ButtonTriggerComboBox.SelectedItem);

                RequirementViewModel requirements = this.Requirements.GetRequirements();

                if (this.command == null)
                {
                    this.command = new InteractiveButtonCommand(this.Game, this.Scene, this.Control, trigger, requirements);
                    ChannelSession.Settings.InteractiveCommands.Add(this.command);
                }

                this.command.Trigger = trigger;
                this.command.Button.cost = int.Parse(this.SparkCostTextBox.Text);
                this.command.Unlocked = this.UnlockedControl.Unlocked;
                this.command.Requirements = requirements;

                if (this.command.Trigger == InteractiveButtonCommandTriggerType.MouseKeyHeld)
                {
                    int.TryParse(this.HeldRateTextBox.Text, out int heldRate);
                    this.command.HeldRate = heldRate;
                }

                await ChannelSession.MixerStreamerConnection.UpdateInteractiveGameVersion(this.Version);
                return this.command;
            }
            return null;
        }

        private void ButtonTriggerComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.ButtonTriggerComboBox.SelectedIndex >= 0)
            {
                InteractiveButtonCommandTriggerType trigger = EnumHelper.GetEnumValueFromString<InteractiveButtonCommandTriggerType>((string)this.ButtonTriggerComboBox.SelectedItem);
                if (trigger == InteractiveButtonCommandTriggerType.MouseKeyHeld)
                {
                    this.HeldRateTextBox.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    this.HeldRateTextBox.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }
    }
}
