using Mixer.Base.Model.MixPlay;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.MixPlay;
using MixItUp.Base.ViewModel.Requirement;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for MixPlayTextBoxCommandDetailsControl.xaml
    /// </summary>
    public partial class MixPlayTextBoxCommandDetailsControl : CommandDetailsControlBase
    {
        public MixPlayGameModel Game { get; private set; }
        public MixPlayGameVersionModel Version { get; private set; }
        public MixPlayTextBoxControlModel Control { get; private set; }

        private MixPlayTextBoxCommand command;

        public MixPlayTextBoxCommandDetailsControl(MixPlayGameModel game, MixPlayGameVersionModel version, MixPlayControlViewModel command)
            : this(game, version, command.TextBox)
        {
            this.command = (MixPlayTextBoxCommand)command.Command;
        }

        public MixPlayTextBoxCommandDetailsControl(MixPlayGameModel game, MixPlayGameVersionModel version, MixPlayTextBoxControlModel control)
        {
            this.Game = game;
            this.Version = version;
            this.Control = control;

            InitializeComponent();
        }

        public override Task Initialize()
        {
            this.Requirements.HideSettingsRequirement();

            this.TextValueSpecialIdentifierTextBlock.Text = SpecialIdentifierStringBuilder.InteractiveTextBoxTextEntrySpecialIdentifierHelpText;

            if (this.Control != null)
            {
                this.NameTextBox.Text = this.Control.controlID;
                this.SparkCostTextBox.IsEnabled = true;
                if (this.Control.cost.HasValue)
                {
                    this.SparkCostTextBox.Text = this.Control.cost.ToString();
                }
                else
                {
                    this.SparkCostTextBox.Text = "0";
                }
            }

            if (this.command != null)
            {
                this.UseChatModerationCheckBox.IsChecked = this.command.UseChatModeration;
                this.UnlockedControl.Unlocked = this.command.Unlocked;
                this.Requirements.SetRequirements(this.command.Requirements);
            }

            return Task.FromResult(0);
        }

        public override async Task<bool> Validate()
        {
            if (!int.TryParse(this.SparkCostTextBox.Text, out int sparkCost) || sparkCost < 0)
            {
                await DialogHelper.ShowMessage("A valid spark cost must be entered");
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
                RequirementViewModel requirements = this.Requirements.GetRequirements();

                if (this.command == null)
                {
                    this.command = new MixPlayTextBoxCommand(this.Game, this.Control, requirements);
                    ChannelSession.Settings.MixPlayCommands.Add(this.command);
                }

                this.Control.cost = int.Parse(this.SparkCostTextBox.Text);
                this.command.UseChatModeration = this.UseChatModerationCheckBox.IsChecked.GetValueOrDefault();
                this.command.Unlocked = this.UnlockedControl.Unlocked;
                this.command.Requirements = requirements;

                await ChannelSession.MixerUserConnection.UpdateMixPlayGameVersion(this.Version);
                return this.command;
            }
            return null;
        }
    }
}