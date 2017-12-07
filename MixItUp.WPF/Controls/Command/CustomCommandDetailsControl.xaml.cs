using MixItUp.Base.Commands;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for EventCommandDetailsControl.xaml
    /// </summary>
    public partial class CustomCommandDetailsControl : CommandDetailsControlBase
    {
        private CustomCommand command;

        public CustomCommandDetailsControl(CustomCommand command)
        {
            this.command = command;
            InitializeComponent();
        }

        public CustomCommandDetailsControl() : this(null)
        {
        }

        public override Task Initialize()
        {
            return Task.FromResult(0);
        }

        public override async Task<bool> Validate()
        {
            return await Task.FromResult(true);
        }

        public override CommandBase GetExistingCommand()
        {
            return this.command;
        }

        public override async Task<CommandBase> GetNewCommand()
        {
            if (await this.Validate())
            {
                return this.command;
            }
            return null;
        }
    }
}