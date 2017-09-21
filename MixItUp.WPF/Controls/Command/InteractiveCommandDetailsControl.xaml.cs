using Mixer.Base.Model.Interactive;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.WPF.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for InteractiveCommandDetailsControl.xaml
    /// </summary>
    public partial class InteractiveCommandDetailsControl : CommandDetailsControlBase
    {
        private InteractiveGameListingModel game;
        private InteractiveGameVersionModel version;
        private InteractiveSceneModel scene;
        private InteractiveControlModel control;

        private InteractiveCommand command;

        public InteractiveCommandDetailsControl(InteractiveGameListingModel game, InteractiveGameVersionModel version, InteractiveSceneModel scene, InteractiveCommand command)
            : this(game, version, scene, command.Control)
        {
            this.command = command;
        }

        public InteractiveCommandDetailsControl(InteractiveGameListingModel game, InteractiveGameVersionModel version, InteractiveSceneModel scene, InteractiveControlModel control)
        {
            this.game = game;
            this.version = version;
            this.scene = scene;
            this.control = control;

            InitializeComponent();
        }

        public override Task Initialize()
        {
            this.ButtonTriggerComboBox.ItemsSource = EnumHelper.GetEnumNames<InteractiveButtonCommandTriggerType>();

            if (this.control != null && this.control is InteractiveButtonControlModel)
            {
                this.ButtonTriggerComboBox.IsEnabled = true;
                this.ButtonTriggerComboBox.SelectedItem = EnumHelper.GetEnumName(InteractiveButtonCommandTriggerType.MouseDown);
                this.SparkCostTextBox.IsEnabled = true;
                this.SparkCostTextBox.Text = ((InteractiveButtonControlModel)this.control).cost.ToString();
                this.CooldownTextBox.IsEnabled = true;
            }

            if (this.command != null)
            {
                if (this.command.Button != null)
                {
                    this.ButtonTriggerComboBox.SelectedItem = EnumHelper.GetEnumName(this.command.Trigger);
                    this.CooldownTextBox.Text = this.command.Cooldown.ToString();
                }
            }

            return Task.FromResult(0);
        }

        public override IEnumerable<ActionTypeEnum> GetAllowedActions() { return InteractiveCommand.AllowedActions; }

        public override bool Validate()
        {
            int sparkCost = 0;
            int cooldown = 0;

            if (this.control is InteractiveButtonControlModel)
            {
                if (this.ButtonTriggerComboBox.SelectedIndex < 0)
                {
                    MessageBoxHelper.ShowDialog("An trigger type must be selected");
                    return false;
                }

                if (!int.TryParse(this.SparkCostTextBox.Text, out sparkCost) || sparkCost <= 0)
                {
                    MessageBoxHelper.ShowDialog("A valid spark cost must be entered");
                    return false;
                }

                if (!int.TryParse(this.CooldownTextBox.Text, out cooldown) || cooldown <= 0)
                {
                    MessageBoxHelper.ShowDialog("A valid cooldown must be entered");
                    return false;
                }
            }

            return true;
        }

        public override CommandBase GetExistingCommand() { return this.command; }

        public override async Task<CommandBase> GetNewCommand()
        {
            if (this.Validate())
            {
                InteractiveButtonCommandTriggerType trigger = EnumHelper.GetEnumValueFromString<InteractiveButtonCommandTriggerType>((string)this.ButtonTriggerComboBox.SelectedItem);
                if (this.command == null)
                {
                    if (this.control is InteractiveButtonControlModel)
                    {                
                        this.command = new InteractiveCommand(this.game, this.scene, (InteractiveButtonControlModel)this.control, trigger);
                    }
                    else
                    {
                        this.command = new InteractiveCommand(this.game, this.scene, (InteractiveJoystickControlModel)this.control);
                    }
                    ChannelSession.Settings.InteractiveControls.Add(this.command);
                }

                if (this.control is InteractiveButtonControlModel)
                {
                    this.command.Trigger = trigger;
                    this.command.Button.cost = int.Parse(this.SparkCostTextBox.Text);
                    this.command.Cooldown = int.Parse(this.CooldownTextBox.Text);

                    await ChannelSession.MixerConnection.Interactive.UpdateInteractiveGameVersion(this.version);
                }
                return this.command;
            }
            return null;
        }
    }
}
