using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for EventCommandDetailsControl.xaml
    /// </summary>
    public partial class EventCommandDetailsControl : CommandDetailsControlBase
    {
        public EventTypeEnum EventType { get; private set; }

        private EventCommand command;

        public EventCommandDetailsControl(EventCommand command)
        {
            this.command = command;
            this.EventType = this.command.EventCommandType;

            InitializeComponent();
        }

        public EventCommandDetailsControl(EventTypeEnum eventType)
        {
            this.EventType = eventType;

            InitializeComponent();
        }

        public EventCommandDetailsControl() : this(null) { }

        public override Task Initialize()
        {
            this.EventTypeTextBox.Text = EnumHelper.GetEnumName(this.EventType);
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

        public override CommandBase GetExistingCommand() { return this.command; }

        public override async Task<CommandBase> GetNewCommand()
        {
            if (await this.Validate())
            {
                if (this.command == null)
                {
                    this.command = new EventCommand(this.EventType);
                    if (ChannelSession.Settings.EventCommands.Any(se => se.EventCommandType.Equals(this.command.EventCommandType)))
                    {
                        await DialogHelper.ShowMessage("This event already exists");
                        return null;
                    }
                    ChannelSession.Settings.EventCommands.Add(this.command);
                }
                this.command.Unlocked = this.UnlockedControl.Unlocked;
                return this.command;
            }
            return null;
        }
    }
}
