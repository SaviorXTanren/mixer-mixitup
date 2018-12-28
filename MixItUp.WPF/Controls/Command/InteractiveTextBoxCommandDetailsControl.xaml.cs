using Mixer.Base.Model.Interactive;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.WPF.Util;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for InteractiveTextBoxCommandDetailsControl.xaml
    /// </summary>
    public partial class InteractiveTextBoxCommandDetailsControl : CommandDetailsControlBase
    {
        public InteractiveGameModel Game { get; private set; }
        public InteractiveGameVersionModel Version { get; private set; }
        public InteractiveSceneModel Scene { get; private set; }
        public InteractiveTextBoxControlModel Control { get; private set; }

        private InteractiveTextBoxCommand command;

        public InteractiveTextBoxCommandDetailsControl(InteractiveGameModel game, InteractiveGameVersionModel version, InteractiveTextBoxCommand command)
        {
            this.Game = game;
            this.Version = version;
            this.command = command;
            this.Control = command.TextBox;

            InitializeComponent();
        }

        public InteractiveTextBoxCommandDetailsControl(InteractiveGameModel game, InteractiveGameVersionModel version, InteractiveSceneModel scene, InteractiveTextBoxControlModel control)
        {
            this.Game = game;
            this.Version = version;
            this.Scene = scene;
            this.Control = control;

            InitializeComponent();
        }

        public override Task Initialize()
        {
            this.TextValueSpecialIdentifierTextBlock.Text = SpecialIdentifierStringBuilder.InteractiveTextBoxTextEntrySpecialIdentifierHelpText;

            if (this.Control != null)
            {
                this.NameTextBox.Text = this.Control.controlID;
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
                this.UseChatModerationCheckBox.IsChecked = this.command.UseChatModeration;
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
                RequirementViewModel requirements = this.Requirements.GetRequirements();

                if (this.command == null)
                {
                    this.command = new InteractiveTextBoxCommand(this.Game, this.Scene, this.Control, requirements);
                    ChannelSession.Settings.InteractiveCommands.Add(this.command);
                }

                this.command.TextBox.cost = int.Parse(this.SparkCostTextBox.Text);
                this.command.UseChatModeration = this.UseChatModerationCheckBox.IsChecked.GetValueOrDefault();
                this.command.Unlocked = this.UnlockedControl.Unlocked;
                this.command.Requirements = requirements;

                await ChannelSession.Connection.UpdateInteractiveGameVersion(this.Version);
                return this.command;
            }
            return null;
        }
    }
}