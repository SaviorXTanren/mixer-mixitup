using Mixer.Base.Clients;
using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.Actions;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows.Command;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for BasicEventCommandEditorControl.xaml
    /// </summary>
    public partial class BasicEventCommandEditorControl : CommandEditorControlBase
    {
        private CommandWindow window;

        private BasicCommandTypeEnum commandType;
        private ConstellationEventTypeEnum eventType;
        private EventCommand command;

        private ActionControlBase actionControl;

        public BasicEventCommandEditorControl(CommandWindow window, EventCommand command)
            : this(window, command.EventType, BasicCommandTypeEnum.None)
        {
            this.command = command;
        }

        public BasicEventCommandEditorControl(CommandWindow window, ConstellationEventTypeEnum eventType, BasicCommandTypeEnum commandType)
        {
            this.window = window;
            this.eventType = eventType;
            this.commandType = commandType;

            InitializeComponent();
        }

        public override CommandBase GetExistingCommand() { return this.command; }

        protected override async Task OnLoaded()
        {
            this.EventTypeTextBlock.Text = EnumHelper.GetEnumName(this.eventType);

            if (this.command != null)
            {
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

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            await this.window.RunAsyncOperation(async () =>
            {
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

                EventCommand newCommand = new EventCommand(this.eventType, ChannelSession.Channel);
                newCommand.IsBasic = true;
                newCommand.Actions.Add(action);

                if (this.command != null)
                {
                    ChannelSession.Settings.EventCommands.Remove(this.command);
                }
                ChannelSession.Settings.EventCommands.Add(newCommand);

                await ChannelSession.SaveSettings();

                this.window.Close();
            });
        }
    }
}
