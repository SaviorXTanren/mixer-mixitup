using MixItUp.Base.Commands;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for CustomCommandDetailsControl.xaml
    /// </summary>
    public partial class CustomCommandDetailsControl : CommandDetailsControlBase
    {
        private CustomCommand command;

        public CustomCommandDetailsControl(CustomCommand command)
        {
            this.DataContext = this.command = command;

            InitializeComponent();
        }

        public override Task Initialize()
        {
            if (this.command != null)
            {
                this.UnlockedControl.Unlocked = this.command.Unlocked;
            }
            return Task.FromResult(0);
        }

        public override Task<bool> Validate()
        {
            return Task.FromResult(true);
        }

        public override CommandBase GetExistingCommand()
        {
            return this.command;
        }

        public override async Task<CommandBase> GetNewCommand()
        {
            if (await this.Validate())
            {
                this.command.Unlocked = this.UnlockedControl.Unlocked;
                return this.command;
            }
            return null;
        }
    }
}