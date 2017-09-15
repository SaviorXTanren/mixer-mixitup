using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.WPF.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for TimerCommandDetailsControl.xaml
    /// </summary>
    public partial class TimerCommandDetailsControl : CommandDetailsControlBase
    {
        private TimerCommand command;

        public TimerCommandDetailsControl(TimerCommand command)
        {
            this.command = command;
            InitializeComponent();
        }

        public TimerCommandDetailsControl() : this(null) { }

        public override Task Initialize()
        {
            if (this.command != null)
            {
                this.NameTextBox.Text = this.command.Name;
            }

            return Task.FromResult(0);
        }

        public override IEnumerable<ActionTypeEnum> GetAllowedActions() { return TimerCommand.AllowedActions; }

        public override bool Validate()
        {
            if (string.IsNullOrEmpty(this.NameTextBox.Text))
            {
                MessageBoxHelper.ShowError("Required command information is missing");
                return false;
            }

            return true;
        }

        public override CommandBase GetExistingCommand() { return this.command; }

        public override Task<CommandBase> GetNewCommand()
        {
            if (this.Validate())
            {
                if (this.command == null)
                {
                    this.command = new TimerCommand(this.NameTextBox.Text);
                    ChannelSession.Settings.TimerCommands.Add(this.command);
                }
                else
                {
                    this.command.Name = this.NameTextBox.Text;
                }
                return Task.FromResult<CommandBase>(this.command);
            }
            return Task.FromResult<CommandBase>(null);
        }
    }
}
