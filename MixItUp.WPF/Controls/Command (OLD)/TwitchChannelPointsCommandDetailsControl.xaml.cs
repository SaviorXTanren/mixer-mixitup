using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for TwitchChannelPointsCommandDetailsControl.xaml
    /// </summary>
    public partial class TwitchChannelPointsCommandDetailsControl : CommandDetailsControlBase
    {
        private TwitchChannelPointsCommand command;

        public TwitchChannelPointsCommandDetailsControl(TwitchChannelPointsCommand command)
        {
            this.command = command;
            InitializeComponent();
        }

        public TwitchChannelPointsCommandDetailsControl() : this(null) { }

        public override Task Initialize()
        {
            if (this.command != null)
            {
                this.NameTextBox.Text = this.command.Name;
                this.UnlockedControl.Unlocked = this.command.Unlocked;
            }
            return Task.FromResult(0);
        }

        public override async Task<bool> Validate()
        {
            if (string.IsNullOrEmpty(this.NameTextBox.Text))
            {
                await DialogHelper.ShowMessage("Reward Name is missing");
                return false;
            }
            return true;
        }

        public override CommandBase GetExistingCommand() { return this.command; }

        public override async Task<CommandBase> GetNewCommand()
        {
            if (await this.Validate())
            {
                if (this.command == null || string.IsNullOrEmpty(this.command.Name))
                {
                    this.command = new TwitchChannelPointsCommand(this.NameTextBox.Text);
                }
                else
                {
                    this.command.Name = this.NameTextBox.Text;
                }
                this.command.Unlocked = this.UnlockedControl.Unlocked;
                return this.command;
            }
            return null;
        }
    }
}